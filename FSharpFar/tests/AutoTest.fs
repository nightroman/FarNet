/// Common test helpers.
[<AutoOpen>]
module AutoTest
open FarNet
open FarNet.FSharp

let isWizard () =
    Window.IsDialog () && far.Dialog.[0].Text = "Wizard"

let isError () =
    Window.IsDialog () && far.Dialog.[0].Text = "Exception" && far.Dialog.[1].Text = "Oh"

let isMyPanel () =
    Window.IsModulePanel () && ((far.Panel :?> Panel).Title = "MyPanel")

let showWideDialog () =
    far.Message "relatively_long_text_message_for_relatively_wide_dialog"

let isWideDialog () =
    Window.IsDialog () && far.Dialog.[1].Text = "relatively_long_text_message_for_relatively_wide_dialog"
