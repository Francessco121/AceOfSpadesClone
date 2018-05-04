using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

/* GUIDropDown.cs
 * Ethan Lafrenais
 * Tristan Smith
*/

namespace Dash.Engine.Graphics.Gui
{
    public delegate void GUIDropDownButtonClick(GUIDropDown dropDown, GUIDropDownButton btn);

    //<-- sc -->
    public class GUIDropDownButtonConfig
    {
        public string text;
        public object value;
        public GUIDropDown opensSub;
        public GUIDropDownButtonClick callback = null;
    }

    public class SubDropdownConfig
    {
        public string Title;
        public GUIDropDownButtonConfig[] subButtons;
    }
    //<!-- sc -->


    public class GUIDropDownButton : GUIButton
    {
        public object Value;
        public GUIDropDown Sub;
        public GUIDropDownButtonClick Callback;
        public int Index;

        GUIDropDown dropdown;

        const float mouseTimeout = 0.01f;
        float timeOut;

        public GUIDropDownButton(UDim2 position, UDim2 size, GUITheme theme,
            string text, GUIButtonClick onClick, GUIDropDown dropdown, object value,
            GUIDropDown sub, GUIDropDownButtonClick callback, int index)
            : base(position, size, text + (sub != null ? " >" : ""), theme)
        {
            Sub = sub;
            this.dropdown = dropdown;
            Parent = dropdown;
            Value = value;
            Callback = callback;
            Index = index;

            if (sub != null)
                ActiveImage = HoverImage;

            Visible = false;
            Label.Visible = false;
        }

        protected override void MouseClick(MouseButton mbtn)
        {
            if (Callback != null)
                Callback(dropdown, this);

            if (Sub == null)
                dropdown.Close();

            base.MouseClick(mbtn);
        }

        public override void Update(float deltaTime)
        {
            if (Sub != null)
            {
                bool mOver = (IsMouseOver || Sub.IsMouseOver || (Sub.Visible && Sub.IsChildMousedOver));
                Sub.Visible = Visible && (timeOut > 0 || mOver);
                Sub.Open = Visible && (timeOut > 0 || mOver);

                if (mOver)
                    timeOut = mouseTimeout;
                else
                    timeOut -= deltaTime;
            }

            base.Update(deltaTime);
        }
    }

    public class GUIDropDown : GUIButton
    {
        public bool Open;
        public int ItemCount
        {
            get { return Items.Count; }
        }

        public string Text
        {
            get { return Label.Text; }
            set { Label.Text = value; }
        }

        public bool IsChildMousedOver
        {
            get
            {
                foreach (GUIDropDownButton btn in Items)
                    if (btn.IsMouseOver || (btn.Sub != null && (btn.Sub.IsMouseOver || btn.Sub.IsChildMousedOver)))
                        return true;

                return false;
            }
        }

        public object CurrentValue { get; private set; }
        public bool HideMainButton;

        public List<GUIDropDownButton> Items { get; private set; }

        public GUIDropDown Child
        {
            get { return child; }
            set
            {
                if (value == this)
                    throw new Exception("Cannot set child dropdown to itself!");

                child = value;
            }
        }

        GUIDropDown child;

        bool applyTextOnClick;

        public GUIDropDown(UDim2 position, UDim2 size, GUITheme theme, bool applyTextOnClick)
            : base(position, size, "", theme)
        {
            this.applyTextOnClick = applyTextOnClick;
            Items = new List<GUIDropDownButton>();
        }

        //<-- sc -->
        public GUIDropDownButton AddItem(GUIDropDownButtonConfig button)
        {
            return AddButton(button.text, button.value, null, button.callback);
        }
        //<!-- sc -->

        public GUIDropDownButton AddItem(string text, object value, GUIDropDownButtonClick callback = null)
        {
            return AddButton(text, value, null, callback);
        }

        //<-- sc -->
        public GUIDropDownButton AddItemSub(GUIDropDownButtonConfig button)
        {

            GUIDropDownButton btn = AddButton(button.text, button.value, button.opensSub, button.callback);

            button.opensSub.Visible = false;
            button.opensSub.Parent = this;
            button.opensSub.Position = new UDim2(1, 0, Size.Y.Scale * (ItemCount + (HideMainButton ? -1 : 0)), Size.Y.Offset * ItemCount);
            return btn;
        }
        //<!-- sc -->

        public GUIDropDownButton AddItemSub(string text, GUIDropDown opensSub, GUIDropDownButtonClick callback = null)
        {
            GUIDropDownButton btn = AddButton(text, null, opensSub, callback);

            opensSub.Visible = false;
            opensSub.Parent = this;
            opensSub.Position = new UDim2(1, 0, Size.Y.Scale * (ItemCount + (HideMainButton ? -1 : 0)), Size.Y.Offset * ItemCount);
            return btn;
        }

        GUIDropDownButton AddButton(string text, object value, GUIDropDown sub, GUIDropDownButtonClick callback)
        {
            GUIDropDownButton btn = new GUIDropDownButton(new UDim2(0, 0, ItemCount + (HideMainButton ? 0 : 1), 0), new UDim2(1, 0, 1, 0),
                Theme, text, OnSubClick, this, value, sub, callback, ItemCount);
            Items.Add(btn);

            if (ItemCount == 1 && applyTextOnClick)
            {
                Label.Text = text;
                CurrentValue = value;
            }

            return btn;
        }

        public void Close()
        {
            GUIDropDown root = this;
            while (root.Parent != null && root.Parent is GUIDropDown)
            {
                root.Open = false;
                root = (GUIDropDown)root.Parent;
            }

            root.Open = false;
        }

        public void RemoveItem(string text)
        {
            foreach (GUIDropDownButton btn in Items)
                if (btn.Label.Text == text)
                {
                    Items.Remove(btn);
                    break;
                }

            for (int i = 0; i < Items.Count; i++)
                Items[i].Index = i;
        }

        public void RemoveItemAt(int index)
        {
            if (index < 0 || index <= ItemCount)
                throw new IndexOutOfRangeException();

            Items.RemoveAt(index);

            for (int i = 0; i < Items.Count; i++)
                Items[i].Index = i;
        }

        protected override void MouseClick(MouseButton mbtn)
        {
            if (mbtn == MouseButton.Left)
                Open = !Open;

            base.MouseClick(mbtn);
        }

        void OnSubClick(GUIButton _btn, MouseButton mbtn)
        {
            if (mbtn != MouseButton.Left)
                return;

            GUIDropDownButton btn = (GUIDropDownButton)_btn;

            if (applyTextOnClick && btn.Sub == null)
            {
                Label.Text = btn.Label.Text;
                CurrentValue = btn.Value;
            }

            if (btn.Callback != null)
                btn.Callback(this, btn);

            if (btn.Sub == null)
                Open = false;
        }

        public override void Update(float deltaTime)
        {
           if (Input.GetMouseButtonUp(MouseButton.Left) && !IsMouseOver && !IsChildMousedOver)
               Open = false;

            foreach (GUIDropDownButton btn in Items)
            {
                btn.Visible = Open;
                btn.Label.Visible = Open;

                if (!Open && btn.Sub != null)
                    btn.Sub.Open = false;
            }

            base.Update(deltaTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (!HideMainButton)
                base.Draw(sb);

            if (Open)
            {
                foreach (GUIDropDownButton btn in Items)
                {
                    btn.Draw(sb);
                    btn.Label.Draw(sb);
                }
            }
        }
    }
}
