module RzR.MinimalResultMessage.FSharp.Tests.MinimalResultInterfaceTests

open System
open System.Collections.Generic
open System.Reflection
open Xunit
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.Abstractions
open RzR.MinimalResultMessage.FSharp.Tests.TestData

[<Fact>]
let MinimalResult_IsAssignableToIMinimalResultOfSameT_Test () =
    Assert.True(typeof<IMinimalResult<int>>.IsAssignableFrom(typeof<MinimalResult<int>>))
    Assert.True(typeof<IMinimalResult<string>>.IsAssignableFrom(typeof<MinimalResult<string>>))
    Assert.True(((box (MinimalResult<int>.Success(1))) :? IMinimalResult<int>))

[<Theory>]
[<InlineData("success")>]
[<InlineData("success-null")>]
[<InlineData("failure-code")>]
[<InlineData("failure-list")>]
[<InlineData("failure-empty")>]
[<InlineData("failure-null")>]
let InterfaceAndConcrete_ExposeSameValues_ForString_Test (key: string) =
    let r = stringScenario key
    let i = r :> IMinimalResult<string>

    Assert.Equal<bool>(r.IsSuccess, i.IsSuccess)
    Assert.Equal<bool>(r.IsFailure, i.IsFailure)
    Assert.Equal<string>(r.Response, i.Response)
    Assert.True(obj.ReferenceEquals(r.Messages, i.Messages), "Messages differ by reference")

    Assert.Equal<bool>(expectedIsSuccess key, i.IsSuccess)
    Assert.Equal<string>(expectedStringResponse key, i.Response)
    match expectedMessageCount key with
    | Some count ->
        Assert.Equal<int>(count, r.Messages.Count)
        Assert.Equal<int>(count, i.Messages.Count)
    | None ->
        Assert.Null(box r.Messages)
        Assert.Null(box i.Messages)

[<Theory>]
[<InlineData("success")>]
[<InlineData("failure-code")>]
[<InlineData("failure-list")>]
[<InlineData("failure-empty")>]
[<InlineData("failure-null")>]
let InterfaceAndConcrete_ExposeSameValues_ForInt_Test (key: string) =
    let r = intScenario key
    let i = r :> IMinimalResult<int>

    Assert.Equal<bool>(r.IsSuccess, i.IsSuccess)
    Assert.Equal<bool>(r.IsFailure, i.IsFailure)
    Assert.Equal<int>(r.Response, i.Response)
    Assert.True(obj.ReferenceEquals(r.Messages, i.Messages), "Messages differ by reference")

    Assert.Equal<bool>(expectedIsSuccess key, i.IsSuccess)
    Assert.Equal<int>(expectedIntResponse key, i.Response)
    match expectedMessageCount key with
    | Some count ->
        Assert.Equal<int>(count, r.Messages.Count)
        Assert.Equal<int>(count, i.Messages.Count)
    | None ->
        Assert.Null(box r.Messages)
        Assert.Null(box i.Messages)

[<Theory>]
[<InlineData("success")>]
[<InlineData("success-null")>]
[<InlineData("failure-code")>]
[<InlineData("failure-list")>]
[<InlineData("failure-empty")>]
[<InlineData("failure-null")>]
let IsFailure_EqualsNotIsSuccess_ForString_Test (key: string) =
    let r = stringScenario key
    let i = r :> IMinimalResult<string>

    Assert.Equal<bool>(not r.IsSuccess, r.IsFailure)
    Assert.Equal<bool>(not i.IsSuccess, i.IsFailure)
    Assert.Equal<bool>(expectedIsSuccess key, r.IsSuccess)

[<Theory>]
[<InlineData("success")>]
[<InlineData("failure-code")>]
[<InlineData("failure-list")>]
[<InlineData("failure-empty")>]
[<InlineData("failure-null")>]
let IsFailure_EqualsNotIsSuccess_ForInt_Test (key: string) =
    let r = intScenario key
    let i = r :> IMinimalResult<int>

    Assert.Equal<bool>(not r.IsSuccess, r.IsFailure)
    Assert.Equal<bool>(not i.IsSuccess, i.IsFailure)
    Assert.Equal<bool>(expectedIsSuccess key, r.IsSuccess)

[<Theory>]
[<InlineData("success")>]
[<InlineData("success-null")>]
[<InlineData("failure-code")>]
[<InlineData("failure-list")>]
[<InlineData("failure-empty")>]
[<InlineData("failure-null")>]
let Properties_ReadTwice_ReturnSameValues_ForString_Test (key: string) =
    let r = stringScenario key
    let i = r :> IMinimalResult<string>

    let success1 = r.IsSuccess
    let success2 = r.IsSuccess
    let failure1 = r.IsFailure
    let failure2 = r.IsFailure
    let response1 = r.Response
    let response2 = r.Response
    let messages1 = r.Messages
    let messages2 = r.Messages
    Assert.Equal<bool>(success1, success2)
    Assert.Equal<bool>(failure1, failure2)
    Assert.True(obj.ReferenceEquals(response1, response2), "concrete Response changed between reads")
    Assert.True(obj.ReferenceEquals(messages1, messages2), "concrete Messages changed between reads")

    let iSuccess1 = i.IsSuccess
    let iSuccess2 = i.IsSuccess
    let iFailure1 = i.IsFailure
    let iFailure2 = i.IsFailure
    let iResponse1 = i.Response
    let iResponse2 = i.Response
    let iMessages1 = i.Messages
    let iMessages2 = i.Messages
    Assert.Equal<bool>(iSuccess1, iSuccess2)
    Assert.Equal<bool>(iFailure1, iFailure2)
    Assert.True(obj.ReferenceEquals(iResponse1, iResponse2), "interface Response changed between reads")
    Assert.True(obj.ReferenceEquals(iMessages1, iMessages2), "interface Messages changed between reads")

    Assert.Equal<bool>(expectedIsSuccess key, r.IsSuccess)
    Assert.Equal<string>(expectedStringResponse key, r.Response)

