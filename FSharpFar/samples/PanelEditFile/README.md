# Panel with editable files

How to create a panel with files which content may be edited.

**Files**

- *PanelEditFile.fs.ini* - configuration
- *PanelEditFile.fs* - demo panel
- *App1.fsx* - demo runner

**How to use**

To start, run

    fs: exec: file = App1.fsx

or, the same

    fs: exec: ;; PanelEditFile.run ()

This opens the panel with some files with two columns, Name and Description.

Press `[F4]` in order to edit the current file content in the non-modal editor.
In our case the content is Description representing some number.

In the editor try to enter valid and invalid number texts. The editor commits
on saving and applies the new data to the panel. If the number text is invalid
then an error is shown, the editor is reopened or stays opened, so that you may
either continue editing or exit without new saving in order to ignore changes.
