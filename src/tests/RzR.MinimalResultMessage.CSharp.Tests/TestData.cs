using System.Collections.ObjectModel;
using RzR.MinimalResultMessage.Abstractions;
using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

public sealed class Payload
{
    public Payload(string name) => Name = name;

    public string Name { get; }
}

public readonly struct Point
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }
}

public static class TestData
{
    public const string Code = "E1";
    public const string Message = "Boom";
    public const string SuccessValue = "ok";
    public const int SuccessInt = 42;

    public const string Success = "success";
    public const string SuccessNull = "success-null";
    public const string FailureCode = "failure-code";
    public const string FailureList = "failure-list";
    public const string FailureEmpty = "failure-empty";
    public const string FailureNull = "failure-null";

    public const string ShapeArray = "array";
    public const string ShapeGenericList = "generic-list";
    public const string ShapeReadOnlyCollection = "readonly-collection";

    public static Dictionary<string, object> NewExtensions() => new Dictionary<string, object> { ["k"] = 1 };

    public static ResultMessage NewMessage(string code, string message) =>
        new ResultMessage(code, message, NewExtensions());

    public static ResultMessage[] TwoMessages() => new[]
    {
        NewMessage("E1", "Boom"),
        NewMessage("E2", "Bang"),
    };

    public static bool ExpectSuccess(string scenario) =>
        scenario.StartsWith("success", StringComparison.Ordinal);

    public static MinimalResult<string?> BuildString(string scenario) => scenario switch
    {
        Success => MinimalResult<string?>.Success(SuccessValue),
        SuccessNull => MinimalResult<string?>.Success(null),
        FailureCode => MinimalResult<string?>.Failure(Code, Message),
        FailureList => MinimalResult<string?>.Failure(new List<ResultMessage>(TwoMessages())),
        FailureEmpty => MinimalResult<string?>.Failure(new List<ResultMessage>()),
        FailureNull => MinimalResult<string?>.Failure((IReadOnlyList<ResultMessage>)null!),
        _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown scenario key."),
    };

    public static MinimalResult<int> BuildInt(string scenario) => scenario switch
    {
        Success => MinimalResult<int>.Success(SuccessInt),
        FailureCode => MinimalResult<int>.Failure(Code, Message),
        FailureList => MinimalResult<int>.Failure(new List<ResultMessage>(TwoMessages())),
        FailureEmpty => MinimalResult<int>.Failure(new List<ResultMessage>()),
        FailureNull => MinimalResult<int>.Failure((IReadOnlyList<ResultMessage>)null!),
        _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown scenario key."),
    };

    public static IReadOnlyList<ResultMessage> Wrap(string shape, ResultMessage[] items) => shape switch
    {
        ShapeArray => items,
        ShapeGenericList => new List<ResultMessage>(items),
        ShapeReadOnlyCollection => new ReadOnlyCollection<ResultMessage>(items),
        _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unknown shape key."),
    };

    public static void AssertEqualPair(object a, object b)
    {
        Assert.True(a.Equals(b), "a.Equals(b) expected true");
        Assert.True(b.Equals(a), "b.Equals(a) expected true (symmetry)");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    public static void AssertNotEqualPair(object a, object b)
    {
        Assert.False(a.Equals(b), "a.Equals(b) expected false");
        Assert.False(b.Equals(a), "b.Equals(a) expected false");
    }

    public static IMinimalResult<int> Calculate(int val1, int val2)
    {
        var res = val1 + val2;

        return MinimalResult<int>.Success(res);
    }
}
