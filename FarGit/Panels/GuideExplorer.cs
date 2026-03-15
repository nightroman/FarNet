using FarNet;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="GuidePanel"/>.
/// Provides a static, read-only git reference guide.
/// </summary>
public class GuideExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"))
{
	public override Panel CreatePanel() => new GuidePanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		// ── Core Concepts ──────────────────────────────────────────────────
		yield return Header("═══  CORE CONCEPTS  ═══");

		yield return Guide("Concept", "repository",
			"A folder tracked by git — contains all your history",
			"REPOSITORY\n\n" +
			"A git repository is just a folder that git is tracking.\n" +
			"It contains:\n\n" +
			"  • All your project files (the working tree)\n" +
			"  • The entire history of every change ever made\n" +
			"  • A hidden '.git' folder where git stores everything\n\n" +
			"You create one with 'git init' or get one via 'git clone'.\n\n" +
			"Because git stores all history locally, you can browse\n" +
			"commits, create branches, and work offline — no server needed.");

		yield return Guide("Concept", "commit",
			"A saved snapshot of your project at a point in time",
			"COMMIT\n\n" +
			"A commit is like a photograph of your entire project at\n" +
			"one moment in time. Each commit stores:\n\n" +
			"  • The content of every tracked file\n" +
			"  • Who made the change (name + email)\n" +
			"  • When the change was made\n" +
			"  • A message describing what changed and why\n" +
			"  • A link to the previous commit (its 'parent')\n\n" +
			"All commits together form a timeline — the history of your\n" +
			"project. You can go back to any commit at any time.\n\n" +
			"Each commit has a unique ID (SHA) like 'a1b2c3d'. You only\n" +
			"need the first 7 characters to identify one.");

		yield return Guide("Concept", "branch",
			"A parallel line of development — a moveable pointer",
			"BRANCH\n\n" +
			"A branch is a name that points to a specific commit.\n" +
			"When you make a new commit, the branch pointer moves forward.\n\n" +
			"Branches let you:\n" +
			"  • Work on a new feature without affecting main code\n" +
			"  • Fix a bug while someone else works on something else\n" +
			"  • Experiment safely — just delete the branch if it fails\n\n" +
			"The default branch is usually called 'main' (or 'master').\n\n" +
			"Common workflow:\n" +
			"  1. Create branch 'feature/login'\n" +
			"  2. Make commits on that branch\n" +
			"  3. Merge back into 'main' when done\n" +
			"  4. Delete the feature branch");

		yield return Guide("Concept", "HEAD",
			"Where you are right now in the repository",
			"HEAD\n\n" +
			"HEAD is git's way of saying 'where you currently are'.\n\n" +
			"Usually HEAD points to a branch (e.g. 'main'), and that\n" +
			"branch points to the latest commit.\n\n" +
			"When you:\n" +
			"  • Switch branches: HEAD moves to that branch\n" +
			"  • Make a commit: the branch + HEAD both advance\n\n" +
			"'Detached HEAD' means HEAD points directly to a commit\n" +
			"rather than a branch. This is fine for browsing history\n" +
			"but create a branch before making new commits in that state.");

		yield return Guide("Concept", "staging area",
			"Files marked to include in the next commit",
			"STAGING AREA  (also called 'the index')\n\n" +
			"The staging area is a preparation zone between your files\n" +
			"and a commit. Think of it like putting items into a box:\n\n" +
			"  Working tree → [staging area] → commit\n\n" +
			"Why stage first? Because you might have changed 10 files\n" +
			"but only want to commit 3 of them right now.\n\n" +
			"Staging lets you be precise:\n" +
			"  • Stage exactly the files that belong together\n" +
			"  • Write a commit message that accurately describes them\n" +
			"  • Keep unrelated changes out of that commit\n\n" +
			"In FarGit: open Status, press Space to mark files, then F5 to stage.");

		yield return Guide("Concept", "remote",
			"A copy of your repository hosted elsewhere",
			"REMOTE\n\n" +
			"A remote is a URL pointing to another copy of your repo,\n" +
			"typically on a server like GitHub, GitLab, or Bitbucket.\n\n" +
			"The most common remote is called 'origin' — it's the server\n" +
			"you cloned from, or where you want to share your work.\n\n" +
			"Remote operations:\n" +
			"  fetch  — download updates, don't change your files yet\n" +
			"  pull   — download + merge updates into your current branch\n" +
			"  push   — upload your commits to the remote server\n" +
			"  clone  — create a local copy of a remote repository\n\n" +
			"You can have multiple remotes (e.g. 'origin' and 'upstream').");

