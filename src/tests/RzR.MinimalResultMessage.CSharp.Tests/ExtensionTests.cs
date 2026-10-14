using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

[Collection("SharedEmptyExtensions")]
public class ExtensionTests
{
    private static Dictionary<string, object> NewInput() =>
        new Dictionary<string, object> { ["userId"] = 42, ["tenant"] = "acme" };

    [Fact]
    public void FailureWithExtensions_Generic_HasOneMessageWithValues_Test()
    {
        var input = NewInput();

        var r = MinimalResult<int>.Failure("NotFound", "User not found", input);

        Assert.True(r.IsFailure);
        Assert.False(r.IsSuccess);
        Assert.Equal(0, r.Response);
        var message = Assert.Single(r.Messages);
        Assert.Equal("NotFound", message.Code);
        Assert.Equal("User not found", message.Message);
        Assert.NotNull(message.Extensions);
        var ext = message.Extensions!;
        Assert.Equal(2, ext.Count);
        Assert.Equal(42, ext["userId"]);
        Assert.Equal("acme", ext["tenant"]);
    }

    [Fact]
    public void FailureWithExtensions_NonGeneric_HasOneMessageWithValues_Test()
    {
        var input = NewInput();

        var r = MinimalResult.Failure("NotFound", "User not found", input);

        Assert.IsType<MinimalResult>(r);
        Assert.True(r.IsFailure);
        Assert.False(r.IsSuccess);
        Assert.Null(r.Response);
        var message = Assert.Single(r.Messages);
        Assert.Equal("NotFound", message.Code);
        Assert.Equal("User not found", message.Message);
        Assert.NotNull(message.Extensions);
        var ext = message.Extensions!;
        Assert.Equal(2, ext.Count);
        Assert.Equal(42, ext["userId"]);
        Assert.Equal("acme", ext["tenant"]);
    }

    [Fact]
    public void FailureWithExtensions_NullMessageText_IsKept_Test()
    {
        var generic = MinimalResult<int>.Failure("E1", null, NewInput());
        var nonGeneric = MinimalResult.Failure("E1", null, NewInput());

        Assert.Null(Assert.Single(generic.Messages).Message);
        Assert.Null(Assert.Single(nonGeneric.Messages).Message);
    }

    [Fact]
    public void FailureWithExtensions_Generic_StoresCopyNotCallerReference_Test()
    {
        var input = NewInput();

        var r = MinimalResult<int>.Failure("E1", "Boom", input);
        input["userId"] = 99;
        input["added"] = true;
        input.Remove("tenant");

        var ext = Assert.Single(r.Messages).Extensions!;
        Assert.NotSame(input, ext);
        Assert.Equal(2, ext.Count);
        Assert.Equal(42, ext["userId"]);
        Assert.Equal("acme", ext["tenant"]);
        Assert.False(ext.ContainsKey("added"));
    }

    [Fact]
    public void FailureWithExtensions_NonGeneric_StoresCopyNotCallerReference_Test()
    {
        var input = NewInput();

        var r = MinimalResult.Failure("E1", "Boom", input);
        input["userId"] = 99;
        input["added"] = true;
        input.Remove("tenant");

        var ext = Assert.Single(r.Messages).Extensions!;
        Assert.NotSame(input, ext);
        Assert.Equal(2, ext.Count);
        Assert.Equal(42, ext["userId"]);
        Assert.Equal("acme", ext["tenant"]);
        Assert.False(ext.ContainsKey("added"));
    }

    [Fact]
    public void FailureWithExtensions_Generic_CannotBeMutatedThroughIDictionary_Test()
    {
        var ext = Assert.Single(MinimalResult<int>.Failure("E1", "Boom", NewInput()).Messages).Extensions!;
        Assert.False(ext is Dictionary<string, object>, "Extensions must not be a concrete Dictionary<string, object>");

        var dict = (IDictionary<string, object>)ext;
        var nonGenericDict = (System.Collections.IDictionary)ext;

        Assert.Throws<NotSupportedException>(() => { dict.Add("x", 1); });
        Assert.Throws<NotSupportedException>(() => { dict["userId"] = 1; });
        Assert.Throws<NotSupportedException>(() => { dict.Remove("userId"); });
        Assert.Throws<NotSupportedException>(() => { dict.Clear(); });
        Assert.Throws<NotSupportedException>(() => { nonGenericDict.Add("x", 1); });
        Assert.Throws<NotSupportedException>(() => { nonGenericDict["userId"] = 1; });

        Assert.Equal(2, ext.Count);
        Assert.Equal(42, ext["userId"]);
        Assert.False(ext.ContainsKey("x"));
    }

