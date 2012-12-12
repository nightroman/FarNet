
--[[
	RightControl macros

	6 macros in the common area:

		CtrlLeft/Right
		CtrlShiftLeft/Right
		CtrlBS/Del

	4 editor macros:

		CtrlAltLeft/Right,
		Home/ShiftHome.

	2 workaround macros ShiftLeft/Right in the common area.
	They should be removed when Mantis 1465 is resolved.

	Note: Home/ShiftHome can be used in the common area, too, but file makes
	them for the editor only. Smart home is not really useful for line editors
	where text normally does not start with spaces.
]]

Macro {
area="Common"; key="CtrlBS"; description="RightControl: delete left"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Editor or Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:delete-left")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="CtrlDel"; description="RightControl: delete right"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Editor or Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:delete-right")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="CtrlLeft"; description="RightControl: step left"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Editor or Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:step-left")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="CtrlRight"; description="RightControl: step right"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Editor or Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:step-right")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="CtrlShiftLeft"; description="RightControl: select left"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Editor or Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:select-left")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="CtrlShiftRight"; description="RightControl: select right"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Editor or Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:select-right")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="ShiftLeft"; description="RightControl: workaround left"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:vertical-left")
else
	Keys("AKey")
end
end;
}

Macro {
area="Common"; key="ShiftRight"; description="RightControl: workaround right"; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end

if Area.Dialog or ((Area.Shell or Area.Info or Area.QView or Area.Tree) and not CmdLine.Empty) then
	Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:vertical-right")
else
	Keys("AKey")
end
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
