# CopyColor

FarNet module CopyColor for Far Manager

*******************************************************************************
## Synopsis

This tool copies the selected editor text with colors to the clipboard as HTML.
This text may be pasted to Google Docs, Microsoft Word, Outlook, and some other
editors. If an editor does not support HTML (WordPad) then plain text is used.

*******************************************************************************
## Installation

- Far Manager
- Plugin FarColorer
- Package [FarNet](https://www.nuget.org/packages/FarNet)
- Package [FarNet.CopyColor](https://www.nuget.org/packages/FarNet.CopyColor)

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

*******************************************************************************
## Description

The tool can only copy text that has been shown and completely colored. If the
selection is not completely in the editor screen then scroll the text to make
it shown and colored once.

TIP: Selection by `[CtrlA]` may not work due to the above limitation. Instead,
use `[CtrlHome]` and `[CtrlShiftPgDn]` repeated until the end to produce
colored selection ready for copy.

Long lines are not supported. The tool cannot copy the selected text if it
contains at least one line longer than the current editor screen width.

*******************************************************************************
## Credits

- [ClipboardHelper by Arthur Teplitzki](https://gist.github.com/ArthurHub/10729205)
