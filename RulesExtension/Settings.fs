namespace OpilioCraft.MetadataEngine.RulesExtension

open System

[<RequireQualifiedAccess>]
module Settings =
    let RulesLocation = IO.Path.Combine(OpilioCraft.Settings.AppDataLocation, "Rules")

// ------------------------------------------------------------------------------------------------

[<RequireQualifiedAccess>]
module RuleName =
    [<Literal>]
    let GuessCamera = "GuessCamera"

    [<Literal>]
    let GuessOwner = "GuessOwner"

    [<Literal>]
    let DetermineDateTaken = "DetermineDateTaken"
