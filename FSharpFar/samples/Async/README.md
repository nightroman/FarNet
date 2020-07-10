# Async flows in Far Manager

[/samples/Testing]: https://github.com/nightroman/FarNet/tree/master/FSharpFar/samples/Testing

This sample demonstrates non-blocking async flows using the following scenario:

A wizard-like dialog is shown. The choices are:

- `[OK]` - close the wizard and complete the program.
- `[Editor]` - non-modal editor to edit some data.
- `[Panel]` - panel to represent some data.
- `[Cancel]` - exit the program.

The editor and panel steps are non-blocking. When they are opened you may go to
other windows, do something else, and then come back. When the editor or panel
exits the flow continues and the wizard dialog is resumed.

This flow is defined in *App.fs* and may be started as:

    fs: Async.Start App.flowWizard

In order to execute it from another directory use the command like:

    fs: //exec with=.\Async.fs.ini ;; Async.Start App.flowWizard

Several flows may be in progress simultaneously. You may do some other work at
the same time. For example, run the flow, click `[Editor]`, switch to panels.
Then run the flow again and also click `[Editor]`. As a result, you have two
flows running. Enter some text in editors, save, exit. Flows resume.

Files *TestXXX.fs* are examples of concurrent flows used for testing. They
start sample flows and then start testing flows. Testing flows drive the
samples simulating user interactions and check the expected results.

*App1.fsx* is the test runner.
For more details about testing see [/samples/Testing].

**Files**

- *Async.fs.ini* - F# configuration
- *App.fs* - sample wizard flow
- *App1.fsx* - auto test runner
- *MyPanel.fs* - demo panel
- *Test.fs* - common test tools
- *TestXXX.fs* - demo and test cases
