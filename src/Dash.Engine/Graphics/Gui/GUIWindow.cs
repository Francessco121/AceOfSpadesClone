/* GUIWindow.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public class GUIWindow : GUIWindowBase
    {
        public string Title
        {
            get { return TitleBar.Text; }
            set { TitleBar.Text = value; }
        }

        public GUILabel TitleBar { get; }
        public GUIButton ExitButton { get; }
        public GUIFrame BackgroundFrame { get; }

        protected GUITheme Theme { get; }

        public GUIWindow(GUISystem system, UDim2 size, string title, GUITheme theme,
            bool closable = true, bool createTitleBar = true)
            : this(system, UDim2.Zero, size, title, theme, closable, createTitleBar)
        {
            Center();
        }

        public GUIWindow(GUISystem system, UDim2 position, UDim2 size, string title, GUITheme theme, 
            bool closable = true, bool createTitleBar = true) 
            : base(system, position, size)
        {
            Theme = theme;
            if (createTitleBar)
            {
                TitleBar = new GUILabel(UDim2.Zero, new UDim2(1f, 0, 0, 20), title,
                    theme.GetField<Color>(Color.White, "Window.TitleBar.TextColor"), theme);
                TitleBar.CapturesMouseClicks = true;
                TitleBar.ZIndex = 100;
                TitleBar.BackgroundImage = theme.GetField<Image>(Image.CreateBlank(new Color(40, 40, 40)), "Window.TitleBar.BackgroundImage");

                if (closable)
                {
                    ExitButton = new GUIButton(new UDim2(1f, -20, 0, 0), new UDim2(0, 20, 0, 20),
                        theme.GetField<string>("X", "Window.TitleBar.CloseButton.Text"),
                        TextAlign.Center,
                        theme,
                        theme.GetField<Image>(Image.CreateBlank(new Color(230, 0, 0)), "Window.TitleBar.CloseButton.NormalImage"),
                        theme.GetField<Image>(Image.CreateBlank(new Color(255, 0, 0)), "Window.TitleBar.CloseButton.HoverImage"),
                        theme.GetField<Image>(Image.CreateBlank(new Color(200, 0, 0)), "Window.TitleBar.CloseButton.ActiveImage"),
                        null);
                    ExitButton.Parent = TitleBar;
                    ExitButton.OnMouseClick += (btn, mbtn) =>
                    {
                        Visible = false;
                    };
                }
            }

            BackgroundFrame = new GUIFrame(new UDim2(0, 0, 0, createTitleBar ? 20 : 0), new UDim2(1f, 0, 1f, createTitleBar ? -20 : 0),
                theme.GetField<Image>(Image.CreateBlank(new Color(70, 70, 70, 200)), "Window.BackgroundImage"));
            BackgroundFrame.ZIndex = -100;

            if (createTitleBar)
            {
                SetDragHandle(TitleBar);
                AddTopLevel(TitleBar);
            }
            AddTopLevel(BackgroundFrame);
        }
    }
}
