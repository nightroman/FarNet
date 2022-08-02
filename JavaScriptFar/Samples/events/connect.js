if (connection) {
    far.Message('Already connected, open any editor.')
}
else {
    connection = far.AnyEditor.Opened.connect(() => {
        far.Message('Opened ' + far.Editor.FileName + '\nRun disconnect.js to disconnect.')
    })

    far.Message('Connected, open any editor.')
}
