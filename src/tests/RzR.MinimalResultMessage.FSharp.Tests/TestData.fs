module RzR.MinimalResultMessage.FSharp.Tests.TestData

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

type Payload(name: string) =
    member _.Name = name

[<Struct>]
type Point = { X: int; Y: int }

let mkMessage (code: string) (message: string) : ResultMessage =
    new ResultMessage(code, message)

let mkMessageWith (code: string) (message: string) (extensions: IReadOnlyDictionary<string, obj>) : ResultMessage =
    new ResultMessage(code, message, extensions)

let extensionsWith (key: string) (value: obj) : Dictionary<string, obj> =
    let d = Dictionary<string, obj>()
    d.[key] <- value
    d

let asExtensions (d: Dictionary<string, obj>) : IReadOnlyDictionary<string, obj> =
    d :> IReadOnlyDictionary<string, obj>

let asReadOnlyList (items: ResultMessage list) : IReadOnlyList<ResultMessage> =
    items :> IReadOnlyList<ResultMessage>

let nullMessages : IReadOnlyList<ResultMessage> = null

let buildCollection (kind: string) (items: ResultMessage list) : IReadOnlyList<ResultMessage> =
    match kind with
    | "fsharp-list" -> asReadOnlyList items
    | "array" -> (Array.ofList items) :> IReadOnlyList<ResultMessage>
    | "generic-list" -> ResizeArray<ResultMessage>(items) :> IReadOnlyList<ResultMessage>
    | "readonly-collection" ->
        ReadOnlyCollection<ResultMessage>(ResizeArray<ResultMessage>(items)) :> IReadOnlyList<ResultMessage>
    | other -> invalidArg "kind" (sprintf "Unknown collection kind '%s'." other)

[<Xunit.CollectionDefinition("SharedEmptyExtensions", DisableParallelization = true)>]
type SharedEmptyExtensionsCollection() = class end

let failureListMessages () : IReadOnlyList<ResultMessage> =
    asReadOnlyList [ mkMessage "E1" "First"; mkMessage "E2" "Second" ]

let emptyMessages () : IReadOnlyList<ResultMessage> =
    asReadOnlyList []

let stringScenario (key: string) : MinimalResult<string> =
    match key with
    | "success" -> MinimalResult<string>.Success("ok")
    | "success-null" -> MinimalResult<string>.Success(null)
    | "failure-code" -> MinimalResult<string>.Failure("E1", "Boom")
    | "failure-list" -> MinimalResult<string>.Failure(failureListMessages ())
    | "failure-empty" -> MinimalResult<string>.Failure(emptyMessages ())
    | "failure-null" -> MinimalResult<string>.Failure(nullMessages)
    | other -> invalidArg "key" (sprintf "Unknown string scenario key '%s'." other)

let intScenario (key: string) : MinimalResult<int> =
    match key with
    | "success" -> MinimalResult<int>.Success(42)
    | "failure-code" -> MinimalResult<int>.Failure("E1", "Boom")
    | "failure-list" -> MinimalResult<int>.Failure(failureListMessages ())
    | "failure-empty" -> MinimalResult<int>.Failure(emptyMessages ())
    | "failure-null" -> MinimalResult<int>.Failure(nullMessages)
    | other -> invalidArg "key" (sprintf "Unknown int scenario key '%s'." other)

let expectedIsSuccess (key: string) : bool =
    key = "success" || key = "success-null"

let expectedStringResponse (key: string) : string =
    if key = "success" then "ok" else null

let expectedIntResponse (key: string) : int =
    if key = "success" then 42 else 0

let expectedMessageCount (key: string) : int option =
    match key with
    | "success" | "success-null" | "failure-empty" -> Some 0
    | "failure-code" -> Some 1
    | "failure-list" -> Some 2
    | "failure-null" -> None
    | other -> invalidArg "key" (sprintf "Unknown scenario key '%s'." other)
