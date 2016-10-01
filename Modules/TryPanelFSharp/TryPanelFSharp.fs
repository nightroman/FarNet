
module TryPanelFSharp
open FarNet
open System

/// Demo explorer which creates and deletes virtual files.
type MyExplorer () =
    inherit Explorer (Guid "ff294f25-b6d4-4eba-a1bc-a37fff6f55bb")
    let _files = ResizeArray<FarFile> ()
    do
        base.CanCreateFile <- true
        base.CanDeleteFiles <- true
        _files.Add (SetFile (Name = "Add [F7]; Remove [Del]/[F8]", Description = "demo file"))
    override x.GetFiles args =
        upcast _files
    override x.CreateFile args =
        let name = args.Data :?> string
        _files.Add (SetFile (Name = name, Description = "demo file"))
        args.PostName <- name
    override x.DeleteFiles args =
        for file in args.Files do
            _files.RemoveAll (fun x -> x.Name = file.Name) |> ignore

/// Demo panel with some user interaction.
type MyPanel (explorer) =
    inherit Panel (explorer)
    do
        base.SortMode <- PanelSortMode.FullName
        base.ViewMode <- PanelViewMode.Descriptions
    override x.UICreateFile args =
        let name = Far.Api.Input ("File name", null, "MyPanel")
        if not (String.IsNullOrEmpty name) then
            args.Data <- name
            base.UICreateFile args
    override x.UIDeleteFiles args =
        if 0 = Far.Api.Message ("Delete files?", "MyPanel", MessageOptions.OkCancel) then
            base.UIDeleteFiles args

/// Demo tool with an item in the plugin menu.
[<System.Runtime.InteropServices.Guid "d6765565-4c52-4877-aac6-3db3e0c88b62">]
[<ModuleTool (Name = "TryPanelFSharp", Options = ModuleToolOptions.Panels)>]
type MyTool () =
    inherit ModuleTool ()
    override x.Invoke (sender, e) = MyPanel(MyExplorer()).Open ()
