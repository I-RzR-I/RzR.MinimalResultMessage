module RzR.MinimalResultMessage.FSharp.Tests.FunctionalTests

open System
open System.Collections.Generic
open Xunit
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.Abstractions
open RzR.MinimalResultMessage.FSharp.Tests.TestData

[<Fact>]
let NonGenericSuccess_ChainOfThree_CreatesOneEmptyCodeMessageWithAllValues_Test () =
    let r = MinimalResult.Success().WithExtension("a", box 1).WithExtension("b", box 2).WithExtension("c", box 3)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.True(isNull (box r.Response))
    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("", r.Messages.[0].Code)
    Assert.True(isNull (box r.Messages.[0].Message))
    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(3, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<int>(2, unbox<int> ext.["b"])
    Assert.Equal<int>(3, unbox<int> ext.["c"])

[<Fact>]
let GenericSuccess_ChainOfThree_KeepsResponseAndCreatesOneMessage_Test () =
    let r = MinimalResult<int>.Success(5).WithExtension("a", box 1).WithExtension("b", box 2).WithExtension("c", box 3)

    Assert.True(r.IsSuccess)
    Assert.Equal<int>(5, r.Response)
    Assert.Equal<Type>(typeof<MinimalResult<int>>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("", r.Messages.[0].Code)
    Assert.True(isNull (box r.Messages.[0].Message))
    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(3, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<int>(2, unbox<int> ext.["b"])
    Assert.Equal<int>(3, unbox<int> ext.["c"])

[<Fact>]
let NonGenericFailure_ChainOfThree_KeepsCodeAndMessage_Test () =
    let r = MinimalResult.Failure("E", "M").WithExtension("a", box 1).WithExtension("b", box 2).WithExtension("c", box 3)

    Assert.True(r.IsFailure)
    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("E", r.Messages.[0].Code)
    Assert.Equal<string>("M", r.Messages.[0].Message)
    Assert.Equal<int>(3, r.Messages.[0].Extensions.Count)

[<Fact>]
let GenericFailure_ChainOfThree_KeepsCodeAndMessage_Test () =
    let r = MinimalResult<int>.Failure("E", "M").WithExtension("a", box 1).WithExtension("b", box 2).WithExtension("c", box 3)

    Assert.True(r.IsFailure)
    Assert.Equal<int>(0, r.Response)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("E", r.Messages.[0].Code)
    Assert.Equal<string>("M", r.Messages.[0].Message)
    Assert.Equal<int>(3, r.Messages.[0].Extensions.Count)

[<Fact>]
let NonGenericFailureWithExtensions_PlusThreeKeys_HasFourEntries_Test () =
    let initial = extensionsWith "x" (box 0) :> IReadOnlyDictionary<string, obj>

    let r = MinimalResult.Failure("E", "M", initial).WithExtension("a", box 1).WithExtension("b", box 2).WithExtension("c", box 3)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<int>(4, r.Messages.[0].Extensions.Count)
    Assert.Equal<int>(0, unbox<int> r.Messages.[0].Extensions.["x"])
    Assert.Equal<int>(3, unbox<int> r.Messages.[0].Extensions.["c"])

[<Fact>]
let GenericFailureWithExtensions_PlusThreeKeys_HasFourEntries_Test () =
    let initial = extensionsWith "x" (box 0) :> IReadOnlyDictionary<string, obj>

    let r = MinimalResult<int>.Failure("E", "M", initial).WithExtension("a", box 1).WithExtension("b", box 2).WithExtension("c", box 3)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<int>(4, r.Messages.[0].Extensions.Count)
    Assert.Equal<int>(0, unbox<int> r.Messages.[0].Extensions.["x"])
    Assert.Equal<int>(3, unbox<int> r.Messages.[0].Extensions.["c"])

[<Fact>]
let WithExtension_KeysDifferingOnlyByCase_AreTheSameKey_Test () =
    let generic = MinimalResult<int>.Failure("E", "M").WithExtension("Key", box 1).WithExtension("key", box 2)
    let nonGeneric = MinimalResult.Success().WithExtension("Key", box 1).WithExtension("key", box 2)

    Assert.Equal<int>(1, generic.Messages.[0].Extensions.Count)
    Assert.Equal<int>(2, unbox<int> generic.Messages.[0].Extensions.["Key"])
    Assert.Equal<int>(2, unbox<int> generic.Messages.[0].Extensions.["KEY"])
    Assert.Equal<int>(1, nonGeneric.Messages.[0].Extensions.Count)
    Assert.Equal<int>(2, unbox<int> nonGeneric.Messages.[0].Extensions.["Key"])
    Assert.Equal<int>(2, unbox<int> nonGeneric.Messages.[0].Extensions.["key"])

[<Fact>]
let FalseConversion_PlusExtension_AddsToUnSuccessfullyReqExecMessage_Test () =
    let converted : MinimalResult = MinimalResult.op_Implicit false

    let r = converted.WithExtension("k", box 1)

    Assert.True(r.IsFailure)
    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("UnSuccessfullyReqExec", r.Messages.[0].Code)
    Assert.True(isNull (box r.Messages.[0].Message))
    Assert.Equal<int>(1, r.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, unbox<int> r.Messages.[0].Extensions.["k"])

[<Fact>]
let TrueConversion_PlusExtension_CreatesNewMessage_Test () =
    let converted : MinimalResult = MinimalResult.op_Implicit true

    let r = converted.WithExtension("k", box 1)

    Assert.True(r.IsSuccess)
    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("", r.Messages.[0].Code)
    Assert.True(isNull (box r.Messages.[0].Message))
    Assert.Equal<int>(1, unbox<int> r.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(0, converted.Messages.Count)

[<Fact>]
let ExceptionConversion_PlusExtension_AddsToUnhandledExceptionMessage_Test () =
    let converted : MinimalResult = MinimalResult.op_Implicit (InvalidOperationException "Boom" :> exn)
    let convertedGeneric : MinimalResult<int> = MinimalResult<int>.op_Implicit (InvalidOperationException "Boom" :> exn)

    let r = converted.WithExtension("k", box 1)
    let rGeneric = convertedGeneric.WithExtension("k", box 1)

    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("UnhandledException", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)
    Assert.Equal<int>(1, unbox<int> r.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(1, rGeneric.Messages.Count)
    Assert.Equal<string>("UnhandledException", rGeneric.Messages.[0].Code)
    Assert.Equal<string>("Boom", rGeneric.Messages.[0].Message)
    Assert.Equal<int>(1, unbox<int> rGeneric.Messages.[0].Extensions.["k"])

[<Fact>]
let ExceptionConversion_UsesOuterMessageAndSharedEmptyExtensions_Test () =
    let shared = MinimalResult.Failure("x", "y").Messages.[0].Extensions

    let r : MinimalResult = MinimalResult.op_Implicit (Exception("outer", Exception("inner")))
    let rGeneric : MinimalResult<int> = MinimalResult<int>.op_Implicit (Exception("outer", Exception("inner")))

    Assert.Equal<string>("UnhandledException", r.Messages.[0].Code)
    Assert.Equal<string>("outer", r.Messages.[0].Message)
    Assert.Same(box shared, box r.Messages.[0].Extensions)
    Assert.Equal<string>("UnhandledException", rGeneric.Messages.[0].Code)
    Assert.Equal<string>("outer", rGeneric.Messages.[0].Message)
    Assert.Same(box shared, box rGeneric.Messages.[0].Extensions)

[<Fact>]
let ValueConversion_PlusExtension_KeepsResponseAndCreatesNewMessage_Test () =
    let converted : MinimalResult<int> = MinimalResult<int>.op_Implicit 5

    let r = converted.WithExtension("k", box 1)

    Assert.True(r.IsSuccess)
    Assert.Equal<int>(5, r.Response)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("", r.Messages.[0].Code)
    Assert.True(isNull (box r.Messages.[0].Message))
    Assert.Equal<int>(1, unbox<int> r.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(0, converted.Messages.Count)

[<Fact>]
let WithExtension_InterfaceViews_ShowSameMessagesAndExtensions_Test () =
    let generic = MinimalResult<int>.Success(5).WithExtension("k", box 1)
    let nonGeneric = MinimalResult.Failure("E", "M").WithExtension("k", box 1)

    let genericView = generic :> IMinimalResult<int>
    let genericBaseView = generic :> IMinimalResult
    let nonGenericView = nonGeneric :> IMinimalResult

    Assert.Equal<int>(5, genericView.Response)
    Assert.True(genericView.IsSuccess)
    Assert.Same(box generic.Messages, box genericView.Messages)
    Assert.Same(box generic.Messages, box genericBaseView.Messages)
    Assert.Equal<int>(1, unbox<int> genericBaseView.Messages.[0].Extensions.["k"])
    Assert.True(nonGenericView.IsFailure)
    Assert.Same(box nonGeneric.Messages, box nonGenericView.Messages)
    Assert.Equal<int>(1, unbox<int> nonGenericView.Messages.[0].Extensions.["k"])

[<Fact>]
let NonGenericFailureChain_OriginalAndIntermediates_AreUnchanged_Test () =
    let r0 = MinimalResult.Failure("E", "M")
    let r1 = r0.WithExtension("a", box 1)
    let r2 = r1.WithExtension("b", box 2)
    let r3 = r2.WithExtension("c", box 3)

    Assert.Equal<int>(0, r0.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, r1.Messages.[0].Extensions.Count)
    Assert.Equal<int>(2, r2.Messages.[0].Extensions.Count)
    Assert.Equal<int>(3, r3.Messages.[0].Extensions.Count)
    Assert.False(r1.Messages.[0].Extensions.ContainsKey("b"))
    Assert.False(r2.Messages.[0].Extensions.ContainsKey("c"))
    Assert.NotSame(box r0, box r1)
    Assert.NotSame(box r1, box r2)
    Assert.NotSame(box r2, box r3)

[<Fact>]
let GenericSuccessChain_OriginalAndIntermediates_AreUnchanged_Test () =
    let r0 = MinimalResult<int>.Success(5)
    let r1 = r0.WithExtension("a", box 1)
    let r2 = r1.WithExtension("b", box 2)
    let r3 = r2.WithExtension("c", box 3)

    Assert.Equal<int>(0, r0.Messages.Count)
    Assert.Equal<int>(1, r1.Messages.[0].Extensions.Count)
    Assert.Equal<int>(2, r2.Messages.[0].Extensions.Count)
    Assert.Equal<int>(3, r3.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, r1.Messages.Count)
    Assert.Equal<int>(1, r2.Messages.Count)
    Assert.Equal<int>(1, r3.Messages.Count)
    Assert.False(r1.Messages.[0].Extensions.ContainsKey("b"))
    Assert.False(r2.Messages.[0].Extensions.ContainsKey("c"))
    Assert.Equal<int>(5, r3.Response)

[<Fact>]
let WithExtension_ValuesOfDifferentTypes_ComeBackUnchanged_Test () =
    let text = String('t', 3)
    let id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301")
    let point = { X = 3; Y = -4 }
    let payload = Payload("p")
    let boxedText = box text
    let boxedPayload = box payload

    let r =
        MinimalResult<int>.Failure("E", "M")
            .WithExtension("int", box 42)
            .WithExtension("string", boxedText)
            .WithExtension("guid", box id)
            .WithExtension("point", box point)
            .WithExtension("payload", boxedPayload)

    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(5, ext.Count)
    Assert.Equal<int>(42, unbox<int> ext.["int"])
    Assert.Same(boxedText, ext.["string"])
    Assert.Equal<Guid>(id, unbox<Guid> ext.["guid"])
    Assert.True((unbox<Point> ext.["point"] = point))
    Assert.Same(boxedPayload, ext.["payload"])

[<Fact>]
let NonGenericWithExtension_ValuesOfDifferentTypes_ComeBackUnchanged_Test () =
    let id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301")
    let payload = Payload("p")
    let boxedPayload = box payload

    let r = MinimalResult.Success().WithExtension("guid", box id).WithExtension("payload", boxedPayload).WithExtension("none", null)

    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(3, ext.Count)
    Assert.Equal<Guid>(id, unbox<Guid> ext.["guid"])
    Assert.Same(boxedPayload, ext.["payload"])
    Assert.True(ext.ContainsKey("none"))
    Assert.True(isNull ext.["none"])

[<Fact>]
let NonGenericWithExtension_ChainOfFifty_HasFiftyEntriesAndKeepsType_Test () =
    let start = MinimalResult.Success()

    let r =
        [ 0 .. 49 ]
        |> List.fold (fun (acc: MinimalResult) i ->
            let next = acc.WithExtension(sprintf "k%d" i, box i)
            Assert.Equal<Type>(typeof<MinimalResult>, next.GetType())
            next) start

    Assert.Equal<int>(1, r.Messages.Count)
    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(50, ext.Count)
    for i in 0 .. 49 do
        Assert.Equal<int>(i, unbox<int> ext.[sprintf "k%d" i])
    Assert.Equal<int>(0, start.Messages.Count)

[<Fact>]
let GenericWithExtension_ChainOfFifty_HasFiftyEntries_Test () =
    let start = MinimalResult<int>.Failure("E", "M")

    let r =
        [ 0 .. 49 ]
        |> List.fold (fun (acc: MinimalResult<int>) i -> acc.WithExtension(sprintf "k%d" i, box i)) start

    Assert.Equal<Type>(typeof<MinimalResult<int>>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<int>(50, r.Messages.[0].Extensions.Count)
    Assert.Equal<int>(49, unbox<int> r.Messages.[0].Extensions.["k49"])
    Assert.Equal<int>(0, start.Messages.[0].Extensions.Count)

[<Fact>]
let NonGenericChains_KeepConcreteTypeAtEveryStep_Test () =
    let s0 = MinimalResult.Success()
    let s1 = s0.WithExtension("a", box 1)
    let s2 = s1.WithExtension("b", box 2)
    let f0 = MinimalResult.Failure("E", "M")
    let f1 = f0.WithExtension("a", box 1)
    let f2 = f1.WithExtension("b", box 2)
    let e0 = MinimalResult.Failure(emptyMessages ())
    let e1 = e0.WithExtension("a", box 1)
    let e2 = e1.WithExtension("b", box 2)

    for r in [ s0; s1; s2; f0; f1; f2; e0; e1; e2 ] do
        Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.True(s2.IsSuccess)
    Assert.True(f2.IsFailure)
    Assert.True(e2.IsFailure)
