
--[[
	RightWords macros

	* CtrlShiftSpace - Correct the word (editor, dialog, command line).
	* CtrlShiftF7 - Correct the selected text (editor).
	* CtrlShiftH - Switch highlighting (editor).
]]

local areaAnyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

Macro {
key="CtrlShiftSpace"; flags="DisableOutput"; description="RightWords: Correct word"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "CA7ECDC0-F446-4BFF-A99D-06C90FE0A3A9") then
	Keys("1")
end
end;
}

Macro {
area="Editor"; key="CtrlShiftF7"; flags="DisableOutput"; description="RightWords: Correct text"; action=function()
if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "CA7ECDC0-F446-4BFF-A99D-06C90FE0A3A9") then
	Keys("2")
end
end;
}

Macro {
area="Editor"; key="CtrlShiftH"; flags="DisableOutput"; description="RightWords: Highlighting"; action=function()
if not Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "10435532-9BB3-487B-A045-B0E6ECAAB6BC") then return end
if 0 == Menu.Select("Drawers") then return end Keys("Enter")
if 0 == Menu.Select("Spelling mistakes") then return end Keys("Enter")
end;
}
