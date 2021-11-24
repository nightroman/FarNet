[<AutoOpen>]
module FSharpFar.IAnyMenuExt
open FarNet

type IAnyMenu with
    /// Shows the menu of named actions.
    /// items: label * action.
    member menu.ShowActions(items: (string * (unit -> unit)) seq) =
        menu.Items.Clear()
        for text, action in items do
            (menu.Add text).Data <- action
        if menu.Show() then
            (menu.SelectedData :?> (unit -> unit)) ()

    /// Shows the menu of items.
    /// text: Called to get item text.
    /// show: Called to process selected item.
    member menu.ShowItems(text: 'Item -> string) (show: 'Item -> 'r) (items: 'Item seq) =
        menu.Items.Clear()
        for item in items do
            (menu.Add(text item)).Data <- item
        if menu.Show() then
            show (menu.SelectedData :?> 'Item)
        else
            Unchecked.defaultof<'r>

    /// Shows the menu of items with keys.
    /// text: Called to get item text.
    /// show: Called to process selected item and key.
    member menu.ShowItemsWithKeys(text: 'Item -> string) (show: 'Item -> KeyData -> 'r) (items: 'Item seq) =
        menu.Items.Clear()
        for item in items do
            (menu.Add(text item)).Data <- item
        if menu.Show() && menu.Selected >= 0 then
            show (menu.SelectedData :?> 'Item) menu.Key
        else
            Unchecked.defaultof<'r>
