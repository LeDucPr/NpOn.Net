namespace CommonDb.Connections;

public interface IDbConnect
{
    public string? ConnectString { get; }
    public bool IsConnected { get; }
    public Task Connect(string connectString, IDbDriver? driver);
    public Task Disconnect();
    public Task<IDbDriver> Transact();
    public Task<IDbDriver> Execute();
    public Task<IDbDriver> Query();
    public void SetTimeout();
}