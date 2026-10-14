# Minimal result message - USING

This guide shows how to return and read results with `RzR.MinimalResultMessage` from C# and F#. The library targets `netstandard2.0` and `netstandard2.1`. It has two result types, one message type and two interfaces:

- `MinimalResult<T>` - an operation that returns a value of type `T`.
- `MinimalResult` - an operation with no return value.
- `ResultMessage` - one message with a code, optional text and optional additional data.
- `IMinimalResult` and `IMinimalResult<T>` - read-only views of a result.

Results cannot be created with `new`. Use the static `Success` and `Failure` methods or the implicit conversions.

## Namespaces and setup

Reference the `RzR.MinimalResultMessage` package or project, then import the three namespaces. The C# examples below assume these `using` directives plus `System` and `System.Collections.Generic` (implicit usings).

```csharp
using RzR.MinimalResultMessage;
using RzR.MinimalResultMessage.Abstractions;
using RzR.MinimalResultMessage.Models;
```

```fsharp
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Abstractions
open RzR.MinimalResultMessage.Models
```

`RzR.MinimalResultMessage` holds the result types, `Models` holds `ResultMessage`, and `Abstractions` holds the interfaces.

## MinimalResult&lt;T&gt;

Use `MinimalResult<T>` when the operation produces a value.