		yield return Guide("Concept", "merge",
			"Combining the history of two branches",
			"MERGE\n\n" +
			"Merging takes the commits from one branch and combines them\n" +
			"with another. Two things can happen:\n\n" +
			"FAST-FORWARD merge (clean)\n" +
			"  The target branch hadn't moved, so git just moves the\n" +
			"  pointer forward. No merge commit is created.\n\n" +
			"MERGE COMMIT (when both branches have new commits)\n" +
			"  Git creates a special commit with two parents, recording\n" +
			"  that these two histories were combined.\n\n" +
			"CONFLICTS\n" +
			"  If the same part of a file was changed on both branches,\n" +
			"  git can't decide which to keep. It marks the file with\n" +
			"  conflict markers (<<<<<<<, =======, >>>>>>>) and asks you\n" +
			"  to resolve them manually, then stage and commit.");

		yield return Guide("Concept", "stash",
			"Temporary storage for uncommitted changes",
			"STASH\n\n" +
			"A stash saves your current uncommitted changes privately\n" +
			"so you can work on something else, then come back.\n\n" +
			"Think of it as a clipboard for work-in-progress.\n\n" +
			"Common uses:\n" +
			"  • You're mid-feature but need to fix a bug urgently\n" +
			"  • You need to pull updates but have unstaged changes\n" +
			"  • You want to try a different approach without losing this one\n\n" +
			"Operations:\n" +
			"  Apply — restore stash, keep it in the stash list\n" +
			"  Pop   — restore stash and remove it from the list\n" +
			"  Drop  — discard the stash permanently");

		yield return Guide("Concept", "tag",
			"A named bookmark on a specific commit",
			"TAG\n\n" +
			"A tag is a permanent, human-readable name for a commit.\n" +
			"Unlike branches, tags don't move — they stay on their commit.\n\n" +
			"Use tags to mark releases and milestones:\n" +
			"  v1.0, v2.3.1, beta-1, release-2024-03\n\n" +
			"Two types:\n" +
			"  Lightweight — just a name pointing to a commit\n" +
			"  Annotated   — includes tagger name, date, and message\n\n" +
			"To share tags with others you must explicitly push them:\n" +
			"  git push origin v1.0\n" +
			"  git push origin --tags  (push all tags)");

		// ── Common Operations ──────────────────────────────────────────────
		yield return Header("═══  COMMON OPERATIONS  ═══");

		yield return Guide("Operation", "git init",
			"Start tracking a folder as a git repository",
			"git init\n\n" +
			"Creates a new, empty git repository in the current folder.\n" +
			"It adds a hidden '.git' folder to store all git data.\n\n" +
			"Use this when starting a brand-new project:\n" +
			"  1. Navigate to your project folder\n" +
			"  2. Run git init\n" +
			"  3. Add your files and make your first commit\n\n" +
			"Note: Only do this once per project. If you cloned the repo,\n" +
			"git init is not needed — the repo already exists.");

		yield return Guide("Operation", "git clone",
			"Copy a remote repository to your machine",
			"git clone <url>\n\n" +
			"Downloads a complete copy of a remote repository, including\n" +
			"all history, branches, and tags.\n\n" +
			"Example:\n" +
			"  git clone https://github.com/user/project.git\n\n" +
			"This creates a folder called 'project' with all the files.\n" +
			"The remote is automatically named 'origin'.\n\n" +
			"In FarGit: Guide Me → Clone a repository.");

		yield return Guide("Operation", "git status",
			"See what has changed since the last commit",
			"git status\n\n" +
			"Shows the current state of your working tree:\n" +
			"  Staged    — changes ready to commit\n" +
			"  Unstaged  — changed files not yet staged\n" +
			"  Untracked — new files git doesn't know about\n\n" +
			"In FarGit: the Status panel (Dashboard → Enter on any status row)\n" +
			"shows this information visually, colour-coded by state.");

		yield return Guide("Operation", "git add / stage",
			"Mark changes to include in the next commit",
			"git add <file>   or   git add .\n\n" +
			"Stages (marks) files so they'll be included in the next commit.\n\n" +
			"  git add filename.txt   — stage one file\n" +
			"  git add .              — stage everything\n\n" +
			"In FarGit (Status panel):\n" +
			"  F5             — stage the highlighted file\n" +
			"  Space + F5     — stage marked (selected) files\n" +
			"  Dashboard F5   — stage all changes at once");

