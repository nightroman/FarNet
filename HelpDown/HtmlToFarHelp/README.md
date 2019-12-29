# HtmlToFarHelp

[Pandoc]: https://github.com/jgm/pandoc
[MarkdownToHtml]: https://www.nuget.org/packages/MarkdownToHtml
[About-PowerShellFar.text]: https://github.com/nightroman/FarNet/blob/master/PowerShellFar/About-PowerShellFar.text

---
### Synopsis

HtmlToFarHelp.exe converts HTML files with compatible structure to HLF
(Far Manager help format) and performs sanity checks for unique topic
anchors, valid topic links, and etc.

The tool requires .NET Framework 4.0.

### Syntax

    HtmlToFarHelp.exe key=value ...
    HtmlToFarHelp.exe "key = value; ..."

Keys

    From = Input HTML file
    To   = Output HLF file

Examples

    HtmlToFarHelp from=Manual.htm to=Manual.hlf
    HtmlToFarHelp "to = Manual.hlf; from = Manual.htm"

---
### Description

*HtmlToFarHelp.exe* is supposed to be used together with markdown converters
like [Pandoc] or [MarkdownToHtml] (*obsolete, to be retired*).

The recommended scenario is to compose manuals in markdown files, convert them
to HTML by a converter and then convert to HLF. Markdown is simpler than HLF
and HTML, easier to write, read, and maintain.

It is fine to compose in HTML and convert to HLF. But this is not recommended
due to the limited subset of supported HTML and not documented or stable rules.

Conversion options are specified in a source as HTML comments in the original
markdown or HTML. Here is the example with available keys and default values:

    <!--HLF:
        Language = English,English;
        PluginContents = <none>;
        Margin = 1;
        IndentCode = 4;
        IndentList = 2;
        IndentPara = 0;
        IndentQuote = 4;
        CenterHeading = false;
        PlainCode = false;
        PlainHeading = false;
    -->

Global options are set before the first topic, `Language` and `PluginContents`
should be defined there. Then options can be changed anywhere. Use an empty set
in order to reset the current options to global:

    <!--HLF:-->

[About-PowerShellFar.text] is the example of a large markdown file used for
making HTML and HLF versions of documentation. HTML in this case is not an
intermediate file, it is also used as the manual.

---
### Demo

The directory *Demo* contains:

- *Demo.text*: The markdown source file showing a lot of features.
- *Demo.htm*: The HTML file created from *Demo.text* by *MarkdownToHtml*.
- *Demo.hlf*: Finally, the HLF file created from *Demo.htm* by *HtmlToFarHelp*.

---
### Download

HtmlToFarHelp is distributed as the NuGet package [HtmlToFarHelp](https://www.nuget.org/packages/HtmlToFarHelp).
Download and unpack it in the current location by this PowerShell command:

    Invoke-Expression "& {$((New-Object Net.WebClient).DownloadString('https://github.com/nightroman/PowerShelf/raw/master/Save-NuGetTool.ps1'))} HtmlToFarHelp"

---
### See also

- [Release Notes](https://github.com/nightroman/FarNet/blob/master/HelpDown/HtmlToFarHelp/Release-Notes.md)
