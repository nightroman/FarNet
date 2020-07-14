# Async flows in Far Manager

This sample demonstrates non-blocking async flows using the following scenario.

A wizard-like dialog is shown. The choices are:

- `[OK]` - close the wizard and complete the program.
- `[Editor]` - non-modal editor to edit some data.
- `[Panel]` - panel to represent some data.
- `[Cancel]` - exit the program.

The editor and panel steps are non-blocking. When they are opened you may
switch to other Far Manager windows, do something else, and then come back.
When the editor or panel exits the flow resumes with the wizard dialog.

This async flow is defined in *Wizard.fs*. You may start it as:

    fs: Async.Start Wizard.flowWizard

In order to execute it from another directory use the command like:

    fs: //exec with=.\.fs.ini ;; Async.Start Wizard.flowWizard

Several async flows may be in started simultaneously. For example, start the
wizard flow, click `[Editor]`, switch to panels. Then run the flow again and
also click `[Editor]`. You now have two flows running. Enter some text in
editors, save, exit. Corresponding flows resume with wizard dialogs.

**Files**

- *.fs.ini* - F# configuration
- *MyPanel.fs* - panel used by demo
- *Wizard.fs* - sample wizard dialog flow
- *Parallel.fs* - dialogs doing parallel jobs
