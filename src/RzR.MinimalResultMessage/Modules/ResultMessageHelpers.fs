namespace RzR.MinimalResultMessage.Modules

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open RzR.MinimalResultMessage.Models

/// <summary>
/// Internal helpers for building result messages: extension snapshots, message creation and exception checks.
/// </summary>
module internal ResultMessageHelpers =
    /// <summary>
    /// Copies the entries of an extensions map into a new mutable, case-insensitive dictionary.
    /// Keys that differ only by case are merged; the last one enumerated wins.
    /// </summary>
    /// <param name="extensions">The extensions map to copy.</param>
    /// <param name="capacity">The initial capacity of the new dictionary.</param>
    /// <returns>A new case-insensitive dictionary with the keys and values of the source.</returns>
    let CopyExtensions (extensions: IReadOnlyDictionary<string, obj>, capacity: int) : Dictionary<string, obj> =
        let copy = ExtensionsDefaults.NewExtensions capacity
        for KeyValue(key, value) in extensions do
            copy.[key] <- value
        copy

    /// <summary>
    /// Turns caller-supplied extensions into the value stored on a message.
    /// </summary>
    /// <param name="extensions">The extensions supplied by the caller; may be null.</param>
    /// <returns>The shared empty map when extensions is null or empty; otherwise a new read-only snapshot of its entries.</returns>
    let SnapshotExtensions (extensions: IReadOnlyDictionary<string, obj> | null) : IReadOnlyDictionary<string, obj> =
        match extensions with
        | null -> ExtensionsDefaults.EmptyMessageExtensions
        | source when source.Count = 0 -> ExtensionsDefaults.EmptyMessageExtensions
        | source -> ReadOnlyDictionary<string, obj>(CopyExtensions(source, source.Count)) :> IReadOnlyDictionary<string, obj>

    /// <summary>
    /// Builds one message that stores a read-only snapshot of the given extensions.
    /// </summary>
    /// <param name="code">The message code that identifies the message.</param>
    /// <param name="message">The optional message text; may be null.</param>
    /// <param name="extensions">The extensions supplied by the caller; may be null.</param>
    /// <returns>A new <see cref="T:RzR.MinimalResultMessage.Models.ResultMessage"/> whose Extensions is the shared empty map when extensions is null or empty, or a read-only snapshot otherwise.</returns>
    let CreateMessage (code: string, message: string | null, extensions: IReadOnlyDictionary<string, obj> | null) : ResultMessage =
        new ResultMessage(code, message, SnapshotExtensions extensions)

    /// <summary>
    /// Returns the exception to convert into a failed result, or throws when it is null.
    /// </summary>
    /// <param name="exception">The exception passed to the implicit conversion; may be null.</param>
    /// <param name="resultTypeName">The result type name used in the error text.</param>
    /// <returns>The same exception, known to be non-null.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when exception is null.</exception>
    let RequireConvertibleException (``exception``: Exception | null, resultTypeName: string) : Exception =
        match ``exception`` with
        | null ->
            raise (ArgumentNullException(nameof ``exception``,
                $"Cannot implicitly convert a null Exception to a {resultTypeName}. Use {resultTypeName}.Failure(...) instead."))
        | ex -> ex