    [Fact]
    public void FailureWithExtensions_NonGeneric_CannotBeMutatedThroughIDictionary_Test()
    {
        var ext = Assert.Single(MinimalResult.Failure("E1", "Boom", NewInput()).Messages).Extensions!;
        Assert.False(ext is Dictionary<string, object>, "Extensions must not be a concrete Dictionary<string, object>");

        var dict = (IDictionary<string, object>)ext;
        var nonGenericDict = (System.Collections.IDictionary)ext;

        Assert.Throws<NotSupportedException>(() => { dict.Add("x", 1); });
        Assert.Throws<NotSupportedException>(() => { dict["userId"] = 1; });
        Assert.Throws<NotSupportedException>(() => { dict.Remove("userId"); });
        Assert.Throws<NotSupportedException>(() => { dict.Clear(); });
        Assert.Throws<NotSupportedException>(() => { nonGenericDict.Add("x", 1); });
        Assert.Throws<NotSupportedException>(() => { nonGenericDict["userId"] = 1; });

        Assert.Equal(2, ext.Count);
        Assert.Equal(42, ext["userId"]);
        Assert.False(ext.ContainsKey("x"));
    }

    [Fact]
    public void FailureWithExtensions_Generic_NullDictionary_UsesSharedEmptyInstance_Test()
    {
        var shared = Assert.Single(MinimalResult<int>.Failure("E0", "Base").Messages).Extensions;

        var r = MinimalResult<int>.Failure("E1", "Boom", null!);

        Assert.True(r.IsFailure);
        Assert.Equal(0, r.Response);
        var message = Assert.Single(r.Messages);
        Assert.Equal("E1", message.Code);
        Assert.Equal("Boom", message.Message);
        Assert.NotNull(message.Extensions);
        Assert.Same(shared, message.Extensions);
        Assert.Empty(message.Extensions!);
    }

    [Fact]
    public void FailureWithExtensions_Generic_EmptyDictionary_UsesSharedEmptyInstance_Test()
    {
        var shared = Assert.Single(MinimalResult<int>.Failure("E0", "Base").Messages).Extensions;
        var input = new Dictionary<string, object>();

        var r = MinimalResult<int>.Failure("E1", "Boom", input);

        var message = Assert.Single(r.Messages);
        Assert.NotNull(message.Extensions);
        Assert.Same(shared, message.Extensions);
        Assert.NotSame(input, message.Extensions);
        Assert.Empty(message.Extensions!);
    }

    [Fact]
    public void FailureWithExtensions_NonGeneric_NullDictionary_UsesSharedEmptyInstance_Test()
    {
        var shared = Assert.Single(MinimalResult.Failure("E0", "Base").Messages).Extensions;

        var r = MinimalResult.Failure("E1", "Boom", null!);

        Assert.IsType<MinimalResult>(r);
        Assert.True(r.IsFailure);
        Assert.Null(r.Response);
        var message = Assert.Single(r.Messages);
        Assert.Equal("E1", message.Code);
        Assert.Equal("Boom", message.Message);
        Assert.NotNull(message.Extensions);
        Assert.Same(shared, message.Extensions);
        Assert.Empty(message.Extensions!);
    }

    [Fact]
    public void FailureWithExtensions_NonGeneric_EmptyDictionary_UsesSharedEmptyInstance_Test()
    {
        var shared = Assert.Single(MinimalResult.Failure("E0", "Base").Messages).Extensions;
        var input = new Dictionary<string, object>();

        var r = MinimalResult.Failure("E1", "Boom", input);

        var message = Assert.Single(r.Messages);
        Assert.NotNull(message.Extensions);
        Assert.Same(shared, message.Extensions);
        Assert.NotSame(input, message.Extensions);
        Assert.Empty(message.Extensions!);
    }

