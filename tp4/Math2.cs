using System.Runtime.CompilerServices;

namespace tp4;

public static class Math2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Square(float x) => x * x;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static double Square(double x) => x * x;
}