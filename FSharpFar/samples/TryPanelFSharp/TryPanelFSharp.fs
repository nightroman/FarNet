module TryPanelFSharp
open FarNet
open System

/// Demo explorer which creates and deletes virtual files.
type MyExplorer() =
    inherit Explorer(Guid "ff294f25-b6d4-4eba-a1bc-a37fff6f55bb")
    let _files = ResizeArray<FarFile>()
    do
        base.CanCreateFile <- true
        base.CanDeleteFiles <- true
        _files.Add(SetFile(Name = "Add [F7]; Remove [Del]/[F8]", Description = "demo file"))

    override _.GetFiles _ =
        upcast _files

    override _.CreateFile args =
        let name = args.Data :?> string
        _files.Add(SetFile(Name = name, Description = "demo file"))
        args.PostName <- name

    override _.DeleteFiles args =
        for file in args.Files do
            _files.RemoveAll(fun x -> x.Name = file.Name) |> ignore

/// Demo panel with some user interaction.
type MyPanel(explorer) =
    inherit Panel(explorer)
    do
        base.SortMode <- PanelSortMode.FullName
        base.ViewMode <- PanelViewMode.Descriptions

    override _.UICreateFile args =
        let name = far.Input("File name", null, "MyPanel")
        if not (String.IsNullOrEmpty name) then
            args.Data <- name
            base.UICreateFile args

    override _.UIDeleteFiles args =
        if 0 = far.Message("Delete files?", "MyPanel", MessageOptions.OkCancel) then
            base.UIDeleteFiles args

/// Opens the panel.
let run () = MyPanel(MyExplorer()).Open()
