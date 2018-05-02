using Dash.Engine.Diagnostics;

namespace AceOfSpades.Editor.World
{
    class Program
    {
        static void Main(string[] args)
        {
            DashCMD.Start();
            DashCMD.Listen(true);

            ProgramExceptionHandler.RunMainWithHandler(
                () => {
                    using (MainWindow window = new MainWindow())
                        window.Run(60);
                }, //try
                () => { DashCMD.Stop(); }, //finally
                () => { } //shutdown action
            );
        }
    }
}
