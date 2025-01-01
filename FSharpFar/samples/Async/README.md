# Async jobs in Far Manager

This sample demonstrates non-blocking async jobs using the following scenario.

A wizard-like dialog is shown. The choices are:

- `[OK]` - close the wizard and complete the program
- `[Editor]` - open modeless editor with demo text
- `[Panel]` - open panel to represent demo data
- `[Cancel]` - cancel the program

The editor and panel steps are non-blocking. When they are opened you may
switch to other Far Manager windows, do something else, and then come back.
When the editor or panel exits the flow resumes with the wizard dialog.

This async job is defined in *Wizard.fs*. You may start it as:

    fs: Async.Start Wizard.jobWizard

In order to execute it from another directory use the command like:

    fs:exec with=.\.fs.ini ;; Async.Start Wizard.jobWizard

Several async jobs may be in started simultaneously. For example, start the
wizard job, click `[Editor]`, switch to panels. Then run the job again and
also click `[Editor]`. You now have two jobs running. Enter some text in
editors, save, exit. Corresponding jobs resume with wizard dialogs.

**Files**

- *.fs.ini* - F# configuration
- *MyPanel.fs* - panel used by demo
- *Wizard.fs* - sample wizard dialog job
- *Parallel.fs* - dialogs doing parallel jobs
