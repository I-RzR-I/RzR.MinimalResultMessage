using RzR.MinimalResultMessage.Abstractions;
using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

public class ConversionTests
{
    [Fact]
    public void Success_HasNoMessagesAndNullResponse_Test()
    {
        var r = MinimalResult.Success();

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Null(r.Response);
        Assert.Empty(r.Messages);
    }

    [Fact]
    public void Failure_CarriesOneMessageWithThatCodeAndMessage_Test()
    {
        var r = MinimalResult.Failure("E1", "Boom");
        Assert.True(r.IsFailure);
        Assert.Null(r.Response);

        var message = Assert.Single(r.Messages);
        Assert.Equal("E1", message.Code);
        Assert.Equal("Boom", message.Message);
        Assert.Empty(message.Extensions!);
    }

    [Fact]
    public void FailureWithMessageList_KeepsThatList_Test()
    {
        var messages = new List<ResultMessage>(TestData.TwoMessages());

        var r = MinimalResult.Failure(messages);

        Assert.True(r.IsFailure);
        Assert.Same(messages, r.Messages);
    }

    [Fact]
    public void NonGenericResult_IsAMinimalResultOfObjectAndBothInterfaces_Test()
    {
        var r = MinimalResult.Failure("E1", "Boom");
        Assert.Equal(typeof(MinimalResult<object>), typeof(MinimalResult).BaseType);
        Assert.IsAssignableFrom<MinimalResult<object>>(r);
        Assert.IsAssignableFrom<IMinimalResult<object>>(r);

        IMinimalResult view = r;
        Assert.True(view.IsFailure);
    }

    [Fact]
    public void True_ConvertsToSuccess_Test()
    {
        MinimalResult r = true;

        Assert.True(r.IsSuccess);
        Assert.Empty(r.Messages);
    }

    [Fact]
    public void False_ConvertsToFailureWithUnSuccessfullyReqExecCode_Test()
    {
        MinimalResult r = false;

        Assert.True(r.IsFailure);

        var message = Assert.Single(r.Messages);
        Assert.Equal("UnSuccessfullyReqExec", message.Code);
        Assert.Null(message.Message);
        Assert.Empty(message.Extensions!);
    }

    [Fact]
    public void Exception_ConvertsToGenericFailureWithUnhandledExceptionCode_Test()
    {
        MinimalResult<int> r = new InvalidOperationException("Boom");

        Assert.True(r.IsFailure);
        Assert.Equal(0, r.Response);

        var message = Assert.Single(r.Messages);
        Assert.Equal("UnhandledException", message.Code);
        Assert.Equal("Boom", message.Message);
    }

    [Fact]
    public void Exception_ConvertsToNonGenericFailureWithUnhandledExceptionCode_Test()
    {
        MinimalResult r = new InvalidOperationException("Boom");

        Assert.IsType<MinimalResult>(r);
        Assert.True(r.IsFailure);

        var message = Assert.Single(r.Messages);
        Assert.Equal("UnhandledException", message.Code);
        Assert.Equal("Boom", message.Message);
    }

    [Fact]
    public void NullException_IsRejectedWithArgumentNullException_Test()
    {
        Exception? missing = null;

        var generic = Assert.Throws<ArgumentNullException>(() => { MinimalResult<int> r = missing!; });
        var nonGeneric = Assert.Throws<ArgumentNullException>(() => { MinimalResult r = missing!; });

        Assert.Equal("exception", generic.ParamName);
        Assert.Equal("exception", nonGeneric.ParamName);
    }

    [Fact]
    public void Value_ConvertsToSuccess_IncludingTheDefaultValue_Test()
    {
        MinimalResult<int> five = 5;
        MinimalResult<int> zero = 0;

        Assert.True(five.IsSuccess);
        Assert.Equal(5, five.Response);
        Assert.Empty(five.Messages);
        Assert.True(zero.IsSuccess);
    }

    [Fact]
    public void Conversions_WorkAsReturnValues_Test()
    {
        Assert.True(Check(true).IsSuccess);
        Assert.True(Check(false).IsFailure);
        Assert.Equal(42, Parse("42").Response);
        Assert.Equal("UnhandledException", Assert.Single(Parse("x").Messages).Code);

        return;

        static MinimalResult<int> Parse(string text)
        {
            try { return int.Parse(text); }
            catch (FormatException ex) { return ex; }
        }

        static MinimalResult Check(bool ok) => ok;
    }

    [Fact]
    public void BoolToGenericResultOfObject_IsASuccessHoldingTheBool_Test()
    {
        MinimalResult<object> r = false;

        Assert.True(r.IsSuccess);
        Assert.Equal(false, r.Response);
    }
}
