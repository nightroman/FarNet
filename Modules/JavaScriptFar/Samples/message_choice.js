// When it runs by from the editor by F5 it shows the `res` value.
// Points of interest:
// - how to create FarNet types
// - how to create .NET arrays

args = new lib.FarNet.MessageArgs
args.Text = 'Hello from JavaScript!'
args.Caption = 'JavaScript'
args.Buttons = host.newArr(System.String, 3)
args.Buttons[0] = '&Ready'
args.Buttons[1] = '&Steady'
args.Buttons[2] = '&Go'

res = far.Message(args)
