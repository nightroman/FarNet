namespace FSharpFar
open FarNet
open System

[<ModuleHost>]
type FarHost() =
    inherit ModuleHost()

    override __.Connect() =
        Environment.SetEnvironmentVariable("$Version", Environment.Version.ToString());

        // on saving configs, notify sessions
        far.AnyEditor.Saving.Add <| fun e ->
            if String.endsWithIgnoreCase e.FileName ".fs.ini" then
                Session.OnSavingConfig e.FileName
