> **Note** This library targets `netstandard2.0` and `netstandard2.1`. It is written in F# and can be used from both C# and F#.

[![NuGet Version](https://img.shields.io/nuget/v/RzR.MinimalResultMessage.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.MinimalResultMessage/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.MinimalResultMessage.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.MinimalResultMessage)

<br />

The goal of this repository is to give methods and processes one way to report their outcome. It is a structured response and callers don't need a different return shape for every operation.

Every operation returns the same structured, easy-to-parse model. The caller can check whether it succeeded, read the value it produced, and inspect the messages that explain a failure.

This is a deliberately minimal F# take on the author's C# library [RzR.ResultMessage](https://github.com/I-RzR-I/AggregatedGenericResultMessage). It keeps only the core result model: no message types or action helpers, and a single fluent extension (`WithExtension`).

What the library offers:

- `MinimalResult<T>`: the result of an operation that returns a value. Create it with `Success(response)`, `Failure(code, message)` or `Failure(messages)`. It exposes `IsSuccess`, `IsFailure`, `Response` (the default value of `T` on a failure) and `Messages`.
- `MinimalResult`: the non-generic result for operations that return no value. It inherits from `MinimalResult<object>` and offers `Success()`, `Failure(code, message)` and `Failure(messages)`.
- `ResultMessage`: one message attached to a result. It has a `Code`, a `Message` (may be `null`) and `Extensions` (additional data keyed by name, may be `null`).
- Additional failure data: pass a dictionary to `Failure(code, message, extensions)` (stored as a read-only copy), or add one entry at a time with `WithExtension(key, value)`, which works on successes and failures.
- `IMinimalResult` and `IMinimalResult<T>`: interfaces for code that only needs to read a result.
- Implicit conversions:
  - from `T` to a successful `MinimalResult<T>`.
  - from `bool` to `MinimalResult`. `false` gives a failure with code `UnSuccessfullyReqExec`.
  - from `Exception` to a failure with code `UnhandledException` and the exception's `Message`. A `null` exception throws `ArgumentNullException`.

| Namespace | Types |
| --- | --- |
| `RzR.MinimalResultMessage` | `MinimalResult<T>`, `MinimalResult` |
| `RzR.MinimalResultMessage.Models` | `ResultMessage` |
| `RzR.MinimalResultMessage.Abstractions` | `IMinimalResult`, `IMinimalResult<T>` |

Results and messages use reference equality. Two results with the same content are not equal.

The only dependency is `FSharp.Core` (9.0.100 or newer). Add the library to your project and you can start using it.

## Install

```
dotnet add package RzR.MinimalResultMessage
```

or, from the Package Manager Console:

```
Install-Package RzR.MinimalResultMessage
```

## Quick start

### C#

```csharp
using System;
using RzR.MinimalResultMessage;

public static class OrderService
{
    public static MinimalResult<int> ParseQuantity(string input)
    {
        if (int.TryParse(input, out var quantity))
            return quantity; // implicit conversion: success

        return MinimalResult<int>.Failure("InvalidQuantity", $"'{input}' is not a number.");
    }

    public static MinimalResult Save(int quantity)
    {
        try
        {
            // persist the quantity
            return true; // implicit conversion: success
        }
        catch (Exception ex)
        {
            return ex; // implicit conversion: failure with code "UnhandledException"
        }
    }

    public static void Print(MinimalResult<int> result)
    {
        if (result.IsSuccess)
            Console.WriteLine(result.Response);
        else
            foreach (var m in result.Messages)
                Console.WriteLine($"{m.Code}: {m.Message}");
    }
}
```

### F#

In F#, call the conversions explicitly with `op_Implicit`. A type-annotated binding such as `let r : MinimalResult = false` also works, but raises warning FS3391.

```fsharp
open System
open RzR.MinimalResultMessage

let parseQuantity (input: string) : MinimalResult<int> =
    match Int32.TryParse input with
    | true, quantity -> MinimalResult<int>.Success quantity
    | _ -> MinimalResult<int>.Failure("InvalidQuantity", $"'{input}' is not a number.")

let save (quantity: int) : MinimalResult =
    try
        // persist the quantity
        MinimalResult.op_Implicit true
    with ex ->
        MinimalResult.op_Implicit ex

let result = parseQuantity "12"
if result.IsSuccess then printfn $"{result.Response}"
else result.Messages |> Seq.iter (fun m -> printfn $"{m.Code}: {m.Message}")
```

## Content
1. [USING](docs/usage.md)
2. [CHANGELOG](docs/CHANGELOG.md)
3. [BRANCH-GUIDE](docs/branch-guide.md)

## Build and test

Run the commands from the solution folder:

```
cd src
dotnet test --solution RzR.Shared.ResultMessage.slnx
```

> **Note** This requires the .NET 10 SDK. The `global.json` in `src` selects Microsoft.Testing.Platform as the test runner, and that setting only applies when you run `dotnet test` from that folder.
