namespace RzR.MinimalResultMessage.Models

open System.Collections.Generic

/// <summary>
/// A single message attached to a result, identified by a code and optionally carrying text and additional data.
/// </summary>
/// <param name="code">The message code that identifies the message.</param>
/// <param name="message">The optional message text; may be null.</param>
/// <param name="extensions">The optional additional data keyed by name; may be null.</param>
type ResultMessage(code: string, message: string | null, extensions: IReadOnlyDictionary<string, obj> | null) =

    /// <summary>
    /// Creates a message with an empty code (Code = ""), and null Message and Extensions.
    /// </summary>
    new() = ResultMessage("", null, null)

    /// <summary>
    /// Creates a message with only a code; Message and Extensions are null.
    /// </summary>
    /// <param name="code">The message code that identifies the message.</param>
    new(code: string) = ResultMessage(code, null, null)

    /// <summary>
    /// Creates a message with a code and text; Extensions is null.
    /// </summary>
    /// <param name="code">The message code that identifies the message.</param>
    /// <param name="message">The optional message text; may be null.</param>
    new(code: string, message: string | null) = ResultMessage(code, message, null)

    /// <summary>
    /// Gets the message code that identifies the message.
    /// </summary>
    member _.Code = code

    /// <summary>
    /// Gets the message text; may be null when no text was provided.
    /// </summary>
    member _.Message = message

    /// <summary>
    /// Gets the additional data keyed by name; may be null when no data was provided.
    /// </summary>
    member _.Extensions = extensions
