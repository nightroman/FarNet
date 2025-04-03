# Error in run block after UI.

Start-FarTask {
	run {
		$Far.Message('working')
		throw 'oops-run-after'
	}
}
