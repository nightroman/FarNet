-- RightWords sample macros

-- Helpers
local areaAnyEditor="Editor Dialog DialogAutoCompletion Shell ShellAutoCompletion Info QView Tree"
local isEditor=function() return not (Area.Shell or Area.ShellAutoCompletion or Area.Info or Area.QView or Area.Tree) or not CmdLine.Empty end

-- Correct the word (editor, dialog, command line)
Macro {
  key="CtrlShiftSpace"; description="RightWords: Correct word"; area=areaAnyEditor; condition=isEditor;
  action=function()
    if Area.DialogAutoCompletion or Area.ShellAutoCompletion then
      Keys "Esc"
    end
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "CA7ECDC0-F446-4BFF-A99D-06C90FE0A3A9") then
      Keys "1"
    end
  end;
}

-- Correct the selected text (editor)
Macro {
  key="CtrlShiftF7"; description="RightWords: Correct text"; area="Editor";
  action=function()
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "CA7ECDC0-F446-4BFF-A99D-06C90FE0A3A9") then
      Keys "2"
    end
  end;
}

-- Switch highlighting (editor)
Macro {
  key="CtrlShiftH"; description="RightWords: Highlighting"; area="Editor";
  action=function()
    if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "10435532-9BB3-487B-A045-B0E6ECAAB6BC") then
      if Menu.Select("Drawers") > 0 then
        Keys "Enter"
        if Menu.Select("Spelling mistakes") > 0 then
          Keys "Enter"
        end
      end
    end
  end;
}
