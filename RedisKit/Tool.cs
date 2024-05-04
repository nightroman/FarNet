using FarNet;

namespace RedisKit;

[ModuleTool(Name = Host.MyName, Options = ModuleToolOptions.Panels, Id = "06f748aa-e597-4ea8-933f-88fc89e450d9")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = Host.MyName;
		menu.HelpTopic = GetHelpTopic("menu");

		if (Far.Api.Panel is AnyPanel panel)
		{
			panel.AddMenu(menu);
		}
		else
		{
		}

		menu.Add("Help", (s, e) => Host.Instance.ShowHelpTopic(string.Empty));

		menu.Show();
	}
}
