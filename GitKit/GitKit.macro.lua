-- GitKit sample macros

Event {
  group = "FolderChanged"; description = "GitKit setenv";
  action = function() Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[gk:setenv name=_branch]]) end;
}

Macro {
  area="Common"; key="CtrlShiftB"; description = "GitKit setenv";
  action = function() Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", [[gk:setenv name=_branch]]) end;
}
