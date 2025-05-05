namespace tp4;

public struct GearConstants
{
    public double a0 { get; }
    public double a1 { get; }
    public double a2 { get; }
    public double a3 { get; }
    public double a4 { get; }
    public double a5 { get; }

    public GearConstants(double a0, double a1, double a2, double a3, double a4, double a5)
    {
        this.a0 = a0;
        this.a1 = a1;
        this.a2 = a2;
        this.a3 = a3;
        this.a4 = a4;
        this.a5 = a5;
    }

    public static readonly GearConstants PositionDependentForces = new(3.0 / 20.0, 251.0 / 360.0, 1, 11.0 / 18.0, 1.0 / 6.0, 1.0 / 60.0);

    public static readonly GearConstants PositionAndVelocityDependentForces = new(3.0 / 16.0, 251.0 / 360.0, 1, 11.0 / 18.0, 1.0 / 6.0, 1.0 / 60.0);
}