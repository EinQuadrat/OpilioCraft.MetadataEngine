namespace OpilioCraft.MetadataEngine

open System
open System.IO

open FSharp.Data

open OpilioCraft.FSharp
open OpilioCraft.FSharp.FlexibleValues
open OpilioCraft.FSharp.ActivePatterns


[<RequireQualifiedAccess>]
module MetadataEngine =
    // defaults
    let private ResourceExifTool = "ExifTool"

    // managed resources
    let mutable private usedResources : Map<string, IDisposable> = Map.empty

    // resource management
    let preloadExifTool () =
        if not <| usedResources.ContainsKey(ResourceExifTool)
        then
            usedResources <- usedResources |> Map.add ResourceExifTool (ExifTool.Proxy())

    let freeResources () =
        usedResources |> Map.iter (fun _ disposable -> disposable.Dispose())
        usedResources <- Map.empty

    // --------------------------------------------------------------------------------------------

    let private transformExifResult (exif: ExifToolResult) : Map<string, FlexibleValue> =
        let transformJsonValue (nameAsHint: string) jsonValue : FlexibleValue =
            match jsonValue with
            | JsonValue.Boolean x -> FlexibleValue.Boolean x
            | JsonValue.Number x -> FlexibleValue.Decimal x
            | JsonValue.Float x -> FlexibleValue.Float x
    
            | JsonValue.String x -> // we use the nameAsHint to identify DateTime tags more reliable
                match x with
                | IsDateTime x when nameAsHint.Contains("Date") -> FlexibleValue.DateTime x
                | stringValue -> stringValue.Trim() |> FlexibleValue.String
        
            // container items are flattened to a string
            | JsonValue.Array x ->
                seq { for item in jsonValue -> item.AsString().Trim() }
                |> String.concat ", "
                |> fun flattenedArray -> FlexibleValue.String $"#ARRAY# [ {flattenedArray} ]"
            
            | JsonValue.Record x ->
                seq { for (prop, item) in jsonValue.Properties() -> $"{prop} = {item.AsString().Trim()}" }
                |> String.concat ", "
                |> fun flattenedRecord -> FlexibleValue.String $"#RECORD# {{ {flattenedRecord} }}"
        
            // error handling
            | _ -> failwith $"[MetadataEngine] unexpected JsonValue of type \"{jsonValue.GetType().Name}\""
    
        exif.ParsedJson.Properties()
        |> Array.fold ( fun map (name, value) -> (transformJsonValue name value, map) ||> Map.add name ) Map.empty
    
    // --------------------------------------------------------------------------------------------

    let identifyFile (fi: FileInfo) =
        {
            FileInfo = fi
            AsOf = fi.LastWriteTimeUtc
            Fingerprint = fi.FullName |> Fingerprint.fingerprintAsString
        }

    let determineContentType (fi : FileInfo) : ContentType =
        let fileExt = fi.Extension.ToLower()

        let category =        
            match fileExt with
            | Match("^(\.(arw|jpe?g|tiff?|gif))$") _    -> ContentCategory.Image
            | Match("^(\.(mov|mp4|mts))$") _            -> ContentCategory.Movie
            | _                                         -> ContentCategory.Unspecified
    
        {
            Category = category
            FileExtension = fileExt
        }

    let contentCategory (fi : FileInfo) = (determineContentType fi).Category

    let determineCategorySpecificDetails (fi : FileInfo) (contentCategory : ContentCategory) : ContentDetails =
        let result = new ContentDetails()
    
        match contentCategory with
        | ContentCategory.Image
        | ContentCategory.Movie ->
            match fi |> ExifToolHelper.getMetadata with
            | Some exif ->
                result.Add(Slot.ExifTool, true |> FlexibleValue.Boolean) // indicate that we have EXIF data
                for item in exif |> transformExifResult do result.Add($"{SlotPrefix.ExifTool}{item.Key}", item.Value)
    
                exif
                |> ExifToolHelper.tryExtractCamera
                |> Option.map FlexibleValue.String
                |> Option.iter ( fun camera -> result.Add(Slot.Camera, camera) )
    
            | _ -> ignore ()
    
        | _ -> ignore ()
    
        result
    
    // --------------------------------------------------------------------------------------------

    let extractMetadata (filePath: string) : Metadata =
        let fid = filePath |> FileInfo |> identifyFile
        let contentType = fid.FileInfo |> determineContentType

        {
            Id = fid.Fingerprint
            AsOf = fid.AsOf
            ContentType = contentType
            Details = determineCategorySpecificDetails fid.FileInfo contentType.Category
        }

    // --------------------------------------------------------------------------------------------

    let addDetail key value metadata =
        metadata.Details.Add(key, FlexibleValue.Wrap <| value)
        metadata

    let addDetailIfMissing key (valueProvider: Metadata -> _) metadata =
        metadata |>
            if not <| metadata.Details.ContainsKey(key)
            then
                let value = valueProvider metadata in
                addDetail key value
            else
                id
