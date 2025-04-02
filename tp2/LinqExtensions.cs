namespace tp2;

using System;
using System.Linq;
using System.Collections.Generic;

public static class LinqExtensions
{
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        double avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }
    
    public static float StandardDeviation(this IEnumerable<float> values)
    {
        float avg = values.Average();
        return MathF.Sqrt(values.Average(v => MathF.Pow(v - avg, 2)));
    }
}