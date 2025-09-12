using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonDb.Connections;

public class DbConnect : IDbConnect
{
    public string? ConnectString { get; private set; }
    public bool IsConnected { get; private set; } = false;
    private IDbDriver? _dbDriver;
    private readonly ILogger _logger = new Logger<DbConnect>(new NullLoggerFactory());

    public async Task Connect(string connectString, IDbDriver? driver = null)
    {
        try
        {
            ConnectString = connectString;
            _dbDriver = driver;
            if (_dbDriver == null)
                throw new Exception($"Connect failure");
            await _dbDriver.Connect();
            IsConnected = true; 
        }
        catch (Exception ex)
        {
            IsConnected = false; 
            _logger.LogError(ex.Message);
        }
    }

    public async Task Disconnect()
    {
        try
        {
            if (_dbDriver != null)
                await _dbDriver.DisConnect();
            _dbDriver = null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Disconnect failure");
        }
        finally
        {
            IsConnected = false;

        }
    }

    public Task<IDbDriver> Transact()
    {
        throw new NotImplementedException();
    }

    public Task<IDbDriver> Execute()
    {
        throw new NotImplementedException();
    }

    public Task<IDbDriver> Query()
    {
        throw new NotImplementedException();
    }

    public void SetTimeout()
    {
        throw new NotImplementedException();
    }
}