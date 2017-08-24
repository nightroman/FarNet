
## Non-blocking async flows in Far Manager

This sample demonstrates non-blocking async flows using the following scenario:

A wizard-like dialog is shown. The choices are:

- `[OK]` - close the wizard and complete the program.
- `[Editor]` - non-modal editor to edit some data.
- `[Panel]` - panel to represent some data.
- `[Cancel]` - exit the wizard.

The editor and panel steps are non-blocking. When the editor or panel is opened
a user may switch to other windows and then continue with editor or panel. When
this editor or panel exits the flow continues and the wizard dialog is resumed.

This flow is defined in *App.fs* and it may be invoked by *App1.fsx*.

Several flows may be in progress simultaneously and a user may do some other
work at the same time. For example, run *App1.fsx*, click `[Editor]`, switch
to panels. Then run *App1.fsx* again and also click `[Editor]`. As a result,
you have two flows running. In order to see this, in two opened editors enter
different texts. On exit the wizard shows the current flow text.

*App2.fsx* is an example of concurrent flows. It starts the sample flows and
then starts testing flows in order to check and manipulate the sample flows.

**Files**

- *App.fs* - the sample flow
- *App1.fsx* - starts the sample flow normally
    - Invoke this script in order to see the flow in action.
- *App2.fsx* - starts the flow for auto tests
    - Invoke this script in order to see how the flow is automatically
      controlled and tested by concurrent flows with different testing
      scenarios.
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
