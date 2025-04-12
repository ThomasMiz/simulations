using System.Numerics;
using System.Runtime.InteropServices;
using TrippyGL.Utils;

namespace tp3_gpu;

public class SimulationConfig
{
    public const float DefaultContainerRadius = 0.05f;

    public int? RandomSeed { get; init; } = null;

    public float ContainerRadius { get; init; } = DefaultContainerRadius;

    public uint? MaxSteps { get; init; } = null;
    public float? MaxSimulationTime { get; init; } = null;

    public string? OutputFile { get; init; } = null;

    private Random? r;
    private readonly List<ParticleConsts> particleConsts = new();
    private readonly List<PositionAndVelocity> particlePosAndsVels = new();
    
    public int ParticleCount => particleConsts.Count;
    
    public ReadOnlySpan<ParticleConsts> ParticleConstsSpan => CollectionsMarshal.AsSpan(particleConsts);
    public ReadOnlySpan<PositionAndVelocity> PositionAndVelocitySpan => CollectionsMarshal.AsSpan(particlePosAndsVels);

    /// <summary>
    /// Returns true if a particle with the given radius and position would collide with an existing particle.
    /// </summary>
    private bool IntersectsWithExistingParticles(float radius, Vector2 position)
    {
        for (int i = 0; i < particleConsts.Count; i++)
        {
            float radsum = radius + particleConsts[i].Radius;
            if (Vector2.DistanceSquared(position, particlePosAndsVels[i].Position) < radsum * radsum)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Adds a single particle
    /// </summary>
    /// <param name="mass"></param>
    /// <param name="radius"></param>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public SimulationConfig AddSingleParticle(float mass, float radius, Vector2 position, Vector2 velocity)
    {
        if (IntersectsWithExistingParticles(radius, position))
            throw new Exception("Cannot add single particle; would intersect with an existing particle!");
        
        particleConsts.Add(new ParticleConsts(mass, radius));
        particlePosAndsVels.Add(new PositionAndVelocity(position, velocity));
        
        return this;
    }

    private Vector2 GetRandomNonOverlappingPosition(float radius, int retries)
    {
        while (retries > 0)
        {
            retries--;
            Vector2 position = r.RandomDirection2() * r.NextFloat(ContainerRadius - radius);
            if (!IntersectsWithExistingParticles(radius, position))
                return position;
        }

        throw new Exception("Could not generate a random position for all particles; is the simulation too crowded?");
    }

    /// <summary>
    /// Adds particles randomly scattered throughout the container, ensuring they don't intersect each other nor other
    /// existing particles.
    /// </summary>
    public SimulationConfig AddScatteredParticles(uint count, float mass, float radius, float speed)
    {
        const int maxPositioningRetries = 100;
        
        r ??= RandomSeed != null ? new Random(RandomSeed.Value) : new Random();
        
        while (count > 0)
        {
            count--;

            Vector2 position = GetRandomNonOverlappingPosition(radius, maxPositioningRetries);
            Vector2 velocity = r.RandomDirection2() * speed;
            particleConsts.Add(new ParticleConsts(mass, radius));
            particlePosAndsVels.Add(new PositionAndVelocity(position, velocity));
        }
        
        return this;
    }
}