/*
This script is for testing running scripts as tasks.
It uses an artificial error in the end, for testing.
Normal scripts just finish normally.

js: @ task-with-error.js

    Far is blocked for 5 seconds while the script works.
    Then the script error shows.

js: task: @ task-with-error.js

    Use Far as usual, the script works in the background.
    In 5 seconds the script error shows.

One debugging scenario. Start debugging, start VSCode debugger, tick caught /
uncaught exceptions in the breakpoint panel. Run the script. The debugger
breaks on the script error.
*/

function test() {
    // argument
    let sleep = parseInt(args.milliseconds ?? 5000)

    // concurrent dictionary for tasks
    const data = clr.FarNet.User.Data

    // tell the testing script we start
    data.Item.set('_220723_1411_state', 'start')

    // simulate some job
    clr.System.Threading.Thread.Sleep(sleep)

    // tell the testing script we end
    data.Item.set('_220723_1411_state', 'end')

    // end
    throw Error('OK')
}

test()
