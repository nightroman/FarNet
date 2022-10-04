# How to get full paths of various panel items

Getting file systems paths of various panel items is a typical task.
Such paths are used as input for operations on files and folders.

FarNet provides the required API.
But the task it is not straightforward.
For example, the special item ".." normally should be excluded.

[PanelPaths.fs](PanelPaths.fs) contains sample helper functions.

[app.fsx](app.fsx) invokes sample functions and prints the results.

```
fs: exec: file = app.fsx
```
