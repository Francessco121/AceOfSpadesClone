using System;
using System.Text;
using System.Windows;
using System.Collections.Generic;

namespace Dash.ErrorHandler {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable {

        List<string> titles = new List<string>() {
            "Something went boom :(",
            "It dedded",
            "Ya done goofed it",
            "Ethan probably broke it",
            "I DID NOTHING",
            "My right shift crashed it..."
        };

        public MainWindow(ref ExceptionInfo exInfo) {
            InitializeComponent();
            this.Title = titles[new Random().Next(titles.Count)];
            this.windowTitle.Content = this.Title;
            this.errorText.IsReadOnly = true;
            this.Topmost = true;
            PopErrorInfo(ref exInfo);
        }

        public void Dispose() {
            titles = null;
        }

        void PopErrorInfo(ref ExceptionInfo e) {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Main Exception:\n");
            sb.AppendFormat("Type: {4}\nMessage: {0}\nSource: {1}\nTarget Site: {2}\n\nStack:\n{3}\n",
                e.Message, e.Source, e.Site, e.Stack, e.ExceptionType);

            if (e.InnerException != null) {
                sb.AppendFormat("\nInner Exception:\n");

                e = e.InnerException;
                sb.AppendFormat("Type: {4}\nMessage: {0}\nSource: {1}\nTarget Site: {2}\n\nStack:\n{3}\n",
                e.Message, e.Source, e.Site, e.Stack, e.ExceptionType);
            }
            this.errorText.Text = sb.ToString();
        }

        void closeButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        void copyButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(this.errorText.Text))
                return;

            Clipboard.SetText(this.errorText.Text);
        }

       
    }
}
