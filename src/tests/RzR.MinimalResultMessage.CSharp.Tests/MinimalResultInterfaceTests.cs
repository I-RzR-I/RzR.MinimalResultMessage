using System.Reflection;
using RzR.MinimalResultMessage.Abstractions;
using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

public class MinimalResultInterfaceTests
{
    [Fact]
    public void MinimalResultType_IsAssignableToIMinimalResult_Test()
    {
        Assert.True(typeof(IMinimalResult<int>).IsAssignableFrom(typeof(MinimalResult<int>)));
        Assert.True(typeof(IMinimalResult<string>).IsAssignableFrom(typeof(MinimalResult<string>)));
        Assert.True(typeof(IMinimalResult<Guid>).IsAssignableFrom(typeof(MinimalResult<Guid>)));
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.SuccessNull)]
    [InlineData(TestData.FailureCode)]
    [InlineData(TestData.FailureList)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void InterfaceAndConcrete_String_ExposeIdenticalValues_Test(string scenario)
    {
        var r = TestData.BuildString(scenario);
        IMinimalResult<string?> i = r;

        var expectedSuccess = TestData.ExpectSuccess(scenario);
        var expectedResponse = scenario == TestData.Success ? TestData.SuccessValue : null;

        Assert.Equal(expectedSuccess, r.IsSuccess);
        Assert.Equal(expectedSuccess, i.IsSuccess);
        Assert.Equal(!expectedSuccess, r.IsFailure);
        Assert.Equal(!expectedSuccess, i.IsFailure);
        Assert.Equal(expectedResponse, r.Response);
        Assert.Equal(expectedResponse, i.Response);
        Assert.Same(r.Messages, i.Messages);

        if (scenario == TestData.FailureNull)
        {
            Assert.Null(r.Messages);
            Assert.Null(i.Messages);
        }
        else
        {
            Assert.NotNull(i.Messages);
        }
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.FailureCode)]
    [InlineData(TestData.FailureList)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void InterfaceAndConcrete_Int_ExposeIdenticalValues_Test(string scenario)
    {
        var r = TestData.BuildInt(scenario);
        IMinimalResult<int> i = r;

        var expectedSuccess = TestData.ExpectSuccess(scenario);
        var expectedResponse = scenario == TestData.Success ? TestData.SuccessInt : 0;

        Assert.Equal(expectedSuccess, r.IsSuccess);
        Assert.Equal(expectedSuccess, i.IsSuccess);
        Assert.Equal(!expectedSuccess, r.IsFailure);
        Assert.Equal(!expectedSuccess, i.IsFailure);
        Assert.Equal(expectedResponse, r.Response);
        Assert.Equal(expectedResponse, i.Response);
        Assert.Same(r.Messages, i.Messages);

        if (scenario == TestData.FailureNull)
        {
            Assert.Null(r.Messages);
            Assert.Null(i.Messages);
        }
        else
        {
            Assert.NotNull(i.Messages);
        }
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.SuccessNull)]
    [InlineData(TestData.FailureCode)]
    [InlineData(TestData.FailureList)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void IsFailure_IsNegationOfIsSuccess_String_Test(string scenario)
    {
        var r = TestData.BuildString(scenario);
        IMinimalResult<string?> i = r;

        Assert.Equal(TestData.ExpectSuccess(scenario), r.IsSuccess);
        Assert.Equal(!r.IsSuccess, r.IsFailure);
        Assert.Equal(!i.IsSuccess, i.IsFailure);
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.FailureCode)]
    [InlineData(TestData.FailureList)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void IsFailure_IsNegationOfIsSuccess_Int_Test(string scenario)
    {
        var r = TestData.BuildInt(scenario);
        IMinimalResult<int> i = r;

        Assert.Equal(TestData.ExpectSuccess(scenario), r.IsSuccess);
        Assert.Equal(!r.IsSuccess, r.IsFailure);
        Assert.Equal(!i.IsSuccess, i.IsFailure);
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.SuccessNull)]
    [InlineData(TestData.FailureCode)]
    [InlineData(TestData.FailureList)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void Properties_ReadTwice_ReturnSameValueOrReference_String_Test(string scenario)
    {
        var r = TestData.BuildString(scenario);
        IMinimalResult<string?> i = r;

        var success1 = r.IsSuccess;
        var failure1 = r.IsFailure;
        var response1 = r.Response;
        var messages1 = r.Messages;
        var iSuccess1 = i.IsSuccess;
        var iFailure1 = i.IsFailure;
        var iResponse1 = i.Response;
        var iMessages1 = i.Messages;

        var success2 = r.IsSuccess;
        var failure2 = r.IsFailure;
        var response2 = r.Response;
        var messages2 = r.Messages;
        var iSuccess2 = i.IsSuccess;
        var iFailure2 = i.IsFailure;
        var iResponse2 = i.Response;
        var iMessages2 = i.Messages;

        Assert.Equal(success1, success2);
        Assert.Equal(failure1, failure2);
        Assert.Same(response1, response2);
        Assert.Same(messages1, messages2);
        Assert.Equal(iSuccess1, iSuccess2);
        Assert.Equal(iFailure1, iFailure2);
        Assert.Same(iResponse1, iResponse2);
        Assert.Same(iMessages1, iMessages2);
    }

    [Theory]
    [InlineData(TestData.Success)]
    [InlineData(TestData.FailureCode)]
    [InlineData(TestData.FailureList)]
    [InlineData(TestData.FailureEmpty)]
    [InlineData(TestData.FailureNull)]
    public void Properties_ReadTwice_ReturnSameValueOrReference_Int_Test(string scenario)
    {
        var r = TestData.BuildInt(scenario);
        IMinimalResult<int> i = r;

        var success1 = r.IsSuccess;
        var failure1 = r.IsFailure;
        var response1 = r.Response;
        var messages1 = r.Messages;
        var iSuccess1 = i.IsSuccess;
        var iFailure1 = i.IsFailure;
        var iResponse1 = i.Response;
        var iMessages1 = i.Messages;

        var success2 = r.IsSuccess;
        var failure2 = r.IsFailure;
        var response2 = r.Response;
        var messages2 = r.Messages;
        var iSuccess2 = i.IsSuccess;
        var iFailure2 = i.IsFailure;
        var iResponse2 = i.Response;
        var iMessages2 = i.Messages;

        Assert.Equal(success1, success2);
        Assert.Equal(failure1, failure2);
        Assert.Equal(response1, response2);
        Assert.Same(messages1, messages2);
        Assert.Equal(iSuccess1, iSuccess2);
        Assert.Equal(iFailure1, iFailure2);
        Assert.Equal(iResponse1, iResponse2);
        Assert.Same(iMessages1, iMessages2);
    }

    [Theory]
    [InlineData(typeof(MinimalResult<int>))]
    [InlineData(typeof(MinimalResult<string>))]
    public void PublicSurface_HasNoPublicConstructorsOrRepresentationMembers_Test(Type type)
    {
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        foreach (var name in new[] { "isSuccess", "response", "messages" })
        {
            Assert.Empty(type.GetMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        }
    }

    [Theory]
    [InlineData(typeof(MinimalResult<int>))]
    [InlineData(typeof(MinimalResult<string>))]
    public void PublicSurface_StaticMethodsAreSuccessThreeFailuresAndTwoConversions_Test(Type type)
    {
        var statics = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        var names = statics.Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "Failure", "Failure", "Failure", "Success", "op_Implicit", "op_Implicit" }, names);

        var conversionParameterTypes = statics
            .Where(m => m.Name == "op_Implicit")
            .Select(m => m.GetParameters().Select(p => p.ParameterType).ToArray())
            .ToList();
        Assert.Contains(conversionParameterTypes, p => p.SequenceEqual(new[] { type.GetGenericArguments()[0] }));
        Assert.Contains(conversionParameterTypes, p => p.SequenceEqual(new[] { typeof(Exception) }));

        var failureParameterTypes = statics
            .Where(m => m.Name == "Failure")
            .Select(m => m.GetParameters().Select(p => p.ParameterType).ToArray())
            .ToList();
        Assert.Equal(3, failureParameterTypes.Count);
        Assert.Contains(failureParameterTypes, p => p.SequenceEqual(new[] { typeof(IReadOnlyList<ResultMessage>) }));
        Assert.Contains(failureParameterTypes, p => p.SequenceEqual(new[] { typeof(string), typeof(string) }));
        Assert.Contains(failureParameterTypes, p => p.SequenceEqual(new[] { typeof(string), typeof(string), typeof(IReadOnlyDictionary<string, object>) }));

        var success = Assert.Single(statics, m => m.Name == "Success");
        var successParameterTypes = success.GetParameters().Select(p => p.ParameterType).ToArray();
        Assert.Equal(new[] { type.GetGenericArguments()[0] }, successParameterTypes);
    }

    [Fact]
    public void PublicSurface_SuccessOnIntTakesExactlyInt_Test()
    {
        var success = Assert.Single(
            typeof(MinimalResult<int>).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly),
            m => m.Name == "Success");

        var parameterTypes = success.GetParameters().Select(p => p.ParameterType).ToArray();

        Assert.Equal(new[] { typeof(int) }, parameterTypes);
    }
}
