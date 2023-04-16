using FarNet;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

[ModuleCommand(Name = Host.MyName, Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		AnyCommand? command = null;
		Repository? _repo = null;
		try
		{
			DbConnectionStringBuilder _parameters = Parameters.Parse(e.Command);
			string? value;

			if ((value = _parameters.GetValue("init")) is not null)
			{
				command = new InitCommand(value, _parameters);
			}
			else if ((value = _parameters.GetValue("clone")) is not null)
			{
				command = new CloneCommand(value, _parameters);
			}
			else
			{
				_repo = RepositoryFactory.Instance(Host.GetFullPath(_parameters.GetValue("repo")));

				if (_parameters.Count == 0)
				{
					command = new StatusCommand(_repo);
				}
				else if ((value = _parameters.GetValue("panel")) is not null)
				{
					command = new PanelCommand(_repo, value);
				}
				else if ((value = _parameters.GetValue("commit")) is not null)
				{
					command = new CommitCommand(_repo, value, _parameters);
				}
				else if ((value = _parameters.GetValue("checkout")) is not null)
				{
					command = new CheckoutCommand(_repo, value);
				}
			}

			_parameters.AssertNone();

			command?.Invoke();
		}
		catch (ModuleException)
		{
			throw;
		}
		catch (LibGit2SharpException ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
		finally
		{
			_repo?.Release();
		}
	}
}
