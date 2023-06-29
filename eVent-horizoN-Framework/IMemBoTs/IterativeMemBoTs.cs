using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static VNFramework.GraphicsTools;
using static VNFramework.IterativeMemBoTs;
using static VNFramework.PythonController.SocketInterface;

namespace VNFramework
{
    public static class IterativeMemBoTs
    {
        public static ulong SystemSocketID { get; set; }
        public static bool SystemSocketAssigned { get; set; }
        public class ControlByCodeBehaviour : Behaviours.IVNFBehaviour
        {
            public static readonly double SpeedMultiplier = 2d;
            public static readonly double TurnMultiplier = 1d;
            public void Clear() { }
            public void UpdateFunctionality(WorldEntity controllableEntity)
            {
                if(controllableEntity is Bot)
                {
                    Bot myBot = (Bot)controllableEntity;
                    double[] controlCodes = myBot.ControlCodes;
                    if (controlCodes[0] != 0 && !myBot.ImpulseKilledByActiveCollision)
                    {
                        myBot.ApplyForce(new VNFramework.GraphicsTools.Trace(new Vector2(), myBot.RotationRads, controlCodes[0] * SpeedMultiplier).AsAlignedVector);
                    }
                    if (controlCodes[1] != 0)
                    {
                        myBot.Rotate((float)(controlCodes[1] * TurnMultiplier));
                    }
                }
            }
        }
        public static void InitializePhysTest(Point layout)
        {
            ButtonScripts.SpoonsTrip = true;
            Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
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
        public class SocketCloserEntity : WorldEntity
        {
            Process _processToClose = null;
            public SocketCloserEntity(String name, Process processToClose) : base(name, new Vector2(), null, 0)
            {
                _processToClose = processToClose;
            }
            public override void ManualDispose()
            {
                if(!(_processToClose is null))
                {
                    _processToClose.Close();
                    _processToClose.Dispose();
                    _processToClose = null;
                }
                CloseAllSockets();
                base.ManualDispose();
            }
        }
        public static void InitializeMainSim(int botCount, int spawnRadius)
        {
            //botCount = 1;
            Shell.WriteLine("Initializing IMemBoTs simulation.");
            ButtonScripts.SpoonsTrip = true;
            Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
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
                Plant plant = new Plant("PLANT_" + i, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.6f + i * 0.001f, 300, new Vector2());
                plant.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                Shell.UpdateQueue.Add(plant);
                Shell.RenderQueue.Add(plant);
            }
            for (int i = 0; i < 10; i++)
            {
                Spike spike = new Spike("SPIKE_" + i, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.7f + i * 0.001f, 300, new Vector2());
                spike.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                Shell.UpdateQueue.Add(spike);
                Shell.RenderQueue.Add(spike);
            }
            Process pyProcess = PythonController.StartPythonProcess("IMemBoTs\\Python\\socketmanager.py");
            Shell.WriteLine("Python process started. Waiting for socket listener to report...");
            while(true)
            {
                System.Threading.Thread.Sleep(100);
                if (SocketsOpenedFlag)
                {
                    break;
                }
            }
            Shell.WriteLine("Socket listener reports initialization on remote process. Execution will now continue.");
            SystemSocketID = AddNewSocketAsTask();
            Shell.WriteLine("Awaiting confirmation of system socket hook-up...");
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                if (SystemSocketAssigned)
                {
                    break;
                }
            }
            Shell.WriteLine("Socket listener reports system socket assigned. Execution will now continue.");
            Shell.UpdateQueue.Add(new SocketCloserEntity("IMEMBOTS SOCKET CLOSER", pyProcess));
            int total = 0;
            while (total < botCount)
            {
                total++;
                Bot bot = new Bot("IMEMBOT_" + total, new Vector2((float)Math.Sin(total * (Math.PI / 4)), (float)Math.Cos(total * (Math.PI / 4))) * (float)(105 * Math.Ceiling((total-1) / 8d)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 1), new Vector2());
                bot.CenterOrigin = true;
                bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                bot.MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                bot.MyBehaviours.Add(new Behaviours.ConsoleReaderBehaviour());
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
                SocketID = AddNewSocketAsTask();
                int subtractColour = (int)((250 / 9) * (mass - 1));
                ColourValue = new Color(255, 255 - subtractColour, 255 - subtractColour);
                Collider = new RadialCollider(50, new Vector2());
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true));
                Velocity = initialVelocity;
                AutoRotateToVelocityBearing = false;
            }
            private double[] _controlCodes = new double[128];
            public double[] ControlCodes
            {
                get { return _controlCodes; }
            }
            private double[] _senseCodes = new double[128];
            private void DispatchCodesToSocket(double[] codes)
            {
                PySocketQuery thisReceivedQuery = GetQuery(SocketID);
                if(thisReceivedQuery.LastReceive) { return; }
                byte[] data = new byte[1024];
                int startIndex = 0;
                foreach(double d in codes)
                {
                    byte[] thisDouble = BitConverter.GetBytes(d);
                    thisDouble.CopyTo(data, startIndex);
                    startIndex += 8;
                }
                SendQuery(SocketID, data, false);
            }
            private PySocketQuery _lastReceivedQuery;
            private double[] GetCodesFromSocket()
            {
                PySocketQuery thisReceivedQuery = GetQuery(SocketID);
                double[] codes = new double[0];
                //if(thisReceivedQuery.LastReceive && !thisReceivedQuery.Receive.Equals(_lastReceivedQuery.Receive))
                if(thisReceivedQuery.LastReceive)
                {
                    AcknowledgeReceive(SocketID);
                    codes = new double[128];
                    _lastReceivedQuery = thisReceivedQuery;
                    for (int i = 0; i < 128; i++)
                    {
                        codes[i] = BitConverter.ToDouble(thisReceivedQuery.Receive.AsSpan(i * 8, 8).ToArray());
                    }
                }
                return codes;
            }
            public override void Update()
            {
                if (AutoRotateToVelocityBearing) { RotationRads = (float)new VNFramework.GraphicsTools.Trace(Velocity).Bearing; }
                _senseCodes[0] = 132425234534;
                DispatchCodesToSocket(_senseCodes);
                double[] receive = GetCodesFromSocket();
                if(receive.Length > 0)
                {
                    _controlCodes = receive;
                    //Shell.WriteLine("Receieved on socket " + SocketID + ": " + receive[0]);
                }
                base.Update();
            }
            public override void ManualDispose()
            {
                CloseSocket(SocketID);
                base.ManualDispose();
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
