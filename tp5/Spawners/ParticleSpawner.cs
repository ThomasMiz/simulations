namespace tp5.Spawners;

public abstract class ParticleSpawner
{
    public Simulation Simulation { get; set; }

    public void OnInitialized()
    {
        OnInitializedImpl();
    }
    
    protected abstract void OnInitializedImpl();
    
    public abstract void PreUpdate();
}