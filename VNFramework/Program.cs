using System;
using System.Collections.Generic;
using System.Text;

namespace VNFramework
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry method for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Shell())
            {
                game.Run();
            }
        }
    }
}
