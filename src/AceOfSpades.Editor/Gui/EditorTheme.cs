using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

/* EditorTheme.cs
 * Tristan Smith
*/

namespace AceOfSpades.Editor.Gui
{
    public class EditorTheme : GUITheme
    {
        public static GUITheme Glass
        {
            get
            {
                GUITheme t = new GUITheme();

                t.SetField("Button.NormalImage", Image.CreateBlank(Color.fromHtml("#1E1E1E", .2f)));
                t.SetField("Button.HoverImage", Image.CreateBlank(Color.fromHtml("#848484", .8f)));
                t.SetField("Button.ActiveImage", Image.CreateBlank(Color.fromHtml("#8E8E8E", .5f)));
                t.SetField("Button.ToggledImage", Image.CreateBlank(Color.fromHtml("#595959", .7f)));
                t.SetField("Button.TextColor", Color.fromHtml("#ddd"));

                t.SetField("TextField.NormalImage", Image.CreateBlank(Color.fromHtml("#515151")));
                t.SetField("TextField.HoverImage", Image.CreateBlank(Color.fromHtml("#6B6B6B")));
                t.SetField("TextField.ActiveImage", Image.CreateBlank(Color.fromHtml("#777777", .5f)));

                t.SetField("Label.TextColor", Color.fromHtml("#ddd"));
                t.SetField("Label.TextShadowColor", Color.fromHtml("#000", 0.6f));

                t.SetField("Frame.Image", Image.CreateBlank(Color.fromHtml("#1E1E1E", .6f)));

                t.SetField("Window.BackgroundImage", Image.CreateBlank(Color.fromHtml("#1E1E1E", .6f)));

                t.SetField("SmallFont", AssetManager.LoadFont("karmasuture-12"));
                t.SetField("Font", AssetManager.LoadFont("arial-14"));
                t.SetField("BigFont", AssetManager.LoadFont("arial-18"));

                return t;
            }
        }
        public static GUITheme BasicEdtior
        {
            get
            {
                GUITheme t = new GUITheme();

                BMPFont font12 = AssetManager.LoadFont("karmasuture-12");
                BMPFont font14 = AssetManager.LoadFont("karmasuture-14");
                BMPFont font18 = AssetManager.LoadFont("karmasuture-18");

                t.SetField("Button.NormalImage", Image.CreateBlank(Color.fromHtml("#07B2EB")));
                t.SetField("Button.HoverImage", Image.CreateBlank(new Color(7, 189, 255)));
                t.SetField("Button.ActiveImage", Image.CreateBlank(new Color(13, 160, 209)));
                t.SetField("Button.ToggledImage", Image.CreateBlank(new Color(194, 37, 37)));
                t.SetField("Button.TextColor", Color.fromHtml("#ddd"));
                t.SetField("TextField.NormalImage", Image.CreateBlank(new Color(7, 178, 235)));
                t.SetField("TextField.HoverImage", Image.CreateBlank(new Color(7, 189, 255)));
                t.SetField("TextField.ActiveImage", Image.CreateBlank(new Color(13, 160, 209)));
                t.SetField("Checkbox.NormalImage", Image.CreateBlank(new Color(7, 178, 235)));
                t.SetField("Checkbox.HoverImage", Image.CreateBlank(new Color(7, 189, 255)));
                t.SetField("Checkbox.ActiveImage", Image.CreateBlank(new Color(13, 160, 209)));
                t.SetField("Label.TextColor", Color.fromHtml("#ddd"));
                t.SetField("Frame.Image", Image.CreateBlank(new Color(30, 30, 30, 200)));
                t.SetField("Window.BackgroundImage", Image.CreateBlank(new Color(30, 30, 30, 200)));
                t.SetField("SmallFont", font12);
                t.SetField("Font", font14);
                t.SetField("BigFont", font18);

                return t;
            }
        }
    }
}
