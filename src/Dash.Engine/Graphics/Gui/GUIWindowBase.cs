using System;

/* GUIWindowBase.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public delegate void GUIWindowVisibleChanged(GUIWindowBase window);

    public class GUIWindowBase : GUIArea
    {
        public event GUIWindowVisibleChanged OnOpened;
        public event GUIWindowVisibleChanged OnClosed;

        public UDim2 Position;
        public UDim2 Size;
        public UDim2 MaxSize = new UDim2(1f, 0, 1f, 0);
        public UDim2 MinSize;
        public Vector2 Offset;

        public bool IsDraggable = true;

        UDim2 lastPosition;
        UDim2 lastSize;
        UDim2 lastMaxSize;
        UDim2 lastMinSize;

        GUIElement dragHandle;
        Vector2i grabCoordinate;
        Vector2 startGrabOffset;
        bool isDragging;

        public GUIWindowBase(GUISystem system, UDim2 size)
            : base(system)
        {
            Visible = false;
            Size = size;
            Center();
        }

        public GUIWindowBase(GUISystem system, UDim2 position, UDim2 size) 
            : base(system)
        {
            Visible = false;
            Position = position;
            Size = size;
        }

        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
            RecalculateSize();
        }

        void RecalculateSize()
        {
            base.Resize(system.ScreenWidth, system.ScreenHeight);

            Vector2 pos = Position.GetValue(system.ScreenWidth, system.ScreenHeight);
            Vector2 size = Size.GetValue(system.ScreenWidth, system.ScreenHeight);
            Vector2 minSize = MinSize.GetValue(system.ScreenWidth, system.ScreenHeight);
            Vector2 maxSize = MaxSize.GetValue(system.ScreenWidth, system.ScreenHeight);

            Vector2 finalSize = new Vector2(
                MathHelper.Clamp(size.X, minSize.X, maxSize.X),
                MathHelper.Clamp(size.Y, minSize.Y, maxSize.Y));

            Vector2 delta = size - finalSize;
            pos += delta / 2f;

            Area = new Rectangle(pos, finalSize);

            lastPosition = Position;
            lastSize = Size;
            lastMaxSize = MaxSize;
            lastMinSize = MinSize;
        }

        public void Center()
        {
            Position = new UDim2(
                (1f - Size.X.Scale) / 2f,
                -Size.X.Offset / 2f,
                (1f - Size.Y.Scale) / 2f,
                -Size.Y.Offset / 2f);
            Offset = Vector2.Zero;
        }

        protected override void Shown()
        {
            if (OnOpened != null)
                OnOpened(this);

            base.Shown();
        }

        protected override void Hid()
        {
            if (OnClosed != null)
                OnClosed(this);

            base.Hid();
        }

        public override Vector2 GetOffset()
        {
            return Offset;
        }

        public void SetDragHandle(GUIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element", "GUIWindowBase.SetDragHandle expected GUIElement, got null.");

            if (dragHandle != null)
            {
                dragHandle.OnMouseButtonDown -= DragHandle_OnMouseButtonDown;
                dragHandle.OnMouseButtonUp -= DragHandle_OnMouseButtonUp;
            }

            dragHandle = element;
            dragHandle.OnMouseButtonDown += DragHandle_OnMouseButtonDown;
            dragHandle.OnMouseButtonUp += DragHandle_OnMouseButtonUp;
        }

        private void DragHandle_OnMouseButtonDown(MouseButton button)
        {
            if (IsDraggable && button == MouseButton.Left)
            {
                isDragging = true;
                grabCoordinate = Input.CursorPosition;
                startGrabOffset = Offset;
            }
        }

        private void DragHandle_OnMouseButtonUp(MouseButton button)
        {
            if (button == MouseButton.Left)
                isDragging = false;
        }

        public override void Update(float deltaTime)
        {
            if (lastPosition != Position || lastSize != Size || lastMinSize != MinSize || lastMaxSize != MaxSize)
                RecalculateSize();

            base.Update(deltaTime);

            if (isDragging)
            {
                Offset.X = startGrabOffset.X + (Input.CursorX - grabCoordinate.X);
                Offset.Y = startGrabOffset.Y + (Input.CursorY - grabCoordinate.Y);
            }

            Offset.X = MathHelper.Clamp(Offset.X, -Area.X, system.ScreenWidth - (Area.Width + Area.X));
            Offset.Y = MathHelper.Clamp(Offset.Y, -Area.Y, system.ScreenHeight - (Area.Height + Area.Y));
        }
    }
}
