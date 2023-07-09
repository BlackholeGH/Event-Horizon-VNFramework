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
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.X86;
using System.Reflection;
using System.Net.Sockets;

namespace VNFramework
{
    public static class IterativeMemBoTs
    {
        public static ulong SystemSocketID { get; set; }
        public static bool SystemSocketAssigned { get; set; }
        public static readonly String[] FitnessTypes = new string[] { "MoveForwardAndOut", "SurviveEnergize" };
        public interface IFitnessEvaluator
        {
            public void Initialize(Bot thisBot);
            public double Evaluate(Bot thisBot);
            public void Reset();
        }
        public class MoveForwardAndOutFitness : IFitnessEvaluator
        {
            double _extentMovingForwards = 0;
            Vector2 _startPosition;
            public void Initialize(Bot thisBot)
            {
                _extentMovingForwards = 0;
                _startPosition = thisBot.Position;
            }
            public double Evaluate(Bot thisBot)
            {
                double fitness = 0;
                if (SimulationModelRunning)
                {
                    if (thisBot.Velocity.Length() > 0)
                    {
                        Double angleDifference = GraphicsTools.AngleDifference(thisBot.ForwardTrace.Bearing, GraphicsTools.VectorToBearing(thisBot.Velocity));
                        _extentMovingForwards += (Math.PI / 2) - Math.Abs(angleDifference);
                    }
                    else
                    {
                        _extentMovingForwards -= 2;
                    }
                    double distanceTravelled = (_startPosition - thisBot.Position).Length();
                    fitness = (distanceTravelled * 5) + (_extentMovingForwards * 2);
                }
                return fitness;
            }
            public void Reset()
            {
                _extentMovingForwards = 0;
                _startPosition = new Vector2();
            }
        }
        public class SurviveEnergize : IFitnessEvaluator
        {
            double _extentMovingForwards = 0;
            Vector2 _startPosition;
            public void Initialize(Bot thisBot)
            {
                _extentMovingForwards = 0;
                _startPosition = thisBot.Position;
            }
            public double Evaluate(Bot thisBot)
            {
                double fitness = 0;
                if (SimulationModelRunning)
                {
                    if (thisBot.Velocity.Length() > 0)
                    {
                        Double angleDifference = GraphicsTools.AngleDifference(thisBot.ForwardTrace.Bearing, GraphicsTools.VectorToBearing(thisBot.Velocity));
                        _extentMovingForwards += (Math.PI / 2) - Math.Abs(angleDifference);
                    }
                    else
                    {
                        _extentMovingForwards -= 2;
                    }
                    double distanceTravelled = (_startPosition - thisBot.Position).Length();
                    fitness = (distanceTravelled * 5) + _extentMovingForwards;
                }
                return fitness;
            }
            public void Reset()
            {
                _extentMovingForwards = 0;
                _startPosition = new Vector2();
            }
        }
        public class AutoTrainController : WorldEntity
        {
            static Boolean s_extant = false;
            Boolean _thisExtant = false;
            public uint GenerationLength { get; set; }
            public AutoTrainController(uint generationLengthMillis) : base("IMEMBOTS_AUTOTRAIN_CONTROLLER", new Vector2(), null, 1)
            {
                if (s_extant)
                {
                    Shell.DeleteQueue.Add(this);
                }
                else
                {
                    GenerationLength = generationLengthMillis;
                    s_extant = true;
                    _thisExtant = true;
                }
            }
            public override void ManualDispose()
            {
                if (_thisExtant)
                {
                    s_extant = false;
                    AutoTrainerRunning = false;
                }
                base.ManualDispose();
            }
            Boolean _selfActive = false;
            int _lastGenerationStartTime = Environment.TickCount;
            int _reachedEnd = 0;
            void InitGeneration()
            {
                ResetBotPositions();
                _lastGenerationStartTime = Environment.TickCount;
                SendSimStartStopSocCommand(true);
            }
            int _timeOfBotSystemCommandDispatch = 0;
            int _timeOfGenerationApply = 0;
            public override void Update()
            {
                base.Update();
                int thisTickCount = Environment.TickCount;
                if (_thisExtant)
                {
                    if (AutoTrainerRunning != _selfActive)
                    {
                        if (!_selfActive)
                        {
                            InitGeneration();
                            _selfActive = true;
                        }
                        else
                        {
                            SendSimStartStopSocCommand(false);
                            _selfActive = false;
                        }
                    }
                    if (_selfActive)
                    {
                        if (thisTickCount > _lastGenerationStartTime + GenerationLength)
                        {
                            double[] lastSystemCodes = GetLastSystemSocketReturn();
                            if (SimulationModelRunning) { SendSimStartStopSocCommand(false); }
                            if (_reachedEnd == 0)
                            {
                                foreach (Bot bot in Bot.Bots)
                                {
                                    bot.Halt();
                                }
                                _timeOfBotSystemCommandDispatch = Environment.TickCount;
                                SendUpdateFitnessToSocCommand();
                                _reachedEnd = 1;
                            }
                            else if(_reachedEnd == 1)
                            {
                                Boolean returned = true;
                                foreach(Bot bot in Bot.Bots)
                                {
                                    if(bot.LastSystemReturnTime < _timeOfBotSystemCommandDispatch) { returned = false; }
                                }
                                if (returned)
                                {
                                    SendDoBreedSocCommand();
                                    _reachedEnd = 2;
                                }
                            }
                            if (lastSystemCodes.Length > 0)
                            {
                                if (lastSystemCodes[0] == 5d && _reachedEnd == 2)
                                {
                                    SendApplyGenerationSocCommand();
                                    _reachedEnd = 3;
                                    _timeOfGenerationApply = thisTickCount;
                                }
                                else if (lastSystemCodes[0] == 6d && thisTickCount > _timeOfGenerationApply + 1000 && _reachedEnd == 3)
                                {
                                    _reachedEnd = 0;
                                    InitGeneration();
                                }
                            }
                        }
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
        public static Boolean SimulationModelRunning { get; private set; }
        public static Boolean AutoTrainerRunning { get; private set; }
        public static Double MutationChance { get; private set; }
        public static void SendSimStartStopSocCommand(object arg)
        {
            Boolean runsim = false;
            if(arg is Checkbox)
            {
                runsim = ((Checkbox)arg).Toggle;
            }
            else if(arg is bool)
            {
                runsim = (bool)arg;
                WorldEntity startstop = Shell.GetEntityByName("IMEMBOTS_STARTSTOP_BUTTON");
                if(startstop != null && startstop is Checkbox)
                {
                    ((Checkbox)startstop).ForceState(runsim);
                }
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
            SimulationModelRunning = runsim;
            WorldEntity running = Shell.GetEntityByName("IMEMBOTS_RUNNING_STATUS");
            if (running != null && running is TextEntity)
            {
                ((TextEntity)running).Text = "[F:SYSFONT]Sim control [F:SYSFONT," + (SimulationModelRunning ? "C:0-255-0-255]active" : "C:255-0-0-255]paused") + "[F:SYSFONT].";
            }
        }
        public static void SendUpdateSimcodeSocCommand(object arg)
        {
            int simcode = 0;
            if (arg is MonitoringTextInputField)
            {
                try
                {
                    simcode = Int32.Parse(((MonitoringTextInputField)arg).LastSentText);
                }
                catch(Exception e)
                {
                    return;
                }
            }
            else if (arg is int)
            {
                simcode = (int)arg;
            }
            Double[] codes = new double[] { 1, (double)simcode };
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d);
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false);
            WorldEntity simid = Shell.GetEntityByName("IMEMBOTS_SIMID_STATUS");
            if (simid != null && simid is TextEntity)
            {
                ((TextEntity)simid).Text = "Sim ID: [C:0-255-0-255]" + simcode;
            }
        }
        public static void SendSaveWeightsToSocCommand()
        {
            Shell.WriteLine("Sending weight save");
            Double[] codes = new double[] { 2 };
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d);
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1);
        }
        public static void SendUpdateFitnessToSocCommand()
        {
            double avr = 0;
            double peak = Double.MinValue;
            for (int i = 0; i < Bot.Bots.Count; i++)
            {
                Bot.Bots[i].SendSystemCodes(new double[] { Double.MaxValue, 1, Bot.Bots[i].Fitness });
                avr += Bot.Bots[i].Fitness;
                if (Bot.Bots[i].Fitness > peak) { peak = Bot.Bots[i].Fitness; }
            }
            avr = avr / Bot.Bots.Count;
            Shell.WriteLine("Fitness update dispatched to Bot controllers. Current average fitness is " + avr + ". Current peak fitness is " + peak + ".");
            WorldEntity fitness = Shell.GetEntityByName("IMEMBOTS_FITNESS_STATUS");
            if(fitness != null && fitness is TextEntity)
            {
                ((TextEntity)fitness).Text = "[F:SYSFONT]Fitness :: Avr.: [F:SYSFONT,C:255-255-0-255]" + Math.Round(avr) + "[F:SYSFONT],[F:SYSFONT,N,L:99-0]Peak: [F:SYSFONT,C:255-255-0-255]" + Math.Round(peak);
            }
        }
        static int s_queuedGeneration = 1;
        static int s_currentGeneration = 1;
        public static void SendDoBreedSocCommand()
        {
            Double[] codes = new double[] { 5, MutationChance };
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d);
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false);
            s_queuedGeneration++;
        }
        public static void SendApplyGenerationSocCommand()
        {
            Double[] codes = new double[] { 6 };
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d);
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false);
            s_currentGeneration = s_queuedGeneration;
            WorldEntity generation = Shell.GetEntityByName("IMEMBOTS_GENERATION_STATUS");
            if (generation != null && generation is TextEntity)
            {
                ((TextEntity)generation).Text = "[F:SYSFONT]Generation: [F:SYSFONT,C:0-255-0-255]" + s_currentGeneration;
            }
        }
        public static void SetWeightLoadTargetFromController(MonitoringTextInputField textInputReceiver)
        {
            try
            {
                s_currentWeightLoadTarget = Int32.Parse(textInputReceiver.LastSentText);
            }
            catch { }
        }
        private static int s_currentWeightLoadTarget = 1;
        public static int CurrentWeightLoadTarget
        {
            get
            {
                return s_currentWeightLoadTarget;
            }
        }
        public static void SendLoadWeightsToSocCommand()
        {
            Double[] codes = new double[] { 3, CurrentWeightLoadTarget };
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d);
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1);
        }
        public static double[] GetLastSystemSocketReturn()
        {
            PySocketQuery thisReceivedQuery = GetQuery(SystemSocketID);
            double[] codes = new double[0];
            if (thisReceivedQuery.LastReceive)
            {
                codes = new double[128];
                for (int i = 0; i < 128; i++)
                {
                    codes[i] = BitConverter.ToDouble(thisReceivedQuery.Receive.AsSpan(i * 8, 8).ToArray());
                }
            }
            return codes;
        }
        public static void ResetBotPositions()
        {
            for(int i = 0; i < Bot.Bots.Count; i++)
            {
                Vector2 pos = Bot.GetStartPosByIndex(i + 1);
                Bot.Bots[i].QuickMoveTo(pos);
                Bot.Bots[i].Rotate((float)GraphicsTools.VectorToBearing(pos) - Bot.Bots[i].RotationRads);
                Bot.Bots[i].Halt();
                Bot.Bots[i].FitnessEvaluator.Initialize(Bot.Bots[i]);
            }
            foreach(WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if(!(worldEntity is Bot) && worldEntity is DynamicEntity)
                {
                    ((DynamicEntity)worldEntity).Halt(false);
                }
            }
            //Shell.AutoCamera.RecenterCamera();
        }
        public static void ApplyFitnessModel(object arg)
        {
            Type fitnessEvalType = null;
            if(arg is IFitnessEvaluator) { fitnessEvalType = arg.GetType(); }
            else if (arg is String) { fitnessEvalType = Type.GetType("VNFramework.IterativeMemBoTs+" + (String)arg); }
            else if (arg is DropMenu) { fitnessEvalType = Type.GetType("VNFramework.IterativeMemBoTs+" + ((DropMenu)arg).OutputText); }
            Shell.WriteLine("Bot fitness evaluators changed to type: " + fitnessEvalType.FullName);
            foreach(Bot bot in Bot.Bots)
            {
                bot.FitnessEvaluator = (IFitnessEvaluator)Activator.CreateInstance(fitnessEvalType);
            }
        }
        public static void SetToggleAutoTrainer(object arg)
        {
            Boolean autotrain = false;
            if (arg is Checkbox)
            {
                autotrain = ((Checkbox)arg).Toggle;
            }
            else if (arg is bool)
            {
                autotrain = (bool)arg;
            }
            AutoTrainerRunning = autotrain;
            WorldEntity trainingStatus = Shell.GetEntityByName("IMEMBOTS_AUTOTRAIN_STATUS");
            if (trainingStatus != null && trainingStatus is TextEntity)
            {
                ((TextEntity)trainingStatus).Text = "[F:SYSFONT]Autotrainer: [F:SYSFONT," + (AutoTrainerRunning ? "C:0-255-0-255]On" : "C:255-0-0-255]Off");
            }
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
            if (Shell.GetEntityByName("IMEMBOTS_UPPERUIBOX") == null)
            {
                WorldEntity uiBox2 = new WorldEntity("IMEMBOTS_UPPERUIBOX", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_UPPERLEFT"], 0.9f);
                uiBox2.IsUIElement = true;
                Shell.UpdateQueue.Add(uiBox2);
                Shell.RenderQueue.Add(uiBox2);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_STATUS") == null)
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_SIMID_STATUS", "Sim ID: [C:0-255-0-255]1", new Vector2(10, 10), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_GENERATION_STATUS") == null)
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_GENERATION_STATUS", "[F:SYSFONT]Generation: [F:SYSFONT,C:0-255-0-255]1", new Vector2(10, 40), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_RUNNING_STATUS") == null)
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_RUNNING_STATUS", "[F:SYSFONT]Sim control [F:SYSFONT,C:255-0-0-255]paused[F:SYSFONT].", new Vector2(10, 60), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_AUTOTRAIN_STATUS") == null)
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_AUTOTRAIN_STATUS", "[F:SYSFONT]Autotrainer: [F:SYSFONT,C:255-0-0-255]Off", new Vector2(10, 80), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_FITNESS_STATUS") == null)
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_FITNESS_STATUS", "[F:SYSFONT]Fitness :: Avr.: [F:SYSFONT,C:255-255-0-255]0[F:SYSFONT],[F:SYSFONT,N,L:99-0]Peak: [F:SYSFONT,C:255-255-0-255]0", new Vector2(10, 100), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_TITLE_LABEL") == null)
            {
                TextEntity titleLabel = new TextEntity("IMEMBOTS_TITLE_LABEL", "IMemBoTs Neural Simulator", new Vector2(10, 520), 0.95f);
                titleLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(titleLabel);
                Shell.RenderQueue.Add(titleLabel);
            }
            if (Shell.GetEntityByName("BUTTON_PAUSEMENU") == null)
            {
                Button pauseButton = new Button("BUTTON_PAUSEMENU", new Vector2(40, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_BURGERBUTTON"], 0.95f);
                pauseButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Pause"), null);
                pauseButton.IsUIElement = true;
                Shell.UpdateQueue.Add(pauseButton);
                Shell.RenderQueue.Add(pauseButton);
            }
            if (Shell.GetEntityByName("IMEMBOTS_START_LABEL") == null)
            {
                TextEntity startLabel = new TextEntity("IMEMBOTS_START_LABEL", "[F:SYSFONT]Toggle neural controllers:", new Vector2(40, 560), 0.95f);
                startLabel.IsUIElement = true;
                startLabel.BufferLength = 120;
                Shell.UpdateQueue.Add(startLabel);
                Shell.RenderQueue.Add(startLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_STARTSTOP_BUTTON") == null)
            {
                Checkbox startstop = new Checkbox("IMEMBOTS_STARTSTOP_BUTTON", new Vector2(120, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_STARTSTOP"], 0.95f, false);
                startstop.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendSimStartStopSocCommand"), new object[] { startstop });
                startstop.IsUIElement = true;
                Shell.UpdateQueue.Add(startstop);
                Shell.RenderQueue.Add(startstop);
            }
            if (Shell.GetEntityByName("BUTTON_RESET_BOTS") == null)
            {
                TAtlasInfo resetButtonAtlas = new TAtlasInfo();
                resetButtonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Reset positions", new Vector2(150, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                resetButtonAtlas.DivDimensions = new Point(2, 1);
                Button resetBotsButton = new Button("BUTTON_RESET_BOTS", new Vector2(180, 660), resetButtonAtlas, 0.95f);
                resetBotsButton.CenterOrigin = false;
                resetBotsButton.IsUIElement = true;
                resetBotsButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("ResetBotPositions"), null);
                Shell.UpdateQueue.Add(resetBotsButton);
                Shell.RenderQueue.Add(resetBotsButton);
            }
            if (Shell.GetEntityByName("DROPMENU_FITNESS_MODEL") == null)
            {
                DropMenu fitnessModelMenu = new DropMenu("DROPMENU_FITNESS_MODEL", new Vector2(630, 580), 0.95f, new Vector2(200, 20), new Vector2(7, 7), 2, new Color[] { new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255), new Color(200, 140, 0, 255), new Color(255, 207, 0, 255) }, "MoveForwardAndOut", "[F:SYSFONT]", FitnessTypes, false);
                fitnessModelMenu.CenterOrigin = false;
                fitnessModelMenu.IsUIElement = true;
                fitnessModelMenu.SubscribeToEvent(WorldEntity.EventNames.DropMenuSelectFunction, typeof(IterativeMemBoTs).GetMethod("ApplyFitnessModel"), new object[] { fitnessModelMenu });
                Shell.UpdateQueue.Add(fitnessModelMenu);
                Shell.RenderQueue.Add(fitnessModelMenu);
            }
            if (Shell.GetEntityByName("IMEMBOTS_FITNESS_LABEL") == null)
            {
                TextEntity fitnessLabel = new TextEntity("IMEMBOTS_FITNESS_LABEL", "[F:SYSFONT]^ Fitness model ^", new Vector2(655, 615), 0.92f);
                fitnessLabel.IsUIElement = true;
                fitnessLabel.BufferLength = 240;
                Shell.UpdateQueue.Add(fitnessLabel);
                Shell.RenderQueue.Add(fitnessLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_AUTOTRAIN_LABEL") == null)
            {
                TextEntity autoTrainLabel = new TextEntity("IMEMBOTS_AUTOTRAIN_LABEL", "[F:SYSFONT]Toggle autotrain:", new Vector2(535, 610), 0.95f);
                autoTrainLabel.IsUIElement = true;
                autoTrainLabel.BufferLength = 100;
                Shell.UpdateQueue.Add(autoTrainLabel);
                Shell.RenderQueue.Add(autoTrainLabel);
            }
            if (Shell.GetEntityByName("BUTTON_TOGGLE_AUTOTRAIN") == null)
            {
                Checkbox autotrainToggle = new Checkbox("BUTTON_TOGGLE_AUTOTRAIN", new Vector2(550, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_AUTOTRAIN_CHECKBOX"], 0.95f, false);
                autotrainToggle.CenterOrigin = false;
                autotrainToggle.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SetToggleAutoTrainer"), new object[] { autotrainToggle });
                autotrainToggle.IsUIElement = true;
                Shell.UpdateQueue.Add(autotrainToggle);
                Shell.RenderQueue.Add(autotrainToggle);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_FIELD_LABEL") == null)
            {
                TextEntity simFieldLabel = new TextEntity("IMEMBOTS_SIMID_FIELD_LABEL", "[F:SYSFONT]Current sim ID (must be +int):", new Vector2(185, 560), 0.95f);
                simFieldLabel.IsUIElement = true;
                simFieldLabel.BufferLength = 150;
                Shell.UpdateQueue.Add(simFieldLabel);
                Shell.RenderQueue.Add(simFieldLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_FIELD") == null)
            {
                ToggleableTextInputField toggleTextInput = new ToggleableTextInputField("IMEMBOTS_SIMID_FIELD", "1", new Vector2(185, 605), 0.95f);
                toggleTextInput.AssignTextureAtlas((TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_HIGHLIGHTTOGGLE"]);
                toggleTextInput.Scale(new Vector2(100 / 30, -0.25f));
                toggleTextInput.IsUIElement = true;
                toggleTextInput.BufferLength = 120;
                toggleTextInput.SubscribeToEvent(WorldEntity.EventNames.TextEnteredFunction, typeof(IterativeMemBoTs).GetMethod("SendUpdateSimcodeSocCommand"), new object[] { toggleTextInput });
                Shell.UpdateQueue.Add(toggleTextInput);
                Shell.RenderQueue.Add(toggleTextInput);
            }
            if (Shell.GetEntityByName("BUTTON_UPDATE_BOT_FITNESS") == null)
            {
                TAtlasInfo buttonAtlas = new TAtlasInfo();
                buttonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Measure fitness", new Vector2(150, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                buttonAtlas.DivDimensions = new Point(2, 1);
                Button button = new Button("BUTTON_UPDATE_BOT_FITNESS", new Vector2(360, 580), buttonAtlas, 0.95f);
                button.CenterOrigin = false;
                button.IsUIElement = true;
                button.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendUpdateFitnessToSocCommand"), null);
                Shell.UpdateQueue.Add(button);
                Shell.RenderQueue.Add(button);
            }
            if (Shell.GetEntityByName("BUTTON_BREED_NEW_GENERATION") == null)
            {
                TAtlasInfo buttonAtlas = new TAtlasInfo();
                buttonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Breed next gen", new Vector2(150, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                buttonAtlas.DivDimensions = new Point(2, 1);
                Button button = new Button("BUTTON_BREED_NEW_GENERATION", new Vector2(360, 620), buttonAtlas, 0.95f);
                button.CenterOrigin = false;
                button.IsUIElement = true;
                button.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendDoBreedSocCommand"), null);
                Shell.UpdateQueue.Add(button);
                Shell.RenderQueue.Add(button);
            }
            if (Shell.GetEntityByName("BUTTON_APPLY_NEXT_GENERATION") == null)
            {
                TAtlasInfo buttonAtlas = new TAtlasInfo();
                buttonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Apply next gen", new Vector2(150, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                buttonAtlas.DivDimensions = new Point(2, 1);
                Button button = new Button("BUTTON_APPLY_NEXT_GENERATION", new Vector2(360, 660), buttonAtlas, 0.95f);
                button.CenterOrigin = false;
                button.IsUIElement = true;
                button.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendApplyGenerationSocCommand"), null);
                Shell.UpdateQueue.Add(button);
                Shell.RenderQueue.Add(button);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_LOAD_FIELD_LABEL") == null)
            {
                TextEntity simFieldLabel = new TextEntity("IMEMBOTS_SIMID_LOAD_FIELD_LABEL", "[F:SYSFONT]Weights are saved to current sim ID and loaded from specified sim ID.", new Vector2(860, 580), 0.95f);
                simFieldLabel.IsUIElement = true;
                simFieldLabel.BufferLength = 320;
                Shell.UpdateQueue.Add(simFieldLabel);
                Shell.RenderQueue.Add(simFieldLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_LOAD_FIELD") == null)
            {
                ToggleableTextInputField toggleTextInput = new ToggleableTextInputField("IMEMBOTS_SIMID_LOAD_FIELD", "1", new Vector2(1080, 625), 0.95f);
                toggleTextInput.AssignTextureAtlas((TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_HIGHLIGHTTOGGLE"]);
                toggleTextInput.Scale(new Vector2(50 / 30, -0.25f));
                toggleTextInput.IsUIElement = true;
                toggleTextInput.BufferLength = 50;
                toggleTextInput.SubscribeToEvent(WorldEntity.EventNames.TextEnteredFunction, typeof(IterativeMemBoTs).GetMethod("SetWeightLoadTargetFromController"), new object[] { toggleTextInput });
                Shell.UpdateQueue.Add(toggleTextInput);
                Shell.RenderQueue.Add(toggleTextInput);
            }
            if (Shell.GetEntityByName("BUTTON_LOAD_WEIGHTS") == null)
            {
                TAtlasInfo resetButtonAtlas = new TAtlasInfo();
                resetButtonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Load weights / ID:", new Vector2(200, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                resetButtonAtlas.DivDimensions = new Point(2, 1);
                Button resetBotsButton = new Button("BUTTON_LOAD_WEIGHTS", new Vector2(860, 620), resetButtonAtlas, 0.95f);
                resetBotsButton.CenterOrigin = false;
                resetBotsButton.IsUIElement = true;
                resetBotsButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendLoadWeightsToSocCommand"), null);
                Shell.UpdateQueue.Add(resetBotsButton);
                Shell.RenderQueue.Add(resetBotsButton);
            }
            if (Shell.GetEntityByName("BUTTON_SAVE_WEIGHTS") == null)
            {
                TAtlasInfo resetButtonAtlas = new TAtlasInfo();
                resetButtonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Save weights", new Vector2(200, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                resetButtonAtlas.DivDimensions = new Point(2, 1);
                Button resetBotsButton = new Button("BUTTON_SAVE_WEIGHTS", new Vector2(860, 660), resetButtonAtlas, 0.95f);
                resetBotsButton.CenterOrigin = false;
                resetBotsButton.IsUIElement = true;
                resetBotsButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendSaveWeightsToSocCommand"), null);
                Shell.UpdateQueue.Add(resetBotsButton);
                Shell.RenderQueue.Add(resetBotsButton);
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
            foreach(WorldEntity worldEntity in Shell.RenderQueue)
            {
                worldEntity.Drawable = false;
            }
            ButtonScripts.SpoonsTrip = true;
            //Shell.ConsoleWritesOverlay = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
            InitMembotsUI();
            AutoTrainerRunning = false;
            s_queuedGeneration = 1;
            s_currentGeneration = 1;
            int totalWall = 0;
            for (int i = 0; i < 16; i++)
            {
                Vector2 pos = new Vector2((float)Math.Sin(totalWall * (Math.PI / 8)), (float)Math.Cos(totalWall * (Math.PI / 8))) * 700f;
                Wall wall = new Wall("WALL_" + totalWall, pos, 0.8f);
                wall.Rotate((float)(GraphicsTools.VectorToBearing(pos) + Math.PI/2));
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
                Vector2 pos = Bot.GetStartPosByIndex(total);
                Bot bot = new Bot("IMEMBOT_" + total, pos, 0.4f + (0.001f * total), Shell.Rnd.Next(1, 1), new Vector2());
                bot.CenterOrigin = true;
                bot.Rotate((float)GraphicsTools.VectorToBearing(pos) - bot.RotationRads);
                bot.MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour());
                bot.MyBehaviours.Add(new Behaviours.ConsoleReaderBehaviour());
                bot.FitnessEvaluator = new MoveForwardAndOutFitness();
                if (total == 1)
                {
                    bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour());
                    bot.AutoRotateToVelocityBearing = false;
                    //bot.MyStickers.Add(Shell.AutoCamera);
                    Shell.AutoCamera.QuickMoveTo(bot.Position);
                }
                Shell.UpdateQueue.Add(bot);
                Shell.RenderQueue.Add(bot);
            }
            Shell.AutoCamera.AutoSnapToOnResetEntityName = "IMEMBOT_1";
            MutationChance = 0.05;
            Shell.UpdateQueue.Add(new AutoTrainController(6000));
        }
        [Serializable]
        public class Bot : DynamicEntity
        {
            public static List<Bot> Bots = new List<Bot>();
            public static Vector2 GetStartPosByIndex(int index)
            {
                return new Vector2((float)Math.Sin(index * (Math.PI / 4)), (float)Math.Cos(index * (Math.PI / 4))) * (float)(200 * Math.Ceiling((index - 1) / 8d));
            }
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
                Bots.Add(this);
                SocketID = AddNewSocketAsTask();
                int subtractColour = (int)((250 / 9) * (mass - 1));
                ColourValue = new Color(255, 255 - subtractColour, 255 - subtractColour);
                Collider = new RadialCollider(50, new Vector2());
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true));
                Velocity = initialVelocity;
                AutoRotateToVelocityBearing = false;
                SelfMovementForceMultiplier = 4;
                SelfRotationRate = 1;

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
            public void SendSystemCodes(double[] codesToSend)
            {
                _lastSystemCodes = codesToSend;
            }
            double[] _lastSystemCodes = null;
            public static readonly int SenseFrequency = 4;
            int _senseClock = Shell.Rnd.Next(0, SenseFrequency);
            private IFitnessEvaluator _fitnessEvaluator = null;
            public IFitnessEvaluator FitnessEvaluator
            {
                get
                {
                    return _fitnessEvaluator;
                }
                set
                {
                    _fitnessEvaluator = value;
                    _fitnessEvaluator.Initialize(this);
                }
            }
            private Double _fitness = 0;
            public Double Fitness
            {
                get { return _fitness; }
            }
            public Boolean AwaitingSystemReturn
            {
                get
                {
                    return _lastSystemCodes != null;
                }
            }
            int _lastSystemReturnTime = Int32.MinValue;
            public int? LastSystemReturnTime
            {
                get
                {
                    return _lastSystemReturnTime;
                }
            }
            int _lastControlReceive = 0;
            public override void Update()
            {
                if (AutoRotateToVelocityBearing) { RotationRads = (float)new VNFramework.GraphicsTools.Trace(Velocity).Bearing; }

                _senseClock = (_senseClock + 1) % SenseFrequency;

                if (_senseClock == 0) { _senseCodes = SenseEnvironment(); }

                if (_lastSystemCodes == null)
                {
                    DispatchCodesToSocket(_senseCodes);
                    double[] receive = GetCodesFromSocket();
                    if (receive.Length > 0 && receive[0] != Double.MaxValue)
                    {
                        _controlCodes = receive;
                        int now = Environment.TickCount;
                        //Shell.WriteLine(Name + " control code receive latency: " + (now - _lastControlReceive));
                        _lastControlReceive = now;
                    }
                }
                else
                {
                    DispatchCodesToSocket(_lastSystemCodes);
                    double[] receive = GetCodesFromSocket();
                    if (receive.Length > 0 && receive[0] == Double.MaxValue)
                    {
                        _lastSystemCodes = null;
                        _lastSystemReturnTime = Environment.TickCount;
                    }
                }

                if (_controlCodes[0] == 1)
                {
                    ApplyForce(ForwardTrace.AsAlignedVector * (float)(_controlCodes[1] - 0.5) * SelfMovementForceMultiplier);
                    Rotate((float)(_controlCodes[2] - 0.5) * SelfRotationRate);
                }

                if(FitnessEvaluator != null)
                {
                    _fitness = FitnessEvaluator.Evaluate(this);
                }
                base.Update();
            }
            private Boolean _socketClosed = false;
            public override void ManualDispose()
            {
                if(Bots.Contains(this)) { Bots.Remove(this); }
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
