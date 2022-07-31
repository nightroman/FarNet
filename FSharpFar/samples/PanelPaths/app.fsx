
#load "PanelPaths.fs"
open PanelPaths

panelCurrentDirectory ()
|> printfn "Current directory: %s"

panelTryCurrentFilePath ()
|> printfn "Current path: %A"

panelFilePaths ()
|> printfn "File paths: %A"

panelSelectedFilePaths ()
|> printfn "Selected file paths: %A"
