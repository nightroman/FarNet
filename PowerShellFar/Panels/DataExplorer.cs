
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;

namespace PowerShellFar;

sealed class DataExplorer : TableExplorer
{
	const string TypeIdString = "f61f95e9-42fc-4285-9858-3ec7c0d7a58e";

	internal DataPanel? Panel { get; set; }

	public DataExplorer() : base(new Guid(TypeIdString))
	{
		FileComparer = new FileDataComparer();
		Location = "*";
		Functions =
			ExplorerFunctions.DeleteFiles |
			ExplorerFunctions.CreateFile |
			ExplorerFunctions.GetContent;
	}

	///
	public override Panel DoCreatePanel()
	{
		throw new ModuleException("Data panel is not yet supported.");
	}

	///
	public override IList<FarFile> DoGetFiles(GetFilesEventArgs args) => Panel!.Explore(args)!;

	///
	public override void DoDeleteFiles(DeleteFilesEventArgs args) => Panel!.DoDeleteFiles(args);

	///
	public override void DoCreateFile(CreateFileEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		args.Result = JobResult.Ignore;
		Panel!.DoCreateFile();
	}

	///
	public override void DoGetContent(GetContentEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		args.UseText = A.InvokeFormatList(args.File.Data, false);
	}
}
