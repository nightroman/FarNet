// When you run a script from the editor by [F5], simple results are shown in
// the editor title. Multiline results are shown in a separate temp editor.
// This may be effectively used for viewing some complex data.

function test() {
    // get current PowerShell variables
    const variables = FarNet.Tools.PowerShellFar.Invoke('Get-Variable')

    // convert to JS, keep just simple values for this demo
    const res = {}
    for (const x of variables) {
        if (x.Value == null || typeof x.Value != 'object') {
            res[x.Name] = x.Value
        }
    }
    return res
}

// stringify and view after [F5] in the editor
JSON.stringify(test(), null, 2)
