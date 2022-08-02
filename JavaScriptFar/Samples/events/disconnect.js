if (connection) {
    connection.disconnect()
    connection = null
    far.Message('Disconnected.')
}
else {
    far.Message('Not connected.')
}
