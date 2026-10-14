### **v1.0.0** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 14-10-2026
> **Initial release.**

* [DEV] - (RzR) -> Add `MinimalResult<T>` class with `Success(response)`, `Failure(code, message)` and `Failure(messages)` factory methods.
* [DEV] - (RzR) -> `MinimalResult<T>` exposes `IsSuccess`, `IsFailure`, `Response` and `Messages`. On a failure, `Response` is the default value of `T`.
* [DEV] - (RzR) -> `Failure(messages)` stores the given list as-is, without copying it.
* [DEV] - (RzR) -> Implicit conversions to `MinimalResult<T>` from `T` (success) and from `Exception` (failure).
* [DEV] - (RzR) -> Add non-generic `MinimalResult : MinimalResult<object>` with `Success()`, `Failure(code, message)` and `Failure(messages)`. `Response` is always `null`.
* [DEV] - (RzR) -> Implicit conversions to `MinimalResult` from `bool` and from `Exception`.
* [DEV] - (RzR) -> `false` converts to a failure with code `UnSuccessfullyReqExec` and no message text.
* [DEV] - (RzR) -> An `Exception` converts to a failure with code `UnhandledException` and the exception's `Message`. A `null` exception throws `ArgumentNullException`.
* [DEV] - (RzR) -> Add `ResultMessage` class with 4 constructors and the `Code`, `Message`, `Extensions` properties.
* [DEV] - (RzR) -> Add `IMinimalResult` and `IMinimalResult<T>` interfaces, implemented by both result types.
* [DEV] - (RzR) -> Failures created by the library share one empty, read-only `Extensions` dictionary.
* [DEV] - (RzR) -> Add `Failure(code, message, extensions)` to `MinimalResult<T>` and `MinimalResult`. The message stores a read-only copy of `extensions`; a `null` or empty dictionary gives the shared empty one.
* [DEV] - (RzR) -> Add the `WithExtension(key, value)` extension method for both result types. It returns a new result of the same type with the key added to (or overwritten in) the last message's `Extensions`. It works on successes and failures.
* [DEV] - (RzR) -> When the result has no messages, `WithExtension` creates one message with code `""` and no text to hold the extension. `IsSuccess`, `IsFailure` and `Response` are kept.
* [DEV] - (RzR) -> `WithExtension` throws `ArgumentNullException` for a `null` result or key, and `InvalidOperationException` only when the last message is `null`.
* [DEV] - (RzR) -> Extension keys are case-insensitive (ordinal, ignoring case) in every dictionary the library builds; keys that differ only by case are merged.
* [DEV] - (RzR) -> Results and messages use reference equality (no value-equality overrides).
* [DEV] - (RzR) -> Enable nullable reference type annotations.
* [DEV] - (RzR) -> Target `netstandard2.0` and `netstandard2.1`.
* [DEV] - (RzR) -> Depend on `FSharp.Core` 9.0.100 or newer (the lowest version that supports the nullable annotations), instead of the SDK's bundled version.
* [DEV] - (RzR) -> Add F# and C# test suites (xUnit v3) running on `net10.0` and `net48` (`net48` on Windows only).
