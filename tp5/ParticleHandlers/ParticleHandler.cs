using System.Numerics;
using AetherPhysics.Dynamics;
using tp5.Particles;

namespace tp5.ParticleHandlers;

/// <summary>
/// Takes care of spawning, controlling and removing a group of particles on the simulation.
/// </summary>
public abstract class ParticleHandler
{
    protected LinkedList<Particle> particles = new();
    
    public IReadOnlyCollection<Particle> Particles => particles;

    public Simulation Simulation { get; private set; }

    protected void AddParticle(Vector2 position, Particle particle)
    {
        Body body = Simulation.PhysicsWorld.CreateBody(position, 0, BodyType.Dynamic);
        body.Enabled = true;
        
        LinkedListNode<Particle> node = particles.AddLast(particle);
        particle.Node = node;
        particle.Initialize(body);
    }

    protected void RemoveParticle(Particle particle)
    {
        LinkedListNode<Particle> node = particle.Node;
        particles.Remove(node);
        
        Simulation.PhysicsWorld.Remove(particle.Body);
        particle.Node = null;
    }

    public void Initialize(Simulation simulation)
    {
        Simulation = simulation;
        
        OnInitialize();
    }

    public void PreUpdate(float deltaTime, double elapsed)
    {
        foreach (Particle particle in particles)
        {
            particle.PreUpdate(deltaTime, elapsed);
        }

        OnPreUpdate(deltaTime, elapsed);
    }

    public void PostUpdate(float deltaTime, double elapsed)
    {
        foreach (Particle particle in particles)
        {
            particle.PostUpdate(deltaTime, elapsed);
        }

        OnPostUpdate(deltaTime, elapsed);
    }

    protected abstract void OnInitialize();

    protected abstract void OnPreUpdate(float deltaTime, double elapsed);

    protected abstract void OnPostUpdate(float deltaTime, double elapsed);
}