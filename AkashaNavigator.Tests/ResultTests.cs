using System;
using Xunit;
using AkashaNavigator.Models.Common;

namespace AkashaNavigator.Tests
{
/// <summary>
/// Result类型的单元测试
/// </summary>
public class ResultTests
{
    [Fact]
    public void Result_Success_ShouldReturnSuccessResult()
    {
        // Arrange & Act
        var result = Result<int>.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Result_Failure_WithError_ShouldReturnFailureResult()
    {
        // Arrange
        var error = Error.Validation("INVALID_INPUT", "Invalid input provided");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(0, result.Value); // default for int
        Assert.Same(error, result.Error);
    }

    [Fact]
    public void Result_Failure_WithString_ShouldCreateValidationError()
    {
        // Arrange & Act
        var result = Result<int>.Failure("Something went wrong");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Something went wrong", result.Error?.Message);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Equal("VALIDATION_ERROR", result.Error?.Code);
    }

    [Fact]
    public void Result_Failure_WithException_ShouldCreateErrorFromException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = Result<int>.Failure(exception);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Test exception", result.Error?.Message);
        Assert.Same(exception, result.Error?.Exception);
        Assert.Equal("SYSTEM_ERROR", result.Error?.Code);
    }

    [Fact]
    public void Result_Map_ShouldTransformValue_OnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(5);

        // Act
        var doubled = result.Map(x => x * 2);

        // Assert
        Assert.True(doubled.IsSuccess);
        Assert.Equal(10, doubled.Value);
    }

    [Fact]
    public void Result_Map_ShouldPropagateError_OnFailure()
    {
        // Arrange
        var error = Error.Validation("ERROR", "Error");
        var result = Result<int>.Failure(error);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.Same(error, mapped.Error);
    }

    [Fact]
    public void Result_Bind_ShouldChainOperations_OnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var chained = result.Bind(x => Result<int>.Success(x * 2)).Bind(x => Result<int>.Success(x + 5));

        // Assert
        Assert.True(chained.IsSuccess);
        Assert.Equal(25, chained.Value);
    }

    [Fact]
    public void Result_Bind_ShouldShortCircuit_OnFailure()
    {
        // Arrange
        var error = Error.Validation("ERROR", "Error");
        var result = Result<int>.Failure(error);

        // Act
        var chained = result.Bind(x => Result<int>.Success(x * 2));

        // Assert
        Assert.True(chained.IsFailure);
        Assert.Same(error, chained.Error);
    }

    [Fact]
    public void Result_OrElse_ShouldReturnOriginal_OnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var fallback = result.OrElse(() => Result<int>.Success(999));

        // Assert
        Assert.True(fallback.IsSuccess);
        Assert.Equal(10, fallback.Value);
    }

    [Fact]
    public void Result_OrElse_ShouldReturnFallback_OnFailure()
    {
        // Arrange
        var result = Result<int>.Failure(Error.Validation("ERROR", "Error"));

        // Act
        var fallback = result.OrElse(() => Result<int>.Success(999));

        // Assert
        Assert.True(fallback.IsSuccess);
        Assert.Equal(999, fallback.Value);
    }

    [Fact]
    public void Result_GetValueOrDefault_ShouldReturnValue_OnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.GetValueOrDefault(0);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void Result_GetValueOrDefault_ShouldReturnDefault_OnFailure()
    {
        // Arrange
        var result = Result<int>.Failure(Error.Validation("ERROR", "Error"));

        // Act
        var value = result.GetValueOrDefault(999);

        // Assert
        Assert.Equal(999, value);
    }

    [Fact]
    public void Result_Match_ShouldCallSuccess_OnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var called = false;
        int? receivedValue = null;

        // Act
        result.Match(
                onSuccess: value => { called = true; receivedValue = value; },
                onFailure: error => { }
            );

        // Assert
        Assert.True(called);
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void Result_Match_ShouldCallFailure_OnFailure()
    {
        // Arrange
        var error = Error.Validation("ERROR", "Error message");
        var result = Result<int>.Failure(error);
        var called = false;
        Error? receivedError = null;

        // Act
        result.Match(
                onSuccess: value => { },
                onFailure: err => { called = true; receivedError = err; }
            );

        // Assert
        Assert.True(called);
        Assert.Same(error, receivedError);
    }

    [Fact]
    public void Result_OnSuccess_ShouldExecuteAction_OnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var executed = false;

        // Act
        result.OnSuccess(value => executed = true);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void Result_OnSuccess_ShouldNotExecuteAction_OnFailure()
    {
        // Arrange
        var result = Result<int>.Failure(Error.Validation("ERROR", "Error"));
        var executed = false;

        // Act
        result.OnSuccess(value => executed = true);

        // Assert
        Assert.False(executed);
    }

    [Fact]
    public void Result_OnFailure_ShouldExecuteAction_OnFailure()
    {
        // Arrange
        var error = Error.Validation("ERROR", "Error");
        var result = Result<int>.Failure(error);
        var executed = false;

        // Act
        result.OnFailure(err => executed = true);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void Result_ToResult_ShouldConvertSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var converted = result.ToResult();

        // Assert
        Assert.True(converted.IsSuccess);
    }

    [Fact]
    public void Result_ImplicitConversion_ShouldWork()
    {
        // Arrange
        Result<int> result = 42;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }
}

