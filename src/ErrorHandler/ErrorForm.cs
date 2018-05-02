using System;
using System.Text;
using System.Windows.Forms;

namespace ErrorHandler
{
    public partial class ErrorForm : Form
    {
        public ErrorForm(ExceptionInfo e)
        {
            InitializeComponent();

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Main Exception:\n");
            sb.AppendFormat("Type: {4}\nMessage: {0}\nSource: {1}\nTarget Site: {2}\n\nStack:\n{3}\n",
                e.Message, e.Source, e.Site, e.Stack, e.ExceptionType);

            if (e.InnerException != null)
            {
                sb.AppendFormat("\nInner Exception:\n");

                e = e.InnerException;
                sb.AppendFormat("Type: {4}\nMessage: {0}\nSource: {1}\nTarget Site: {2}\n\nStack:\n{3}\n",
                e.Message, e.Source, e.Site, e.Stack, e.ExceptionType);
            }

            errorBox.Text = sb.ToString();
        }

        void closeButton_onClick(object sender, EventArgs e) {
            Application.Exit();
        }

        void copyToClipboard_onClick(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(this.errorBox.Text))
                return;

            Clipboard.SetText(this.errorBox.Text);
        }
    }
}
