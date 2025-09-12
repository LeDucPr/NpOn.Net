namespace CommonDb.Connections;

public interface IDbDriver
{
    Task<bool> Connect();
    Task<bool> DisConnect();
}