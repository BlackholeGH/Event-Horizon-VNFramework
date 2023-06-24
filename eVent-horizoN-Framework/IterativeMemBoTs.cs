using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNFramework
{
    public static class IterativeMemBoTs
    {
        public static void InitializeBots(Point layout)
        {
            ButtonScripts.SpoonsTrip = true;
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            //PythonController.StartPythonProcess("C:\\Users\\Blackhole\\PycharmProjects\\Brains\\venv\\socketmanager.py");
            int total = 0;
            for(int y = 0; y < layout.Y; y++)
            {
                for(int x = 0; x < layout.X; x++)
                {
                    total++;
                    Bot bot = new Bot("IMEMBOT_" + x + "_" + y, new Vector2(125 * (x+1), 125 * (y+1)), 0.4f + (0.001f * total));
                    bot.CenterOrigin = true;
                    bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                    Shell.UpdateQueue.Add(bot);
                    Shell.RenderQueue.Add(bot);
                }
            }
        }
        public class Bot : WorldEntity
        {
            public ulong SocketID
            {
                get; private set;
            }
            public Bot(String name, Vector2 location, float depth) : base(name, location, Shell.AtlasDirectory["BOT"], 0.6f)
            {
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true));
                //SocketID = PythonController.SocketInterface.AddNewSocketAsTask();
            }
        }
    }
}
