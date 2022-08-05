/*
Use debugger to watch console output (JavaScriptFar/README ~ setup VSCode debugger)
See also: console -- https://developer.mozilla.org/en-US/docs/Web/API/console

- go to this script panel
- F11 / JavaScriptFar / Start debugging
- in the opened VSCode start the debugger
- run the script, see the output in debug console
- modify the script, run (F5), see the changed output
*/

// clear console and start timer
console.clear()
console.time("time1")

// messages
console.log(Date())
console.info('my info')
console.warn('my warn')
console.debug('my debug')
console.error('my error')

// groups
console.log("This is the outer level")
console.group()
console.log("Level 2")
console.group()
console.log("Level 3")
console.warn("More of level 3")
console.groupEnd()
console.log("Back to level 2")
console.groupEnd()
console.log("Back to the outer level")

// log timer
console.timeLog("time1")

// object
function Person(firstName, lastName) {
  this.firstName = firstName
  this.lastName = lastName
}
console.log(new Person("John", "Smith"))

// end timer
console.timeEnd("time1")
