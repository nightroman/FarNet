# CopyColor

FarNet module CopyColor for Far Manager

*********************************************************************
## Synopsis

This tool copies the selected text with colors from the editor to the clipboard
using HTML clipboard format. This text can be pasted to Google Docs, Microsoft
Word, Outlook, and some other editors. Not all editors support this (WordPad).

**Project**

 * Source: <https://github.com/nightroman/FarNet/tree/main/CopyColor>
 * Author: Roman Kuzmin

*********************************************************************
## Installation

**Requirements**

 * Far Manager
 * Package FarNet
 * Package FarNet.FarColorer

**Instructions**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

*********************************************************************
## Description

The tool can only copy text that has been shown and completely coloured. If the
selection is not completely in the editor screen area then at first scroll the
text to make it shown and coloured once.

TIP: Selection by `[CtrlA]` may not work. Instead, `[CtrlHome]` and
`[CtrlShiftPgDn]` repeated until the end of a file may produce coloured
selection ready for copy.

Long lines are not supported. The tool cannot copy selected text if it contains
at least one line which is longer than the current editor window width.
