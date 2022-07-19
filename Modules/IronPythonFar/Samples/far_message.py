# Using FarNet and .NET types.

import clr
clr.AddReference('FarNet')

from FarNet import *
from System import *

args = MessageArgs()
args.Text = 'from:' + __file__ + __name__
args.Caption = 'Hello'
args.Buttons = Array[str](['&Ready', '&Steady', '&Go'])

res = far.Message(args)
