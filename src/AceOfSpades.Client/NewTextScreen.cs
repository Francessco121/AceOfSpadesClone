using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Client.NewText
{
    class NewTextScreen : GameScreen
    {
        BMPFont font, font2;
        TextAlign align;

        public NewTextScreen(MainWindow window) 
            : base(window, "NewText")
        {
            font = new BMPFont("Content/Fonts/arial-20.fnt");
            font2 = new BMPFont("Content/Fonts/karmasuture-21.fnt");
        }

        public override void Update(float deltaTime)
        {
            if (Input.GetKeyDown(Key.Keypad7)) align = TextAlign.TopLeft;
            if (Input.GetKeyDown(Key.Keypad8)) align = TextAlign.TopCenter;
            if (Input.GetKeyDown(Key.Keypad9)) align = TextAlign.TopRight;
            if (Input.GetKeyDown(Key.Keypad4)) align = TextAlign.Left;
            if (Input.GetKeyDown(Key.Keypad5)) align = TextAlign.Center;
            if (Input.GetKeyDown(Key.Keypad6)) align = TextAlign.Right;
            if (Input.GetKeyDown(Key.Keypad1)) align = TextAlign.BottomLeft;
            if (Input.GetKeyDown(Key.Keypad2)) align = TextAlign.BottomCenter;
            if (Input.GetKeyDown(Key.Keypad3)) align = TextAlign.BottomRight;
        }

        public override void Draw()
        {
            SpriteBatch sb = Renderer.Sprites.SpriteBatch;

            int x = 0, y = 0;
            if (align == TextAlign.TopCenter || align == TextAlign.Center || align == TextAlign.BottomCenter) x = Window.Width / 2;
            if (align == TextAlign.TopRight || align == TextAlign.Right || align == TextAlign.BottomRight) x = Window.Width;
            if (align == TextAlign.Left || align == TextAlign.Center || align == TextAlign.Right) y = Window.Height / 2;
            if (align == TextAlign.BottomLeft || align == TextAlign.BottomCenter || align == TextAlign.BottomRight) y = Window.Height;

            string text = @"
Powder powder gummies jujubes bear claw cookie gingerbread. Ice cream pie candy canes chupa chups bear 
claw oat cake bear claw jujubes icing. Chocolate bar chocolate bar wafer dragée sweet icing sugar plum sugar plum 
dragée. Lollipop muffin carrot cake liquorice. Tart soufflé danish oat cake pastry sweet toffee dessert. Candy 
gummi bears pie chocolate cake icing caramels jelly beans. Cake brownie croissant tiramisu bear claw candy canes 
biscuit biscuit oat cake. Candy canes sesame snaps brownie chupa chups donut carrot cake chocolate bar macaroon lemon 
drops. Pudding lemon drops sugar plum halvah ice cream bonbon marshmallow chocolate bar dragée. Tart pudding lollipop 
pastry. Toffee sweet cake jujubes carrot cake. Tootsie roll macaroon sugar plum liquorice topping jelly icing lemon 
drops. Jelly-o tootsie roll pastry gummi bears liquorice sweet roll donut macaroon toffee. Cotton candy lemon drops 
sesame snaps lemon drops.";

            font.DrawString(text,
                x, y, sb, align, Color.White, Color.Black);
            font2.DrawString(text,
                Window.Width, Window.Height, sb, TextAlign.BottomRight, Color.White, Color.Black);

        }
    }
}
