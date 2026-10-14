module RzR.MinimalResultMessage.FSharp.Tests.MinimalResultTests

open System
open System.Collections.Generic
open Xunit
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.FSharp.Tests.TestData

[<Fact>]
let Success_WithString_KeepsSameReference_Test () =
    let s = System.String('o', 3)

    let r = MinimalResult<string>.Success(s)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Same(box s, box r.Response)

[<Fact>]
let Success_WithClassInstance_KeepsSameReference_Test () =
    let payload = Payload("p")

    let r = MinimalResult<Payload>.Success(payload)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Same(box payload, box r.Response)

[<Fact>]
let Success_WithInt42_Returns42_Test () =
    let r = MinimalResult<int>.Success(42)

    Assert.True(r.IsSuccess)
    Assert.Equal<int>(42, r.Response)

[<Fact>]
let Success_WithGuid_ReturnsGuid_Test () =
    let g = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301")

    let r = MinimalResult<Guid>.Success(g)

    Assert.True(r.IsSuccess)
    Assert.Equal<Guid>(g, r.Response)

[<Fact>]
let Success_WithDateTime_ReturnsDateTime_Test () =
    let dt = DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)

    let r = MinimalResult<DateTime>.Success(dt)

    Assert.True(r.IsSuccess)
    Assert.Equal<DateTime>(dt, r.Response)

[<Fact>]
let Success_WithCustomStruct_ReturnsEqualStruct_Test () =
    let p = { X = 3; Y = -4 }

    let r = MinimalResult<Point>.Success(p)

    Assert.True(r.IsSuccess)
    Assert.True((r.Response = p))
    Assert.Equal<int>(3, r.Response.X)
    Assert.Equal<int>(-4, r.Response.Y)

[<Fact>]
let Success_WithIntZero_ReturnsZero_Test () =
    let r = MinimalResult<int>.Success(0)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Equal<int>(0, r.Response)

[<Fact>]
let Success_WithGuidEmpty_ReturnsGuidEmpty_Test () =
    let r = MinimalResult<Guid>.Success(Guid.Empty)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Equal<Guid>(Guid.Empty, r.Response)

[<Fact>]
let Success_Messages_IsNonNullAndEmpty_Test () =
    let rInt = MinimalResult<int>.Success(42)
    let rString = MinimalResult<string>.Success("ok")

    Assert.NotNull(box rInt.Messages)
    Assert.Equal<int>(0, rInt.Messages.Count)
    Assert.True(Seq.isEmpty rInt.Messages)
    Assert.NotNull(box rString.Messages)
    Assert.Equal<int>(0, rString.Messages.Count)
    Assert.True(Seq.isEmpty rString.Messages)

[<Fact>]
let Success_WithNullString_HasNullResponseAndEmptyMessages_Test () =
    let r = MinimalResult<string>.Success(null)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Null(box r.Response)
    Assert.NotNull(box r.Messages)
    Assert.Equal<int>(0, r.Messages.Count)

[<Fact>]
let Success_WithNoneOption_ReturnsNone_Test () =
    let r = MinimalResult<int option>.Success(None)

    Assert.True(r.IsSuccess)
    Assert.False(r.IsFailure)
    Assert.Equal<int option>(None, r.Response)

[<Fact>]
let Success_Messages_IsNotICollection_Test () =
    let r = MinimalResult<int>.Success(1)

    Assert.False(((box r.Messages) :? ICollection<ResultMessage>))

[<Fact>]
let FailureWithCodeAndMessage_Messages_IsNotICollection_Test () =
    let r = MinimalResult<int>.Failure("E1", "Boom")

    Assert.False(((box r.Messages) :? ICollection<ResultMessage>))

[<Fact>]
let Success_TwoResults_ShareEmptyMessagesInstance_Test () =
    let a = MinimalResult<int>.Success(1)
    let b = MinimalResult<int>.Success(2)

    Assert.Same(box a.Messages, box b.Messages)

