namespace OpilioCraft.MetadataEngine.RulesExtension

open System.IO

open OpilioCraft.FSharp.Prelude
open OpilioCraft.Lisp

open OpilioCraft.MetadataEngine.Core

// ------------------------------------------------------------------------------------------------

exception CannotLoadRuleException of Path:string * ErrorMessage:string
    with override x.ToString () = $"cannot load rule from file {x.Path}"

exception UnknownRuleException of Name:string
    with override x.ToString () = $"no preloaded rule of name {x.Name}"

exception InvalidRuleDefinitionException of Path:string
    with override x.ToString () = $"rule defined in file {x.Path} is not valid"

exception UnexpectedRuleResultException of Name:string * Result:obj
    with override x.ToString () = $"rule {x.Name} returned an unexpected result type: {x.Result.GetType().FullName}"

// ------------------------------------------------------------------------------------------------

module RulesProvider =
    let private lispRuntime = LispRuntime.Initialize().InjectResultHook(ContentDetailHelper.unwrapContentDetail)

    // --------------------------------------------------------------------------------------------

    let tryLoadRule (pathToRuleDefinition : string) : Result<Expression, string> =
        if File.Exists pathToRuleDefinition
        then
            lispRuntime.LoadFile pathToRuleDefinition
            |> lispRuntime.ParseWithResult
        else
            Error($"rule file does not exist: {pathToRuleDefinition}")

    let loadRule (pathToRuleDefinition : string) : Expression =
        tryLoadRule pathToRuleDefinition
        |> Result.defaultWith( fun err -> raise <| CannotLoadRuleException(pathToRuleDefinition, err) )

    // --------------------------------------------------------------------------------------------
    
    let private preloadRules rulesLocation : Map<string, Expression> =
        if Directory.Exists rulesLocation
        then
            Directory.EnumerateFiles(rulesLocation, "*.lisp")
            |> Seq.map (fun file -> Path.GetFileNameWithoutExtension(file), file)
            |> Seq.map (fun (name, path) -> name, loadRule path)
            |> Map.ofSeq
        else
            Map.empty

    let private rules = preloadRules Settings.RulesLocation

    // --------------------------------------------------------------------------------------------

    let tryGetRule name =
        rules
        |> Map.tryFind name

    let getRule name =
        tryGetRule name
        |> Option.defaultWith( fun _ -> raise <| UnknownRuleException(name) )

    // --------------------------------------------------------------------------------------------
    
    let tryApplyRule data rule : Result<FlexibleValue, string> =
        lispRuntime.InjectObjectData(data).EvalWithResult rule
        |> function
            | Ok (Atom fval) -> Ok(fval)
            | Ok _ -> Error $"rule did not return an atom"
            | Error err -> Error err

    let verifyAsString =
        Option.map (
            function
            | FlexibleValue.String stringValue -> stringValue
            | _ -> failwith $"rule did not return a string value"
        )
