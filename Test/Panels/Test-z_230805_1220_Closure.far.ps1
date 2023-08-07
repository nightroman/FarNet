# 230805_1220 -- GetNewClosure is needed

$230805_1220 = '1234567890'

5 | Out-FarPanel @(
	@{Expression={$230805_1220.Substring(0, $_)}}
	@{Expression={$230805_1220.Substring(0, $_)}.GetNewClosure()}
)
