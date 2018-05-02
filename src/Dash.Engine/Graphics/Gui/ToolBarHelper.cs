using System.Collections.Generic;

/* ToolBarHelper.cs v2
 * Tristan Smith
*/

namespace Dash.Engine.Graphics.Gui {
    public class ToolBarHelper {

        Dictionary<string, GUIDropDown> TopMost;
        Dictionary<string, Dictionary<string, GUIDropDownButton>> LevelOne;
        Dictionary<string, Dictionary<string, GUIDropDownButton>> SubMenu;
        GUIFrame ToolBar;
        
        public ToolBarHelper(GUIFrame toolbar) {
            this.ToolBar = toolbar;

            TopMost = new Dictionary<string, GUIDropDown>();
            LevelOne = new Dictionary<string, Dictionary<string, GUIDropDownButton>>();
            SubMenu = new Dictionary<string, Dictionary<string, GUIDropDownButton>>();

            foreach (GUIDropDown ddb in ToolBar.Children) {
                TopMost.Add(ddb.Label.Text, ddb);
            }            

            foreach (KeyValuePair<string, GUIDropDown> btnLevelOne in TopMost) {
                GUIDropDown main = btnLevelOne.Value;
                Dictionary<string, GUIDropDownButton> tmpChildren = new Dictionary<string, GUIDropDownButton>();
                foreach (GUIElement child in main.Children) {
                    if(child.GetType() == typeof(GUIDropDownButton)) { //we check for GUILabel
                        //We can now safely cast as GUIDropDownButton
                        GUIDropDownButton Safechild = child as GUIDropDownButton;

                        if (Safechild.Sub == null) { //check if it's a dropdown menu
                            tmpChildren.Add(Safechild.Label.Text, Safechild); //if not then just add to LevelTop
                        } else if (Safechild.Sub != null) { //if it's a dropdown then loop once more
                            Dictionary<string, GUIDropDownButton> tmpSubMenu = new Dictionary<string, GUIDropDownButton>();
                            foreach (GUIElement childSub in Safechild.Sub.Items) {
                                if (childSub.GetType() == typeof(GUIDropDownButton)) {
                                    GUIDropDownButton childButton = childSub as GUIDropDownButton;
                                    tmpSubMenu.Add(childButton.Label.Text, childButton);
                                }
                            }
                            SubMenu.Add(Safechild.Label.Text.Replace(">", "").Trim(), tmpSubMenu); //ffs the space was causing errors
                        }

                    }
                }
                LevelOne.Add(main.Label.Text, tmpChildren);
            }
        }

        public Dictionary<string, GUIDropDown> GetTopMost() {
            return TopMost;
        }

        public Dictionary<string, Dictionary<string, GUIDropDownButton>> GetLevelOne() {
            return LevelOne;
        }

        public Dictionary<string, Dictionary<string, GUIDropDownButton>> GetSubMenus() {
            return SubMenu;
        }
    }
}
