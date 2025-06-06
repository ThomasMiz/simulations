namespace tp5.Spawners;

public abstract class ParticleSpawner
{
    public Simulation Simulation { get; set; }

    public bool IsDone { get; protected set; } = false;

    public void OnInitialized()
    {
        OnInitializedImpl();
    }

    protected abstract void OnInitializedImpl();

    public abstract void PreUpdate();
}