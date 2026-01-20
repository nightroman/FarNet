# FarNet.Drawer

This module is empty, replaced by [FarNet.EditorKit](https://www.nuget.org/packages/FarNet.EditorKit)

## Migrate settings

**(1) file masks**

Edit `%FARPROFILE%\FarNet\FarNet.xml` and find lines like:

```xml
  <Module Name="Drawer">
    <Drawer Id="a9a6f877-e049-4438-a315-d5914b200988" Mask="*" />
    <Drawer Id="ae160caa-6f5b-43f1-b94a-f2a4fa6ba000" Mask="*.tsv" />
  </Module>
```

Change `Name="Drawer"` to `Name="EditorKit"`.\
Restart Far Manager.

**(2) other settings**

Open EditorKit settings: `F11 / FarNet / Settings / EditorKit`.

Replace XML elements `CurrentWord`, `FixedColumn`, `Tabs` with elements from
`%FARPROFILE%\FarNet\Drawer\Settings.xml`.

## Tidy up

Remove the old module and its data:

- `%FARHOME%\FarNet\Modules\Drawer`
- `%FARPROFILE%\FarNet\Drawer`
