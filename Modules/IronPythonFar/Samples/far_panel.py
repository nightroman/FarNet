# Primitive FarNet panel.

import clr
clr.AddReference('FarNet')

from FarNet import *
from System import *

class MyExplorer(Explorer):
    def __new__(self):
        return Explorer.__new__(self, Guid.Parse('87569a64-367b-4a60-86f7-11762d9e92e8'))

    def GetFiles(self, args):
        file1 = SetFile()
        file1.Name = 'file1'
        file2 = SetFile()
        file2.Name = 'file2'
        return [ file1, file2 ]

class MyPanel(Panel):
    def __new__(self, explorer):
        return Panel.__new__(self, explorer)

panel = MyPanel(MyExplorer())
panel.Title = 'IronPythonFar'
panel.Open()
