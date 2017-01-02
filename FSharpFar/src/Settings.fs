
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

    [<UserScopedSetting>]
    [<DefaultSettingValue("Red")>]
    [<SettingsManageability(SettingsManageability.Roaming)>]
    member x.ErrorBackgroundColor
        with get () = x.["ErrorBackgroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["ErrorBackgroundColor"] <- value

    [<UserScopedSetting>]
    [<DefaultSettingValue("White")>]
    [<SettingsManageability(SettingsManageability.Roaming)>]
    member x.ErrorForegroundColor
        with get () = x.["ErrorForegroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["ErrorForegroundColor"] <- value

    [<UserScopedSetting>]
    [<DefaultSettingValue("Yellow")>]
    [<SettingsManageability(SettingsManageability.Roaming)>]
    member x.WarningBackgroundColor
        with get () = x.["WarningBackgroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["WarningBackgroundColor"] <- value

    [<UserScopedSetting>]
    [<DefaultSettingValue("Black")>]
    [<SettingsManageability(SettingsManageability.Roaming)>]
    member x.WarningForegroundColor
        with get () = x.["WarningForegroundColor"] :?> ConsoleColor
        and set (value: ConsoleColor) = x.["WarningForegroundColor"] <- value
