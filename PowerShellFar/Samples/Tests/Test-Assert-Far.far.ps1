<#
.Synopsis
	How Assert-Far works.
#>

function Test-User($User) {
	Assert-Far ($User.Name) -Message 'Name must not be empty.'
	Assert-Far ($User.Age -gt 0) -Message 'Age must be positive.'
}

# data with invalid Age
$User = [PSCustomObject]@{
	Name = 'John Doe'
	Age = -42
}

# this triggers assert
Test-User $User

# if you [Ignore], return the original or corrected in debugger
return $User

<#
.Description
	Scenario "correct and continue" with Add-Debugger:
	- when assert fails, in its dialog, click [Debug]
	- in "Attach debugger...", click [Add-Debugger]
	- in Add-Debugger input box, enter `$user.age=55`
	- then enter `q` or `c` or `d` (see [1])
	- in Assert-Far dialog shown again, click [Ignore]
	As a result, the script continues with the corrected state.

	[1]
	With this assert debugger, it does not matter how to continue:
	`q` (quit), `c` (continue), `d` (detach), or even by stepping.
	The same assert dialog is shown again and you choose what to do.
#>
