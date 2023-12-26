namespace OpilioCraft.MetadataEngine.Core

open System
open System.Text.Json.Serialization
open OpilioCraft.FSharp.Prelude

// exceptions
exception MetadataEngineException of ErrorMsg : string
    with override x.Message = $"[Metadata] {x.ErrorMsg}"

// Extended file identification type
type FileIdentificator =
    {
        FileInfo    : IO.FileInfo
        AsOf        : DateTime
        Fingerprint : Fingerprint
    }

// metadata itself
type Metadata =
    {
        Id          : Fingerprint // SHA256 hash
        AsOf        : DateTime // as UTC timestamp
        ContentType : ContentType
        Details     : ContentDetails
    }
        
    [<JsonIgnore>]
    member x.AsOfLocal = x.AsOf.ToLocalTime()

    member x.TryGetDetail key =
        x.Details.TryGetValue(key)
        |> function
            | true, v    -> Some v
            | false, _   -> None

    
// content specification
and ContentType =
    {
        Category      : ContentCategory
        FileExtension : string
    }
    
and ContentCategory =
    | Unspecified = 0
    | Image = 1
    | Movie = 2
    | Digitalized = 3 // digitalized former analogue stuff

// content type specific metadata
and ContentDetails= Collections.Generic.Dictionary<string,FlexibleValue>