[<Fact>]
let FailureWithCodeAndMessage_IsFailure_CarriesCodeAndMessage_Test () =
    let r = MinimalResult<int>.Failure("E1", "Boom")

    Assert.False(r.IsSuccess)
    Assert.True(r.IsFailure)
    Assert.Equal<string>("E1", r.Messages.[0].Code)
    Assert.Equal<string>("Boom", r.Messages.[0].Message)

[<Fact>]
let Failure_Response_IsDefaultForIntStringAndGuid_Test () =
    let rInt = MinimalResult<int>.Failure("E1", "Boom")
    let rString = MinimalResult<string>.Failure("E1", "Boom")
    let rGuid = MinimalResult<Guid>.Failure("E1", "Boom")

    Assert.Equal<int>(0, rInt.Response)
    Assert.Null(box rString.Response)
    Assert.Equal<Guid>(Guid.Empty, rGuid.Response)

[<Fact>]
let Failure_WithIntOption_ResponseIsNone_Test () =
    let r = MinimalResult<int option>.Failure("E1", "Boom")

    Assert.True(r.IsFailure)
    Assert.Equal<int option>(None, r.Response)

[<Theory>]
[<InlineData("E1", "Boom")>]
[<InlineData("  E2  ", "  padded  ")>]
let FailureWithCodeAndMessage_CarriesExactlyOneVerbatimMessage_Test (code: string, message: string) =
    let r = MinimalResult<int>.Failure(code, message)

    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>(code, r.Messages.[0].Code)
    Assert.Equal<string>(message, r.Messages.[0].Message)

[<Fact>]
let Failure_MessageExtensions_IsNonNullEmptyDictionary_Test () =
    let ext = MinimalResult<int>.Failure("E1", "Boom").Messages.[0].Extensions

    Assert.NotNull(box ext)
    Assert.Equal<int>(0, ext.Count)
    Assert.False(ext.ContainsKey("x"))
    let found, _ = ext.TryGetValue("x")
    Assert.False(found)
    Assert.True(Seq.isEmpty ext)

[<Collection("SharedEmptyExtensions")>]
module SharedExtensionsMutation =

    [<Fact>]
    let Failure_MessageExtensions_CannotBeMutatedThroughAnyCast_Test () =
        let ext = MinimalResult<int>.Failure("E1", "Boom").Messages.[0].Extensions

        Assert.False(((box ext) :? Dictionary<string, obj>), "Extensions is a mutable Dictionary")
        Assert.Equal<int>(0, ext.Count)
        Assert.True(((box ext) :? IDictionary<string, obj>), "Extensions does not implement IDictionary<string, obj>")
        Assert.True(((box ext) :? System.Collections.IDictionary), "Extensions does not implement IDictionary")

        let d = (box ext) :?> IDictionary<string, obj>
        Assert.Throws<NotSupportedException>(Action(fun () -> d.Add("k", box 1))) |> ignore
        Assert.Throws<NotSupportedException>(Action(fun () -> d.["k"] <- box 1)) |> ignore
        Assert.Throws<NotSupportedException>(Action(fun () -> d.Remove("k") |> ignore)) |> ignore
        Assert.Throws<NotSupportedException>(Action(fun () -> d.Clear())) |> ignore

        let nd = (box ext) :?> System.Collections.IDictionary
        Assert.Throws<NotSupportedException>(Action(fun () -> nd.Add(box "k", box 1))) |> ignore
        Assert.Throws<NotSupportedException>(Action(fun () -> nd.[box "k"] <- box 1)) |> ignore

        Assert.Equal<int>(0, ext.Count)
        Assert.Equal<int>(0, MinimalResult<int>.Failure("E2", "Other").Messages.[0].Extensions.Count)

[<Fact>]
let Failure_NullCodeAndMessage_KeepsNulls_Test () =
    let r = MinimalResult<int>.Failure(null, null)

    Assert.True(r.IsFailure)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Null(box r.Messages.[0].Code)
    Assert.Null(box r.Messages.[0].Message)

