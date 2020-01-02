# HtmlToFarHelp Release Notes

## v1.2.2

The first heading, any from `h1` to `h6`, becomes the main topic "Contents",
even if it does not have an identifier. In theory, this may be incompatible.
But in practice "Contents" is usually already the first with sort of list of
links to following topics, so having something before it would be strange.

New conversion option `TopicHeading` tells which headings should be used for
help topics. The default is `h6`, all headings with identifiers define topics.

With these changes HtmlToFarHelp becomes well designed for Markdown dialects
"Git Flavored Markdown" (modern) and "PHP Markdown Extra" (legacy, obsolete?).
See README for details.

## v1.1.0

**Requires .NET Framework 4.0.** It is needed for some new XML features.

The tool supports input fragments, i.e. HTML content without usual `html`,
`head`, `body` elements. For example, if `pandoc` is used for converting
markdown to HTML then `--standalone` is not required.

Fixed mixed EOL in output HLF.

More tweaks for Pandoc HTML.

## v1.0.5

Avoid unwanted new lines in Pandoc HTML definition lists.

## v1.0.4

Adapted for Pandoc HTML.

## v1.0.3

Fixed mixed line ends.

## v1.0.2

Added missing HLF option `PluginContents`. Without it help files are not shown
in the plugin help list (`[F1] [ShiftF2]`).

## v1.0.1

Horizontal rules are natively supported by Far 3.0.3831

Removed the obsolete inline option `CenterRule`.

The package moved to NuGet.
