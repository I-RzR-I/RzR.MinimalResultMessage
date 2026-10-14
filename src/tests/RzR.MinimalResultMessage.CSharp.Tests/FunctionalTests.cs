using RzR.MinimalResultMessage.Abstractions;
using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

[Collection("SharedEmptyExtensions")]
public class FunctionalTests
{
    private static ResultMessage OnlyMessage(IReadOnlyList<ResultMessage> messages)
    {
        Assert.NotNull(messages);
        var message = Assert.Single(messages);
        Assert.NotNull(message);
        return message;
    }

    private static IReadOnlyDictionary<string, object> ExtensionsOf(ResultMessage message)
    {
        Assert.NotNull(message.Extensions);
        return message.Extensions!;
    }

    [Fact]
    public void NonGeneric_SuccessChainOfThree_HasOneEmptyCodeMessageWithThreeExtensions_Test()
    {
        var r = MinimalResult.Success().WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

        Assert.IsType<MinimalResult>(r);
        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Null(r.Response);
        var message = OnlyMessage(r.Messages);
        Assert.Equal("", message.Code);
        Assert.Null(message.Message);
        var ext = ExtensionsOf(message);
        Assert.Equal(3, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(3, ext["c"]);
    }

    [Fact]
    public void Generic_SuccessChainOfThree_KeepsResponseAndHasThreeExtensions_Test()
    {
        var r = MinimalResult<int>.Success(5).WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

        Assert.IsType<MinimalResult<int>>(r);
        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Equal(5, r.Response);
        var message = OnlyMessage(r.Messages);
        Assert.Equal("", message.Code);
        Assert.Null(message.Message);
        var ext = ExtensionsOf(message);
        Assert.Equal(3, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(3, ext["c"]);
    }

    [Fact]
    public void NonGeneric_FailureChainOfThree_KeepsCodeAndMessage_Test()
    {
        var r = MinimalResult.Failure("E", "M").WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

        Assert.IsType<MinimalResult>(r);
        Assert.True(r.IsFailure);
        var message = OnlyMessage(r.Messages);
        Assert.Equal("E", message.Code);
        Assert.Equal("M", message.Message);
        var ext = ExtensionsOf(message);
        Assert.Equal(3, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(3, ext["c"]);
    }

    [Fact]
    public void Generic_FailureChainOfThree_KeepsCodeAndMessage_Test()
    {
        var r = MinimalResult<int>.Failure("E", "M").WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

        Assert.True(r.IsFailure);
        Assert.Equal(0, r.Response);
        var message = OnlyMessage(r.Messages);
        Assert.Equal("E", message.Code);
        Assert.Equal("M", message.Message);
        var ext = ExtensionsOf(message);
        Assert.Equal(3, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(3, ext["c"]);
    }

    [Fact]
    public void NonGeneric_FailureWithExtensionsPlusThree_CountsFour_Test()
    {
        var initial = new Dictionary<string, object> { ["x"] = 0 };

        var r = MinimalResult.Failure("E", "M", initial).WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

        var ext = ExtensionsOf(OnlyMessage(r.Messages));
        Assert.Equal(4, ext.Count);
        Assert.Equal(0, ext["x"]);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(3, ext["c"]);
        Assert.Single(initial);
    }

    [Fact]
    public void Generic_FailureWithExtensionsPlusThree_CountsFour_Test()
    {
        var initial = new Dictionary<string, object> { ["x"] = 0 };

        var r = MinimalResult<int>.Failure("E", "M", initial).WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

        var ext = ExtensionsOf(OnlyMessage(r.Messages));
        Assert.Equal(4, ext.Count);
        Assert.Equal(0, ext["x"]);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(3, ext["c"]);
        Assert.Single(initial);
    }

    [Fact]
    public void NonGeneric_SameKeyTwice_CountStaysAndLatestValueWins_Test()
    {
        var once = MinimalResult.Success().WithExtension("a", 1).WithExtension("b", 2);

        var twice = once.WithExtension("a", 99);

        var ext = ExtensionsOf(OnlyMessage(twice.Messages));
        Assert.Equal(2, ext.Count);
        Assert.Equal(99, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Equal(1, ExtensionsOf(OnlyMessage(once.Messages))["a"]);
    }

    [Fact]
    public void Generic_SameKeyTwice_CountStaysAndLatestValueWins_Test()
    {
        var initial = new Dictionary<string, object> { ["a"] = 1, ["b"] = 2 };

        var r = MinimalResult<int>.Failure("E", "M", initial).WithExtension("a", "first").WithExtension("a", "second");

        var ext = ExtensionsOf(OnlyMessage(r.Messages));
        Assert.Equal(2, ext.Count);
        Assert.Equal("second", ext["a"]);
        Assert.Equal(2, ext["b"]);
    }

    [Fact]
    public void KeysAreCaseInsensitive_KeyAndkeyAreTheSameEntry_Test()
    {
        var nonGeneric = MinimalResult.Success().WithExtension("Key", 1).WithExtension("key", 2);
        var generic = MinimalResult<int>.Failure("E", "M").WithExtension("Key", 1).WithExtension("key", 2);

        var nonGenericExt = ExtensionsOf(OnlyMessage(nonGeneric.Messages));
        var genericExt = ExtensionsOf(OnlyMessage(generic.Messages));
        Assert.Single(nonGenericExt);
        Assert.Equal(2, nonGenericExt["Key"]);
        Assert.Equal(2, nonGenericExt["KEY"]);
        Assert.Single(genericExt);
        Assert.Equal(2, genericExt["Key"]);
        Assert.Equal(2, genericExt["key"]);
    }

    [Theory]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void Generic_FailureWithEmptyOrNullList_ThenWithExtension_CreatesEmptyCodeMessageAndStaysFailure_Test(string scenario)
    {
        var r = TestData.BuildInt(scenario).WithExtension("a", 1).WithExtension("b", 2);

        Assert.True(r.IsFailure);
        Assert.False(r.IsSuccess);
        var message = OnlyMessage(r.Messages);
        Assert.Equal("", message.Code);
        Assert.Null(message.Message);
        var ext = ExtensionsOf(message);
        Assert.Equal(2, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
    }

    [Fact]
    public void FalseConversion_ThenWithExtension_AddsToUnSuccessfullyReqExecMessage_Test()
    {
        MinimalResult r = false;

        var updated = r.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(updated);
        Assert.True(updated.IsFailure);
        var message = OnlyMessage(updated.Messages);
        Assert.Equal("UnSuccessfullyReqExec", message.Code);
        Assert.Null(message.Message);
        var entry = Assert.Single(ExtensionsOf(message));
        Assert.Equal("k", entry.Key);
        Assert.Equal(1, entry.Value);
        Assert.Empty(ExtensionsOf(OnlyMessage(r.Messages)));
    }

    [Fact]
    public void TrueConversion_ThenWithExtension_CreatesNewMessage_Test()
    {
        MinimalResult t = true;

        var updated = t.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(updated);
        Assert.True(updated.IsSuccess);
        Assert.Empty(t.Messages);
        var message = OnlyMessage(updated.Messages);
        Assert.Equal("", message.Code);
        Assert.Null(message.Message);
        Assert.Equal(1, ExtensionsOf(message)["k"]);
    }

    [Fact]
    public void ExceptionConversion_ThenWithExtension_AddsToUnhandledExceptionMessage_Test()
    {
        MinimalResult e = new InvalidOperationException("x");
        MinimalResult<int> ge = new InvalidOperationException("x");

        var updated = e.WithExtension("k", 1);
        var updatedGeneric = ge.WithExtension("k", 1);

        Assert.IsType<MinimalResult>(updated);
        Assert.True(updated.IsFailure);
        var message = OnlyMessage(updated.Messages);
        Assert.Equal("UnhandledException", message.Code);
        Assert.Equal("x", message.Message);
        Assert.Equal(1, Assert.Single(ExtensionsOf(message)).Value);

        Assert.True(updatedGeneric.IsFailure);
        var genericMessage = OnlyMessage(updatedGeneric.Messages);
        Assert.Equal("UnhandledException", genericMessage.Code);
        Assert.Equal("x", genericMessage.Message);
        Assert.Equal(1, Assert.Single(ExtensionsOf(genericMessage)).Value);
    }

    [Fact]
    public void ValueConversion_ThenWithExtension_KeepsResponseAndCreatesNewMessage_Test()
    {
        MinimalResult<int> v = 5;

        var updated = v.WithExtension("k", 1);

        Assert.True(updated.IsSuccess);
        Assert.Equal(5, updated.Response);
        Assert.Empty(v.Messages);
        var message = OnlyMessage(updated.Messages);
        Assert.Equal("", message.Code);
        Assert.Null(message.Message);
        Assert.Equal(1, ExtensionsOf(message)["k"]);
    }

    [Fact]
    public void NonGeneric_ThreeMessagesThenTwoCalls_ChangesOnlyTheLastMessage_Test()
    {
        var messages = new[]
        {
            TestData.NewMessage("E1", "m1"),
            TestData.NewMessage("E2", "m2"),
            TestData.NewMessage("E3", "m3"),
        };
        var original = MinimalResult.Failure(new List<ResultMessage>(messages));

        var updated = original.WithExtension("a", 1).WithExtension("b", 2);

        Assert.IsType<MinimalResult>(updated);
        Assert.Equal(3, updated.Messages.Count);
        Assert.Same(messages[0], updated.Messages[0]);
        Assert.Same(messages[1], updated.Messages[1]);
        Assert.NotSame(messages[2], updated.Messages[2]);
        Assert.Equal("E3", updated.Messages[2].Code);
        Assert.Equal("m3", updated.Messages[2].Message);
        var ext = ExtensionsOf(updated.Messages[2]);
        Assert.Equal(3, ext.Count);
        Assert.Equal(1, ext["k"]);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Single(ExtensionsOf(messages[2]));
        Assert.Same(messages[2], original.Messages[2]);
    }

    [Fact]
    public void Generic_ThreeMessagesThenTwoCalls_ChangesOnlyTheLastMessage_Test()
    {
        var messages = new[]
        {
            new ResultMessage("E1", "m1"),
            new ResultMessage("E2", "m2"),
            new ResultMessage("E3", "m3"),
        };
        var original = MinimalResult<int>.Failure(new List<ResultMessage>(messages));

        var updated = original.WithExtension("a", 1).WithExtension("b", 2);

        Assert.Equal(3, updated.Messages.Count);
        Assert.Same(messages[0], updated.Messages[0]);
        Assert.Same(messages[1], updated.Messages[1]);
        Assert.NotSame(messages[2], updated.Messages[2]);
        Assert.Null(updated.Messages[0].Extensions);
        Assert.Null(updated.Messages[1].Extensions);
        var ext = ExtensionsOf(updated.Messages[2]);
        Assert.Equal(2, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
        Assert.Null(messages[2].Extensions);
    }

    [Fact]
    public void Generic_InterfaceViews_ShowSameMessagesAndExtensions_Test()
    {
        var r = MinimalResult<int>.Success(5).WithExtension("a", 1).WithExtension("b", 2);

        IMinimalResult<int> typed = r;
        IMinimalResult plain = r;

        Assert.Equal(5, typed.Response);
        Assert.True(typed.IsSuccess);
        Assert.True(plain.IsSuccess);
        Assert.False(plain.IsFailure);
        Assert.Same(r.Messages, typed.Messages);
        Assert.Same(r.Messages, plain.Messages);
        var ext = ExtensionsOf(OnlyMessage(plain.Messages));
        Assert.Equal(2, ext.Count);
        Assert.Equal(1, ext["a"]);
        Assert.Equal(2, ext["b"]);
    }

    [Fact]
    public void NonGeneric_InterfaceViews_ShowSameMessagesAndExtensions_Test()
    {
        var r = MinimalResult.Failure("E", "M").WithExtension("a", 1);

        IMinimalResult<object?> typed = r;
        IMinimalResult plain = r;

        Assert.Null(typed.Response);
        Assert.True(typed.IsFailure);
        Assert.True(plain.IsFailure);
        Assert.Same(r.Messages, typed.Messages);
        Assert.Same(r.Messages, plain.Messages);
        var message = OnlyMessage(plain.Messages);
        Assert.Equal("E", message.Code);
        Assert.Equal(1, Assert.Single(ExtensionsOf(message)).Value);
    }

    [Fact]
    public void NonGeneric_ChainIntermediates_KeepTheirOwnCounts_Test()
    {
        var r0 = MinimalResult.Success();
        var r1 = r0.WithExtension("a", 1);
        var r2 = r1.WithExtension("b", 2);
        var r3 = r2.WithExtension("c", 3);

        Assert.Empty(r0.Messages);
        Assert.Single(ExtensionsOf(OnlyMessage(r1.Messages)));
        Assert.Equal(2, ExtensionsOf(OnlyMessage(r2.Messages)).Count);
        Assert.Equal(3, ExtensionsOf(OnlyMessage(r3.Messages)).Count);
        Assert.False(ExtensionsOf(OnlyMessage(r1.Messages)).ContainsKey("b"));
        Assert.False(ExtensionsOf(OnlyMessage(r2.Messages)).ContainsKey("c"));
    }

    [Fact]
    public void Generic_ChainIntermediates_KeepTheirOwnCounts_Test()
    {
        var r0 = MinimalResult<int>.Failure("E", "M");
        var r1 = r0.WithExtension("a", 1);
        var r2 = r1.WithExtension("b", 2);
        var r3 = r2.WithExtension("c", 3);

        Assert.Empty(ExtensionsOf(OnlyMessage(r0.Messages)));
        Assert.Single(ExtensionsOf(OnlyMessage(r1.Messages)));
        Assert.Equal(2, ExtensionsOf(OnlyMessage(r2.Messages)).Count);
        Assert.Equal(3, ExtensionsOf(OnlyMessage(r3.Messages)).Count);
        Assert.Empty(ExtensionsOf(OnlyMessage(MinimalResult<int>.Failure("x", "y").Messages)));
    }

    [Fact]
    public void ValuesOfDifferentTypes_ComeBackEqualAndClassInstanceIsSameReference_Test()
    {
        var guid = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");
        var payload = new Payload("p");

        var nonGeneric = MinimalResult.Success()
            .WithExtension("i", 42)
            .WithExtension("s", "text")
            .WithExtension("g", guid)
            .WithExtension("p", payload);
        var generic = MinimalResult<int>.Failure("E", "M")
            .WithExtension("i", 42)
            .WithExtension("s", "text")
            .WithExtension("g", guid)
            .WithExtension("p", payload);

        foreach (var ext in new[] { ExtensionsOf(OnlyMessage(nonGeneric.Messages)), ExtensionsOf(OnlyMessage(generic.Messages)) })
        {
            Assert.Equal(4, ext.Count);
            Assert.Equal(42, ext["i"]);
            Assert.Equal("text", ext["s"]);
            Assert.Equal(guid, Assert.IsType<Guid>(ext["g"]));
            Assert.Same(payload, ext["p"]);
        }
    }

    [Fact]
    public void LongChainOfFifty_HasFiftyExtensions_Test()
    {
        var nonGeneric = MinimalResult.Success();
        var generic = MinimalResult<int>.Success(7);

        for (var i = 0; i < 50; i++)
        {
            nonGeneric = nonGeneric.WithExtension("k" + i, i);
            generic = generic.WithExtension("k" + i, i);
        }

        var nonGenericExt = ExtensionsOf(OnlyMessage(nonGeneric.Messages));
        var genericExt = ExtensionsOf(OnlyMessage(generic.Messages));
        Assert.Equal(50, nonGenericExt.Count);
        Assert.Equal(50, genericExt.Count);
        Assert.Equal(7, generic.Response);
        for (var i = 0; i < 50; i++)
        {
            Assert.Equal(i, nonGenericExt["k" + i]);
            Assert.Equal(i, genericExt["k" + i]);
        }
    }

    [Fact]
    public void NonGeneric_Chains_KeepConcreteTypeAtEveryStep_Test()
    {
        MinimalResult fromFalse = false;
        var starts = new[]
        {
            MinimalResult.Success(),
            MinimalResult.Failure("E", "M"),
            MinimalResult.Failure(new List<ResultMessage>()),
            fromFalse,
        };

        foreach (var start in starts)
        {
            var step1 = start.WithExtension("a", 1);
            var step2 = step1.WithExtension("b", 2);
            var step3 = step2.WithExtension("c", 3);

            Assert.IsType<MinimalResult>(step1);
            Assert.IsType<MinimalResult>(step2);
            Assert.IsType<MinimalResult>(step3);
            Assert.Equal(start.IsSuccess, step3.IsSuccess);
        }
    }
}
