﻿using Microsoft.Xna.Framework;
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
using static VNFramework.GraphicsTools;
using Tileset = VNFramework.GraphicsTools.TileRenderer.Tileset;

namespace VNFramework
{
    /// <summary>
    /// Struct that represents a recallable save state
    /// </summary>
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
    /// <summary>
    /// The Shell class is instanced on game load and is responsible for managing the application backend, including loading data, displaying graphics and audio, the update loop for entities, and performing save/load operations.
    /// </summary>
    public partial class Shell : Game
    {
        public static String FrameworkVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (" + System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion + ")";
        public static Random Rnd = new Random();
        static Hashtable s_flags = new Hashtable();
        public static String GlobalWorldState = "DEFAULT";
        private static Boolean s_hasConsole = true;
        public static Camera AutoCamera = null;
        public static Boolean LooseCamera { get; set; }
        public static Boolean HasConsole
        {
            get
            {
                return s_hasConsole;
            }
        }
        public static GraphicsDevice PubGD;
        public static WorldEntity GetEntityByName(String name)
        {
            object o = null;
            for (int i = 0; i < UpdateQueue.Count; i++)
            {
                if (((WorldEntity)UpdateQueue[i]).Name.ToUpper() == name.ToUpper())
                {
                    o = UpdateQueue[i];
                    return (WorldEntity)o;
                }
            }
            return null;
        }
        public static WorldEntity GetEntityByID(ulong id)
        {
            object o = null;
            for (int i = 0; i < UpdateQueue.Count; i++)
            {
                if (((WorldEntity)UpdateQueue[i]).EntityID == id)
                {
                    o = UpdateQueue[i];
                    return (WorldEntity)o;
                }
            }
            return null;
        }
        public static Vector2 CoordNormalize(Vector2 @in)
        {
            return @in * new Vector2(Resolution.X / WindowSize.X, Resolution.Y / WindowSize.Y);
        }
        static GraphicsDeviceManager s_graphics;
        private SpriteBatch _spriteBatch;
        public SpriteBatch ShellSpriteBatch
        {
            get
            {
                return _spriteBatch;
            }
        }
        static List<WorldEntity> s_nonSerializables = new List<WorldEntity>();
        public static List<WorldEntity> NonSerializables
        {
            get { return s_nonSerializables; }
            set { s_nonSerializables = value; }
        }
        public static RecallableState? SerializeState()
        {
            return SerializeState(NonSerializables);
        }
        public static RecallableState? SerializeState(List<WorldEntity> skip)
        {
            List<ulong> updateIDs = new List<ulong>();
            List<ulong> renderIDs = new List<ulong>();
            List<ulong> serializedIDs = new List<ulong>();
            IFormatter serFormatter = new BinaryFormatter();
            List<byte[]> streams = new List<byte[]>();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Surrogates.Vector2SS v2ss = new Surrogates.Vector2SS();
            Surrogates.PointSS pss = new Surrogates.PointSS();
            Surrogates.RectangleSS rss = new Surrogates.RectangleSS();
            Surrogates.Texture2DSS t2dss = new Surrogates.Texture2DSS();
            Surrogates.ColorSS css = new Surrogates.ColorSS();
            Surrogates.SpriteFontSS sfss = new Surrogates.SpriteFontSS();
            Surrogates.EventSRSS esrss = new Surrogates.EventSRSS();
            Surrogates.BasicEffectSS bess = new Surrogates.BasicEffectSS();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), v2ss);
            surrogateSelector.AddSurrogate(typeof(Point), new StreamingContext(StreamingContextStates.All), pss);
            surrogateSelector.AddSurrogate(typeof(Rectangle), new StreamingContext(StreamingContextStates.All), rss);
            surrogateSelector.AddSurrogate(typeof(Texture2D), new StreamingContext(StreamingContextStates.All), t2dss);
            surrogateSelector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), css);
            surrogateSelector.AddSurrogate(typeof(SpriteFont), new StreamingContext(StreamingContextStates.All), sfss);
            surrogateSelector.AddSurrogate(typeof(WorldEntity.EventSubRegister), new StreamingContext(StreamingContextStates.All), esrss);
            surrogateSelector.AddSurrogate(typeof(BasicEffect), new StreamingContext(StreamingContextStates.All), bess);
            serFormatter.SurrogateSelector = surrogateSelector;
            try
            {
                foreach (WorldEntity worldEntity in UpdateQueue)
                {
                    if (skip.Contains(worldEntity)) { continue; }
                    updateIDs.Add(worldEntity.EntityID);
                    worldEntity.OnSerializeDo();
                    MemoryStream entityStream = new MemoryStream();
                    serFormatter.Serialize(entityStream, worldEntity);
                    entityStream.Close();
                    streams.Add(entityStream.ToArray());
                    serializedIDs.Add(worldEntity.EntityID);
                }
                foreach (WorldEntity worldEntity in RenderQueue)
                {
                    if (skip.Contains(worldEntity)) { continue; }
                    renderIDs.Add(worldEntity.EntityID);
                    if (!serializedIDs.Contains(worldEntity.EntityID))
                    {
                        worldEntity.OnSerializeDo();
                        MemoryStream entityStream = new MemoryStream();
                        serFormatter.Serialize(entityStream, worldEntity);
                        entityStream.Close();
                        streams.Add(entityStream.ToArray());
                        serializedIDs.Add(worldEntity.EntityID);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine("Failed to serialize state due to " + e.GetType().Name + ": " + e.Message);
                return null;
            }
            RecallableState outState = new RecallableState();
            outState.RenderIDs = renderIDs.ToArray().Select(x => (ulong)x).ToArray();
            outState.UpdateIDs = updateIDs.ToArray().Select(x => (ulong)x).ToArray();
            outState.SerializedEnts = streams.ToArray().Select(x => (byte[])x).ToArray();
            outState.LabelEntity = ScriptProcessor.LabelEntity;
            outState.SongCom = ScriptProcessor.SongCom;
            outState.Flags = (Hashtable)s_flags.Clone();
            return outState;
        }
        public static void DeserializeState(RecallableState recallableState, Boolean reinstantiatePastStateList)
        {
            if (reinstantiatePastStateList)
            {
                ScriptProcessor.PastStates.Clear();
                ScriptProcessor.PastStates.Push(recallableState);
            }
            List<WorldEntity> reconstructEnts = new List<WorldEntity>();
            IFormatter serFormatter = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Surrogates.Vector2SS v2ss = new Surrogates.Vector2SS();
            Surrogates.PointSS pss = new Surrogates.PointSS();
            Surrogates.RectangleSS rss = new Surrogates.RectangleSS();
            Surrogates.Texture2DSS t2dss = new Surrogates.Texture2DSS();
            Surrogates.ColorSS css = new Surrogates.ColorSS();
            Surrogates.SpriteFontSS sfss = new Surrogates.SpriteFontSS();
            Surrogates.EventSRSS esrss = new Surrogates.EventSRSS();
            Surrogates.BasicEffectSS bess = new Surrogates.BasicEffectSS();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), v2ss);
            surrogateSelector.AddSurrogate(typeof(Point), new StreamingContext(StreamingContextStates.All), pss);
            surrogateSelector.AddSurrogate(typeof(Rectangle), new StreamingContext(StreamingContextStates.All), rss);
            surrogateSelector.AddSurrogate(typeof(Texture2D), new StreamingContext(StreamingContextStates.All), t2dss);
            surrogateSelector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), css);
            surrogateSelector.AddSurrogate(typeof(SpriteFont), new StreamingContext(StreamingContextStates.All), sfss);
            surrogateSelector.AddSurrogate(typeof(MethodInfo), new StreamingContext(StreamingContextStates.All), esrss);
            surrogateSelector.AddSurrogate(typeof(WorldEntity.EventSubRegister), new StreamingContext(StreamingContextStates.All), esrss);
            surrogateSelector.AddSurrogate(typeof(BasicEffect), new StreamingContext(StreamingContextStates.All), bess);
            serFormatter.SurrogateSelector = surrogateSelector;
            foreach (byte[] byteR in recallableState.SerializedEnts)
            {
                MemoryStream decodeStream = new MemoryStream(byteR);
                WorldEntity worldEntity = (WorldEntity)serFormatter.Deserialize(decodeStream);
                decodeStream.Close();
                reconstructEnts.Add(worldEntity);
            }
            List<WorldEntity> newUEnts = new List<WorldEntity>();
            List<WorldEntity> newREnts = new List<WorldEntity>();
            foreach (VoidDel voidDelegate in MouseLeftClick.GetInvocationList())
            {
                MouseLeftClick -= voidDelegate;
            }
            RunQueue = new List<VoidDel>();
            foreach (WorldEntity worldEntity in NonSerializables)
            {
                worldEntity.AddEventTriggers();
            }
            List<WorldEntity> tempList = new List<WorldEntity>();
            foreach (WorldEntity worldEntity in DeleteQueue)
            {
                if (NonSerializables.Contains(worldEntity)) { tempList.Add(worldEntity); }
            }
            DeleteQueue = tempList;
            foreach (WorldEntity worldEntity in reconstructEnts)
            {
                if (recallableState.UpdateIDs.Contains(worldEntity.EntityID)) { newUEnts.Add(worldEntity); }
                if (recallableState.RenderIDs.Contains(worldEntity.EntityID)) { newREnts.Add(worldEntity); }
                worldEntity.ReissueID();
                worldEntity.OnDeserializeDo();
            }
            ScriptProcessor.LabelEntity = recallableState.LabelEntity;
            if (ScriptProcessor.SongCom != recallableState.SongCom && recallableState.SongCom != null && recallableState.SongCom.Split('|').Length > 1)
            {
                ScriptProcessor.ActivateScriptElement(recallableState.SongCom);
            }
            if (recallableState.Flags != null) { s_flags = (Hashtable)recallableState.Flags.Clone(); }
            foreach (WorldEntity worldEntity in UpdateQueue)
            {
                if (NonSerializables.Contains(worldEntity)) { newUEnts.Add(worldEntity); }
            }
            foreach (WorldEntity worldEntity in RenderQueue)
            {
                if (NonSerializables.Contains(worldEntity)) { newREnts.Add(worldEntity); }
            }
            UpdateQueue = new List<WorldEntity>(newUEnts);
            RenderQueue = new List<WorldEntity>(newREnts);
            foreach (WorldEntity worldEntity in reconstructEnts)
            {
                worldEntity.ResubscribeEvents();
            }
            ButtonScripts.UnHideUI();
        }
        public static VoidDel GlobalVoid = null;
        public static Boolean QueryFullscreen()
        {
            return s_graphics.IsFullScreen;
        }
        public static void ToggleFullscreen()
        {
            if (!s_graphics.IsFullScreen)
            {
                Vector2 fullScreenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
                s_graphics.PreferredBackBufferWidth = (int)fullScreenSize.X;
                s_graphics.PreferredBackBufferHeight = (int)fullScreenSize.Y;
                WindowSize = fullScreenSize;
                s_graphics.ApplyChanges();
                s_graphics.IsFullScreen = true;
            }
            else
            {
                Vector2 fullScreenSize = Resolution;
                s_graphics.PreferredBackBufferWidth = (int)fullScreenSize.X;
                s_graphics.PreferredBackBufferHeight = (int)fullScreenSize.Y;
                WindowSize = fullScreenSize;
                s_graphics.ApplyChanges();
                s_graphics.IsFullScreen = false;
            }
            if (!(CaptureFullscreen is null)) { CaptureFullscreen.ForceState(s_graphics.IsFullScreen); }
            s_graphics.ApplyChanges();
        }
        public static void DefaultSettings()
        {
            Mute = false;
            GlobalVolume = 0.6f;
            TextEntity.TickWriteInterval = 30;
            SaveLoadModule.ApplicableSaveType = "FullySerializedBinary";
            if (s_graphics.IsFullScreen) { ToggleFullscreen(); }
            if (CaptureFullscreen != null)
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
        public static List<Object[]> InternalLog = new List<object[]>();
        public RenderTarget2D TrueDisplay;
        public static List<WorldEntity> RenderQueue = new List<WorldEntity>();
        public static List<WorldEntity> UpdateQueue = new List<WorldEntity>();
        public static List<WorldEntity> DeleteQueue = new List<WorldEntity>();
        public static List<VoidDel> RunQueue = new List<VoidDel>();
        public static TAtlasInfo TestAtlas = new TAtlasInfo();
        public static SpriteFont SysFont;
        public static SpriteFont Default;
        public static SpriteFont King;
        public static Vector2 WindowSize { get; private set; }
        private static Vector2? AsyncResUpdate = null;
        private static Vector2 s_resolution = new Vector2(1280, 720);
        public static Vector2 Resolution
        {
            get
            {
                return s_resolution;
            }
            set
            {
                s_resolution = value;
                DefaultShell.TrueDisplay = new RenderTarget2D(
                    DefaultShell.GraphicsDevice,
                    (int)s_resolution.X,
                    (int)s_resolution.Y,
                    false,
                    DefaultShell.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    DepthFormat.Depth24);
                if (!s_graphics.IsFullScreen)
                {
                    s_graphics.PreferredBackBufferWidth = (int)s_resolution.X;
                    s_graphics.PreferredBackBufferHeight = (int)s_resolution.Y;
                    WindowSize = s_resolution;
                    s_graphics.ApplyChanges();
                }
                if (AutoCamera != null)
                {
                    AutoCamera.RecenterPosition = s_resolution / 2;
                    AutoCamera.RecenterCamera();
                }
            }
        }
        public static Shell DefaultShell { get; set; }
        public Shell(String bootManifestFilePath, String bootManifestTitle)
        {
            _bootManifest = bootManifestFilePath;
            _bootManifestReadTitle = bootManifestTitle;
            DefaultShell = this;
            s_hasConsole = true;
            try { int window_height = Console.WindowHeight; }
            catch { s_hasConsole = false; }
            this.IsMouseVisible = true;
            s_graphics = new GraphicsDeviceManager(this);
            s_graphics.PreferHalfPixelOffset = true;
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
            ConsoleWritesOverlay = false;
            WriteLine("[SHELL INITIALIZED AT " + System.DateTime.Now.ToLongTimeString() + " " + System.DateTime.Now.ToShortDateString() + "]");
            WriteLine("Blackhole's eVent horizoN Framework");
            WriteLine("Version: " + FrameworkVersion);
            ActiveProcesses = new List<System.Diagnostics.Process>();
            s_graphics.HardwareModeSwitch = false;
            BackdropColour = Color.Black;
            Resolution = new Vector2(640, 480);
            this.IsMouseVisible = true;
            SaveLoadModule.InitializeAppFolders();
            LooseCamera = true;
            AutoCamera = new Camera("Default Shell Autocamera");
            UsingKeyboardInputs = null;
            Mute = false;
            GlobalVolume = 0.6f;
            PauseUpdates = false;
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
            s_flags = new Hashtable();
            s_flags.Add("SPOONSTRIP", ButtonScripts.SpoonsTrip);
            Sofia.InitSofiaFlags();
        }
        public static void UpdateFlag(String index, object value)
        {
            if (s_flags.ContainsKey(index.ToUpper())) { s_flags[index.ToUpper()] = value; }
            else { s_flags.Add(index.ToUpper(), value); }
            WriteLine("Flag updated: " + index + " set to " + value);
        }
        public static object ReadFlag(String index)
        {
            if (s_flags.ContainsKey(index.ToUpper())) { return s_flags[index.ToUpper()]; }
            else { return null; }
        }
        private static Boolean _mute = false;
        public static Boolean Mute
        {
            get
            {
                return _mute;
            }
            set
            {
                _mute = value;
                if(_mute)
                {
                    if(!(CaptureVolume is null))
                    {
                        CaptureVolume.ForceState(0f);
                        CaptureVolume.Enabled = false;
                    }
                    MediaPlayer.IsMuted = true;
                    foreach(SoundEffectInstance sfxInstance in ActiveSounds)
                    {
                        sfxInstance.Volume = 0f;
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
                    foreach (SoundEffectInstance sfxInstance in ActiveSounds)
                    {
                        sfxInstance.Volume = GlobalVolume;
                    }
                }
            }
        }
        public static Boolean PlaySoundInstant(String sfxIndex)
        {
            return PlaySoundInstant(sfxIndex, false, 0f);
        }
        public static Boolean PlaySoundInstant(String sfxIndex, Boolean loop)
        {
            return PlaySoundInstant(sfxIndex, loop, 0f);
        }
        public static Boolean PlaySoundInstant(String sfxIndex, Boolean loop, float pitch)
        {
            if(Mute | !SFXDirectory.ContainsKey(sfxIndex.ToUpper())) { return false; }
            SoundEffectInstance LocalSound = ((SoundEffect)SFXDirectory[sfxIndex.ToUpper()]).CreateInstance();
            LocalSound.Volume = GlobalVolume;
            LocalSound.IsLooped = loop;
            LocalSound.Pitch = pitch;
            LocalSound.Play();
            ActiveSounds.Add(LocalSound);
            return true;
        }
        private static float s_globalVolume = 1f;
        public static float GlobalVolume
        {
            get
            {
                return s_globalVolume;
            }
            set
            {
                s_globalVolume = value;
                MediaPlayer.Volume = s_globalVolume;
                foreach (SoundEffectInstance sfxInstance in ActiveSounds)
                {
                    sfxInstance.Volume = s_globalVolume;
                }
            }
        }
        public static void QueueInstantTrack(Song song)
        {
            QueueInstantTrack(song, 60f);
        }
        public static void QueueInstantTrack(Song song, float fadeoutDivisor)
        {
            if (MediaPlayer.State == MediaState.Stopped) { MediaPlayer.Play(song); }
            else
            {
                s_queuedSong = song;
                s_fadeoutAmount = s_globalVolume / (fadeoutDivisor);
            }
        }
        public static void OneFadeout()
        {
            OneFadeout(60f);
        }
        public static void OneFadeout(float fadeoutDivisor)
        {
            if(MediaPlayer.State == MediaState.Stopped) { return; }
            s_queuedSong = null;
            s_fadeoutAmount = s_globalVolume / (fadeoutDivisor);
        }
        static float s_fadeoutAmount = -100;
        static Song s_queuedSong = null;
        public static TAtlasInfo ButtonAtlas = new TAtlasInfo();
        public static Texture2D TestLongRect;
        public static Dictionary<string, TAtlasInfo> AtlasDirectory = new Dictionary<string, TAtlasInfo>();
        public static Dictionary<string, SoundEffect> SFXDirectory = new Dictionary<string, SoundEffect>();
        public static Dictionary<string, Song> SongDirectory = new Dictionary<string, Song>();
        public static Dictionary<string, Tileset> TilesetDirectory = new Dictionary<string, Tileset>();
        public static List<SoundEffectInstance> ActiveSounds = new List<SoundEffectInstance>();
        public static Checkbox CaptureFullscreen = null;
        public static Slider CaptureTextrate = null;
        public static TextEntity CaptureRateDisplay = null;
        public static Checkbox CaptureMute = null;
        public static Checkbox CaptureSaveType = null;
        public static Slider CaptureVolume = null;
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();
        public float LoadPercentage { get; set; }
        public object AResLockObj = new object();
        public object LPLockObj = new object();
        String _bootManifest;
        String _bootManifestReadTitle;
        public String BootManifest
        {
            get { return _bootManifest; }
        }
        public String BootManifestReadTitle
        {
            get { return _bootManifestReadTitle; }
        }
        public static void AutoShift()
        {
            if (Shell.AllowEnter) { Shell.DoNextShifter = true; }
        }
        protected object[] AsyncLoad()
        {
            WriteLine("Preload complete, loading remaining content...");
            ScriptProcessor.ScriptCache = new Dictionary<string, object[]>();

            TestAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/LethalHexWalk1");
            TestAtlas.DivDimensions = new Point(1, 1);

            TAtlasInfo insertAtlas = new TAtlasInfo();
            insertAtlas.Atlas = Content.Load<Texture2D>("Textures/Logos/MatmutLogo");
            insertAtlas.DivDimensions = new Point(1, 1);
            AtlasDirectory.Add("MATMUTLOGO", insertAtlas);
            insertAtlas = new TAtlasInfo();
            insertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/MatmutBG");
            insertAtlas.DivDimensions = new Point(1, 1);
            AtlasDirectory.Add("MATMUTBG", insertAtlas);

            insertAtlas = new TAtlasInfo();
            insertAtlas.DivDimensions = new Point(1, 1);
            insertAtlas.SetManualSR(new Rectangle(new Point(0, 0), new Point(320, 180)));
            AtlasDirectory.Add("THUMBBLANK", insertAtlas);

            Dictionary<string, string> manifests = ManifestReader.ReadManifestFile(BootManifest);
            object[] resources = ManifestReader.ParseManifest((String)manifests[BootManifestReadTitle], this);
            String firstScript = "";
            Boolean runFirstAsUnique = true;
            WriteLine("Reading application metainfo...");
            if (((Dictionary<object, object>)resources[0]).ContainsKey("startatscript"))
            {
                firstScript = (String)((Dictionary<object, object>)resources[0])["startatscript"];
            }
            if (((Dictionary<object, object>)resources[0]).ContainsKey("useunique"))
            {
                runFirstAsUnique = (Boolean)((Dictionary<object, object>)resources[0])["useunique"];
            }
            if (((Dictionary<object, object>)resources[0]).ContainsKey("defaultresolution"))
            {
                Monitor.Enter(AResLockObj);
                AsyncResUpdate = (Vector2)((Dictionary<object, object>)resources[0])["defaultresolution"];
                Monitor.Exit(AResLockObj);
            }
            WriteLine("Ingesting utility scripts...");
            ManifestReader.IngestScriptFile("vnf_utils.esa");
            WriteLine("Ingesting application scripts...");
            int i = 0;
            foreach (String script in ((List<string>)resources[1]))
            {
                ManifestReader.IngestScriptFile(script);
                i++;
                Monitor.Enter(LPLockObj);
                LoadPercentage = (float)(0.95f + (0.05 * (i / ((List<string>)resources[1]).Count)));
                Monitor.Exit(LPLockObj);
            }
            WriteLine("Integrating loaded resources...");
            foreach (String key in ((Dictionary<object, SpriteFont>)resources[2]).Keys)
            {
                Fonts.Add(key, ((Dictionary<object, SpriteFont>)resources[2])[key]);
            }
            foreach (String key in ((Dictionary<object, SoundEffect>)resources[3]).Keys)
            {
                SFXDirectory.Add(key, ((Dictionary<object, SoundEffect>)resources[3])[key]);
            }
            foreach (String key in ((Dictionary<object, Song>)resources[4]).Keys)
            {
                SongDirectory.Add(key, ((Dictionary<object, Song>)resources[4])[key]);
            }
            foreach (String key in ((Dictionary<object, TAtlasInfo>)resources[5]).Keys)
            {
                AtlasDirectory.Add(key, ((Dictionary<object, TAtlasInfo>)resources[5])[key]);
            }
            foreach (String key in ((Dictionary<object, Tileset>)resources[6]).Keys)
            {
                TilesetDirectory.Add(key, ((Dictionary<object, Tileset>)resources[6])[key]);
            }
            List<string> adKeys = new List<string>();
            foreach (String key in AtlasDirectory.Keys)
            {
                adKeys.Add(key);
            }
            foreach (String key in adKeys)
            {
                TAtlasInfo copy = ((TAtlasInfo)AtlasDirectory[key]);
                copy.ReferenceHash = key;
                AtlasDirectory[key] = copy;
            }
            TestLongRect = Content.Load<Texture2D>("Textures/Entities/UI/Elements/TestLongRect");
            ButtonAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/ButtonDefault");
            ButtonAtlas.DivDimensions = new Point(2, 1);
            Monitor.Enter(LPLockObj);
            LoadPercentage = 1;
            Monitor.Exit(LPLockObj);
            WriteLine("Content load complete.");
            return new object[] { firstScript, runFirstAsUnique };
        }
        private WorldEntity _loadBar;
        public WorldEntity LoadBarObj
        {
            get
            {
                return _loadBar;
            }
        }
        private WorldEntity _loadCover;
        private TextEntity _loadText;
        protected override void LoadContent()
        {
            WriteLine("VNF is loading content...");
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TrueDisplay = new RenderTarget2D(
                GraphicsDevice,
                (int)Resolution.X,
                (int)Resolution.Y,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            SysFont = Content.Load<SpriteFont>("Fonts/SysFont");
            Default = Content.Load<SpriteFont>("Fonts/Default");
            King = Content.Load<SpriteFont>("Fonts/Zenda");

            TAtlasInfo barAtlas = new TAtlasInfo();
            barAtlas.Atlas = Content.Load<Texture2D>("Textures/Preload/LoadingBar");
            barAtlas.DivDimensions = new Point(1, 1);
            TAtlasInfo coverAtlas = new TAtlasInfo();
            coverAtlas.Atlas = Content.Load<Texture2D>("Textures/Preload/LoadingCover");
            coverAtlas.DivDimensions = new Point(1, 1);

            Vector2 assumedScreenSize = Resolution;

            _loadBar = new WorldEntity("LOADBAR", new Vector2((assumedScreenSize.X / 2) - 250, (assumedScreenSize.Y / 2) + 50), barAtlas, 0.5f);
            _loadCover = new WorldEntity("LOADCOVER", new Vector2((assumedScreenSize.X / 2) + 243, (assumedScreenSize.Y / 2) + 57), coverAtlas, 1f);
            _loadCover.SetManualOrigin(new Vector2(486, 0));
            _loadText = new TextEntity("LOADTEXT", "[F:SYSFONT]Loading content...", new Vector2((assumedScreenSize.X / 2) - 250, (assumedScreenSize.Y / 2) + 150), 1f);
            _loadText.BufferLength = 500;
            _loadText.TypeWrite = false;

            RenderQueue.Add(_loadBar);
            RenderQueue.Add(_loadCover);
            RenderQueue.Add(_loadText);

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
            foreach(System.Diagnostics.Process process in ActiveProcesses)
            {
                process.Kill();
                process.Close();
            }
            WriteLine("Client closing...");
            WriteLine("[SHELL EXITING AT " + System.DateTime.Now.ToLongTimeString() + " " + System.DateTime.Now.ToShortDateString() + "]");
            SaveLoadModule.WriteFinals();
        }

        [field: NonSerialized]
        public static event VoidDel MouseLeftClick;
        protected MouseState LastMouseState;
        protected KeyboardState LastKeyState;
        public static Boolean ExitOut = false;
        public static Boolean AllowEnter = true;
        public static Boolean DoNextShifter { get; set; }
        float lastCapturedVol = 0f;
        float lastCapturedText = 0f;
        public Queue LoadGraphicsQueue { get; set; }
        public static GameTime LastUpdateGameTime { get; set; }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            LastUpdateGameTime = gameTime;
            KeyboardState kCurrent = Keyboard.GetState();
            if (LoadOperation != null && !LoadOperation.IsCompleted)
            {
                try
                {
                    Monitor.Enter(LoadGraphicsQueue);
                    while (LoadGraphicsQueue.Count > 0)
                    {
                        Task graphTask = (Task)LoadGraphicsQueue.Dequeue();
                        graphTask.RunSynchronously();
                    }
                }
                finally { Monitor.Exit(LoadGraphicsQueue); }
                try
                {
                    Monitor.Enter(LPLockObj);
                    _loadCover.Scale(new Vector2((1 - LoadPercentage) - _loadCover.Size.X, 0));
                }
                finally { Monitor.Exit(LPLockObj); }
                try
                {
                    Monitor.Enter(AResLockObj);
                    if (AsyncResUpdate != null)
                    {
                        Resolution = (Vector2)AsyncResUpdate;
                        _loadBar.QuickMoveTo(new Vector2((Resolution.X / 2) - 250, (Resolution.Y / 2) + 50));
                        _loadText.QuickMoveTo(new Vector2((Resolution.X / 2) - 250, (Resolution.Y / 2) + 150));
                        AsyncResUpdate = null;
                    }
                }
                finally { Monitor.Exit(AResLockObj);}
                _loadText.Text = "[F:SYSFONT]" + LastLogLine;
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11)) { ToggleFullscreen(); }
                LastKeyState = kCurrent;
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
                    object[] loadResult = LoadOperation.GetAwaiter().GetResult();
                    String firstScript = (String)loadResult[0];
                    Boolean runFirstAsUnique = (Boolean)loadResult[1];
                    if (firstScript.Length > 0)
                    {
                        WriteLine("Priming first script.");
                        if (runFirstAsUnique)
                        {
                            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer("INTRO_SNIFFER_UNIQUE", ScriptProcessor.RetrieveScriptByName(firstScript), firstScript));
                        }
                        else
                        {
                            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer(firstScript + "_SNIFFER", ScriptProcessor.RetrieveScriptByName(firstScript), firstScript));
                        }
                    }
                    _loadBar.AnimationQueue.Add(Animation.Retrieve("FADEOUT"));
                    _loadBar.TransientAnimation = true;
                    UpdateQueue.Add(_loadBar);
                    DeleteQueue.Add(_loadCover);
                    DeleteQueue.Add(_loadText);
                    LoadOperation.Dispose();
                    LoadOperation = null;
                    UpdateCycleStarted = true;
                }
                MainUpdate(gameTime, kCurrent);
            }
            base.Update(gameTime);
        }
        public static WorldEntity UsingKeyboardInputs { get; set; }
        public static Boolean ConsoleOpen { get; set; }
        public static Color BackdropColour { get; set; }
        public Boolean PauseUpdates { get; set; }
        public Boolean UpdateCycleStarted { get; set; }
        public static List<System.Diagnostics.Process> ActiveProcesses { get; set; }
        Queue<String> _systemTextQueue = new Queue<string>();
        public static void RequestDisplaySystemText(String message)
        {
            if(!DefaultShell.UpdateCycleStarted || message == "") { return; }
            DefaultShell._systemTextQueue.Enqueue(message);
        }
        //Double lastTotalKE = 0d;
        WorldEntity _preConsoleUsingKeyboard = null;
        protected void MainUpdate(GameTime gameTime, KeyboardState kCurrent)
        {
            if (this.IsActive)
            {
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    if (ScriptProcessor.ActiveGame())
                    {
                        if (!ButtonScripts.Paused) { ButtonScripts.Pause(); }
                        else { ButtonScripts.Unpause(); }
                    }
                    else { ButtonScripts.Quit(); }
                }
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemTilde) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemTilde))
                {
                    if (!ConsoleOpen)
                    {
                        _preConsoleUsingKeyboard = UsingKeyboardInputs;
                        ButtonScripts.OpenAndConstructConsole();
                        ConsoleOpen = true;
                    }
                    else
                    {
                        ButtonScripts.CloseConsole();
                        UsingKeyboardInputs = _preConsoleUsingKeyboard;
                        ConsoleOpen = false;
                    }
                }
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H) && UsingKeyboardInputs is null && ScriptProcessor.ActiveGame())
                {
                    ButtonScripts.RefreshUIHideState();
                }
                if (AutoCamera != null && LooseCamera)
                {
                    if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R) && UsingKeyboardInputs is null)
                    {
                        AutoCamera.RecenterCamera();
                        AutoCamera.ResetZoom();
                    }
                    AutoCamera.MouseDragEnabled = kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F) && UsingKeyboardInputs is null;
                    if (!UpdateQueue.Contains(AutoCamera)) { UpdateQueue.Add(AutoCamera); }
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || ExitOut)
                {
                    PythonController.SocketInterface.CloseAllSockets();
                    Exit();
                }
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11)) { ToggleFullscreen(); }
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2)) { PauseUpdates = !PauseUpdates; }
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) && UsingKeyboardInputs is null) { DynamicEntity.GlobalGravity = (float)Math.Round((DynamicEntity.GlobalGravity + 0.02) * 100) / 100f; }
                if (kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) && UsingKeyboardInputs is null) { DynamicEntity.GlobalGravity = (float)Math.Round((DynamicEntity.GlobalGravity - 0.02) * 100) / 100f; }
            }
            if (((kCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) && !LastKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) && !ConsoleOpen && UsingKeyboardInputs is null && this.IsActive) || DoNextShifter) && AllowEnter)
            {
                DoNextShifter = false;
                Boolean found = false;
                foreach (WorldEntity worldEntity in UpdateQueue)
                {
                    if (worldEntity is TextEntity && worldEntity.Name == "TEXT_MAIN")
                    {
                        if (((TextEntity)worldEntity).WrittenAll()) { GlobalWorldState = "CONTINUE"; }
                        ((TextEntity)worldEntity).SkipWrite();
                        found = true;
                        break;
                    }
                }
                if (!found) { GlobalWorldState = "CONTINUE"; }
            }
            LastKeyState = kCurrent;
            int currentSystemMessageDisplayTotal = 0;
            while(_systemTextQueue.Count > 0)
            {
                TextEntity messageText = new TextEntity("SYSTEM_MESSAGE_TEXT", "[C:255-255-0-255]" + _systemTextQueue.Dequeue().Replace("[", "(").Replace("]", ")").Replace("\n", " /n "), new Vector2(10, 5 + (currentSystemMessageDisplayTotal * (Default.MeasureString(" ").Y + 5))), 1f);
                currentSystemMessageDisplayTotal++;
                messageText.BufferLength = 5000;
                messageText.CameraImmune = true;
                messageText.TransientAnimation = true;
                messageText.AnimationQueue.Add(Animation.Retrieve("FADEOUTLONG"));
                UpdateQueue.Add(messageText);
                RenderQueue.Add(messageText);
            }
            if (!PauseUpdates)
            {
                List<DynamicEntity> dynamicEntities = new List<DynamicEntity>();
                foreach (WorldEntity worldEntity in UpdateQueue)
                {
                    worldEntity.Update();
                    if(worldEntity is DynamicEntity) { dynamicEntities.Add((DynamicEntity)worldEntity); }
                }

                foreach (DynamicEntity dynamicEntity in dynamicEntities)
                {
                    dynamicEntity.CheckAndResolveCollisions();
                }
                
                /*
                Double totalKE = 0d;
                foreach (DynamicEntity dynamicEntity in dynamicEntities)
                {
                    totalKE += Math.Pow(dynamicEntity.Velocity.Length(), 2) * dynamicEntity.Mass;
                }
                
                //Console.WriteLine();
                if (Math.Round(totalKE) != Math.Round(lastTotalKE) && lastTotalKE > 0 && totalKE > 0)
                {
                    Console.WriteLine("Kinetic energy factor discrepancy! Went from " + lastTotalKE + " to " + totalKE + "! At " + Shell.DefaultShell.LastUpdateGameTime.TotalGameTime);
                    Console.WriteLine();
                }
                else
                {
                    //Console.WriteLine("No discrepancy! At " + Shell.DefaultShell.LastUpdateGameTime.TotalGameTime);
                    //Console.WriteLine();
                }
                lastTotalKE = totalKE;*/
            }
            foreach (WorldEntity worldEntity in DeleteQueue)
            {
                if (UpdateQueue.Contains(worldEntity)) { UpdateQueue.Remove(worldEntity); }
                if (RenderQueue.Contains(worldEntity)) { RenderQueue.Remove(worldEntity); }
                if (NonSerializables.Contains(worldEntity)) { NonSerializables.Remove(worldEntity); }
                worldEntity.ManualDispose();
            }
            DeleteQueue = new List<WorldEntity>();
            foreach (VoidDel runVoid in RunQueue)
            {
                runVoid();
            }
            RunQueue = new List<VoidDel>();
            for(int i = 0; i < ActiveProcesses.Count; i++)
            {
                System.Diagnostics.Process process = ActiveProcesses[i];
                if (process.HasExited)
                {
                    process.Close();
                    ActiveProcesses.RemoveAt(i);
                    i--;
                }
            }
            if(s_fadeoutAmount != -100)
            {
                if(MediaPlayer.Volume > 0)
                {
                    MediaPlayer.Volume -= s_fadeoutAmount;
                }
                else
                {
                    MediaPlayer.Volume = GlobalVolume;
                    if (s_queuedSong != null) { MediaPlayer.Play(s_queuedSong); }
                    else { MediaPlayer.Stop(); }
                    s_queuedSong = null;
                    s_fadeoutAmount = -100;
                }
            }
            List<SoundEffectInstance> removeSounds = new List<SoundEffectInstance>();
            if (!(CaptureVolume is null) && CaptureVolume.Enabled)
            {
                if (CaptureVolume.Output() != lastCapturedVol)
                {
                    GlobalVolume = CaptureVolume.Output();
                }
                lastCapturedVol = CaptureVolume.Output();
            }
            foreach (SoundEffectInstance sound in ActiveSounds)
            {
                if(sound.State == SoundState.Stopped) { removeSounds.Add(sound); }
            }
            foreach(SoundEffectInstance removeSound in removeSounds)
            {
                ActiveSounds.Remove(removeSound);
            }
            if (!(CaptureTextrate is null) && CaptureTextrate.Enabled)
            {
                if (CaptureTextrate.Output() != lastCapturedText)
                {
                    TextEntity.TickWriteInterval = TextEntity.GetTicksFromSliderValue(CaptureTextrate.Output());
                    if(!(CaptureRateDisplay is null))
                    {
                        CaptureRateDisplay.Text = TextEntity.TickWriteInterval + (TextEntity.TickWriteInterval != 1 ? " milliseconds" : " millisecond");
                        CaptureRateDisplay.ReWrite();
                    }
                }
                lastCapturedText = CaptureTextrate.Output();
            }
            MouseState currentMouseState = Mouse.GetState();
            if(this.IsActive && currentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && LastMouseState.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed && MouseLeftClick != null) { MouseLeftClick(); }
            LastMouseState = currentMouseState;
            if (GlobalVoid != null)
            {
                GlobalVoid();
                GlobalVoid = null;
            }
        }

        public static Boolean HoldRender = false;
        public static void DoRenderOperation(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, String[] excludeEnts)
        {
            if (AutoCamera == null)
            {
                foreach (WorldEntity worldEntity in RenderQueue)
                {
                    if (worldEntity.Drawable && !excludeEnts.Contains(worldEntity.Name))
                    {
                        if (worldEntity.CustomCamera != null) { worldEntity.Draw(spriteBatch, worldEntity.CustomCamera); }
                        else { worldEntity.Draw(spriteBatch); }
                        foreach (VertexRenderable ivr in worldEntity.MyVertexRenderables)
                        {
                            if (worldEntity.CustomCamera != null) { ivr.DrawVertices(graphicsDevice, worldEntity.CustomCamera, ivr.AlignToEntity ? worldEntity : null); }
                            else { ivr.DrawVertices(graphicsDevice, ivr.AlignToEntity ? worldEntity : null); }
                        }
                    }
                }
            }
            else
            {
                foreach (WorldEntity worldEntity in RenderQueue)
                {
                    if (worldEntity.Drawable && !excludeEnts.Contains(worldEntity.Name))
                    {
                        if (worldEntity.CustomCamera != null) { worldEntity.Draw(spriteBatch, worldEntity.CustomCamera); }
                        else { worldEntity.Draw(spriteBatch, AutoCamera); }
                        foreach (VertexRenderable ivr in worldEntity.MyVertexRenderables)
                        {
                            if (worldEntity.CustomCamera != null) { ivr.DrawVertices(graphicsDevice, worldEntity.CustomCamera, ivr.AlignToEntity ? worldEntity : null); }
                            else { ivr.DrawVertices(graphicsDevice, AutoCamera, ivr.AlignToEntity ? worldEntity : null); }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            foreach(WorldEntity worldEntity in RenderQueue)
            {
                if(!(worldEntity is Pane || worldEntity is VerticalScrollPane || worldEntity is TileRenderer.TileDisplay)) { continue; }
                else if (worldEntity is Pane && ((Pane)worldEntity).RenderAlways) { ((Pane)worldEntity).Render(); }
                else if (worldEntity is VerticalScrollPane && ((VerticalScrollPane)worldEntity).AssociatedPane.RenderAlways) { ((VerticalScrollPane)worldEntity).AssociatedPane.Render(); }
                else if (worldEntity is TileRenderer.TileDisplay && ((TileRenderer.TileDisplay)worldEntity).NeedUpdateRender) { ((TileRenderer.TileDisplay)worldEntity).DoRenderUpdate(); }
            }
            GraphicsDevice.SetRenderTarget(TrueDisplay);
            GraphicsDevice.Clear(BackdropColour);
            ShellSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            if (!HoldRender)
            {
                DoRenderOperation(GraphicsDevice, ShellSpriteBatch, new string[0]);
            }
            ShellSpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            ShellSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            ShellSpriteBatch.Draw(TrueDisplay, new Rectangle(0, 0, (int)WindowSize.X, (int)WindowSize.Y), Color.White);
            ShellSpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
