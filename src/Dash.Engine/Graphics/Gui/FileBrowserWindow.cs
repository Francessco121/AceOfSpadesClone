using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dash.Engine.Graphics.Gui
{
    public enum FileBrowserMode
    {
        OpenFile, Save
    }

    public delegate void FileBrowserCompleted(FileBrowserWindow window);

    public class FileBrowserWindow : GUIWindow
    {
        class BrowserOptionButton : GUIButton
        {
            public bool ParentOption;
            public bool IsFolder;
            public bool IsNothing;

            public BrowserOptionButton(UDim2 position, UDim2 size, GUITheme theme, string text, TextAlign textAlign, 
                bool gotoParent, bool isFolder) 
                : base(position, size, text, textAlign, theme)
            {
                ParentOption = gotoParent;
                IsFolder = isFolder;
            }
        }

        public string CurrentDirectory
        {
            get { return dirField.Text; }
            set { dirField.Text = value; }
        }

        public string FileName
        {
            get { return selectedFileLabel.Text; }
            set { selectedFileLabel.Text = value; }
        }

        GUITextField dirField;
        GUITextField selectedFileLabel;

        BrowserOptionButton[] options;
        string[] exts;
        FileBrowserCompleted callback;
        FileBrowserMode mode;

        public FileBrowserWindow(GUISystem system, GUITheme theme, UDim2 size, string title, 
            FileBrowserMode mode, string[] exts, FileBrowserCompleted callback) 
            : base(system, size, title, theme)
        {
            this.mode = mode;
            this.callback = callback;
            this.exts = exts;

            BMPFont smallFont = theme.GetField<BMPFont>(null, "SmallFont", "Font");

            dirField = new GUITextField(new UDim2(0, 0, 0, 20), new UDim2(1, -40, 0, 20), Environment.CurrentDirectory, TextAlign.Left, theme);
            dirField.Label.Font = smallFont;

            GUIButton goToDirBtn = new GUIButton(new UDim2(1, -40, 0, 20), new UDim2(0, 40, 0, 20), "Go", theme);
            goToDirBtn.OnMouseClick += (btn, mbtn) =>
            {
                ScanDir();
            };

            GUILabel fileNameLabel = new GUILabel(new UDim2(0, 0, 1, -40), new UDim2(1, -200, 0, 20), "File Name:", TextAlign.Left, theme);
            selectedFileLabel = new GUITextField(new UDim2(0, 0, 1, -20), new UDim2(1, -200, 0, 20), "", TextAlign.Left, theme);
            selectedFileLabel.Label.Font = smallFont;
            GUIButton doneBtn = new GUIButton(new UDim2(1, -100, 1, -40), new UDim2(0, 100, 0, 40), 
                mode == FileBrowserMode.OpenFile ? "Open" : "Save", TextAlign.Center, theme);
            doneBtn.OnMouseClick += (btn, mbtn) => { callback(this); Visible = false; };
            GUIButton cancelBtn = new GUIButton(new UDim2(1, -200, 1, -40), new UDim2(0, 100, 0, 40), "Cancel", TextAlign.Center, theme);
            cancelBtn.OnMouseClick += (btn, mbtn) => { Visible = false; };

            AddTopLevel(dirField, goToDirBtn, fileNameLabel, selectedFileLabel, doneBtn, cancelBtn);
            ScanDir();
        }

        void FileSelected(GUIButton _btn, MouseButton mbtn)
        {
            if (mbtn != MouseButton.Left)
                return;

            BrowserOptionButton btn = (BrowserOptionButton)_btn;

            if (btn.IsNothing)
                return;

            string fileName = btn.Label.Text;

            if (btn.ParentOption)
            {
                DirectoryInfo parent = Directory.GetParent(CurrentDirectory);
                if (parent != null)
                {
                    CurrentDirectory = parent.FullName;
                    ScanDir();
                }
            }
            else
            {
                string path = Path.Combine(CurrentDirectory, fileName);

                if (btn.IsFolder)
                {
                    CurrentDirectory = path;
                    ScanDir();
                }
                else
                {
                    if (mode == FileBrowserMode.OpenFile)
                    {
                        for (int i = 0; i < options.Length; i++)
                            options[i].Toggled = false;
                        btn.Toggled = true;
                    }

                    FileName = mode == FileBrowserMode.OpenFile ? path : Path.GetFileName(path);
                }
            }
        }

        void ScanDir()
        {
            if (options != null)
                RemoveTopLevel(options);

            if (Directory.Exists(CurrentDirectory))
            {
                string[] files, folders;

                try
                {
                    files = Directory.GetFiles(CurrentDirectory);
                    folders = Directory.GetDirectories(CurrentDirectory);
                }
                catch (Exception e)
                {
                    options = new BrowserOptionButton[1];

                    BrowserOptionButton btn = new BrowserOptionButton(new UDim2(0, 0, 0, 40), new UDim2(1, 0, 0, 20), 
                        Theme, "An error occured! " + e.GetType().FullName,
                        TextAlign.Left, false, false)
                    { IsNothing = true };
                    btn.OnMouseClick += (b, mbtn) => { FileSelected(b, mbtn); };
                    AddTopLevel(btn);

                    btn.NormalImage = Image.CreateBlank(new Color(194, 33, 33));
                    btn.HoverImage = Image.CreateBlank(new Color(194, 33, 33));
                    btn.ActiveImage = Image.CreateBlank(new Color(194, 33, 33));

                    options[0] = btn;
                    return;
                }

                List<string> specificFiles = new List<string>();

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file);
                    if (exts.Length == 0 || exts.Contains(ext))
                        specificFiles.Add(Path.GetFileName(file));
                }

                options = new BrowserOptionButton[specificFiles.Count + folders.Length + 1];
                for (int i = 0; i < folders.Length + 1; i++)
                {
                    string file = i == 0 ? ".." : new DirectoryInfo((folders[i - 1])).Name;

                    BrowserOptionButton btn = new BrowserOptionButton(new UDim2(0, 0, 0, 40 + i * 20), new UDim2(1, 0, 0, 20), 
                        Theme, file, TextAlign.Left, i == 0, true);
                    btn.OnMouseClick += (b, mbtn) => { FileSelected(b, mbtn); };
                    AddTopLevel(btn);

                    btn.NormalImage = Image.CreateBlank(new Color(30, 30, 30));
                    btn.HoverImage = Image.CreateBlank(new Color(50, 50, 50));
                    btn.ActiveImage = Image.CreateBlank(new Color(20, 20, 20));

                    options[i] = btn;
                }

                for (int i = folders.Length + 1; i < options.Length; i++)
                {
                    string file = specificFiles[i - (folders.Length + 1)];

                    BrowserOptionButton btn = new BrowserOptionButton(new UDim2(0, 0, 0, 40 + i * 20), new UDim2(1, 0, 0, 20), 
                        Theme, file, TextAlign.Left, false, false);
                    btn.OnMouseClick += (b, mbtn) => { FileSelected(b, mbtn); };
                    AddTopLevel(btn);

                    btn.NormalImage = Image.CreateBlank(new Color(30, 30, 30));
                    btn.HoverImage = Image.CreateBlank(new Color(50, 50, 50));
                    btn.ActiveImage = Image.CreateBlank(new Color(20, 20, 20));

                    options[i] = btn;
                }
            }
            else
            {
                options = new BrowserOptionButton[1];

                BrowserOptionButton btn = new BrowserOptionButton(new UDim2(0, 0, 0, 40), new UDim2(1, 0, 0, 20), 
                    Theme, "Directory does not exist!", TextAlign.Left, false, false)
                { IsNothing = true };
                btn.OnMouseClick += (b, mbtn) => { FileSelected(b, mbtn); };
                AddTopLevel(btn);

                btn.NormalImage = Image.CreateBlank(new Color(194, 33, 33));
                btn.HoverImage = Image.CreateBlank(new Color(194, 33, 33));
                btn.ActiveImage = Image.CreateBlank(new Color(194, 33, 33));

                options[0] = btn;
            }
        }

        public override void Update(float deltaTime)
        {
            if (Input.GetKeyDown(Key.Enter) && dirField.HasFocus)
            {
                ScanDir();
                dirField.HasFocus = false;
            }

            base.Update(deltaTime);
        }
    }
}