    [Fact]
    public void WithExtension_Generic_ReturnsNewResultAndLeavesOriginalUnchanged_Test()
    {
        var original = MinimalResult<int>.Failure("NotFound", "User not found");
        var originalMessage = Assert.Single(original.Messages);
        var originalExtensions = originalMessage.Extensions;

        var updated = original.WithExtension("userId", 42);

        Assert.NotSame(original, updated);
        Assert.Same(originalMessage, Assert.Single(original.Messages));
        Assert.Same(originalExtensions, originalMessage.Extensions);
        Assert.Empty(originalMessage.Extensions!);
        Assert.Equal("NotFound", originalMessage.Code);
        Assert.Equal("User not found", originalMessage.Message);
    }

    [Fact]
    public void WithExtension_NonGeneric_ReturnsNewResultAndLeavesOriginalUnchanged_Test()
    {
        var original = MinimalResult.Failure("NotFound", "User not found");
        var originalMessage = Assert.Single(original.Messages);
        var originalExtensions = originalMessage.Extensions;

        var updated = original.WithExtension("userId", 42);

        Assert.NotSame(original, updated);
        Assert.Same(originalMessage, Assert.Single(original.Messages));
        Assert.Same(originalExtensions, originalMessage.Extensions);
        Assert.Empty(originalMessage.Extensions!);
        Assert.Equal("NotFound", originalMessage.Code);
        Assert.Equal("User not found", originalMessage.Message);
    }

    [Fact]
    public void WithExtension_Generic_KeepsTypeFlagsAndResponseAndAddsValue_Test()
    {
        var original = MinimalResult<int>.Failure("NotFound", "User not found");

        MinimalResult<int> updated = original.WithExtension("userId", 42);

        Assert.IsType<MinimalResult<int>>(updated);
        Assert.Equal(original.IsSuccess, updated.IsSuccess);
        Assert.Equal(original.IsFailure, updated.IsFailure);
        Assert.True(updated.IsFailure);
        Assert.Equal(original.Response, updated.Response);
        Assert.Equal(0, updated.Response);
        var message = Assert.Single(updated.Messages);
        Assert.Equal("NotFound", message.Code);
        Assert.Equal("User not found", message.Message);
        Assert.NotNull(message.Extensions);
        var entry = Assert.Single(message.Extensions!);
        Assert.Equal("userId", entry.Key);
        Assert.Equal(42, entry.Value);
    }

    [Fact]
    public void WithExtension_NonGeneric_KeepsTypeFlagsAndResponseAndAddsValue_Test()
    {
        var original = MinimalResult.Failure("NotFound", "User not found");

        MinimalResult updated = original.WithExtension("userId", 42);

        Assert.IsType<MinimalResult>(updated);
        Assert.Equal(original.IsSuccess, updated.IsSuccess);
        Assert.Equal(original.IsFailure, updated.IsFailure);
        Assert.True(updated.IsFailure);
        Assert.Null(updated.Response);
        var message = Assert.Single(updated.Messages);
        Assert.Equal("NotFound", message.Code);
        Assert.Equal("User not found", message.Message);
        Assert.NotNull(message.Extensions);
        var entry = Assert.Single(message.Extensions!);
        Assert.Equal("userId", entry.Key);
        Assert.Equal(42, entry.Value);
    }

    [Fact]
    public void WithExtension_NonGeneric_PicksNonGenericOverloadWithoutCast_Test()
    {
        MinimalResult r2 = MinimalResult.Failure("E", "M").WithExtension("k", 1);

        Assert.IsType<MinimalResult>(r2);
        Assert.Equal(1, Assert.Single(r2.Messages).Extensions!["k"]);
    }

    [Fact]
    public void WithExtension_Generic_ChangesOnlyTheLastMessage_Test()
    {
        var messages = TestData.TwoMessages();
        var original = MinimalResult<int>.Failure(new List<ResultMessage>(messages));

        var updated = original.WithExtension("userId", 42);

        Assert.Equal(2, updated.Messages.Count);
        Assert.Same(messages[0], updated.Messages[0]);
        Assert.NotSame(messages[1], updated.Messages[1]);
        Assert.Same(messages[0], original.Messages[0]);
        Assert.Same(messages[1], original.Messages[1]);

        var last = updated.Messages[1];
        Assert.Equal("E2", last.Code);
        Assert.Equal("Bang", last.Message);
        Assert.NotNull(last.Extensions);
        Assert.Equal(2, last.Extensions!.Count);
        Assert.Equal(1, last.Extensions["k"]);
        Assert.Equal(42, last.Extensions["userId"]);

        var originalLastEntry = Assert.Single(messages[1].Extensions!);
        Assert.Equal("k", originalLastEntry.Key);
        var firstEntry = Assert.Single(updated.Messages[0].Extensions!);
        Assert.Equal("k", firstEntry.Key);
    }

