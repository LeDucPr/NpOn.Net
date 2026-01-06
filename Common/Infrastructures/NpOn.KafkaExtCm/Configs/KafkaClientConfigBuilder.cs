using Confluent.Kafka;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Configs;

public class KafkaClientConfigBuilder
{
    private string? _serverUrl;
    private string? _saslPassword;
    private string? _saslUserName;
    private SecurityProtocol _securityProtocol = SecurityProtocol.Plaintext; // not pass (default)

    // Sasl
    private bool _isUseSasl; // = false;
    private SaslMechanism? _saslMechanism;

    // Ssl 
    private bool _isUseSsl; // = false;
    private string? _sslCaLocation;
    private string? _sslCertificateLocation;
    private string? _sslKeyLocation;
    private string? _sslKeyPassword;

    public void SetServerUrl(string serverUrl) => _serverUrl = serverUrl;

    // Sasl
    public void SetUseSasl(string saslUserName, string saslPassword, SaslMechanism saslMechanism)
    {
        _isUseSasl = true;
        _saslUserName = saslUserName;
        _saslPassword = saslPassword;
        _saslMechanism = saslMechanism;
        _securityProtocol = _isUseSsl ? SecurityProtocol.SaslSsl : SecurityProtocol.SaslPlaintext;
    }


    // Ssl
    public void SetUseSsl(string sslCaLocation, string sslCertificateLocation,
        string sslKeyLocation, string sslKeyPassword)
    {
        _isUseSsl = true;
        _sslCaLocation = sslCaLocation;
        _sslCertificateLocation = sslCertificateLocation;
        _sslKeyLocation = sslKeyLocation;
        _sslKeyPassword = sslKeyPassword;
        _securityProtocol = _isUseSasl ? SecurityProtocol.SaslSsl : SecurityProtocol.Ssl;
    }

    public ClientConfig Build()
    {
        if (string.IsNullOrEmpty(_serverUrl))
        {
            throw new InvalidOperationException("Server URL must be set.");
        }

        ClientConfig clientConfig = new ClientConfig
        {
            BootstrapServers = _serverUrl,
        };

        if (_isUseSasl && !string.IsNullOrEmpty(_saslUserName) && !string.IsNullOrEmpty(_saslPassword))
        {
            clientConfig.SaslUsername = _saslUserName;
            clientConfig.SaslPassword = _saslPassword;
            clientConfig.SaslMechanism = _saslMechanism;
        }

        if (_isUseSsl)
        {
            clientConfig.SslCaLocation = _sslCaLocation;
            clientConfig.SslCertificateLocation = _sslCertificateLocation;
            clientConfig.SslKeyLocation = _sslKeyLocation;
            clientConfig.SslKeyPassword = _sslKeyPassword;
        }

        clientConfig.SecurityProtocol = _securityProtocol;

        return clientConfig;
    }
}

public static class KafkaClientConfigBuilderExtensions
{
    public static KafkaClientConfigBuilder SetServer(this KafkaClientConfigBuilder builder, string serverUrl)
    {
        builder.SetServerUrl(serverUrl);
        return builder;
    }

    public static KafkaClientConfigBuilder UseSasl(this KafkaClientConfigBuilder builder,
        string saslUserName, string saslPassword, SaslMechanism saslMechanism)
    {
        builder.SetUseSasl(saslUserName, saslPassword, saslMechanism);
        return builder;
    }

    public static KafkaClientConfigBuilder UseSsl(this KafkaClientConfigBuilder builder,
        string caLocation, string certificateLocation, string keyLocation, string keyPassword)
    {
        builder.SetUseSsl(caLocation, certificateLocation, keyLocation, keyPassword);
        return builder;
    }
}