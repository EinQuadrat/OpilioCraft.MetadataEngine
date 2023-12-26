module OpilioCraft.MetadataEngine.RulesAddon.Snippets

//// load user settings on demand
//let private assertRuleFileExists ruleName ruleDefinitionFile =
//    if not <| File.Exists ruleDefinitionFile
//    then
//        raise <| InvalidUserSettingsException(Settings.FrameworkConfigFile, $"definition file of rule {ruleName} does not exist")


//let verifyFrameworkConfig config =
//    config
//    |> Verify.isVersion Settings.FrameworkVersion

//    |> UserSettings.tryGetProperty "Rules"
//    |> Option.iter (Map.iter assertRuleFileExists)

//    config
