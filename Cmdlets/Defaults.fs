namespace OpilioCraft.MetadataEngine.Cmdlets

open OpilioCraft.FSharp

module Defaults =
    let FingerprintStrategy = Fingerprint.FingerprintStrategy.GuessFirst

    [<Literal>]
    let FilenameTemplate = "{date|yyyyMMddTHHmmss}{date-ext}_{camera}_{owner}"