[<Fact>]
let Failure_EmptyCodeAndMessage_KeepsEmptyStrings_Test () =
    let r = MinimalResult<int>.Failure("", "")

    Assert.True(r.IsFailure)
    Assert.Equal<int>(1, r.Messages.Count)
    Assert.Equal<string>("", r.Messages.[0].Code)
    Assert.Equal<string>("", r.Messages.[0].Message)

[<Fact>]
let Failure_IdenticalResults_ShareExtensionsButAreNotEqual_Test () =
    let r1 = MinimalResult<int>.Failure("E1", "Boom")
    let r2 = MinimalResult<int>.Failure("E1", "Boom")

    Assert.Equal<string>("E1", r1.Messages.[0].Code)
    Assert.Equal<string>("Boom", r1.Messages.[0].Message)
    Assert.Same(box r1.Messages.[0].Extensions, box r2.Messages.[0].Extensions)
    Assert.False(r1.Equals(box r2), "results use reference equality")
    Assert.False((r1 = r2), "results use reference equality")
    Assert.NotSame(box r1, box r2)

[<Fact>]
let Failure_ExtensionsInstance_IsSharedAcrossDifferentT_Test () =
    let rInt = MinimalResult<int>.Failure("E1", "Boom")
    let rString = MinimalResult<string>.Failure("E1", "Boom")

    Assert.Same(box rInt.Messages.[0].Extensions, box rString.Messages.[0].Extensions)

[<Fact>]
let FailureWithMessages_IsFailure_HasDefaultResponse_Test () =
    let messages = asReadOnlyList [ mkMessage "E1" "One"; mkMessage "E2" "Two" ]

    let rInt = MinimalResult<int>.Failure(messages)
    let rString = MinimalResult<string>.Failure(messages)

    Assert.False(rInt.IsSuccess)
    Assert.True(rInt.IsFailure)
    Assert.Equal<int>(0, rInt.Response)
    Assert.False(rString.IsSuccess)
    Assert.True(rString.IsFailure)
    Assert.Null(box rString.Response)

[<Fact>]
let FailureWithMessages_HundredWithDuplicates_KeepsAllInOrder_Test () =
    let pool = [| for i in 0 .. 6 -> mkMessage (sprintf "E%d" i) (sprintf "M%d" i) |]
    let items = Array.init 100 (fun i -> pool.[i % pool.Length])

    let r = MinimalResult<int>.Failure(items :> IReadOnlyList<ResultMessage>)

    Assert.Equal<int>(100, r.Messages.Count)
    for i in 0 .. 99 do
        Assert.Same(box items.[i], box r.Messages.[i])

[<Fact>]
let FailureWithMessages_MutableList_IsAliased_Test () =
    let callerList = ResizeArray<ResultMessage>([ mkMessage "E1" "One" ])
    let r = MinimalResult<int>.Failure(callerList :> IReadOnlyList<ResultMessage>)
    let extra = mkMessage "E2" "Two"

    Assert.Same(box callerList, box r.Messages)

    callerList.Add(extra)
    Assert.Equal<int>(2, r.Messages.Count)
    Assert.Same(box extra, box r.Messages.[1])

    callerList.Clear()
    Assert.Equal<int>(0, r.Messages.Count)

[<Fact>]
let FailureWithMessages_EmptyList_HasEmptyMessages_Test () =
    let r = MinimalResult<int>.Failure(asReadOnlyList [])

    Assert.True(r.IsFailure)
    Assert.NotNull(box r.Messages)
    Assert.Equal<int>(0, r.Messages.Count)

[<Fact>]
let FailureWithMessages_NullList_HasNullMessagesAndDefaultResponse_Test () =
    let r = MinimalResult<int>.Failure(nullMessages)

    Assert.True(r.IsFailure)
    Assert.False(r.IsSuccess)
    Assert.Null(box r.Messages)
    Assert.Equal<int>(0, r.Response)

