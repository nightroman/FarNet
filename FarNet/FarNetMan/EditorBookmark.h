
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class EditorBookmark sealed : IEditorBookmark
{
public:
	virtual ICollection<TextFrame>^ Bookmarks() override;
	virtual void AddSessionBookmark() override;
	virtual void ClearSessionBookmarks() override;
	virtual void RemoveSessionBookmarkAt(int index) override;
	virtual ICollection<TextFrame>^ SessionBookmarks() override;
	virtual void GoToNextSessionBookmark() override;
	virtual void GoToPreviousSessionBookmark() override;
internal:
	static EditorBookmark Instance;
private:
	static TextFrame NewTextFrame(const EditorBookMarks& bookmarks, int index);
};
}
