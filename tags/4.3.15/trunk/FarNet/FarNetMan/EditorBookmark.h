
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class EditorBookmark sealed : IEditorBookmark
{
public:
	virtual ICollection<TextFrame>^ Bookmarks() override;
	virtual void AddStackBookmark() override;
	virtual void ClearStackBookmarks() override;
	virtual void RemoveStackBookmarkAt(int index) override;
	virtual ICollection<TextFrame>^ StackBookmarks() override;
	virtual void GoToNextStackBookmark() override;
	virtual void GoToPreviousStackBookmark() override;
internal:
	static EditorBookmark Instance;
private:
	static TextFrame NewTextFrame(const EditorBookMarks& bookmarks, int index);
};
}
