using System.Numerics;
using tp5.Particles;
using TrippyGL.Utils;

namespace tp5.ParticleHandlers;

public class ConstantForceAvoidingParticleHandler : ParticleHandler
{
    public float SpawnRate { get; set; }
    private readonly float spawnEvery;
    private double nextSpawnTime = 0;

    public bool IsLeftToRight { get; }
    public Vector2 Force { get; }
    public float MaxEvasiveForce { get; }
    public float ParticleRadius { get; }
    public float ParticleSensingRadius { get; }

    private float spawnX;
    private float spawnMinY, spawnMaxY;
    private float limitX;

    private readonly Random random = new();

    public ConstantForceAvoidingParticleHandler(float spawnRate, float force, float maxEvasiveForce, float particleRadius, float particleSensingRadius, bool isLeftToRight)
    {
        // Will spawn spawnRate particles per second
        SpawnRate = spawnRate;
        spawnEvery = 1 / spawnRate;

        force = MathF.Abs(force);
        Force = new Vector2(isLeftToRight ? force : -force, 0);
        MaxEvasiveForce = maxEvasiveForce;
        ParticleRadius = particleRadius;
        ParticleSensingRadius = particleSensingRadius;
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

    protected override void OnPreUpdate(float deltaTime, double elapsed)
    {
        while (nextSpawnTime < elapsed)
        {
            nextSpawnTime += spawnEvery;
            float spawnY = random.NextFloat(spawnMinY, spawnMaxY);
            AddParticle(new Vector2(spawnX, spawnY), new ConstantForceAvoidingParticle(ParticleRadius, ParticleSensingRadius, Force, MaxEvasiveForce));
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