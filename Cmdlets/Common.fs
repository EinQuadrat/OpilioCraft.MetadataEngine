module OpilioCraft.MetadataEngine.Cmdlets.Common

let inline tee f x = (f x) ; x
let inline teeIf cond f x = (if cond then f x) ; x
let inline teeP p f x = (if p x then f x) ; x

