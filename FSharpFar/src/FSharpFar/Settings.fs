namespace FSharpFar
open FarNet
open System
open System.Xml.Serialization

[<Serializable>]
[<XmlRoot("Data")>]
type SettingsData() =
    member val ErrorBackgroundColor = ConsoleColor.Red with get, set
    member val ErrorForegroundColor = ConsoleColor.White with get, set
    member val WarningBackgroundColor = ConsoleColor.Yellow with get, set
    member val WarningForegroundColor = ConsoleColor.Black with get, set

type Settings() =
    inherit ModuleSettings<SettingsData>()
    static member val Default = Settings()
