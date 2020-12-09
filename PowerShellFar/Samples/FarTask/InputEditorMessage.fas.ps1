<#
.Synopsis
	Demo with input box, non-modal editor, message box.

.Description
	The data ($Data.Text) flows through three jobs. Note that the editor job is
	not modal, you can do something else in Far. But when the editor exits the
	task continues with the next job.
#>

param($Text = 'Hello async world')

# Initial text.
$Data.Text = $Text

# Input box with our text as the default. Here the job returns the result, so
# we use `$Data.Text = job {...}`. Alternatively, we can use a job with no
# output and set $Data.Text in it: `job { $Data.Text = ... }`.
$Data.Text = job {
	$Far.Input('Type something', $null, 'Input', $Data.Text)
}

# This job starts a non-modal editor with the initial text $Data.Text and sets
# the new $Data.Text value in the editor `Saving` event handler.
job {
	$editor = $Far.CreateEditor()
	$editor.FileName = $Far.TempName()
	$editor.DisableHistory = $true
	$editor.DeleteSource = 'File'
	$editor.add_Opened({$this.SetText($Data.Text)}) # input
	$editor.add_Saving({$Data.Text = $this.GetText()}) # output
	[FarNet.Tasks]::Editor($editor)
}

# This message box shows the result $Data.Text when the editor exits.
job {
	$Far.Message($Data.Text, "Result")
}

# Tasks may output data. To consume the result, use Start-FarTask -AsTask and
# await the result task in the parent async scenario. In this example the
# output is checked by tests.
$Data.Text
