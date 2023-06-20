using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace VNFramework
{
    /// <summary>
    /// Class that contains the code entry point Main(). Instances the game class and starts it running.
    /// </summary>
    class Launcher
    {
        static void Main(string[] args)
        {
            using (var game = new VNFramework.Shell("ultrasofiaworld_manifest.ehm", "ULTRASOFIAWORLD"))
            {
                game.Run();
            }
        }
    }
}

