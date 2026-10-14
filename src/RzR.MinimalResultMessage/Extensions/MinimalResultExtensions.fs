namespace RzR.MinimalResultMessage

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Runtime.CompilerServices
open RzR.MinimalResultMessage.Models
open RzR.MinimalResultMessage.Modules

/// <summary>
/// Extension methods that add data to the messages of a <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> or <see cref="T:RzR.MinimalResultMessage.MinimalResult"/>.
/// </summary>
[<Extension; AbstractClass; Sealed>]
type MinimalResultExtensions =

    static member private AddToLastMessage(messages: IReadOnlyList<ResultMessage>, key: string, value: obj) : IReadOnlyList<ResultMessage> =
        let updatedMessages =
            match (if obj.ReferenceEquals(messages, null) then Array.empty else Seq.toArray messages) with
            | [||] -> [| ResultMessage() |]
            | copied -> copied
        let lastIndex = updatedMessages.Length - 1
        let last = updatedMessages.[lastIndex]
        if obj.ReferenceEquals(last, null) then
            raise (InvalidOperationException("Cannot add an extension because the last message is null."))
        let extensions =
            match last.Extensions with
            | null -> ExtensionsDefaults.NewExtensions 1
            | existing -> ResultMessageHelpers.CopyExtensions(existing, existing.Count + 1)
        extensions.[key] <- value
        updatedMessages.[lastIndex] <- ResultMessage(last.Code, last.Message, ReadOnlyDictionary<string, obj>(extensions))
        List.ofArray updatedMessages :> IReadOnlyList<ResultMessage>

    static member private EnsureArguments(result: objnull, key: string | null) : unit =
        match result with
        | null -> raise (ArgumentNullException(nameof result))
        | _ -> ()
        match key with
        | null -> raise (ArgumentNullException(nameof key))
        | _ -> ()

    /// <summary>
    /// Returns a new result whose last message is replaced by one whose Extensions are a copy of the old ones plus the given key and value. The copy is shallow: values are stored by reference.
    /// On a result with no messages (for example Success()), a message with an empty code and no text is created to hold the extension, and later calls add to it.
    /// </summary>
    /// <param name="result">The source result; it is not changed.</param>
    /// <param name="key">The extension key, compared case-insensitively. An existing key on the last message is overwritten, even if it differs only by case.</param>
    /// <param name="value">The extension value; should not be null.</param>
    /// <returns>A new <see cref="T:RzR.MinimalResultMessage.MinimalResult`1"/> with the same IsSuccess and Response, where only the last message is replaced; on a result with no messages, it holds one new message with an empty code and no text that carries the extension. When result is a <see cref="T:RzR.MinimalResultMessage.MinimalResult"/>, the new result is one too.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when result or key is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the last message is null.</exception>
    [<Extension>]
    static member WithExtension(result: MinimalResult<'T>, key: string, value: obj) : MinimalResult<'T> =
        MinimalResultExtensions.EnsureArguments(box result, key)
        let messages = MinimalResultExtensions.AddToLastMessage(result.Messages, key, value)
        match box result with
        | :? MinimalResult as nonGeneric -> unbox<MinimalResult<'T>> (box (MinimalResult(nonGeneric.IsSuccess, messages)))
        | _ -> MinimalResult<'T>(result.IsSuccess, result.Response, messages)

    /// <summary>
    /// Returns a new result whose last message is replaced by one whose Extensions are a copy of the old ones plus the given key and value. The copy is shallow: values are stored by reference.
    /// On a result with no messages (for example Success()), a message with an empty code and no text is created to hold the extension, and later calls add to it.
    /// </summary>
    /// <param name="result">The source result; it is not changed.</param>
    /// <param name="key">The extension key, compared case-insensitively. An existing key on the last message is overwritten, even if it differs only by case.</param>
    /// <param name="value">The extension value; should not be null.</param>
    /// <returns>A new <see cref="T:RzR.MinimalResultMessage.MinimalResult"/> with the same IsSuccess, where only the last message is replaced; on a result with no messages, it holds one new message with an empty code and no text that carries the extension.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when result or key is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the last message is null.</exception>
    [<Extension>]
    static member WithExtension(result: MinimalResult, key: string, value: obj) : MinimalResult =
        MinimalResultExtensions.EnsureArguments(box result, key)
        MinimalResult(result.IsSuccess, MinimalResultExtensions.AddToLastMessage(result.Messages, key, value))
