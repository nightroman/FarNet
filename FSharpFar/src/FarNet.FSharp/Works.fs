/// FarNet FSharp tools available on `open FarNet`.
[<AutoOpen>]
module FarNet.Works

/// The FarNet.Far.Api instance.
let far = Far.Api

/// The Guid attribute shortcut.
[<System.Obsolete("Use module item attribute property Id.")>]
type GuidAttribute = System.Runtime.InteropServices.GuidAttribute
