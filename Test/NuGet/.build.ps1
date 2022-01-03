
function Clean {
	remove C:\TEMP\FarHome
}

task . {
	Invoke-Build **
	Clean
}

task Clean {
	Clean
}
