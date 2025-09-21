// get panel cursor commit

open FarNet
open GitKit
open LibGit2Sharp

do
    let panel = Api.CommitsPanel()
    let file = panel.CurrentFile :?> Panels.CommitFile

    use repo = panel.UseRepository()
    let commit = repo.Lookup<Commit>(file.CommitSha)

    User.Data["PanelCommit"] <- {|
        Author = commit.Author
        Message = commit.Message
    |}
