module RzR.MinimalResultMessage.FSharp.Tests.ConversionTests

#nowarn "3391"

open System
open Xunit
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.Abstractions

[<Fact>]
let NonGenericSuccess_HasNoMessagesAndNullResponse_Test () =
    let r = MinimalResult.Success()

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Null(r.Response)
    Assert.Equal<int>(0, r.Messages.Count)

[<Fact>]
let NonGenericFailure_WithCodeAndMessage_CarriesOneMatchingMessage_Test () =
    let r = MinimalResult.Failure("E1", "Boom")

    Assert.True(r.IsFailure)
    Assert.Null(r.Response)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("E1", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)
    Assert.Equal<int>(0, r.Messages.[0].Extensions.Count)

[<Fact>]
let NonGenericFailure_WithMessageList_KeepsSameList_Test () =
    let messages = [| new ResultMessage("E1", "One") |] :> Collections.Generic.IReadOnlyList<ResultMessage>

    let r = MinimalResult.Failure(messages)

    Assert.True(r.IsFailure)
    Assert.Same(messages, r.Messages)

[<Fact>]
let NonGenericFactories_ReturnNonGenericType_Test () =
    Assert.Equal(typeof<MinimalResult>, MinimalResult.Success().GetType())
    Assert.Equal(typeof<MinimalResult>, MinimalResult.Failure("E1", "Boom").GetType())

[<Fact>]
let NonGenericMinimalResult_IsMinimalResultOfObjAndBothInterfaces_Test () =
    let r = MinimalResult.Failure("E1", "Boom")

    Assert.Equal(typeof<MinimalResult<obj>>, typeof<MinimalResult>.BaseType)
    Assert.IsAssignableFrom<MinimalResult<obj>>(r) |> ignore
    Assert.IsAssignableFrom<IMinimalResult<obj>>(r) |> ignore
    Assert.IsAssignableFrom<IMinimalResult>(r) |> ignore
    Assert.True((r :> IMinimalResult).IsFailure)

[<Fact>]
let NonGenericType_InheritedGenericSuccess_IsStillVisible_Test () =
    let r = MinimalResult.Success(box 42)

    Assert.Equal(typeof<MinimalResult<obj>>, r.GetType())
    Assert.Equal(box 42, r.Response)

[<Fact>]
let TrueConversion_IsNonGenericSuccess_Test () =
    let r : MinimalResult = MinimalResult.op_Implicit true

    Assert.True(r.IsSuccess)
    Assert.Equal<int>(0, r.Messages.Count)
    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())

[<Fact>]
let FalseConversion_IsFailureWithUnSuccessfullyReqExecCodeAndNoMessage_Test () =
    let r : MinimalResult = MinimalResult.op_Implicit false

    Assert.True(r.IsFailure)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("UnSuccessfullyReqExec", r.Messages.[0].Code)
    Assert.Null(r.Messages.[0].Message)
    Assert.Equal<int>(0, r.Messages.[0].Extensions.Count)

[<Fact>]
let ExceptionConversion_ToGeneric_IsFailureWithUnhandledExceptionCode_Test () =
    let r : MinimalResult<int> = MinimalResult<int>.op_Implicit (InvalidOperationException "Boom" :> exn)

    Assert.True(r.IsFailure)
    Assert.Equal<int>(0, r.Response)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("UnhandledException", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)

[<Fact>]
let ExceptionConversion_ToNonGeneric_IsFailureWithUnhandledExceptionCode_Test () =
    let r : MinimalResult = MinimalResult.op_Implicit (InvalidOperationException "Boom" :> exn)

    Assert.Equal(typeof<MinimalResult>, r.GetType())
    Assert.True(r.IsFailure)
    Assert.Equal<string>("UnhandledException", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)

[<Fact>]
let NullExceptionConversion_ThrowsArgumentNullException_Test () =
    let generic =
        Assert.Throws<ArgumentNullException>(Action(fun () ->
            (MinimalResult<int>.op_Implicit (null: exn) : MinimalResult<int>) |> ignore))
    let nonGeneric =
        Assert.Throws<ArgumentNullException>(Action(fun () ->
            (MinimalResult.op_Implicit (null: exn) : MinimalResult) |> ignore))

    Assert.Equal<string>("exception", generic.ParamName)
    Assert.Equal<string>("exception", nonGeneric.ParamName)

[<Fact>]
let ValueConversion_IncludingDefault_IsGenericSuccess_Test () =
    let five : MinimalResult<int> = MinimalResult<int>.op_Implicit 5
    let zero : MinimalResult<int> = MinimalResult<int>.op_Implicit 0

    Assert.True(five.IsSuccess)
    Assert.Equal<int>(5, five.Response)
    Assert.Equal<int>(0, five.Messages.Count)
    Assert.True(zero.IsSuccess)
    Assert.Equal<int>(0, zero.Response)
    Assert.Equal<int>(0, zero.Messages.Count)

[<Fact>]
let NullStringConversion_IsSuccess_Test () =
    let r : MinimalResult<string> = MinimalResult<string>.op_Implicit (null: string)

    Assert.True(r.IsSuccess)
    Assert.Null(r.Response)

let private takesResult (r: MinimalResult) = r

[<Fact>]
let TypeDirectedConversions_BehaveLikeExplicitOnes_Test () =
    let fromFalse : MinimalResult = false
    let fromException : MinimalResult = Exception "x"
    let fromValue : MinimalResult<int> = 5
    let passedFalse = takesResult false

    Assert.True(fromFalse.IsFailure)
    Assert.Equal<string>("UnSuccessfullyReqExec", fromFalse.Messages.[0].Code)
    Assert.True(fromException.IsFailure)
    Assert.Equal<string>("x", fromException.Messages.[0].Message)
    Assert.Equal<int>(5, fromValue.Response)
    Assert.True(passedFalse.IsFailure)
