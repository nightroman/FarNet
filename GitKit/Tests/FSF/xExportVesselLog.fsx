// export file revisions to ./z

open System
open System.IO
open LibGit2Sharp

/// Repository path.
let inRoot = @"C:\Bin\Far\x64"

/// Starting from date.
let inTime = DateTime.UtcNow - TimeSpan.FromDays(10)

/// Files to export.
let inFiles = [
    "1far/Local/FarNet/Vessel/VesselHistory.txt"
    "1far/Local/FarNet/Vessel/VesselFolders.txt"
]

/// Temp output directory.
let dir = $"{__SOURCE_DIRECTORY__}/z"

do
    if Directory.Exists dir then
        Directory.Delete(dir, true)

    Directory.CreateDirectory(dir) |> ignore

    use repo = new Repository(inRoot)

    repo.Commits
    |> Seq.windowed 2
    |> Seq.takeWhile (fun a -> a.[0].Author.When.DateTime >= inTime)
    |> Seq.iter (fun a ->
        for change in repo.Diff.Compare<TreeChanges>(a.[1].Tree, a.[0].Tree) do
            for file in inFiles do
                if change.Path = file then
                    let blob = repo.Lookup change.Oid :?> Blob
                    let text = blob.GetContentText()
                    let path = $"{dir}/{Path.GetFileNameWithoutExtension(file)}.{change.Oid}.txt"
                    File.WriteAllText(path, text)
    )
