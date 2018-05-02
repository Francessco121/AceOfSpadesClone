/* ToolBarCreator.cs
 * Tristan Smith
*/

//TODO figure out if spliting this class to handle ToolBars and StatusBars

namespace Dash.Engine.Graphics.Gui {

    /// <summary>
    /// Helps with creation of ToolBars and Dropdown menus
    /// </summary>
    public class ToolBarCreator {

        float buttonWidth;
        int countDiv;
        GUITheme theme;
        GUIFrame bar;

        /// <summary>
        /// Create new helper
        /// </summary>
        /// <param name="WindowWidth"></param>
        /// <param name="theme"></param>
        public ToolBarCreator(GUITheme theme) {
            this.theme = theme;
            this.countDiv = 0;
            this.bar = new GUIFrame(UDim2.Zero, new UDim2(1, 0, 0, 40), this.theme);
        }

        /// <summary>
        /// How many buttons do we want?
        /// WindowWidth / NumOfButtonsWanted
        /// </summary>
        /// <param name="NumberOfButtons"></param>
        public void SetButtonWidth(int NumberOfButtons) {
            this.buttonWidth = 1f / NumberOfButtons;
        }

        /// <summary>
        /// Get ToolBar 
        /// </summary>
        /// <returns></returns>
        public GUIFrame GetToolBar() {
            return this.bar;
        }

        /// <summary>
        /// Add Buttons/Dropdowns to ToolBar
        /// </summary>
        /// <param name="name"></param>
        /// <param name="buttons"></param>
        /// <param name="subButtons"></param>
        public void Add(string name, GUIDropDownButtonConfig[] buttons, params SubDropdownConfig[] subButtons) {

            GUIDropDown toAdd = new GUIDropDown( //DONT EVER TOUCH THE MATHS HERE, I DONT WANT MORE GREY HAIR pls
                (this.countDiv == 0 ? UDim2.Zero : new UDim2(this.buttonWidth * this.countDiv, 0, 0, 0)), //if this is our first button then pin it to the far left
                new UDim2(this.buttonWidth, 0, 1, 0),
                this.theme,
                false) { Parent = this.bar, Text = name };

            foreach(GUIDropDownButtonConfig btnConfig in buttons) { //foreach button wanted
               toAdd.AddItem(btnConfig); //add to bar
            }

            if (subButtons != null) { //if there is a sub menu
                for(int i = 0; i < subButtons.Length; i++) { //then for how many
                    GUIDropDown subToAdd = new GUIDropDown(UDim2.Zero, new UDim2(.5f, 0, 1, 0), this.theme, false) { HideMainButton = true }; //create a dropdown menu
                    toAdd.AddItemSub((subButtons[i].Title != null ? subButtons[i].Title : "Sub Drop Menu"), subToAdd); //add it to the current button with title
                    foreach (GUIDropDownButtonConfig btnConfig in subButtons[i].subButtons) { //then foreach button within the sub menu
                        subToAdd.AddItem(btnConfig); //add to new dropdown menu entry
                    }
                }
            }

            this.countDiv++; //increment up so all buttons fit; somewhat
        }
    }
}
