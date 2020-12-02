
--[=[
    This file contains example macros which invoke PowerShellFar features.
    DO NOT USE THIS AS IT IS, USE YOUR OWN MACROS, THIS IS ONLY AN EXAMPLE

    Requires:
    _G.FarNet = function(cmd) return Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", cmd) end
]=]

local areaAnyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

Macro {
  area="Shell"; key="Space"; flags="EmptyCommandLine"; description="PSF: Easy prefix"; action=function()
  Keys "p s : Space"
  end;
}
Macro {
  area="Shell"; key="-"; flags="EmptyCommandLine"; description="PSF: Run in console"; action=function()
  FarNet [[::ps: Invoke-Far]]
  end;
}

Macro {
  area="Shell"; key="F5"; flags="NotEmptyCommandLine"; description="PSF: Easy invoke"; action=function()
  FarNet [[::ps:$Psf.InvokeSelectedCode()]]
  end;
}

Macro {
  key="CtrlSpace"; description="PSF: Complete-Word-.ps1"; area=areaAnyEditor; condition=isEditor; action=function()
  if Area.DialogAutoCompletion then Keys "Esc" end
  FarNet [[vps:Complete-Word-.ps1]]
  end;
}

Macro {
  area="Common"; key="CtrlShiftL"; description="PSF: Favorites menu"; action=function()
  FarNet [[:vps:Menu-Favorites-.ps1]]
  end;
}

Macro {
  area="Common"; key="AltF10"; description="PSF: Command history"; action=function()
  FarNet [[vps:$Psf.ShowHistory()]]
  end;
}

Macro {
  area="Common"; key="CtrlShiftP"; description="PSF: Command box"; action=function()
  FarNet [[:vps:$Psf.InvokeInputCode()]]
  end;
}

-- used to hang with `:ps:` -- https://bugs.farmanager.com/view.php?id=3354
Macro {
  area="Shell"; key="F10"; description="PSF: Quit Far"; action=function()
  if not FarNet [[ps:$Far.Quit()]] then Keys "F10" end
  end;
}
