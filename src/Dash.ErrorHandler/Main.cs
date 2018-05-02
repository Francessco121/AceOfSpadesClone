using System;

namespace Dash.ErrorHandler {
    class Program {
        [STAThread]
        static void Main(string[] args) {

            ExceptionInfo exInfo = new ExceptionInfo();
            bool writingInner = false;

            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];
                string[] v = arg.Split(new char[] { ':' }, 2);

                ExceptionInfo e = writingInner ? exInfo.InnerException : exInfo;

                if (v[0] == "-stack")
                    e.Stack = v[1];
                else if (v[0] == "-site")
                    e.Site = v[1];
                else if (v[0] == "-message")
                    e.Message = v[1];
                else if (v[0] == "-source")
                    e.Source = v[1];
                else if (v[0] == "-exceptionType")
                    e.ExceptionType = v[1];
                else if (v[0] == "-beginInner" && v[1] == "true") {
                    exInfo.InnerException = new ExceptionInfo();
                    writingInner = true;
                }
            }

            exInfo.FillNullValues();
            if (exInfo.InnerException != null)
                exInfo.InnerException.FillNullValues();

            using (MainWindow mainWindow = new MainWindow(ref exInfo)) {
                mainWindow.ShowDialog();
            }
        }
    }
}
