
function global:TestManyMandatoryParameters {
	param (
		[Parameter(Mandatory=$true, HelpMessage='Help for Name')]
		[string]$Name,
		[Parameter(Mandatory=$true)]
		[string[]]$Tags,
		[Parameter(Mandatory=$true)]
		[System.Security.SecureString]$Password
	)
	$PSBoundParameters
}
