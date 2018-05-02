using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

/* GUIElement.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public delegate void GUIElementEmptyEvent();
    public delegate void GUIElementClickEvent(MouseButton button);

    public abstract class GUIElement
    {
        public event GUIElementEmptyEvent OnMouseEnter;
        public event GUIElementEmptyEvent OnMouseLeave;
        public event GUIElementClickEvent OnMouseButtonDown;
        public event GUIElementClickEvent OnMouseButtonUp;

        public GUIElement Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                DeltaState.HierarchyChanged = true;

                if (parent != null)
                    parent.RemoveChild(this);

                // Check for heirarchy recursion loop
                if (value != null && value.parent == this)
                    throw new InvalidOperationException("Two GUI elements can't be parented to eachother!");

                parent = value;
                if (value != null)
                    parent.AddChild(this);
                else
                    GUIArea = null;
            }
        }
        GUIElement parent;

        public float ZIndex
        {
            get { return zIndex; }
            set
            {
                DeltaState.ZIndexChanged = value != zIndex;
                zIndex = value;
            }
        }
        float zIndex;

        public UDim2 Position;
        public UDim2 Size;
        public UDim2 MaxSize = new UDim2(1f, 0, 1f, 0);
        public UDim2 MinSize = UDim2.Zero;
        public Vector4 Padding;
        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible == value)
                    return;

                if (value)
                    Shown();
                else
                    Hid();

                visible = value;
            }
        }
        bool visible = true;
        public bool Disabled;
        public bool CapturesMouseClicks;

        public GUIArea GUIArea { get; internal set; }
        public int HierarchyLevel { get; internal set; }
        public bool IsMouseOver { get; internal set; }
        public Rectangle CalculatedRectangle
        {
            get { return rect + (GUIArea != null ? GUIArea.GetOffset() : Vector2.Zero); }
            set { rect = value; }
        }

        Rectangle rect;
        public GUITheme Theme { get; protected set; }

        internal List<GUIElement> Children { get; }
        internal GUIElementDeltaState DeltaState { get; }
        internal GUIElementMouseState MouseState { get; }

        public GUIElement(GUITheme theme)
            : this()
        {
            Theme = theme;
        }

        public GUIElement()
        {
            Children = new List<GUIElement>();
            DeltaState = new GUIElementDeltaState(this);
            MouseState = new GUIElementMouseState(this);
        }

        protected virtual void Shown() { }
        protected virtual void Hid() { }

        public virtual void MouseEnter()
        {
            if (OnMouseEnter != null)
                OnMouseEnter();
        }

        public virtual void MouseLeave()
        {
            if (OnMouseLeave != null)
                OnMouseLeave();
        }

        public virtual void MouseButtonDown(MouseButton mbtn)
        {
            if (OnMouseButtonDown != null)
                OnMouseButtonDown(mbtn);
        }

        public virtual void MouseButtonUp(MouseButton mbtn)
        {
            if (OnMouseButtonUp != null)
                OnMouseButtonUp(mbtn);
        }

        internal protected bool CanDraw()
        {
            if (GUIArea != null && !GUIArea.Visible)
                return false;
            else if (!Visible)
                return false;
            else if (parent != null)
                return parent.CanDraw();
            else
                return true;
        }

        void AddChild(GUIElement element)
        {
            Children.Add(element);
            DeltaState.HierarchyChanged = true;
        }

        void RemoveChild(GUIElement element)
        {
            Children.Remove(element);
            DeltaState.HierarchyChanged = true;
        }

        public virtual Rectangle CalculateDimensions(Rectangle parentDim)
        {
            // Calculate base position and sizing
            Vector2 pos = Position.GetValue(parentDim.Width, parentDim.Height)
                + parentDim.Location;
            Vector2 size = Size.GetValue(parentDim.Width, parentDim.Height);
            Vector2 minSize = MinSize.GetValue(parentDim.Width, parentDim.Height);
            Vector2 maxSize = MaxSize.GetValue(parentDim.Width, parentDim.Height);

            // Apply min/max sizes
            Vector2 finalSize = new Vector2(
                MathHelper.Clamp(size.X, minSize.X, maxSize.X),
                MathHelper.Clamp(size.Y, minSize.Y, maxSize.Y));

            Vector2 delta = size - finalSize;
            pos += delta / 2f;

            // Apply padding
            pos.X += Padding.X;
            pos.Y += Padding.Y;
            finalSize.Y -= (Padding.X + Padding.Z);
            finalSize.Y -= (Padding.Y + Padding.W);

            // All set
            return new Rectangle(pos, finalSize);
        }

        public virtual void Update(float deltaTime) { }
        public abstract void Draw(SpriteBatch sb);
    }
}
