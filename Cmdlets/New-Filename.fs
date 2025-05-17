namespace OpilioCraft.MetadataEngine.Cmdlets

open System
open System.Management.Automation

open OpilioCraft.FSharp.Prelude
open OpilioCraft.FSharp.PowerShell
open OpilioCraft.FSharp.PowerShell.CmdletExtension
open OpilioCraft.MetadataEngine
open OpilioCraft.MetadataEngine.FilesystemExtension
open OpilioCraft.MetadataEngine.RulesExtension

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.New, "Filename")>]
[<OutputType(typeof<string>)>]
type public NewFilenameCommand() =
    inherit PathExpectingCommand()

    // filename creator
    let mutable applyPattern : Metadata -> string = fun metadata -> metadata.Details["Filename"].AsString

    [<Parameter>]
    [<Alias("NamePattern", "NameTemplate")>]
    member val Template = Defaults.FilenameTemplate with get,set

    [<Parameter>]
    member val IncludeFingerprint = SwitchParameter(false) with get,set

    // cmdlet behaviour
    override x.BeginProcessing() =
        base.BeginProcessing()

        try
            applyPattern <- FilenameCreator.Initialize(x.Template).Apply
            x.WriteVerbose($"Used filename template: {x.Template}")
        with
            | exn -> x.WriteAsError(ErrorCategory.NotSpecified, exn)

    override x.ProcessPath(path) =
        // get metadata
        let metadata =
            MetadataEngine.extractMetadata path
            |> MetadataEngine.addDetail "Filename" (path |> IO.FileInfo |> _.Name)
            |> PredefinedRules.applyMediaRules

        // construct new filename
        String.concat "" <| [
            (applyPattern metadata)
            (if x.IncludeFingerprint.IsPresent then Fingerprint.FingerprintSeparator + metadata.Id else String.Empty)
            metadata.ContentType.FileExtension
        ]

        |> x.WriteObject
