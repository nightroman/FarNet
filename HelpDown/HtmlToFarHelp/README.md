# HtmlToFarHelp

[Pandoc]: https://github.com/jgm/pandoc
[MarkdownToHtml]: https://www.nuget.org/packages/MarkdownToHtml

## Synopsis

HtmlToFarHelp converts HTML with compatible structure to HLF (Far Manager help
format) and performs sanity checks for unique topic identifies and valid links.

The tool requires .NET Framework 4.0.

## Download

Get the tool as the NuGet package [HtmlToFarHelp](https://www.nuget.org/packages/HtmlToFarHelp).
You may download and unpack it to the current location by this PowerShell command:

    Invoke-Expression "& {$((New-Object Net.WebClient).DownloadString('https://github.com/nightroman/PowerShelf/raw/master/Save-NuGetTool.ps1'))} HtmlToFarHelp"

## Syntax

Usage:

    HtmlToFarHelp key=value ...
    HtmlToFarHelp "key = value; ..."

Input:

    from = input HTML file
    to = output HLF file

Example:

    HtmlToFarHelp from=Manual.htm to=Manual.hlf
    HtmlToFarHelp "to = Manual.hlf; from = Manual.htm"

## Description

The tool is supposed to be used with Markdown converters like [Pandoc]. Compose
manuals in Markdown, convert to HTML by a converter and then convert to HLF by
HtmlToFarHelp.

It is fine to compose in HTML and convert to HLF. But this is not recommended
due to the limited subset of supported HTML and not documented or stable rules.

### Conversion options

Conversion options are specified as HTML comments in the source Markdown or
HTML. Here is the example with all available keys and default values:

    <!--HLF:
        Language = English,English;
        PluginContents = ;
        Margin = 1;
        IndentCode = 4;
        IndentList = 2;
        IndentPara = 0;
        IndentQuote = 4;
        PlainCode = false;
        TopicHeading = h6;
        PlainHeading = false;
        CenterHeading = false;
    -->

Global options should be defined before the first heading, `Language` and
`PluginContents` should be defined there. Other options may change later.
Use an empty set in order to reset the current options to global.
Example:

    <!--Tell to render code blocks as plain text-->
    <!--HLF: PlainCode = true-->

    This code block is not highlighted in HLF:

        some
        text

    <!--Reset options to global-->
    <!--HLF:-->

### Headings and topics

The first heading, any from `h1` to `h6`, becomes the main topic "Contents".
If it has an identifier and it is not "Contents" then links to this heading
should use this identifier, to keep GitHub or VSCode internal links valid.
In HLF these links are replaced with "Contents" automatically.

Other headings define topics if they have identifies and their levels are less
than or equal to `TopicHeading`.

The default `TopicHeading` is `h6`, so that all headings with identifies define
help topics. Headings without identifies are internal in help topics.

Some Markdown converters generate identifies for all headings. In this case, in
order to use internal headings, `TopicHeading` must be changed. For example, if
it is set to `h2` then headings `h3` - `h6` are internal even with identifiers.

## Markdown

### Git Flavored Markdown

HTML may be produced by this command:

    pandoc MyHelp.md --output MyHelp.htm --from=gfm

The above is enough for converting *MyHelp.htm* to HLF.

To keep line breaks similar to source, use `--wrap=preserve`. This should not
affect HLF help rendering but may be useful for HLF inspection in the editor.

To make HTML for documentation, not just conversion, use `--standalone` and
define the page title as `--metadata=pagetitle:MyHelp`.

GFM advantages:

- The source is well rendered on GitHub and may be used as README.
- Useful auto-generated heading identifies for links to topics.
- GitHub and VSCode support navigation by these links.
- Clear escaping rules ~ any ASCII punctuation.

GFM disadvantage:

- Definition lists are not supported.

Definition lists may be enabled as `--from=gfm+definition_lists`. In this case
the source may be not suitable for GitHub rendering. Using other lists with
several paragraphs per item may work better. It is not a big issue.

### PHP Markdown Extra

HtmlToFarHelp was originally designed for this Markdown dialect because it
supported heading identifiers and definition lists and MarkdownDeep was
available at that time.

Today, the recommended converter is [Pandoc]:

    pandoc MyHelp.md --output MyHelp.htm --from=markdown_phpextra

The obsolete converter [MarkdownToHtml] is still available but it will be
unlisted on NuGet in the future.

Pandoc `markdown_phpextra` comparison with MarkdownDeep (MarkdownToHtml):

- Pandoc does not handle fenced code blocks in lists (not very useful in HLF)
- Escaping: Pandoc ~ "Standard Markdown", MarkdownDeep ~ "PHP Markdown Extra"

## Demo

The package directory *Demo* contains:

- *README.md*: Markdown sample with features and test cases.
- *README.htm*: HTML file created from *README.md* by *Pandoc*.
- *README.hlf*: HLF file created from *README.htm* by *HtmlToFarHelp*.
- *Convert-MarkdownToHelp.ps1*: Demo script used for the above conversions.

## See also

- [Release Notes](https://github.com/nightroman/FarNet/blob/master/HelpDown/HtmlToFarHelp/Release-Notes.md)
