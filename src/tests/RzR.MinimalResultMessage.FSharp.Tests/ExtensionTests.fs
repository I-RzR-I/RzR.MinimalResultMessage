module RzR.MinimalResultMessage.FSharp.Tests.ExtensionTests

open System
open System.Collections.Generic
open Xunit
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.FSharp.Tests.TestData

let private sampleExtensions () : Dictionary<string, obj> =
    let d = Dictionary<string, obj>()
    d.["a"] <- box 1
    d.["b"] <- box "two"
    d

let private noExtensions : IReadOnlyDictionary<string, obj> = null

let private assertReadOnly (ext: IReadOnlyDictionary<string, obj>) =
    Assert.False(((box ext) :? Dictionary<string, obj>), "Extensions is a mutable Dictionary")
    Assert.True(((box ext) :? IDictionary<string, obj>), "Extensions does not implement IDictionary<string, obj>")
    Assert.True(((box ext) :? System.Collections.IDictionary), "Extensions does not implement IDictionary")
    let countBefore = ext.Count
    let d = (box ext) :?> IDictionary<string, obj>
    Assert.Throws<NotSupportedException>(Action(fun () -> d.Add("z", box 1))) |> ignore
    Assert.Throws<NotSupportedException>(Action(fun () -> d.["a"] <- box 99)) |> ignore
    Assert.Throws<NotSupportedException>(Action(fun () -> d.Remove("a") |> ignore)) |> ignore
    Assert.Throws<NotSupportedException>(Action(fun () -> d.Clear())) |> ignore
    let nd = (box ext) :?> System.Collections.IDictionary
    Assert.Throws<NotSupportedException>(Action(fun () -> nd.Add(box "z", box 1))) |> ignore
    Assert.Throws<NotSupportedException>(Action(fun () -> nd.[box "a"] <- box 99)) |> ignore
    Assert.Equal<int>(countBefore, ext.Count)
    Assert.False(ext.ContainsKey("z"))

