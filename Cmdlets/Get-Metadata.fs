namespace OpilioCraft.MetadataEngine.Cmdlets

open System.Management.Automation

open OpilioCraft.FSharp.PowerShell
open OpilioCraft.MetadataEngine

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Get, "Metadata")>]
[<OutputType(typeof<Metadata>)>]
type public GetMetadataCommand() =
    inherit PathExpectingCommand()

    // cmdlet funtionality
    override x.ProcessPath(path) =
        path
        |> MetadataEngine.extractMetadata
        |> x.WriteObject
