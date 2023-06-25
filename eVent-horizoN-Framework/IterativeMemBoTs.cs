using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VNFramework.GraphicsTools;
using static VNFramework.IterativeMemBoTs;

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
            int totalWall = 0;
            for(int i = 0; i < 5; i++)
            {
                for (int ii = 0; ii < 2; ii++)
                {
                    Wall wall = new Wall("WALL_" + totalWall, new Vector2(175 + (i * 150), 50 + (850 * ii)), 0.8f);
                    wall.Rotate((float)(Math.PI / 2));
                    totalWall++;
                    Shell.UpdateQueue.Add(wall);
                    Shell.RenderQueue.Add(wall);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int ii = 0; ii < 2; ii++)
                {
                    Wall wall = new Wall("WALL_" + totalWall, new Vector2(50 + (850 * ii), 175 + (i * 150)), 0.8f);
                    totalWall++;
                    Shell.UpdateQueue.Add(wall);
                    Shell.RenderQueue.Add(wall);
                }
            }
            int total = 0;
            for(int y = 0; y < layout.Y; y++)
            {
                for(int x = 0; x < layout.X; x++)
                {
                    total++;
                    Bot bot = new Bot("IMEMBOT_" + x + "_" + y, new Vector2(200 + 125 * (x+1), 200 + 125 * (y+1)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 5));
                    bot.CenterOrigin = true;
                    bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                    Shell.UpdateQueue.Add(bot);
                    Shell.RenderQueue.Add(bot);
                }
            }
        }
        public class Bot : DynamicEntity
        {
            public ulong SocketID
            {
                get; private set;
            }
            public Bot(String name, Vector2 location, float depth, double mass) : base(name, location, Shell.AtlasDirectory["BOT"], depth, mass)
            {
                int subtractColour = (int)(50 * mass);
                ColourValue = new Color(255, 255 - subtractColour, 255 - subtractColour);
                Collider = new RadialCollider(50, new Vector2());
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true));
                Velocity = new Vector2(Shell.Rnd.Next(-10, 10), Shell.Rnd.Next(-10, 10));
                //SocketID = PythonController.SocketInterface.AddNewSocketAsTask();
            }
            public override void Update()
            {
                RotationRads = (float)new Trace(Velocity).Bearing;
                base.Update();
            }
        }
        public class Wall : WorldEntity
        {
            public Wall(String name, Vector2 location, float depth) : base(name, location, Shell.AtlasDirectory["HEXWALL"], depth)
            {
                CenterOrigin = true;
                Collider = new Polygon(new Rectangle(Hitbox.Location - VNFUtils.ConvertVector(Position), Hitbox.Size));
            }
        }
    }
}
