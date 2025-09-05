
-- Insert "ps: " to empty command line
Macro {
  key="Space"; description="PSF: Easy prefix";
  area="Shell"; flags="EmptyCommandLine";
  action=function()
    Keys "p s : Space"
  end;
}

-- PowerShell command history
Macro {
  key="AltF10"; description="PSF: Command history";
  area="Common";
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#history]])
  end;
}

-- PowerShell line breakpoint
Macro {
  key="CtrlF9"; description="PSF: Line breakpoint";
  area="Editor"; filemask = "*.ps1,*.psm1";
  action=function()
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#line-breakpoint]])
  end;
}

-- Complete PowerShell code
Macro {
  key="F9"; description="PSF: Complete";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#complete]])
  end;
}

-- Invoke "Complete-Word.ps1"
Macro {
  key="CtrlSpace"; description="PSF: Complete-Word.ps1";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[vps:Complete-Word.ps1]])
  end;
}
