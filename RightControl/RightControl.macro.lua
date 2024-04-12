-- RightControl macros

local areaCmdLine="Shell Info QView Tree"
local areaAnyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

-- editor, dialog, command line: CtrlLeft/Right, CtrlShiftLeft/Right, CtrlBS/Del

Macro {
  key="CtrlBS"; description="RightControl: delete left";
  area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:delete-left]])
  end;
}

Macro {
  key="CtrlDel"; description="RightControl: delete right";
  area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:delete-right]])
  end;
}

Macro {
  key="CtrlLeft"; description="RightControl: step left";
  area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:step-left]])
  end;
}

Macro {
  key="CtrlRight"; description="RightControl: step right";
  area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:step-right]])
  end;
}

Macro {
  key="CtrlShiftLeft"; description="RightControl: select left";
  area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:select-left]])
  end;
}

Macro {
  key="CtrlShiftRight"; description="RightControl: select right";
  area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then Keys("Esc") end
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:select-right]])
  end;
}

-- editor macros: CtrlAltLeft/Right, Home/ShiftHome.

Macro {
  key="CtrlAltLeft"; description="RightControl: vertical left";
  area="Editor";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:vertical-left]])
  end;
}

Macro {
  key="CtrlAltRight"; description="RightControl: vertical right";
  area="Editor";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:vertical-right]])
  end;
}

Macro {
  key="Home"; description="RightControl: go to smart home";
  area="Editor";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:go-to-smart-home]])
  end;
}

Macro {
  key="ShiftHome"; description="RightControl: select to smart home";
  area="Editor";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:select-to-smart-home]])
  end;
}

-- command line: Home/ShiftHome, End/ShiftEnd

Macro {
  key="Home"; description="RightControl: go to smart home /CtrlHome";
  area=areaCmdLine; flags="NotEmptyCommandLine";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:go-to-smart-home]])
  end;
}

Macro {
  key="ShiftHome"; description="RightControl: select to smart home /AltShiftHome";
  area=areaCmdLine; flags="NotEmptyCommandLine";
  action=function()
    Plugin.Call("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[RightControl:select-to-smart-home]])
  end;
}

Macro {
  key="End"; description="Go to command line end /CtrlEnd";
  area=areaCmdLine; flags="NotEmptyCommandLine";
  action=function()
    Keys "CtrlEnd"
  end;
}

Macro {
  key="ShiftEnd"; description="Select command line to end /AltShiftEnd";
  area=areaCmdLine; flags="NotEmptyCommandLine";
  action=function()
    Keys "AltShiftEnd"
  end;
}
