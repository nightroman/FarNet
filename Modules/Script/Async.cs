using FarNet;
using System;
using System.Threading.Tasks;

namespace Script;

// Type with async methods.
public static class Async
{
	// fn: script=Script; method=Script.Async.Test; unload=true
	public static async Task Test()
	{
		var dialog = Far.Api.CreateDialog(-1, -1, 50, 3);
		dialog.AddBox(0, 0, 0, 0, "Modeless dialog");
		var edit = dialog.AddEdit(1, -1, 48, "");

		var res = await Tasks.Dialog(dialog, e =>
		{
			return e.Control is null ? null : edit.Text;
		});

		if (res is null)
			return;

		if (res.Trim().Length == 0)
			throw new Exception("Empty string!");

			await Far.Api.PostJobAsync(() =>
		{
			Far.Api.Message(res, "You entered");
		});
	}
}
