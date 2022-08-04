// Without EnableStringifyEnhancements this script fails:
// Error: The best overloaded method match for 'FarNet.IFar.Message(FarNet.MessageArgs)' has some invalid arguments

let dic = new clr.System.Collections.Generic.Dictionary(System.String, System.Object)
dic.Add('foo', 123)
dic.Add('bar', 'baz')

far.Message(JSON.stringify(dic))
