--[[
    Helper macros for some PowerShellFar commands.
    *USE YOUR OWN MACROS, THIS IS JUST AN EXAMPLE*
]]

-- Insert prefix "ps:" to empty command line
Macro {
  key="Space"; description="PSF: Easy prefix";
  area="Shell"; flags="EmptyCommandLine";
  action=function()
    Keys "p s :"
  end;
}

-- With prefix "ps:;" keep typing command and invoking by [Enter]
Macro {
  key="Enter"; description="PSF: Invoke selected in Enter-mode";
  area="Shell ShellAutoCompletion Info QView Tree";
  condition=function()
    return CmdLine.Value:sub(1, 4) == "ps:;"
  end;
  action=function()
    if Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[:ps:#invoke]])
  end;
}

-- With prefix "ps:;" clear command and keep this prefix
Macro {
  key="Esc"; description="PSF: [Esc] in Enter-mode";
  area="Shell Info QView Tree";
  condition=function()
    return CmdLine.Value:sub(1, 4) == "ps:;" and CmdLine.ItemCount > 4
  end;
  action=function()
    panel.SetCmdLine(nil, "ps:;")
  end;
}

-- With prefix "ps:;" do usual [Enter] and keep command line
Macro {
  key="ShiftEsc"; description="PSF: [Enter] in Enter-mode";
  area="Shell ShellAutoCompletion Info QView Tree";
  condition=function()
    return CmdLine.Value:sub(1, 4) == "ps:;"
  end;
  action=function()
    if Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#enter]])
  end;
}

-- Show PowerShell command history list
Macro {
  key="AltF10"; description="PSF: Command history";
  area="Common";
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#history]])
  end;
}

-- Complete PowerShell code in editors
Macro {
  key="CtrlSpace"; description="PSF: Complete";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#complete]])
  end;
}

-- Invoke PowerShell script "Complete-Word.ps1" in editors
Macro {
  key="F9"; description="PSF: Complete-Word.ps1";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[vps:Complete-Word.ps1]])
  end;
}