/// <summary>
/// Error类型的单元测试
/// </summary>
public class ErrorTests
{
    [Fact]
    public void Error_FileSystem_ShouldCreateFileSystemError()
    {
        // Act
        var error = Error.FileSystem("FILE_NOT_FOUND", "File not found", filePath: "/test/path.txt");

        // Assert
        Assert.Equal(ErrorCategory.FileSystem, error.Category);
        Assert.Equal("FILE_NOT_FOUND", error.Code);
        Assert.Equal("File not found", error.Message);
        Assert.True(error.Metadata.ContainsKey("FilePath"));
        Assert.Equal("/test/path.txt", error.Metadata["FilePath"]);
    }

    [Fact]
    public void Error_Validation_ShouldCreateValidationError()
    {
        // Act
        var error = Error.Validation("INVALID_INPUT", "Input is invalid", "请输入有效的内容");

        // Assert
        Assert.Equal(ErrorCategory.Validation, error.Category);
        Assert.Equal("INVALID_INPUT", error.Code);
        Assert.Equal("Input is invalid", error.Message);
        Assert.Equal("请输入有效的内容", error.UserMessage);
    }

    [Fact]
    public void Error_Permission_ShouldCreatePermissionError()
    {
        // Act
        var error = Error.Permission("ACCESS_DENIED", "Access denied", "权限不足");

        // Assert
        Assert.Equal(ErrorCategory.Permission, error.Category);
        Assert.Equal("ACCESS_DENIED", error.Code);
        Assert.Equal("权限不足", error.UserMessage);
    }

    [Fact]
    public void Error_Plugin_ShouldCreatePluginError()
    {
        // Act
        var error = Error.Plugin("LOAD_FAILED", "Failed to load plugin", pluginId: "test-plugin");

        // Assert
        Assert.Equal(ErrorCategory.Plugin, error.Category);
        Assert.Equal("LOAD_FAILED", error.Code);
        Assert.True(error.Metadata.ContainsKey("PluginId"));
        Assert.Equal("test-plugin", error.Metadata["PluginId"]);
    }

    [Fact]
    public void Error_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var error = Error.Validation("INVALID_INPUT", "Input is invalid");

        // Act
        var str = error.ToString();

        // Assert
        Assert.Contains("[Validation]", str);
        Assert.Contains("INVALID_INPUT", str);
        Assert.Contains("Input is invalid", str);
    }
}
}
