
(*
    (1) Open this file in editor; check -> `see` is not defined because we do not add use-files.
    I tried to add them to checker. This did not help, probably because they are .fsx, not .fs.

    (2) Load or exec this -> works fin because `see` is preloaded from use-file to the session.
    So there is some inconsistency between check and exec with use-files.
    I am not sure how to deal with this and if this is a problem at all.

    So:
    -- use-files are just for interactive
    -- for checker, use proper modules and `[<AutoOpen>]`
*)

let x = 42
see x
