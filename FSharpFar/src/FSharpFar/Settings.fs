namespace FSharpFar
open FarNet
open System
open System.Xml.Serialization
open System.ComponentModel

[<XmlRoot("Data")>]
type SettingsData() =
    member val ErrorBackgroundColor = ConsoleColor.Red with get, set
    member val ErrorForegroundColor = ConsoleColor.White with get, set
    member val WarningBackgroundColor = ConsoleColor.Yellow with get, set
    member val WarningForegroundColor = ConsoleColor.Black with get, set

type Settings() =
    inherit ModuleSettings<SettingsData>()
    static member val Default = Settings()

[<XmlRoot("Data")>]
type WorkingsData() =
    member val AutoCheck = true with get, set
    member val AutoTips = true with get, set

[<Browsable(false)>]
type Workings() =
    inherit ModuleSettings<WorkingsData>(ModuleSettingsArgs(IsLocal = true))
    static member val Default = Workings()
