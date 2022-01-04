--[=[
    This file contains example macros which invoke PowerShellFar commands.
    DO NOT USE THIS AS IT IS, USE YOUR OWN MACROS, THIS IS ONLY AN EXAMPLE
]=]

local FarNet=function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
local anyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

Macro {
  area="Shell"; key="Space"; flags="EmptyCommandLine"; description="PSF: Easy prefix"; action=function()
  Keys "p s : Space"
  end;
}

Macro {
  area="Shell"; key="Ctrl"; flags="EmptyCommandLine"; description="PSF: Command console"; action=function()
  FarNet [[vps: $Psf.StartCommandConsole()]]
  end;
}

Macro {
  area="Shell"; key="F10"; description="PSF: Quit Far"; action=function()
  if not FarNet [[vps:$Far.Quit()]] then Keys "F10" end
  end;
}

Macro {
  area="Common"; key="AltF10"; description="PSF: Command history"; action=function()
  FarNet [[vps:$Psf.ShowHistory()]]
  end;
}

Macro {
  area=anyEditor; condition=isEditor; key="CtrlSpace"; description="PSF: Complete-Word-.ps1"; action=function()
  if Area.DialogAutoCompletion then Keys "Esc" end
  FarNet [[vps:Complete-Word-.ps1]]
  end;
}
