// Without EnableAllLoading and DocumentSearchPath this script fails:
// Error: Could not load file or assembly 'const.js'. The system cannot find the file specified.

import * as Const from 'const.js'

far.Message(Const.myName)
