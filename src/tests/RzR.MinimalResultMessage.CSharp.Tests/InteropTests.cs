using RzR.MinimalResultMessage.Abstractions;
using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

public class InteropTests
{
    private static MinimalResult<T> FailFromGenericContext<T>() => MinimalResult<T>.Failure(TestData.Code, TestData.Message);

    private static MinimalResult<T> SucceedFromGenericContext<T>(T value) => MinimalResult<T>.Success(value);

    [Fact]
    public void ResultMessageConstructors_AreEmptyCodeCodeMessageAndCodeMessageExtensions_Test()
    {
        var signatures = typeof(ResultMessage).GetConstructors()
            .Select(c => string.Join(",", c.GetParameters().Select(p => p.ParameterType.Name)))
            .OrderBy(s => s.Length)
            .ToArray();

        Assert.Equal(
            new[] { "", "String", "String,String", "String,String,IReadOnlyDictionary`2" },
            signatures);
    }

    [Fact]
    public void CSharpEqualityOperatorAndEquals_OnMinimalResults_AreBothReferenceEquality_Test()
    {
        var a = MinimalResult<int>.Success(1);
        var b = MinimalResult<int>.Success(1);

        var operatorResult = a == b;

        Assert.False(operatorResult, "C# == has no op_Equality and should compare references");
        Assert.False(a.Equals(b), "Equals should also compare references");
    }

    [Fact]
    public void Types_DeclareNoEqualityOperators_Test()
    {
        Assert.Null(typeof(ResultMessage).GetMethod("op_Equality"));
        Assert.Null(typeof(MinimalResult<int>).GetMethod("op_Equality"));
    }

    [Fact]
    public void FailureWithSingleNullLiteral_BindsToReadOnlyListOverload_Test()
    {
        var r = MinimalResult<int>.Failure(null!);

        Assert.True(r.IsFailure);
        Assert.Null(r.Messages);
    }

    [Fact]
    public void FailureWithTwoNullLiterals_BindsToCodeMessageOverload_Test()
    {
        var r = MinimalResult<int>.Failure(null!, null);

        Assert.True(r.IsFailure);
        var single = Assert.Single(r.Messages);

        Assert.Null(single.Code);
        Assert.Null(single.Message);
        Assert.NotNull(single.Extensions);
    }

    [Fact]
    public void GenericStaticFailure_CalledFromOpenGenericContext_ReturnsDefaultResponse_Test()
    {
        var rInt = FailFromGenericContext<int>();
        var rString = FailFromGenericContext<string>();
        var rGuid = FailFromGenericContext<Guid>();

        Assert.True(rInt.IsFailure);
        Assert.Equal(0, rInt.Response);
        Assert.True(rString.IsFailure);
        Assert.Null(rString.Response);
        Assert.True(rGuid.IsFailure);
        Assert.Equal(Guid.Empty, rGuid.Response);
    }

    [Fact]
    public void GenericStaticSuccess_CalledFromOpenGenericContext_ReturnsValue_Test()
    {
        var payload = new Payload("p");

        var rPayload = SucceedFromGenericContext(payload);
        var rInt = SucceedFromGenericContext(7);

        Assert.True(rPayload.IsSuccess);
        Assert.Same(payload, rPayload.Response);
        Assert.True(rInt.IsSuccess);
        Assert.Equal(7, rInt.Response);
    }

    [Fact]
    public void PublicMembersAndExplicitInterfaceImplementation_Coexist_Test()
    {
        var r = MinimalResult<string>.Failure(TestData.Code, TestData.Message);
        IMinimalResult<string> i = r;

        Assert.False(r.IsSuccess);
        Assert.False(i.IsSuccess);
        Assert.True(r.IsFailure);
        Assert.True(i.IsFailure);
        Assert.Null(i.Response);
        Assert.Same(r.Messages, i.Messages);
    }
}
