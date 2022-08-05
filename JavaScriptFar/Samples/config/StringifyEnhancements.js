// Without EnableStringifyEnhancements JSON.stringify returns undefined and
// this script fails because far.Message cannot accept undefined.

let dic = new clr.System.Collections.Generic.Dictionary(System.String, System.Object)
dic.Add('foo', 123)
dic.Add('bar', 'baz')

far.Message(JSON.stringify(dic))
