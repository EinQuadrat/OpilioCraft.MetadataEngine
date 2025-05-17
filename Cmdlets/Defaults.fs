namespace OpilioCraft.MetadataEngine.Cmdlets

open OpilioCraft.FSharp.Prelude

module Defaults =
    let FingerprintStrategy = Fingerprint.Strategy.GuessFirst

    [<Literal>]
    let FilenameTemplate = "{date|yyyyMMddTHHmmss}{date-ext}_{camera}_{owner}"
