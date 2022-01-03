<#
.Synopsis
	Bad completion profile.
.Description
	-- Start Far
	-- Add this folder to the path
	-- Trigger any completion
	-- Examine $Error for issues
#>

### Issue - unexpected output from profile
42

$TabExpansionOptions.InputProcessors += {
	### Issue - invalid input processor result
	42
}

$TabExpansionOptions.ResultProcessors += {
	### Issue - unexpected result processor output
	42
}
