namespace FarNet.FSharp
open FarNet

/// Helper methods dealing with Far Manager windows.
[<AbstractClass; Sealed>]
type Window =
    /// Gets true if the current window is dialog.
    static member IsDialog() =
        far.Window.Kind = WindowKind.Dialog

    /// Gets true if the current window is editor.
    static member IsEditor() =
        far.Window.Kind = WindowKind.Editor

    /// Gets true if the current window is viewer.
    static member IsViewer() =
        far.Window.Kind = WindowKind.Viewer

    /// Gets true if the current window is native panel.
    static member IsNativePanel() =
        far.Window.Kind = WindowKind.Panels && not far.Panel.IsPlugin

    /// Gets true if the current window is module panel.
    static member IsModulePanel() =
        far.Window.Kind = WindowKind.Panels && far.Panel :? Panel
