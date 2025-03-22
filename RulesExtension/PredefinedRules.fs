module OpilioCraft.MetadataEngine.RulesExtension.PredefinedRules

open OpilioCraft.MetadataEngine
open OpilioCraft.FSharp.Prelude

let tryDetermineDateTaken (metadata : Metadata) : System.DateTime option =
    RulesProvider.getRule RuleName.DetermineDateTaken
    |> RulesProvider.tryApplyRule metadata
    |> Result.toOption
    |> Option.bind (function | FlexibleValue.DateTime dateTime -> Some dateTime | _ -> None)
    |> Option.map (fun localDateTime -> localDateTime.ToUniversalTime())

let tryDetermineCamera (metadata : Metadata) : string option =
    RulesProvider.getRule RuleName.GuessCamera
    |> RulesProvider.tryApplyRule metadata
    |> Result.toOption
    |> RulesProvider.verifyAsString

let tryDetermineOwner (metadata : Metadata) : string option =
    RulesProvider.getRule RuleName.GuessOwner
    |> RulesProvider.tryApplyRule metadata
    |> Result.toOption
    |> RulesProvider.verifyAsString

// high-order rules
let applyMediaRules (metadata : Metadata) : Metadata =
    match metadata.ContentType.Category with
    | ContentCategory.Image
    | ContentCategory.Movie ->
        metadata
        |> MetadataEngine.addDetailIfMissing Slot.DateTaken (tryDetermineDateTaken >> Option.defaultValue metadata.AsOf)
        |> MetadataEngine.addDetailIfMissing Slot.Camera (tryDetermineCamera >> Option.defaultValue "NA")
        |> MetadataEngine.addDetailIfMissing Slot.Owner (tryDetermineOwner >> Option.defaultValue "NA")

    | _ -> metadata
