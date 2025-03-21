module OpilioCraft.MetadataEngine.Cmdlets.Defaults

open OpilioCraft.FSharp.Prelude


let FingerprintStrategy = Fingerprint.Strategy.GuessFirst

[<Literal>]
let FilenameTemplate = "{date|yyyyMMddTHHmmss}{date-ext}_{camera}_{owner}"
