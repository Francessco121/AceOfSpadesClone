using Dash.Engine.Graphics.OpenGL;
using System;

/* GUIColorPicker.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public class GUIColorPicker : GUIElement
    {
        class GUIHueSlider : GUIFrame
        {
            public float Hue { get; private set; }

            GUIFrame tracker;

            GUIColorPicker picker;
            bool mousePressed;

            float yHue;

            public GUIHueSlider(GUIColorPicker picker, UDim2 position, UDim2 size, Image image)
                : base(position, size, image)
            {
                this.picker = picker;
                CapturesMouseClicks = true;

                tracker = new GUIFrame(new UDim2(0, 0, 0, -1), new UDim2(1f, 0, 0, 3), Image.Blank)
                { Parent = this, NeverCaptureMouse = true };
            }

            public void SetHue(float hue)
            {
                Hue = MathHelper.Clamp(hue, 0, 359.9f);
                UpdateTracker((359.9f - Hue) / 359.9f);
            }

            public override void MouseButtonDown(MouseButton mbtn)
            {
                if (mbtn == MouseButton.Left)
                    mousePressed = true;

                base.MouseButtonDown(mbtn);
            }

            public override void MouseButtonUp(MouseButton mbtn)
            {
                if (mbtn == MouseButton.Left)
                    mousePressed = false;

                base.MouseButtonUp(mbtn);
            }

            public override void Update(float deltaTime)
            {
                if (mousePressed)
                {
                    float yh = (Input.CursorY - CalculatedRectangle.Y) / CalculatedRectangle.Height;
                    UpdateTracker(yh);

                    if (yh != yHue)
                    {
                        yHue = yh;
                        picker.SetHue(Hue = MathHelper.Clamp(360 - (yh * 360), 0, 359.9f));
                        picker.UpdateColor();
                    }
                }

                base.Update(deltaTime);
            }

            void UpdateTracker(float yh)
            {
                tracker.Position.Y.Scale = MathHelper.Clamp(yh, 0, 1);
            }
        }

        class GUIAlphaSlider : GUIFrame
        {
            public byte Alpha { get; private set; } = 255;

            GUIFrame tracker;
            GUIFrame overlay;

            GUIColorPicker picker;
            bool mousePressed;

            float yAlpha;

            public GUIAlphaSlider(GUIColorPicker picker, UDim2 position, UDim2 size, 
                Image checkerImage, Image alphaImage)
                : base(position, size, checkerImage)
            {
                this.picker = picker;
                CapturesMouseClicks = true;

                overlay = new GUIFrame(UDim2.Zero, new UDim2(1f, 0, 1f, 0), alphaImage)
                { Parent = this, NeverCaptureMouse = true };

                tracker = new GUIFrame(new UDim2(0, 0, 0, -1), new UDim2(1f, 0, 0, 3), Image.Blank)
                { Parent = this, NeverCaptureMouse = true, ZIndex = 1 };
            }

            public void SetAlpha(int alpha)
            {
                Alpha = (byte)MathHelper.Clamp(alpha, 0, 255);
                UpdateTracker(1f - (Alpha / 255f));
            }

            public override void MouseButtonDown(MouseButton mbtn)
            {
                if (mbtn == MouseButton.Left)
                    mousePressed = true;

                base.MouseButtonDown(mbtn);
            }

            public override void MouseButtonUp(MouseButton mbtn)
            {
                if (mbtn == MouseButton.Left)
                    mousePressed = false;

                base.MouseButtonUp(mbtn);
            }

            public override Rectangle CalculateDimensions(Rectangle parentDim)
            {
                Rectangle dim = base.CalculateDimensions(parentDim);
                float square = Image.Texture.Width * 1.5f;
                Image.ClippingRectangle = new Rectangle(0, 0,
                    Image.Texture.Width * (float)Math.Ceiling(dim.Width / square),
                    Image.Texture.Height * (float)Math.Ceiling(dim.Height / square));
                return dim;
            }

            public override void Update(float deltaTime)
            {
                if (mousePressed)
                {
                    float ya = (Input.CursorY - CalculatedRectangle.Y) / CalculatedRectangle.Height;
                    ya = MathHelper.Clamp(ya, 0, 1);
                    UpdateTracker(ya);

                    if (ya != yAlpha)
                    {
                        yAlpha = ya;
                        Alpha = (byte)((1f - ya) * 255f);
                        picker.UpdateColor();
                    }
                }

                base.Update(deltaTime);
            }

            void UpdateTracker(float ya)
            {
                tracker.Position.Y.Scale = ya;
            }
        }

        class GUIHSVPalette : GUIFrame
        {
            public float Saturation { get; private set; } = 1f;
            public float Value { get; private set; } = 1f;

            GUIFrame tracker;

            GUIColorPicker picker;
            bool mousePressed;

            float _xp = 1f, _yp;

            public GUIHSVPalette(GUIColorPicker picker, UDim2 position, UDim2 size, Image image)
                : base(position, size, image)
            {
                this.picker = picker;
                CapturesMouseClicks = true;

                tracker = new GUIFrame(new UDim2(1f, -1, 0, -1), new UDim2(0, 3, 0, 3), Image.Blank)
                { Parent = this, NeverCaptureMouse = true };
            }

            public void SetSV(float saturation, float value)
            {
                Saturation = MathHelper.Clamp(saturation / 100f, 0, 1);
                Value = MathHelper.Clamp(value / 100f, 0, 1);

                UpdateSlider(Saturation, 1f - Value);
            }

            public override void MouseButtonDown(MouseButton mbtn)
            {
                if (mbtn == MouseButton.Left)
                    mousePressed = true;

                base.MouseButtonDown(mbtn);
            }

            public override void MouseButtonUp(MouseButton mbtn)
            {
                if (mbtn == MouseButton.Left)
                    mousePressed = false;

                base.MouseButtonUp(mbtn);
            }

            public override void Update(float deltaTime)
            {
                if (mousePressed)
                {
                    float xp = (Input.CursorX - CalculatedRectangle.X) / CalculatedRectangle.Width;
                    float yp = (Input.CursorY - CalculatedRectangle.Y) / CalculatedRectangle.Height;

                    xp = MathHelper.Clamp(xp, 0, 1);
                    yp = MathHelper.Clamp(yp, 0, 1);

                    if (xp != _xp || yp != _yp)
                    {
                        _xp = xp;
                        _yp = yp;

                        UpdateSlider(xp, yp);

                        Saturation = xp;
                        Value = 1f - yp;

                        picker.UpdateColor();
                    }
                }

                base.Update(deltaTime);
            }

            void UpdateSlider(float xp, float yp)
            {
                tracker.Position.X.Scale = xp;
                tracker.Position.Y.Scale = yp;
            }
        }

        static Texture huePalette;
        static void GenerateHuePalette()
        {
            if (huePalette == null)
            {
                huePalette = new Texture();
                huePalette.Bind();

                huePalette.SetMinMag(TextureMinFilter.Linear, TextureMagFilter.Linear);
                huePalette.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

                Color[] data = new Color[360];
                for (int y = 359; y >= 0; y--)
                {
                    Color color = Maths.HSVToRGB(y, 1, 1);
                    data[359 - y] = color;
                }

                huePalette.SetData(1, 90, data);

                huePalette.Unbind();
            }
        }

        static Texture checkersTex;
        static Texture alphaSliderTex;
        static void GenerateAlphaTextures()
        {
            if (checkersTex == null)
            {
                checkersTex = new Texture();
                checkersTex.Bind();

                checkersTex.SetMinMag(TextureMinFilter.Linear, TextureMagFilter.Linear);
                checkersTex.SetWrapMode(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

                Color[] data = new Color[32 * 32];
                for (int x = 0; x < 32; x++)
                    for (int y = 0; y < 32; y++)
                    {
                        Color color = (((x + 3) / 16 + y / 16) % 2) == 0 ? Color.White : new Color(200, 200, 200);
                        data[x + ((31 - y) * 32)] = color;
                    }

                checkersTex.SetData(8, 32, data);

                checkersTex.Unbind();

                alphaSliderTex = new Texture();
                alphaSliderTex.Bind();

                alphaSliderTex.SetMinMag(TextureMinFilter.Linear, TextureMagFilter.Linear);
                alphaSliderTex.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

                data = new Color[100];
                for (int y = 0; y < 100; y++)
                {
                    Color color = new Color(0, 0, 0, (100 - y) / 100f);
                    data[y] = color;
                }

                alphaSliderTex.SetData(1, 25, data);

                alphaSliderTex.Unbind();
            }
        }

        public Color Color { get; private set; }
        public bool AllowAlpha { get; set; }

        GUIHueSlider hueSlider;
        GUIHSVPalette hsvPicker;
        GUIAlphaSlider alphaSlider;
        Texture hsvPalette;
        GUIFrame colorPreview;

        GUILabel alphaLabel;
        GUITextField hueTextField, saturationTextField, valueTextField,
            redTextField, greenTextField, blueTextField, alphaTextField;

        public GUIColorPicker(UDim2 position, UDim2 size, GUITheme theme)
            : base(theme)
        {
            // Generate static textures if necessary
            GenerateHuePalette();
            GenerateAlphaTextures();

            // Generate dynamic palette
            hsvPalette = new Texture();
            hsvPalette.Bind();

            hsvPalette.SetMinMag(TextureMinFilter.Linear, TextureMagFilter.Linear);
            hsvPalette.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            // Pre-allocate some room
            hsvPalette.SetData(25, 100, PixelType.UnsignedByte, PixelFormat.Rgba, IntPtr.Zero);

            hsvPalette.Unbind();

            // Set properties
            Position = position;
            Size = size;

            // Build layout
            // Left layout
            GUIFrame pickerFrame = new GUIFrame(UDim2.Zero, new UDim2(1f, -90, 1f, 0),
                Image.CreateBlank(Color.Transparent))
            { Parent = this };

            hsvPicker = new GUIHSVPalette(this,
                UDim2.Zero, new UDim2(1f, -70, 1f, 0),
                new Image(hsvPalette))
            { Parent = pickerFrame };

            alphaSlider = new GUIAlphaSlider(this,
                new UDim2(1f, -60, 0, 0), new UDim2(0, 25, 1f, 0),
                new Image(checkersTex), new Image(alphaSliderTex))
            { Parent = pickerFrame };

            hueSlider = new GUIHueSlider(this, 
                new UDim2(1f, -25, 0, 0), new UDim2(0, 25, 1f, 0), 
                new Image(huePalette))
            { Parent = pickerFrame };

            // Right layout
            GUIFrame editFrame = new GUIFrame(new UDim2(1f, -80, 0, 0), new UDim2(0, 80, 1f, 0),
                Image.CreateBlank(Color.Transparent))
            { Parent = this };

            colorPreview = new GUIFrame(new UDim2(1f, -60, 0, 0), new UDim2(0, 60, 0, 60), Image.Blank)
            { Parent = editFrame };

            GUIFrame textFieldFrame = new GUIFrame(new UDim2(0, 0, 0, 70), new UDim2(1f, 0, 1f, -70),
                Image.CreateBlank(Color.Transparent))
            { Parent = editFrame };

            BMPFont smallFont = theme.GetField<BMPFont>(null, "SmallFont");

            hueTextField = AddEditField(textFieldFrame, smallFont, "H", 0);
            saturationTextField = AddEditField(textFieldFrame, smallFont, "S", 1);
            valueTextField = AddEditField(textFieldFrame, smallFont, "V", 2);
            redTextField = AddEditField(textFieldFrame, smallFont, "R", 3);
            greenTextField = AddEditField(textFieldFrame, smallFont, "G", 4);
            blueTextField = AddEditField(textFieldFrame, smallFont, "B", 5);
            alphaTextField = AddEditField(textFieldFrame, smallFont, "A", 6);

            alphaLabel = (GUILabel)textFieldFrame.Children[textFieldFrame.Children.Count - 2];

            #region OnTextChanged Event Handlers
            hueTextField.OnTextChanged += (field, text) =>
            {
                float hue;
                if (float.TryParse(text, out hue))
                    hueSlider.SetHue(hue);

                UpdateColor();
            };

            saturationTextField.OnTextChanged += (field, text) =>
            {
                float sat;
                if (float.TryParse(text, out sat))
                    hsvPicker.SetSV(sat, hsvPicker.Value * 100);

                UpdateColor();
            };

            valueTextField.OnTextChanged += (field, text) =>
            {
                float val;
                if (float.TryParse(text, out val))
                    hsvPicker.SetSV(hsvPicker.Saturation * 100, val);

                UpdateColor();
            };

            redTextField.OnTextChanged += (field, text) =>
            {
                float r;
                if (float.TryParse(text, out r))
                {
                    float h, s, v;
                    Maths.RGBToHSV((byte)MathHelper.Clamp(r, 0, 255), Color.G, Color.B, out h, out s, out v);
                    hueSlider.SetHue(h);
                    hsvPicker.SetSV(s * 100, v * 100);
                }

                SetHue(hueSlider.Hue);
                UpdateColor();
            };

            greenTextField.OnTextChanged += (field, text) =>
            {
                float g;
                if (float.TryParse(text, out g))
                {
                    float h, s, v;
                    Maths.RGBToHSV(Color.R, (byte)MathHelper.Clamp(g, 0, 255), Color.B, out h, out s, out v);
                    hueSlider.SetHue(h);
                    hsvPicker.SetSV(s * 100, v * 100);
                }

                SetHue(hueSlider.Hue);
                UpdateColor();
            };

            blueTextField.OnTextChanged += (field, text) =>
            {
                float b;
                if (float.TryParse(text, out b))
                {
                    float h, s, v;
                    Maths.RGBToHSV(Color.R, Color.G, (byte)MathHelper.Clamp(b, 0, 255), out h, out s, out v);
                    hueSlider.SetHue(h);
                    hsvPicker.SetSV(s * 100, v * 100);
                }

                SetHue(hueSlider.Hue);
                UpdateColor();
            };

            alphaTextField.OnTextChanged += (field, text) =>
            {
                float alpha;
                if (float.TryParse(text, out alpha))
                    alphaSlider.SetAlpha((int)alpha);

                UpdateColor();
            };
            #endregion

            // Setup defaults
            SetHue(0);
            UpdateColor();
        }

        public void SetColor(Color color)
        {
            float h, s, v;
            Maths.RGBToHSV(color.R, color.G, color.B, out h, out s, out v);
            hueSlider.SetHue(h);
            hsvPicker.SetSV(s * 100, v * 100);
            alphaSlider.SetAlpha(color.A);

            SetHue(hueSlider.Hue);
            UpdateColor();
            Color = color;
        }

        GUITextField AddEditField(GUIFrame textFieldFrame, BMPFont font, string text, int y)
        {
            GUILabel lbl = new GUILabel(new UDim2(0, 0, 0.142f * y, 0), new UDim2(1f, 0, 0.142f, 0),
                text + ":", TextAlign.Left, Theme)
            { Parent = textFieldFrame, Font = font };
            GUITextField field = new GUITextField(new UDim2(0, 20, 0.142f * y, 4), new UDim2(1f, -20, 0.142f, -8),
                "0", TextAlign.Center, Theme)
            { Parent = textFieldFrame };
            field.Label.Font = font;

            return field;
        }

        void SetHue(float hue)
        {
            Color[] data = new Color[100 * 100];
            for (int x = 0; x < 100; x++)
                for (int y = 0; y < 100; y++)
                {
                    Color color = Maths.HSVToRGB(hue, x / 100f, y / 100f);
                    data[x + ((99 - y) * 100)] = color;
                }

            hsvPalette.Bind();
            hsvPalette.SetData(25, 100, data);
            hsvPalette.Unbind();
        }

        void UpdateColor()
        {
            Color color = Maths.HSVToRGB(hueSlider.Hue,
                hsvPicker.Saturation, hsvPicker.Value);
            Color = new Color(color.R, color.G, color.B, AllowAlpha ? alphaSlider.Alpha : (byte)255);
            colorPreview.Image.Color = Color;

            hueTextField.Text = ((int)hueSlider.Hue).ToString();
            saturationTextField.Text = ((int)(hsvPicker.Saturation * 100f)).ToString();
            valueTextField.Text = ((int)(hsvPicker.Value * 100f)).ToString();
            redTextField.Text = Color.R.ToString();
            greenTextField.Text = Color.G.ToString();
            blueTextField.Text = Color.B.ToString();
            alphaTextField.Text = Color.A.ToString();
        }

        public override void Update(float deltaTime)
        {
            alphaSlider.Visible = alphaTextField.Visible = alphaLabel.Visible = AllowAlpha;
            hsvPicker.Size.X.Offset = AllowAlpha ? -70 : -35;
            base.Update(deltaTime);
        }

        public override void Draw(SpriteBatch sb) { }
    }
}
