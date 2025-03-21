namespace OpilioCraft.MetadataEngine.Cmdlets

open System
open System.Management.Automation

open OpilioCraft.FSharp.Prelude
open OpilioCraft.FSharp.PowerShell
open OpilioCraft.FSharp.PowerShell.CmdletExtension
open OpilioCraft.MetadataEngine.Core
open OpilioCraft.MetadataEngine.FilesystemExtension
open OpilioCraft.MetadataEngine.RulesExtension

// ------------------------------------------------------------------------------------------------

[<AbstractClass>]
type public DigitalAssetCommand () =
    inherit PathExpectingCommand ()

    // filename creator
    let mutable applyPattern : Metadata -> string = fun metadata -> metadata.Details["Filename"].AsString

    // parameters
    [<Parameter>]
    [<Alias("NamePattern", "NameTemplate")>]
    member val Template = Defaults.FilenameTemplate with get,set

    [<Parameter>]
    member val IncludeFingerprint = SwitchParameter(false) with get,set

    [<Parameter>]
    member val ResetFileDate = SwitchParameter(false) with get,set

    [<Parameter>]
    member val WhatIf = SwitchParameter(false) with get,set

    // cmdlet behaviour
    override x.BeginProcessing() =
        base.BeginProcessing()

        try
            applyPattern <- FilenameCreator.Initialize(x.Template).Apply
            x.WriteVerbose($"Used filename template: {x.Template}")
        with
            | exn -> exn |> x.WriteAsError ErrorCategory.NotSpecified

    override x.ProcessPath path =
        // get metadata
        let metadata =
            MetadataEngine.extractMetadata path
            |> MetadataEngine.addDetail "Filename" (path |> IO.FileInfo |> _.Name)
            |> PredefinedRules.applyMediaRules

        // construct new filename
        let newFilename = String.concat "" <| [
            (applyPattern metadata)
            (if x.IncludeFingerprint.IsPresent then Fingerprint.FingerprintSeparator + metadata.Id else String.Empty)
            metadata.ContentType.FileExtension
        ]

        let targetPath = IO.Path.Combine(x.TargetPath(path), newFilename)

        // run action
        if path.Equals(targetPath) // skip unchanged names
        then
            x.WriteVerbose($"skipping unchanged item {path}")
        else if IO.File.Exists(targetPath)
        then
            x.WriteWarning($"target already exists: {targetPath}")
        else
            x.WriteVerbose($"{x.ActionName} {path} to {targetPath}")
            if not x.WhatIf.IsPresent then x.ItemAction(path, targetPath, false)

        // additional updates
        if x.ResetFileDate.IsPresent && (not x.WhatIf.IsPresent)
        then
            if metadata.Details.ContainsKey(Slot.DateTaken)
            then
                let dateTaken = metadata.Details.["DateTaken"].AsDateTime
                IO.File.SetCreationTimeUtc(targetPath, dateTaken)
                IO.File.SetLastWriteTimeUtc(targetPath, dateTaken)
                IO.File.SetLastAccessTimeUtc(targetPath, dateTaken)
            else
                x.WriteWarning($"cannot reset file date due to missing DateTaken: {path}")

    abstract member ActionName : string with get
    abstract member ItemAction : string * string * bool -> unit
    abstract member TargetPath : string -> string

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Rename, "DigitalAsset")>]
[<OutputType(typeof<unit>)>]
type public RenameDigitalAssetCommand () =
    inherit DigitalAssetCommand ()

    override _.ActionName = "rename"
    override _.ItemAction(sourcePath, targetPath, overwrite) =
        let targetPath = IO.Path.Combine(IO.Path.GetDirectoryName(sourcePath), targetPath)
        IO.File.Copy(sourcePath, targetPath, overwrite)

    override _.TargetPath sourcePath = IO.Path.GetDirectoryName(sourcePath)

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Move, "DigitalAsset")>]
[<OutputType(typeof<unit>)>]
type public MoveDigitalAssetCommand () =
    inherit DigitalAssetCommand ()

    [<Parameter>]
    member val TargetDir = String.Empty with get,set

    override _.ActionName = "move"
    override _.ItemAction(sourcePath, targetPath, overwrite) = IO.File.Move(sourcePath, targetPath, overwrite)
    override x.TargetPath _ = x.TargetDir

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Copy, "DigitalAsset")>]
[<OutputType(typeof<unit>)>]
type public CopyDigitalAssetCommand () =
    inherit DigitalAssetCommand ()

    [<Parameter>]
    member val TargetDir = String.Empty with get,set

    override _.ActionName = "copy"
    override _.ItemAction(sourcePath, targetPath, overwrite) = IO.File.Copy(sourcePath, targetPath, overwrite)
    override x.TargetPath _ = x.TargetDir

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.New, "DigitalAssetLink")>]
[<OutputType(typeof<unit>)>]
type public LinkDigitalAssetCommand () =
    inherit DigitalAssetCommand ()

    // parameters
    [<Parameter>]
    member val TargetDir = String.Empty with get,set

    [<Parameter>]
    [<ValidateSet("Hard", "Symbolic", IgnoreCase=true)>]
    member val LinkType = "Hard" with get,set

    // cmdlet behaviour
    override _.ActionName = "link"

    override x.ItemAction(sourcePath, targetPath, _) =
        match x.LinkType with
        | "Hard" -> LinkHelper.createHardLink sourcePath targetPath
        | "Symbolic" -> LinkHelper.createSymbolicLink sourcePath targetPath
        | _ -> failwith "unsupported link type"

    override x.TargetPath _ = x.TargetDir
