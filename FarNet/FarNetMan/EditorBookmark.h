
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
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
};
}
