namespace RzR.MinimalResultMessage.Modules

/// <summary>
/// Internal well-known message codes.
/// </summary>
module internal ResultMessageDefaults =
    /// <summary>
    /// Code used when an exception is converted into a failed result.
    /// </summary>
    [<Literal>]
    let UnhandledExceptionCode = "UnhandledException"

    /// <summary>
    /// Code used when false is converted into a failed result.
    /// </summary>
    [<Literal>]
    let UnSuccessfullyReqExecCode = "UnSuccessfullyReqExec"
