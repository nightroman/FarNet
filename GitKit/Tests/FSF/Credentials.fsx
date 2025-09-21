// get credentials, user name / password dialog

open FarNet
open GitKit
open LibGit2Sharp

do
    far.LoadModule("GitKit")
    let handler = Host.LocalCredentialsHandler("https://github.com/nightroman/FarNet.git", null, SupportedCredentialTypes.Default)

    User.Data["LocalCredentialsHandler"] <- handler
