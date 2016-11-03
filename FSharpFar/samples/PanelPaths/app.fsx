
#load "PanelPaths.fs"
open PanelPaths

panelCurrentDirectory ()
|> printfn "Current directory: %s"

panelTryCurrentFilePath ()
|> printfn "Current path: %A"

panelShownFilePaths ()
|> printfn "Shown paths: %A"

panelSelectedFilePaths ()
|> printfn "Selected paths: %A"
