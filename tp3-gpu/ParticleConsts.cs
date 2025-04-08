using System.Runtime.InteropServices;

namespace tp3_gpu;

[StructLayout(LayoutKind.Sequential)]
public struct ParticleConsts
{
    public float Mass { get; set; }
    public float Radius { get; set; }
    
    public ParticleConsts(float mass, float radius)
    {
        Mass = mass;
        Radius = radius;
    }
}