module Module
open FarNet
open TryPanelFSharp

/// The module menu item: F11 \ TryPanelFSharp
[<ModuleTool(Name = "TryPanelFSharp", Options = ModuleToolOptions.Panels)>]
[<Guid "d6765565-4c52-4877-aac6-3db3e0c88b62">]
type MyTool() =
    inherit ModuleTool()
    override _.Invoke(_, _) = run ()

/// The module command "TryPanelFSharp:"
[<ModuleCommand(Name = "TryPanelFSharp", Prefix = "TryPanelFSharp")>]
[<Guid "fe7cbfed-6224-4811-8b7e-565b79ee73c0">]
type MyCommand() =
    inherit ModuleCommand()
    override _.Invoke(_, _) = run ()
