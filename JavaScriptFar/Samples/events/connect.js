if (connection) {
    message('Already connected, open any editor.')
}
else {
    connection = far.AnyEditor.Opened.connect((editor) => {
        message(`Opened ${editor.FileName}\nRun disconnect.js to disconnect.`)
    })

    message('Connected, open any editor.')
}
