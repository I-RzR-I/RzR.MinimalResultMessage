module RzR.MinimalResultMessage.FSharp.Tests.ResultMessageTests

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open Xunit
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.FSharp.Tests.TestData

[<Fact>]
let ResultMessage_Constructor_ExposesCodeMessageAndSameExtensions_Test () =
    let d = extensionsWith "k" (box 1)

    let rm = new ResultMessage("E1", "Boom", asExtensions d)

    Assert.Equal<string>("E1", rm.Code)
    Assert.Equal<string>("Boom", rm.Message)
    Assert.Same(box d, box rm.Extensions)
    Assert.Equal<int>(1, unbox<int> (rm.Extensions.["k"]))

[<Fact>]
let Extensions_IsTypedReadOnlyDictionaryOfStringToObj_Test () =
    let prop = typeof<ResultMessage>.GetProperty("Extensions", BindingFlags.Public ||| BindingFlags.Instance)

    Assert.False(isNull prop, "Public instance property Extensions not found")
    Assert.Equal<Type>(typeof<IReadOnlyDictionary<string, obj>>, prop.PropertyType)

[<Fact>]
let DefaultConstructor_GivesEmptyCodeAndNullMessageAndExtensions_Test () =
    let rm = new ResultMessage()

    Assert.Equal<string>("", rm.Code)
    Assert.Null(box rm.Message)
    Assert.Null(box rm.Extensions)

[<Fact>]
let Constructor_WithEmptyCodeAndMessage_ReturnsThemUnchanged_Test () =
    let rm = new ResultMessage("", "")

    Assert.Equal<string>("", rm.Code)
    Assert.Equal<string>("", rm.Message)
    Assert.Null(box rm.Extensions)

[<Fact>]
let Properties_HaveNoPublicSetter_Test () =
    for name in [ "Code"; "Message"; "Extensions" ] do
        let prop = typeof<ResultMessage>.GetProperty(name, BindingFlags.Public ||| BindingFlags.Instance)
        Assert.False(isNull prop, sprintf "Public instance property %s not found" name)
        Assert.True(isNull (prop.GetSetMethod()), sprintf "Property %s exposes a public setter" name)

[<Fact>]
let ShorterConstructors_LeaveMessageAndExtensionsNull_Test () =
    let codeOnly = new ResultMessage("E1")
    let codeAndMessage = new ResultMessage("E1", "Boom")

    Assert.Equal<string>("E1", codeOnly.Code)
    Assert.Null(codeOnly.Message)
    Assert.Null(codeOnly.Extensions)
    Assert.Equal<string>("E1", codeAndMessage.Code)
    Assert.Equal<string>("Boom", codeAndMessage.Message)
    Assert.Null(codeAndMessage.Extensions)

[<Fact>]
let Equality_IsReferenceBased_EqualOnlyToSelf_Test () =
    let ext = asExtensions (extensionsWith "k" (box 1))
    let a = mkMessageWith "E1" "Boom" ext
    let b = mkMessageWith "E1" "Boom" ext

    Assert.NotSame(box a, box b)
    Assert.False(a.Equals(box b), "distinct instances with the same content are not Equal")
    Assert.True(a.Equals(box a), "reflexivity: a.Equals(a) expected true")
    Assert.False((a = b))
    Assert.False(a.Equals((null: obj)), "Equals(null) expected false")
    Assert.False(a.Equals(box "str"), "Equals(string) expected false")

[<Fact>]
let Equality_DifferentCodeCase_NotEqual_Test () =
    let ext = asExtensions (extensionsWith "k" (box 1))
    let a = mkMessageWith "E1" "Boom" ext
    let b = mkMessageWith "e1" "Boom" ext

    Assert.False(a.Equals(box b))
    Assert.False(b.Equals(box a))
    Assert.False((a = b))

[<Fact>]
let Equality_DifferentMessage_NotEqual_Test () =
    let ext = asExtensions (extensionsWith "k" (box 1))
    let a = mkMessageWith "E1" "Boom" ext
    let b = mkMessageWith "E1" "boom" ext

    Assert.False(a.Equals(box b))
    Assert.False(b.Equals(box a))
    Assert.False((a = b))

[<Fact>]
let Equality_DistinctDictionariesWithSameContent_NotEqual_Test () =
    let a = mkMessageWith "E1" "Boom" (asExtensions (extensionsWith "k" (box 1)))
    let b = mkMessageWith "E1" "Boom" (asExtensions (extensionsWith "k" (box 1)))

    Assert.False(a.Equals(box b))
    Assert.False((a = b))

[<Fact>]
let Equality_DictionaryVsReadOnlyDictionary_NotEqual_Test () =
    let plain = asExtensions (extensionsWith "k" (box 1))
    let wrapped = ReadOnlyDictionary<string, obj>(extensionsWith "k" (box 1)) :> IReadOnlyDictionary<string, obj>
    let a = mkMessageWith "E1" "Boom" plain
    let b = mkMessageWith "E1" "Boom" wrapped

    Assert.False(a.Equals(box b))
    Assert.False(b.Equals(box a))
    Assert.False((a = b))

[<Fact>]
let Equality_BothExtensionsNull_NotEqual_Test () =
    let a = mkMessage "E1" "Boom"
    let b = mkMessage "E1" "Boom"

    Assert.NotSame(box a, box b)
    Assert.False(a.Equals(box b))
    Assert.False((a = b))

[<Fact>]
let EqualityOperator_DistinctMessagesWithSameContent_IsFalse_Test () =
    let ext = asExtensions (extensionsWith "k" (box 1))
    let a = mkMessageWith "E1" "Boom" ext
    let b = mkMessageWith "E1" "Boom" ext

    Assert.NotSame(box a, box b)
    Assert.False((a = b))

[<Fact>]
let EqualityOperator_TwoSuccessOfOne_IsFalse_Test () =
    let a = MinimalResult<int>.Success(1)
    let b = MinimalResult<int>.Success(1)

    Assert.NotSame(box a, box b)
    Assert.False((a = b))

[<Fact>]
let ResultMessage_ImplementsNoEqualityOrComparisonInterface_Test () =
    let t = typeof<ResultMessage>

    Assert.False(typeof<IEquatable<ResultMessage>>.IsAssignableFrom(t))
    Assert.False(typeof<System.Collections.IStructuralEquatable>.IsAssignableFrom(t))
    Assert.False(typeof<IComparable>.IsAssignableFrom(t))

[<Fact>]
let MinimalResult_ImplementsNoEqualityOrComparisonInterface_Test () =
    let t = typeof<MinimalResult<int>>

    Assert.False(typeof<IEquatable<MinimalResult<int>>>.IsAssignableFrom(t))
    Assert.False(typeof<System.Collections.IStructuralEquatable>.IsAssignableFrom(t))
    Assert.False(typeof<IComparable>.IsAssignableFrom(t))
    Assert.False(typeof<IComparable<MinimalResult<int>>>.IsAssignableFrom(t))
    Assert.False(typeof<System.Collections.IStructuralComparable>.IsAssignableFrom(t))
