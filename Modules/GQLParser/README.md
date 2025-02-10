# FarNet script GQLParser

GQLParser provides methods for formatting GraphQL source code.
See [Format.cs](Format.cs) for available parameters.

**Format the specified or cursor file**

```
fn: script=GQLParser; method=.Format.File :: path=...
```

**Format the editor selected text**

```
fn: script=GQLParser; method=.Format.Editor
```

**Note**

For complex commands use command files

```
fn:@ <file>
```
