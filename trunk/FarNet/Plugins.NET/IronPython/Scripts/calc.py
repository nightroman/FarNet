# IronPython calculator
from System import *
def calc(sender, e):
	s = far.Input("Enter expression", "IronPythonCalculator", '', None)
	if s:
		far.Msg( "Result=" + repr(eval(s)))

# registers ipy prefix:
	# type
		# ipy:<statement> to run IronPython statement
		# ipy:?<expression> to evaluate IronPython expression
def prefixHandler(s):
	far.Write('ipy:'+s, ConsoleColor.DarkYellow, ConsoleColor.Black)
	if s.startswith('?'):
		far.Write(repr(eval(s[1:])))
	else:
		exec s
far.RegisterPluginsMenuItem("calc.ipy", calc)
far.RegisterPrefix("ipy", prefixHandler)