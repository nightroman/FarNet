
--[[
	This file contains example macros which invoke PowerShellFar features.
	DO NOT USE THIS AS IT IS, USE YOUR OWN MACROS, THIS IS ONLY AN EXAMPLE
]]

local areaAnyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

Macro {
area="Shell"; key="Space"; flags="EmptyCommandLine"; description="PSF: Easy prefix"; action=function()
Keys("p s : Space")
end;
}

Macro {
area="Shell"; key="F5"; flags="NotEmptyCommandLine"; description="PSF: Easy invoke"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "::ps:$Psf.InvokeSelectedCode()")
end;
}

Macro {
key="CtrlSpace"; description="PSF: Complete-Word-.ps1"; area=areaAnyEditor; condition=isEditor; action=function()
if Area.DialogAutoCompletion then Keys("Esc") end
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps:Complete-Word-.ps1")
end;
}

Macro {
area="Common"; key="CtrlShiftL"; description="PSF: Favorites menu"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps:Menu-Favorites-.ps1")
end;
}

Macro {
area="Common"; key="AltF10"; description="PSF: Command history"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps:$Psf.ShowHistory()")
end;
}

Macro {
area="Common"; key="CtrlShiftP"; description="PSF: Command box"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps:$Psf.InvokeInputCode()")
end;
}

Macro {
area="Editor"; key="CtrlShiftM"; description="PSF: Show editor bookmarks"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":vps:Select-Bookmark-")
end;
}

Macro {
area="Shell"; key="F10"; description="PSF: Quit Far"; action=function()
if not Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":ps:$Far.Quit()") then Keys("F10") end
end;
}

Macro {
area="Shell"; key="AltF12"; description="PSF: Folder history"; action=function()
Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":ps:Show-History-.ps1 -Folder")
end;
}

Macro {
area="Shell"; key="CtrlShiftF12"; description="PSF: Standard folder history"; action=function()
Keys("AltF12")
end;
}
