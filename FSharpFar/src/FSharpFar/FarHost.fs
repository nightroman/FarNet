namespace FSharpFar
open FarNet

type FarHost() =
    inherit ModuleHost()

    do
        // on saving configs, notify sessions
        far.AnyEditor.Saving.Add <| fun e ->
            if String.endsWithIgnoreCase e.FileName ".fs.ini" then
                Session.OnSavingConfig e.FileName
