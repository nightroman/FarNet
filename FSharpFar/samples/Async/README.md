
# Non-blocking async flows in Far Manager

This sample demonstrates non-blocking async flows using the following scenario:

A wizard-like dialog is shown. The choices are:

- `[OK]` - close the wizard and complete the program.
- `[Editor]` - non-modal editor to edit some data.
- `[Panel]` - panel to represent some data.
- `[Cancel]` - exit the wizard.

The editor and panel steps are non-blocking. When the editor or panel is opened
a user may switch to other windows and then continue with editor or panel. When
this editor or panel exits the flow continues and the wizard dialog is resumed.

This flow is defined in the module file *App.fs* and it may be invoked as:

    fs: //exec file=App.fs ;; FarNet.FSharp.Job.Start App.flowWizard

Several flows may be in progress simultaneously and a user may do some other
work at the same time. For example, run the flow, click `[Editor]`, switch to
panels. Then run the flow again and also click `[Editor]`. As a result, you
have two flows running. In order to see this, in two opened editors enter
different texts. On exit the wizard shows the current flow text.

The script *App.fsx* is an example of concurrent flows. It starts sample flows
and then starts testing flows in order to check and manipulate sample flows.

**Files**

- *Async.fs.ini* - F# config
- *App.fs* - the sample wizard flow
- *App.fsx* - starts flows for auto tests
- *MyPanel.fs* - some panel used by flows
- *Parallel.fs* - shows some parallel jobs
- *Test.fs* - common test tools
- *Test...fs* - other test flows

**NOTE**

If you have Visual Studio installed, invoke `Project` from the module menu in
order to open this directory files as a project in Visual Studio.
