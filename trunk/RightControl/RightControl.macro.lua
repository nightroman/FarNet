
--[[
	RightControl macros

	6 macros for editor, dialog, command line:
	CtrlLeft/Right, CtrlShiftLeft/Right, CtrlBS/Del.

	4 editor macros:
	CtrlAltLeft/Right, Home/ShiftHome.

	Note: Home/ShiftHome can be used for dialog and command line, too, but
	smart home is not useful where text normally does not start with spaces.
]]

local areaAnyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local areaLineEditor="Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

Macro {
key="CtrlBS"; description="RightControl: delete left"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:delete-left")
end;
}

Macro {
key="CtrlDel"; description="RightControl: delete right"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:delete-right")
end;
}

Macro {
key="CtrlLeft"; description="RightControl: step left"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:step-left")
end;
}

Macro {
key="CtrlRight"; description="RightControl: step right"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:step-right")
end;
}

Macro {
key="CtrlShiftLeft"; description="RightControl: select left"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:select-left")
end;
}

Macro {
key="CtrlShiftRight"; description="RightControl: select right"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:select-right")
end;
}

Macro {
area="Editor"; key="CtrlAltLeft"; description="RightControl: vertical left"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:vertical-left")
end;
}

Macro {
area="Editor"; key="CtrlAltRight"; description="RightControl: vertical right"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:vertical-right")
end;
}

Macro {
area="Editor"; key="Home"; description="RightControl: go to smart home"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:go-to-smart-home")
end;
}

Macro {
area="Editor"; key="ShiftHome"; description="RightControl: select to smart home"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:select-to-smart-home")
end;
}
