using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using RzR.MinimalResultMessage.Models;

namespace RzR.MinimalResultMessage.CSharp.Tests;

public class ResultMessageTests
{
    private static PropertyInfo GetPublicProperty(Type type, string name)
    {
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        return property!;
    }

    [Fact]
    public void Constructor_WithCodeMessageAndExtensions_ExposesValuesAndSameExtensionsReference_Test()
    {
        var extensions = new Dictionary<string, object> { ["k"] = 1 };

        var rm = new ResultMessage("E1", "Boom", extensions);

        Assert.Equal("E1", rm.Code);
        Assert.Equal("Boom", rm.Message);
        Assert.Same(extensions, rm.Extensions);
        Assert.Equal(1, Assert.IsType<int>(rm.Extensions!["k"]));
    }

    [Fact]
    public void ExtensionsProperty_IsDeclaredAsReadOnlyDictionaryOfStringObject_Test()
    {
        var property = GetPublicProperty(typeof(ResultMessage), nameof(ResultMessage.Extensions));

        Assert.Equal(typeof(IReadOnlyDictionary<string, object>), property.PropertyType);
    }

    [Fact]
    public void Constructor_WithAllNulls_DoesNotThrowAndReturnsNulls_Test()
    {
        var rm = new ResultMessage(null!, null!, null!);

        Assert.Null(rm.Code);
        Assert.Null(rm.Message);
        Assert.Null(rm.Extensions);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_DoesNotThrowAndReturnsEmptyStrings_Test()
    {
        var extensions = new Dictionary<string, object>();

        var rm = new ResultMessage(string.Empty, string.Empty, extensions);

        Assert.Equal(string.Empty, rm.Code);
        Assert.Equal(string.Empty, rm.Message);
        Assert.Same(extensions, rm.Extensions);
    }

    [Theory]
    [InlineData("Code")]
    [InlineData("Message")]
    [InlineData("Extensions")]
    public void PublicProperty_HasNoPublicSetter_Test(string propertyName)
    {
        var property = GetPublicProperty(typeof(ResultMessage), propertyName);

        Assert.Null(property.GetSetMethod(nonPublic: false));
        Assert.False(property.CanWrite, $"{propertyName} should not be writable");
    }

    [Fact]
    public void Equals_IsReferenceEquality_EqualOnlyToItself_Test()
    {
        var extensions = TestData.NewExtensions();
        var a = new ResultMessage("E1", "Boom", extensions);
        var b = new ResultMessage("E1", "Boom", extensions);

        Assert.NotSame(a, b);
        Assert.True(a.Equals((object)a), "reflexive");
        TestData.AssertNotEqualPair(a, b);
    }

    [Fact]
    public void Equals_NullOrDifferentType_ReturnsFalse_Test()
    {
        var a = new ResultMessage("E1", "Boom", TestData.NewExtensions());

        Assert.False(a.Equals((object?)null));
        Assert.False(a.Equals((object)"str"));
    }

    [Theory]
    [InlineData("E1", "Boom", "e1", "Boom")]
    [InlineData("E1", "Boom", "E1", "boom")]
    [InlineData("E1", "Boom", "E2", "Boom")]
    [InlineData("E1", "Boom", "E1", "Boom ")]
    public void Equals_DifferingOnlyInCodeOrMessage_IsNotEqual_Test(string code1, string message1, string code2, string message2)
    {
        var extensions = TestData.NewExtensions();
        var a = new ResultMessage(code1, message1, extensions);
        var b = new ResultMessage(code2, message2, extensions);

        TestData.AssertNotEqualPair(a, b);
    }

    [Theory]
    [InlineData("dictionary")]
    [InlineData("readonly-dictionary")]
    public void Equals_DistinctExtensionsInstancesWithIdenticalContent_IsNotEqual_Test(string shape)
    {
        IReadOnlyDictionary<string, object> ext1 = shape == "dictionary"
            ? TestData.NewExtensions()
            : new ReadOnlyDictionary<string, object>(TestData.NewExtensions());
        IReadOnlyDictionary<string, object> ext2 = shape == "dictionary"
            ? TestData.NewExtensions()
            : new ReadOnlyDictionary<string, object>(TestData.NewExtensions());
        var a = new ResultMessage("E1", "Boom", ext1);
        var b = new ResultMessage("E1", "Boom", ext2);

        Assert.NotSame(ext1, ext2);
        TestData.AssertNotEqualPair(a, b);
    }

    [Fact]
    public void Equals_BothExtensionsNull_IsNotEqual_Test()
    {
        var a = new ResultMessage("E1", "Boom");
        var b = new ResultMessage("E1", "Boom");

        TestData.AssertNotEqualPair(a, b);
    }

    [Fact]
    public void CSharpEqualityOperatorAndEquals_OnResultMessages_AreBothReferenceEquality_Test()
    {
        var extensions = TestData.NewExtensions();
        var a = new ResultMessage("E1", "Boom", extensions);
        var b = new ResultMessage("E1", "Boom", extensions);

        var operatorResult = a == b;

        Assert.False(operatorResult, "C# == has no op_Equality and should compare references");
        Assert.False(a.Equals(b), "Equals should also compare references");
    }

    [Fact]
    public void ResultMessage_ImplementsNoEqualityOrComparisonInterface_Test()
    {
        var type = typeof(ResultMessage);

        Assert.False(typeof(IEquatable<ResultMessage>).IsAssignableFrom(type), "should not implement IEquatable<Self>");
        Assert.False(typeof(IStructuralEquatable).IsAssignableFrom(type), "should not implement IStructuralEquatable");
        Assert.False(typeof(IComparable).IsAssignableFrom(type), "should not implement IComparable");
    }

    [Theory]
    [InlineData(typeof(MinimalResult<int>))]
    [InlineData(typeof(MinimalResult<string>))]
    [InlineData(typeof(MinimalResult))]
    public void Result_ImplementsNoEqualityOrComparisonInterface_Test(Type type)
    {
        var equatableOfSelf = typeof(IEquatable<>).MakeGenericType(type);

        Assert.False(equatableOfSelf.IsAssignableFrom(type), $"{type} should not implement IEquatable<Self>");
        Assert.False(typeof(IStructuralEquatable).IsAssignableFrom(type), $"{type} should not implement IStructuralEquatable");
        Assert.False(typeof(IComparable).IsAssignableFrom(type), $"{type} should not implement IComparable");
    }
}
