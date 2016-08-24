
// FarNet module FSharpFar
// Copyright (c) 2016 Roman Kuzmin

[<AutoOpen>]
module FSharpFar.FarKit

open FarNet
open System

let getFsfLocalData() = Far.Api.GetModuleManager("FSharpFar").GetFolderPath(SpecialFolder.LocalData, true)
