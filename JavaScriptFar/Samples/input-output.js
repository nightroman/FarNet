// Input dialog with result printed to the console.

res = far.Input('Enter your name', null, 'JavaScript', 'John Doe')
if (res) {
    far.UI.WriteLine('Hello, ' + res)
}
