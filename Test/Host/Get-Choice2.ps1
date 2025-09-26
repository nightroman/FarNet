<#
.Synopsis
	Shows PS style menu and gets multiple user choices.

.Description
	*) Returns choice indexes.
	*) Choice keys are indicated by '&' in menu items.
	*) Help strings may be empty or nulls (items are used).

.Parameter Caption
		Menu caption.

.Parameter Message
		Menu message.

.Parameter Choices
		Choice info pairs: item1, help1, item2, help2, ...

.Parameter DefaultChoices
		Default choice indexes (i.e. selected on [Enter])

.Link
	http://stackoverflow.com/questions/3664061/what-cmdlets-use-the-ihostuisupportsmultiplechoiceselection-interface-to-prompt-f
#>

[CmdletBinding()]
param(
	[string]$Caption = 'Confirm'
	,
	[string]$Message = 'Are you sure you want to continue?'
	,
	[string[]]$Choices = ('&Yes', 'Continue', '&No', 'Stop')
	,
	[int[]]$DefaultChoices = @(0)
)

if ($Choices.Count % 2) { throw "Choice count must be even." }

$descriptions = @()
for($i = 0; $i -lt $Choices.Count; $i += 2) {
	$c = [System.Management.Automation.Host.ChoiceDescription]$Choices[$i]
	$c.HelpMessage = $Choices[$i + 1]
	if (!$c.HelpMessage) {
		$c.HelpMessage = $Choices[$i].Replace('&', '')
	}
	$descriptions += $c
}

$Host.UI.PromptForChoice($Caption, $Message, [System.Management.Automation.Host.ChoiceDescription[]]$descriptions, $DefaultChoices)
