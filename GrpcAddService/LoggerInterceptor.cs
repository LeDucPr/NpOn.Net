using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcAddService;

public class LoggerInterceptor(
    ILogger<LoggerInterceptor> logger,
    IHttpContextAccessor? httpContextAccessor
) : Interceptor
{
    private const string MicroserviceCallerUserName = "caller-user";
    private const string MicroserviceCallerMachineName = "caller-machine";
    private const string MicroserviceCallerOsVersion = "caller-os";
    private const string MicroserviceCallerSessionCode = "caller-vnnss"; // session
    public string? Host { get; set; }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        AddCallerMetadata(ref context);
        var call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(
            HandleResponse(context.Method.ServiceName, context.Method.Name, request,
                call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus, call.GetTrailers, call.Dispose);
    }

    private async Task<TResponse> HandleResponse<TResponse>(string serviceName, string action, object request,
        Task<TResponse> t)
    {
        var startTime = Stopwatch.GetTimestamp();
        var actionTracking = $"/{serviceName}/{action}";
        try
        {
            var response = await t;
            var executeTime = Stopwatch.GetElapsedTime(startTime);
            logger.LogInformation("Service Request host {Host} url {ActionTracking} executeTime {ExecuteTime}",
                Host,
                actionTracking,
                executeTime);
            return response;
        }
        catch (Exception ex)
        {
            var executeTime = Stopwatch.GetElapsedTime(startTime);
            // Log error to the console.
            // Note: Configuring .NET Core logging is the recommended way to log errors
            // https://docs.microsoft.com/aspnet/core/grpc/diagnostics#grpc-client-logging
            // var initialColor = Console.ForegroundColor;
            // Console.ForegroundColor = ConsoleColor.Red;
            LogError(ex,
                $"GRPC call error - ServiceName: {serviceName} - Action: {action} - Message: {ex.Message}", serviceName,
                action);
            // Console.ForegroundColor = initialColor;
            throw;
        }
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        AddCallerMetadata(ref context);
        return continuation(context);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        AddCallerMetadata(ref context);
        return continuation(request, context);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        AddCallerMetadata(ref context);
        return continuation(context);
    }

    private void AddCallerMetadata<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
    {
        var headers = context.Options.Headers;

        // Call doesn't have a headers collection to add to.
        // Need to create a new context with headers for the call.
        if (headers == null)
        {
            headers = new Metadata();
            var options = context.Options.WithHeaders(headers);
            context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
        }

        var callerUserEntry = headers.Get(MicroserviceCallerUserName);
        var callerMachineEntry = headers.Get(MicroserviceCallerMachineName);
        var callerOsEntry = headers.Get(MicroserviceCallerOsVersion);
        var callerSessionCodeEntry = headers.Get(MicroserviceCallerSessionCode);
        var callerSessionCode = (callerSessionCodeEntry?.Value) ?? string.Empty;
        if (callerUserEntry != null)
        {
            headers.Remove(callerUserEntry);
        }

        if (callerMachineEntry != null)
        {
            headers.Remove(callerMachineEntry);
        }

        if (callerOsEntry != null)
        {
            headers.Remove(callerOsEntry);
        }

        if (callerSessionCodeEntry != null)
        {
            headers.Remove(callerSessionCodeEntry);
        }

        // Add caller metadata to call headers
        headers.Add(MicroserviceCallerUserName, Environment.UserName);
        headers.Add(MicroserviceCallerMachineName, Environment.MachineName);
        headers.Add(MicroserviceCallerOsVersion, Environment.OSVersion.ToString());
        headers.Add(MicroserviceCallerSessionCode, callerSessionCode);
        if (httpContextAccessor != null)
        {
            string? authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
            if (authorizationHeader?.Length > 0)
            {
                headers.Add("Authorization", authorizationHeader);
            }
        }
    }

    private void LogError(Exception exception, string message, string serviceName, string action)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "CallToHost", Host ?? string.Empty },
                   { "CallToServiceName", serviceName },
                   { "CallToAction", action }
               }))
        {
            logger.LogError(exception, "{Message}", message);
        }
    }
}