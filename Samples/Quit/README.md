# "Quit()" method and handlers

In order to tell Far to exit, use `$Far.Quit()`, either directly from scripts or by `[F10]` using macro:

```lua
Macro {
  area="Shell"; key="F10"; description="PSF: Quit Far";
  action=function()
    if not Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[vps:$Far.Quit()]]) then Keys "F10" end
  end;
}
```

**Samples**

- [Quit-Running-FarTask.far.ps1](Quit-Running-FarTask.far.ps1)
- [Register-Quitting-Once.far.ps1](Register-Quitting-Once.far.ps1)
