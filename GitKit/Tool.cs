﻿using FarNet;
using GitKit.About;
using GitKit.Commands;
using GitKit.Panels;
using LibGit2Sharp;
using System;

namespace GitKit;

[ModuleTool(Name = Host.MyName, Options = ModuleToolOptions.Panels, Id = "7250a2f6-4eb1-4771-bf69-7a0bccd5c516")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = Host.MyName;
		menu.HelpTopic = GetHelpTopic("menu");

		if (Far.Api.Panel is AbcPanel panel)
		{
			panel.AddMenu(menu);
		}
		else
		{
			menu.Add(Const.CopySha, (s, e) => CopySha());
			menu.Add(Const.BlameFile, (_, _) => BlameFile());
			menu.Add(Const.CommitLog, (_, _) => CommitLog());
		}

		menu.Add(Const.Help, (s, e) => Host.Instance.ShowHelpTopic(string.Empty));

		menu.Show();
	}

	static void CopySha()
	{
		try
		{
			using var repo = new Repository(Lib.GetGitDir(Far.Api.CurrentDirectory));

			UI.CopySha(Lib.GetExistingTip(repo));
		}
		catch (Exception ex)
		{
			throw new ModuleException(ex.Message);
		}
	}

	static void BlameFile()
	{
		new BlameCommand(CommandParameters.Parse("blame")).Invoke();
	}

	static void CommitLog()
	{
		new CommitsCommand(CommandParameters.Parse("commits path=?")).Invoke();
	}
}
