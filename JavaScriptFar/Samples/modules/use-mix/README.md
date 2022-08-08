# P.O.C. mixed modules in a session

This use case might be rare or unlikely but it works and covered by tests.

`_session.*` files .js, .cjs, .mjs are auto loaded in alphabetical order:

- [_session.1.js](_session.1.js) declares common variables
- [_session.2.cjs](_session.2.cjs) exposes CommonJS module assets for scripts
- [_session.2.mjs](_session.2.mjs) exposes Standard module assets for scripts

Then:

- [try.js](try.js) script uses assert from both CommonJS and Standard worlds
