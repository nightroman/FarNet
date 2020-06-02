open Module1

let name =
    if fsi.CommandLineArgs.Length > 1 then
        fsi.CommandLineArgs.[1]
    else
        "unknown"

hello name
