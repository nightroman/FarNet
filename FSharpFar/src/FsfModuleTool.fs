
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

module FSharpFar.FsfModuleTools

open FarNet
open Interactive
open System

[<System.Runtime.InteropServices.Guid("65bd5625-769a-4253-8fde-ffcc3f72489d")>]
[<ModuleTool(Name = "FSharpFar", Options = ModuleToolOptions.AllMenus)>]
type FsfModuleTool() =
    inherit ModuleTool()
    override x.Invoke(sender, e) =
        let menu = Far.Api.CreateMenu()
        menu.Title <- "FSharpFar"
        menu.Add("&1. Interactive [1]") |> ignore
        menu.Add("&2. Interactive new") |> ignore
        if menu.Show() then
            match menu.Selected with
            | 0 -> openInteractive1()
            | _ -> openInteractive2()
