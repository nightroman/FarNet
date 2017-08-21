
## Non-blocking async flows in Far Manager

This sample demonstrates non-blocking async flows using the following scenario:

- A user edits some text in the editor and closes the editor.
- A dialog with options is shown after that.
- Then a panel is opened for the next step.
- When the panel closes a message is shown.

This flow is defined in *App.fs* and it may be invoked by *App1.fsx*.

The main point of interest is that each step in this workflow is non-blocking,
except dialogs, of course. For example, when an editor is opened a user may
switch to panels or another editor and do some other work at first and then
continue with the editor and the rest of flow.

Several flows may be invoked simultaneously and normally a user may do
something else in Far at the same time. *App2.fsx* is an example of
concurrent flows, it starts the main sample flow and then starts
testing flows in order to check and manipulate the main flow.

**Files**

- *App1.fsx* - starts the sample flow normally
    - Invoke this script in order to see the flow in action.
- *App2.fsx* - starts the flow for auto tests
    - Invoke this script in order to see how the flow is automatically
      controlled and tested by concurrent flows with different testing
      scenarios.
- *App.fs* - the sample flow
- *MyPanel.fs* - some panel used by flows
- *AsyncFar.fs.ini* - FSF config file
- *AsyncFar.fsproj* - VS project file

**Tools**

- [Async.fs](https://github.com/nightroman/FarNet/blob/master/FSharpFar/src/Async.fs)
    - The prototype async tools for F#.

**Notes**

This is also an example of using `.fs.ini` and `.fsproj` together. `.fs.ini` is
a very simple "project" file for developing and running scripts right in Far.
`.fsproj` may be used for development with Visual Studio or VSCode.

This is also an example of a simple FarNet panel created in F#. See another
sample *TryPanelFSharp* for a more complex panel with some operations like
adding and removing panels items interactively.
