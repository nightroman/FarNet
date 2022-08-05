// A message box with custom buttons and the choice index result.
// When run from editor by [F5], the result is shown in the title.

function test() {
    const args = new clr.FarNet.MessageArgs
    args.Text = 'Hello from JavaScript!'
    args.Caption = 'JavaScript'
    args.Buttons = host.newArr(System.String, 3)
    args.Buttons[0] = '&Ready'
    args.Buttons[1] = '&Steady'
    args.Buttons[2] = '&Go'
    return far.Message(args)
}

test()
