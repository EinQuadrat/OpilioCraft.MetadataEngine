namespace OpilioCraft.MetadataEngine.Cmdlets

open System
open System.Management.Automation

open OpilioCraft.FSharp.PowerShell
open OpilioCraft.MetadataEngine
open OpilioCraft.MetadataEngine.RulesExtension

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsDiagnostic.Repair, "FileDate")>]
[<OutputType(typeof<unit>)>]
type public RepairFileDateCommand() =
    inherit PathExpectingCommand()

    // cmdlet behaviour
    override x.ProcessPath(path) =
        // get metadata
        let metadata =
            MetadataEngine.extractMetadata path
            |> PredefinedRules.applyMediaRules

        // correct file timestamps
        if metadata.Details.ContainsKey(Slot.DateTaken)
            then
                let dateTaken = metadata.Details.["DateTaken"].AsDateTime
                IO.File.SetCreationTimeUtc(path, dateTaken)
                IO.File.SetLastWriteTimeUtc(path, dateTaken)
                IO.File.SetLastAccessTimeUtc(path, dateTaken)
            else
                x.WriteWarning($"cannot reset file date due to missing DateTaken: {path}")
