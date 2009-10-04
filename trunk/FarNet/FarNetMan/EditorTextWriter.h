/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class EditorTextWriter : TextWriter
{
	IEditor^ _editor;
internal:
	EditorTextWriter(IEditor^ editor);
public:
	virtual void Write(Char value) override;
	virtual void Write(String^ value) override;
	virtual property System::Text::Encoding^ Encoding { System::Text::Encoding^ get() override { return System::Text::Encoding::Unicode; } }
};
}
