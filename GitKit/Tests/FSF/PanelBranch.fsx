// get panel cursor branch

open FarNet
open GitKit
open LibGit2Sharp

do
    let panel = Api.BranchesPanel()
    let file = panel.CurrentFile :?> Panels.BranchFile

    use repo = panel.UseRepository()
    let branch = repo.Branches[file.Name]

    User.Data["PanelBranch"] <- {|
        FriendlyName = branch.FriendlyName
        RemoteName = branch.RemoteName
        Message = branch.Tip.Message
        Author = branch.Tip.Author
    |}
