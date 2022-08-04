/*
Use debugger to see debug console output (JavaScriptFar/README ~ setup VSCode debugger)

- open this script in Far Manager editor
- [ShiftF5] to start debugging, click OK, wait for VSCode, start debugger there
- debugger breaks into this script
- step through the script or just [F5] to continue
- watch the output in debug console
- note that debugger stays connected
- [F5] in Far Manager editor to repeat
- or modify the script and then [F5]
- see new output in debug console

https://developer.mozilla.org/en-US/docs/Web/API/console
*/

// clear console and start timer
console.clear()
console.time("time1")

// messages
console.log('my log')
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
