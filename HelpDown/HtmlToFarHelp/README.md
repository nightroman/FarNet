# HtmlToFarHelp

[Pandoc]: https://github.com/jgm/pandoc

## Synopsis

HtmlToFarHelp converts HTML with compatible structure to HLF (Far Manager help
format) and performs sanity checks for unique topic identifiers and valid links.

The tool requires .NET Framework 4.7.2.

## Download

Get the tool as the NuGet package [HtmlToFarHelp](https://www.nuget.org/packages/HtmlToFarHelp).
You may download and unpack it to the current location by this PowerShell command:

    iex "& {$(irm https://raw.githubusercontent.com/nightroman/PowerShelf/main/Save-NuGetTool.ps1)} HtmlToFarHelp"

## Syntax

Usage:

    HtmlToFarHelp key=value ...
    HtmlToFarHelp "key = value; ..."

Input:

    from = input HTML file
    to = output HLF file

Example:

    HtmlToFarHelp from=Manual.html to=Manual.hlf
    HtmlToFarHelp "to = Manual.hlf; from = Manual.html"

## Description

The tool is supposed to be used with Markdown converters like [Pandoc]. Compose
in Markdown, convert it to HTML and then convert HTML to HLF by HtmlToFarHelp.

It is fine to compose in HTML and convert to HLF. But this is not recommended
due to some HTML elements not supported and not documented or stable rules.

### Conversion options

Conversion options are specified as HTML comments in the source Markdown or
HTML. The example with all available keys and default values:

    <!--HLF:
        Language = English,English;
        PluginContents = ;
        TopicHeading = h6;
        Margin = 1;
        IndentCode = 4;
        IndentList = 2;
        IndentPara = 0;
        IndentQuote = 4;
        PlainCode = false;
        PlainHeading = false;
        CenterHeading = false;
        EmptyLinesBeforeTopic = 1;
        EmptyLinesAfterHeading = 1;
        EmptyLinesBeforeHeading = 1;
        ListBullet = ;
        ListItemEmptyLine = false;
        HighlightListBullet = false;
        HighlightListNumber = false;
    -->

Global options should be defined before the first heading, `Language` and
`PluginContents` should be defined there. See Far API manuals for help
options `.Language` and `.PluginContents`.

The role of `TopicHeading` is described further. Other options set formatting
in HLF. They may be changed anywhere in the source and stay changed until the
next change or reset. Use an empty set to reset the current options to global.

Example:

    <!--show code blocks as plain text-->
    <!--HLF: PlainCode = true-->

    This code block is not highlighted in HLF:

        some
        text

    <!--reset options to global-->
    <!--HLF:-->

`ListBullet` is one or more space separated strings used as list bullets.
Positions correspond to list item depths. The last string is used for all
deeper levels.

`ListItemEmptyLine` tells to use an empty line between list items.

### Headings and topics

The first heading, any of `h1` - `h6`, with or without identifier, becomes the
main help topic "Contents". If it has an identifier other than "Contents" then
in HLF it is changed to "Contents" together with related links. Source links
should use the original heading identifier anyway. If it is too long, e.g.
generated, use the shortcut, see *Demo*.

Other headings define topics if they have identifiers and their levels are less
than or equal to `TopicHeading`. The default is `h6`, so that all headings with
identifiers define help topics and headings without identifiers are internal in
topics.

Some converters generate identifiers for all headings. In order to use internal
headings change `TopicHeading`. For example, if it is set to `h2` then headings
`h1` and `h2` define help topics and `h3` - `h6` are internal in topics.

## Markdown

### GitHub Flavored Markdown

HTML may be produced by this command:

    pandoc MyHelp.md --output MyHelp.html --from=gfm

To keep line breaks similar to source, use `--wrap=preserve`. This should not
affect HLF help rendering but may be useful for HLF inspection in the editor.

For making just HLF use `--syntax-highlighting=none` to disable not needed
highlighting. For making HTML and HLF default highlighting may be preserved.

To make HTML for documentation, use `--standalone` and set the page title
`--metadata=pagetitle:MyTitle`. If this HTML is used for HLF then set
language `--metadata=lang:en` to fix invalid XML in some cases.

GFM advantages:

- The source is well rendered on GitHub and may be used as README.
- Useful auto-generated heading identifiers for links to topics.
- GitHub and VSCode support navigation by these links.
- Clear escaping rules, any ASCII punctuation.

GFM disadvantage:

- Definition lists are not supported.

Definition lists may be enabled as `--from=gfm+definition_lists`. In this case
the source may be not suitable for GitHub rendering. Using other lists with
several paragraphs per item may work better. It is not a big issue.

### PHP Markdown Extra

HtmlToFarHelp was originally designed for this Markdown dialect because it
supported heading identifiers and definition lists and MarkdownDeep was
available at that time.

The recommended converter is [Pandoc]:

    pandoc MyHelp.md --output MyHelp.html --from=markdown_phpextra

Pandoc `markdown_phpextra` comparison with obsolete MarkdownDeep:

- Escaping: Pandoc ~ "Standard Markdown", MarkdownDeep ~ "PHP Markdown Extra"
- Pandoc does not handle fenced code blocks in lists (not very useful in HLF)
- Pandoc replaces multiple consecutive spaces with one in usual text

## Demo

The package directory *Demo* contains:

- *README.md*: Markdown sample with features and test cases.
- *README.html*: HTML file created from *README.md* by *Pandoc*.
- *README.hlf*: HLF file created from *README.html* by *HtmlToFarHelp*.
- *Convert-MarkdownToHelp.ps1*: Demo script used for the above conversions.

## Tools

The FarNet module PowerShellFar comes with HLF utility scripts:

- *Show-FarMarkdown.ps1* is used for opening the HLF viewer at the current topic
  from the editor with .md or .text files. In other words, on composing HLF in
  Markdown you may preview result help topics.
- *Show-Hlf.ps1* is used for opening the HLF viewer at the current topic from
  the editor with .hlf files. It is similar to the official plugin *HlfViewer*.

## See also

- [HtmlToFarHelp Release Notes](https://github.com/nightroman/FarNet/blob/main/HelpDown/HtmlToFarHelp/Release-Notes.md)
- [GitHub Flavored Markdown](https://github.github.com/gfm/)
- [PHP Markdown Extra](https://michelf.ca/projects/php-markdown/extra/)
- [Pandoc User's Guide](https://pandoc.org/MANUAL.html)
