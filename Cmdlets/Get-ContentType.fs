namespace OpilioCraft.MetadataEngine.Cmdlets

open System.IO
open System.Management.Automation

open OpilioCraft.FSharp.PowerShell
open OpilioCraft.MetadataEngine.Core

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Get, "ContentType")>]
[<OutputType(typeof<ContentType>)>]
type public GetContentTypeCommand () =
    inherit PathExpectingCommand ()

    // cmdlet behaviour
    override x.ProcessPath path =
        path
        |> FileInfo
        |> MetadataEngine.determineContentType
        |> x.WriteObject        
