## How to use Windows forms and charting

Do not call the Windows forms method `Show()`, it is not working in FSharpFar
without Windows forms event loop. Call the modal `ShowDialog()` instead, in a
separate thread, if the dialog should not block Far Manager.

The provided examples use `System.Windows.Forms.DataVisualization.Charting` and
Windows forms directly without any extra packages. Run `*.fsx` scripts in order
to show sample charts.

For easier charting in F# consider using [FarNet.FSharp.Charting](https://github.com/nightroman/FarNet.FSharp.Charting),
see [/samples](https://github.com/nightroman/FarNet.FSharp.Charting/tree/master/samples).

**Using fsx.exe or fsi.exe**

All .fsx scripts in this folder may be invoked by `fsx.exe` or `fsi.exe`
without Far Manager.

There is a little trick with async (non modal) shows. In Far Manager they are
simply started by `Async.Start`. The chart is shown and Far Manager is not
blocked, good.

But if you also want to run same scripts by `fsx` then shows must be started by
`Async.RunSynchronously`, otherwise `fsx` exists immediately.

The following code automatically chooses the starting method:

```fsharp
async {
    ...
}
|> if fsi.CommandLineArgs.[0].EndsWith(".fsx") then Async.RunSynchronously else Async.Start
```