[<Theory>]
[<InlineData("success")>]
[<InlineData("failure-code")>]
[<InlineData("failure-list")>]
[<InlineData("failure-empty")>]
[<InlineData("failure-null")>]
let Properties_ReadTwice_ReturnSameValues_ForInt_Test (key: string) =
    let r = intScenario key
    let i = r :> IMinimalResult<int>

    let success1 = r.IsSuccess
    let success2 = r.IsSuccess
    let failure1 = r.IsFailure
    let failure2 = r.IsFailure
    let response1 = r.Response
    let response2 = r.Response
    let messages1 = r.Messages
    let messages2 = r.Messages
    Assert.Equal<bool>(success1, success2)
    Assert.Equal<bool>(failure1, failure2)
    Assert.Equal<int>(response1, response2)
    Assert.True(obj.ReferenceEquals(messages1, messages2), "concrete Messages changed between reads")

    let iSuccess1 = i.IsSuccess
    let iSuccess2 = i.IsSuccess
    let iFailure1 = i.IsFailure
    let iFailure2 = i.IsFailure
    let iResponse1 = i.Response
    let iResponse2 = i.Response
    let iMessages1 = i.Messages
    let iMessages2 = i.Messages
    Assert.Equal<bool>(iSuccess1, iSuccess2)
    Assert.Equal<bool>(iFailure1, iFailure2)
    Assert.Equal<int>(iResponse1, iResponse2)
    Assert.True(obj.ReferenceEquals(iMessages1, iMessages2), "interface Messages changed between reads")

    Assert.Equal<bool>(expectedIsSuccess key, r.IsSuccess)
    Assert.Equal<int>(expectedIntResponse key, r.Response)

[<Fact>]
let MinimalResult_HasNoPublicConstructors_Test () =
    let t = typeof<MinimalResult<int>>

    let ctors = t.GetConstructors(BindingFlags.Public ||| BindingFlags.Instance)

    Assert.Equal<int>(0, ctors.Length)

[<Fact>]
let MinimalResult_ExposesNoPublicMembersNamedAfterPrivateFields_Test () =
    let t = typeof<MinimalResult<int>>
    let flags = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.Static

    for name in [ "isSuccess"; "response"; "messages" ] do
        let members = t.GetMember(name, flags)
        Assert.True((members.Length = 0), sprintf "Unexpected public member '%s'" name)

[<Fact>]
let MinimalResult_PublicStaticMethods_AreSuccessFailuresAndConversions_Test () =
    let t = typeof<MinimalResult<int>>
    let staticMethods = t.GetMethods(BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly)

    let names = staticMethods |> Array.map (fun m -> m.Name) |> Array.sort
    let parameterTypes (methodName: string) : Type[][] =
        staticMethods
        |> Array.filter (fun m -> m.Name = methodName)
        |> Array.map (fun m -> m.GetParameters() |> Array.map (fun p -> p.ParameterType))
    let failureOverloads = parameterTypes "Failure"
    let successOverloads = parameterTypes "Success"

    let conversionOverloads = parameterTypes "op_Implicit"

    Assert.Equal<string>("Failure,Failure,Failure,Success,op_Implicit,op_Implicit", String.Join(",", names))
    Assert.True(
        conversionOverloads |> Array.exists (fun ps -> ps = [| typeof<int> |]),
        "No op_Implicit taking exactly (int)")
    Assert.True(
        conversionOverloads |> Array.exists (fun ps -> ps = [| typeof<Exception> |]),
        "No op_Implicit taking exactly (Exception)")

    Assert.Equal<int>(3, failureOverloads.Length)
    Assert.True(
        failureOverloads |> Array.exists (fun ps -> ps = [| typeof<IReadOnlyList<ResultMessage>> |]),
        "No Failure overload taking exactly IReadOnlyList<ResultMessage>")
    Assert.True(
        failureOverloads |> Array.exists (fun ps -> ps = [| typeof<string>; typeof<string> |]),
        "No Failure overload taking exactly (string, string)")
    Assert.True(
        failureOverloads |> Array.exists (fun ps -> ps = [| typeof<string>; typeof<string>; typeof<IReadOnlyDictionary<string, obj>> |]),
        "No Failure overload taking exactly (string, string, IReadOnlyDictionary<string, obj>)")

    Assert.Equal<int>(1, successOverloads.Length)
    Assert.True((successOverloads.[0] = [| typeof<int> |]), "Success on MinimalResult<int> does not take exactly (int)")
