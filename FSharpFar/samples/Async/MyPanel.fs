// Simple panel used by the sample wizard.

module MyPanel
open FarNet
open System

type MyFile (obj) =
    inherit FarFile ()
    override __.Name = sprintf "%A" obj
    override __.Description = obj.GetType().FullName

type MyExplorer (items) =
    inherit Explorer (Guid "4c22f997-b124-490c-a2fe-2364d8d51330")
    override __.GetFiles _ =
        upcast (items |> Seq.map (fun x -> MyFile x :> FarFile) |> Seq.toArray)

type MyPanel (explorer) =
    inherit Panel (explorer)
    do
        base.SortMode <- PanelSortMode.Unsorted
        base.ViewMode <- PanelViewMode.Descriptions

let panel items =
    MyPanel (MyExplorer items)
