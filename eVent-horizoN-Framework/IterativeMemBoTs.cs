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
        public static void InitializePhysTest(Point layout)
        {
            ButtonScripts.SpoonsTrip = true;
            Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
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
                    Bot bot = new Bot("IMEMBOT_" + x + "_" + y, new Vector2(175 + 120 * (x+1), 175 + 120 * (y+1)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 11), new Vector2(Shell.Rnd.Next(-5, 5), Shell.Rnd.Next(-5, 5)));
                    bot.CenterOrigin = true;
                    bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                    if(total == 1)
                    {
                        bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour());
                        bot.AutoRotateToVelocityBearing = false;
                        bot.Stickers.Add(Shell.AutoCamera);
                        Shell.AutoCamera.QuickMoveTo(bot.Position);
                    }
                    Shell.UpdateQueue.Add(bot);
                    Shell.RenderQueue.Add(bot);
                }
            }
        }
        public static void InitializeMainSim(int botCount, int spawnRadius)
        {
            ButtonScripts.SpoonsTrip = true;
            Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
            //PythonController.StartPythonProcess("C:\\Users\\Blackhole\\PycharmProjects\\Brains\\venv\\socketmanager.py");
            int totalWall = 0;
            for (int i = 0; i < 20; i++)
            {
                Wall wall = new Wall("WALL_" + totalWall, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.8f);
                wall.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                totalWall++;
                Shell.UpdateQueue.Add(wall);
                Shell.RenderQueue.Add(wall);
            }
            for (int i = 0; i < 10; i++)
            {
                Plant plant = new Plant("PLANT_" + i, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.6f + i * 0.001f, 30, new Vector2());
                plant.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                Shell.UpdateQueue.Add(plant);
                Shell.RenderQueue.Add(plant);
            }
            for (int i = 0; i < 10; i++)
            {
                Spike spike = new Spike("SPIKE_" + i, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.7f + i * 0.001f, 30, new Vector2());
                spike.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                Shell.UpdateQueue.Add(spike);
                Shell.RenderQueue.Add(spike);
            }
            int total = 0;
            while (total < botCount)
            {
                total++;
                Bot bot = new Bot("IMEMBOT_" + total, new Vector2((float)Math.Sin(total * (Math.PI / 4)), (float)Math.Cos(total * (Math.PI / 4))) * (float)(105 * Math.Ceiling((total-1) / 8d)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 11), new Vector2());
                bot.CenterOrigin = true;
                bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                bot.MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                if (total == 1)
                {
                    bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour());
                    bot.AutoRotateToVelocityBearing = false;
                    bot.Stickers.Add(Shell.AutoCamera);
                    Shell.AutoCamera.QuickMoveTo(bot.Position);
                }
                Shell.UpdateQueue.Add(bot);
                Shell.RenderQueue.Add(bot);
            }
        }
        public class Bot : DynamicEntity
        {
            public ulong SocketID
            {
                get; private set;
            }
            public Boolean AutoRotateToVelocityBearing { get; set; }
            public Bot(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["BOT"], depth, mass)
            {
                int subtractColour = (int)((250 / 9) * (mass - 1));
                ColourValue = new Color(255, 255 - subtractColour, 255 - subtractColour);
                Collider = new RadialCollider(50, new Vector2());
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true));
                Velocity = initialVelocity;
                AutoRotateToVelocityBearing = true;
                //MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                //SocketID = PythonController.SocketInterface.AddNewSocketAsTask();
            }
            public override void Update()
            {
                /*if(Name == "IMEMBOT_1")
                {
                    Console.WriteLine("Position: " + Position);
                    Console.WriteLine("Velocity: " + Velocity);
                    Console.WriteLine("Acceleration: " + Acceleration);
                }*/
                if (AutoRotateToVelocityBearing) { RotationRads = (float)new Trace(Velocity).Bearing; }
                base.Update();
            }
        }
        public class Plant : DynamicEntity
        {
            public Plant(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["FOODPLANT"], depth, mass)
            {
                CenterOrigin = true;
                Collider = new RadialCollider(60, new Vector2());
                Velocity = initialVelocity;
                MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                AngularVelocity = (float)(Shell.Rnd.NextDouble() - 0.5) * 0.05f;
            }
        }
        public class Spike : DynamicEntity
        {
            public Spike(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["DAMAGESPIKE"], depth, mass)
            {
                CenterOrigin = true;
                Collider = new RadialCollider(60, new Vector2());
                Velocity = initialVelocity;
                MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                AngularVelocity = (float)(Shell.Rnd.NextDouble() - 0.5) * 0.05f;
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
