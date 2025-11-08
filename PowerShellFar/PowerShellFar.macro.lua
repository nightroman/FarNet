
Macro {
  key="Space"; description='PSF: Easy prefix, insert "ps:space" to command line';
  area="Shell"; flags="EmptyCommandLine";
  action=function()
    Keys "p s : Space"
  end;
}

Macro {
  key="Ctrl="; description="PSF Command console";
  area="Shell Editor Viewer";
  action = function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "vps:$Psf.StartCommandConsole()")
  end;
}

Macro {
  key="AltF10"; description="PSF: Command history";
  area="Common";
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#history]])
  end;
}

Macro {
  key="CtrlF9"; description="PSF: Line breakpoint";
  area="Editor"; filemask = "*.ps1,*.psm1";
  action=function()
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#line-breakpoint]])
  end;
}

Macro {
  key="F9"; description="PSF: Complete code";
  area="Dialog Editor Shell QView Tree Info DialogAutoCompletion ShellAutoCompletion";
  condition=function()
    return Area.Dialog or Area.Editor or Area.DialogAutoCompletion or Area.ShellAutoCompletion or not CmdLine.Empty
  end;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys "Esc" end
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[ps:#complete]])
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
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[vps:Complete-Word.ps1]])
  end;
}

CommandLine {
  prefixes = "cc"; description = "PSF: Command output to clipboard";
  action = function(prefix, text)
    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "vps:" .. text .. " | Set-Clipboard")
  end;
}

Macro {
  area="Editor"; key="AltF4"; description="Save, close, open as Far /e.";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "vps:Open-FarEditor -Detach")
  end;
}
