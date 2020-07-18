using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

/*VNF GENERAL TO-DO:
 * Inbuilt console
 */

namespace VNFramework
{
    [Serializable]
    public struct RecallableState
    {
        public ulong[] UpdateIDs;
        public ulong[] RenderIDs;
        public byte[][] SerializedEnts;
        public string LabelEntity;
        public string SongCom;
        public Hashtable Flags;
    }
    public class Shell : Game
    {
        public static String FrameworkVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (" + System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion + ")";
        public static Random Rnd = new Random();
        static Hashtable Flags = new Hashtable();
        public static String GlobalWorldState = "DEFAULT";
        private static Boolean pHasConsole = true;
        public static Camera AutoCamera = null;
        public static Boolean LooseCamera { get; set; }
        public static Boolean HasConsole
        {
            get
            {
                return pHasConsole;
            }
        }
        public static GraphicsDevice PubGD;
        public static WorldEntity GetEntityByName(String Name)
        {
            object O = null;
            for (int i = 0; i < UpdateQueue.Count; i++)
            {
                if (((WorldEntity)UpdateQueue[i]).Name.ToUpper() == Name.ToUpper())
                {
                    O = UpdateQueue[i];
                    return (WorldEntity)O;
                }
            }
            return null;
        }
        public static WorldEntity GetEntityByID(ulong ID)
        {
            object O = null;
            for (int i = 0; i < UpdateQueue.Count; i++)
            {
                if (((WorldEntity)UpdateQueue[i]).EntityID == ID)
                {
                    O = UpdateQueue[i];
                    return (WorldEntity)O;
                }
            }
            return null;
        }
        public static Vector2 CoordNormalize(Vector2 In)
        {
            return In * (720 / ScreenSize.Y);
        }
        static GraphicsDeviceManager graphics;
        private SpriteBatch pSpriteBatch;
        public SpriteBatch spriteBatch
        {
            get
            {
                return pSpriteBatch;
            }
        }
        public static RecallableState SerializeState()
        {
            ArrayList UpdateIDs = new ArrayList();
            ArrayList RenderIDs = new ArrayList();
            ArrayList SerializedIDs = new ArrayList();
            IFormatter SerFormatter = new BinaryFormatter();
            ArrayList Streams = new ArrayList();
            SurrogateSelector SS = new SurrogateSelector();
            Surrogates.Vector2SS V2SS = new Surrogates.Vector2SS();
            Surrogates.PointSS PSS = new Surrogates.PointSS();
            Surrogates.RectangleSS RSS = new Surrogates.RectangleSS();
            Surrogates.Texture2DSS T2DSS = new Surrogates.Texture2DSS();
            Surrogates.ColorSS CSS = new Surrogates.ColorSS();
            Surrogates.SpriteFontSS SFSS = new Surrogates.SpriteFontSS();
            SS.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), V2SS);
            SS.AddSurrogate(typeof(Point), new StreamingContext(StreamingContextStates.All), PSS);
            SS.AddSurrogate(typeof(Rectangle), new StreamingContext(StreamingContextStates.All), RSS);
            SS.AddSurrogate(typeof(Texture2D), new StreamingContext(StreamingContextStates.All), T2DSS);
            SS.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), CSS);
            SS.AddSurrogate(typeof(SpriteFont), new StreamingContext(StreamingContextStates.All), SFSS);
            SerFormatter.SurrogateSelector = SS;
            foreach (WorldEntity W in UpdateQueue)
            {
                UpdateIDs.Add(W.EntityID);
                W.OnSerializeDo();
                MemoryStream EntityStream = new MemoryStream();
                SerFormatter.Serialize(EntityStream, W);
                EntityStream.Close();
                Streams.Add(EntityStream.ToArray());
                SerializedIDs.Add(W.EntityID);
            }
            foreach(WorldEntity W in RenderQueue)
            {
                RenderIDs.Add(W.EntityID);
                if(!SerializedIDs.Contains(W.EntityID))
                {
                    W.OnSerializeDo();
                    MemoryStream EntityStream = new MemoryStream();
                    SerFormatter.Serialize(EntityStream, W);
                    EntityStream.Close();
                    Streams.Add(EntityStream.ToArray());
                    SerializedIDs.Add(W.EntityID);
                }
            }
            RecallableState Out = new RecallableState();
            Out.RenderIDs = RenderIDs.ToArray().Select(x => (ulong)x).ToArray();
            Out.UpdateIDs = UpdateIDs.ToArray().Select(x => (ulong)x).ToArray();
            Out.SerializedEnts = Streams.ToArray().Select(x => (byte[])x).ToArray();
            Out.LabelEntity = ScriptProcessor.LabelEntity;
            Out.SongCom = ScriptProcessor.SongCom;
            Out.Flags = (Hashtable)Flags.Clone();
            return Out;
        }
        public static void DeserializeState(RecallableState S, Boolean ReinstantiatePast)
        {
            if (ReinstantiatePast)
            {
                ScriptProcessor.PastStates.Clear();
                ScriptProcessor.PastStates.Push(S);
            }
            ArrayList ReconstructEnts = new ArrayList();
            IFormatter SerFormatter = new BinaryFormatter();
            SurrogateSelector SS = new SurrogateSelector();
            Surrogates.Vector2SS V2SS = new Surrogates.Vector2SS();
            Surrogates.PointSS PSS = new Surrogates.PointSS();
            Surrogates.RectangleSS RSS = new Surrogates.RectangleSS();
            Surrogates.Texture2DSS T2DSS = new Surrogates.Texture2DSS();
            Surrogates.ColorSS CSS = new Surrogates.ColorSS();
            Surrogates.SpriteFontSS SFSS = new Surrogates.SpriteFontSS();
            SS.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), V2SS);
            SS.AddSurrogate(typeof(Point), new StreamingContext(StreamingContextStates.All), PSS);
            SS.AddSurrogate(typeof(Rectangle), new StreamingContext(StreamingContextStates.All), RSS);
            SS.AddSurrogate(typeof(Texture2D), new StreamingContext(StreamingContextStates.All), T2DSS);
            SS.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), CSS);
            SS.AddSurrogate(typeof(SpriteFont), new StreamingContext(StreamingContextStates.All), SFSS);
            SerFormatter.SurrogateSelector = SS;
            foreach (byte[] Br in S.SerializedEnts)
            {
                MemoryStream DecodeStream = new MemoryStream(Br);
                WorldEntity WE = (WorldEntity)SerFormatter.Deserialize(DecodeStream);
                DecodeStream.Close();
                ReconstructEnts.Add(WE);
            }
            ArrayList NewUEnts = new ArrayList();
            ArrayList NewREnts = new ArrayList();
            foreach (VoidDel V in MouseLeftClick.GetInvocationList())
            {
                MouseLeftClick -= V;
            }
            RunQueue = new ArrayList();
            DeleteQueue = new ArrayList();
            UpdateQueue = new ArrayList();
            RenderQueue = new ArrayList();
            foreach (WorldEntity W in ReconstructEnts)
            {
                if (S.UpdateIDs.Contains(W.EntityID)) { NewUEnts.Add(W); }
                if (S.RenderIDs.Contains(W.EntityID)) { NewREnts.Add(W); }
                W.ReissueID();
                W.OnDeserializeDo();
            }
            ScriptProcessor.LabelEntity = S.LabelEntity;
            if(ScriptProcessor.SongCom != S.SongCom && S.SongCom != null && S.SongCom.Split('|').Length > 1)
            {
                ScriptProcessor.ActivateScriptElement(S.SongCom);
            }
            if (S.Flags != null) { Flags = (Hashtable)S.Flags.Clone(); }
            UpdateQueue = new ArrayList(NewUEnts);
            RenderQueue = new ArrayList(NewREnts);
            foreach (WorldEntity W in ReconstructEnts)
            {
                W.ResubscribeEvents();
            }
            ButtonScripts.UnHideUI();
        }
        public static VoidDel GlobalVoid = null;
        private static object CWriteLockObj = new object();
        public static void WriteLine(String Text)
        {
            pLastLogLine = Text;
            if (Text[0] != '[' && Text[Text.Length - 1] != ']')
            {
                Text = "[" + System.DateTime.Now.ToLongTimeString() + "] " + Text;
            }
            try
            {
                Monitor.Enter(CWriteLockObj);
                InternalLog += Text + "\n";
                if (pHasConsole) { Console.WriteLine(Text); }
            }
            finally { Monitor.Exit(CWriteLockObj); }
        }
        private static String pLastLogLine = "";
        public static String LastLogLine
        {
            get
            {
                String Out = "";
                try
                {
                    Monitor.Enter(CWriteLockObj);
                    Out = pLastLogLine;
                }
                finally { Monitor.Exit(CWriteLockObj); }
                return Out;
            }
        }
        public static Boolean QueryFullscreen()
        {
            return graphics.IsFullScreen;
        }
        public static void ToggleFullscreen()
        {
            if(!graphics.IsFullScreen)
            {
                Vector2 FullScreenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
                graphics.PreferredBackBufferWidth = (int)FullScreenSize.X;
                graphics.PreferredBackBufferHeight = (int)FullScreenSize.Y;
                ScreenSize = FullScreenSize;
                graphics.ApplyChanges();
                graphics.IsFullScreen = true;
            }
            else
            {
                Vector2 FullScreenSize = new Vector2(1280, 720);
                graphics.PreferredBackBufferWidth = (int)FullScreenSize.X;
                graphics.PreferredBackBufferHeight = (int)FullScreenSize.Y;
                ScreenSize = FullScreenSize;
                graphics.ApplyChanges();
                graphics.IsFullScreen = false;
            }
            if (!(CaptureFullscreen is null)) { CaptureFullscreen.ForceState(graphics.IsFullScreen); }
            graphics.ApplyChanges();
        }
        public static void DefaultSettings()
        {
            Mute = false;
            GlobalVolume = 0.6f;
            TextEntity.TickWriteInterval = 30;
            SaveLoadModule.ApplicableSaveType = "FullySerializedBinary";
            if(graphics.IsFullScreen) { ToggleFullscreen(); }
            if(CaptureFullscreen != null)
            {
                CaptureFullscreen.ForceState(QueryFullscreen());
            }
            if (CaptureVolume != null)
            {
                CaptureVolume.ForceState(GlobalVolume);
                CaptureVolume.Enabled = !Mute;
            }
            if (CaptureTextrate != null)
            {
                CaptureTextrate.ForceState(TextEntity.GetSliderValueFromTicks(TextEntity.TickWriteInterval));
            }
            if (CaptureRateDisplay != null)
            {
                CaptureRateDisplay.Text = TextEntity.TickWriteInterval + (TextEntity.TickWriteInterval != 1 ? " milliseconds" : " millisecond");
                CaptureRateDisplay.ReWrite();
            }
            if (CaptureMute != null)
            {
                CaptureMute.ForceState(Mute);
            }
            if (CaptureSaveType != null)
            {
                CaptureSaveType.ForceState(SaveLoadModule.ApplicableSaveType == "ScriptStem");
            }
        }
        public static String InternalLog = "";
        public RenderTarget2D TrueDisplay;
        public static ArrayList RenderQueue = new ArrayList();
        public static ArrayList UpdateQueue = new ArrayList();
        public static ArrayList DeleteQueue = new ArrayList();
        public static ArrayList RunQueue = new ArrayList();
        public static TAtlasInfo TestAtlas = new TAtlasInfo();
        public static SpriteFont SysFont;
        public static SpriteFont Default;
        public static SpriteFont King;
        public static Vector2 ScreenSize = new Vector2(1280, 720);
        public static Shell DefaultShell { get; set; }
        public Shell(String BootManifestFilePath, String BootManifestTitle)
        {
            pBootManifest = BootManifestFilePath;
            pBootManifestReadTitle = BootManifestTitle;
            DefaultShell = this;
            pHasConsole = true;
            try { int window_height = Console.WindowHeight; }
            catch { pHasConsole = false; }
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = (int)ScreenSize.Y;
            graphics.PreferredBackBufferWidth = (int)ScreenSize.X;
            graphics.HardwareModeSwitch = false;
            this.IsMouseVisible = true;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            WriteLine("[SHELL INITIALIZED AT " + System.DateTime.Now.ToLongTimeString() + " " + System.DateTime.Now.ToShortDateString() + "]");
            WriteLine("Blackhole's eVent horizoN Framework");
            WriteLine("Version: " + FrameworkVersion);
            SaveLoadModule.InitializeAppFolders();
            LooseCamera = true;
            AutoCamera = new Camera("Default Shell Autocamera");
            Mute = false;
            GlobalVolume = 0.6f;
            SaveLoadModule.ApplicableSaveType = "FullySerializedBinary";
            SaveLoadModule.PullOrInitPersistentState();
            ScriptProcessor.AllowScriptExit = true;
            PubGD = GraphicsDevice;
            ResetFlags();
            DoNextShifter = false;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public static void ResetFlags()
        {
            Flags = new Hashtable();
            Sofia.InitSofiaFlags();
        }
        public static void UpdateFlag(String Index, object Value)
        {
            if (Flags.ContainsKey(Index.ToUpper())) { Flags[Index.ToUpper()] = Value; }
            else { Flags.Add(Index.ToUpper(), Value); }
            WriteLine("Flag updated: " + Index + " set to " + Value);
        }
        public static object ReadFlag(String Index)
        {
            if (Flags.ContainsKey(Index.ToUpper())) { return Flags[Index.ToUpper()]; }
            else { return null; }
        }
        private static Boolean pMute = false;
        public static Boolean Mute
        {
            get
            {
                return pMute;
            }
            set
            {
                pMute = value;
                if(pMute)
                {
                    if(!(CaptureVolume is null))
                    {
                        CaptureVolume.ForceState(0f);
                        CaptureVolume.Enabled = false;
                    }
                    MediaPlayer.IsMuted = true;
                    foreach(SoundEffectInstance S in ActiveSounds)
                    {
                        S.Volume = 0f;
                    }
                }
                else
                {
                    if (!(CaptureVolume is null))
                    {
                        CaptureVolume.ForceState(GlobalVolume);
                        CaptureVolume.Enabled = true;
                    }
                    MediaPlayer.IsMuted = false;
                    foreach (SoundEffectInstance S in ActiveSounds)
                    {
                        S.Volume = GlobalVolume;
                    }
                }
            }
        }
        public static Boolean PlaySoundInstant(String SFXIndex)
        {
            return PlaySoundInstant(SFXIndex, false);
        }
        public static Boolean PlaySoundInstant(String SFXIndex, Boolean Loop)
        {
            if(Mute | !SFXDirectory.ContainsKey(SFXIndex.ToUpper())) { return false; }
            SoundEffectInstance LocalSound = ((SoundEffect)SFXDirectory[SFXIndex.ToUpper()]).CreateInstance();
            LocalSound.Volume = GlobalVolume;
            LocalSound.IsLooped = Loop;
            LocalSound.Play();
            ActiveSounds.Add(LocalSound);
            return true;
        }
        private static float pGlobalVolume = 1f;
        public static float GlobalVolume
        {
            get
            {
                return pGlobalVolume;
            }
            set
            {
                pGlobalVolume = value;
                MediaPlayer.Volume = pGlobalVolume;
                foreach (SoundEffectInstance S in ActiveSounds)
                {
                    S.Volume = pGlobalVolume;
                }
            }
        }
        public static void QueueInstantTrack(Song S)
        {
            QueueInstantTrack(S, 60f);
        }
        public static void QueueInstantTrack(Song S, float FadeoutDivisor)
        {
            if (MediaPlayer.State == MediaState.Stopped) { MediaPlayer.Play(S); }
            else
            {
                QueuedSong = S;
                FadeoutAmount = pGlobalVolume / (FadeoutDivisor);
            }
        }
        public static void OneFadeout()
        {
            OneFadeout(60f);
        }
        public static void OneFadeout(float FadeoutDivisor)
        {
            if(MediaPlayer.State == MediaState.Stopped) { return; }
            QueuedSong = null;
            FadeoutAmount = pGlobalVolume / (FadeoutDivisor);
        }
        static float FadeoutAmount = -100;
        static Song QueuedSong = null;
        public static TAtlasInfo ButtonAtlas = new TAtlasInfo();
        public static Texture2D TestLongRect;
        public static Hashtable AtlasDirectory = new Hashtable();
        public static Hashtable SFXDirectory = new Hashtable();
        public static Hashtable SongDirectory = new Hashtable();
        public static ArrayList ActiveSounds = new ArrayList();
        public static Checkbox CaptureFullscreen = null;
        public static Slider CaptureTextrate = null;
        public static TextEntity CaptureRateDisplay = null;
        public static Checkbox CaptureMute = null;
        public static Checkbox CaptureSaveType = null;
        public static Slider CaptureVolume = null;
        public static Hashtable Fonts = new Hashtable();
        public float LoadPercentage { get; set; }
        public object LPLockObj = new object();
        String pBootManifest;
        String pBootManifestReadTitle;
        public String BootManifest
        {
            get { return pBootManifest; }
        }
        public String BootManifestReadTitle
        {
            get { return pBootManifestReadTitle; }
        }
        public VoidDel PullAutoShift
        {
            get
            {
                return new VoidDel(delegate () { if (Shell.AllowEnter) { Shell.DoNextShifter = true; } });
            }
        }
        protected object[] AsyncLoad()
        {
            WriteLine("Preload complete, loading remaining content...");
            ScriptProcessor.ScriptCache = new Hashtable();

            TestAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/LethalHexWalk1");
            TestAtlas.DivDimensions = new Point(1, 1);

            TAtlasInfo InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Logos/MatmutLogo");
            InsertAtlas.DivDimensions = new Point(1, 1);
            AtlasDirectory.Add("MATMUTLOGO", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/MatmutBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            AtlasDirectory.Add("MATMUTBG", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SetManualSR(new Rectangle(new Point(0, 0), new Point(320, 180)));
            AtlasDirectory.Add("THUMBBLANK", InsertAtlas);

            Hashtable Manifests = ManifestReader.ReadManifestFile(BootManifest);
            object[] Resources = ManifestReader.ParseManifest((String)Manifests[BootManifestReadTitle], this);
            String FirstScript = "";
            Boolean RunFirstAsUnique = true;
            WriteLine("Reading application metainfo...");
            if (((Hashtable)Resources[0]).ContainsKey("startatscript"))
            {
                FirstScript = (String)((Hashtable)Resources[0])["startatscript"];
            }
            if (((Hashtable)Resources[0]).ContainsKey("useunique"))
            {
                RunFirstAsUnique = (Boolean)((Hashtable)Resources[0])["useunique"];
            }
            WriteLine("Ingesting utility scripts...");
            ManifestReader.IngestScriptFile("vnf_utils.esa");
            WriteLine("Ingesting application scripts...");
            int i = 0;
            foreach (String Script in ((ArrayList)Resources[1]))
            {
                ManifestReader.IngestScriptFile(Script);
                i++;
                Monitor.Enter(LPLockObj);
                LoadPercentage = (float)(0.95f + (0.05 * (i / ((ArrayList)Resources[1]).Count)));
                Monitor.Exit(LPLockObj);
            }
            WriteLine("Integrating loaded resources...");
            foreach (object key in ((Hashtable)Resources[2]).Keys)
            {
                Fonts.Add(key, (SpriteFont)((Hashtable)Resources[2])[key]);
            }
            foreach (object key in ((Hashtable)Resources[3]).Keys)
            {
                SFXDirectory.Add(key, (SoundEffect)((Hashtable)Resources[3])[key]);
            }
            foreach (object key in ((Hashtable)Resources[4]).Keys)
            {
                SongDirectory.Add(key, (Song)((Hashtable)Resources[4])[key]);
            }
            foreach (String key in ((Hashtable)Resources[5]).Keys)
            {
                AtlasDirectory.Add(key, (TAtlasInfo)((Hashtable)Resources[5])[key]);
            }
            ArrayList ADKeys = new ArrayList();
            foreach (String K in AtlasDirectory.Keys)
            {
                ADKeys.Add(K);
            }
            foreach (String K in ADKeys)
            {
                TAtlasInfo Copy = ((TAtlasInfo)AtlasDirectory[K]);
                Copy.ReferenceHash = K;
                AtlasDirectory[K] = Copy;
            }
            TestLongRect = Content.Load<Texture2D>("Textures/Entities/UI/Elements/TestLongRect");
            ButtonAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/ButtonDefault");
            ButtonAtlas.DivDimensions = new Point(2, 1);
            Monitor.Enter(LPLockObj);
            LoadPercentage = 1;
            Monitor.Exit(LPLockObj);
            WriteLine("Content load complete.");
            return new object[] { FirstScript, RunFirstAsUnique };
        }
        private WorldEntity LoadBar;
        public WorldEntity LoadBarObj
        {
            get
            {
                return LoadBar;
            }
        }
        private WorldEntity LoadCover;
        private TextEntity LoadText;
        protected override void LoadContent()
        {
            WriteLine("VNF is loading content...");
            // Create a new SpriteBatch, which can be used to draw textures.
            pSpriteBatch = new SpriteBatch(GraphicsDevice);
            TrueDisplay = new RenderTarget2D(
                GraphicsDevice,
                1280,
                720,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            SysFont = Content.Load<SpriteFont>("Fonts/SysFont");
            Default = Content.Load<SpriteFont>("Fonts/Default");
            King = Content.Load<SpriteFont>("Fonts/Zenda");

            TAtlasInfo BarAtlas = new TAtlasInfo();
            BarAtlas.Atlas = Content.Load<Texture2D>("Textures/Preload/LoadingBar");
            BarAtlas.DivDimensions = new Point(1, 1);
            TAtlasInfo CoverAtlas = new TAtlasInfo();
            CoverAtlas.Atlas = Content.Load<Texture2D>("Textures/Preload/LoadingCover");
            CoverAtlas.DivDimensions = new Point(1, 1);

            Vector2 AssumedScreenSize = new Vector2(1280, 720);

            LoadBar = new WorldEntity("LOADBAR", new Vector2((AssumedScreenSize.X / 2) - 250, (AssumedScreenSize.Y / 2) + 100), BarAtlas, 0.5f);
            LoadCover = new WorldEntity("LOADCOVER", new Vector2((AssumedScreenSize.X / 2) + 243, (AssumedScreenSize.Y / 2) + 107), CoverAtlas, 1f);
            LoadCover.SetManualOrigin(new Vector2(486, 0));
            LoadText = new TextEntity("LOADTEXT", "[F:SYSFONT]Loading content...", new Vector2((AssumedScreenSize.X / 2) - 250, (AssumedScreenSize.Y / 2) + 200), 1f);
            LoadText.BufferLength = 500;
            LoadText.TypeWrite = false;

            RenderQueue.Add(LoadBar);
            RenderQueue.Add(LoadCover);
            RenderQueue.Add(LoadText);

            LoadGraphicsQueue = new Queue();
            LoadPercentage = 0f;

            LoadOperation = new Task<object[]>(AsyncLoad);
            LoadOperation.Start();
        }
        protected Task<object[]> LoadOperation = null;
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            WriteLine("Client closing...");
            WriteLine("[SHELL EXITING AT " + System.DateTime.Now.ToLongTimeString() + " " + System.DateTime.Now.ToShortDateString() + "]");
            SaveLoadModule.WriteFinals();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        [field: NonSerialized]
        public static event VoidDel MouseLeftClick;
        protected MouseState LastMouseState;
        protected KeyboardState LastKeyState;
        public static Boolean ExitOut = false;
        public static Boolean AllowEnter = true;
        public static Boolean DoNextShifter { get; set; }
        float LastCapturedVol = 0f;
        float LastCapturedText = 0f;
        public Queue LoadGraphicsQueue { get; set; }
        protected override void Update(GameTime gameTime)
        {
            KeyboardState KCurrent = Keyboard.GetState();
            if (LoadOperation != null && !LoadOperation.IsCompleted)
            {
                try
                {
                    Monitor.Enter(LoadGraphicsQueue);
                    while (LoadGraphicsQueue.Count > 0)
                    {
                        Task GraphTask = (Task)LoadGraphicsQueue.Dequeue();
                        GraphTask.RunSynchronously();
                    }
                }
                finally { Monitor.Exit(LoadGraphicsQueue); }
                try
                {
                    Monitor.Enter(LPLockObj);
                    LoadCover.Scale(new Vector2((1 - LoadPercentage) - LoadCover.ScaleSize.X, 0));
                }
                finally { Monitor.Exit(LPLockObj); }
                LoadText.Text = "[F:SYSFONT]" + LastLogLine;
                if (KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11)) { ToggleFullscreen(); }
                LastKeyState = KCurrent;
            }
            else
            {
                if (LoadOperation != null)
                {
                    if(LoadOperation.IsFaulted)
                    {
                        MessageBox.Show("VNFramework could not launch as the application was unable to read the specified application manifest file.", "eVent horizoN Client Loader");
                        Exit();
                        return;
                    }
                    object[] LoadResult = LoadOperation.GetAwaiter().GetResult();
                    String FirstScript = (String)LoadResult[0];
                    Boolean RunFirstAsUnique = (Boolean)LoadResult[1];
                    if (FirstScript.Length > 0)
                    {
                        WriteLine("Priming first script.");
                        if (RunFirstAsUnique)
                        {
                            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer("INTRO_SNIFFER_UNIQUE", ScriptProcessor.RetrieveScriptByName(FirstScript), FirstScript));
                        }
                        else
                        {
                            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer(FirstScript + "_SNIFFER", ScriptProcessor.RetrieveScriptByName(FirstScript), FirstScript));
                        }
                    }
                    LoadBar.AnimationQueue.Add(Animation.Retrieve("FADEOUT"));
                    LoadBar.TransientAnimation = true;
                    UpdateQueue.Add(LoadBar);
                    DeleteQueue.Add(LoadCover);
                    DeleteQueue.Add(LoadText);
                    LoadOperation.Dispose();
                    LoadOperation = null;
                }
                MainUpdate(gameTime, KCurrent);
            }
            base.Update(gameTime);
        }
        protected void MainUpdate(GameTime gameTime, KeyboardState KCurrent)
        {
            if (KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                if (ScriptProcessor.ActiveGame())
                {
                    if (!ButtonScripts.Paused) { ButtonScripts.Pause(); }
                    else { ButtonScripts.Unpause(); }
                }
                else { ButtonScripts.Quit(); }
            }
            if (KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H) && ScriptProcessor.ActiveGame())
            {
                ButtonScripts.RefreshUIHideState();
            }
            if (AutoCamera != null && LooseCamera)
            {
                if (KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R))
                {
                    AutoCamera.CenterDefault();
                    AutoCamera.ResetZoom();
                }
                AutoCamera.MouseDragEnabled = KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F);
                if(!UpdateQueue.Contains(AutoCamera)) { UpdateQueue.Add(AutoCamera); }
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || ExitOut)
            {
                Exit();
            }
            if (((KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter)) || DoNextShifter) && AllowEnter)
            {
                DoNextShifter = false;
                Boolean Found = false;
                foreach(WorldEntity E in UpdateQueue)
                {
                    if(E is TextEntity && E.Name == "TEXT_MAIN")
                    {
                        if(((TextEntity)E).WrittenAll()) { GlobalWorldState = "CONTINUE"; }
                        ((TextEntity)E).SkipWrite();
                        Found = true;
                        break;
                    }
                }
                if(!Found) { GlobalWorldState = "CONTINUE"; }
            }      
            if (KCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11)) { ToggleFullscreen(); }
            LastKeyState = KCurrent;
            foreach (WorldEntity E in UpdateQueue)
            {
                E.Update();
            }
            foreach (WorldEntity E in DeleteQueue)
            {
                if (UpdateQueue.Contains(E)) { UpdateQueue.Remove(E); }
                if (RenderQueue.Contains(E)) { RenderQueue.Remove(E); }
                E.ManualDispose();
            }
            DeleteQueue = new ArrayList();
            foreach (VoidDel V in RunQueue)
            {
                V();
            }
            RunQueue = new ArrayList();
            if(FadeoutAmount != -100)
            {
                if(MediaPlayer.Volume > 0)
                {
                    MediaPlayer.Volume -= FadeoutAmount;
                }
                else
                {
                    MediaPlayer.Volume = GlobalVolume;
                    if (QueuedSong != null) { MediaPlayer.Play(QueuedSong); }
                    else { MediaPlayer.Stop(); }
                    QueuedSong = null;
                    FadeoutAmount = -100;
                }
            }
            ArrayList RemSounds = new ArrayList();
            if (!(CaptureVolume is null) && CaptureVolume.Enabled)
            {
                if (CaptureVolume.Output() != LastCapturedVol)
                {
                    GlobalVolume = CaptureVolume.Output();
                }
                LastCapturedVol = CaptureVolume.Output();
            }
            foreach (SoundEffectInstance S in ActiveSounds)
            {
                if(S.State == SoundState.Stopped) { RemSounds.Add(S); }
            }
            foreach(SoundEffectInstance RS in RemSounds)
            {
                ActiveSounds.Remove(RS);
            }
            if (!(CaptureTextrate is null) && CaptureTextrate.Enabled)
            {
                if (CaptureTextrate.Output() != LastCapturedText)
                {
                    TextEntity.TickWriteInterval = TextEntity.GetTicksFromSliderValue(CaptureTextrate.Output());
                    if(!(CaptureRateDisplay is null))
                    {
                        CaptureRateDisplay.Text = TextEntity.TickWriteInterval + (TextEntity.TickWriteInterval != 1 ? " milliseconds" : " millisecond");
                        CaptureRateDisplay.ReWrite();
                    }
                }
                LastCapturedText = CaptureTextrate.Output();
            }
            MouseState Current = Mouse.GetState();
            if(Current.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && LastMouseState.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed && MouseLeftClick != null) { MouseLeftClick(); }
            LastMouseState = Current;
            if (GlobalVoid != null)
            {
                GlobalVoid();
                GlobalVoid = null;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static Boolean HoldRender = false;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(TrueDisplay);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            if (!HoldRender)
            {
                if (AutoCamera == null)
                {
                    foreach (WorldEntity E in RenderQueue)
                    {
                        if (E.Drawable)
                        {
                            if (E.CustomCamera != null) { E.Draw(spriteBatch, E.CustomCamera); }
                            else { E.Draw(spriteBatch); }
                        }
                    }
                }
                else
                {
                    foreach (WorldEntity E in RenderQueue)
                    {
                        if (E.Drawable)
                        {
                            if (E.CustomCamera != null) { E.Draw(spriteBatch, E.CustomCamera); }
                            else { E.Draw(spriteBatch, AutoCamera); }
                        }
                    }
                }
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(TrueDisplay, new Rectangle(0, 0, (int)ScreenSize.X, (int)ScreenSize.Y), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
