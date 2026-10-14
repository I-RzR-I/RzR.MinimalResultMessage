namespace RzR.MinimalResultMessage.CSharp.Tests;

[CollectionDefinition("SharedEmptyExtensions", DisableParallelization = true)]
public class SharedEmptyExtensionsCollection
{
}

[Collection("SharedEmptyExtensions")]
public class SharedEmptyExtensionsMutationTests
{
    [Fact]
    public void FailureWithCode_Extensions_CannotBeMutatedViaGenericIDictionary_Test()
    {
        var ext = MinimalResult<int>.Failure("E1", "Boom").Messages[0].Extensions!;
        Assert.Empty(ext);
        Assert.False(ext is Dictionary<string, object>, "Extensions must not be a concrete Dictionary<string, object>");

        if (ext is IDictionary<string, object> dict)
        {
            Assert.Throws<NotSupportedException>(() => { dict.Add("x", 1); });
            Assert.Throws<NotSupportedException>(() => { dict["x"] = 1; });
            Assert.Throws<NotSupportedException>(() => { dict.Remove("x"); });
            Assert.Throws<NotSupportedException>(() => { dict.Clear(); });
        }

        Assert.Empty(ext);
        Assert.False(ext.ContainsKey("x"));
        Assert.Empty(MinimalResult<int>.Failure("E2", "Other").Messages[0].Extensions!);
        Assert.Empty(MinimalResult<string>.Failure("E3", "Another").Messages[0].Extensions!);
    }

    [Fact]
    public void FailureWithCode_Extensions_CannotBeMutatedViaNonGenericIDictionary_Test()
    {
        var ext = MinimalResult<int>.Failure("E1", "Boom").Messages[0].Extensions!;
        Assert.Empty(ext);
        Assert.False(ext is Dictionary<string, object>, "Extensions must not be a concrete Dictionary<string, object>");

        if (ext is System.Collections.IDictionary dict)
        {
            Assert.Throws<NotSupportedException>(() => { dict.Add("x", 1); });
            Assert.Throws<NotSupportedException>(() => { dict["x"] = 1; });
        }

        Assert.Empty(ext);
        Assert.False(ext.ContainsKey("x"));
        Assert.Empty(MinimalResult<int>.Failure("E2", "Other").Messages[0].Extensions!);
    }
}
