namespace OpilioCraft.MetadataEngine.RulesExtension

[<RequireQualifiedAccess>]
module Settings =
    let RulesLocation = System.IO.Path.Combine(OpilioCraft.Settings.AppDataLocation, "Rules")

// ------------------------------------------------------------------------------------------------

[<RequireQualifiedAccess>]
module RuleName =
    [<Literal>]
    let GuessCamera = "GuessCamera"

    [<Literal>]
    let GuessOwner = "GuessOwner"

    [<Literal>]
    let DetermineDateTaken = "DetermineDateTaken"
