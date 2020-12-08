# An error in ps: code.

Start-FarTask {
	ps: {
		throw 'oops-ps:'
	}
}
