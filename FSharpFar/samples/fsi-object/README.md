## Interactive session settings

The script *fsi.fsx* shows how to configure the interactive session settings
using the global object `fsi` and its members dealing with preferences,
formatters, etc.

**How to use**

You may invoke this or similar script manually from any session. The session
does not matter, `fsi` is the global object.

Alternatively, in order to get this script invoked automatically, you may add
it to a configuration file as

```ini
[fsi]
--use:fsi.fsx
```

**How to test**

(1) Either invoke *fsi.fsx* in any session or open the interactive session with
*Fsi.fs.ini* which is configured to load *fsi.fsx*

    fs: //open with=Fsi.fs.ini

(2) In any interactive session editor type `[1..200]` (a list with 200 items)
and press `[ShiftEnter]`. As a result, you can see that

- All 200 items are printed instead of the default 100.
- Output is formatted with the editor width instead of the default 78.

(3) In the interactive editor type `far.Panel.ShownFiles` (current panel files)
and press `[ShiftEnter]`. As a result, you can see that files are printed in
the compact form `{File=...; Length=...}` instead of the default lengthy
property sets. Which form is "better" is another story, it's just a demo.
