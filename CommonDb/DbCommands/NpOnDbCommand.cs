using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonDb.DbCommands;

public interface INpOnDbCommand
{
    string CommandText { get; }
    bool IsValidCommandText { get; }
    EDb DataBaseType { get; }
    EDbLanguage? DatabaseLanguage { get; }
}

public class NpOnDbCommand : INpOnDbCommand
{
    private readonly EDb _eDb;
    private readonly string? _commandText;
    private readonly EDbLanguage? _dbLanguage;
    private readonly ILogger<NpOnDbCommand> _logger = new Logger<NpOnDbCommand>(new NullLoggerFactory());

    public NpOnDbCommand(EDb eDb, string? commandText)
    {
        _commandText = commandText ?? string.Empty;
        try
        {
            _eDb = eDb;
            _dbLanguage = _eDb.ChooseLanguage();
            _commandText = commandText;
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    protected virtual bool CheckValid()
    {
        return true; // default 
    }

    // implements
    public bool IsValidCommandText => CheckValid();

    public string CommandText => _commandText ?? string.Empty;

    public EDb DataBaseType => _eDb;

    public EDbLanguage? DatabaseLanguage => _dbLanguage;
}