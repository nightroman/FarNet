
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
ref class History sealed : IHistory
{
public:
	virtual array<HistoryInfo^>^ GetHistory(GetHistoryArgs^ args) override;
internal:
	static History Instance;
private:
	History() {}
};
}
