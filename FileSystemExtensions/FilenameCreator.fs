namespace OpilioCraft.MetadataEngine.FilesystemExtension

open System
open System.Globalization

open OpilioCraft.FSharp.Prelude
open OpilioCraft.MetadataEngine
open OpilioCraft.StringTemplate

module private FilenameCreatorLib =
    // defaults
    [<Literal>]
    let DefaultDateTimeFormat = "yyyyMMddTHHmmss"

    // details helper
    let private tryGetDetail key (metadata : Metadata) =
        metadata.Details.TryGetValue(key)
        |> function
            | true, v    -> Some v
            | false, _   -> None

    let private determineDateTaken (metadata : Metadata) : DateTime =
        if metadata.Details.ContainsKey(Slot.DateTaken)
        then
            metadata.Details[Slot.DateTaken].AsDateTime
        else
            metadata.AsOf

    let toTitleCase value =
        let textInfo = (new CultureInfo("en-US", false)).TextInfo in
        textInfo.ToTitleCase(value)

    // data provider
    let getDateTaken (metadata : Metadata) (args : string list) : string =
        let (format, timezone) =
            match args with
            | [ format ; timezoneId ] -> format, TimeZoneInfo.FindSystemTimeZoneById(timezoneId)
            | [ format ] -> format, TimeZoneInfo.Local
            | [] -> DefaultDateTimeFormat, TimeZoneInfo.Local
            | _ -> failwith "date expects zero, one or two arguments"

        TimeZoneInfo.ConvertTimeFromUtc(metadata |> determineDateTaken, timezone).ToString(format)

    let getDateTakenUTC (metadata : Metadata) (args : string list) : string =
        let format =
            match args with
            | format :: _ -> format
            | _ -> DefaultDateTimeFormat

        (metadata |> determineDateTaken).ToString(format)

    let getCamera (metadata : Metadata) (args : string list) : string =
        metadata
        |> tryGetDetail Slot.Camera
        |> Option.bind (function | FlexibleValue.String camera -> camera |> Some | _ -> None)
        |> Option.map (fun camera -> match args with | "TitleCase" :: tail -> toTitleCase camera | _ -> camera)
        |> Option.defaultValue "NA"

    let getSeqNo (metadata : Metadata) _ : string =
        metadata
        |> tryGetDetail Slot.SequenceNo
        |> Option.bind (function | FlexibleValue.Numeral seqNo -> seqNo.ToString() |> Some | _ -> None)
        |> Option.defaultValue ""

    let getSubSec (metadata : Metadata) _ : string =
        metadata
        |> tryGetDetail Slot.SubSeconds
        |> Option.bind (function | FlexibleValue.Numeral seqNo -> seqNo.ToString() |> Some | _ -> None)
        |> Option.defaultValue ""

    let getOwner (metadata : Metadata) _ : string =
        metadata
        |> tryGetDetail Slot.Owner
        |> Option.bind (function | FlexibleValue.String owner -> owner |> Some | _ -> None)
        |> Option.defaultValue "NA"

    let getDateExtension (metadata : Metadata) (args : string list) =
        let separator =
            match args with
            | sep :: _ -> sep
            | _ -> "."

        let tryGetAsNumber slot =
            tryGetDetail slot metadata
            |> Option.bind (function | FlexibleValue.Numeral seqNo -> seqNo |> Some | _ -> None)

        tryGetAsNumber Slot.SubSeconds
        |> Option.orElse (tryGetAsNumber Slot.SequenceNo)
        |> Option.map (fun value -> $"{separator}{value}")
        |> Option.defaultValue ""

    let getFilename (metadata : Metadata) _ : string =
        metadata
        |> tryGetDetail "Filename"
        |> Option.bind (function | FlexibleValue.String value -> value |> Some | _ -> None)
        |> Option.defaultValue ":::" // prevent renaming
    
    let getFingerprint (metadata : Metadata) _ : string =
        metadata.Id

    // template engine
    let genericPlaceholderMap : Map<string, Metadata -> string list -> string> =
        Map.ofList
            [
                "date", getDateTaken
                "date-utc", getDateTakenUTC
                "camera", getCamera
                "seqno", getSeqNo
                "subsec", getSubSec
                "owner", getOwner

                // smart placeholder
                "date-ext", getDateExtension

                // manually added metadata, could be missing
                "filename", getFilename
                "fingerprint", getFingerprint
                "id", getFingerprint
            ]

type FilenameCreator ( stringTemplate : StringTemplate ) =
    member _.Apply (metadata : Metadata) : string =
        let placeholderMap =
            FilenameCreatorLib.genericPlaceholderMap
            |> Map.map (fun _ (body : Metadata -> string list -> string) -> body metadata)

        Runtime.EvalRelaxed placeholderMap stringTemplate

    static member Initialize ( namePattern : string ) =
        let stringTemplate = Runtime.Parse namePattern in
        new FilenameCreator(stringTemplate)
