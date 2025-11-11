
task test-user-menus {
	exec {fsx ../../../apps/test-user-menus.fsx}
}

task test_01_same_dir_fsx {
	Set-Location test_01
	($r = exec {fsx Test.fsx})
	equals $r.Count 2
	equals $r[0] 'fs: [|"Test.fsx"|]'
	equals $r[1] 'fsx: [|"Test.fsx"|]'
}

task test_01_diff_dir_fsx {
	($r = exec {fsx test_01\Test.fsx})
	equals $r.Count 2
	equals $r[0] 'fs: [|"test_01\Test.fsx"|]'
	equals $r[1] 'fsx: [|"test_01\Test.fsx"|]'
}

task test_01_diff_dir_use {
	($r = exec {fsx --exec --quiet --use:test_01\Test.fsx})
	equals $r.Count 2
	equals $r[0] 'fs: [|"test_01\Test.fsx"|]'
	equals $r[1] 'fsx: [|"test_01\Test.fsx"|]'
}

task test_01_diff_dir_ini {
	($r = exec {fsx test_01\.fs.ini test_01\Test.fsx})
	equals $r.Count 2
	equals $r[0] 'fs: [|"test_01\Test.fsx"|]'
	equals $r[1] 'fsx: [|"test_01\Test.fsx"|]'
}

task test_01_arguments_fsx {
	Set-Location test_01
	($r = exec {fsx Test.fsx 1 'a a' '"b"' '\"c\"'})
	#! depends on env
	assert ($r[0] -in @(
		'fs: [|"Test.fsx"; "1"; "a a"; ""b""; "\"c\""|]'
		'fs: [|"Test.fsx"; "1"; "a a"; "b"; ""c""|]'
	))
}

task test_01_arguments_fsi {
	Set-Location test_01
	($r = exec {& dotnet fsi Abc.fs Test.fsx 1 'a a' '"b"' '\"c\"'})
	#! depends on env
	assert ($r[0] -in @(
		'fs: [|"Test.fsx"; "1"; "a a"; "b"; ""c""|]'
		'fs: [|"Test.fsx"; "1"; "a a"; ""b""; "\"c\""|]'
		'fs: [|"Test.fsx"; "1"; "a a"; ""b""; "\"c\""; "--preferreduilang:en-US"|]'
	))
}

task sample1 {
	Set-Location ..\..\..\samples\fsx-sample
	($r = exec {fsx App1.fsx Joe})
	equals $r 'Hello, Joe!'
}

task sample2 {
	Set-Location ..\..\..\samples\fsx-sample
	($r = exec {'May' | fsx App2.fsx})
	equals $r.Count 2
	equals $r[0] 'Enter your name:'
	equals $r[1] 'Hello, May!'
}

task test_02 {
	($r = exec {fsx test_02\.fs.ini ..\..\..\samples\fsx-sample\App2.fsx})
	equals $r 'Hello, Dummy!'
}
