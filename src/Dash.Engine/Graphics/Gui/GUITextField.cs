using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dash.Engine.Graphics.Gui
{
    public delegate void GUITextFieldTextChanged(GUITextField field, string text);

    public class GUITextField : GUIButton
    {
        public event GUITextFieldTextChanged OnTextChanged;
        public event GUITextFieldTextChanged OnEnterPressed;

        public string Text
        {
            get { return Label.Text; }
            set { Label.Text = value; }
        }
        public bool HasFocus { get; set; }
        public int MaxLength = int.MaxValue;

        int cursorPos;

        const float blinkDelay = 0.5f;
        float blinkTime;
        bool showPipe;
        bool hadFocus;
        bool enterHeld;

        public GUITextField(UDim2 position, UDim2 size, GUITheme theme)
            : this(position, size, "", theme)
        { }

        public GUITextField(UDim2 position, UDim2 size, string text, GUITheme theme)
            : this(position, size, text, theme,
                  theme.GetField<Image>(Image.Blank, "TextField.NormalImage"),
                  theme.GetField<Image>(Image.Blank, "TextField.HoverImage"),
                  theme.GetField<Image>(Image.Blank, "TextField.ActiveImage"),
                  theme.GetField<Image>(Image.Blank, "TextField.ToggledImage"))
        { }

        public GUITextField(UDim2 position, UDim2 size, string text, TextAlign textAlign, GUITheme theme)
            : this(position, size, text, textAlign, theme,
                  theme.GetField<Image>(Image.Blank, "TextField.NormalImage"),
                  theme.GetField<Image>(Image.Blank, "TextField.HoverImage"),
                  theme.GetField<Image>(Image.Blank, "TextField.ActiveImage"),
                  theme.GetField<Image>(Image.Blank, "TextField.ToggledImage") )
        { }

        public GUITextField(UDim2 position, UDim2 size, string text, GUITheme theme,
            Image normalImg, Image hoverImg, Image activeImg, Image toggledImg)
            : this(position, size, text, TextAlign.Left, theme, normalImg, hoverImg, activeImg, toggledImg)
        { }

        public GUITextField(UDim2 position, UDim2 size, string text, TextAlign textAlign, GUITheme theme,
            Image normalImg, Image hoverImg, Image activeImg, Image toggledImg)
            : base(position, size, text, textAlign, theme, normalImg, hoverImg, activeImg, toggledImg)
        { }

        public GUITextField(UDim2 position, UDim2 size, string text, TextAlign textAlign,
            BMPFont font, Color textColor,
            Image normalImg, Image hoverImg, Image activeImg, Image toggledImg)
            : base(position, size, text, textAlign, font, textColor, normalImg, hoverImg, activeImg, toggledImg)
        { }

        protected override void MouseClick(MouseButton mbtn)
        {
            HasFocus = true;
            cursorPos = Label.Text.Length;

            showPipe = true;
            blinkTime = blinkDelay;
            base.MouseClick(mbtn);
        }

        public override void Update(float deltaTime)
        {
            if (!CanDraw() || Input.GetMouseButtonUp(MouseButton.Left))
                HasFocus = false;

            if (HasFocus)
            {
                int spacesMoved;
                Key[] controlKeys;

                string text = Text;

                cursorPos = MathHelper.Clamp(cursorPos, 0, text.Length);

                DashKeyboard.ProcessTextKeyInput(Input.CurrentKeyboardState, deltaTime, cursorPos,
                    ref text, out spacesMoved, out controlKeys, new Key[0], MaxLength);

                Label.Text = text;
                cursorPos += spacesMoved;

                if (spacesMoved > 0)
                {
                    blinkTime = blinkDelay;
                    showPipe = true;
                    GUIArea.ElementUsedKeyboard();
                }

                for (int i = 0; i < controlKeys.Length; i++)
                {
                    Key key = controlKeys[i];

                    if (key == Key.Backspace && Text.Length > 0)
                    {
                        Text = Text.Remove(Text.Length - 1);
                        cursorPos--;
                    }
                }

                if (Input.GetKeyDown(Key.Enter) || Input.GetKeyDown(Key.KeypadEnter))
                    enterHeld = true;

                if (enterHeld && (Input.GetKeyUp(Key.Enter) || Input.GetKeyUp(Key.KeypadEnter)))
                {
                    HasFocus = false;
                    enterHeld = false;

                    if (OnEnterPressed != null)
                        OnEnterPressed(this, Text);
                }

                if (blinkTime > 0)
                    blinkTime -= deltaTime;
                else
                {
                    blinkTime = blinkDelay;
                    showPipe = !showPipe;
                }
            }

            Label.TextExtension = showPipe && HasFocus ? "|" : " ";

            if (!HasFocus && hadFocus && OnTextChanged != null)
                OnTextChanged(this, Text);


            hadFocus = HasFocus;
            base.Update(deltaTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            Image image;
            if (IsMouseOver && !MousePressedDown)
                image = HoverImage;
            else if (MousePressedDown)
                image = ActiveImage;
            else
                image = NormalImage;

            image.Draw(sb, CalculatedRectangle);
        }
    }
}
