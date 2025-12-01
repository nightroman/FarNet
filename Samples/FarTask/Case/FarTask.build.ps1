
Set-StrictMode -Version 3

# Synopsis: Task variables
task panel-task-vars {
	Start-FarTask {
		$variables = Get-Variable
		job {
			$Var.variables | Out-FarPanel Name, Value, Options, Description -Title "Task variables"
		}
	}
}

# Synopsis: Job variables
task panel-job-vars {
	Start-FarTask {
		job {
			Get-Variable | Out-FarPanel Name, Value, Options, Description -Title "Job variables"
		}
	}
}

# Synopsis: dump-variables in a new session, 5 .csv
task dump-variables {
	Start-Process pwsf "-noe -pan -c ./dump-variables.far.ps1"
}

# Synopsis: Time GetNewClosure vs GetScriptBlock
# For just making unbound scripts use GetScriptBlock.
# GetNewClosure is 3x slower but it is needed as such.
task time-script-making-ways {
	$sb = (Get-Command Invoke-Build.ps1).ScriptBlock
	Measure-Command2.ps1 -Count 1e4 @(
		{
			$sb.GetNewClosure()
		}
		{
			$sb.Ast.GetScriptBlock()
		}
	)
}