		yield return Guide("Operation", "git commit",
			"Save staged changes as a new snapshot",
			"git commit -m \"message\"\n\n" +
			"Creates a new commit from everything in the staging area.\n\n" +
			"Write a short, meaningful message:\n" +
			"  Good: 'Fix password reset link in email'\n" +
			"  Bad:  'stuff' / 'fix' / 'asdf'\n\n" +
			"Convention for message format:\n" +
			"  • First line: short summary (50 chars max)\n" +
			"  • Blank line\n" +
			"  • Optional longer explanation\n\n" +
			"In FarGit: F7 from any panel opens the commit editor.");

		yield return Guide("Operation", "git push",
			"Upload local commits to a remote server",
			"git push\n\n" +
			"Sends your local commits to the remote server so others\n" +
			"can see them (or so you have a backup).\n\n" +
			"First push of a new branch:\n" +
			"  git push -u origin branch-name\n" +
			"  (-u sets the upstream so future pushes just need 'git push')\n\n" +
			"In FarGit: Remote panel → F7, or Guide Me → Share my work.");

		yield return Guide("Operation", "git pull",
			"Download and merge remote changes into current branch",
			"git pull\n\n" +
			"Fetches changes from the remote and merges them into your\n" +
			"current branch. Equivalent to:\n" +
			"  git fetch + git merge\n\n" +
			"If there are conflicts, you'll need to resolve them manually.\n\n" +
			"Best practice: commit or stash your work before pulling.\n\n" +
			"In FarGit: Remote panel → F6, or Guide Me → Get latest changes.");

		yield return Guide("Operation", "git fetch",
			"Download remote changes without merging",
			"git fetch\n\n" +
			"Downloads all changes from the remote but does NOT modify\n" +
			"your working files or current branch.\n\n" +
			"Use it to:\n" +
			"  • See what others have done without affecting your work\n" +
			"  • Update remote-tracking branches (origin/main, etc.)\n" +
			"  • Check ahead/behind status before deciding to pull\n\n" +
			"After fetching, the Dashboard will show updated ahead/behind counts.");

		yield return Guide("Operation", "git branch",
			"List, create, or delete branches",
			"git branch                 — list local branches\n" +
			"git branch -a              — list all (including remote)\n" +
			"git branch feature-login   — create a new branch\n" +
			"git branch -d old-branch   — delete a branch\n\n" +
			"In FarGit: Branches panel (Dashboard → Enter on Branches row):\n" +
			"  Enter / F3  — switch to branch\n" +
			"  F5          — merge branch into current\n" +
			"  F7          — create new branch\n" +
			"  F8          — delete branch");

		yield return Guide("Operation", "git merge",
			"Combine another branch into the current one",
			"git merge branch-name\n\n" +
			"Integrates the history of 'branch-name' into your current branch.\n\n" +
			"If git can do it cleanly:\n" +
			"  Fast-forward — pointer just moves forward (clean)\n" +
			"  Merge commit — new commit with two parents (when histories diverged)\n\n" +
			"If there are conflicts:\n" +
			"  1. Open conflicted files and look for <<<<<<< markers\n" +
			"  2. Edit to keep the correct code\n" +
			"  3. Remove all conflict markers\n" +
			"  4. Stage the file and commit\n\n" +
			"In FarGit: Branches panel → F5 on the branch you want to merge in.");

		yield return Guide("Operation", "git revert",
			"Safely undo a commit by creating a new one",
			"git revert <commit-sha>\n\n" +
			"Creates a NEW commit that undoes the changes from a specific\n" +
			"previous commit. The original commit stays in history.\n\n" +
			"This is the SAFE way to undo because it:\n" +
			"  • Doesn't rewrite history\n" +
			"  • Works safely on shared/public branches\n" +
			"  • Can be undone itself if needed\n\n" +
			"Compare with 'git reset' which DOES rewrite history and\n" +
			"should only be used on commits you haven't shared yet.");

		yield return Guide("Operation", "git cherry-pick",
			"Apply a single commit from another branch",
			"git cherry-pick <commit-sha>\n\n" +
			"Takes one specific commit from anywhere in history and\n" +
			"applies its changes to your current branch.\n\n" +
			"Use cases:\n" +
			"  • A bug fix was committed to 'dev', you need it in 'main'\n" +
			"  • You want just one commit from a branch, not the whole thing\n\n" +
			"Find the commit SHA from the log (7 characters is enough):\n" +
			"  git log --oneline");

