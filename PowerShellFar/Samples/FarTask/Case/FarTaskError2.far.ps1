# An error in job code.

Start-FarTask {
	job {
		throw 'oops-job'
	}
}
