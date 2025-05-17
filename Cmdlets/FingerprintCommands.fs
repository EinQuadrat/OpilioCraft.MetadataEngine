namespace OpilioCraft.MetadataEngine.Cmdlets

open System
open System.Management.Automation

open OpilioCraft.FSharp.Prelude
open OpilioCraft.FSharp.Prelude.FingerprintExtension
open OpilioCraft.FSharp.PowerShell

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Get, "Fingerprint")>]
[<OutputType(typeof<QualifiedFingerprint>, typeof<string>)>]
type public GetFingerprintCommand() =
    inherit PathExpectingCommand()

    // strategy to determine fingerprints
    member val FingerprintStrategy = Defaults.FingerprintStrategy with get, set        

    // user option to influence used fingerprint strategy
    [<Parameter>]
    member val Strategy : string = Defaults.FingerprintStrategy.ToString() with get, set

    // to request retrieving fingerprint as string
    [<Parameter>]
    member val AsString = SwitchParameter(false) with get, set

    // validations
    override x.BeginProcessing() =
        base.BeginProcessing()

        // validate specified fingerprint strategy
        x.FingerprintStrategy <- x.Strategy |> Assert.isValidFingerprintStrategy

    // cmdlet behaviour
    override x.ProcessPath(path) =
        let qualifiedFingerprint = path |> Fingerprint.getFingerprint x.FingerprintStrategy in
        
        if x.AsString.IsPresent
        then
                x.WriteObject(qualifiedFingerprint.Value)
        else
                x.WriteObject(qualifiedFingerprint)

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsDiagnostic.Test, "Fingerprint")>]
[<OutputType(typeof<bool>, typeof<DetailedResult>)>]
type public TestFingerprintCommand() =
    inherit PathExpectingCommand()

    [<Parameter()>]
    member val IgnoreMissing = SwitchParameter(false) with get, set

    [<Parameter()>]
    member val Detailed = SwitchParameter(false) with get, set

    // cmdlet behaviour
    override x.ProcessPath(path) =
        let testResult =
            match Fingerprint.tryGuessFingerprint path with
            | Some(fingerprint) -> fingerprint = (Fingerprint.fingerprintAsString path)
            | None when x.IgnoreMissing.IsPresent -> true
            | None -> x.WriteWarning($"Path does not contain a fingerprint: {path}"); false

        if x.Detailed.IsPresent
        then
            x.WriteObject({ Path = path; TestResult = testResult })
        else
            x.WriteObject(testResult)

and DetailedResult =
    {
        Path : string
        TestResult : bool
    }

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsData.Update, "Fingerprint")>]
[<OutputType(typeof<Void>)>]
type public UpdateFingerprintCommand() =
    inherit PathExpectingCommand() // disable support for fingerprint strategy

    // cmdlet behaviour
    override _.ProcessPath(path) =
        let fingerprint = path |> Fingerprint.fingerprintAsString
        let augmentedPath = IO.Path.InjectFingerprint(path, fingerprint)

        if not <| path.Equals(augmentedPath)
        then
            IO.File.Move(path, augmentedPath)

// ------------------------------------------------------------------------------------------------

[<Cmdlet(VerbsCommon.Remove, "Fingerprint")>]
[<OutputType(typeof<Void>)>]
type public RemoveFingerprintCommand() =
    inherit PathExpectingCommand() // disable support for fingerprint strategy

    // cmdlet behaviour
    override _.ProcessPath(path) =
        let directory = IO.Path.GetDirectoryName(path)
        let plainFilename = IO.Path.GetFilenameWithoutFingerprint(path)
        let extension = IO.Path.GetExtension(path)
        let cleanPath = IO.Path.Combine(directory, $"{plainFilename}{extension}")

        if not <| path.Equals(cleanPath)
        then
            IO.File.Move(path, cleanPath)
