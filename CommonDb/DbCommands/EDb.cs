namespace CommonDb.DbCommands;

public enum EDb : byte
{
    Unknown = 0,
    Cassandra = 1,
    ScyllaDb = 2, 
    Postgres = 3,
    MongoDb = 4,
}

public static class EDbExtension{
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
}