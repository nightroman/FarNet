--[[
    Vessel module macros

    * AltF11 - smart file history instead of standard
    * CtrlShiftF11 - standard "File view history"

    * AltF12 - smart folder history instead of standard
    * CtrlShiftF12 - standard "Folder history"

    * AltF8 - smart command history instead of standard
    * CtrlShiftF8 - standard "Command history"
]]

Macro {
  area="Common"; key="AltF11"; description="Vessel: Smart file history";
  action=function()
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "58AD5E13-D2BA-4F4C-82CD-F53A66E9E8C0") then
        Keys "1"
    else
        Keys "AKey"
    end
  end;
}

Macro {
  area="Common"; key="CtrlShiftF11"; description="Vessel: Standard file history";
  action=function()
    Keys "AltF11"
  end;
}

Macro {
  area="Common"; key="AltF12"; description="Vessel: Smart folder history";
  action=function()
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "58AD5E13-D2BA-4F4C-82CD-F53A66E9E8C0") then
        Keys "2"
    else
        Keys "AKey"
    end
  end;
}

Macro {
  area="Shell QView Tree Info ShellAutoCompletion"; key="CtrlShiftF12"; description="Vessel: Standard folder history";
  action=function()
    Keys "AltF12"
  end;
}

Macro {
  area="Editor Viewer Shell QView Tree Info ShellAutoCompletion"; key="AltF8"; description="Vessel: Smart command history";
  action=function()
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "58AD5E13-D2BA-4F4C-82CD-F53A66E9E8C0") then
        Keys "3"
    else
        Keys "AKey"
    end
  end;
}

Macro {
  area="Shell QView Tree Info ShellAutoCompletion"; key="CtrlShiftF8"; description="Vessel: Standard command history";
  action=function()
    Keys "AltF8"
  end;
}
