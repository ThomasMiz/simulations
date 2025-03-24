namespace tp2;

public struct ClusterStats
{
    public int ClusterCount { get; set; }
    public int MinClusterSize { get; set; }
    public int MaxClusterSize { get; set; }
    public float AverageClusterSize { get; set; }

    public override string ToString()
    {
        return $"ClusterCount={ClusterCount}, MinClusterSize={MinClusterSize}, MaxClusterSize={MaxClusterSize}, AverageClusterSize={AverageClusterSize}";
    }
}