[<Fact>]
let FailureWithExtensions_CarriesCodeMessageAndValues_Test () =
    let input = sampleExtensions () :> IReadOnlyDictionary<string, obj>

    let r = MinimalResult<int>.Failure("NotFound", "User not found", input)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("NotFound", r.Messages.[0].Code)
    Assert.Equal<string>("User not found", r.Messages.[0].Message)
    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(2, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<string>("two", unbox<string> ext.["b"])

[<Fact>]
let NonGenericFailureWithExtensions_CarriesCodeMessageAndValues_Test () =
    let input = sampleExtensions () :> IReadOnlyDictionary<string, obj>

    let r = MinimalResult.Failure("NotFound", "User not found", input)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("NotFound", r.Messages.[0].Code)
    Assert.Equal<string>("User not found", r.Messages.[0].Message)
    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(2, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<string>("two", unbox<string> ext.["b"])

[<Fact>]
let FailureWithExtensions_StoresCopyNotCallerReference_Test () =
    let caller = sampleExtensions ()

    let r = MinimalResult<int>.Failure("E1", "Boom", caller :> IReadOnlyDictionary<string, obj>)
    caller.["a"] <- box 99
    caller.Add("z", box 3)
    caller.Remove("b") |> ignore

    let ext = r.Messages.[0].Extensions
    Assert.NotSame(box caller, box ext)
    Assert.Equal<int>(2, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<string>("two", unbox<string> ext.["b"])
    Assert.False(ext.ContainsKey("z"))

[<Fact>]
let NonGenericFailureWithExtensions_StoresCopyNotCallerReference_Test () =
    let caller = sampleExtensions ()

    let r = MinimalResult.Failure("E1", "Boom", caller :> IReadOnlyDictionary<string, obj>)
    caller.["a"] <- box 99
    caller.Add("z", box 3)
    caller.Remove("b") |> ignore

    let ext = r.Messages.[0].Extensions
    Assert.NotSame(box caller, box ext)
    Assert.Equal<int>(2, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<string>("two", unbox<string> ext.["b"])
    Assert.False(ext.ContainsKey("z"))

[<Fact>]
let FailureWithExtensions_MutationThroughIDictionary_ThrowsNotSupported_Test () =
    let r = MinimalResult<int>.Failure("E1", "Boom", sampleExtensions () :> IReadOnlyDictionary<string, obj>)

    let ext = r.Messages.[0].Extensions

    assertReadOnly ext
    Assert.Equal<int>(1, unbox<int> ext.["a"])

[<Fact>]
let NonGenericFailureWithExtensions_MutationThroughIDictionary_ThrowsNotSupported_Test () =
    let r = MinimalResult.Failure("E1", "Boom", sampleExtensions () :> IReadOnlyDictionary<string, obj>)

    let ext = r.Messages.[0].Extensions

    assertReadOnly ext
    Assert.Equal<int>(1, unbox<int> ext.["a"])

[<Fact>]
let FailureWithNullExtensions_UsesSharedEmptyExtensions_Test () =
    let reference = MinimalResult<int>.Failure("E1", "Boom")

    let r = MinimalResult<int>.Failure("E1", "Boom", noExtensions)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("E1", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)
    Assert.Same(box reference.Messages.[0].Extensions, box r.Messages.[0].Extensions)

[<Fact>]
let NonGenericFailureWithNullExtensions_UsesSharedEmptyExtensions_Test () =
    let reference = MinimalResult.Failure("E1", "Boom")

    let r = MinimalResult.Failure("E1", "Boom", noExtensions)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("E1", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)
    Assert.Same(box reference.Messages.[0].Extensions, box r.Messages.[0].Extensions)

[<Fact>]
let FailureWithEmptyExtensions_UsesSharedEmptyExtensions_Test () =
    let reference = MinimalResult<int>.Failure("E1", "Boom")
    let empty = Dictionary<string, obj>()

    let r = MinimalResult<int>.Failure("E1", "Boom", empty :> IReadOnlyDictionary<string, obj>)

    Assert.NotSame(box empty, box r.Messages.[0].Extensions)
    Assert.Same(box reference.Messages.[0].Extensions, box r.Messages.[0].Extensions)

[<Fact>]
let NonGenericFailureWithEmptyExtensions_UsesSharedEmptyExtensions_Test () =
    let reference = MinimalResult.Failure("E1", "Boom")
    let empty = Dictionary<string, obj>()

    let r = MinimalResult.Failure("E1", "Boom", empty :> IReadOnlyDictionary<string, obj>)

    Assert.NotSame(box empty, box r.Messages.[0].Extensions)
    Assert.Same(box reference.Messages.[0].Extensions, box r.Messages.[0].Extensions)

[<Fact>]
let FailureWithExtensions_HasDefaultResponseAndIsFailure_Test () =
    let r = MinimalResult<int>.Failure("E1", "Boom", sampleExtensions () :> IReadOnlyDictionary<string, obj>)

    Assert.True(r.IsFailure)
    Assert.False(r.IsSuccess)
    Assert.Equal<int>(0, r.Response)
    Assert.Equal<Type>(typeof<MinimalResult<int>>, r.GetType())

[<Fact>]
let NonGenericFailureWithExtensions_HasNullResponseIsFailureAndNonGenericType_Test () =
    let r : MinimalResult = MinimalResult.Failure("E1", "Boom", sampleExtensions () :> IReadOnlyDictionary<string, obj>)

    Assert.True(r.IsFailure)
    Assert.False(r.IsSuccess)
    Assert.True(isNull (box r.Response))
    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())

[<Fact>]
let WithExtension_ReturnsNewInstanceAndLeavesOriginalUnchanged_Test () =
    let original = MinimalResult<int>.Failure("NotFound", "User not found")
    let originalMessages = original.Messages
    let originalMessage = original.Messages.[0]
    let originalExtensions = originalMessage.Extensions

    let updated = original.WithExtension("userId", box 42)

    Assert.NotSame(box original, box updated)
    Assert.Same(box originalMessages, box original.Messages)
    Assert.Same(box originalMessage, box original.Messages.[0])
    Assert.Same(box originalExtensions, box original.Messages.[0].Extensions)
    Assert.Equal<int>(0, original.Messages.[0].Extensions.Count)
    Assert.NotSame(box originalMessage, box updated.Messages.[0])
    Assert.Equal<int>(1, updated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(42, unbox<int> updated.Messages.[0].Extensions.["userId"])

[<Fact>]
let NonGenericWithExtension_ReturnsNewInstanceAndLeavesOriginalUnchanged_Test () =
    let original = MinimalResult.Failure("NotFound", "User not found")
    let originalMessages = original.Messages
    let originalMessage = original.Messages.[0]
    let originalExtensions = originalMessage.Extensions

    let updated = original.WithExtension("userId", box 42)

    Assert.NotSame(box original, box updated)
    Assert.Same(box originalMessages, box original.Messages)
    Assert.Same(box originalMessage, box original.Messages.[0])
    Assert.Same(box originalExtensions, box original.Messages.[0].Extensions)
    Assert.Equal<int>(0, original.Messages.[0].Extensions.Count)
    Assert.NotSame(box originalMessage, box updated.Messages.[0])
    Assert.Equal<int>(1, updated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(42, unbox<int> updated.Messages.[0].Extensions.["userId"])

[<Fact>]
let WithExtension_KeepsTypeFlagsAndResponse_Test () =
    let original = MinimalResult<int>.Failure("E1", "Boom")

    let updated : MinimalResult<int> = original.WithExtension("k", box 1)

    Assert.Equal<Type>(typeof<MinimalResult<int>>, updated.GetType())
    Assert.Equal<bool>(original.IsSuccess, updated.IsSuccess)
    Assert.Equal<bool>(original.IsFailure, updated.IsFailure)
    Assert.True(updated.IsFailure)
    Assert.Equal<int>(original.Response, updated.Response)
    Assert.Equal<int>(0, updated.Response)

[<Fact>]
let NonGenericWithExtension_KeepsTypeFlagsAndResponse_Test () =
    let original = MinimalResult.Failure("E1", "Boom")

    let updated : MinimalResult = original.WithExtension("k", box 1)

    Assert.Equal<Type>(typeof<MinimalResult>, updated.GetType())
    Assert.Equal<bool>(original.IsSuccess, updated.IsSuccess)
    Assert.Equal<bool>(original.IsFailure, updated.IsFailure)
    Assert.True(updated.IsFailure)
    Assert.True(isNull (box updated.Response))

[<Fact>]
let WithExtension_AddsToLastMessageOnly_Test () =
    let firstExtensions = extensionsWith "first" (box 1)
    let lastExtensions = extensionsWith "a" (box 1)
    let m0 = mkMessageWith "E1" "One" (asExtensions firstExtensions)
    let m1 = mkMessage "E2" "Two"
    let m2 = mkMessageWith "E3" "Three" (asExtensions lastExtensions)
    let original = MinimalResult<int>.Failure(asReadOnlyList [ m0; m1; m2 ])

    let updated = original.WithExtension("k", box "v")
    let twice = updated.WithExtension("k2", box 2)

    Assert.Equal<int>(3, twice.Messages.Count)
    Assert.Same(box m0, box twice.Messages.[0])
    Assert.Same(box m1, box twice.Messages.[1])
    Assert.Equal<int>(3, twice.Messages.[2].Extensions.Count)
    Assert.Equal<int>(2, updated.Messages.[2].Extensions.Count)

    Assert.Equal<int>(3, updated.Messages.Count)
    Assert.Same(box m0, box updated.Messages.[0])
    Assert.Same(box m1, box updated.Messages.[1])
    Assert.NotSame(box m2, box updated.Messages.[2])
    Assert.Equal<string>("E3", updated.Messages.[2].Code)
    Assert.Equal<string>("Three", updated.Messages.[2].Message)
    let ext = updated.Messages.[2].Extensions
    Assert.Equal<int>(2, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<string>("v", unbox<string> ext.["k"])
    Assert.Equal<int>(1, m0.Extensions.Count)
    Assert.Equal<int>(1, lastExtensions.Count)
    Assert.False(lastExtensions.ContainsKey("k"))
    Assert.Same(box m2, box original.Messages.[2])

[<Fact>]
let NonGenericWithExtension_AddsToLastMessageOnly_Test () =
    let firstExtensions = extensionsWith "first" (box 1)
    let lastExtensions = extensionsWith "a" (box 1)
    let m0 = mkMessageWith "E1" "One" (asExtensions firstExtensions)
    let m1 = mkMessage "E2" "Two"
    let m2 = mkMessageWith "E3" "Three" (asExtensions lastExtensions)
    let original = MinimalResult.Failure(asReadOnlyList [ m0; m1; m2 ])

    let updated = original.WithExtension("k", box "v")
    let twice = updated.WithExtension("k2", box 2)

    Assert.Equal<int>(3, twice.Messages.Count)
    Assert.Same(box m0, box twice.Messages.[0])
    Assert.Same(box m1, box twice.Messages.[1])
    Assert.Equal<int>(3, twice.Messages.[2].Extensions.Count)
    Assert.Equal<int>(2, updated.Messages.[2].Extensions.Count)

    Assert.Equal<int>(3, updated.Messages.Count)
    Assert.Same(box m0, box updated.Messages.[0])
    Assert.Same(box m1, box updated.Messages.[1])
    Assert.NotSame(box m2, box updated.Messages.[2])
    Assert.Equal<string>("E3", updated.Messages.[2].Code)
    Assert.Equal<string>("Three", updated.Messages.[2].Message)
    let ext = updated.Messages.[2].Extensions
    Assert.Equal<int>(2, ext.Count)
    Assert.Equal<int>(1, unbox<int> ext.["a"])
    Assert.Equal<string>("v", unbox<string> ext.["k"])
    Assert.Equal<int>(1, m0.Extensions.Count)
    Assert.Equal<int>(1, lastExtensions.Count)
    Assert.False(lastExtensions.ContainsKey("k"))
    Assert.Same(box m2, box original.Messages.[2])

[<Fact>]
let WithExtension_LastMessageWithNullExtensions_TreatsThemAsEmpty_Test () =
    let message = mkMessage "E1" "Boom"
    let original = MinimalResult<int>.Failure(asReadOnlyList [ message ])

    let updated = original.WithExtension("k", box 1)

    Assert.True(isNull (box message.Extensions))
    Assert.Equal<string>("E1", updated.Messages.[0].Code)
    Assert.Equal<string>("Boom", updated.Messages.[0].Message)
    Assert.Equal<int>(1, updated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, unbox<int> updated.Messages.[0].Extensions.["k"])

[<Fact>]
let NonGenericWithExtension_LastMessageWithNullExtensions_TreatsThemAsEmpty_Test () =
    let message = mkMessage "E1" "Boom"
    let original = MinimalResult.Failure(asReadOnlyList [ message ])

    let updated = original.WithExtension("k", box 1)

    Assert.True(isNull (box message.Extensions))
    Assert.Equal<string>("E1", updated.Messages.[0].Code)
    Assert.Equal<string>("Boom", updated.Messages.[0].Message)
    Assert.Equal<int>(1, updated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, unbox<int> updated.Messages.[0].Extensions.["k"])

[<Fact>]
let WithExtension_NewExtensions_AreReadOnly_Test () =
    let updated = MinimalResult<int>.Failure("E1", "Boom").WithExtension("a", box 1)

    assertReadOnly updated.Messages.[0].Extensions

[<Fact>]
let NonGenericWithExtension_NewExtensions_AreReadOnly_Test () =
    let updated = MinimalResult.Failure("E1", "Boom").WithExtension("a", box 1)

    assertReadOnly updated.Messages.[0].Extensions

[<Fact>]
let WithExtension_ExistingKey_IsOverwritten_Test () =
    let original = MinimalResult<int>.Failure("E1", "Boom", asExtensions (extensionsWith "k" (box 1)))

    let updated = original.WithExtension("k", box 2)
    let repeated = MinimalResult<int>.Success(5).WithExtension("k", box 1).WithExtension("k", box 2).WithExtension("k", box 3)

    Assert.Equal<int>(1, updated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(2, unbox<int> updated.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(1, unbox<int> original.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(1, repeated.Messages.Count)
    Assert.Equal<int>(1, repeated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(3, unbox<int> repeated.Messages.[0].Extensions.["k"])

[<Fact>]
let NonGenericWithExtension_ExistingKey_IsOverwritten_Test () =
    let original = MinimalResult.Failure("E1", "Boom", asExtensions (extensionsWith "k" (box 1)))

    let updated = original.WithExtension("k", box 2)
    let repeated = MinimalResult.Failure("E", "M").WithExtension("k", box 1).WithExtension("k", box 2).WithExtension("k", box 3)

    Assert.Equal<int>(1, updated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(2, unbox<int> updated.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(1, unbox<int> original.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(1, repeated.Messages.[0].Extensions.Count)
    Assert.Equal<int>(3, unbox<int> repeated.Messages.[0].Extensions.["k"])

[<Fact>]
let WithExtension_Chained_LastMessageHasAllKeys_Test () =
    let r = MinimalResult<int>.Failure("E1", "Boom").WithExtension("a", box 1).WithExtension("b", box 2)

    Assert.Equal<Type>(typeof<MinimalResult<int>>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<int>(2, r.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, unbox<int> r.Messages.[0].Extensions.["a"])
    Assert.Equal<int>(2, unbox<int> r.Messages.[0].Extensions.["b"])

[<Fact>]
let NonGenericWithExtension_Chained_LastMessageHasAllKeys_Test () =
    let r : MinimalResult = MinimalResult.Failure("E1", "Boom").WithExtension("a", box 1).WithExtension("b", box 2)

    Assert.Equal<Type>(typeof<MinimalResult>, r.GetType())
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<int>(2, r.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, unbox<int> r.Messages.[0].Extensions.["a"])
    Assert.Equal<int>(2, unbox<int> r.Messages.[0].Extensions.["b"])

[<Fact>]
let WithExtension_DoesNotModifySharedEmptyExtensions_Test () =
    let shared = MinimalResult<int>.Failure("x", "y").Messages.[0].Extensions

    MinimalResult<int>.Failure("E1", "Boom").WithExtension("leak", box 1) |> ignore
    MinimalResult.Failure("E1", "Boom").WithExtension("leak", box 1) |> ignore
    MinimalResult<int>.Failure("E1", "Boom", noExtensions).WithExtension("leak", box 1) |> ignore

    let fresh = MinimalResult<int>.Failure("x", "y").Messages.[0].Extensions
    let freshNonGeneric = MinimalResult.Failure("x", "y").Messages.[0].Extensions
    Assert.Same(box shared, box fresh)
    Assert.Equal<int>(0, fresh.Count)
    Assert.False(fresh.ContainsKey("leak"))
    Assert.Equal<int>(0, freshNonGeneric.Count)
    Assert.False(freshNonGeneric.ContainsKey("leak"))

[<Fact>]
let WithExtension_OnSuccess_CreatesMessageHoldingExtension_Test () =
    let generic = MinimalResult<int>.Success(1)
    let nonGeneric = MinimalResult.Success()

    let updatedGeneric = generic.WithExtension("k", box 1)
    let updatedNonGeneric = nonGeneric.WithExtension("k", box 1)

    Assert.True(updatedGeneric.IsSuccess)
    Assert.Equal<int>(1, updatedGeneric.Response)
    Assert.Equal<int>(1, updatedGeneric.Messages.Count)
    Assert.Equal<string>("", updatedGeneric.Messages.[0].Code)
    Assert.True(isNull (box updatedGeneric.Messages.[0].Message))
    Assert.Equal<int>(1, updatedGeneric.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, unbox<int> updatedGeneric.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(0, generic.Messages.Count)
    Assert.True(updatedNonGeneric.IsSuccess)
    Assert.Equal<Type>(typeof<MinimalResult>, updatedNonGeneric.GetType())
    Assert.Equal<int>(1, updatedNonGeneric.Messages.Count)
    Assert.Equal<string>("", updatedNonGeneric.Messages.[0].Code)
    Assert.True(isNull (box updatedNonGeneric.Messages.[0].Message))
    Assert.Equal<int>(1, unbox<int> updatedNonGeneric.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(0, nonGeneric.Messages.Count)

[<Fact>]
let WithExtension_OnFailureWithEmptyMessages_CreatesMessageHoldingExtension_Test () =
    let generic = MinimalResult<int>.Failure(emptyMessages ())
    let nonGeneric = MinimalResult.Failure(emptyMessages ())

    let updatedGeneric = generic.WithExtension("k", box 1)
    let updatedNonGeneric = nonGeneric.WithExtension("k", box 1)

    Assert.True(updatedGeneric.IsFailure)
    Assert.Equal<int>(1, updatedGeneric.Messages.Count)
    Assert.Equal<string>("", updatedGeneric.Messages.[0].Code)
    Assert.True(isNull (box updatedGeneric.Messages.[0].Message))
    Assert.Equal<int>(1, unbox<int> updatedGeneric.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(0, generic.Messages.Count)
    Assert.True(updatedNonGeneric.IsFailure)
    Assert.Equal<Type>(typeof<MinimalResult>, updatedNonGeneric.GetType())
    Assert.Equal<int>(1, updatedNonGeneric.Messages.Count)
    Assert.Equal<string>("", updatedNonGeneric.Messages.[0].Code)
    Assert.Equal<int>(1, unbox<int> updatedNonGeneric.Messages.[0].Extensions.["k"])
    Assert.Equal<int>(0, nonGeneric.Messages.Count)
    Assert.False(updatedGeneric.IsSuccess)
    Assert.False(updatedNonGeneric.IsSuccess)
    Assert.True(isNull (box updatedNonGeneric.Messages.[0].Message))
    Assert.Equal<int>(1, updatedGeneric.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, updatedNonGeneric.Messages.[0].Extensions.Count)

[<Fact>]
let WithExtension_OnFailureWithNullMessages_CreatesMessageHoldingExtension_Test () =
    let generic = MinimalResult<int>.Failure(nullMessages)
    let nonGeneric = MinimalResult.Failure(nullMessages)

    let updatedGeneric = generic.WithExtension("k", box 1)
    let updatedNonGeneric = nonGeneric.WithExtension("k", box 1)

    Assert.True(updatedGeneric.IsFailure)
    Assert.Equal<int>(1, updatedGeneric.Messages.Count)
    Assert.Equal<string>("", updatedGeneric.Messages.[0].Code)
    Assert.True(isNull (box updatedGeneric.Messages.[0].Message))
    Assert.Equal<int>(1, unbox<int> updatedGeneric.Messages.[0].Extensions.["k"])
    Assert.True(isNull (box generic.Messages))
    Assert.True(updatedNonGeneric.IsFailure)
    Assert.Equal<Type>(typeof<MinimalResult>, updatedNonGeneric.GetType())
    Assert.Equal<int>(1, updatedNonGeneric.Messages.Count)
    Assert.Equal<string>("", updatedNonGeneric.Messages.[0].Code)
    Assert.Equal<int>(1, unbox<int> updatedNonGeneric.Messages.[0].Extensions.["k"])
    Assert.True(isNull (box nonGeneric.Messages))
    Assert.False(updatedGeneric.IsSuccess)
    Assert.False(updatedNonGeneric.IsSuccess)
    Assert.True(isNull (box updatedNonGeneric.Messages.[0].Message))
    Assert.Equal<int>(1, updatedGeneric.Messages.[0].Extensions.Count)
    Assert.Equal<int>(1, updatedNonGeneric.Messages.[0].Extensions.Count)

[<Fact>]
let WithExtension_NullKey_ThrowsArgumentNullExceptionForKey_Test () =
    let generic = MinimalResult<int>.Failure("E1", "Boom")
    let nonGeneric = MinimalResult.Failure("E1", "Boom")
    let nullKey = Unchecked.defaultof<string>

    let genericEx =
        Assert.Throws<ArgumentNullException>(Action(fun () -> generic.WithExtension(nullKey, box 1) |> ignore))
    let nonGenericEx =
        Assert.Throws<ArgumentNullException>(Action(fun () -> nonGeneric.WithExtension(nullKey, box 1) |> ignore))

    Assert.Equal<string>("key", genericEx.ParamName)
    Assert.Equal<string>("key", nonGenericEx.ParamName)

[<Fact>]
let WithExtension_NullResult_ThrowsArgumentNullExceptionForResult_Test () =
    let genericEx =
        Assert.Throws<ArgumentNullException>(Action(fun () ->
            MinimalResultExtensions.WithExtension((Unchecked.defaultof<MinimalResult<int>>), "k", box 1) |> ignore))
    let nonGenericEx =
        Assert.Throws<ArgumentNullException>(Action(fun () ->
            MinimalResultExtensions.WithExtension((Unchecked.defaultof<MinimalResult>), "k", box 1) |> ignore))

    Assert.Equal<string>("result", genericEx.ParamName)
    Assert.Equal<string>("result", nonGenericEx.ParamName)

[<Fact>]
let WithExtension_NullLastMessage_ThrowsInvalidOperationException_Test () =
    let generic = MinimalResult<int>.Failure([| Unchecked.defaultof<ResultMessage> |] :> IReadOnlyList<ResultMessage>)
    let nonGeneric = MinimalResult.Failure([| Unchecked.defaultof<ResultMessage> |] :> IReadOnlyList<ResultMessage>)

    Assert.Throws<InvalidOperationException>(Action(fun () -> generic.WithExtension("k", box 1) |> ignore)) |> ignore
    Assert.Throws<InvalidOperationException>(Action(fun () -> nonGeneric.WithExtension("k", box 1) |> ignore)) |> ignore

[<Fact>]
let WithExtension_OnNonGenericResultTypedAsBase_KeepsConcreteType_Test () =
    let r : MinimalResult<obj> = MinimalResult.Failure("a", "b") :> MinimalResult<obj>

    let updated = r.WithExtension("k", box 1)

    Assert.Equal<Type>(typeof<MinimalResult>, updated.GetType())
    Assert.True(updated.IsFailure)
    let last = updated.Messages.[updated.Messages.Count - 1]
    Assert.Equal<string>("a", last.Code)
    Assert.Equal<string>("b", last.Message)
    Assert.Equal<int>(1, unbox<int> last.Extensions.["k"])

[<Fact>]
let WithExtension_Messages_IsNotICollection_Test () =
    let updatedResults : IReadOnlyList<ResultMessage> list =
        [ MinimalResult<int>.Failure("E1", "Boom").WithExtension("k", box 1).Messages
          MinimalResult<int>.Success(1).WithExtension("k", box 1).Messages
          MinimalResult<int>.Failure(asReadOnlyList [ mkMessage "E1" "One"; mkMessage "E2" "Two" ]).WithExtension("k", box 1).Messages
          MinimalResult.Failure("E1", "Boom").WithExtension("k", box 1).Messages
          MinimalResult.Success().WithExtension("k", box 1).Messages
          MinimalResult.Failure(asReadOnlyList [ mkMessage "E1" "One"; mkMessage "E2" "Two" ]).WithExtension("k", box 1).Messages ]

    for messages in updatedResults do
        Assert.False(((box messages) :? ICollection<ResultMessage>), "WithExtension Messages is an ICollection<ResultMessage>")

[<Fact>]
let WithExtension_OnFailureFromCallerArray_LeavesArrayAndOriginalUnchanged_Test () =
    let arr = [| mkMessage "E1" "One"; mkMessage "E2" "Two" |]
    let lastBefore = arr.[arr.Length - 1]
    let generic = MinimalResult<int>.Failure(arr :> IReadOnlyList<ResultMessage>)
    let nonGeneric = MinimalResult.Failure(arr :> IReadOnlyList<ResultMessage>)

    let updatedGeneric = generic.WithExtension("k", box 1)
    let updatedNonGeneric = nonGeneric.WithExtension("k", box 1)

    Assert.Same(box lastBefore, box arr.[arr.Length - 1])
    Assert.True(isNull (box arr.[arr.Length - 1].Extensions))
    Assert.Same(box arr, box generic.Messages)
    Assert.Same(box arr, box nonGeneric.Messages)
    Assert.NotSame(box arr, box updatedGeneric.Messages)
    Assert.NotSame(box arr, box updatedNonGeneric.Messages)
    Assert.NotSame(box lastBefore, box updatedGeneric.Messages.[1])
    Assert.NotSame(box lastBefore, box updatedNonGeneric.Messages.[1])
    Assert.Same(box arr.[0], box updatedGeneric.Messages.[0])
    Assert.Same(box arr.[0], box updatedNonGeneric.Messages.[0])

[<Fact>]
let WithExtension_CallerListMutatedAfterwards_DoesNotAffectUpdated_Test () =
    let callerList = ResizeArray<ResultMessage>([ mkMessage "E1" "One"; mkMessage "E2" "Two" ])
    let generic = MinimalResult<int>.Failure(callerList :> IReadOnlyList<ResultMessage>)
    let nonGeneric = MinimalResult.Failure(callerList :> IReadOnlyList<ResultMessage>)
    let updatedGeneric = generic.WithExtension("k", box 1)
    let updatedNonGeneric = nonGeneric.WithExtension("k", box 1)

    callerList.Add(mkMessage "E3" "Three")

    Assert.Equal<int>(3, generic.Messages.Count)
    Assert.Equal<int>(3, nonGeneric.Messages.Count)
    Assert.Equal<int>(2, updatedGeneric.Messages.Count)
    Assert.Equal<int>(2, updatedNonGeneric.Messages.Count)

    callerList.Clear()

    Assert.Equal<int>(0, generic.Messages.Count)
    Assert.Equal<int>(0, nonGeneric.Messages.Count)
    Assert.Equal<int>(2, updatedGeneric.Messages.Count)
    Assert.Equal<int>(2, updatedNonGeneric.Messages.Count)
    Assert.Equal<string>("E2", updatedGeneric.Messages.[1].Code)
    Assert.Equal<int>(1, unbox<int> updatedNonGeneric.Messages.[1].Extensions.["k"])

[<Fact>]
let WithExtension_NullValue_IsStoredUnderKey_Test () =
    let r = MinimalResult<int>.Failure("E1", "Boom").WithExtension("none", Unchecked.defaultof<obj>)

    let ext = r.Messages.[0].Extensions
    Assert.Equal<int>(1, ext.Count)
    Assert.True(ext.ContainsKey("none"))
    Assert.True(isNull (box ext.["none"]))

[<Fact>]
let WithExtension_LastMessageWithNullCode_KeepsNullCodeAndMessage_Test () =
    let fromNulls = MinimalResult<int>.Failure(null, null).WithExtension("k", box 1)
    let fromNullsNonGeneric = MinimalResult.Failure(null, null).WithExtension("k", box 1)
    let fromCodeOnly = MinimalResult<int>.Failure(asReadOnlyList [ new ResultMessage("x") ]).WithExtension("k", box 1)
    let fromCodeOnlyNonGeneric = MinimalResult.Failure(asReadOnlyList [ new ResultMessage("x") ]).WithExtension("k", box 1)

    Assert.True(isNull (box fromNulls.Messages.[0].Code))
    Assert.True(isNull (box fromNulls.Messages.[0].Message))
    Assert.True(fromNulls.Messages.[0].Extensions.ContainsKey("k"))
    Assert.True(isNull (box fromNullsNonGeneric.Messages.[0].Code))
    Assert.True(isNull (box fromNullsNonGeneric.Messages.[0].Message))
    Assert.True(fromNullsNonGeneric.Messages.[0].Extensions.ContainsKey("k"))
    Assert.Equal<string>("x", fromCodeOnly.Messages.[0].Code)
    Assert.True(isNull (box fromCodeOnly.Messages.[0].Message))
    Assert.True(fromCodeOnly.Messages.[0].Extensions.ContainsKey("k"))
    Assert.Equal<string>("x", fromCodeOnlyNonGeneric.Messages.[0].Code)
    Assert.True(isNull (box fromCodeOnlyNonGeneric.Messages.[0].Message))
    Assert.True(fromCodeOnlyNonGeneric.Messages.[0].Extensions.ContainsKey("k"))

[<Fact>]
let WithExtension_OnSuccess_PreservesReferenceAndStructResponse_Test () =
    let text = String('s', 3)
    let id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301")
    let point = { X = 3; Y = -4 }

    let stringResult = MinimalResult<string>.Success(text).WithExtension("k", box "extension-value")
    let guidResult = MinimalResult<Guid>.Success(id).WithExtension("k", box (Guid.NewGuid()))
    let pointResult = MinimalResult<Point>.Success(point).WithExtension("k", box { X = 100; Y = 200 })
    let nullStringResult = MinimalResult<string>.Success(null).WithExtension("k", box "extension-value")

    Assert.Same(box text, box stringResult.Response)
    Assert.Equal<Guid>(id, guidResult.Response)
    Assert.True((pointResult.Response = point))
    Assert.True(isNull (box nullStringResult.Response))
    Assert.True(stringResult.IsSuccess)
    Assert.True(guidResult.IsSuccess)
    Assert.True(pointResult.IsSuccess)
    Assert.True(nullStringResult.IsSuccess)

[<Fact>]
let WithExtension_OnFailure_KeepsDefaultResponseForGuidAndString_Test () =
    let guidResult = MinimalResult<Guid>.Failure("E1", "Boom").WithExtension("k", box (Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301")))
    let stringResult = MinimalResult<string>.Failure("E1", "Boom").WithExtension("k", box "extension-value")

    Assert.True(guidResult.IsFailure)
    Assert.Equal<Guid>(Guid.Empty, guidResult.Response)
    Assert.True(stringResult.IsFailure)
    Assert.True(isNull (box stringResult.Response))

[<Fact>]
let FailureWithExtensions_SnapshotKeys_AreCaseInsensitive_Test () =
    let input = Dictionary<string, obj>()
    input.["Key"] <- box 1

    let generic = MinimalResult<int>.Failure("E1", "Boom", input :> IReadOnlyDictionary<string, obj>)
    let nonGeneric = MinimalResult.Failure("E1", "Boom", input :> IReadOnlyDictionary<string, obj>)

    Assert.False(input.ContainsKey("key"))
    for ext in [ generic.Messages.[0].Extensions; nonGeneric.Messages.[0].Extensions ] do
        Assert.True(ext.ContainsKey("Key"))
        Assert.True(ext.ContainsKey("key"))
        Assert.True(ext.ContainsKey("KEY"))
        Assert.Equal<int>(1, unbox<int> ext.["kEy"])

[<Fact>]
let FailureWithExtensions_KeysDifferingOnlyByCase_AreMergedLastWins_Test () =
    let input = Dictionary<string, obj>()
    input.["a"] <- box 1
    input.["A"] <- box 2

    let generic = MinimalResult<int>.Failure("E1", "Boom", input :> IReadOnlyDictionary<string, obj>)
    let nonGeneric = MinimalResult.Failure("E1", "Boom", input :> IReadOnlyDictionary<string, obj>)

    Assert.Equal<int>(2, input.Count)
    for ext in [ generic.Messages.[0].Extensions; nonGeneric.Messages.[0].Extensions ] do
        Assert.Equal<int>(1, ext.Count)
        Assert.Equal<int>(2, unbox<int> ext.["a"])

[<Fact>]
let FailureWithExtensions_ReferenceAndNullValues_AreKeptAsIs_Test () =
    let payload = Payload("p")
    let boxedPayload = box payload
    let input = Dictionary<string, obj>()
    input.["payload"] <- boxedPayload
    input.["none"] <- Unchecked.defaultof<obj>

    let generic = MinimalResult<int>.Failure("E1", "Boom", input :> IReadOnlyDictionary<string, obj>)
    let nonGeneric = MinimalResult.Failure("E1", "Boom", input :> IReadOnlyDictionary<string, obj>)

    for ext in [ generic.Messages.[0].Extensions; nonGeneric.Messages.[0].Extensions ] do
        Assert.Equal<int>(2, ext.Count)
        Assert.Same(boxedPayload, ext.["payload"])
        Assert.True(ext.ContainsKey("none"))
        Assert.True(isNull (box ext.["none"]))
