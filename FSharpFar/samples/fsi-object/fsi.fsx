// Configure session settings

// PrintLength - Gets or sets the total print length of the interactive session.
// The default is 100. If you run `[1..200]` in the interactive then it prints
// the first 100 values followed by `...`. If the value is changed to 200 then
// all 200 items are printed.

fsi.PrintLength <- 200

// For Far related settings.
open FarNet

// PrintWidth - Gets or sets the print width of the interactive session.
// The default is 78. The following code adjusts it to the Far window.

fsi.PrintWidth <- far.UI.WindowSize.X - 2

// AddPrinter - Adds a "printer", the function ('T -> string) converting a type
// instance to a string printed as the interactive output.

fsi.AddPrinter (fun (file: FarFile) ->
    sprintf "{Name=%s; Length=%i}" file.Name file.Length
)
