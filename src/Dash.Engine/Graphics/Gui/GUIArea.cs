using Dash.Engine.Graphics.Gui;

/* GUIArea.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// A GUI surface for a 2D environment.
    /// </summary>
    public class GUIArea
    {
        public Rectangle Area;

        public float ZIndex
        {
            get { return zIndex; }
            set
            {
                ZIndexChanged = value != zIndex;
                zIndex = value;
            }
        }
        float zIndex;

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

        internal bool ZIndexChanged;
        bool resized;

        protected GUISystem system;
        GUIHierarchy hierarchy;

        bool elementUsedKeyboard;

        public GUIArea(GUISystem system)
        {
            this.system = system;
            hierarchy = new GUIHierarchy();

            Resize(system.ScreenWidth, system.ScreenHeight);
        }

        public void ElementUsedKeyboard()
        {
            elementUsedKeyboard = true;
        }

        public virtual void Resize(int width, int height)
        {
            Area.Width = width;
            Area.Height = height;

            resized = true;
        }

        protected virtual void Shown() { }
        protected virtual void Hid() { }

        public virtual Vector2 GetOffset()
        {
            return Vector2.Zero;
        }

        public void AddTopLevel(params GUIElement[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
                hierarchy.AddTopLevel(elements[i]);
        }

        public void RemoveTopLevel(params GUIElement[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
                hierarchy.RemoveTopLevel(elements[i]);
        }

        public virtual void Update(float deltaTime)
        {
            // Process hierarchy changes
            bool hierarchyChanged = hierarchy.Update();

            // Update all element dimensions
            foreach (GUIElement tel in hierarchy.TopLevelElements)
                UpdateElementDimensions(tel, resized || hierarchyChanged);

            resized = false;

            // Update each non-disabled element
            for (int i = 0; i < hierarchy.AllSorted.Count; i++)
            {
                GUIElement element = hierarchy.AllSorted[i];
                if (!element.Disabled)
                    element.Update(deltaTime);
            }
        }

        public void ProcessMouse(bool handledClick, bool handledMouseOver, 
            out bool outHandledClick, out bool outHandledMouseOver)
        {
            // Apply mouse click and mouse over events
            for (int i = hierarchy.AllSorted.Count - 1; i >= 0; i--)
            {
                GUIElement element = hierarchy.AllSorted[i];
                GUIElementMouseState ms = element.MouseState;

                bool mouseOver = element.CapturesMouseClicks && element.CanDraw()
                    && element.CalculatedRectangle.Contains(Input.CursorPosition);
                element.IsMouseOver = !handledMouseOver && mouseOver;

                // Handle mouse enter/leave
                if (!handledMouseOver && mouseOver)
                {
                    handledMouseOver = true;
                    if (!ms.MouseWasOver)
                    {
                        element.MouseEnter();
                        ms.MouseWasOver = true;
                    }
                }
                else if (ms.MouseWasOver)
                {
                    element.MouseLeave();
                    ms.MouseWasOver = false;
                }

                // Handle mouse down/up
                for (int m = 0; m < MouseState.AllButtons.Length; m++)
                {
                    MouseButton mbtn = MouseState.AllButtons[m];

                    if (mouseOver && !element.Disabled
                        && !handledClick && Input.GetMouseButtonDown(mbtn))
                    {
                        ms.ButtonsDown.Add(mbtn);
                        element.MouseButtonDown(mbtn);
                        handledClick = true;
                    }
                    else if (!Input.GetMouseButton(mbtn) && ms.ButtonsDown.Contains(mbtn))
                    {
                        ms.ButtonsDown.Remove(mbtn);
                        element.MouseButtonUp(mbtn);
                        handledClick = true;
                    }
                }
            }

            outHandledClick = handledClick || elementUsedKeyboard;
            outHandledMouseOver = handledMouseOver;
            elementUsedKeyboard = false;
        }

        public virtual void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < hierarchy.AllSorted.Count; i++)
            {
                GUIElement element = hierarchy.AllSorted[i];
                if (element.CanDraw())
                    element.Draw(sb);
            }
        }

        /// <summary>
        /// Recursivly calls CalculateElementDimensions on this
        /// element and it's children.
        /// </summary>
        void UpdateElementDimensions(GUIElement element, bool parentChanged)
        {
            element.GUIArea = this;
            parentChanged = CalculateElementDimensions(element, parentChanged) || parentChanged;
            for (int i = 0; i < element.Children.Count; i++)
                UpdateElementDimensions(element.Children[i], parentChanged);
        }

        /// <summary>
        /// Recalculates the given element's dimensions
        /// when needed.
        /// </summary>
        /// <returns>Whether or not this element was recalculated.</returns>
        bool CalculateElementDimensions(GUIElement element, bool parentChanged)
        {
            Rectangle parentRect;
            if (element.Parent == null)
                parentRect = Area;
            else
                parentRect = element.Parent.CalculatedRectangle - GetOffset();

            if (parentChanged || element.DeltaState.CheckForDirty())
            {
                Rectangle rect = element.CalculateDimensions(parentRect);
                element.CalculatedRectangle = rect;
                return true;
            }
            else
                return false;
        }
    }
}
