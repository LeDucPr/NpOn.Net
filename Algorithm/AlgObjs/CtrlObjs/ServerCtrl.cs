namespace Algorithm.AlgObjs.CtrlObjs;

public record ServerCtrl : BaseCtrl
{
    public string? ServerName { get; set; }
    public string? ServerType { get; set; } // to enum ??
    public string? Host { get; set; }
    public int? Port { get; set; }
}

// Enum  -- 
// Replication
// Clustering
// Sharding 
// Partitioning (Zone)
// CachingLayer