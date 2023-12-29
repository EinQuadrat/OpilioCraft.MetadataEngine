namespace OpilioCraft.MetadataEngine.Cmdlets

open System.Management.Automation

open OpilioCraft.MetadataEngine.Core
open OpilioCraft.MetadataEngine.RulesExtension

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Set, "MetadataEngine", DefaultParameterSetName="Configure")>]
[<OutputType(typeof<unit>)>]
type public SetMetadataEngineCommand () =
    inherit PSCmdlet ()

    // parameters
    [<Parameter(ParameterSetName="Initialize")>]
    member val PreloadExifTool = SwitchParameter(false) with get, set

    [<Parameter(ParameterSetName="Configure")>]
    member val ReloadRules = SwitchParameter(false) with get, set

    [<Parameter(ParameterSetName="Cleanup")>]
    member val Cleanup = SwitchParameter(false) with get,set

    // cmdlet behaviour
    override x.BeginProcessing() =
        base.BeginProcessing()

        match x.ParameterSetName with
        | "Initialize" ->
            if x.PreloadExifTool.IsPresent
            then
                MetadataEngine.preloadExifTool() // multiple calls are harmless

        | "Configure" ->
            if x.ReloadRules.IsPresent
            then
                RulesProvider.reloadPredefinedRules()

        | "Cleanup" ->
            if x.Cleanup.IsPresent
            then
                MetadataEngine.freeResources()

        | _ -> ignore()
