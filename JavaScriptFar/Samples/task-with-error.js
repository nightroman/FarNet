/*
This script is for testing running scripts as tasks.
It uses an artificial error in the end, for testing.
Normal scripts just finish normally.

js: @ task-with-error.js

	Far is blocked for 10 seconds while the script works.
	Then the script error shows.

js: @ task: task-with-error.js

	Use Far as usual, the script works in the background.
	In 10 seconds the script error shows.

js: @ task: debug: task-with-error.js

	This is useful in many ways. When the debugger breaks in the script you may
	use Far as usual and also use the debugger as long as Far stays opened.
*/

// concurrent dictionary for tasks
const data = clr.FarNet.User.Data

// try get the setting or use the default
let sleep = host.newVar(System.Object)
if (!data.TryGetValue('_220723_1411_sleep', sleep.out)) {
	sleep = 10000
}

// tell the testing script we start
data.Item.set('_220723_1411_state', 'start')

// simulate some long-ish job
clr.System.Threading.Thread.Sleep(host.cast(System.Int32, sleep))

// tell the testing script we end
data.Item.set('_220723_1411_state', 'end')

// end
throw Error('OK')
