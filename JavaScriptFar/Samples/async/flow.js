// Example of async flow with non modal intermediate steps.

// Shows a non modal dialog and gets the user input or null.
async function showDialog(title) {
    let dialog = far.CreateDialog(-1, -1, 52, 3)
    dialog.AddBox(0, 0, 0, 0, title)
    let edit = dialog.AddEdit(1, -1, 50, '')
    function result(args) {
        return args.Control ? edit.Text : null
    }
    return await clr.FarNet.Tasks.Dialog(dialog, host.func(1, result)).ToPromise()
}

// Shows two non modal dialogs and the final result message.
// If any dialog gets nothing then the flow stops.
async function show() {
    let res1 = await showDialog('dialog1')
    if (!res1)
        return
    let res2 = await showDialog(res1)
    if (!res2)
        return
    await clr.FarNet.Tasks.Job(host.proc(0, () => far.Message(res2, 'result')))
}

show()
