module PanelEditFile
open FarNet
open System

// Demo explorer with files which can be edited.
type MyExplorer() =
    inherit Explorer(Guid "d89eb9e3-807d-4190-bcad-b2acc53d202f")
    let files = [
        SetFile(Name="Euler's number (e)", Description="2.71828")
        SetFile(Name="Archimedes' constant (π)", Description="3.14159")
    ]
    do
        base.CanGetContent <- true
        base.CanSetText <- true

    override x.GetFiles _ =
        files |> Seq.cast

    override x.GetContent args =
        args.CanSet <- true
        args.UseText <- args.File.Description

    override x.SetText args =
        args.File.Description <- args.Text |> float |> string

// Demo panel.
type MyPanel(explorer) =
    inherit Panel(explorer)
    do
        base.SortMode <- PanelSortMode.Unsorted
        base.ViewMode <- PanelViewMode.Descriptions

    override x.UISetText args =
        base.UISetText(args)
        x.Update(true)

// Opens the panel.
let run () = MyPanel(MyExplorer()).Open()
