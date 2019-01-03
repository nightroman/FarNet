
## Interactive session settings

The script *fsi.fsx* shows how to access the `fsi` object and its members
dealing with interactive session settings, i.e. formatters, preferences, etc.

Normally, this or similar script should be added to the configuration file as

```ini
[fsi]
--use:fsi.fsx
```

**How to test:**

(1) Open the interactive session with *Fsi.fs.ini* which uses *fsi.fsx*

    fs: //open with=Fsi.fs.ini

(2) In the interactive editor type `[1..200]` (a list with 200 items) and press
`[ShiftEnter]`. As a result, you can see that

- All 200 items are printed instead of the default 100.
- Output is formatted with the editor width instead of the default 78.

(3) In the interactive editor type `far.Panel.ShownFiles` (current panel files)
and press `[ShiftEnter]`. As a result, you can see that files are printed in
the compact form `{File=...; Length=...}` instead of the default lengthy
property sets. Which form is "better" is another story, it's just a demo.

***

- [Interactive.InteractiveSession Class](https://msdn.microsoft.com/visualfsharpdocs/conceptual/interactive.interactivesession-class-%5bfsharp%5d)
