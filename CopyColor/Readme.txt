
Module   : FarNet.CopyColor
Release  : 2011-07-09
Category : Editors
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= PREREQUISITES =

 * Far Manager 2.0.1807
 * Plugin FarNet 4.5.0
 * Plugin Colorer


= DESCRIPTION =

This tool copies selected text with colors from the editor to the clipboard
using HTML clipboard format. This text can be pasted into Microsoft Word,
Outlook, and some other editors. Not all editors support this format. For
example, WordPad does not.


= DETAILS =

The tool can only copy text that has been shown and completely coloured. If the
selection is not completely in the editor screen area then at first scroll the
text to make it shown and coloured once.

TIP: Selection by [CtrlA] may not work. Instead, [CtrlHome] and [CtrlShiftPgDn]
repeated until the end of a file may produce coloured selection ready for copy.

Long lines are not supported. The tool cannot copy selected text if it contains
at least one line which is longer than the current editor window width.
