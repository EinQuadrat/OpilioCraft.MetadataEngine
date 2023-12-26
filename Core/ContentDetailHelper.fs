module OpilioCraft.MetadataEngine.Core.ContentDetailHelper

open OpilioCraft.FSharp.Prelude

// needed to support ObjectPath expression for ItemDetail objects
let unwrapContentDetail (incoming : 'a) : obj =
    match box incoming with
    | :? FlexibleValue as detail -> detail.Unwrap
    | otherwise -> otherwise
