# RightControl

FarNet module RightControl for Far Manager

*********************************************************************
## Synopsis

This tool alters some operations in editors, edit boxes, and the command line.
They are: *Step*, *Select*, *Delete* by words, *Go*, *Select* to *smart home*.
New actions are similar to what many popular editors do on stepping, selecting,
deleting by words, and etc. Example: Visual Studio, Word, WordPad, etc.

**Project**

 * Source: <https://github.com/nightroman/FarNet/tree/main/RightControl>
 * Author: Roman Kuzmin

*********************************************************************
## Installation

**Requirements**

 * Far Manager
 * Package FarNet
 * Package FarNet.RightControl

**Instructions**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

After installing the module copy the macro file
*RightControl.macro.lua* to *%FARPROFILE%\Macros\scripts*.

*********************************************************************
## Commands

The module works by commands called from macros associated with keys:

**Word commands**

- `step-left ~ [CtrlLeft]`
- `step-right ~ [CtrlRight]`
- `select-left ~ [CtrlShiftLeft]`
- `select-right ~ [CtrlShiftRight]`
- `delete-left ~ [CtrlBS]`
- `delete-right ~ [CtrlDel]`
- `vertical-left ~ [CtrlAltLeft]`
- `vertical-right ~ [CtrlAltRight]`

**Smart home commands**

- `go-to-smart-home ~ [Home]`
- `select-to-smart-home ~ [ShiftHome]`

*********************************************************************
## Settings

Module settings: `[F11] \ FarNet \ Settings \ RightControl`

- `RegexLeft`

    This regular expression defines caret stops on moving left.

- `RegexRight`

    This regular expression defines caret stops on moving right.

**Regex examples**

Default patterns. Stops are similar to Visual Studio:

    (?x: ^ | $ | (?<=\b|\s)\S )
    (?x: ^ | $ | (?<=\b|\s)\S )

Patterns with stops similar to Word/WordPad (`_` stops, too):

    (?x: ^ | $ | (?<=\b|\s)\S | (?<=[^_])_ | (?<=_)[^_\s] )
    (?x: ^ | $ | (?<=\b|\s)\S | (?<=[^_])_ | (?<=_)[^_\s] )

Default patterns with two more breaks: letter case and digits:

    (?x: ^ | $ | (?<=\b|\s)\S | (?<=\p{Ll})\p{Lu} | (?<=\D)\d | (?<=\d)[^\d\s] )
    (?x: ^ | $ | (?<=\b|\s)\S | (?<=\p{Ll})\p{Lu} | (?<=\D)\d | (?<=\d)[^\d\s] )


The same with comments, thanks to `(?x:)` together with ignored white spaces:

    (?x:
        ^ | $ # start or end of line
        |
        (?<=\b|\s)\S # not a space with a word bound or a space before
        |
        (?<=\p{Ll})\p{Lu} # an upper case letter with a lower case letter before
        |
        (?<=\D)\d | (?<=\d)[^\d\s] # a digit/not-digit with a not-digit/digit before
    )
