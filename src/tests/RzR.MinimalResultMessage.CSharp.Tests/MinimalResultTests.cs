using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

public class MinimalResultTests
{
    [Fact]
    public void Success_WithString_IsSuccessAndResponseIsSameReference_Test()
    {
        var value = new string('o', 1) + "k";

        var r = MinimalResult<string>.Success(value);

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Same(value, r.Response);
    }

    [Fact]
    public void Success_WithCustomClassInstance_IsSuccessAndResponseIsSameReference_Test()
    {
        var payload = new Payload("p");

        var r = MinimalResult<Payload>.Success(payload);

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Same(payload, r.Response);
    }

    [Fact]
    public void Success_WithInt_ResponseEqualsValue_Test()
    {
        var r = MinimalResult<int>.Success(42);

        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Response);
    }

    [Fact]
    public void Success_WithGuid_ResponseEqualsValue_Test()
    {
        var value = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");

        var r = MinimalResult<Guid>.Success(value);

        Assert.True(r.IsSuccess);
        Assert.Equal(value, r.Response);
    }

    [Fact]
    public void Success_WithDateTime_ResponseEqualsValue_Test()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var r = MinimalResult<DateTime>.Success(value);

        Assert.True(r.IsSuccess);
        Assert.Equal(value, r.Response);
        Assert.Equal(DateTimeKind.Utc, r.Response.Kind);
    }

    [Fact]
    public void Success_WithCustomStruct_ResponseEqualsValue_Test()
    {
        var value = new Point(3, -7);

        var r = MinimalResult<Point>.Success(value);

        Assert.True(r.IsSuccess);
        Assert.Equal(3, r.Response.X);
        Assert.Equal(-7, r.Response.Y);
    }

    [Fact]
    public void Success_WithDefaultInt_IsSuccessAndResponseIsZero_Test()
    {
        var r = MinimalResult<int>.Success(0);

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Equal(0, r.Response);
    }

    [Fact]
    public void Success_WithEmptyGuid_IsSuccessAndResponseIsEmptyGuid_Test()
    {
        var r = MinimalResult<Guid>.Success(Guid.Empty);

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Equal(Guid.Empty, r.Response);
    }

    [Fact]
    public void Success_Messages_IsNotNullAndEmpty_Test()
    {
        var r = MinimalResult<int>.Success(42);
        Assert.NotNull(r.Messages);

        var count = r.Messages.Count;
        Assert.Equal(0, count);
        Assert.Empty(r.Messages);
    }

    [Fact]
    public void Success_WithNullString_IsSuccessWithNullResponseAndEmptyMessages_Test()
    {
        var r = MinimalResult<string?>.Success(null);

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Null(r.Response);
        Assert.NotNull(r.Messages);
        Assert.Empty(r.Messages);
    }

    [Fact]
    public void Success_WithNullNullableInt_IsSuccessWithoutValue_Test()
    {
        var r = MinimalResult<int?>.Success(null);

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.False(r.Response.HasValue);
    }

    [Fact]
    public void LibraryCreatedMessages_AreNotMutableCollections_Test()
    {
        var success = MinimalResult<int>.Success(1);
        var failure = MinimalResult<int>.Failure(TestData.Code, TestData.Message);

        var successIsCollection = success.Messages is ICollection<ResultMessage>;
        var failureIsCollection = failure.Messages is ICollection<ResultMessage>;

        Assert.False(successIsCollection, "Success.Messages should not be ICollection<ResultMessage>");
        Assert.False(failureIsCollection, "Failure(code,message).Messages should not be ICollection<ResultMessage>");
    }

    [Fact]
    public void Success_TwoCalls_ShareTheSameEmptyMessagesInstance_Test()
    {
        var r1 = MinimalResult<int>.Success(1);
        var r2 = MinimalResult<int>.Success(2);

        Assert.Same(r1.Messages, r2.Messages);
    }

    [Fact]
    public void FailureWithCode_IsFailureAndNotSuccess_Test()
    {
        var r = MinimalResult<int>.Failure("E1", "Boom");

        Assert.False(r.IsSuccess);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void FailureWithCode_IntResponse_IsZero_Test()
    {
        var r = MinimalResult<int>.Failure("E1", "Boom");

        Assert.Equal(0, r.Response);
    }

    [Fact]
    public void FailureWithCode_StringResponse_IsNull_Test()
    {
        var r = MinimalResult<string>.Failure("E1", "Boom");

        Assert.Null(r.Response);
    }

    [Fact]
    public void FailureWithCode_GuidResponse_IsEmptyGuid_Test()
    {
        var r = MinimalResult<Guid>.Failure("E1", "Boom");

        Assert.Equal(Guid.Empty, r.Response);
    }

    [Fact]
    public void FailureWithCode_NullableIntResponse_HasNoValue_Test()
    {
        var r = MinimalResult<int?>.Failure("E1", "Boom");

        Assert.True(r.IsFailure);
        Assert.False(r.Response.HasValue);
    }

    [Theory]
    [InlineData("E1", "Boom")]
    [InlineData("  E1  ", "  padded  ")]
    public void FailureWithCode_HasExactlyOneMessageWithVerbatimCodeAndMessage_Test(string code, string message)
    {
        var r = MinimalResult<int>.Failure(code, message);

        var single = Assert.Single(r.Messages);
        Assert.Equal(code, single.Code);
        Assert.Equal(message, single.Message);
    }

    [Fact]
    public void FailureWithCode_Extensions_IsNotNullAndEmpty_Test()
    {
        var ext = MinimalResult<int>.Failure("E1", "Boom").Messages[0].Extensions;

        Assert.NotNull(ext);
        var count = ext.Count;
        Assert.Equal(0, count);
        Assert.False(ext.ContainsKey("x"));
        Assert.False(ext.TryGetValue("x", out _));
        Assert.Empty(ext);
    }

    [Fact]
    public void FailureWithCode_Extensions_IsNotAMutableDictionaryInstance_Test()
    {
        var ext = MinimalResult<int>.Failure("E1", "Boom").Messages[0].Extensions;

        var isDictionary = ext is Dictionary<string, object>;

        Assert.False(isDictionary, "Extensions must not be a concrete Dictionary<string, object>");
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void FailureWithCode_NullOrEmptyCodeAndMessage_IsFailureWithThoseValues_Test(string? code, string? message)
    {
        var r = MinimalResult<int>.Failure(code!, message);

        Assert.True(r.IsFailure);
        var single = Assert.Single(r.Messages);

        Assert.Equal(code, single.Code);
        Assert.Equal(message, single.Message);
    }

    [Fact]
    public void FailureWithCode_TwoIdenticalCalls_ShareExtensionsButAreDistinctNotEqualResults_Test()
    {
        var r1 = MinimalResult<int>.Failure("E1", "Boom");
        var r2 = MinimalResult<int>.Failure("E1", "Boom");

        Assert.Same(r1.Messages[0].Extensions, r2.Messages[0].Extensions);
        TestData.AssertNotEqualPair(r1, r2);
        Assert.NotSame(r1, r2);
    }

    [Fact]
    public void FailureWithCode_DifferentTypeArguments_ShareTheSameExtensionsInstance_Test()
    {
        var rInt = MinimalResult<int>.Failure("E1", "Boom");
        var rString = MinimalResult<string>.Failure("E1", "Boom");

        Assert.Same(rInt.Messages[0].Extensions, rString.Messages[0].Extensions);
    }

    [Fact]
    public void FailureWithMessages_IntResponse_IsFailureWithDefaultResponse_Test()
    {
        var r = MinimalResult<int>.Failure(new List<ResultMessage>(TestData.TwoMessages()));

        Assert.False(r.IsSuccess);
        Assert.True(r.IsFailure);
        Assert.Equal(0, r.Response);
    }

    [Fact]
    public void FailureWithMessages_StringResponse_IsFailureWithNullResponse_Test()
    {
        var r = MinimalResult<string>.Failure(new List<ResultMessage>(TestData.TwoMessages()));

        Assert.False(r.IsSuccess);
        Assert.True(r.IsFailure);
        Assert.Null(r.Response);
    }

    [Fact]
    public void FailureWithMessages_HundredItemsWithDuplicates_PreservesCountOrderAndReferences_Test()
    {
        var pool = Enumerable.Range(0, 7).Select(i => TestData.NewMessage("E" + i, "M" + i)).ToArray();
        var list = new List<ResultMessage>();
        for (var i = 0; i < 100; i++)
        {
            list.Add(pool[i % pool.Length]);
        }

        var r = MinimalResult<int>.Failure(list);

        Assert.Equal(100, r.Messages.Count);
        for (var i = 0; i < 100; i++)
        {
            Assert.Same(list[i], r.Messages[i]);
        }
    }

    [Fact]
    public void FailureWithMessages_MutableList_IsAliasedAndMutationsAreVisible_Test()
    {
        var list = new List<ResultMessage>(TestData.TwoMessages());
        var late = TestData.NewMessage("E3", "Late");

        var r = MinimalResult<int>.Failure(list);

        Assert.Same(list, r.Messages);
        list.Add(late);
        Assert.Equal(3, r.Messages.Count);
        Assert.Same(late, r.Messages[2]);
        list.Clear();
        Assert.Empty(r.Messages);
    }

    [Fact]
    public void FailureWithMessages_EmptyList_IsFailureWithEmptyMessages_Test()
    {
        var r = MinimalResult<int>.Failure(new List<ResultMessage>());

        Assert.True(r.IsFailure);
        Assert.False(r.IsSuccess);
        Assert.NotNull(r.Messages);
        Assert.Empty(r.Messages);
    }

    [Fact]
    public void FailureWithMessages_NullList_IsFailureWithNullMessagesAndDefaultResponse_Test()
    {
        var r = MinimalResult<int>.Failure((IReadOnlyList<ResultMessage>)null!);

        Assert.True(r.IsFailure);
        Assert.False(r.IsSuccess);
        Assert.Null(r.Messages);
        Assert.Equal(0, r.Response);
    }

    [Fact]
    public void FailureWithMessages_NullList_StringResponse_IsNull_Test()
    {
        var r = MinimalResult<string>.Failure((IReadOnlyList<ResultMessage>)null!);

        Assert.True(r.IsFailure);
        Assert.Null(r.Messages);
        Assert.Null(r.Response);
    }

    [Fact]
    public void FailureWithMessages_NullElementAndNullExtensions_ArePassedThroughUnchanged_Test()
    {
        var noExtensions = new ResultMessage("E1", "Boom", null!);
        var items = new ResultMessage[] { null!, noExtensions };

        var r = MinimalResult<int>.Failure(items);

        Assert.True(r.IsFailure);
        Assert.Equal(2, r.Messages.Count);
        Assert.Null(r.Messages[0]);
        Assert.Same(noExtensions, r.Messages[1]);
        Assert.Null(r.Messages[1].Extensions);
    }

    [Theory]
    [InlineData(TestData.ShapeArray)]
    [InlineData(TestData.ShapeGenericList)]
    [InlineData(TestData.ShapeReadOnlyCollection)]
    public void FailureWithMessages_AnyReadOnlyListShape_PreservesCountAndElementReferencesInOrder_Test(string shape)
    {
        var items = TestData.TwoMessages();
        var input = TestData.Wrap(shape, items);

        var r = MinimalResult<int>.Failure(input);

        Assert.True(r.IsFailure);
        Assert.Equal(2, r.Messages.Count);
        Assert.Same(items[0], r.Messages[0]);
        Assert.Same(items[1], r.Messages[1]);
    }

    [Theory]
    [InlineData(TestData.ShapeArray)]
    [InlineData(TestData.ShapeGenericList)]
    [InlineData(TestData.ShapeReadOnlyCollection)]
    public void FailureWithMessages_AnyReadOnlyListShape_IsStoredWithoutCopy_Test(string shape)
    {
        var input = TestData.Wrap(shape, TestData.TwoMessages());

        var r = MinimalResult<int>.Failure(input);

        Assert.Same(input, r.Messages);
    }

    [Fact]
    public void Results_UseReferenceEquality_EqualOnlyToThemselves_Test()
    {
        var a = MinimalResult<int>.Success(1);
        var b = MinimalResult<int>.Success(1);

        Assert.NotSame(a, b);
        Assert.True(a.Equals((object)a), "reflexive");
        TestData.AssertNotEqualPair(a, b);
        Assert.False(a.Equals((object?)null));
    }

    [Fact]
    public void Success_SameNumericValueDifferentTypeArgument_IsNotEqual_Test()
    {
        var asInt = MinimalResult<int>.Success(1);
        var asLong = MinimalResult<long>.Success(1L);

        Assert.False(asInt.Equals((object)asLong));
        Assert.False(asLong.Equals((object)asInt));
    }

    [Fact]
    public void Success_DifferentValues_IsNotEqual_Test()
    {
        TestData.AssertNotEqualPair(MinimalResult<int>.Success(1), MinimalResult<int>.Success(2));
    }

    [Fact]
    public void Success_VersusFailure_IsNotEqual_Test()
    {
        TestData.AssertNotEqualPair(
            MinimalResult<string>.Success("x"),
            MinimalResult<string>.Failure(TestData.Code, TestData.Message));
    }

    [Fact]
    public void SuccessOfDefaultInt_VersusFailure_IsNotEqual_Test()
    {
        TestData.AssertNotEqualPair(
            MinimalResult<int>.Success(0),
            MinimalResult<int>.Failure(TestData.Code, TestData.Message));
    }

    [Fact]
    public void Success_NullStringTwice_AreDistinctNotEqualResults_Test()
    {
        var a = MinimalResult<string?>.Success(null);
        var b = MinimalResult<string?>.Success(null);

        TestData.AssertNotEqualPair(a, b);
    }

    [Fact]
    public void FailureWithMessages_SameListInstance_AreDistinctNotEqualResults_Test()
    {
        var list = new List<ResultMessage>(TestData.TwoMessages());

        var a = MinimalResult<int>.Failure(list);
        var b = MinimalResult<int>.Failure(list);

        Assert.Same(a.Messages, b.Messages);
        TestData.AssertNotEqualPair(a, b);
    }
}
