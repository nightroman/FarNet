using FarNet;
using FarNet.Works;
using System;
using System.IO;
using System.Text;

namespace IronPythonFar;

[System.Runtime.InteropServices.Guid("7221f8a0-6917-4264-a2fc-ea3eb7545e3c")]
[ModuleEditor(Name = "Editor", Mask = "*.py")]
public class Editor : ModuleEditor
{
    IEditor _editor;

    public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
    {
        _editor = editor;
        editor.KeyDown += Editor_KeyDown;
    }

    private void Editor_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key.VirtualKeyCode)
        {
            case KeyCode.F5 when e.Key.Is():
                ExecuteFile();
                break;
        }
    }

    private void ExecuteFile()
    {
        _editor.Save();

        string print = string.Empty;
        try
        {
            Actor.ExecuteFile(_editor.FileName, s => print = s);
        }
        finally
        {
            if (print.Length > 0)
            {
                var temp = Kit.TempFileName("txt");
                File.WriteAllText(temp, print);
                ShowTempFile(temp, "Python output");
            }
        }
    }

    private static void ShowTempFile(string fileName, string title)
    {
        var editor = Far.Api.CreateEditor();
        editor.Title = title;
        editor.FileName = fileName;
        editor.CodePage = 65001;
        editor.IsLocked = true;
        editor.DisableHistory = true;
        editor.DeleteSource = DeleteSource.UnusedFile;
        editor.Open();
    }
}
