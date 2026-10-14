namespace RzR.MinimalResultMessage.Abstractions

open System.Collections.Generic
open RzR.MinimalResultMessage.Models

/// <summary>
/// Represents the outcome of an operation: whether it succeeded and the messages attached to it.
/// </summary>
type IMinimalResult =
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    abstract member IsSuccess: bool with get
    /// <summary>
    /// Gets a value indicating whether the operation failed; the opposite of IsSuccess.
    /// </summary>
    abstract member IsFailure: bool with get
    /// <summary>
    /// Gets the read-only list of messages attached to the result.
    /// </summary>
    abstract member Messages: IReadOnlyList<ResultMessage> with get

/// <summary>
/// Represents the outcome of an operation that also carries a response value.
/// </summary>
/// <typeparam name="T">The type of the response value.</typeparam>
type IMinimalResult<'T> =
    /// <summary>
    /// Gets the response value produced by the operation.
    /// </summary>
    abstract member Response: 'T with get
    inherit IMinimalResult
