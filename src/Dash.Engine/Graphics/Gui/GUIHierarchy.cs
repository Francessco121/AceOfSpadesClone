using System;
using System.Collections.Generic;

/* GUIHierarchy.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// Hierarchy manager for GUIElements.
    /// Sorts elements by ZIndex and keeps track of any changes.
    /// </summary>
    public class GUIHierarchy
    {
        public List<GUIElement> AllSorted { get; private set; }
        public HashSet<GUIElement> TopLevelElements { get; private set; }

        bool requireReSort;

        List<GUIElement> allSortedBackBuffer;
        HashSet<GUIElement> topLevelBackBuffer;

        public GUIHierarchy()
        {
            AllSorted = new List<GUIElement>();
            TopLevelElements = new HashSet<GUIElement>();
            allSortedBackBuffer = new List<GUIElement>();
            topLevelBackBuffer = new HashSet<GUIElement>();
        }

        /// <summary>
        /// Adds a top level element and all of it's children.
        /// </summary>
        public void AddTopLevel(GUIElement element)
        {
            if (element.Parent != null)
                throw new InvalidOperationException("Cannot add GUIElement with parent to a GUIHierarchy!");

            if (TopLevelElements.Add(element))
                AddElementWithChildren(element);
        }

        /// <summary>
        /// Removes a top level element and all of it's children.
        /// </summary>
        public void RemoveTopLevel(GUIElement element)
        {
            if (element.Parent != null)
                throw new InvalidOperationException("Cannot remove GUIElement with parent from a GUIHierarchy!");

            if (TopLevelElements.Remove(element))
                RemoveElementAndChildren(element);
        }

        /// <summary>
        /// Checks the hierarchy for changes and ensures that 
        /// this.AllSorted and this.TopLevelElements is up to date.
        /// </summary>
        /// <returns>Whether or not changes were found.</returns>
        public bool Update()
        {
            bool changesFound;
            if (changesFound = ScanForChange())
                ProcessHierarchyChanges();

            if (requireReSort)
                Sort();

            return changesFound;
        }

        void Sort()
        {
            requireReSort = false;
            AllSorted.Sort(CompareZ);
        }

        float GetZ(GUIElement e)
        {
            return e.ZIndex + e.HierarchyLevel + (e.Parent != null ? GetZ(e.Parent) : 0);
        }

        int CompareZ(GUIElement a, GUIElement b)
        {
            return GetZ(a).CompareTo(GetZ(b));
        }

        void ProcessHierarchyChanges()
        {
            requireReSort = true;
            topLevelBackBuffer.Clear();
            allSortedBackBuffer.Clear();

            // Check for changes in top level elements
            foreach (GUIElement tel in TopLevelElements)
            {
                if (tel.Parent != null)
                {
                    // Top level element got a parent,
                    // so add the new top-most parent here.
                    GUIElement newTop = GetTopLevel(tel);
                    topLevelBackBuffer.Add(newTop);
                }
                else
                    topLevelBackBuffer.Add(tel);
            }

            // Go through new hierarchy and add to back buffer
            foreach (GUIElement tel in topLevelBackBuffer)
                ProcessHierarchyChange(tel, 0);

            // Swap buffers
            HashSet<GUIElement> tempTop = TopLevelElements;
            TopLevelElements = topLevelBackBuffer;
            topLevelBackBuffer = tempTop;
            List<GUIElement> tempSorted = AllSorted;
            AllSorted = allSortedBackBuffer;
            allSortedBackBuffer = tempSorted;

            // Clear
            topLevelBackBuffer.Clear();
            allSortedBackBuffer.Clear();
        }

        void ProcessHierarchyChange(GUIElement element, int hierarchyLevel)
        {
            // Recursive add element hierarchy to sorted back buffer
            allSortedBackBuffer.Add(element);
            element.HierarchyLevel = hierarchyLevel;

            for (int i = 0; i < element.Children.Count; i++)
                ProcessHierarchyChange(element.Children[i], hierarchyLevel + 1);
        }

        void RemoveElementAndChildren(GUIElement element)
        {
            requireReSort = true;

            // Remove this element
            AllSorted.Remove(element);

            // Remove this element's children
            for (int i = 0; i < element.Children.Count; i++)
                RemoveElementAndChildren(element.Children[i]);
        }

        void AddElementWithChildren(GUIElement element)
        {
            requireReSort = true;

            // Add this element
            if (!AllSorted.Contains(element))
                AllSorted.Add(element);

            // Add this element's children
            for (int i = 0; i < element.Children.Count; i++)
                AddElementWithChildren(element.Children[i]);
        }

        bool ScanForChange()
        {
            // Search all elements for changes,
            // reset hierarchy changed and zindex 
            // changed value along the way.
            bool foundChange = false;
            for (int i = 0; i < AllSorted.Count; i++)
            {
                GUIElement element = AllSorted[i];
                GUIElementDeltaState delta = element.DeltaState;

                if (delta.HierarchyChanged)
                {
                    foundChange = true;
                    delta.HierarchyChanged = false;
                }

                if (delta.ZIndexChanged)
                {
                    requireReSort = true;
                    delta.ZIndexChanged = false;
                }
            }

            return foundChange;
        }

        /// <summary>
        /// Returns the top-most parent of the given element.
        /// </summary>
        /// <param name="element">The element to start from.</param>
        /// <returns>The top-most parent.</returns>
        public static GUIElement GetTopLevel(GUIElement element)
        {
            GUIElement top = element;
            while (top.Parent != null)
                top = top.Parent;

            return top;
        }
    }
}