    [Fact]
    public void WithExtension_NonGeneric_ChangesOnlyTheLastMessage_Test()
    {
        var messages = TestData.TwoMessages();
        var original = MinimalResult.Failure(new List<ResultMessage>(messages));

        var updated = original.WithExtension("userId", 42);

        Assert.IsType<MinimalResult>(updated);
        Assert.Equal(2, updated.Messages.Count);
        Assert.Same(messages[0], updated.Messages[0]);
        Assert.NotSame(messages[1], updated.Messages[1]);
        Assert.Same(messages[0], original.Messages[0]);
        Assert.Same(messages[1], original.Messages[1]);

        var last = updated.Messages[1];
        Assert.Equal("E2", last.Code);
        Assert.Equal("Bang", last.Message);
        Assert.NotNull(last.Extensions);
        Assert.Equal(2, last.Extensions!.Count);
        Assert.Equal(1, last.Extensions["k"]);
        Assert.Equal(42, last.Extensions["userId"]);

        var originalLastEntry = Assert.Single(messages[1].Extensions!);
        Assert.Equal("k", originalLastEntry.Key);
    }

    [Fact]
    public void WithExtension_LastMessageWithNullExtensions_IsTreatedAsEmpty_Test()
    {
        var first = new ResultMessage("E1", "Boom");
        var last = new ResultMessage("E2", "Bang");
        var generic = MinimalResult<int>.Failure(new List<ResultMessage> { first, last });
        var nonGeneric = MinimalResult.Failure(new List<ResultMessage> { first, last });

        var updatedGeneric = generic.WithExtension("userId", 42);
        var updatedNonGeneric = nonGeneric.WithExtension("userId", 42);

        Assert.Same(first, updatedGeneric.Messages[0]);
        Assert.Same(first, updatedNonGeneric.Messages[0]);
        Assert.Null(last.Extensions);
        var genericEntry = Assert.Single(updatedGeneric.Messages[1].Extensions!);
        var nonGenericEntry = Assert.Single(updatedNonGeneric.Messages[1].Extensions!);
        Assert.Equal("userId", genericEntry.Key);
        Assert.Equal(42, genericEntry.Value);
        Assert.Equal("userId", nonGenericEntry.Key);
        Assert.Equal(42, nonGenericEntry.Value);
    }

    [Fact]
    public void WithExtension_Generic_OverwritesExistingKey_Test()
    {
        var original = MinimalResult<int>.Failure("E1", "Boom", new Dictionary<string, object> { ["k"] = 1 });

        var updated = original.WithExtension("k", 2);

        var entry = Assert.Single(Assert.Single(updated.Messages).Extensions!);
        Assert.Equal("k", entry.Key);
        Assert.Equal(2, entry.Value);
        Assert.Equal(1, Assert.Single(original.Messages).Extensions!["k"]);
    }

    [Fact]
    public void WithExtension_NonGeneric_OverwritesExistingKey_Test()
    {
        var original = MinimalResult.Failure("E1", "Boom", new Dictionary<string, object> { ["k"] = 1 });

        var updated = original.WithExtension("k", 2);

        var entry = Assert.Single(Assert.Single(updated.Messages).Extensions!);
        Assert.Equal("k", entry.Key);
        Assert.Equal(2, entry.Value);
        Assert.Equal(1, Assert.Single(original.Messages).Extensions!["k"]);
    }

