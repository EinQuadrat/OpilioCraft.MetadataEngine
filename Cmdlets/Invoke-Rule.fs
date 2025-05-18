namespace OpilioCraft.MetadataEngine.Cmdlets

open System
open System.Management.Automation

open OpilioCraft.FSharp.FlexibleValues
open OpilioCraft.FSharp.PowerShell.CmdletExtension
open OpilioCraft.Lisp

open OpilioCraft.MetadataEngine
open OpilioCraft.MetadataEngine.RulesExtension

// -------------------------------------------------------------------------------------------

// represents status of rules engine
type private RuleEvaluator =
    | NotInitialized
    | Initialized of Evaluator:(obj -> FlexibleValue option)

// cmdlet
[<Cmdlet(VerbsLifecycle.Invoke, "Rule", DefaultParameterSetName="PredefinedRule")>]
[<OutputType(typeof<FlexibleValue>, typeof<String>)>]
type public InvokeItemRuleCommand() =
    inherit PSCmdlet()

    // rule
    let mutable ruleEvaluator = NotInitialized
    
    // cmdlet params
    [<Parameter(Position=0, Mandatory=true, ValueFromPipeline=true)>]
    member val InputObject : obj = String.Empty with get,set

    [<Parameter(ParameterSetName="PredefinedRule", Mandatory=true)>]
    member val LoadRule = String.Empty with get,set

    [<Parameter(ParameterSetName="CustomRule", Mandatory=true)>]
    [<ValidateNotNullOrEmpty>]
    member val Rule = String.Empty with get,set // LISP expression

    [<Parameter>]
    member val AsString = SwitchParameter(false) with get,set

    [<Parameter>]
    [<ValidateNotNull>]
    [<AllowEmptyString>]
    member val DefaultValue = "NA" with get,set

    // cmdlet funtionality
    override x.BeginProcessing() =
        base.BeginProcessing()

        try
            let lispRuntime = LispRuntime.Initialize().InjectResultHook(ContentDetailHelper.unwrapContentDetail)
            let lispExpr =
                match x.ParameterSetName with
                | "PredefinedRule" ->
                    RulesProvider.tryGetRule(x.LoadRule)
                    |> Option.orElseWith (fun _ -> RulesProvider.tryLoadRule(x.LoadRule) |> Result.toOption)
                    |> Option.defaultWith (fun _ -> failwith $"cannot load rule {x.LoadRule}")

                | "CustomRule" ->
                    let lispRuntime = LispRuntime.Initialize().InjectResultHook(ContentDetailHelper.unwrapContentDetail) in

                    lispRuntime.TryParse(x.Rule)
                    |> Option.defaultWith (fun _ -> failwith $"cannot compile rule {x.Rule}")
                
                | _ -> failwith "[FATAL] unexpected ParameterSetName"


            ruleEvaluator <- Initialized (
                fun data ->
                    lispRuntime.InjectObjectData(data).TryEval(lispExpr)
                    |> Option.bind ( function | Atom fval -> Some fval | _ -> None )
                )

        with
            | exn -> x.ThrowAsTerminatingError(ErrorCategory.ResourceUnavailable, exn)

    override x.ProcessRecord() =
        base.ProcessRecord()

        try
            let result =
                match ruleEvaluator with
                | NotInitialized -> failwith "[FATAL] rule could not be initialized"
                | Initialized evaluator ->
                    match x.InputObject with
                    | :? PSObject as psObj -> psObj.BaseObject // automatically unwrap PSObject
                    | anObj -> anObj
                    |> evaluator
            
                |> Option.defaultValue (x.DefaultValue |> FlexibleValue.String)

            if x.AsString.IsPresent
            then
                result.ToString() |> x.WriteObject
            else
                result |> x.WriteObject

        with
            | exn -> x.WriteAsError(ErrorCategory.NotSpecified, exn)
