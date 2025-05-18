using Silk.NET.Maths;

namespace tp4;

public class GravityPeriodicSolutions
{
    // https://nhsjs.com/2024/the-stability-of-three-body-solutions-with-a-fourth-body/
    
    public static readonly List<ParticleState> Solution1 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.347111, 0.532728) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.347111, 0.532728) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.347111, -2 * 0.532728) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution2 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.306893, 0.125507) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.306893, 0.125507) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.306893, -2 * 0.125507) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution3 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.392955, 0.097579) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.392955, 0.097579) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.392955, -2 * 0.097579) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution4 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.184279, 0.587188) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.184279, 0.587188) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.184279, -2 * 0.587188) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution5 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.080584, 0.58836) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.080584, 0.58836) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.080584, -2 * 0.58836) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution6 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.083300, 0.127889) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.083300, 0.127889) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.083300, -2 * 0.127889) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution7 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.464445, 0.396060) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.464445, 0.396060) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.464445, -2 * 0.396060) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState> Solution8 =
    [
        new ParticleState { Position = new Vector2D<double>(-1, 0), Velocity = new Vector2D<double>(0.439166, 0.452968) },
        new ParticleState { Position = new Vector2D<double>(1, 0), Velocity = new Vector2D<double>(0.439166, 0.452968) },
        new ParticleState { Position = new Vector2D<double>(0, 0), Velocity = new Vector2D<double>(-2 * 0.439166, -2 * 0.452968) },
        //new ParticleState { Position = new Vector2D<double>(0, 0.5), Velocity = new Vector2D<double>(0, 0) }
    ];

    public static readonly List<ParticleState>[] All = [Solution1, Solution2, Solution3, Solution4, Solution5, Solution6, Solution7, Solution8];
}