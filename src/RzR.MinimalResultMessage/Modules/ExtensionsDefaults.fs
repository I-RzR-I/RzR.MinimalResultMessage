namespace RzR.MinimalResultMessage.Modules

open System
open System.Collections.Generic
open System.Collections.ObjectModel

/// <summary>
/// Internal defaults for extensions dictionaries: the case-insensitive dictionary factory and the shared empty map.
/// </summary>
module internal ExtensionsDefaults =
    /// <summary>
    /// Creates an empty extensions dictionary whose keys are compared case-insensitively (ordinal, ignoring case).
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <returns>A new, empty, case-insensitive dictionary.</returns>
    let NewExtensions (capacity: int) : Dictionary<string, obj> =
        Dictionary<string, obj>(capacity, StringComparer.OrdinalIgnoreCase)

    /// <summary>
    /// Shared, read-only, empty extensions map used when a failure carries no extensions.
    /// </summary>
    let EmptyMessageExtensions : IReadOnlyDictionary<string, obj> =
        ReadOnlyDictionary<string, obj>(NewExtensions 0) :> IReadOnlyDictionary<string, obj>
