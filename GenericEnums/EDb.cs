using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EDb : byte
{
    [Display(Name = "Unknown")] Unknown = 0,
    [Display(Name = "Cassandra")] Cassandra = 1,
    [Display(Name = "ScyllaDb")] ScyllaDb = 2,
    [Display(Name = "Postgres")] Postgres = 3,
    [Display(Name = "MongoDb")] MongoDb = 4,
}

public static class EDbExtension
{
    public static EDbLanguage ChooseLanguage(this EDb db)
    {
        return db switch
        {
            EDb.Unknown => throw new NotSupportedException($"The database language for '{db}' is not supported."),
            EDb.Cassandra => EDbLanguage.Cql,
            EDb.ScyllaDb => EDbLanguage.Cql,
            EDb.Postgres => EDbLanguage.Sql,
            EDb.MongoDb => EDbLanguage.Bson,
            _ => throw new NotSupportedException($"The database language for '{db}' is not supported."),
        };
    }

    public static bool IsValid(this EDb dbType)
    {
        List<EDb> validTypes =
        [
            EDb.Cassandra,
            EDb.ScyllaDb,
            EDb.Postgres,
            EDb.MongoDb
        ];
        if (validTypes.Contains(dbType)) return true;
        return false;
    }
}