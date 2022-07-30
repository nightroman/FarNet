namespace FSharpFar
open FarNet
open System
open System.Xml.Serialization

[<XmlRoot("Data")>]
type SettingsData() =
    member val AutoCheck = false with get, set
    member val ErrorBackgroundColor = ConsoleColor.Red with get, set
    member val ErrorForegroundColor = ConsoleColor.White with get, set
    member val WarningBackgroundColor = ConsoleColor.Yellow with get, set
    member val WarningForegroundColor = ConsoleColor.Black with get, set

type Settings() =
    inherit ModuleSettings<SettingsData>()
    static member val Default = Settings()
