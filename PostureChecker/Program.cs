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
            //DataBase data = new DataBase();
            //data.MySqlOp();
            ApplicationConfiguration.Initialize();

            //����������
            Application.Run(new Menu());
        }
    }
}