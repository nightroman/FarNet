--[[
    Sample macros for some PowerShellFar commands.
    *USE YOUR OWN MACROS, THIS IS JUST AN EXAMPLE*
]]

Macro {
  area="Shell"; key="Space"; description="PSF: Easy prefix";
  flags="EmptyCommandLine";
  action=function()
    Keys "p s : Space"
  end;
}

Macro {
  area="Common"; key="AltF10"; description="PSF: Command history";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[vps:$Psf.ShowHistory()]])
  end;
}

Macro {
  key="F9"; description="PSF: TabExpansion";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7") then
      Keys "7"
    end
  end;
}

Macro {
  key="CtrlSpace"; description="PSF: Complete-Word.ps1";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[vps:Complete-Word.ps1]])
  end;
}
