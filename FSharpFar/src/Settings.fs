
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

namespace FSharpFar

open System
open System.Configuration
open FarNet.Settings

[<SettingsProvider(typedefof<ModuleSettingsProvider>)>]
type Settings () =
    inherit ModuleSettings ()
    static member Default = Settings ()

    [<DefaultSettingValue("Red"); SettingsManageability(SettingsManageability.Roaming); UserScopedSetting>]
    member x.ErrorBackgroundColor
        with get () = x.["ErrorBackgroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["ErrorBackgroundColor"] <- value

    [<DefaultSettingValue("White"); SettingsManageability(SettingsManageability.Roaming); UserScopedSetting>]
    member x.ErrorForegroundColor
        with get () = x.["ErrorForegroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["ErrorForegroundColor"] <- value

    [<DefaultSettingValue("Yellow"); SettingsManageability(SettingsManageability.Roaming); UserScopedSetting>]
    member x.WarningBackgroundColor
        with get () = x.["WarningBackgroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["WarningBackgroundColor"] <- value

    [<DefaultSettingValue("Black"); SettingsManageability(SettingsManageability.Roaming); UserScopedSetting>]
    member x.WarningForegroundColor
        with get () = x.["WarningForegroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["WarningForegroundColor"] <- value
