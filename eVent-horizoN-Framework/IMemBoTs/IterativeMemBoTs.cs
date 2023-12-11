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
    /// <summary>
    /// Defines the classes and functionality for the IMemBoTs simulation application.
    /// </summary>
    public static class IterativeMemBoTs
    {
        public static ulong SystemSocketID { get; set; } //ID of the system socket
        public static bool SystemSocketAssigned { get; set; } //Boolean value for if the socket has been assigned
        public static readonly String[] FitnessTypes = new string[] { "MoveForwardAndOut", "SurviveEnergize" }; //List of fitness evaluator types
        /// <summary>
        /// Fitness evaluator interface defines the API for querying bot fitness
        /// </summary>
        public interface IFitnessEvaluator
        {
            public void Initialize(Bot thisBot); //Set initial state
            public double Evaluate(Bot thisBot); //Update fitness per world interactions
            public void Reset(); //Reset fitness evaluator to base state
        }
        /// <summary>
        /// MoveForwardAndOut rewards moving forwards and away from bot spawn location.
        /// </summary>
        public class MoveForwardAndOut : IFitnessEvaluator
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
                    if (thisBot.Velocity.Length() > 0) //Reward forward movement
                    {
                        Double angleDifference = GraphicsTools.AngleDifference(thisBot.ForwardTrace.Bearing, GraphicsTools.VectorToBearing(thisBot.Velocity));
                        _extentMovingForwards += (Math.PI / 2) - Math.Abs(angleDifference);
                    }
                    else
                    {
                        _extentMovingForwards -= 0.1;
                    }
                    double distanceTravelled = (_startPosition - thisBot.Position).Length(); //Reward total travel distance
                    fitness = (distanceTravelled * 5) + (_extentMovingForwards); //Weight contributions
                }
                return fitness;
            }
            public void Reset()
            {
                _extentMovingForwards = 0;
                _startPosition = new Vector2();
            }
        }
        /// <summary>
        /// SurviveEnergize rewards survival in the artificial life context
        /// </summary>
        public class SurviveEnergize : IFitnessEvaluator
        {
            int _startTime;
            int _lastTime = -1;
            int _maxtime;
            public void Initialize(Bot thisBot)
            {
                _maxtime = (int)AutoTrainController.GenerationLength;
                _fitness = 0;
                _lastTime = -1;
            }
            double _fitness = 0;
            public double Evaluate(Bot thisBot)
            {               
                if (SimulationModelRunning && !thisBot.Dead) //Fitness is awarded in proportion to energy (1 - hunger) while the bot is alive
                {
                    int time = Environment.TickCount;
                    if (_lastTime == -1) { _lastTime = time; }
                    int timePassed = time - _lastTime;
                    _fitness += ((double)timePassed / (double)_maxtime) * (1d - thisBot.Hunger) * 50000d;
                    _lastTime = time;
                }
                return _fitness - (thisBot.Spiked ? 50000d : 0); //Encoutering a spike imparts a penalty.
            }
            public void Reset()
            {
                //_startTime = Environment.TickCount;
                _maxtime = (int)AutoTrainController.GenerationLength;
                _fitness = 0;
                _lastTime = -1;
            }
        }
        /// <summary>
        /// AutoTrainController automatically controls the training loop
        /// </summary>
        public class AutoTrainController : WorldEntity
        {
            static Boolean s_extant = false;
            Boolean _thisExtant = false;
            public static uint GenerationLength { get; set; } //Time of one generation
            private object[] _resetParams = null;
            public AutoTrainController(uint generationLengthMillis, object[] resetParams) : base("IMEMBOTS_AUTOTRAIN_CONTROLLER", new Vector2(), null, 1)
            {
                //Only one AutoTrainController should be loaded at a time.
                if (s_extant)
                {
                    Shell.DeleteQueue.Add(this);
                }
                else
                {
                    _resetParams = resetParams;
                    GenerationLength = generationLengthMillis;
                    s_extant = true;
                    _thisExtant = true;
                }
            }
            public override void ManualDispose() //Disposal method
            {
                if (_thisExtant) //If this was the canonical auto train controller then it resets the flags
                {
                    s_extant = false;
                    AutoTrainerRunning = false;
                }
                base.ManualDispose();
            }
            Boolean _selfActive = false;
            int _lastGenerationStartTime = Environment.TickCount;
            int _reachedEnd = 0;
            /// <summary>
            /// Method to set generation start parameters
            /// </summary>
            void InitGeneration()
            {
                ResetBotPositions((bool)_resetParams[0], (int)_resetParams[1], (int)_resetParams[2]); //Reset bot parameters as required
                _lastGenerationStartTime = Environment.TickCount;
                SendSimStartStopSocCommand(true); //Instruct the python script to commence
            }
            int _timeOfBotSystemCommandDispatch = 0;
            int _timeOfGenerationApply = 0;
            /// <summary>
            /// Overload of the update function to perform live monitoring
            /// </summary>
            public override void Update()
            {
                base.Update();
                int thisTickCount = Environment.TickCount;
                if (_thisExtant) //Only run if this is the canonical controller
                {
                    if (AutoTrainerRunning != _selfActive) //Set self state to match the public property
                    {
                        if (!_selfActive)
                        {
                            InitGeneration(); //Start generation
                            _selfActive = true;
                        }
                        else
                        {
                            SendSimStartStopSocCommand(false); //Halt simulation
                            _selfActive = false;
                        }
                    }
                    if (_selfActive) //If the controller is active
                    {
                        Boolean allDead = true;
                        foreach (Bot bot in Bot.Bots) //Check that all bots are not dead
                        {
                            if(!bot.Dead)
                            { 
                                allDead = false;
                                break;
                            }
                        }
                        if (thisTickCount > _lastGenerationStartTime + GenerationLength || allDead) //If bots are dead or the generation time is exceeded, perform generation advance steps.
                        {
                            double[] lastSystemCodes = GetLastSystemSocketReturn();
                            if (SimulationModelRunning) { SendSimStartStopSocCommand(false); } //Stop simulation
                            if (_reachedEnd == 0) //Step 0
                            {
                                foreach (Bot bot in Bot.Bots) //Remove velocity from bots
                                {
                                    bot.Halt();
                                }
                                _timeOfBotSystemCommandDispatch = Environment.TickCount;
                                SendUpdateFitnessToSocCommand(); //Update fitnesses
                                _reachedEnd = 1; //Advance to next step
                            }
                            else if(_reachedEnd == 1) //Step 1
                            {
                                Boolean returned = true;
                                foreach(Bot bot in Bot.Bots)
                                {
                                    if(bot.LastSystemReturnTime < _timeOfBotSystemCommandDispatch) { returned = false; } //Ensure that each bot had its fitness updated
                                }
                                if (returned)
                                {
                                    SendDoBreedSocCommand(); //Perform breed operation
                                    _reachedEnd = 2; //Advance to next step
                                }
                            }
                            if (lastSystemCodes.Length > 0) //If system code return from breed/advance operation
                            {
                                if (lastSystemCodes[0] == 5d && _reachedEnd == 2) //Step 2
                                {
                                    SendApplyGenerationSocCommand(); //Apply next generation controllers to socket handlers
                                    _reachedEnd = 3;
                                    _timeOfGenerationApply = thisTickCount;
                                }
                                else if (lastSystemCodes[0] == 6d && thisTickCount > _timeOfGenerationApply + 1000 && _reachedEnd == 3) //Step 3
                                {
                                    _reachedEnd = 0;
                                    InitGeneration(); //Start next generation
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Initialize the "physics test" environment.
        /// This initialization protocal was for testing the initial physics implementation and was not used in experimentation.
        /// </summary>
        /// <param name="layout">Spawn grid dimensions for physics objects</param>
        public static void InitializePhysTest(Point layout)
        {
            //Prepare environment
            ButtonScripts.SpoonsTrip = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
            int totalWall = 0;
            //Spawn bounding walls
            for (int i = 0; i < 5; i++) //Horizontal
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
            for (int i = 0; i < 5; i++) //Vertical
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
            //Spawn IMemBoTs for physics test
            for (int y = 0; y < layout.Y; y++)
            {
                for (int x = 0; x < layout.X; x++)
                {
                    total++;
                    Bot bot = new Bot("IMEMBOT_" + x + "_" + y, new Vector2(175 + 120 * (x + 1), 175 + 120 * (y + 1)), 0.4f + (0.001f * total), Shell.Rnd.Next(1, 11), new Vector2(Shell.Rnd.Next(-5, 5), Shell.Rnd.Next(-5, 5)));
                    bot.CenterOrigin = true;
                    bot.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                    if (total == 1) //One IMemBoT should be user controllable
                    {
                        bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour());
                        bot.AutoRotateToVelocityBearing = false;
                        bot.MyStickers.Add(Shell.AutoCamera); //Camera should follow the bot
                        Shell.AutoCamera.QuickMoveTo(bot.Position); //Camera should init at the controllable bot
                    }
                    Shell.UpdateQueue.Add(bot);
                    Shell.RenderQueue.Add(bot);
                }
            }
        }
        /// <summary>
        /// Initialize the "physics test" environment.
        /// This initialization protocal was for testing the initial physics implementation and was not used in experimentation.
        /// </summary>
        /// <param name="layout">Spawn grid dimensions for physics objects</param>
        public static void InitializePhysTest2(Point layout)
        {
            //Prepare environment
            ButtonScripts.SpoonsTrip = true;
            Shell.OneFadeout();
            ScriptProcessor.ClearNonUIEntities();
            Shell.LooseCamera = true;
            ScriptProcessor.AssertGameRunningWithoutScript = true;
            Shell.BackdropColour = Color.Aquamarine;
            //Spawn bounding walls
            Wall wall = new Wall("WALL_1", new Vector2(-5000, 0), 0.8f);
            wall.CenterOrigin = true;
            wall.Scale(new Vector2(2, 66));
            Shell.UpdateQueue.Add(wall);
            Shell.RenderQueue.Add(wall);
            wall = new Wall("WALL_2", new Vector2(5000, 0), 0.8f);
            wall.CenterOrigin = true;
            wall.Scale(new Vector2(2, 66));
            Shell.UpdateQueue.Add(wall);
            Shell.RenderQueue.Add(wall);
            wall = new Wall("WALL_3", new Vector2(0, 5000), 0.8f);
            wall.CenterOrigin = true;
            wall.Scale(new Vector2(2, 66));
            wall.Rotate((float)(Math.PI / 2));
            Shell.UpdateQueue.Add(wall);
            Shell.RenderQueue.Add(wall);
            int total = 0;
            //Spawn IMemBoTs for physics test
            for (int y = 0; y < layout.Y; y++)
            {
                for (int x = 0; x < layout.X; x++)
                {
                    total++;
                    Plant plant = new Plant("PLANT" + x + "_" + y, new Vector2(-2400 + 200 * (x + 1), -2400 + 200 * (y + 1)), 0.4f + (0.001f * total), 10, new Vector2(Shell.Rnd.Next(-5, 5), Shell.Rnd.Next(-5, 5)));
                    plant.CenterOrigin = true;
                    Shell.UpdateQueue.Add(plant);
                    Shell.RenderQueue.Add(plant);
                }
            }
            DynamicEntity.GlobalGravity = 2;
        }
        //Public static properties to control IMemBoT simulation parameters.
        public static Boolean SimulationModelRunning { get; private set; } //Is the simulation running?
        public static Boolean AutoTrainerRunning { get; private set; } //Is the autotrainer running?
        public static Double MutationAddedWeight { get; private set; } //Value for the additional mutation weighting
        public static Double ParamStandardUncertainty { get; private set; } //Value for parameter uncertainty.
        static List<double> s_generationFitness = new List<double>();
        /// <summary>
        /// List of average fitness for each generation.
        /// </summary>
        public static List<double> GenerationFitness
        {
            get
            {
                return s_generationFitness;
            }
            private set
            {
                s_generationFitness = value;
            }
        }
        /// <summary>
        /// Command to dispatch start/stop command for the simulation over the socket bridge
        /// </summary>
        /// <param name="arg">Boolean parameter or UI object sending the command</param>
        public static void SendSimStartStopSocCommand(object arg)
        {
            Boolean runsim = false;
            if(arg is Checkbox) //Derive from UI Checkbox
            {
                runsim = ((Checkbox)arg).Toggle;
            }
            else if(arg is bool) //Derive from Boolean
            {
                runsim = (bool)arg;
                WorldEntity startstop = Shell.GetEntityByName("IMEMBOTS_STARTSTOP_BUTTON");
                if(startstop != null && startstop is Checkbox)
                {
                    ((Checkbox)startstop).ForceState(runsim); //Update UI toggle to match boolean toggle
                }
            }
            Double[] codes = new double[] { 4, runsim ? 1 : 0 }; //Set system codes for dispatch
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d); //Convert to bytes
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1); //Send codes over socket bridge via system socket.
            SimulationModelRunning = runsim;
            WorldEntity running = Shell.GetEntityByName("IMEMBOTS_RUNNING_STATUS");
            if (running != null && running is TextEntity) //Update "simulation running" text if applicable.
            {
                ((TextEntity)running).Text = "[F:SYSFONT]Sim control [F:SYSFONT," + (SimulationModelRunning ? "C:0-255-0-255]active" : "C:255-0-0-255]paused") + "[F:SYSFONT].";
            }
        }
        static int s_assertedSimCode = 1;
        /// <summary>
        /// Sim code for the current simulation
        /// </summary>
        public static int AssertedSimCode
        {
            get
            {
                return s_assertedSimCode;
            }
        }
        /// <summary>
        /// Command to update the sim code on the python side
        /// </summary>
        /// <param name="arg">Update value, either set directly as an int or derived from a text input UI object</param>
        public static void SendUpdateSimcodeSocCommand(object arg)
        {
            int simcode = 0;
            if (arg is MonitoringTextInputField) //If value is via a text field
            {
                try
                {
                    simcode = Int32.Parse(((MonitoringTextInputField)arg).LastSentText); //Parse integer from text
                }
                catch(Exception e)
                {
                    return;
                }
            }
            else if (arg is int) //If value is an int 
            {
                simcode = (int)arg; //Use value directly
            }
            Double[] codes = new double[] { 1, (double)simcode }; //Set codes for dispatch
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d); //Convert to bytes
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false); //Send over socket bridge via system socket
            s_assertedSimCode = simcode;
            WorldEntity simid = Shell.GetEntityByName("IMEMBOTS_SIMID_STATUS");
            if (simid != null && simid is TextEntity)
            {
                ((TextEntity)simid).Text = "Sim ID: [C:0-255-0-255]" + simcode; //Update UI text if applicable
            }
        }
        /// <summary>
        /// Dispatch socket command to save weights for the current simulation controllers
        /// </summary>
        public static void SendSaveWeightsToSocCommand()
        {
            Shell.WriteLine("Sending weight save");
            Double[] codes = new double[] { 2 }; //Set codes for dispatch
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d); //Convert to raw bytes
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1); //Dispatch over system socket
        }
        /// <summary>
        /// Dispatch socket command to update bot fitness
        /// </summary>
        public static void SendUpdateFitnessToSocCommand()
        {
            double avr = 0;
            double peak = Double.MinValue;
            for (int i = 0; i < Bot.Bots.Count; i++) //Fitness updates are set individually for each bot as a handler system command
            {
                Bot.Bots[i].SendSystemCodes(new double[] { Double.MaxValue, 1, Bot.Bots[i].Fitness }); //Dispatch code for each bot reporting fitness
                avr += Bot.Bots[i].Fitness; //Tally each fitness
                if (Bot.Bots[i].Fitness > peak) { peak = Bot.Bots[i].Fitness; } //Update peak fitness for generation
            }
            avr = avr / Bot.Bots.Count; //Determine average fitness
            Shell.WriteLine("Fitness update dispatched to Bot controllers. Current average fitness is " + avr + ". Current peak fitness is " + peak + ".");
            GenerationFitness.Add(avr); //Add average fitness to record
            WorldEntity fitness = Shell.GetEntityByName("IMEMBOTS_FITNESS_STATUS");
            if(fitness != null && fitness is TextEntity) //Update UI with fitness info
            {
                ((TextEntity)fitness).Text = "[F:SYSFONT]Fitness :: Avr.: [F:SYSFONT,C:255-255-0-255]" + Math.Round(avr) + "[F:SYSFONT],[F:SYSFONT,N,L:99-0]Peak: [F:SYSFONT,C:255-255-0-255]" + Math.Round(peak);
            }
        }
        /// <summary>
        /// Method to clear stored fitnesses
        /// </summary>
        public static void ClearStoredFitnesses()
        {
            Shell.WriteLine("Clearing IMemBoTs fitness scores.");
            GenerationFitness?.Clear();
        }
        /// <summary>
        /// Method to write stored average fitnesses for each generation to a .csv file.
        /// </summary>
        public static void WriteStoredFitnesses()
        {
            if (GenerationFitness != null)
            {
                Shell.WriteLine("Writing IMemBoTs fitness scores to CSV file.");
                object[][] csvData = new object[][] { (new int[GenerationFitness.Count]).Select((x, i) => (object)(i + 1)).ToArray(), GenerationFitness.Select(x => (object)x).ToArray() };
                String[] csvHeaders = new string[] { "Generation", "Avr. Fitness" };
                SaveLoadModule.WriteCSV("IMemBoTs\\", "training_fitnesses_simulation_" + AssertedSimCode + "_" + System.DateTime.Now.ToLocalTime().ToString().Replace("\\", "_").Replace("/", "_").Replace(" ", "_").Replace(":", "_").Replace(".", "_") + ".csv", csvData, csvHeaders);
            }
        }
        static int s_queuedGeneration = 1; //Generation that is queued by the genetic algorithm
        static int s_currentGeneration = 1; //Current generation number
        /// <summary>
        /// Method to send command to perform the genetic breeding operation to the python code.
        /// </summary>
        public static void SendDoBreedSocCommand()
        {
            Double[] codes = new double[] { 5, MutationAddedWeight, ParamStandardUncertainty }; //Set code parameters
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d); //Covert to raw bytes
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1, 10); //Send over system socket
            s_queuedGeneration++;
        }
        /// <summary>
        /// Method to send command to apply queued next generation to the active controller handlers, thus advancing the generation to the newly bred child generation
        /// </summary>
        public static void SendApplyGenerationSocCommand()
        {
            Double[] codes = new double[] { 6 }; //Set code for dispatch
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d); //Convert to raw bytes
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1); //Dispatch over socket bridge to Python process
            s_currentGeneration = s_queuedGeneration;
            WorldEntity generation = Shell.GetEntityByName("IMEMBOTS_GENERATION_STATUS");
            if (generation != null && generation is TextEntity)
            {
                ((TextEntity)generation).Text = "[F:SYSFONT]Generation: [F:SYSFONT,C:0-255-0-255]" + s_currentGeneration; //Update UI text
            }
        }
        /// <summary>
        /// Method to set the load target for loading saved weights to UI specified value
        /// </summary>
        /// <param name="textInputReceiver">Text field containing the target Sim ID</param>
        public static void SetWeightLoadTargetFromController(MonitoringTextInputField textInputReceiver)
        {
            try
            {
                s_currentWeightLoadTarget = Int32.Parse(textInputReceiver.LastSentText); //Parse ID int from text field
            }
            catch { }
        }
        private static int s_currentWeightLoadTarget = 1;
        /// <summary>
        /// Current target ID for loading weights from file
        /// </summary>
        public static int CurrentWeightLoadTarget
        {
            get
            {
                return s_currentWeightLoadTarget;
            }
        }
        /// <summary>
        /// Method for dispatching command to load weights from file over socket bridge to the Python process
        /// </summary>
        public static void SendLoadWeightsToSocCommand()
        {
            Double[] codes = new double[] { 3, CurrentWeightLoadTarget }; //Set command parameters with current target ID
            Byte[] data = new byte[1024];
            int startIndex = 0;
            foreach (double d in codes)
            {
                byte[] thisDouble = BitConverter.GetBytes(d); //Convert to raw bytes
                thisDouble.CopyTo(data, startIndex);
                startIndex += 8;
            }
            PythonController.SocketInterface.SendQuery(SystemSocketID, data, false, 1); //Dispatch over system sockets
        }
        /// <summary>
        /// Retrieve the last response from the system socket, being the last return from the paired Python executable.
        /// </summary>
        /// <returns>The retrieved codes sent via socket.</returns>
        public static double[] GetLastSystemSocketReturn()
        {
            PySocketQuery thisReceivedQuery = GetQuery(SystemSocketID); //Get the query
            double[] codes = new double[0];
            if (thisReceivedQuery.LastReceive)
            {
                codes = new double[128];
                for (int i = 0; i < 128; i++)
                {
                    codes[i] = BitConverter.ToDouble(thisReceivedQuery.Receive.AsSpan(i * 8, 8).ToArray()); //Parse the query
                }
            }
            return codes;
        }
        /// <summary>
        /// Reset the position of objects in the standard "artificial life" environment.
        /// </summary>
        /// <param name="shuffle">Boolean value; should random positions be shuffled?</param>
        /// <param name="spawnRadius">Radius of the Plant/Spike "spawn circle"</param>
        /// <param name="circleRandomness">Extent of deviation from the "spawn circle" allowed</param>
        public static void ResetEnvironmentObjects(Boolean shuffle, int spawnRadius, int circleRandomness)
        {
            foreach (WorldEntity worldEntity in Shell.UpdateQueue) //Each WorldEntity is considered.
            {
                if (!(worldEntity is Bot) && worldEntity is DynamicEntity) //Bot positions are not updated here
                {
                    ((DynamicEntity)worldEntity).Halt(false); //Arrest momentum of moving DynamicEntities
                    if (worldEntity is Plant)
                    {
                        ((Plant)worldEntity).Reset(); //Reset Plant positions
                    }
                    if (worldEntity is Spike) { ((Spike)worldEntity).Reset(); } //Reset Spike positions.
                    if (shuffle) //If shuffle is enabled
                    {
                        if(worldEntity is Plant || worldEntity is Spike) //Randomize Plant and Spike positions within parameters
                        {
                            Double circangle = Shell.Rnd.NextDouble() * Math.PI * 2;
                            worldEntity.QuickMoveTo((new Vector2((float)Math.Sin(circangle), (float)Math.Cos(circangle)) * (spawnRadius / 2)) + new Vector2(Shell.Rnd.Next(-circleRandomness, circleRandomness), Shell.Rnd.Next(-circleRandomness, circleRandomness)));
                        }
                        if(worldEntity is Wall) //Randomize wall positions within parameters
                        {
                            worldEntity.QuickMoveTo(new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)));
                            worldEntity.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Method to reset bot positions
        /// </summary>
        /// <param name="shuffleObstacles">Should environment objects be shuffled?</param>
        /// <param name="spawnRadius">Spawn circle radius for environment reset shuffle</param>
        /// <param name="circleRandomness">Spawn circle position randomness for environment reset operation</param>
        public static void ResetBotPositions(Boolean shuffleObstacles, int spawnRadius, int circleRandomness)
        {
            for (int i = 0; i < Bot.Bots.Count; i++) //For each bot...
            {
                Vector2 pos = Bot.GetStartPosByIndex(i + 1); //Retrieve spawn location
                Bot.Bots[i].QuickMoveTo(pos); //Move bot to spawn
                Bot.Bots[i].Rotate((float)GraphicsTools.VectorToBearing(pos) - Bot.Bots[i].RotationRads); //Rotate to bearing pointing out from center
                Bot.Bots[i].Halt(); //Hold bot still
                Bot.Bots[i].Dead = false; //Bot should be alive
                Bot.Bots[i].Spiked = false; //Bot should not be spiked
                Bot.Bots[i].Hunger = 0; //Bot should not be hungry
                Bot.Bots[i].FitnessEvaluator.Initialize(Bot.Bots[i]); //Initialize bot fitness evaluator
            }
            ResetEnvironmentObjects(shuffleObstacles, spawnRadius, circleRandomness); //Reset environments
            //Shell.AutoCamera.RecenterCamera();
        }
        /// <summary>
        /// Method to apply the specified FitnessEvaluator to each bot
        /// </summary>
        /// <param name="arg">Fitness evaluator type, specified as either a type, a string, or a UI element with string output</param>
        public static void ApplyFitnessModel(object arg)
        {
            Type fitnessEvalType = null;
            if(arg is IFitnessEvaluator) { fitnessEvalType = arg.GetType(); } //Set as raw type
            else if (arg is String) { fitnessEvalType = Type.GetType("VNFramework.IterativeMemBoTs+" + (String)arg); } //Extract type from string
            else if (arg is DropMenu) { fitnessEvalType = Type.GetType("VNFramework.IterativeMemBoTs+" + ((DropMenu)arg).OutputText); } //Extract type from UI object
            Shell.WriteLine("Bot fitness evaluators changed to type: " + fitnessEvalType.FullName);
            foreach(Bot bot in Bot.Bots)
            {
                bot.FitnessEvaluator = (IFitnessEvaluator)Activator.CreateInstance(fitnessEvalType); //Create a new evaluator of the correct type for each bot.
            }
        }
        /// <summary>
        /// Method for toggling on/off the AutoTrainer procedure.
        /// </summary>
        /// <param name="arg">Boolean toggle, either as a raw boolean or from a UI toggle</param>
        public static void SetToggleAutoTrainer(object arg)
        {
            Boolean autotrain = false;
            if (arg is Checkbox)
            {
                autotrain = ((Checkbox)arg).Toggle; //Get boolean from UI checkbox toggle
            }
            else if (arg is bool)
            {
                autotrain = (bool)arg; //Set as boolean
            }
            AutoTrainerRunning = autotrain; //Update static property
            WorldEntity trainingStatus = Shell.GetEntityByName("IMEMBOTS_AUTOTRAIN_STATUS");
            if (trainingStatus != null && trainingStatus is TextEntity)
            {
                ((TextEntity)trainingStatus).Text = "[F:SYSFONT]Autotrainer: [F:SYSFONT," + (AutoTrainerRunning ? "C:0-255-0-255]On" : "C:255-0-0-255]Off"); //Update UI text
            }
        }
        /// <summary>
        /// Initialize the "circle escape" simulation environment
        /// </summary>
        /// <param name="spawnRadius">Radius to spawn Plant entities</param>
        static void InitCircleEscapeEnvironment(int spawnRadius)
        {
            int totalWall = 0;
            //Spawn 16 walls in a circle around spawn location
            for (int i = 0; i < 16; i++)
            {
                Vector2 pos = new Vector2((float)Math.Sin(totalWall * (Math.PI / 8)), (float)Math.Cos(totalWall * (Math.PI / 8))) * 700f;
                Wall wall = new Wall("WALL_" + totalWall, pos, 0.8f);
                wall.Rotate((float)(GraphicsTools.VectorToBearing(pos) + Math.PI / 2));
                totalWall++;
                Shell.UpdateQueue.Add(wall);
                Shell.RenderQueue.Add(wall);
            }
            //Spawn Plant entities at random within radius from spawn location
            for (int i = 0; i < 20; i++)
            {
                Plant plant = new Plant("PLANT_" + i, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.6f + i * 0.001f, 300, new Vector2());
                plant.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2));
                Shell.UpdateQueue.Add(plant);
                Shell.RenderQueue.Add(plant);
            }
        }
        /// <summary>
        /// Initialize a random environment for the artificial life simulation
        /// </summary>
        /// <param name="spawnRadius">Entity radius from the spawn coordinate (0,0)</param>
        /// <param name="circleRandomness">Randomness to apply to objects spawned in the "spawn circle"</param>
        /// <param name="numWall">Number of walls to spawn</param>
        /// <param name="numPlant">Number of plants to spawn</param>
        /// <param name="numSpike">Number of spikes to spawn</param>
        static void InitRandEnvironment(int spawnRadius, int circleRandomness, int numWall, int numPlant, int numSpike)
        {
            int totalWall = 0;
            for (int i = 0; i < numWall; i++) //Spawn each wall
            {
                Wall wall = new Wall("WALL_" + totalWall, new Vector2(Shell.Rnd.Next(-spawnRadius, spawnRadius), Shell.Rnd.Next(-spawnRadius, spawnRadius)), 0.8f); //At a random location within the radius
                wall.Rotate((float)(Shell.Rnd.NextDouble() * Math.PI * 2)); //At a random rotation
                totalWall++;
                Shell.UpdateQueue.Add(wall);
                Shell.RenderQueue.Add(wall);
            }
            for (int i = 0; i < numPlant; i++) //Spawn each plant
            {
                Double circangle = Shell.Rnd.NextDouble() * Math.PI * 2; //At a random angle along the "spawn circle"
                //Where the spawn circle radius is half the spawn radius
                //"Circle randomness" makes placement less even
                Plant plant = new Plant("PLANT_" + i, (new Vector2((float)Math.Sin(circangle), (float)Math.Cos(circangle)) * (spawnRadius / 2)) + new Vector2(Shell.Rnd.Next(-circleRandomness, circleRandomness), Shell.Rnd.Next(-circleRandomness, circleRandomness)), 0.6f + i * 0.001f, 300, new Vector2());
                Shell.UpdateQueue.Add(plant);
                Shell.RenderQueue.Add(plant);
            }
            for (int i = 0; i < numSpike; i++) //Spawn each spike
            {
                Double circangle = Shell.Rnd.NextDouble() * Math.PI * 2; //At a random angle along the "spawn circle"
                //Where the spawn circle radius is half the spawn radius
                //"Circle randomness" makes placement less even
                Spike spike = new Spike("SPIKE_" + i, (new Vector2((float)Math.Sin(circangle), (float)Math.Cos(circangle)) * (spawnRadius / 2)) + new Vector2(Shell.Rnd.Next(-circleRandomness, circleRandomness), Shell.Rnd.Next(-circleRandomness, circleRandomness)), 0.7f + i * 0.001f, 300, new Vector2());
                Shell.UpdateQueue.Add(spike);
                Shell.RenderQueue.Add(spike);
            }
        }
        /// <summary>
        /// Set the default UI initializer to apply the "MoveForwardAndOut" fitness
        /// </summary>
        public static void InitMembotsUI()
        {
            InitMembotsUI("MoveForwardAndOut");
        }
        /// <summary>
        /// Initialize the IMemBoTs UI
        /// </summary>
        /// <param name="startingFitness">Initial fitness evaluator to assign bots</param>
        public static void InitMembotsUI(String startingFitness)
        {
            if (Shell.GetEntityByName("IMEMBOTS_UIBOX") == null) //Create UI box
            {
                WorldEntity uiBox = new WorldEntity("IMEMBOTS_UIBOX", new Vector2(0, 502), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_BACKING"], 0.9f);
                uiBox.IsUIElement = true;
                Shell.UpdateQueue.Add(uiBox);
                Shell.RenderQueue.Add(uiBox);
            }
            if (Shell.GetEntityByName("IMEMBOTS_UPPERUIBOX") == null) //Create secondary UI box
            {
                WorldEntity uiBox2 = new WorldEntity("IMEMBOTS_UPPERUIBOX", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_UPPERLEFT"], 0.9f);
                uiBox2.IsUIElement = true;
                Shell.UpdateQueue.Add(uiBox2);
                Shell.RenderQueue.Add(uiBox2);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_STATUS") == null) //Create Sim ID label
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_SIMID_STATUS", "Sim ID: [C:0-255-0-255]1", new Vector2(10, 10), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_GENERATION_STATUS") == null) //Create generation label
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_GENERATION_STATUS", "[F:SYSFONT]Generation: [F:SYSFONT,C:0-255-0-255]1", new Vector2(10, 40), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_RUNNING_STATUS") == null) //Create sim running label
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_RUNNING_STATUS", "[F:SYSFONT]Sim control [F:SYSFONT,C:255-0-0-255]paused[F:SYSFONT].", new Vector2(10, 60), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_AUTOTRAIN_STATUS") == null) //Create AutoTrainer running label
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_AUTOTRAIN_STATUS", "[F:SYSFONT]Autotrainer: [F:SYSFONT,C:255-0-0-255]Off", new Vector2(10, 80), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_FITNESS_STATUS") == null) //Create fitness label
            {
                TextEntity statusLabel = new TextEntity("IMEMBOTS_FITNESS_STATUS", "[F:SYSFONT]Fitness :: Avr.: [F:SYSFONT,C:255-255-0-255]0[F:SYSFONT],[F:SYSFONT,N,L:99-0]Peak: [F:SYSFONT,C:255-255-0-255]0", new Vector2(10, 100), 0.95f);
                statusLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(statusLabel);
                Shell.RenderQueue.Add(statusLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_TITLE_LABEL") == null) //Create title label
            {
                TextEntity titleLabel = new TextEntity("IMEMBOTS_TITLE_LABEL", "IMemBoTs Neural Simulator", new Vector2(10, 520), 0.95f);
                titleLabel.IsUIElement = true;
                Shell.UpdateQueue.Add(titleLabel);
                Shell.RenderQueue.Add(titleLabel);
            }
            if (Shell.GetEntityByName("BUTTON_PAUSEMENU") == null) //Create pause menu button
            {
                Button pauseButton = new Button("BUTTON_PAUSEMENU", new Vector2(40, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_BURGERBUTTON"], 0.95f);
                pauseButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Pause"), null);
                pauseButton.IsUIElement = true;
                Shell.UpdateQueue.Add(pauseButton);
                Shell.RenderQueue.Add(pauseButton);
            }
            if (Shell.GetEntityByName("IMEMBOTS_START_LABEL") == null) //Create start button label
            {
                TextEntity startLabel = new TextEntity("IMEMBOTS_START_LABEL", "[F:SYSFONT]Toggle neural controllers:", new Vector2(40, 560), 0.95f);
                startLabel.IsUIElement = true;
                startLabel.BufferLength = 120;
                Shell.UpdateQueue.Add(startLabel);
                Shell.RenderQueue.Add(startLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_STARTSTOP_BUTTON") == null) //Create start/stop button
            {
                Checkbox startstop = new Checkbox("IMEMBOTS_STARTSTOP_BUTTON", new Vector2(120, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_STARTSTOP"], 0.95f, false);
                startstop.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SendSimStartStopSocCommand"), new object[] { startstop });
                startstop.IsUIElement = true;
                Shell.UpdateQueue.Add(startstop);
                Shell.RenderQueue.Add(startstop);
            }
            if (Shell.GetEntityByName("BUTTON_RESET_BOTS") == null) //Create bot reset button
            {
                TAtlasInfo resetButtonAtlas = new TAtlasInfo();
                resetButtonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Reset positions", new Vector2(150, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                resetButtonAtlas.DivDimensions = new Point(2, 1);
                Button resetBotsButton = new Button("BUTTON_RESET_BOTS", new Vector2(180, 660), resetButtonAtlas, 0.95f);
                resetBotsButton.CenterOrigin = false;
                resetBotsButton.IsUIElement = true;
                resetBotsButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("ResetBotPositions"), new object[] { false, 0, 0 });
                Shell.UpdateQueue.Add(resetBotsButton);
                Shell.RenderQueue.Add(resetBotsButton);
            }
            DropMenu fitnessModelMenu = null;
            if (Shell.GetEntityByName("DROPMENU_FITNESS_MODEL") == null) //Create fitness model dropdown
            {
                fitnessModelMenu = new DropMenu("DROPMENU_FITNESS_MODEL", new Vector2(630, 570), 0.95f, new Vector2(200, 20), new Vector2(7, 7), 2, new Color[] { new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255), new Color(200, 140, 0, 255), new Color(255, 207, 0, 255) }, startingFitness, "[F:SYSFONT]", FitnessTypes, false);
                fitnessModelMenu.CenterOrigin = false;
                fitnessModelMenu.IsUIElement = true;
                fitnessModelMenu.SubscribeToEvent(WorldEntity.EventNames.DropMenuSelectFunction, typeof(IterativeMemBoTs).GetMethod("ApplyFitnessModel"), new object[] { fitnessModelMenu });
                Shell.UpdateQueue.Add(fitnessModelMenu);
                Shell.RenderQueue.Add(fitnessModelMenu);
            }
            if (Shell.GetEntityByName("IMEMBOTS_FITNESS_LABEL") == null) //Create fitness model label
            {
                TextEntity fitnessLabel = new TextEntity("IMEMBOTS_FITNESS_LABEL", "[F:SYSFONT]^ Fitness model ^", new Vector2(655, 605), 0.92f);
                fitnessLabel.IsUIElement = true;
                fitnessLabel.BufferLength = 240;
                Shell.UpdateQueue.Add(fitnessLabel);
                Shell.RenderQueue.Add(fitnessLabel);
            }
            if (Shell.GetEntityByName("BUTTON_RESET_FITNESS_SCORES") == null) //Create reset fitness scores button
            {
                TAtlasInfo resetFitnessButtonAtlas = new TAtlasInfo();
                resetFitnessButtonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Clear fitness history", new Vector2(200, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                resetFitnessButtonAtlas.DivDimensions = new Point(2, 1);
                Button resetFitnessButton = new Button("BUTTON_RESET_FITNESS_SCORES", new Vector2(630, 620), resetFitnessButtonAtlas, 0.91f);
                resetFitnessButton.CenterOrigin = false;
                resetFitnessButton.IsUIElement = true;
                resetFitnessButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("ClearStoredFitnesses"), null);
                if(fitnessModelMenu != null)
                {
                    resetFitnessButton.SubscribeToEvent(fitnessModelMenu, WorldEntity.EventNames.ButtonPressFunction, typeof(Button).GetMethod("AssertObscuringState"), new object[] { fitnessModelMenu });
                }
                Shell.UpdateQueue.Add(resetFitnessButton);
                Shell.RenderQueue.Add(resetFitnessButton);
            }
            if (Shell.GetEntityByName("BUTTON_WRITE_FITNESS_SCORES") == null) //Create button to write average fitness to a .csv
            {
                TAtlasInfo writeFitnessButtonAtlas = new TAtlasInfo();
                writeFitnessButtonAtlas.Atlas = ButtonScripts.CreateCustomButton("[F:SYSFONT]Write fitness CSV file", new Vector2(200, 20), new Vector2(10, 7), new Color(255, 207, 0, 255), new Color(255, 94, 0, 255), new Color(255, 151, 32, 255));
                writeFitnessButtonAtlas.DivDimensions = new Point(2, 1);
                Button writeFitnessButton = new Button("BUTTON_WRITE_FITNESS_SCORES", new Vector2(630, 660), writeFitnessButtonAtlas, 0.91f);
                writeFitnessButton.CenterOrigin = false;
                writeFitnessButton.IsUIElement = true;
                writeFitnessButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("WriteStoredFitnesses"), null);
                if (fitnessModelMenu != null)
                {
                    writeFitnessButton.SubscribeToEvent(fitnessModelMenu, WorldEntity.EventNames.ButtonPressFunction, typeof(Button).GetMethod("AssertObscuringState"), new object[] { fitnessModelMenu });
                }
                Shell.UpdateQueue.Add(writeFitnessButton);
                Shell.RenderQueue.Add(writeFitnessButton);
            }
            if (Shell.GetEntityByName("IMEMBOTS_AUTOTRAIN_LABEL") == null) //Create autotrain button label
            {
                TextEntity autoTrainLabel = new TextEntity("IMEMBOTS_AUTOTRAIN_LABEL", "[F:SYSFONT]Toggle autotrain:", new Vector2(535, 610), 0.95f);
                autoTrainLabel.IsUIElement = true;
                autoTrainLabel.BufferLength = 100;
                Shell.UpdateQueue.Add(autoTrainLabel);
                Shell.RenderQueue.Add(autoTrainLabel);
            }
            if (Shell.GetEntityByName("BUTTON_TOGGLE_AUTOTRAIN") == null) //Create autotrain button
            {
                Checkbox autotrainToggle = new Checkbox("BUTTON_TOGGLE_AUTOTRAIN", new Vector2(550, 650), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_AUTOTRAIN_CHECKBOX"], 0.95f, false);
                autotrainToggle.CenterOrigin = false;
                autotrainToggle.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(IterativeMemBoTs).GetMethod("SetToggleAutoTrainer"), new object[] { autotrainToggle });
                autotrainToggle.IsUIElement = true;
                Shell.UpdateQueue.Add(autotrainToggle);
                Shell.RenderQueue.Add(autotrainToggle);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_FIELD_LABEL") == null) //Create simulation ID field label
            {
                TextEntity simFieldLabel = new TextEntity("IMEMBOTS_SIMID_FIELD_LABEL", "[F:SYSFONT]Current sim ID (must be +int):", new Vector2(185, 560), 0.95f);
                simFieldLabel.IsUIElement = true;
                simFieldLabel.BufferLength = 150;
                Shell.UpdateQueue.Add(simFieldLabel);
                Shell.RenderQueue.Add(simFieldLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_FIELD") == null) //Create simulation ID text field
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
            if (Shell.GetEntityByName("BUTTON_UPDATE_BOT_FITNESS") == null) //Create button to measure fitness manually
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
            if (Shell.GetEntityByName("BUTTON_BREED_NEW_GENERATION") == null) //Create button to breed the next controller generation
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
            if (Shell.GetEntityByName("BUTTON_APPLY_NEXT_GENERATION") == null) //Create button to apply the next controller generation
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
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_LOAD_FIELD_LABEL") == null) //Create load weights SimID field label
            {
                TextEntity simFieldLabel = new TextEntity("IMEMBOTS_SIMID_LOAD_FIELD_LABEL", "[F:SYSFONT]Weights are saved to current sim ID and loaded from specified sim ID.", new Vector2(860, 580), 0.95f);
                simFieldLabel.IsUIElement = true;
                simFieldLabel.BufferLength = 320;
                Shell.UpdateQueue.Add(simFieldLabel);
                Shell.RenderQueue.Add(simFieldLabel);
            }
            if (Shell.GetEntityByName("IMEMBOTS_SIMID_LOAD_FIELD") == null) //Create load weights SimID text field
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
            if (Shell.GetEntityByName("BUTTON_LOAD_WEIGHTS") == null) //Create button to load weights
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
            if (Shell.GetEntityByName("BUTTON_SAVE_WEIGHTS") == null) //Create button to load weights
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
            if (Shell.GetEntityByName("BUTTON_HIDE_UI") == null) //Create button to hide the UI
            {
                Checkbox hideUI = new Checkbox("BUTTON_HIDE_UI", new Vector2(1100, 660), (TAtlasInfo)Shell.AtlasDirectory["IMEMBOTS_EYECHECKBOX"], 0.95f, false);
                hideUI.CenterOrigin = false;
                hideUI.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("RefreshUIHideState"), null);
                hideUI.IsUIElement = true;
                Shell.UpdateQueue.Add(hideUI);
                Shell.RenderQueue.Add(hideUI);
            }
        }
        /// <summary>
        /// Method to initiailize the main simulation environment
        /// </summary>
        /// <param name="botCount">Number of Bots to spawn</param>
        /// <param name="spawnRadius">Radius in which to spawn entities</param>
        public static void InitializeMainSim(int botCount, int spawnRadius)
        {
            Shell.WriteLine("Initializing IMemBoTs simulation.");
            foreach(WorldEntity worldEntity in Shell.RenderQueue)
            {
                worldEntity.Drawable = false; //Hide any existing objects on screen
            }
            ButtonScripts.SpoonsTrip = true;
            Shell.OneFadeout(); //Fade out any music that is playing
            ScriptProcessor.ClearNonUIEntities(); //Clear any non-UI entities
            Shell.LooseCamera = true; //Camera position can be modified via click and drag
            ScriptProcessor.AssertGameRunningWithoutScript = true; //The ScriptProcessor needs to be told that the engine is active without an active script
            Shell.BackdropColour = Color.Aquamarine; //Set backdrop to aquamarine
            GenerationFitness?.Clear(); //Clear fitnesses
            Type fitnessType = typeof(SurviveEnergize); //Set fitness evaluator type to current fitness type
            InitMembotsUI(fitnessType.Name); //Initialize the IMemBoTs simulation UI
            AutoTrainerRunning = false; //Autotrainer should not be running by default
            s_queuedGeneration = 1; //Queued generation starts at one
            s_currentGeneration = 1; //Current generation starts at one
            //InitCircleEscapeEnvironment(spawnRadius);
            InitRandEnvironment(3000, 600, 15, 20, 0); //Initialize random environment
            Process pyProcess = PythonController.StartPythonProcess("IMemBoTs\\Python\\socketmanager.py"); //Start the python script
            Shell.WriteLine("Python process started. Waiting for socket listener to report...");
            while(true) //Wait for the PythonController to report that the Python process has started
            {
                System.Threading.Thread.Sleep(100);
                if (SocketsOpenedFlag)
                {
                    break;
                }
            }
            Shell.WriteLine("Socket listener reports initialization on remote process. Execution will now continue.");
            SystemSocketID = AddNewSocketAsTask(); //Create system socket.
            Shell.WriteLine("Awaiting confirmation of system socket hook-up...");
            while (true) //Wait for the system socket to be assigned correctly by the Python script
            {
                System.Threading.Thread.Sleep(100);
                if (SystemSocketAssigned)
                {
                    break;
                }
            }
            Shell.WriteLine("Socket listener reports system socket assigned. Execution will now continue.");
            Shell.UpdateQueue.Add(new SocketCloserEntity("IMEMBOTS SOCKET CLOSER", pyProcess)); //Instantiate socket closer entity to handle socket closing on simulation exit
            int total = 0;
            while (total < botCount) //Spawn bots
            {
                total++;
                Vector2 pos = Bot.GetStartPosByIndex(total); //Set spawn position from lookup function
                Bot bot = new Bot("IMEMBOT_" + total, pos, 0.4f + (0.001f * total), Shell.Rnd.Next(1, 1), new Vector2()); //Set bot parameters
                bot.CenterOrigin = true; //Set bot graphics origin to central
                bot.Rotate((float)GraphicsTools.VectorToBearing(pos) - bot.RotationRads); //Rotate to point away from spawn coordinate
                bot.MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour()); //Apply drag physics to bot
                bot.FitnessEvaluator = (IFitnessEvaluator)Activator.CreateInstance(fitnessType); //Apply fitness evaluator of correct type to bot
                bot.UsesFood = true; //Tell bot whether this simulation uses food/energy
                if (total == 1) //For the first bot only
                {
                    bot.MyBehaviours.Add(new Behaviours.DynamicWASDControlBehaviour()); //Apply user control
                    bot.AutoRotateToVelocityBearing = false;
                    Shell.AutoCamera.QuickMoveTo(bot.Position); //Center the camera
                }
                Shell.UpdateQueue.Add(bot);
                Shell.RenderQueue.Add(bot);
            }
            Shell.AutoCamera.AutoSnapToOnResetEntityName = "IMEMBOT_1"; //Camera should auto-reset to bot 1
            MutationAddedWeight = 0.5; //Set mutation added weight property
            ParamStandardUncertainty = 0.005; //Set parameter uncertainty propoerty
            Shell.UpdateQueue.Add(new AutoTrainController(30000, new object[] { true, 3000, 400 })); //Create autotrain controller
            //Shell.UpdateQueue.Add(new AutoTrainController(10000, new object[] { true, 3000, 400 }));
        }
        /// <summary>
        /// The Bot is the dynamic entity that can be controlled by a neural controller within the simulation environment.
        /// </summary>
        [Serializable]
        public class Bot : DynamicEntity
        {
            /// <summary>
            /// Static list of all bots
            /// </summary>
            public static List<Bot> Bots = new List<Bot>();
            /// <summary>
            /// Function to retrieve the spawn location of each bot
            /// </summary>
            /// <param name="index">Index of its bot for the function</param>
            /// <returns>Spawn location as a vector 2</returns>
            public static Vector2 GetStartPosByIndex(int index)
            {
                //Bots spawn in a pre-defined circular shell pattern, each shell consisting 8 bots except the first.
                return new Vector2((float)Math.Sin(index * (Math.PI / 4)), (float)Math.Cos(index * (Math.PI / 4))) * (float)(200 * Math.Ceiling((index - 1) / 8d));
            }
            /// <summary>
            /// Socket ID for this bot
            /// </summary>
            public ulong SocketID
            {
                get; private set;
            }
            /// <summary>
            /// Property defining whether the bot should rotate automatically towards is motion direction
            /// </summary>
            public Boolean AutoRotateToVelocityBearing { get; set; }
            /// <summary>
            /// Method to create sensory "whisker" traces
            /// </summary>
            protected void SetupTraces()
            {
                //Forward sensory trace
                GraphicsTools.Trace forwardSense = new GraphicsTools.Trace(new Vector2(), 0, 500);
                forwardSense.DrawColour = Color.Yellow;
                forwardSense.AlignToEntity = true; //Rotate to match the bot bearing
                forwardSense.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(forwardSense); //Render trace as part of the bot
                _senseTraces.Add(forwardSense); //Should be used for world sensing

                //Left sensory trace
                GraphicsTools.Trace leftSense = new GraphicsTools.Trace(new Vector2(), -Math.PI / 4, 500);
                leftSense.DrawColour = Color.Yellow;
                leftSense.AlignToEntity = true; //Rotate to match the bot bearing
                leftSense.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(leftSense); //Render trace as part of the bot
                _senseTraces.Add(leftSense); //Should be used for world sensing

                GraphicsTools.Trace rightSense = new GraphicsTools.Trace(new Vector2(), Math.PI / 4, 500);
                rightSense.DrawColour = Color.Yellow;
                rightSense.AlignToEntity = true; //Rotate to match the bot bearing
                rightSense.CalculateVertices(Shell.PubGD);
                MyVertexRenderables.Add(rightSense); //Render trace as part of the bot
                _senseTraces.Add(rightSense); //Should be used for world sensing
            }
            private List<GraphicsTools.Trace> _senseTraces = new List<GraphicsTools.Trace>(); //List of sense traces for this bot
            /// <summary>
            /// Array property of sense traces for this bot
            /// </summary>
            public GraphicsTools.Trace[] SenseTraces
            {
                get { return _senseTraces.ToArray(); }
            }
            /// <summary>
            /// Bot constructor
            /// </summary>
            /// <param name="name">WorldEntity name</param>
            /// <param name="location">Spawn position vector</param>
            /// <param name="depth">Depth within the 2D rendering scene</param>
            /// <param name="mass">DynamicEntity mass</param>
            /// <param name="initialVelocity">DynamicEntity start velocity</param>
            public Bot(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["BOT"], depth, mass)
            {
                Bots.Add(this); //Add this bot to the static list
                SocketID = AddNewSocketAsTask(); //Create a new socket for this bot and extract the ID
                int subtractColour = (int)((250 / 9) * (mass - 1)); //Set bot colour dependent on mass
                ColourValue = new Color(255, 255 - subtractColour, 255 - subtractColour);
                Collider = new RadialCollider(50, new Vector2()); //Create bot collider
                AnimationQueue.Add(Animation.PlayAllFrames(Atlas, 200, true)); //Start bot graphics animation
                Velocity = initialVelocity; //Set start velocity
                AutoRotateToVelocityBearing = false; //The bot should not rotate to always face forward
                SelfMovementForceMultiplier = 3; //Set self movement force
                SelfRotationRate = 0.2f; //Set rotation rate

                SetupTraces(); //Set up sensory traces
            }
            private double[] _controlCodes = new double[128];
            /// <summary>
            /// Property to store the codes returned by the socket bridge for this bot's handler
            /// </summary>
            public double[] ControlCodes
            {
                get { return _controlCodes; }
            }
            private double[] _senseCodes = new double[128];
            /// <summary>
            /// Method to dispatch sensory codes via socket bridge to this bot's controller
            /// </summary>
            /// <param name="codes">Codes to send</param>
            private void DispatchCodesToSocket(double[] codes)
            {
                PySocketQuery thisReceivedQuery = GetQuery(SocketID); //Check that query is ready to be sent
                if (thisReceivedQuery.LastReceive) { return; } //Return if not
                byte[] data = new byte[1024];
                int startIndex = 0;
                foreach (double d in codes)
                {
                    byte[] thisDouble = BitConverter.GetBytes(d); //Convert codes to bytes
                    thisDouble.CopyTo(data, startIndex);
                    startIndex += 8;
                }
                SendQuery(SocketID, data, false); //Dispatch codes
            }
            private PySocketQuery _lastReceivedQuery;
            /// <summary>
            /// Receive codes from the socket handler for this bot
            /// </summary>
            /// <returns>Returned codes from the Python script</returns>
            private double[] GetCodesFromSocket()
            {
                PySocketQuery thisReceivedQuery = GetQuery(SocketID); //Get query
                double[] codes = new double[0];
                if (thisReceivedQuery.LastReceive) //If something was received
                {
                    AcknowledgeReceive(SocketID); //Acknowledge that it has been received
                    codes = new double[128];
                    _lastReceivedQuery = thisReceivedQuery;
                    for (int i = 0; i < 128; i++)
                    {
                        codes[i] = BitConverter.ToDouble(thisReceivedQuery.Receive.AsSpan(i * 8, 8).ToArray()); //Convert it to double values
                    }
                }
                return codes;
            }
            /// <summary>
            /// Sense the environment and return sense codes to be dispatched
            /// </summary>
            /// <returns>Double array of values relating to world sensing returns</returns>
            public double[] SenseEnvironment()
            {
                double[] sensingOut = new double[128];
                sensingOut[0] = Math.Min(10d, GraphicsTools.Trace.GetAlignedComponent(Velocity, ForwardTrace.AsAlignedVector).Length() / 10d); //Forward-aligned velocity
                sensingOut[1] = Math.Min(10d, GraphicsTools.Trace.GetPerpendicularComponent(Velocity, ForwardTrace.AsAlignedVector).Length() / 10d); //Sideways-aligned velocity
                sensingOut[8] = Hunger; //Hunger
                for (int i = 0; i < _senseTraces.Count; i++) //For each sensory trace
                {
                    GraphicsTools.Trace thisTrace = _senseTraces[i];
                    GraphicsTools.Trace sensingTrace = thisTrace.Scale(new Vector2(), Size).Rotate(new Vector2(), RotationRads).Translate(Position); //Rotate, translate and scale based on bot position
                    Double nearestDistance = Double.NaN;
                    WorldEntity nearestObject = null;
                    Boolean detect = false;
                    foreach (WorldEntity worldEntity in Shell.UpdateQueue) //Check if the trace is colliding with any entity in the world
                    {
                        if (worldEntity == this) { continue; } //Ignore if it collides the bot itself
                        if (worldEntity.Collider != null && (Position - worldEntity.Position).Length() <= worldEntity.Collider.GetMaximumExtent() + sensingTrace.Length) //Ignore any entity that could never be within collision distance based on collider extents
                        {
                            Vector2? potentialIntersection = worldEntity.Collider.GetFirstIntersection(sensingTrace); //Get first potential intersection
                            if (potentialIntersection != null) //If intersection is confirmed
                            {
                                detect = true;
                                Double distance = (((Vector2)potentialIntersection) - sensingTrace.Origin).Length(); //Check intersection distance
                                if (Double.IsNaN(nearestDistance) || distance < nearestDistance) //Update distance value and object type if it is the closest collision found
                                {
                                    nearestDistance = distance;
                                    nearestObject = worldEntity;
                                }
                            }
                        }
                    }
                    if (detect) //If something is detected
                    {
                        _senseTraces[i].DrawColour = Color.Red; //Turn the trace red
                        sensingOut[2 + (i * 2)] = nearestDistance / sensingTrace.Length; //Set the first sensory out to depend on the distance
                        //Calculate object type value and set it
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
                    else //Otherwise...
                    {
                        _senseTraces[i].DrawColour = Color.Yellow; //Set trace yellow
                        sensingOut[2 + (i * 2)] = 1d; //Maximum distance for sense trace
                        sensingOut[3 + (i * 2)] = 0d; //0 value for object type
                    }
                }
                return sensingOut;
            }
            /// <summary>
            /// Multiplier for self movement force
            /// </summary>
            public float SelfMovementForceMultiplier { get; set; }
            /// <summary>
            /// Rate for self rotation
            /// </summary>
            public float SelfRotationRate { get; set; }
            /// <summary>
            /// Method to send system codes via the bot's assigned socket.
            /// </summary>
            /// <param name="codesToSend">System codes ready for dispatch</param>
            public void SendSystemCodes(double[] codesToSend)
            {
                _lastSystemCodes = codesToSend;
            }
            double[] _lastSystemCodes = null;
            public static readonly int SenseFrequency = 4; //Only sense every so many frames
            int _senseClock = Shell.Rnd.Next(0, SenseFrequency);
            private IFitnessEvaluator _fitnessEvaluator = null;
            /// <summary>
            /// The FitnessEvaluator entity paired to this bot.
            /// </summary>
            public IFitnessEvaluator FitnessEvaluator
            {
                get
                {
                    return _fitnessEvaluator;
                }
                set
                {
                    _fitnessEvaluator = value;
                    _fitnessEvaluator.Initialize(this); //Initialize on assign
                }
            }
            private Double _fitness = 0;
            /// <summary>
            /// This bot's current fitness, starting at zero,
            /// </summary>
            public Double Fitness
            {
                get { return _fitness; }
            }
            /// <summary>
            /// Property representing whether the bot is awaiting a system code return over its associated socket.
            /// </summary>
            public Boolean AwaitingSystemReturn
            {
                get
                {
                    return _lastSystemCodes != null;
                }
            }
            int _lastSystemReturnTime = Int32.MinValue;
            /// <summary>
            /// Time of last system code return over the bot's associated socket.
            /// </summary>
            public int? LastSystemReturnTime
            {
                get
                {
                    return _lastSystemReturnTime;
                }
            }
            int _lastControlReceive = 0;
            double _hunger = 0;
            /// <summary>
            /// Hunger value for this bot
            /// </summary>
            public double Hunger
            {
                get { return _hunger; }
                set { _hunger = value; }
            }
            /// <summary>
            /// Whether this bot should use food.
            /// </summary>
            public Boolean UsesFood { get; set; }
            Boolean _dead = false;
            /// <summary>
            /// Boolean property for whether this bot is considered "dead" within the simulation.
            /// </summary>
            public Boolean Dead
            {
                get { return _dead; }
                set
                {
                    if (value && !_dead)
                    {
                        ColourValue = new Color(0, 0, 0, 255); //Dead bots should be black
                    }
                    else if (_dead && !value)
                    {
                        ColourValue = new Color(255, 255, 255, 255); //Live bots are set to full colour
                    }
                    _dead = value;
                }
            }
            /// <summary>
            /// Boolean property - did this bot hit a spike?
            /// </summary>
            public Boolean Spiked { get; set; }
            /// <summary>
            /// Update sequence for this bot.
            /// </summary>
            public override void Update()
            {
                if (AutoRotateToVelocityBearing) { RotationRads = (float)new VNFramework.GraphicsTools.Trace(Velocity).Bearing; } //Change rotation if auto-bearing align is enabled

                _senseClock = (_senseClock + 1) % SenseFrequency; //Update sense clock

                if (_senseClock == 0) //If sense clock is zero then it is time to sense the environment
                {
                    _senseCodes = SenseEnvironment(); //Retrieve sense codes from environment sensing function
                    //if (Name == "IMEMBOT_1") { Shell.WriteLine(String.Join(' ', _senseCodes.Take(9).Select(x => x.ToString()).ToArray())); }
                }

                if (_lastSystemCodes == null) //If the bot is not waiting on a system code response over its associated socket
                {
                    DispatchCodesToSocket(_senseCodes); //Dispatch the latest world sense codes
                    double[] receive = GetCodesFromSocket(); //Receive the return value
                    if (receive.Length > 0 && receive[0] != Double.MaxValue)
                    {
                        _controlCodes = receive; //Set the control codes from the socket return
                        int now = Environment.TickCount;
                        //Shell.WriteLine(Name + " control code receive latency: " + (now - _lastControlReceive));
                        _lastControlReceive = now;
                    }
                }
                else //If the bot needs to get a system code response
                {
                    DispatchCodesToSocket(_lastSystemCodes); //Re-send system codes
                    double[] receive = GetCodesFromSocket(); //attempt to receive
                    if (receive.Length > 0 && receive[0] == Double.MaxValue) //if the response is a reply to the system codes
                    {
                        _lastSystemCodes = null; //flip bot out of system code receive mode
                        _lastSystemReturnTime = Environment.TickCount;
                    }
                }
                foreach(WorldEntity worldEntity in PreviousFrameCollides) //Check what the bot collided with in the previous frame
                {
                    if (UsesFood && worldEntity is Plant) //For plant collides, set hunger to 0
                    {
                        _hunger = 0;
                    }
                    else if (worldEntity is Spike) //For spike collides, kill bot
                    {
                        Dead = true;
                        Spiked = true;
                    }
                }
                if (_controlCodes[0] == 1 && !Dead) //If the bot is not dead, respond to control codes from Python controller
                {
                    Vector2 forward = ForwardTrace.AsAlignedVector * (float)(_controlCodes[1]) * SelfMovementForceMultiplier; //Move forward
                    Vector2 backward = ForwardTrace.AsAlignedVector * -(float)(_controlCodes[2]) * SelfMovementForceMultiplier / 10; //Move backward
                    float right = (float)(_controlCodes[3]) * SelfRotationRate; //Move left
                    float left = (float)(-_controlCodes[4]) * SelfRotationRate; //Move right
                    ApplyForce(forward + backward); //Apply movement
                    Rotate(right + left); //Apply rotation
                    if(UsesFood) //For food simulations, apply hunger updates
                    {
                        _hunger += forward.Length() / 600d;
                        _hunger += backward.Length() / 600d;
                        _hunger += right / 1000d;
                        _hunger += -left / 1000d;
                        _hunger += 0.001;
                        ColourValue = new Color((int)((1 - _hunger) * 255d), (int)((1 - _hunger) * 255d), 255, 255); //Colour is dependent on hunger
                    }
                }
                if(_hunger >= 1) { Dead = true; } //Kill bot if it has no energy
                if(FitnessEvaluator != null)
                {
                    _fitness = FitnessEvaluator.Evaluate(this); //Update bot fitness each iteration
                }
                base.Update();
            }
            private Boolean _socketClosed = false;
            /// <summary>
            /// Manual dispose override for this bot, to dispose it and close its socket connection
            /// </summary>
            public override void ManualDispose()
            {
                if(Bots.Contains(this)) { Bots.Remove(this); }
                if (PythonController.SocketInterface.OpenSockets.Contains(SocketID) && !_socketClosed) { CloseSocket(SocketID); }
                _socketClosed = true;
                base.ManualDispose();
            }
        }
        /// <summary>
        /// The plant entity does not move itself, but is randomly placed and restores bot food.
        /// </summary>
        [Serializable]
        public class Plant : DynamicEntity
        {
            Vector2 _originalPosition = Vector2.Zero;
            /// <summary>
            /// Plant constructor
            /// </summary>
            /// <param name="name">WorldEntity name</param>
            /// <param name="location">Vector2 spawn location</param>
            /// <param name="depth">Depth within 2D scene</param>
            /// <param name="mass">Mass</param>
            /// <param name="initialVelocity">Initial velocity for DynamicEntity</param>
            public Plant(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["FOODPLANT"], depth, mass)
            {
                _originalPosition = location; //Record start position
                CenterOrigin = true; //Set graphical origin
                Collider = new RadialCollider(60, new Vector2()); //Create collider
                Velocity = initialVelocity; //Set initial velocity
                MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour()); //Apply drag physics to this object
                AngularVelocity = (float)(Shell.Rnd.NextDouble() - 0.5) * 0.05f; //Impart random angular velocity for graphical flair (does not affect the simulation)
            }
            /// <summary>
            /// Reset plant position to its original location
            /// </summary>
            public void Reset()
            {
                QuickMoveTo(_originalPosition);
                Velocity = new Vector2();
                Acceleration = new Vector2();
            }
        }
        /// <summary>
        /// The spike entity will kill any bot that touches it.
        /// </summary>
        [Serializable]
        public class Spike : DynamicEntity
        {
            Vector2 _originalPosition = Vector2.Zero;
            /// <summary>
            /// Spike constructor
            /// </summary>
            /// <param name="name">WorldEntity name</param>
            /// <param name="location">Vector2 spawn location</param>
            /// <param name="depth">Depth within 2D scene</param>
            /// <param name="mass">Mass</param>
            /// <param name="initialVelocity">Initial velocity for DynamicEntity</param>
            public Spike(String name, Vector2 location, float depth, double mass, Vector2 initialVelocity) : base(name, location, Shell.AtlasDirectory["DAMAGESPIKE"], depth, mass)
            {
                _originalPosition = location; //Record start position
                CenterOrigin = true; //Set graphical origin
                Collider = new RadialCollider(60, new Vector2()); //Create collider
                Velocity = initialVelocity; //Set initial velocity
                MyBehaviours.Add(new Behaviours.DragPhysicsBehaviour()); //Apply drag physics to this object
                AngularVelocity = (float)(Shell.Rnd.NextDouble() - 0.5) * 0.05f; //Impart random angular velocity for graphical flair (does not affect the simulation)
            }
            /// <summary>
            /// Reset spike position to its original location
            /// </summary>
            public void Reset()
            {
                QuickMoveTo(_originalPosition);
                Velocity = new Vector2();
                Acceleration = new Vector2();
            }
        }
        /// <summary>
        /// The wall entity does nothing and simply acts as an obstacle in the simulation
        /// </summary>
        [Serializable]
        public class Wall : WorldEntity
        {
            /// <summary>
            /// Wall constructor
            /// </summary>
            /// <param name="name">WorldEntity name</param>
            /// <param name="location">Spawn location, position within the world as a Vector2</param>
            /// <param name="depth">Depth within the 2D scene</param>
            public Wall(String name, Vector2 location, float depth) : base(name, location, Shell.AtlasDirectory["HEXWALL"], depth)
            {
                CenterOrigin = true;
                Collider = new Polygon(new Rectangle(Hitbox.Location - VNFUtils.ConvertVector(Position), Hitbox.Size)); //Create polygon collider for this static wall
            }
        }
    }
}
