// Object panel used by the sample wizard.

module MyPanel
open FarNet
open System

type MyFile (obj) =
    inherit FarFile ()
    override _.Name = sprintf "%A" obj
    override _.Description = obj.GetType().FullName

type MyExplorer (items) =
    inherit Explorer (Guid "4c22f997-b124-490c-a2fe-2364d8d51330")
    override _.GetFiles _ =
        items |> Seq.map MyFile |> Seq.cast
    override x.CreatePanel () =
        Panel(x, Title="Objects", SortMode=PanelSortMode.Unsorted, ViewMode=PanelViewMode.Descriptions)

let panel items =
    (MyExplorer items).CreatePanel()
