using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class ErrorHandlingInterceptor : Interceptor
{
    private readonly ILogger<ErrorHandlingInterceptor> _logger;

    public ErrorHandlingInterceptor(ILogger<ErrorHandlingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning("RpcException: {Message}", ex.Status.Detail);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning("Timeout: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning("Not supported: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.Unimplemented, ex.Message));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var messages = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            _logger.LogWarning("Validation failed: {Messages}", messages);
            throw new RpcException(new Status(StatusCode.InvalidArgument, messages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected internal server error.");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}
