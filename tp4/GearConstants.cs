namespace tp4;

public struct GearConstants
{
    public float a0 { get; }
    public float a1 { get; }
    public float a2 { get; }
    public float a3 { get; }
    public float a4 { get; }
    public float a5 { get; }

    public GearConstants(float a0, float a1, float a2, float a3, float a4, float a5)
    {
        this.a0 = a0;
        this.a1 = a1;
        this.a2 = a2;
        this.a3 = a3;
        this.a4 = a4;
        this.a5 = a5;
    }

    public static readonly GearConstants PositionDependentForces = new(3f / 20f, 251f / 360f, 1, 11f / 18f, 1f / 6f, 1f / 60f);

    public static readonly GearConstants PositionAndVelocityDependentForces = new(3f / 16f, 251f / 360f, 1, 11f / 18f, 1f / 6f, 1f / 60f);
}