		// ── Common Workflows ───────────────────────────────────────────────
		yield return Header("═══  COMMON WORKFLOWS  ═══");

		yield return Guide("Workflow", "Daily work",
			"The everyday cycle: pull → edit → commit → push",
			"DAILY WORK CYCLE\n\n" +
			"1. PULL latest changes from your team\n" +
			"   Remote panel → F6 (Pull)\n" +
			"   This ensures you're working on the latest version.\n\n" +
			"2. EDIT your files\n" +
			"   Make your changes in the editor.\n\n" +
			"3. STAGE the changes you want to save\n" +
			"   Status panel → Space to mark files → F5 to stage\n" +
			"   Or Dashboard → F5 to stage everything.\n\n" +
			"4. COMMIT with a meaningful message\n" +
			"   Any panel → F7 to open the commit editor.\n\n" +
			"5. PUSH to share your work\n" +
			"   Remote panel → F7 (Push)\n\n" +
			"Repeat steps 2-4 as often as you like before pushing.");

		yield return Guide("Workflow", "Feature branch",
			"Isolated development: create → work → merge → push",
			"FEATURE BRANCH WORKFLOW\n\n" +
			"Use a separate branch for each feature or task:\n\n" +
			"1. CREATE a branch\n" +
			"   Branches panel → F7, name it 'feature/my-thing'\n\n" +
			"2. WORK on your feature\n" +
			"   Make commits on this branch. The 'main' branch is unaffected.\n\n" +
			"3. MERGE back when done\n" +
			"   Switch back to 'main' (Branches → Enter on main)\n" +
			"   Then merge your feature branch (F5 on it)\n\n" +
			"4. PUSH the merged main\n" +
			"   Remote panel → F7 (Push)\n\n" +
			"5. DELETE the feature branch\n" +
			"   Branches panel → F8 on the old branch\n\n" +
			"This keeps 'main' always in a working state.");

		yield return Guide("Workflow", "Hotfix",
			"Emergency fix: branch from main → fix → merge → push",
			"HOTFIX WORKFLOW\n\n" +
			"When production has a bug that needs immediate fixing:\n\n" +
			"1. Switch to 'main' (or whatever your stable branch is)\n" +
			"   Branches panel → Enter on 'main'\n\n" +
			"2. Create a hotfix branch\n" +
			"   F7 → name it 'hotfix/description'\n\n" +
			"3. Fix the bug and commit\n" +
			"   Make the minimal change needed. Commit with a clear message.\n\n" +
			"4. Merge hotfix into main\n" +
			"   Switch to 'main', then F5 to merge 'hotfix/description'\n\n" +
			"5. Tag the fix (optional but recommended)\n" +
			"   Tags panel → F7 → name it 'v1.0.1'\n\n" +
			"6. Push\n" +
			"   Remote panel → F7");

		yield return Guide("Workflow", "Collaboration",
			"Working with others: fork → branch → PR → merge",
			"COLLABORATION WORKFLOW  (GitHub / GitLab style)\n\n" +
			"When contributing to a project you don't own:\n\n" +
			"1. FORK the repository on GitHub\n" +
			"   (This creates your own copy on the server)\n\n" +
			"2. CLONE your fork\n" +
			"   Guide Me → Clone a repository\n" +
			"   Use your fork's URL, not the original.\n\n" +
			"3. Add the ORIGINAL as 'upstream' remote\n" +
			"   Remote panel → Ins → name: 'upstream', URL: original repo URL\n\n" +
			"4. CREATE a feature branch and do your work\n\n" +
			"5. PUSH your branch to your fork\n" +
			"   Remote panel → F7\n\n" +
			"6. Open a PULL REQUEST on GitHub\n" +
			"   GitHub will show a button when you push a new branch.\n\n" +
			"7. KEEP IN SYNC with upstream\n" +
			"   Fetch from 'upstream', merge into your main, push to origin.");

		if (args.Panel is GuidePanel panel && panel.Title is null)
		{
			panel.Title = "Git Reference Guide";
		}
	}

	static SetFile Header(string text) => new()
	{
		Name = text,
		Attributes = FileAttributes.ReadOnly,
	};

	static GuideFile Guide(string category, string name, string summary, string detail)
		=> new(category, name, summary, detail);
}
