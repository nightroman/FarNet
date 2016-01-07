
# MarkdownToHtml

### Synopsis

**OBSOLETE**: MarkdownToHtml.exe converts markdown files to HTML files.

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

* Markdown dialect is [PHP Markdown Extra].
* Conversion is done by [MarkdownDeep].

**WARNING**: The core `MarkdownDeep` has some known issues and it is not
developed for a while. Consider to use other tools, for example [pandoc].

---

[PHP Markdown Extra]: http://michelf.com/projects/php-markdown/extra/
[MarkdownDeep]: http://www.toptensoftware.com/markdowndeep/
[pandoc]: https://github.com/jgm/pandoc
