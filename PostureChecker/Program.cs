namespace PostureChecker
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            //调用主窗口
            Application.Run(new Form1());
        }
    }
}