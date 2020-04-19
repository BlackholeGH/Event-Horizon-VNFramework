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
        /// The main entry Vector2 for the application.
        /// </summary>
        public static Shell AccessShell;
        [STAThread]
        static void Main()
        {
            using (var game = new Shell())
            {
                AccessShell = game;
                game.Run();
            }
        }
    }
}
