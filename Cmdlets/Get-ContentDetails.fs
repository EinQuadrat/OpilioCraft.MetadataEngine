namespace OpilioCraft.MetadataEngine.Cmdlets

open System
open System.Collections
open System.Management.Automation
open System.Text.RegularExpressions

open OpilioCraft.FSharp.PowerShell
open OpilioCraft.MetadataEngine.Core

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Get, "ContentDetails")>]
[<OutputType(typeof<ContentDetails>, typeof<Hashtable>)>]
type public GetItemDetailsCommand () =
    inherit PathExpectingCommand ()

    // predicate used for filtering details
    let mutable predicate : (string -> bool) = fun _ -> true

    // cmdlet params
    [<Parameter(Position=1)>]
    member val Name = String.Empty with get, set // convention: empty string matches all

    [<Parameter>]
    member val ExactMatch = SwitchParameter(false) with get, set

    [<Parameter>]
    member val AsHashtable = SwitchParameter(false) with get, set

    // cmdlet funtionality
    override x.BeginProcessing() =
        base.BeginProcessing()

        // any filter specified?
        if not <| String.IsNullOrEmpty x.Name
        then
            if x.ExactMatch.IsPresent
            then
                predicate <- fun key -> key.Equals(x.Name)
            else
                let compiledRegex = Regex(x.Name, RegexOptions.Compiled) in
                predicate <- fun key -> compiledRegex.IsMatch(key)

    override x.ProcessPath path =
        path
        |> MetadataEngine.extractMetadata
        |> fun metadata -> metadata.Details

        |> Seq.filter (fun kv -> predicate kv.Key)

        |>
            if x.AsHashtable.IsPresent // transformation needed?
            then
                Seq.fold (fun (ht : Hashtable) item -> ht.Add(item.Key, item.Value.Unwrap); ht) (Hashtable())
                >> x.WriteObject // output is Hashtable
            else
                x.WriteObject // output is ItemDetails