[<Fact>]
let FailureWithMessages_NullElementAndNullExtensions_ArePassedThrough_Test () =
    let noExtensions = mkMessage "E1" "Boom"
    let items = [| noExtensions; Unchecked.defaultof<ResultMessage> |]

    let r = MinimalResult<int>.Failure(items :> IReadOnlyList<ResultMessage>)

    Assert.True(r.IsFailure)
    Assert.Equal<int>(2, r.Messages.Count)
    Assert.Same(box noExtensions, box r.Messages.[0])
    Assert.Null(box r.Messages.[0].Extensions)
    Assert.Null(box r.Messages.[1])

[<Theory>]
[<InlineData("fsharp-list")>]
[<InlineData("array")>]
[<InlineData("generic-list")>]
[<InlineData("readonly-collection")>]
let FailureWithMessages_EachCollectionKind_KeepsCountAndElementReferences_Test (kind: string) =
    let items = [ mkMessage "E1" "One"; mkMessage "E2" "Two"; mkMessage "E3" "Three" ]
    let input = buildCollection kind items

    let r = MinimalResult<int>.Failure(input)

    Assert.True(r.IsFailure)
    Assert.Equal<int>(3, r.Messages.Count)
    items |> List.iteri (fun i m -> Assert.Same(box m, box r.Messages.[i]))

[<Theory>]
[<InlineData("fsharp-list")>]
[<InlineData("array")>]
[<InlineData("generic-list")>]
[<InlineData("readonly-collection")>]
let FailureWithMessages_EachCollectionKind_KeepsInputReference_Test (kind: string) =
    let input = buildCollection kind [ mkMessage "E1" "One"; mkMessage "E2" "Two" ]

    let r = MinimalResult<int>.Failure(input)

    Assert.Same(box input, box r.Messages)

[<Fact>]
let Results_UseReferenceEquality_EqualOnlyToThemselves_Test () =
    let a = MinimalResult<int>.Success(1)
    let b = MinimalResult<int>.Success(1)

    Assert.True(a.Equals(box a), "reflexivity: a.Equals(a) expected true")
    Assert.True((a = a))
    Assert.False(a.Equals(box b), "distinct instances with the same content are not Equal")
    Assert.False((a = b))
    Assert.False(a.Equals((null: obj)), "Equals(null) expected false")

[<Fact>]
let Success_IntVsInt64SameValue_AreNotEqual_Test () =
    let a = MinimalResult<int>.Success(1)
    let b = MinimalResult<int64>.Success(1L)

    Assert.False(a.Equals(box b))

[<Fact>]
let Success_DifferentValues_AreNotEqual_Test () =
    let a = MinimalResult<int>.Success(1)
    let b = MinimalResult<int>.Success(2)

    Assert.False(a.Equals(box b))
    Assert.False(b.Equals(box a))
    Assert.False((a = b))

[<Fact>]
let Success_VsFailure_AreNotEqual_Test () =
    let s = MinimalResult<string>.Success("x")
    let f = MinimalResult<string>.Failure("E1", "Boom")

    Assert.False(s.Equals(box f))
    Assert.False(f.Equals(box s))
    Assert.False((s = f))

[<Fact>]
let SuccessZero_VsFailureWithMatchingResponse_AreNotEqual_Test () =
    let s = MinimalResult<int>.Success(0)
    let f = MinimalResult<int>.Failure("E1", "Boom")

    Assert.Equal<int>(s.Response, f.Response)
    Assert.False(s.Equals(box f))
    Assert.False(f.Equals(box s))
    Assert.False((s = f))

[<Fact>]
let Success_TwoNullStrings_AreNotEqual_Test () =
    let a = MinimalResult<string>.Success(null)
    let b = MinimalResult<string>.Success(null)

    Assert.False(a.Equals(box b))
    Assert.False((a = b))

[<Fact>]
let FailureWithMessages_SameListInstance_AreNotEqual_Test () =
    let shared = ResizeArray<ResultMessage>([ mkMessage "E1" "One"; mkMessage "E2" "Two" ]) :> IReadOnlyList<ResultMessage>

    let r1 = MinimalResult<int>.Failure(shared)
    let r2 = MinimalResult<int>.Failure(shared)

    Assert.Same(box r1.Messages, box r2.Messages)
    Assert.False(r1.Equals(box r2))
    Assert.False((r1 = r2))
