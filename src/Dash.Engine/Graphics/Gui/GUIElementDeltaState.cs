/* GUIElementDeltaState.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    class GUIElementDeltaState
    {
        public bool ZIndexChanged;
        public bool HierarchyChanged;

        GUIElement element;

        public GUIElementDeltaState(GUIElement element)
        {
            this.element = element;
        }

        UDim2 lastPosition;
        UDim2 lastSize;
        UDim2 lastMinSize;
        UDim2 lastMaxSize;
        Vector4 lastPadding;

        public bool CheckForDirty()
        {
            bool isDirty = 
                lastPosition != element.Position 
                || lastSize != element.Size
                || lastMinSize != element.MinSize
                || lastMaxSize != element.MaxSize
                || lastPadding != element.Padding;

            if (isDirty)
            {
                lastPosition = element.Position;
                lastSize = element.Size;
                lastMinSize = element.MinSize;
                lastMaxSize = element.MaxSize;
                lastPadding = element.Padding;
            }

            return isDirty;
        }
    }
}
