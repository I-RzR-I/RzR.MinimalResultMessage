namespace RzR.MinimalResultMessage

open System
open System.Collections.Generic
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.Modules
open RzR.MinimalResultMessage.Abstractions

/// <summary>
/// Result of an operation that returns a value of type T on success.
/// On a failure, Response is default(T), which is null for reference types.
/// </summary>
/// <typeparam name="T">The type of the response value.</typeparam>
/// <param name="isSuccess">Whether the operation succeeded.</param>
/// <param name="response">The response value; the default value of T on a failure.</param>
/// <param name="messages">The result messages.</param>
type MinimalResult<'T> internal (isSuccess: bool, response: 'T, messages: IReadOnlyList<ResultMessage>) =

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    member _.IsSuccess = isSuccess
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    member _.IsFailure = not isSuccess
    /// <summary>
    /// Gets the response value. On a failure, this is the default value of T.
    /// </summary>
    member _.Response = response
    /// <summary>
    /// Gets the result messages. Empty for a result created by Success(); WithExtension may add one message. Null when the result was created with Failure(null).
    /// </summary>
    member _.Messages = messages

    interface IMinimalResult<'T> with
        member _.Response = response

    interface IMinimalResult with
        member _.IsSuccess = isSuccess
        member _.IsFailure = not isSuccess
        member _.Messages = messages

    /// <summary>
    /// Creates a successful result holding the response.
    /// </summary>
    /// <param name="response">The response value.</param>
    /// <returns>A successful <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> with no messages.</returns>
    static member Success(response: 'T) : MinimalResult<'T> =
        MinimalResult<'T>(true, response, [])

    /// <summary>
    /// Creates a failed result with one message that uses the shared empty Extensions.
    /// </summary>
    /// <param name="code">The message code.</param>
    /// <param name="message">The message text, or null.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> whose Response is the default value of T.</returns>
    static member Failure(code: string, message: string | null) : MinimalResult<'T> =
        MinimalResult<'T>.Failure(code, message, null)

    /// <summary>
    /// Creates a failed result with one message that stores a read-only copy of the given extensions.
    /// </summary>
    /// <param name="code">The message code.</param>
    /// <param name="message">The message text, or null.</param>
    /// <param name="extensions">The additional data for the message, or null. Null or empty uses the shared empty Extensions; otherwise a read-only snapshot is stored, so later changes to the caller's dictionary are not visible. The copy is shallow: values are stored by reference. Keys in the stored copy are compared case-insensitively (ordinal, ignoring case); keys that differ only by case are merged and the last one enumerated wins.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> whose Response is the default value of T.</returns>
    static member Failure(code: string, message: string | null, extensions: IReadOnlyDictionary<string, obj> | null) : MinimalResult<'T> =
        MinimalResult<'T>(false, Unchecked.defaultof<'T>, [ ResultMessageHelpers.CreateMessage(code, message, extensions) ])

    /// <summary>
    /// Creates a failed result with the given messages. The list is stored as-is, without copying it.
    /// </summary>
    /// <param name="messages">The result messages.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> whose Response is the default value of T.</returns>
    static member Failure(messages: IReadOnlyList<ResultMessage>) : MinimalResult<'T> =
        MinimalResult<'T>(false, Unchecked.defaultof<'T>, messages)

    /// <summary>
    /// Converts a value into a successful result.
    /// </summary>
    /// <param name="response">The response value.</param>
    /// <returns>A successful <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> holding the value.</returns>
    static member op_Implicit(response: 'T) : MinimalResult<'T> =
        MinimalResult<'T>.Success(response)

    /// <summary>
    /// Converts an exception into a failed result with code "UnhandledException" and the exception's Message.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> with one message.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when exception is null.</exception>
    static member op_Implicit(``exception``: Exception | null) : MinimalResult<'T> =
        let ex = ResultMessageHelpers.RequireConvertibleException(``exception``, "MinimalResult<T>")
        MinimalResult<'T>.Failure(ResultMessageDefaults.UnhandledExceptionCode, ex.Message)

/// <summary>
/// Result of an operation that returns no value. Response is always null.
/// Static members inherited from MinimalResult&lt;obj&gt; (for example Success(value)) return MinimalResult&lt;obj&gt;, not MinimalResult.
/// </summary>
/// <param name="isSuccess">Whether the operation succeeded.</param>
/// <param name="messages">The result messages.</param>
type MinimalResult internal (isSuccess: bool, messages: IReadOnlyList<ResultMessage>) =
    inherit MinimalResult<obj | null>(isSuccess, null, messages)

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="T:RzR.MinimalResultMessage.MinimalResult"/> with no messages.</returns>
    static member Success() : MinimalResult =
        MinimalResult(true, [])

    /// <summary>
    /// Creates a failed result with one message that uses the shared empty Extensions.
    /// </summary>
    /// <param name="code">The message code.</param>
    /// <param name="message">The message text, or null.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult"/> with one message.</returns>
    static member Failure(code: string, message: string | null) : MinimalResult =
        MinimalResult.Failure(code, message, null)

    /// <summary>
    /// Creates a failed result with one message that stores a read-only copy of the given extensions.
    /// </summary>
    /// <param name="code">The message code.</param>
    /// <param name="message">The message text, or null.</param>
    /// <param name="extensions">The additional data for the message, or null. Null or empty uses the shared empty Extensions; otherwise a read-only snapshot is stored, so later changes to the caller's dictionary are not visible. The copy is shallow: values are stored by reference. Keys in the stored copy are compared case-insensitively (ordinal, ignoring case); keys that differ only by case are merged and the last one enumerated wins.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult"/> with one message.</returns>
    static member Failure(code: string, message: string | null, extensions: IReadOnlyDictionary<string, obj> | null) : MinimalResult =
        MinimalResult(false, [ ResultMessageHelpers.CreateMessage(code, message, extensions) ])

    /// <summary>
    /// Creates a failed result with the given messages. The list is stored as-is, without copying it.
    /// </summary>
    /// <param name="messages">The result messages.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult"/> holding the given messages.</returns>
    static member Failure(messages: IReadOnlyList<ResultMessage>) : MinimalResult =
        MinimalResult(false, messages)

    /// <summary>
    /// Converts a boolean into a result. true converts to Success(); false converts to a failure with code "UnSuccessfullyReqExec" and no message text.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <returns>A successful or failed <see cref="T:RzR.MinimalResultMessage.MinimalResult"/>.</returns>
    static member op_Implicit(isSuccess: bool) : MinimalResult =
        if isSuccess then MinimalResult.Success()
        else MinimalResult.Failure(ResultMessageDefaults.UnSuccessfullyReqExecCode, null)

    /// <summary>
    /// Converts an exception into a failed result with code "UnhandledException" and the exception's Message.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A failed <see cref="T:RzR.MinimalResultMessage.MinimalResult"/> with one message.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when exception is null.</exception>
    static member op_Implicit(``exception``: Exception | null) : MinimalResult =
        let ex = ResultMessageHelpers.RequireConvertibleException(``exception``, "MinimalResult")
        MinimalResult.Failure(ResultMessageDefaults.UnhandledExceptionCode, ex.Message)
