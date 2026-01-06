using Common.Extensions.NpOn.CommonEnums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Common.Infrastructures.NpOn.CommonDb.DbCommands;

public interface INpOnDbExecCommand
{
    string FuncName { get; }

    Dictionary<string, object> Params { get; }
    // for output
    string AliasForSingleColumnOutput { get; }
    bool IsValidCommandText { get; }
    EDb DataBaseType { get; }
    EDbLanguage? DatabaseLanguage { get; }
}

public class NpOnDbExecCommand : INpOnDbExecCommand
{
    private readonly EDb _eDb;
    private readonly string? _funcName;
    private readonly Dictionary<string, object> _parameters;
    private readonly EDbLanguage? _dbLanguage;
    private readonly ILogger<NpOnDbCommand> _logger = new Logger<NpOnDbCommand>(new NullLoggerFactory());
    private readonly string? _aliasForSingleColumnOutput;

    public NpOnDbExecCommand(EDb eDb, string? funcName, Dictionary<string, object> parameters,
        string? aliasForSingleColumnOutput = null)
    {
        if (string.IsNullOrWhiteSpace(funcName))
            return; // funcName endpoint does not null or empty
        if (parameters is not { Count: > 0 })
            _parameters = [];
        else
            _parameters = parameters;
        _funcName = funcName;
        _aliasForSingleColumnOutput = aliasForSingleColumnOutput;
        try
        {
            _eDb = eDb;
            _dbLanguage = _eDb.ChooseLanguage();
            // _commandText = commandText;
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    public NpOnDbExecCommand(EDb eDb, string? funcName, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(funcName))
            return; // funcName endpoint does not null or empty

        _funcName = funcName;
        _parameters = new Dictionary<string, object>();

        if (parameters is { Length: > 0 })
        {
            if (parameters.Length % 2 != 0)
                return;
            // throw new ArgumentException("Parameters must be provided in key-value pairs.", nameof(parameters));

            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (parameters[i] is string key)
                    _parameters[key] = parameters[i + 1];
                else
                    return;
                // throw new ArgumentException($"Parameter key at index {i} must be a string.", nameof(parameters));
            }
        }

        try
        {
            _eDb = eDb;
            _dbLanguage = _eDb.ChooseLanguage();
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
    public string? AliasForSingleColumnOutput => _aliasForSingleColumnOutput;
    public bool IsValidCommandText => CheckValid();

    public string FuncName => _funcName ?? string.Empty;
    public Dictionary<string, object> Params => _parameters;

    public EDb DataBaseType => _eDb;

    public EDbLanguage? DatabaseLanguage => _dbLanguage;
}