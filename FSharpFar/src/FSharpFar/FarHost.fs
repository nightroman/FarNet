namespace FSharpFar
open FarNet

[<ModuleHost>]
type FarHost () =
    inherit ModuleHost ()

    override __.Connect () =
        // on saving configs, notify sessions
        far.AnyEditor.Saving.Add <| fun e ->
            if String.endsWithIgnoreCase e.FileName ".fs.ini" then
                Session.OnSavingConfig e.FileName
