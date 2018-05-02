using Dash.Engine.Diagnostics;

namespace AceOfSpades.Editor.Models {
    class Program {
        static int Main() {
            DashCMD.Start();
            DashCMD.Listen(true);

            ProgramExceptionHandler.RunMainWithHandler(
                () => {
                    using (MainWindow window = new MainWindow()) {
                        window.Run(60);
                    }
                },
                () => { DashCMD.Stop(); },
                () => { }
                );

            return 0;
        }
    }
}
