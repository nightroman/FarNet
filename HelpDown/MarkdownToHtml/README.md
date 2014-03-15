
MarkdownToHtml
==============

### Synopsis

MarkdownToHtml.exe converts markdown files to HTML files.

The tool requires .NET Framework 3.5 or above.

---
### Syntax

    MarkdownToHtml.exe key=value ...
    MarkdownToHtml.exe "key = value; ..."

Keys

    From  = Input markdown file
    To    = Output HTML file
    Title = Optional HTML title

Examples

    MarkdownToHtml from=README.md to=README.htm
    MarkdownToHtml "to = README.htm; from = README.md; title = Introduction to ..."

---
### Description

MarkdownToHtml.exe is used in order to convert markdown files to HTML files.

* Markdown dialect is [PHP Markdown Extra][1].
* Conversion is done by [MarkdownDeep][2].

---

[1]: http://michelf.com/projects/php-markdown/extra/
[2]: http://www.toptensoftware.com/markdowndeep/
