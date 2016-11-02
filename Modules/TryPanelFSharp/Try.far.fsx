
(*
    This script for FarNet.FSharpFar lets to test the panel without building
    and installing the module and restarting Far Manager after code changes.

    fs: //exec file = ...\Try.far.fsx
*)

#load "TryPanelFSharp.fs"
open TryPanelFSharp

MyPanel(MyExplorer()).Open()
