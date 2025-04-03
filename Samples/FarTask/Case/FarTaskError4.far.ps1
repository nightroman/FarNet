# Error in run block before UI.

Start-FarTask {
	run {
		throw 'oops-run-before'
	}
}