    [Fact]
    public void WithExtension_Generic_ChainingKeepsAllValues_Test()
    {
        var first = MinimalResult<int>.Failure("E1", "Boom").WithExtension("a", 1);

        var second = first.WithExtension("b", 2);

        var ext = Assert.Single(second.Messages).Extensions!;
        Assert.Equal(2, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        var firstEntry = Assert.Single(Assert.Single(first.Messages).Extensions!);
        Assert.Equal("a", firstEntry.Key);
    }

    [Fact]
    public void WithExtension_NonGeneric_ChainingKeepsAllValues_Test()
    {
        MinimalResult chained = MinimalResult.Failure("E1", "Boom").WithExtension("a", 1).WithExtension("b", 2);

        var ext = Assert.Single(chained.Messages).Extensions!;
        Assert.Equal(2, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
    }

    [Fact]
    public void WithExtension_ResultExtensions_AreReadOnly_Test()
    {
        var generic = MinimalResult<int>.Failure("E1", "Boom").WithExtension("k", 1);
        var nonGeneric = MinimalResult.Failure("E1", "Boom").WithExtension("k", 1);

        var genericExt = Assert.Single(generic.Messages).Extensions!;
        var nonGenericExt = Assert.Single(nonGeneric.Messages).Extensions!;
        Assert.False(genericExt is Dictionary<string, object>, "Extensions must not be a concrete Dictionary<string, object>");
        Assert.False(nonGenericExt is Dictionary<string, object>, "Extensions must not be a concrete Dictionary<string, object>");

        var genericDict = (IDictionary<string, object>)genericExt;
        var nonGenericDict = (IDictionary<string, object>)nonGenericExt;
        Assert.Throws<NotSupportedException>(() => { genericDict["k"] = 2; });
        Assert.Throws<NotSupportedException>(() => { genericDict.Add("x", 1); });
        Assert.Throws<NotSupportedException>(() => { nonGenericDict["k"] = 2; });
        Assert.Throws<NotSupportedException>(() => { nonGenericDict.Add("x", 1); });

        Assert.Equal(1, genericExt["k"]);
        Assert.Equal(1, nonGenericExt["k"]);
    }

    [Fact]
    public void WithExtension_NeverModifiesSharedEmptyDictionary_Test()
    {
        var shared = Assert.Single(MinimalResult<int>.Failure("x", "y").Messages).Extensions;
        Assert.NotNull(shared);

        var generic = MinimalResult<int>.Failure("E1", "Boom").WithExtension("k", 1);
        var nonGeneric = MinimalResult.Failure("E1", "Boom").WithExtension("k", 1);

        Assert.Empty(shared!);
        Assert.NotSame(shared, Assert.Single(generic.Messages).Extensions);
        Assert.NotSame(shared, Assert.Single(nonGeneric.Messages).Extensions);

        var freshGeneric = Assert.Single(MinimalResult<int>.Failure("x", "y").Messages).Extensions;
        var freshNonGeneric = Assert.Single(MinimalResult.Failure("x", "y").Messages).Extensions;
        Assert.Same(shared, freshGeneric);
        Assert.Same(shared, freshNonGeneric);
        Assert.Empty(freshGeneric!);
        Assert.Empty(freshNonGeneric!);
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void WithExtension_Generic_NoMessages_CreatesOneEmptyCodeMessage_Test(string scenario)
    {
        var r = TestData.BuildInt(scenario);
        var originalMessages = r.Messages;

        var updated = r.WithExtension("k", 1);

        Assert.NotSame(r, updated);
        Assert.Same(originalMessages, r.Messages);
        Assert.IsType<MinimalResult<int>>(updated);
        Assert.Equal(TestData.ExpectSuccess(scenario), updated.IsSuccess);
        Assert.Equal(r.IsFailure, updated.IsFailure);
        Assert.Equal(r.Response, updated.Response);
        AssertSingleEmptyCodeMessageWithK(updated.Messages);
    }

    [Fact]
    public void WithExtension_NonGeneric_Success_CreatesOneEmptyCodeMessage_Test()
    {
        var r = MinimalResult.Success();

        var updated = r.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(updated);
        Assert.True(updated.IsSuccess);
        Assert.Null(updated.Response);
        Assert.Empty(r.Messages);
        AssertSingleEmptyCodeMessageWithK(updated.Messages);
    }

    [Fact]
    public void WithExtension_NonGeneric_EmptyMessageList_CreatesOneEmptyCodeMessage_Test()
    {
        var list = new List<ResultMessage>();
        var r = MinimalResult.Failure(list);

        var updated = r.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(updated);
        Assert.True(updated.IsFailure);
        Assert.Empty(list);
        Assert.Empty(r.Messages);
        AssertSingleEmptyCodeMessageWithK(updated.Messages);
    }

    [Fact]
    public void WithExtension_NonGeneric_NullMessageList_CreatesOneEmptyCodeMessage_Test()
    {
        var r = MinimalResult.Failure((IReadOnlyList<ResultMessage>)null!);

        var updated = r.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(updated);
        Assert.True(updated.IsFailure);
        Assert.Null(r.Messages);
        AssertSingleEmptyCodeMessageWithK(updated.Messages);
    }

    private static void AssertSingleEmptyCodeMessageWithK(IReadOnlyList<ResultMessage> messages)
    {
        Assert.NotNull(messages);
        var message = Assert.Single(messages);
        Assert.NotNull(message);
        Assert.Equal("", message.Code);
        Assert.Null(message.Message);
        Assert.NotNull(message.Extensions);
        var entry = Assert.Single(message.Extensions!);
        Assert.Equal("k", entry.Key);
        Assert.Equal(1, entry.Value);
    }

    [Fact]
    public void WithExtension_NullKey_ThrowsArgumentNullException_Test()
    {
        var generic = MinimalResult<int>.Failure("E1", "Boom");
        var nonGeneric = MinimalResult.Failure("E1", "Boom");

        var genericEx = Assert.Throws<ArgumentNullException>(() => generic.WithExtension(null!, 1));
        var nonGenericEx = Assert.Throws<ArgumentNullException>(() => nonGeneric.WithExtension(null!, 1));

        Assert.Equal("key", genericEx.ParamName);
        Assert.Equal("key", nonGenericEx.ParamName);
    }

    [Fact]
    public void WithExtension_Generic_NullResult_ThrowsArgumentNullException_Test()
    {
        MinimalResult<int>? r = null;

        var viaExtensionSyntax = Assert.Throws<ArgumentNullException>(() => r!.WithExtension("k", 1));
        var viaStaticCall = Assert.Throws<ArgumentNullException>(
            () => MinimalResultExtensions.WithExtension((MinimalResult<int>)null!, "k", 1));

        Assert.Equal("result", viaExtensionSyntax.ParamName);
        Assert.Equal("result", viaStaticCall.ParamName);
    }

    [Fact]
    public void WithExtension_NonGeneric_NullResult_ThrowsArgumentNullException_Test()
    {
        MinimalResult? r = null;

        var viaExtensionSyntax = Assert.Throws<ArgumentNullException>(() => r!.WithExtension("k", 1));
        var viaStaticCall = Assert.Throws<ArgumentNullException>(
            () => MinimalResultExtensions.WithExtension((MinimalResult)null!, "k", 1));

        Assert.Equal("result", viaExtensionSyntax.ParamName);
        Assert.Equal("result", viaStaticCall.ParamName);
    }

    [Fact]
    public void WithExtension_Generic_NullLastMessage_ThrowsInvalidOperationException_Test()
    {
        var r = MinimalResult<int>.Failure(new ResultMessage[] { null! });

        Assert.Throws<InvalidOperationException>(() => r.WithExtension("k", 1));
    }

    [Fact]
    public void WithExtension_NonGeneric_NullLastMessage_ThrowsInvalidOperationException_Test()
    {
        var r = MinimalResult.Failure(new ResultMessage[] { null! });

        Assert.Throws<InvalidOperationException>(() => r.WithExtension("k", 1));
    }

    [Fact]
    public void WithExtension_BaseStaticType_KeepsConcreteNonGenericType_Test()
    {
        MinimalResult<object?> r = MinimalResult.Failure("a", "b");

        var x = r.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(x);
        Assert.True(x.IsFailure);
        var message = Assert.Single(x.Messages);
        Assert.Equal("a", message.Code);
        Assert.Equal("b", message.Message);
        var entry = Assert.Single(message.Extensions!);
        Assert.Equal("k", entry.Key);
        Assert.Equal(1, entry.Value);
    }

    [Fact]
    public void WithExtension_Messages_IsNotICollection_Test()
    {
        var results = new IReadOnlyList<ResultMessage>[]
        {
            MinimalResult<int>.Failure("E", "M").WithExtension("k", 1).Messages,
            MinimalResult<int>.Success(5).WithExtension("k", 1).Messages,
            MinimalResult<int>.Failure(new List<ResultMessage>(TestData.TwoMessages())).WithExtension("k", 1).Messages,
            MinimalResult.Failure("E", "M").WithExtension("k", 1).Messages,
            MinimalResult.Success().WithExtension("k", 1).Messages,
            MinimalResult.Failure(new List<ResultMessage>(TestData.TwoMessages())).WithExtension("k", 1).Messages,
        };

        foreach (var messages in results)
        {
            Assert.False(messages is ICollection<ResultMessage>, "Messages returned by WithExtension must not be an ICollection<ResultMessage>");
        }
    }

    [Fact]
    public void WithExtension_OnFailureFromCallerArray_LeavesArrayAndOriginalUnchanged_Test()
    {
        var genericArr = TestData.TwoMessages();
        var nonGenericArr = TestData.TwoMessages();
        var genericLast = genericArr[1];
        var nonGenericLast = nonGenericArr[1];
        var generic = MinimalResult<int>.Failure(genericArr);
        var nonGeneric = MinimalResult.Failure(nonGenericArr);

        var updatedGeneric = generic.WithExtension("added", 2);
        var updatedNonGeneric = nonGeneric.WithExtension("added", 2);

        Assert.Same(genericLast, genericArr[1]);
        Assert.Same(nonGenericLast, nonGenericArr[1]);
        Assert.Same(genericArr, generic.Messages);
        Assert.Same(nonGenericArr, nonGeneric.Messages);
        Assert.NotSame(genericLast, updatedGeneric.Messages[1]);
        Assert.NotSame(nonGenericLast, updatedNonGeneric.Messages[1]);
        Assert.False(genericLast.Extensions!.ContainsKey("added"));
        Assert.False(nonGenericLast.Extensions!.ContainsKey("added"));
        Assert.Equal(2, updatedGeneric.Messages[1].Extensions!["added"]);
        Assert.Equal(2, updatedNonGeneric.Messages[1].Extensions!["added"]);
    }

    [Fact]
    public void WithExtension_CallerListMutatedAfterwards_DoesNotAffectUpdated_Test()
    {
        var genericList = new List<ResultMessage>(TestData.TwoMessages());
        var nonGenericList = new List<ResultMessage>(TestData.TwoMessages());
        var generic = MinimalResult<int>.Failure(genericList);
        var nonGeneric = MinimalResult.Failure(nonGenericList);
        var updatedGeneric = generic.WithExtension("k2", 2);
        var updatedNonGeneric = nonGeneric.WithExtension("k2", 2);

        genericList.Add(new ResultMessage("E3"));
        nonGenericList.Add(new ResultMessage("E3"));

        Assert.Equal(2, updatedGeneric.Messages.Count);
        Assert.Equal(2, updatedNonGeneric.Messages.Count);
        Assert.Equal(3, generic.Messages.Count);
        Assert.Equal(3, nonGeneric.Messages.Count);

        genericList.Clear();
        nonGenericList.Clear();

        Assert.Equal(2, updatedGeneric.Messages.Count);
        Assert.Equal(2, updatedNonGeneric.Messages.Count);
        Assert.Empty(generic.Messages);
        Assert.Empty(nonGeneric.Messages);
        Assert.Equal("E2", updatedGeneric.Messages[1].Code);
        Assert.Equal("E2", updatedNonGeneric.Messages[1].Code);
    }

    [Fact]
    public void WithExtension_NullValue_IsStoredUnderKey_Test()
    {
        var generic = MinimalResult<int>.Failure("E", "M").WithExtension("k", null!);
        var nonGeneric = MinimalResult.Success().WithExtension("k", null!);

        foreach (var messages in new[] { generic.Messages, nonGeneric.Messages })
        {
            var ext = Assert.Single(messages).Extensions!;
            Assert.Single(ext);
            Assert.True(ext.ContainsKey("k"));
            Assert.Null(ext["k"]);
        }
    }

    [Fact]
    public void WithExtension_LastMessageWithNullCode_KeepsNullCodeAndMessage_Test()
    {
        var genericNullCode = MinimalResult<int>.Failure(null!, null).WithExtension("k", 1);
        var nonGenericNullCode = MinimalResult.Failure(null!, null).WithExtension("k", 1);
        var genericCodeOnly = MinimalResult<int>.Failure(new[] { new ResultMessage("x") }).WithExtension("k", 1);
        var nonGenericCodeOnly = MinimalResult.Failure(new[] { new ResultMessage("x") }).WithExtension("k", 1);

        foreach (var messages in new[] { genericNullCode.Messages, nonGenericNullCode.Messages })
        {
            var message = Assert.Single(messages);
            Assert.Null(message.Code);
            Assert.Null(message.Message);
            Assert.True(message.Extensions!.ContainsKey("k"));
        }

        foreach (var messages in new[] { genericCodeOnly.Messages, nonGenericCodeOnly.Messages })
        {
            var message = Assert.Single(messages);
            Assert.Equal("x", message.Code);
            Assert.Null(message.Message);
            Assert.True(message.Extensions!.ContainsKey("k"));
        }
    }

    [Fact]
    public void WithExtension_OnSuccess_PreservesReferenceAndStructResponse_Test()
    {
        var text = new string('a', 3);
        var guid = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");

        var stringResult = MinimalResult<string>.Success(text).WithExtension("k", 1);
        var guidResult = MinimalResult<Guid>.Success(guid).WithExtension("k", 1);
        var nullStringResult = MinimalResult<string?>.Success(null).WithExtension("k", 1);

        Assert.Same(text, stringResult.Response);
        Assert.Equal(guid, guidResult.Response);
        Assert.Null(nullStringResult.Response);
        Assert.True(stringResult.IsSuccess);
        Assert.True(guidResult.IsSuccess);
        Assert.True(nullStringResult.IsSuccess);
    }

    [Fact]
    public void WithExtension_OnFailure_KeepsDefaultResponseForGuidAndString_Test()
    {
        var guidResult = MinimalResult<Guid>.Failure("E", "M").WithExtension("k", 1);
        var stringResult = MinimalResult<string?>.Failure("E", "M").WithExtension("k", 1);

        Assert.Equal(Guid.Empty, guidResult.Response);
        Assert.Null(stringResult.Response);
        Assert.True(guidResult.IsFailure);
        Assert.True(stringResult.IsFailure);
    }

    [Fact]
    public void FailureWithExtensions_SnapshotKeys_AreCaseInsensitive_Test()
    {
        var input = new Dictionary<string, object> { ["Key"] = 1 };

        var generic = MinimalResult<int>.Failure("E", "M", input);
        var nonGeneric = MinimalResult.Failure("E", "M", input);

        Assert.False(input.ContainsKey("key"));
        foreach (var messages in new[] { generic.Messages, nonGeneric.Messages })
        {
            var ext = Assert.Single(messages).Extensions!;
            Assert.True(ext.ContainsKey("Key"));
            Assert.True(ext.ContainsKey("key"));
            Assert.True(ext.ContainsKey("KEY"));
            Assert.Equal(1, ext["kEy"]);
        }
    }

    [Fact]
    public void FailureWithExtensions_KeysDifferingOnlyByCase_AreMergedLastWins_Test()
    {
        var input = new Dictionary<string, object> { ["a"] = 1, ["A"] = 2 };

        var generic = MinimalResult<int>.Failure("E", "M", input);
        var nonGeneric = MinimalResult.Failure("E", "M", input);

        Assert.Equal(2, input.Count);
        foreach (var messages in new[] { generic.Messages, nonGeneric.Messages })
        {
            var ext = Assert.Single(messages).Extensions!;
            Assert.Single(ext);
            Assert.Equal(2, ext["a"]);
        }
    }

    [Fact]
    public void FailureWithExtensions_ReferenceAndNullValues_AreKeptAsIs_Test()
    {
        var payload = new Payload("p");
        var input = new Dictionary<string, object> { ["p"] = payload, ["n"] = null! };

        var generic = MinimalResult<int>.Failure("E", "M", input);
        var nonGeneric = MinimalResult.Failure("E", "M", input);

        foreach (var messages in new[] { generic.Messages, nonGeneric.Messages })
        {
            var ext = Assert.Single(messages).Extensions!;
            Assert.Equal(2, ext.Count);
            Assert.Same(payload, ext["p"]);
            Assert.True(ext.ContainsKey("n"));
            Assert.Null(ext["n"]);
        }
    }

    [Fact]
    public void ExceptionConversion_UsesOuterMessageAndSharedEmptyExtensions_Test()
    {
        var shared = Assert.Single(MinimalResult.Failure("x", "y").Messages).Extensions;
        Assert.NotNull(shared);

        MinimalResult nonGeneric = new Exception("outer", new Exception("inner"));
        MinimalResult<int> generic = new Exception("outer", new Exception("inner"));

        foreach (var messages in new[] { nonGeneric.Messages, generic.Messages })
        {
            var message = Assert.Single(messages);
            Assert.Equal("UnhandledException", message.Code);
            Assert.Equal("outer", message.Message);
            Assert.Same(shared, message.Extensions);
        }
    }
}
