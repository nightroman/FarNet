# ColorerPack - syntax and color schemes for Colorer

**Colorer schemes**

- fsharp.hrc
- graphql.hrc
- markdown.hrc
- [powershell.hrc](#powershellhrc)
- [r.hrc](#rhrc)
- [visual.hrd](#visualhrd)

**Extra schemes**

- dbcs.hrc // DB Connection String
- dib.hrc // Polyglot Notebook

*********************************************************************
## powershell.hrc

PowerShell syntax scheme. Designed together with with *visual.hrd*.

### Outlined regions

- Functions and filters;
- Triple-hash line comments `###`;
- `task` entries (*Invoke-Build*, *psake*).

### Regex syntax

Regex syntax is colored in string literals following the regex type shortcut
`[regex]` and regex operators (`-match`, `-replace`, ...):

```powershell
[regex]'(?i)^text$'
-match '(?i)^text$'
-replace '(?i)^text$'
```

In addition regex syntax is colored in strings after the conventional comment
`<#regex#>`. This here-string with regex is colored:

```powershell
<#regex#>@'
(?ix)
^ text1   # comment1
 text2 $  # comment2
'@
```

### SQL syntax

SQL syntax is colored in here-strings (`@'...'@`, `@"..."@`) after the
conventional comment `<#sql#>`:

```powershell
<#sql#>@'
SELECT *
FROM table1
WHERE data1 = @param1
'@
```

*********************************************************************
## r.hrc

R syntax scheme. R is a programming language and software environment for
statistical computing and graphics.
See [R project home page](http://www.r-project.org/).

### Features

- Outlined functions.
- Outlined triple-hash comments `###`.
- TODO, BUG, FIX, web-addresses, etc. in other comments.

The scheme covers most of R syntax quite well.
This code snippet highlights some typical features:

```r
### Draws simple quantiles/ECDF
# R home page: http://www.r-project.org/
# !! see ecdf() {library(stats)} for a better example
plot(x <- sort(rnorm(47)), type = "s", main = "plot(x, type = \"s\")")
points(x, cex = .5, col = "dark red")
```

*********************************************************************
## visual.hrd

***visual.hrd***

Console color scheme with white background. It is initially called *visual* for
similarity to Visual Studio default colors used for some file types. It is also
designed to support *powershell.hrc* and tweak other schemes colors.

***visual-rgb.hrd***

RGB color scheme generated from *visual.hrd* with RGB values for standard
console colors. This scheme can be used with the *colorer.exe* in order to
create HTML files with the same colors as they are in the Far Manager editor
with *visual.hrd*.

*********************************************************************
