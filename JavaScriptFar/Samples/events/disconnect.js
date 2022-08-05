if (connection) {
    connection.disconnect()
    connection = null
    message('Disconnected.')
}
else {
    message('Not connected.')
}
