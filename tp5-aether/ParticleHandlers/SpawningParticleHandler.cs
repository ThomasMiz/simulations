using System.Numerics;
using tp5.Particles;
using TrippyGL.Utils;

namespace tp5.ParticleHandlers;

/// <summary>
/// A particle handler that spawns particles with a given delegate and removes them automatically once they reach their
/// destination.
/// </summary>
public abstract class SpawningParticleHandler : ParticleHandler
{
    public float SpawnRate { get; set; }
    private readonly float spawnEvery;
    private double nextSpawnTime = 0;

    public bool IsLeftToRight { get; }
    public float ParticleRadius { get; }

    private float spawnX;
    private float spawnMinY, spawnMaxY;
    private float limitX;

    protected readonly Random random = new();

    public SpawningParticleHandler(float spawnRate, float particleRadius, bool isLeftToRight)
    {
        // Will spawn spawnRate particles per second
        SpawnRate = spawnRate;
        spawnEvery = 1 / spawnRate;

        ParticleRadius = particleRadius;
        IsLeftToRight = isLeftToRight;
    }

    protected override void OnInitialize()
    {
        nextSpawnTime = Simulation.SecondsElapsed + spawnEvery;

        Rectangle bounds = Simulation.Bounds;

        spawnMinY = bounds.Bottom + ParticleRadius;
        spawnMaxY = bounds.Top - ParticleRadius;

        spawnX = IsLeftToRight ? bounds.Left : bounds.Right;
        limitX = IsLeftToRight ? bounds.Right : bounds.Left;
    }

    protected abstract Particle SpawnParticle();

    protected override void OnPreUpdate(float deltaTime, double elapsed)
    {
        while (nextSpawnTime < elapsed)
        {
            nextSpawnTime += spawnEvery;
            float spawnY = random.NextFloat(spawnMinY, spawnMaxY);
            AddParticle(new Vector2(spawnX, spawnY), SpawnParticle());
        }
    }

    protected override void OnPostUpdate(float deltaTime, double elapsed)
    {
        LinkedListNode<Particle> node = particles.First;

        while (node != null)
        {
            LinkedListNode<Particle> next = node.Next;
            if ((node.Value.Body.Position.X > limitX) == IsLeftToRight)
            {
                RemoveParticle(node.Value);
            }

            node = next;
        }
    }
}