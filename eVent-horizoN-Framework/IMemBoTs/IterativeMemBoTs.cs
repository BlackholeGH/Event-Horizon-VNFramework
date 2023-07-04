using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static VNFramework.GraphicsTools;
using static VNFramework.IterativeMemBoTs;
using static VNFramework.PythonController.SocketInterface;
using static VNFramework.PythonController;
using SharpFont;

namespace VNFramework
{
    public static class IterativeMemBoTs
    {
        public static ulong SystemSocketID { get; set; }
        public static bool SystemSocketAssigned { get; set; }
        [Serializable]
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
            //Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
            int totalWall = 0;
            for (int i = 0; i < 5; i++)
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
            for (int y = 0; y < layout.Y; y++)
            {
                for (int x = 0; x < layout.X; x++)
                {
                    total++;
                    Bot bot = new Bot("IMEMBOT_" + x + "_" + y, new Vector2(175 + 120 * (x + 1), 175 + 120 * (y + 1)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 11), new Vector2(Shell.Rnd.Next(-5, 5), Shell.Rnd.Next(-5, 5)));
                    bot.CenterOrigin = true;
                    bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                    if (total == 1)
                    {
                        bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour());
                        bot.AutoRotateToVelocityBearing = false;
                        bot.MyStickers.Add(Shell.AutoCamera);
                        Shell.AutoCamera.QuickMoveTo(bot.Position);
                    }
                    Shell.UpdateQueue.Add(bot);
                    Shell.RenderQueue.Add(bot);
                }
            }
        }
        public static void SendSimStartStopSocketCommand(object arg)
        {
            Boolean runsim = false;
            if(arg is Checkbox)
            {
                runsim = ((Checkbox)arg).Toggle;
            }
            else if(arg is bool)
            {
                runsim = (bool)arg;
            }
            Double[] codes = new double[] { 4, runsim ? 1 : 0 };
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d);
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false);
        }
        public static void InitMembotsUI()
        {
            if (Shell.GetEntityByName("IMEMBOTS_UIBOX") == null)
            {
                WorldEntity uiBox = new WorldEntity("IMEMBOTS_UIBOX", new Vector2(0, 502), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_BACKING"], 0.9f);
                uiBox.IsUIElement = true;
                Shell.UpdateQueue.Add(uiBox);
                Shell.RenderQueue.Add(uiBox);
            }
            if (Shell.GetEntityByName("IMEMBOTS_TITLELABEL") == null)
            {
                TextEntity titleLabel = new TextEntity("IMEMBOTS_TITLELABEL", "IMemBoTs Neural Simulator", new Vector2(10, 520), 0.95f);
                titleLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(titleLabel);
                Shell.RenderQueue.Add(titleLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_STARTLABEL") == null)
            {
                TextEntity startLabel = new TextEntity("IMEMBOTS_STARTLABEL", "[F:SYSFONT]Toggle neural controllers:", new Vector2(20, 560), 0.95f);
                startLabel.IsUIElement = true;
                startLabel.BufferLength = 120;
                Shell.UpdateQueue.Add(startLabel);
                Shell.RenderQueue.Add(startLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_STARTSTOPBUTTON") == null)
            {
                Checkbox startstop = new Checkbox("IMEMBOTS_STARTSTOPBUTTON", new Vector2(70, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_STARTSTOP"], 0.95f, false);
                startstop.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendSimStartStopSocketCommand"), new object[] { startstop });
                startstop.IsUIElement = true;
                Shell.UpdateQueue.Add(startstop);
                Shell.RenderQueue.Add(startstop);
            }
            if (Shell.GetEntityByName("BUTTON_HIDE_UI") == null)
            {
                Checkbox hideUI = new Checkbox("BUTTON_HIDE_UI", new Vector2(1100, 660), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_EYECHECKBOX"], 0.95f, false);
                hideUI.CenterOrigin = false;
                hideUI.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("RefreshUIHideState"), null);
                hideUI.IsUIElement = true;
                Shell.UpdateQueue.Add(hideUI);
                Shell.RenderQueue.Add(hideUI);
            }
        }
        public static void InitializeMainSim(int botCount, int spawnRadius)
        {
            //botCount = 1;
            Shell.WriteLine("Initializing IMemBoTs simulation.");
            ButtonScripts.SpoonsTrip = true;
            //Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
            InitMembotsUI();
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
                Bot bot = new Bot("IMEMBOT_" + total, new Vector2((float)Math.Sin(total * (Math.PI / 4)), (float)Math.Cos(total * (Math.PI / 4))) * (float)(200 * Math.Ceiling((total-1) / 8d)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 1), new Vector2());
                bot.CenterOrigin = true;
                bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                bot.MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                bot.MyBehaviours.Add(new Behaviours.ConsoleReaderBehaviour());
                if (total == 1)
                {
                    bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour());
                    bot.AutoRotateToVelocityBearing = false;
                    bot.MyStickers.Add(Shell.AutoCamera);
                    Shell.AutoCamera.QuickMoveTo(bot.Position);
                }
                Shell.UpdateQueue.Add(bot);
                Shell.RenderQueue.Add(bot);
            }
            Shell.AutoCamera.AutoSnapToOnResetEntityName = "IMEMBOT_1";
        }
        [Serializable]
        public class Bot : DynamicEntity
        {
            public ulong SocketID
            {
                get; private set;
            }
            public Boolean AutoRotateToVelocityBearing { get; set; }
            protected void SetupTraces()
            {
                /*GraphicsTools.Trace testTrace = new GraphicsTools.Trace(new Vector2(), new Vector2(100, -100));
                testTrace.DrawColour = Color.Yellow;
                testTrace.AlignToEntity = false;
                testTrace.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(testTrace);*/

                GraphicsTools.Trace forwardSense = new GraphicsTools.Trace(new Vector2(), 0, 500);
                forwardSense.DrawColour = Color.Yellow;
                forwardSense.AlignToEntity = true;
                forwardSense.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(forwardSense);
                _senseTraces.Add(forwardSense);

                GraphicsTools.Trace leftSense = new GraphicsTools.Trace(new Vector2(), -Math.PI / 4, 500);
                leftSense.DrawColour = Color.Yellow;
                leftSense.AlignToEntity = true;
                leftSense.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(leftSense);
                _senseTraces.Add(leftSense);

                GraphicsTools.Trace rightSense = new GraphicsTools.Trace(new Vector2(), Math.PI / 4, 500);
                rightSense.DrawColour = Color.Yellow;
                rightSense.AlignToEntity = true;
                rightSense.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(rightSense);
                _senseTraces.Add(rightSense);
            }
            private List<GraphicsTools.Trace> _senseTraces = new List<GraphicsTools.Trace>();
            public Bot(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["BOT"], depth, mass)
            {
                SocketID = AddNewSocketAsTask();
                int subtractColour = (int)((250 / 9) * (mass - 1));
                ColourValue = new Color(255, 255 - subtractColour, 255 - subtractColour);
                Collider = new RadialCollider(50, new Vector2());
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true));
                Velocity = initialVelocity;
                AutoRotateToVelocityBearing = false;
                SelfMovementForceMultiplier = 5;
                SelfRotationRate = 5;

                SetupTraces();
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
            public double[] SenseEnvironment()
            {
                double[] sensingOut = new double[128];
                sensingOut[0] = Math.Min(10d, GraphicsTools.Trace.GetAlignedComponent(Velocity, ForwardTrace.AsAlignedVector).Length() / 10d); //Forward-aligned velocity
                sensingOut[1] = Math.Min(10d, GraphicsTools.Trace.GetPerpendicularComponent(Velocity, ForwardTrace.AsAlignedVector).Length() / 10d); //Sideways-aligned velocity
                for (int i = 0; i < _senseTraces.Count; i++)
                {
                    GraphicsTools.Trace thisTrace = _senseTraces[i];
                    GraphicsTools.Trace sensingTrace = thisTrace.Scale(new Vector2(), Size).Rotate(new Vector2(), RotationRads).Translate(Position);
                    Double nearestDistance = Double.NaN;
                    WorldEntity nearestObject = null;
                    Boolean detect = false;
                    foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                    {
                        if (worldEntity == this) { continue; }
                        if (worldEntity.Collider != null && (Position - worldEntity.Position).Length() <= worldEntity.Collider.GetMaximumExtent() + sensingTrace.Length)
                        {
                            Vector2? potentialIntersection = worldEntity.Collider.GetFirstIntersection(sensingTrace);
                            if (potentialIntersection != null)
                            {
                                detect = true;
                                Double distance = (((Vector2)potentialIntersection) - sensingTrace.Origin).Length();
                                if(Double.IsNaN(nearestDistance) || distance < nearestDistance)
                                {
                                    nearestDistance = distance;
                                    nearestObject = worldEntity;
                                }
                            }
                        }
                    }
                    if (detect)
                    {
                        _senseTraces[i].DrawColour = Color.Red;
                        sensingOut[2 + (i * 2)] = nearestDistance / sensingTrace.Length;
                        if (nearestObject is Plant)
                        {
                            sensingOut[3 + (i * 2)] = 1d;
                        }
                        else if (nearestObject is Spike)
                        {
                            sensingOut[3 + (i * 2)] = -1d;
                        }
                        else
                        {
                            sensingOut[3 + (i * 2)] = 0d;
                        }
                    }
                    else
                    {
                        _senseTraces[i].DrawColour = Color.Yellow;
                        sensingOut[2 + (i * 2)] = 1d;
                        sensingOut[3 + (i * 2)] = 0d;
                    }
                }
                return sensingOut;
            }
            public float SelfMovementForceMultiplier { get; set; }
            public float SelfRotationRate { get; set; }
            public override void Update()
            {
                if (AutoRotateToVelocityBearing) { RotationRads = (float)new VNFramework.GraphicsTools.Trace(Velocity).Bearing; }

                _senseCodes = SenseEnvironment();

                DispatchCodesToSocket(_senseCodes);

                double[] receive = GetCodesFromSocket();
                if(receive.Length > 0)
                {
                    _controlCodes = receive;
                    //Shell.WriteLine("Receieved on socket " + SocketID + ": " + receive[0]);
                }
                ApplyForce(ForwardTrace.AsAlignedVector * (float)_controlCodes[0] * SelfMovementForceMultiplier);
                Rotate((float)_controlCodes[1] * SelfRotationRate);
                base.Update();
            }
            private Boolean _socketClosed = false;
            public override void ManualDispose()
            {
                if (PythonController.SocketInterface.OpenSockets.Contains(SocketID) && !_socketClosed) { CloseSocket(SocketID); }
                _socketClosed = true;
                base.ManualDispose();
            }
        }
        [Serializable]
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
        [Serializable]
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
        [Serializable]
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