- `Success(value)` - `IsSuccess` is true, `Response` holds the value, `Messages` is empty. A later `WithExtension` call returns a success with one message.
- `Failure(code, message)` - `IsFailure` is true, `Messages` holds one message.
- `Failure(code, message, extensions)` - like `Failure(code, message)`, and the message carries a read-only copy of `extensions`. See [Extensions](#extensions-attaching-additional-data-to-a-failure).
- `Failure(IReadOnlyList<ResultMessage>)` - `IsFailure` is true, `Messages` is the list you passed.

On a failure, `Response` is `default(T)`: `null` for reference types, `0` for `int`, `Guid.Empty` for `Guid`. Check `IsSuccess` before you read `Response`.

```csharp
static MinimalResult<string> FindUserName(Dictionary<int, string> users, int id)
{
    if (users.TryGetValue(id, out var name))
    {
        return MinimalResult<string>.Success(name);
    }

    return MinimalResult<string>.Failure("UserNotFound", $"No user with id {id}.");
}

var users = new Dictionary<int, string> { [1] = "Ana" };
var found = FindUserName(users, 1);
var missing = FindUserName(users, 2);

Console.WriteLine($"{found.IsSuccess} {found.Response}");
Console.WriteLine($"{missing.IsFailure} {missing.Response == null} {missing.Messages[0].Code}");

var errors = new List<ResultMessage>
{
    new ResultMessage("NameRequired", "Name is required."),
    new ResultMessage("EmailInvalid", "Email is not valid.")
};
var validation = MinimalResult<int>.Failure(errors);

Console.WriteLine($"{validation.IsFailure} {validation.Response} {validation.Messages.Count}");
```

The output is `True Ana`, then `True True UserNotFound`, then `True 0 2`.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

let findUserName (users: IDictionary<int, string>) (id: int) : MinimalResult<string> =
    match users.TryGetValue id with
    | true, name -> MinimalResult<string>.Success name
    | _ -> MinimalResult<string>.Failure("UserNotFound", $"No user with id {id}.")

let users = dict [ 1, "Ana" ]
let found = findUserName users 1
let missing = findUserName users 2

printfn "%b %s" found.IsSuccess found.Response
printfn "%b %b %s" missing.IsFailure (isNull (box missing.Response)) missing.Messages.[0].Code

let errors =
    [| ResultMessage("NameRequired", "Name is required.")
       ResultMessage("EmailInvalid", "Email is not valid.") |] :> IReadOnlyList<ResultMessage>
let validation = MinimalResult<int>.Failure errors

printfn "%b %d %d" validation.IsFailure validation.Response validation.Messages.Count
```

The output is `true Ana`, then `true true UserNotFound`, then `true 0 2`.

## MinimalResult (no return value)

Use the non-generic `MinimalResult` for commands such as save, delete or send. It has the same factories without a value: `Success()`, `Failure(code, message)`, `Failure(code, message, extensions)` and `Failure(messages)`.

`MinimalResult` inherits `MinimalResult<object>`, so it also implements `IMinimalResult<object>` and `IMinimalResult`. `Response` is always `null`.

```csharp
static MinimalResult Deactivate(Dictionary<int, bool> accounts, int id)
{
    if (!accounts.ContainsKey(id))
    {
        return MinimalResult.Failure("AccountNotFound", $"No account with id {id}.");
    }

    accounts[id] = false;
    return MinimalResult.Success();
}

var accounts = new Dictionary<int, bool> { [7] = true };
var ok = Deactivate(accounts, 7);
var failed = Deactivate(accounts, 8);

Console.WriteLine($"{ok.IsSuccess} {ok.Response == null} {ok.Messages.Count}");
Console.WriteLine($"{failed.IsFailure} {failed.Response == null} {failed.Messages[0].Code}");
```

The output is `True True 0`, then `True True AccountNotFound`.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage

let deactivate (accounts: Dictionary<int, bool>) (id: int) : MinimalResult =
    if accounts.ContainsKey id then
        accounts.[id] <- false
        MinimalResult.Success()
    else
        MinimalResult.Failure("AccountNotFound", $"No account with id {id}.")

let accounts = Dictionary<int, bool>()
accounts.[7] <- true

let ok = deactivate accounts 7
let failed = deactivate accounts 8

printfn "%b %b %d" ok.IsSuccess (isNull ok.Response) ok.Messages.Count
printfn "%b %s" failed.IsFailure failed.Messages.[0].Code
```

The output is `true true 0`, then `true AccountNotFound`.

## ResultMessage

A `ResultMessage` is immutable. It has four constructors:

| Constructor | Code | Message | Extensions |
|---|---|---|---|
| `new ResultMessage()` | `""` | `null` | `null` |
| `new ResultMessage(code)` | `code` | `null` | `null` |
| `new ResultMessage(code, message)` | `code` | `message` | `null` |
| `new ResultMessage(code, message, extensions)` | `code` | `message` | `extensions` |

Use `Extensions` (`IReadOnlyDictionary<string, object>`) for additional data such as a field name or a retry delay. The `ResultMessage` constructor stores the dictionary you pass as-is. The `Failure(code, message, extensions)` factory stores a copy instead.

`Failure(code, message)` builds its message with a shared, empty, read-only dictionary as `Extensions`, so `Extensions` is never `null` there. Do not cast it to `IDictionary` and modify it: it is a `ReadOnlyDictionary` and throws `NotSupportedException`. To attach data to a single-message failure, use `Failure(code, message, extensions)` or `WithExtension` (see [Extensions](#extensions-attaching-additional-data-to-a-failure)). To attach data to several messages, build the messages yourself and use `Failure(messages)`.

```csharp
var empty = new ResultMessage();
var codeOnly = new ResultMessage("Timeout");
var withText = new ResultMessage("Timeout", "The payment service did not respond.");
var withData = new ResultMessage(
    "Timeout",
    "The payment service did not respond.",
    new Dictionary<string, object> { ["service"] = "payments", ["elapsedMs"] = 5000 });

Console.WriteLine(empty.Code == "" && empty.Message == null && empty.Extensions == null);
Console.WriteLine(codeOnly.Message == null && codeOnly.Extensions == null);
Console.WriteLine(withText.Extensions == null);

var payment = MinimalResult<decimal>.Failure(new List<ResultMessage> { withData });
Console.WriteLine(payment.Messages[0].Extensions!["service"]);

var simple = MinimalResult<decimal>.Failure("Timeout", "The payment service did not respond.");
Console.WriteLine(simple.Messages[0].Extensions!.Count);
```

The output is `True`, `True`, `True`, `payments`, then `0`.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

let empty = ResultMessage()
let codeOnly = ResultMessage("Timeout")
let withData =
    ResultMessage(
        "Timeout",
        "The payment service did not respond.",
        readOnlyDict [ "service", ("payments" :> obj); "elapsedMs", (5000 :> obj) ])

printfn "%b" (empty.Code = "" && isNull empty.Message && isNull empty.Extensions)
printfn "%b" (isNull codeOnly.Message && isNull codeOnly.Extensions)

let payment = MinimalResult<decimal>.Failure([| withData |] :> IReadOnlyList<ResultMessage>)
match payment.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%O" ext.["service"]

let simple = MinimalResult<decimal>.Failure("Timeout", "The payment service did not respond.")
match simple.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%d" ext.Count
```

The output is `true`, `true`, `payments`, then `0`.

## Extensions: attaching additional data to a failure

A failure often needs more than a code and a text: the field that failed validation, the id that was not found, or how long to wait before a retry. Put that data in the message's `Extensions`. There are two ways to set it without building `ResultMessage` objects yourself:

- `Failure(code, message, extensions)` when you have all the data up front.
- `WithExtension(key, value)` when you add data one entry at a time, for example in a later layer.

Keys are **case-insensitive** in every dictionary the library builds: `"UserId"`, `"userid"` and `"USERID"` are the same key. If the dictionary you pass to `Failure(code, message, extensions)` holds keys that differ only by case, they are merged into one entry and the last one enumerated wins. A dictionary you pass to the `ResultMessage` constructor yourself is stored as you built it.

### Failure(code, message, extensions)

Both `MinimalResult<T>` and `MinimalResult` have this overload. `extensions` is an `IReadOnlyDictionary<string, object>` and may be `null`. The result is a failure with exactly one message, just like `Failure(code, message)`. `Response` is `default(T)` for `MinimalResult<T>` and `null` for `MinimalResult`, and the non-generic overload returns a real `MinimalResult`.

The message does not keep your dictionary. It stores a read-only copy taken at call time, so:

- adding, removing or changing entries in your dictionary afterwards does not change the result;
- `Extensions` is never the same reference as the dictionary you passed;
- casting `Extensions` to `IDictionary<string, object>` and modifying it throws `NotSupportedException`.

The copy is shallow. Values are stored by reference, so if a value is mutable (for example a `List<string>`) and you change it later, the change is visible through the result. `WithExtension` copies the same way.

```csharp
var details = new Dictionary<string, object> { ["field"] = "Email", ["maxLength"] = 254 };
var result = MinimalResult<int>.Failure("EmailTooLong", "Email is too long.", details);

details["field"] = "Name";
details.Add("trimmed", true);

var extensions = result.Messages[0].Extensions!;
Console.WriteLine($"{extensions["field"]} {extensions.Count} {ReferenceEquals(details, extensions)}");

var command = MinimalResult.Failure(
    "Locked",
    "The record is locked.",
    new Dictionary<string, object> { ["retryAfterSeconds"] = 30 });
Console.WriteLine($"{command.GetType().Name} {command.Messages[0].Extensions!["retryAfterSeconds"]}");
```

The output is `Email 2 False`, then `MinimalResult 30`. The result still shows the values from the moment `Failure` was called.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage

let details = Dictionary<string, obj>()
details.["field"] <- ("Email" :> obj)
details.["maxLength"] <- (254 :> obj)

let result =
    MinimalResult<int>.Failure("EmailTooLong", "Email is too long.", details :> IReadOnlyDictionary<string, obj>)

details.["field"] <- ("Name" :> obj)
details.Add("trimmed", (true :> obj))

match result.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%O %d %b" ext.["field"] ext.Count (obj.ReferenceEquals(details, ext))

let command =
    MinimalResult.Failure("Locked", "The record is locked.", readOnlyDict [ "retryAfterSeconds", (30 :> obj) ])

match command.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%s %O" (command.GetType().Name) ext.["retryAfterSeconds"]
```

The output is `Email 2 false`, then `MinimalResult 30`.

### Null or empty extensions

If `extensions` is `null` or has no entries, the message gets the same shared, empty, read-only dictionary that `Failure(code, message)` uses. `Extensions` is therefore never `null` on a result built by either overload, and no new dictionary is allocated.

```csharp
var plain = MinimalResult<int>.Failure("NotFound", "User not found.");
var fromNull = MinimalResult<int>.Failure("NotFound", "User not found.", null);
var fromEmpty = MinimalResult<int>.Failure("NotFound", "User not found.", new Dictionary<string, object>());

Console.WriteLine(ReferenceEquals(plain.Messages[0].Extensions, fromNull.Messages[0].Extensions));
Console.WriteLine(ReferenceEquals(plain.Messages[0].Extensions, fromEmpty.Messages[0].Extensions));
Console.WriteLine(fromNull.Messages[0].Extensions!.Count);
```

The output is `True`, `True`, then `0`.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage

let plain = MinimalResult<int>.Failure("NotFound", "User not found.")
let fromNull = MinimalResult<int>.Failure("NotFound", "User not found.", null)
let fromEmpty =
    MinimalResult<int>.Failure("NotFound", "User not found.", Dictionary<string, obj>() :> IReadOnlyDictionary<string, obj>)

printfn "%b" (obj.ReferenceEquals(plain.Messages.[0].Extensions, fromNull.Messages.[0].Extensions))
printfn "%b" (obj.ReferenceEquals(plain.Messages.[0].Extensions, fromEmpty.Messages.[0].Extensions))
```

The output is `true`, then `true`.

### WithExtension(key, value)

`WithExtension` is an extension method in the `RzR.MinimalResultMessage` namespace, so the usual `using RzR.MinimalResultMessage;` or `open RzR.MinimalResultMessage` brings it in. It works on both result types, on successes as well as failures.

- **It returns a new result.** The original result and its messages are not changed. The new result has the same `IsSuccess`, `IsFailure` and `Response`, so a success stays a success.
- **It keeps the type.** On a `MinimalResult` it returns a `MinimalResult`; on a `MinimalResult<T>` it returns a `MinimalResult<T>`.
- **It changes the last message only.** The last message is replaced by a new `ResultMessage` with the same `Code` and `Message` and a read-only copy of its `Extensions` plus `key = value`. A `null` `Extensions` counts as empty. All other messages are kept as the same instances.
- **It creates a message when there is none.** See [On a result with no messages](#on-a-result-with-no-messages).
- **An existing key is overwritten, ignoring case.** `WithExtension("Key", 1).WithExtension("key", 2)` leaves one entry with the value `2`. The number of extensions does not grow.
- **Calls can be chained.** Each call adds to the result of the previous one.

Extensions passed to `Failure(code, message, extensions)` count toward the same message. A failure created with 1 extension and then given 3 more keys through `WithExtension` ends up with 4.

The shared empty dictionary is never modified, so other failures are not affected.

```csharp
var original = MinimalResult<int>.Failure("NotFound", "User not found.");
var enriched = original
    .WithExtension("userId", 42)
    .WithExtension("source", "cache")
    .WithExtension("userId", 43);

var extensions = enriched.Messages[0].Extensions!;
Console.WriteLine($"{extensions["userId"]} {extensions["source"]} {extensions.Count}");
Console.WriteLine($"{ReferenceEquals(original, enriched)} {original.Messages[0].Extensions!.Count}");

var command = MinimalResult.Failure("Locked", "The record is locked.").WithExtension("retryAfterSeconds", 30);
Console.WriteLine($"{command.GetType().Name} {command.IsFailure}");

var errors = new List<ResultMessage>
{
    new ResultMessage("NameRequired", "Name is required."),
    new ResultMessage("EmailInvalid", "Email is not valid.")
};
var validation = MinimalResult<int>.Failure(errors).WithExtension("field", "Email");
Console.WriteLine($"{validation.Messages[0].Extensions == null} {validation.Messages[1].Extensions!["field"]}");
```

The output is `43 cache 2`, `False 0`, `MinimalResult True`, then `True Email`. The second `userId` call overwrote the first, the original result still has no extensions, and only the last validation message received the `field` entry.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

let original = MinimalResult<int>.Failure("NotFound", "User not found.")
let enriched =
    original.WithExtension("userId", (42 :> obj)).WithExtension("source", ("cache" :> obj)).WithExtension("userId", (43 :> obj))

match enriched.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%O %O %d" ext.["userId"] ext.["source"] ext.Count

match original.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%b %d" (obj.ReferenceEquals(original, enriched)) ext.Count

let command = MinimalResult.Failure("Locked", "The record is locked.").WithExtension("retryAfterSeconds", (30 :> obj))
printfn "%s %b" (command.GetType().Name) command.IsFailure

let errors =
    [| ResultMessage("NameRequired", "Name is required.")
       ResultMessage("EmailInvalid", "Email is not valid.") |] :> IReadOnlyList<ResultMessage>
let validation = MinimalResult<int>.Failure(errors).WithExtension("field", ("Email" :> obj))

printfn "%b" (isNull validation.Messages.[0].Extensions)
match validation.Messages.[1].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%O" ext.["field"]
```

The output is `43 cache 2`, `false 0`, `MinimalResult true`, `true`, then `Email`.

The next example combines both sources of extensions on one failure.

```csharp
var withDetails = MinimalResult.Failure(
        "Locked",
        "The record is locked.",
        new Dictionary<string, object> { ["recordId"] = 7 })
    .WithExtension("a", 1)
    .WithExtension("b", 2)
    .WithExtension("c", 3);

Console.WriteLine($"{withDetails.Messages.Count} {withDetails.Messages[0].Extensions!.Count}");
```

The output is `1 4`.

```fsharp
open RzR.MinimalResultMessage

let failure = MinimalResult.Failure("Locked", "The record is locked.", readOnlyDict [ "recordId", (7 :> obj) ])
let withDetails = failure.WithExtension("a", (1 :> obj)).WithExtension("b", (2 :> obj)).WithExtension("c", (3 :> obj))

match withDetails.Messages.[0].Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%d %d" withDetails.Messages.Count ext.Count
```

The output is `1 4`.

### On a result with no messages

A result can have no messages: `Success()`, `Success(value)`, a `true` conversion, `Failure` with an empty list, or `Failure(null)`. `WithExtension` then creates one message to hold the extension: `Code` is `""`, `Message` is `null`, and `Extensions` holds `key = value`. Later calls add to that message, because it is now the last one.

`IsSuccess`, `IsFailure` and `Response` do not change. `Success()` itself still has an empty `Messages` list; only the result returned by `WithExtension` has the new message.

```csharp
var plain = MinimalResult.Success();
var tagged = plain.WithExtension("a", 1).WithExtension("b", 2).WithExtension("c", 3);

var message = tagged.Messages[0];
Console.WriteLine($"{tagged.IsSuccess} {tagged.Messages.Count} {plain.Messages.Count}");
Console.WriteLine($"{message.Code == ""} {message.Message == null} {message.Extensions!.Count}");
```

The output is `True 1 0`, then `True True 3`. The result is still a success. It has 1 message with 3 extensions, while the original `Success()` result still has no messages.

```fsharp
open RzR.MinimalResultMessage

let plain = MinimalResult.Success()
let tagged = plain.WithExtension("a", (1 :> obj)).WithExtension("b", (2 :> obj)).WithExtension("c", (3 :> obj))

let message = tagged.Messages.[0]
printfn "%b %d %d" tagged.IsSuccess tagged.Messages.Count plain.Messages.Count

match message.Extensions with
| null -> printfn "no extensions"
| ext -> printfn "%b %b %d" (message.Code = "") (isNull message.Message) ext.Count
```

The output is `true 1 0`, then `true true 3`.

### When WithExtension throws

- **`InvalidOperationException`** when the last element of `Messages` is `null`, for example after `Failure(new List<ResultMessage> { null! })`. An empty or `null` `Messages` list does not throw; see [On a result with no messages](#on-a-result-with-no-messages).
- **`ArgumentNullException`** when `key` is `null` (`ParamName` is `"key"`) or when the result itself is `null` (`ParamName` is `"result"`).

Do not pass `null` as `value`: the parameter is a non-nullable `object`.

```csharp
try
{
    MinimalResult.Failure(new List<ResultMessage> { null! }).WithExtension("userId", 42);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.GetType().Name);
}

try
{
    MinimalResult.Failure("Locked", "The record is locked.").WithExtension(null!, 42);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine(ex.ParamName);
}
```

The output is `InvalidOperationException`, then `key`.

```fsharp
open System
open System.Collections.Generic
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

try
    let withNullMessage = [| Unchecked.defaultof<ResultMessage> |] :> IReadOnlyList<ResultMessage>
    MinimalResult.Failure(withNullMessage).WithExtension("userId", (42 :> obj)) |> ignore
with :? InvalidOperationException as ex ->
    printfn "%s" (ex.GetType().Name)

try
    MinimalResult.Failure("Locked", "The record is locked.").WithExtension(Unchecked.defaultof<string>, (42 :> obj)) |> ignore
with :? ArgumentNullException as ex ->
    printfn "%s" ex.ParamName
```

The output is `InvalidOperationException`, then `key`.

## Implicit conversions

The conversions keep `return` statements short. They are defined as `op_Implicit` operators.

| From | To | Result |
|---|---|---|
| `T` | `MinimalResult<T>` | `Success(value)`, including `default(T)` and a `null` string |
| `bool` | `MinimalResult` | `true` gives `Success()`. `false` gives a failure with one message: Code `"UnSuccessfullyReqExec"`, Message `null`. |
| `Exception` | `MinimalResult<T>` or `MinimalResult` | Failure with one message: Code `"UnhandledException"`, Message `ex.Message` |

Converting a `null` exception throws `ArgumentNullException` with `ParamName` `"exception"`. Use `Failure(...)` when you have no exception object.

```csharp
static MinimalResult<int> ParseQuantity(string text)
{
    try
    {
        return int.Parse(text);
    }
    catch (Exception ex)
    {
        return ex;
    }
}

static MinimalResult EndSession(HashSet<string> sessions, string token) => sessions.Remove(token);

var parsed = ParseQuantity("42");
var invalid = ParseQuantity("forty-two");
Console.WriteLine($"{parsed.IsSuccess} {parsed.Response}");
Console.WriteLine($"{invalid.IsFailure} {invalid.Messages[0].Code}");

var sessions = new HashSet<string> { "abc" };
var ended = EndSession(sessions, "abc");
var unknown = EndSession(sessions, "abc");
Console.WriteLine($"{ended.IsSuccess} {unknown.Messages[0].Code} {unknown.Messages[0].Message == null}");

MinimalResult crashed = new InvalidOperationException("Connection lost");
Console.WriteLine($"{crashed.Messages[0].Code} {crashed.Messages[0].Message}");

Exception missing = null!;
try
{
    MinimalResult rejected = missing;
}
catch (ArgumentNullException ex)
{
    Console.WriteLine(ex.ParamName);
}
```

The output is `True 42`, `True UnhandledException`, `True UnSuccessfullyReqExec True`, `UnhandledException Connection lost`, then `exception`. The `Message` of the failed parse is the `FormatException` text from the runtime.

In F#, call `op_Implicit` explicitly and annotate the target type where the compiler cannot infer it. Upcast exceptions to `exn` when the overload is not obvious.

```fsharp
open System
open System.Collections.Generic
open RzR.MinimalResultMessage

let parseQuantity (text: string) : MinimalResult<int> =
    try
        MinimalResult<int>.op_Implicit (int text)
    with ex ->
        MinimalResult<int>.op_Implicit ex

let endSession (sessions: HashSet<string>) (token: string) : MinimalResult =
    MinimalResult.op_Implicit (sessions.Remove token)

let parsed = parseQuantity "42"
let invalid = parseQuantity "forty-two"
printfn "%d" parsed.Response
printfn "%s" invalid.Messages.[0].Code

let sessions = HashSet<string>([ "abc" ])
let ended = endSession sessions "abc"
let unknown = endSession sessions "abc"
printfn "%b" ended.IsSuccess
printfn "%s" unknown.Messages.[0].Code

let crashed : MinimalResult = MinimalResult.op_Implicit (InvalidOperationException "Connection lost" :> exn)
printfn "%s" crashed.Messages.[0].Message

try
    let rejected : MinimalResult = MinimalResult.op_Implicit (null: exn | null)
    printfn "%b" rejected.IsSuccess
with :? ArgumentNullException as ex ->
    printfn "%s" ex.ParamName
```

The output is `42`, `UnhandledException`, `true`, `UnSuccessfullyReqExec`, `Connection lost`, then `exception`.

F# type-directed conversions also work when the target type is known. The compiler raises warning FS3391 for each one. Silence it with `#nowarn "3391"` if you use this style on purpose.

```fsharp
#nowarn "3391"

open System
open RzR.MinimalResultMessage

let fromFalse : MinimalResult = false
let fromException : MinimalResult = Exception "Connection lost"
let fromValue : MinimalResult<int> = 42

printfn "%s %s %d" fromFalse.Messages.[0].Code fromException.Messages.[0].Code fromValue.Response
```

The output is `UnSuccessfullyReqExec UnhandledException 42`.

## Interfaces

`IMinimalResult` exposes `IsSuccess`, `IsFailure` and `Messages`. `IMinimalResult<T>` adds `Response`. Both result types implement them.

Accept `IMinimalResult` in code that only cares about the outcome, such as a logger, an HTTP status mapper or a metrics recorder. It then takes `MinimalResult`, `MinimalResult<int>` and `MinimalResult<Order>` alike. Accept `IMinimalResult<T>` when you also need the value. Return the concrete types from your own methods so callers keep the factories and conversions.

```csharp
static void LogResult(string operation, IMinimalResult result)
{
    if (result.IsSuccess)
    {
        Console.WriteLine($"{operation}: OK");
        return;
    }

    foreach (var message in result.Messages)
    {
        Console.WriteLine($"{operation}: {message.Code} {message.Message}");
    }
}

static string Describe(IMinimalResult<int> result) =>
    result.IsSuccess ? $"value {result.Response}" : $"failed with {result.Messages.Count} message(s)";

LogResult("count", MinimalResult<int>.Success(5));
LogResult("delete", MinimalResult.Failure("Locked", "The record is locked."));
Console.WriteLine(Describe(MinimalResult<int>.Success(5)));
```

The output is `count: OK`, `delete: Locked The record is locked.`, then `value 5`.

```fsharp
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Abstractions

let logResult (operation: string) (result: IMinimalResult) =
    if result.IsSuccess then
        printfn "%s: OK" operation
    else
        for message in result.Messages do
            printfn "%s: %s %s" operation message.Code (string message.Message)

let describe (result: IMinimalResult<int>) =
    if result.IsSuccess then sprintf "value %d" result.Response
    else sprintf "failed with %d message(s)" result.Messages.Count

logResult "count" (MinimalResult<int>.Success 5)
logResult "delete" (MinimalResult.Failure("Locked", "The record is locked."))
printfn "%s" (describe (MinimalResult<int>.Success 5))
```

The output is the same as the C# example.

## Equality and nullability

- **Reference equality.** `MinimalResult<T>`, `MinimalResult` and `ResultMessage` do not override `Equals` or `==`. Two results with the same content are not equal. Compare `IsSuccess`, `Response` and message codes instead.
- **Messages are not copied.** `Failure(messages)` stores your list as-is. If you change the list afterwards, the result changes too. Pass a copy, such as `list.ToArray()`, if you keep using the list.
- **Extensions are copied.** Unlike `Failure(messages)`, `Failure(code, message, extensions)` and `WithExtension` store a read-only copy of the data, so later changes to your dictionary do not reach the result.
- **Nullable annotations.** `Code` is non-null. `Message` and `Extensions` may be `null`. `Extensions` is never `null` on a message built by `Failure(code, message)`, `Failure(code, message, extensions)` or `WithExtension`, but the annotation stays nullable. In C# with `<Nullable>enable</Nullable>`, the compiler warns if you dereference them without a check.

```csharp
var a = MinimalResult<int>.Success(1);
var b = MinimalResult<int>.Success(1);
Console.WriteLine($"{a == b} {a.Equals(b)}");

var messages = new List<ResultMessage> { new ResultMessage("E1", "First") };
var result = MinimalResult<int>.Failure(messages);
messages.Add(new ResultMessage("E2", "Second"));
Console.WriteLine($"{result.Messages.Count} {ReferenceEquals(messages, result.Messages)}");

var message = new ResultMessage("E1");
string text = message.Message ?? "(no text)";
int extensionCount = message.Extensions?.Count ?? 0;
Console.WriteLine($"{message.Code} {text} {extensionCount}");
```

The output is `False False`, `2 True`, then `E1 (no text) 0`.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

let a = MinimalResult<int>.Success 1
let b = MinimalResult<int>.Success 1
printfn "%b %b" (a = b) (a.Equals b)

let messages = ResizeArray<ResultMessage>()
messages.Add(ResultMessage("E1", "First"))
let result = MinimalResult<int>.Failure(messages)
messages.Add(ResultMessage("E2", "Second"))
printfn "%d %b" result.Messages.Count (obj.ReferenceEquals(messages, result.Messages))

let message = ResultMessage("E1")
let text = match message.Message with | null -> "(no text)" | t -> t
let extensionCount = match message.Extensions with | null -> 0 | ext -> ext.Count
printfn "%s %s %d" message.Code text extensionCount
```

The output is `false false`, `2 true`, then `E1 (no text) 0`.

## Behavior notes / pitfalls

- **C#: `MinimalResult<object> r = false;` is a success.** The value conversion `T -> MinimalResult<T>` wins over the bool rule, so `r` is a success whose `Response` is `false`. Declare the variable as `MinimalResult` to get the failure.
- **C#: `MinimalResult<Exception> r = ex;` does not compile (CS0457).** With `T = Exception`, the value and exception conversions both match. Call `MinimalResult<Exception>.Success(ex)` or `Failure(...)`.
- **C#: `MinimalResult.Success(5)` does not compile (CS1501).** The non-generic `Success()` hides the inherited generic one. Use `MinimalResult<int>.Success(5)`. In F#, the inherited `Success` is still visible and returns a `MinimalResult<obj>`.
- **F#: a bare `null` passed to `op_Implicit` binds to the exception conversion and throws `ArgumentNullException` at runtime;** write `(null: exn | null)` to make the intent explicit.
- **`Failure(null)` leaves `Messages` null.** A single `null` argument binds to the list overload and is stored as-is. Code that loops over `Messages` will then throw `NullReferenceException`. `WithExtension` does not throw here: it returns a result with one message (`Code` `""`) that holds the extension.

```text
MinimalResult<Exception> r = new InvalidOperationException("x");   error CS0457
var r = MinimalResult.Success(5);                                    error CS1501```

```csharp
MinimalResult<object> boxed = false;
MinimalResult command = false;
Console.WriteLine($"{boxed.IsSuccess} {boxed.Response}");
Console.WriteLine($"{command.IsFailure} {command.Messages[0].Code}");

var noMessages = MinimalResult<int>.Failure(null!);
Console.WriteLine($"{noMessages.IsFailure} {noMessages.Messages == null}");
```

The output is `True False`, `True UnSuccessfullyReqExec`, then `True True`.

```fsharp
open System.Collections.Generic
open RzR.MinimalResultMessage
open RzR.MinimalResultMessage.Models

let inherited = MinimalResult.Success(box 5)
printfn "%s %O" (inherited.GetType().Name) inherited.Response

let noMessages = MinimalResult<int>.Failure(Unchecked.defaultof<IReadOnlyList<ResultMessage>>)
printfn "%b %b" noMessages.IsFailure (isNull (box noMessages.Messages))
```

The output is ``MinimalResult`1 5``, then `true true`.
