
module GitKit
open FarNet
open GitKit
open LibGit2Sharp

let private setFarTitle prefix =
    let mutable title = far.UI.WindowTitle
    let sep = " :: "
    let ix = title.IndexOf(sep)
    if ix >= 0 then
        title <- title[(ix + sep.Length) ..]

    far.UI.WindowTitle <- prefix + sep + title

let setFarTitleGitBranch () =
    use repo = Api.TryRepository()
    if isNull repo then
        setFarTitle "no-repo"
    else
        let branch = repo.Head
        let name = branch.FriendlyName
        let status = repo.RetrieveStatus()
        let prefix = if status.IsDirty then $"*{name}" else $"{name}"
        setFarTitle prefix
