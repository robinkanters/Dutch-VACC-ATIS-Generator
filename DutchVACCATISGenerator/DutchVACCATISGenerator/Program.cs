using System;
using System.Windows.Forms;

namespace DutchVACCATISGenerator
{
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.DutchVACCATISGenerator());
        }
    }
}
