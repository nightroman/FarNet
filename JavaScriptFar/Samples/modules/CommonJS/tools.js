// Demo module for tests.

const Const = require('./const.js')

/**
 * Shows a message box.
 */
function hello() {
    far.Message(`Hello from ${Const.myName}`)
}

module.exports = { hello }
