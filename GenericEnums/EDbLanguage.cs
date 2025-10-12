using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EDbLanguage //: Int32
{
    [Display(Name = "Unknown")] Unknown = 1 << 0,
    [Display(Name = "Sql")] Sql = 1 << 1,
    [Display(Name = "Cql")] Cql = 1 << 2,
    [Display(Name = "Bson")] Bson = 1 << 3,
}