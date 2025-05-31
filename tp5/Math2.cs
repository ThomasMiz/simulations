using System.Runtime.CompilerServices;

namespace tp5;

public static class Math2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Square(float x) => x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Square(double x) => x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cube(float x) => x * x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Cube(double x) => x * x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow4(float x) => x * x * x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Pow4(double x) => x * x * x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow5(float x) => x * x * x * x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Pow5(double x) => x * x * x * x * x;
}