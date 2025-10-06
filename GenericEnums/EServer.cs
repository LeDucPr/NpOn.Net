using System.ComponentModel.DataAnnotations;

namespace Enums;

[Flags]
public enum EServer
{
    [Display(Name = "Replication")] Replication = 1 << 0,
    [Display(Name = "Clustering")] Clustering = 1 << 1,
    [Display(Name = "Sharding")] Sharding = 1 << 2,
    [Display(Name = "Partitioning")] Partitioning = 1 << 3, // (Zone)
    [Display(Name = "CachingLayer")] CachingLayer = 1 << 4,
}