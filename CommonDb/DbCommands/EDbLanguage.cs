namespace CommonDb.DbCommands;

[Flags]
public enum EDbLanguage
{
    Unknown = 1 << 0,
    Sql = 1 << 1,
    Cql = 1 << 2,
    Bson = 1 << 3,
}