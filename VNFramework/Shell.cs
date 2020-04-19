using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
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

/*VNF GENERAL TO-DO:
 * Sub-frames to render to
 * Construct objects from string params
 * Read all scripts from file with above
 * Inbuilt console
 * Load entities from file
 * Make interactive UI elements fire events for simpler use
 * "Camera" objects that can be passed to WorldEntity.Draw() to allow scrolling perspectives
 * External resource manifests
 */

namespace VNFramework
{
    //Texture atlas info type
    [Serializable]
    public struct TAtlasInfo
    {
        [field: NonSerialized]
        public Texture2D Atlas;
        public Rectangle SourceRect;
        public Point DivDimensions;
        public Point FrameSize()
        {
            return new Point(SourceRect.Width / DivDimensions.X, SourceRect.Height / DivDimensions.Y);
        }
        public Hashtable FrameLookup;
        public String ReferenceHash;
    }
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
    public delegate void VoidDel();
    public class Shell : Game
    {
        public const String FrameworkVersion = "1.3_DEV";
        public static Random Rnd = new Random();
        static Hashtable Flags = new Hashtable();
        public static String GlobalWorldState = "DEFAULT";
        private static Boolean pHasConsole = false;
        public static Camera AutoCamera = null;
        public static Boolean LooseCamera { get; set; }
        public static Boolean HasConsole
        {
            get
            {
                return pHasConsole;
            }
        }
        public static Point ConvertVector(Vector2 V)
        {
            return new Point((int)V.X, (int)V.Y);
        }
        public static Vector2 ConvertPoint(Point P)
        {
            return new Vector2(P.X, P.Y);
        }
        public static Point PointMultiply(Point P, Vector2 V)
        {
            return new Point((int)(V.X * P.X), (int)(V.Y * P.Y));
        }
        public static Point PointMultiply(Point P, Point P2)
        {
            return new Point((int)(P2.X * P.X), (int)(P2.Y * P.Y));
        }
        public static double GetLinearDistance(Vector2 A, Vector2 B)
        {
            return Math.Sqrt((double)((B.X - A.X) * (B.X - A.X)) + ((B.Y - A.Y) * (B.Y - A.Y)));
        }
        public static GraphicsDevice PubGD;
        //Function to extract texture objects defined by rectangles from a larger spritesheet
        public Texture2D ExtractTexture(Texture2D Sheet, Rectangle Source)
        {
            return ExtractTexture(Sheet, Source, new Vector2(1, 1));
        }
        public Texture2D ExtractTexture(Texture2D Sheet, Rectangle Source, Vector2 Scaling)
        {
            RenderTarget2D Output = new RenderTarget2D(GraphicsDevice, (int)(Source.Width * Scaling.X), (int)(Source.Height * Scaling.Y), false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            GraphicsDevice.SetRenderTarget(Output);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(Sheet, new Rectangle(new Point(0, 0), new Point((int)(Source.Width * Scaling.X), (int)(Source.Height * Scaling.Y))), Source, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            Color[] texdata = new Color[Output.Width * Output.Height];
            Output.GetData(texdata);
            Texture2D NOut = new Texture2D(GraphicsDevice, Output.Width, Output.Height);
            NOut.SetData(texdata);
            return NOut;
        }
        public Texture2D CombineTextures(Point DestinationDims, Texture2D TextureA, Rectangle SourceA, Vector2 PositionA, Vector2 ScalingA, Texture2D TextureB, Rectangle SourceB, Vector2 PositionB, Vector2 ScalingB)
        {
            RenderTarget2D Output = new RenderTarget2D(GraphicsDevice, DestinationDims.X, DestinationDims.Y, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            GraphicsDevice.SetRenderTarget(Output);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(TextureA, new Rectangle(new Point((int)PositionA.X, (int)PositionA.Y), new Point((int)(SourceA.Width * ScalingA.X), (int)(SourceA.Height * ScalingA.Y))), SourceA, Color.White);
            spriteBatch.Draw(TextureB, new Rectangle(new Point((int)PositionB.X, (int)PositionB.Y), new Point((int)(SourceB.Width * ScalingB.X), (int)(SourceB.Height * ScalingB.Y))), SourceB, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            Color[] texdata = new Color[Output.Width * Output.Height];
            Output.GetData(texdata);
            Texture2D NOut = new Texture2D(GraphicsDevice, Output.Width, Output.Height);
            NOut.SetData(texdata);
            return NOut;
        }
        public Texture2D GetNovelTextureOfColour(Color Colour, Point Dims)
        {
            RenderTarget2D Output = new RenderTarget2D(GraphicsDevice, Dims.X, Dims.Y, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            GraphicsDevice.SetRenderTarget(Output);
            GraphicsDevice.Clear(Colour);
            GraphicsDevice.SetRenderTarget(null);
            Color[] texdata = new Color[Output.Width * Output.Height];
            Output.GetData(texdata);
            Texture2D NOut = new Texture2D(GraphicsDevice, Output.Width, Output.Height);
            NOut.SetData(texdata);
            return NOut;
        }
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
        public static Texture2D GetFromRT(RenderTarget2D In)
        {
            Texture2D Out = new Texture2D(PubGD, In.Width, In.Height);
            Color[] texdata = new Color[Out.Width * Out.Height];
            In.GetData(texdata);
            Out.SetData(texdata);
            return Out;
        }
        static GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Boolean Ser = false;
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
            ButtonScripts.UnHideUI();
        }
        public static VoidDel GlobalVoid = null;
        public static void WriteLine(String Text)
        {
            InternalLog += Text + "\n";
            if(pHasConsole) { Console.WriteLine(Text); }
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
        static Boolean Fullscreen = false;
        public static TAtlasInfo TestAtlas = new TAtlasInfo();
        public static SpriteFont Default;
        public static SpriteFont King;
        public static Vector2 ScreenSize = new Vector2(1280, 720);
        public static Shell DefaultShell { get; set; }
        public Shell()
        {
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
            LooseCamera = true;
            AutoCamera = new Camera("Default Shell Autocamera");
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            WriteLine("[SHELL INITIALIZED AT " + System.DateTime.Now.ToShortTimeString() + " " + System.DateTime.Now.ToShortDateString() + "]");
            WriteLine("Blackhole's Event Horizon Framework (Visual novel implementation): Version " + FrameworkVersion);
            SaveLoadModule.InitializeAppFolders();
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
        WorldEntity TestObj;
        Button TestButton;
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
        protected override void LoadContent()
        {
            WriteLine("VNF is loading content...");
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            TrueDisplay = new RenderTarget2D(
                GraphicsDevice,
                1280,
                720,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            TestAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/LethalHexWalk1");
            TestAtlas.DivDimensions = new Point(1, 1);
            TestAtlas.SourceRect = new Rectangle(new Point(0, 0), TestAtlas.Atlas.Bounds.Size);

            //Define fonts
            Default = Content.Load<SpriteFont>("Fonts/Default");
            King = Content.Load<SpriteFont>("Fonts/Zenda");
            Fonts.Add("DEFAULT", Default);
            Fonts.Add("KING", King);

            Fonts.Add("MACABRE", Content.Load<SpriteFont>("Fonts/Abaddon"));

            //Define SFX
            SFXDirectory.Add("TYPE_1", Content.Load<SoundEffect>("Audio/SFX/Typewriter_1"));
            SFXDirectory.Add("TYPE_2", Content.Load<SoundEffect>("Audio/SFX/Typewriter_2"));
            SFXDirectory.Add("TYPE_3", Content.Load<SoundEffect>("Audio/SFX/Typewriter_3"));
            SFXDirectory.Add("YAY", Content.Load<SoundEffect>("Audio/SFX/Humans_yay"));
            SFXDirectory.Add("LASERBUILD", Content.Load<SoundEffect>("Audio/SFX/ChargedLaserBlast_A"));
            SFXDirectory.Add("LASERBLAST", Content.Load<SoundEffect>("Audio/SFX/ChargedLaserBlast_B"));
            SFXDirectory.Add("DEEPBOOM", Content.Load<SoundEffect>("Audio/SFX/Deep_Boom_1"));
            SFXDirectory.Add("BHLOGO", Content.Load<SoundEffect>("Audio/UI/Logos/BH_Chip_Short_RevIntro"));
            SFXDirectory.Add("UT_SAVE", Content.Load<SoundEffect>("Audio/UI/Responsive/UT_Save"));
            SFXDirectory.Add("SOFIA_I", Content.Load<SoundEffect>("Audio/SFX/Sofia_I"));
            SFXDirectory.Add("SOFIA_AM", Content.Load<SoundEffect>("Audio/SFX/Sofia_Am"));
            SFXDirectory.Add("SOFIA_SOFIA", Content.Load<SoundEffect>("Audio/SFX/Sofia_Sofia"));
            SFXDirectory.Add("SOFIA_HOH", Content.Load<SoundEffect>("Audio/SFX/Sofia_HOH"));
            SFXDirectory.Add("R_CYMBAL", Content.Load<SoundEffect>("Audio/SFX/Reverse_Cymbal"));

            //Define songs
            SongDirectory.Add("NIGHTFLIER", Content.Load<Song>("Audio/Music/Nightflier_1"));
            SongDirectory.Add("SPOONS", Content.Load<Song>("Audio/Music/Incompetech_Spoons"));
            SongDirectory.Add("DEUCES", Content.Load<Song>("Audio/Music/Incompetech_Deuces"));
            SongDirectory.Add("AMBIMENT", Content.Load<Song>("Audio/Music/Incompetech_Ambiment"));
            SongDirectory.Add("MIRAGE", Content.Load<Song>("Audio/Music/Incompetech_Mirage"));
            SongDirectory.Add("SPIDER", Content.Load<Song>("Audio/Music/Incompetech_Spider"));
            SongDirectory.Add("DARKLING", Content.Load<Song>("Audio/Music/A Darkling Plain"));
            SongDirectory.Add("KING", Content.Load<Song>("Audio/Music/Banner of a King"));
            SongDirectory.Add("CRIMINAL", Content.Load<Song>("Audio/Music/Criminal Intent"));
            SongDirectory.Add("SOURCE", Content.Load<Song>("Audio/Music/SOURCE"));
            SongDirectory.Add("CREDITS", Content.Load<Song>("Audio/Music/Sofia's Theme"));
            SongDirectory.Add("LEGEND", Content.Load<Song>("Audio/Music/Another Legend"));
            SongDirectory.Add("MEDLEY", Content.Load<Song>("Audio/Music/Medley of ULTRASOFIAWORLD"));
            SongDirectory.Add("ORDINARY", Content.Load<Song>("Audio/Music/An Ordinary Day in the Life of Sofia"));
            SongDirectory.Add("GODHEAD", Content.Load<Song>("Audio/Music/Godhead Approaching (Loopready)"));
            SongDirectory.Add("BATTLE", Content.Load<Song>("Audio/Music/Battle Against SOFIA (endcut)"));
            SongDirectory.Add("EPILOGUE", Content.Load<Song>("Audio/Music/Battle-Epilogue"));
            SongDirectory.Add("QUINTESSENCE", Content.Load<Song>("Audio/Music/Quintessence"));

            SongDirectory.Add("PORTAL", Content.Load<Song>("Audio/SFX/Ambient/PortalBloops_WithFade"));
            SongDirectory.Add("DEEP1", Content.Load<Song>("Audio/SFX/Ambient/Deep_Rumble_1_Lpr"));
            SongDirectory.Add("DEEP2", Content.Load<Song>("Audio/SFX/Ambient/Deep_Rumble_2_Lpr"));
            SongDirectory.Add("DEEP3", Content.Load<Song>("Audio/SFX/Ambient/Deep_Rumble_3_Lpr"));
            SongDirectory.Add("BIRDS", Content.Load<Song>("Audio/SFX/Ambient/Ambient_Birds"));

            //Define texture atlas details...
            TAtlasInfo InsertAtlas = new TAtlasInfo();
            //InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/VineTB");
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/TextBox_Dark");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("UIBOX", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            //InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/VineTB");
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/UI_NameBacking_5");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("NAMEBACKING", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            //InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/VineSettings");
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/Settings_Dark");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SETTINGSPANE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Elements/SofiaWorldTitle");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIAWORLD", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/Purple_Spot");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PURPLE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/SofiaLetter");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("LETTER", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/SofiaCrimeShack");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CRIMESHACK", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Effects/Env_Portal");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PORTAL", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/JudgingGlowBase");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("ESSENCEGLOW", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/BigJudgingGlow");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("BIGJUDGINGGLOW", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Effects/Env_BGFlasher");
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("BGFLASHER", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Effects/Env_WhiteoutGradient");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("WHITEGRADIENT", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Effects/Env_Source");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOURCE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Effects/Env_Runes");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("RUNEGLOW", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Effects/Env_Splash_Glow");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SPLASHGLOW", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/TestBackground");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("TESTBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/TempBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("TEMPBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/BlackBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("BLACK", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/StarscapeBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("STARBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/HouseBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("HOUSEBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/CellarBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CELLARBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/CastleExteriorBG_Resize");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CASTLEEXTBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/CastleInteriorBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CASTLEINTBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/SourceCaveBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOURCECAVEBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/SofiaWorldBlur_Sizemod");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SWBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/CrookedCaveBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CROOKEDCAVEBG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Logos/BMS");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("BMS", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Logos/Presenting");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PRESENTING", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Logos/MatmutLogo");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MATMUTLOGO", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Backdrops/MatmutBG");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MATMUTBG", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_1_Unformed_Base");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY1_BASE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_1_Unformed_Lightning");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY1_BOLTS", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_2_Meteors");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY2", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_3_Stars");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY3", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_4_Formation");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY4", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_5_Lineup");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY5", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_6_Handprint");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY6", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Environment/Scenes/Mystic/Story_7_Fadingprint");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MYSTICSTORY7", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/FinalLogo");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_LOGO", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/Continued");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_CONTINUED", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/NotContinued");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_NOTCONTINUED", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/CreditRoll");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_ROLL", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/Starring");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_STARRING", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsHerself");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_HERSELF", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsCool");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_COOL", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsGolem");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_GOLEM", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsKing");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_KING", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsCrooked");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_CROOKED", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsMystic");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_MYSTIC", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/AsBig");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_BIG", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/LetterAs");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_LETTER", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/PortalAs");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_PORTAL", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/MirandaHead");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_MIRANDA", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/Production");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_PRODUCTION", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/MyCredits");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_MYCREDITS", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/Soundtrack");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_OST", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/FrameworkAndLogos");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_LOGOS", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Credits/HappyBirthday");
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SOFIA_CREDITS_BIRTHDAY", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            //InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/SaveSlotBase");
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/SaveSlotBase_Dark");
            InsertAtlas.DivDimensions = new Point(3, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SAVESLOT", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/RestoreDefaults");
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("RESTOREBUTTON", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/SofiaSprites");
            InsertAtlas.DivDimensions = new Point(4, 3);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            Hashtable FrameLookup = new Hashtable();
            FrameLookup.Add("EXCITED", new Point(0, 0));
            FrameLookup.Add("HAPPY", new Point(1, 0));
            FrameLookup.Add("CONSIDERING", new Point(2, 0));
            FrameLookup.Add("UNIMPRESSED", new Point(3, 0));
            FrameLookup.Add("GRINNING", new Point(0, 1));
            FrameLookup.Add("JUDGING", new Point(1, 1));
            FrameLookup.Add("DOWNCAST", new Point(2, 1));
            FrameLookup.Add("LAUGHING", new Point(3, 1));
            FrameLookup.Add("WORRIED", new Point(0, 2));
            FrameLookup.Add("THINKING", new Point(1, 2));
            InsertAtlas.FrameLookup = FrameLookup;
            AtlasDirectory.Add("SOFIASPRITES", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/BigSofiaHeads");
            InsertAtlas.DivDimensions = new Point(4, 3);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("BIGSOFIA", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/CoolSofia");
            InsertAtlas.DivDimensions = new Point(4, 3);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            InsertAtlas.FrameLookup = FrameLookup;
            AtlasDirectory.Add("COOLSOFIA", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/KingSofia");
            InsertAtlas.DivDimensions = new Point(4, 3);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            InsertAtlas.FrameLookup = FrameLookup;
            AtlasDirectory.Add("KINGSOFIA", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/CrookedSofia");
            InsertAtlas.DivDimensions = new Point(4, 3);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            InsertAtlas.FrameLookup = FrameLookup;
            AtlasDirectory.Add("CROOKEDSOFIA", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/MysticSofia");
            InsertAtlas.DivDimensions = new Point(4, 3);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            InsertAtlas.FrameLookup = FrameLookup;
            AtlasDirectory.Add("MYSTICSOFIA", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/GolemSofia");
            InsertAtlas.DivDimensions = new Point(3, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            FrameLookup = new Hashtable();
            FrameLookup.Add("GRINNING", new Point(0, 0));
            FrameLookup.Add("STARE", new Point(1, 0));
            FrameLookup.Add("PLACID", new Point(2, 0));
            InsertAtlas.FrameLookup = FrameLookup;
            AtlasDirectory.Add("GOLEMSOFIA", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/Sprites/SofiaMemes");
            InsertAtlas.DivDimensions = new Point(4, 4);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MEMES", InsertAtlas);

            InsertAtlas = new TAtlasInfo();
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), new Point(320, 180));
            AtlasDirectory.Add("THUMBBLANK", InsertAtlas);

            //Texture2D MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/MainMenuUIButtons");
            Texture2D MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/MainMenuUIButtons_Dark");
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 0, 500, 140));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PLAYBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 140, 500, 140));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("LOADBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 280, 500, 140));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CREDITSBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 420, 500, 140));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SETTINGSBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 560, 250, 70));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("BACKBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 630, 250, 70));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("NEXTBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 700, 250, 70));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PREVBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(250, 560, 75, 70));
            InsertAtlas.DivDimensions = new Point(3, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SCROLLBAR", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(250, 642, 144, 48));
            InsertAtlas.DivDimensions = new Point(3, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SLIDERKNOB", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(250, 690, 100, 50));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("MAPBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(325, 560, 82, 41));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSEMENUBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(407, 560, 82, 41));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("RETURNBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(325, 601, 82, 41));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SKIPBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(407, 601, 82, 41));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("ARCHIVEBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(407, 642, 82, 82));
            InsertAtlas.DivDimensions = new Point(2, 2);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("CHECKBOX", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 855, 104, 102));
            InsertAtlas.DivDimensions = new Point(2, 2);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("EYECHECKBOX", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 770, 500, 70));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("QUITBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 840, 500, 15));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SLIDERBAR", InsertAtlas);

            MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/CrimeQuizImages");
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 0, 600, 400));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("HACKER_IMAGEBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 400, 600, 400));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PIRATE_IMAGEBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 800, 600, 400));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("THIEF_IMAGEBUTTON", InsertAtlas);

            //MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/PauseMenuActual");
            MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/PauseMenuActual_Dark");
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 0, 460, 500));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSEMENUPANE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(460, 0, 880, 80));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSERETURNBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(460, 80, 880, 80));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSESAVEBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(460, 160, 880, 80));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSESETTINGSBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(460, 240, 880, 80));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSEMAINMENUBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(460, 320, 880, 80));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("PAUSEQUITBUTTON", InsertAtlas);

            //MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/WriteSaveUI");
            MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/WriteSaveUI_Dark");
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 0, 560, 500));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SAVEPANE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 500, 250, 70));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("YESBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 570, 250, 70));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("NOBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(560, 0, 400, 300));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SAVEWRITTENPANE", InsertAtlas);

            MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/DeleteSaveUI_Dark");
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 0, 560, 500));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("DELETEPANE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 500, 640, 180));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("DELETESAVEBUTTON", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(560, 0, 400, 300));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("SAVEDELETEDPANE", InsertAtlas);

            MainUISource = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/ExitDialogue");
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 0, 400, 249));
            InsertAtlas.DivDimensions = new Point(1, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("EXITPANE", InsertAtlas);
            InsertAtlas = new TAtlasInfo();
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 249, 202, 54));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("QUITYESBUTTON", InsertAtlas);
            InsertAtlas.Atlas = ExtractTexture(MainUISource, new Rectangle(0, 303, 148, 54));
            InsertAtlas.DivDimensions = new Point(2, 1);
            InsertAtlas.SourceRect = new Rectangle(new Point(0, 0), InsertAtlas.Atlas.Bounds.Size);
            AtlasDirectory.Add("QUITNOBUTTON", InsertAtlas);

            ArrayList ADKeys = new ArrayList();
            foreach(String K in AtlasDirectory.Keys)
            {
                ADKeys.Add(K);
            }
            foreach(String K in ADKeys)
            {
                TAtlasInfo Copy = ((TAtlasInfo)AtlasDirectory[K]);
                Copy.ReferenceHash = K;
                AtlasDirectory[K] = Copy;
            }
            TestLongRect = Content.Load<Texture2D>("Textures/Entities/UI/Elements/TestLongRect");
            ButtonAtlas.Atlas = Content.Load<Texture2D>("Textures/Entities/UI/Buttons/ButtonDefault");
            ButtonAtlas.DivDimensions = new Point(2, 1);
            ButtonAtlas.SourceRect = new Rectangle(new Point(0, 0), ButtonAtlas.Atlas.Bounds.Size);
            /*TextEntity Test = new TextEntity("Test", "[F:MACABRE]Line 1 GGGGGGGGGGGGGGGGGGGGGGGGG[F:MACABRE]Placing some of your essence into it should kickstart a reaction that will reverse the decay that our world has been experiencing.", new Vector2(50, 50), 1f);
            //TextEntity Test = new TextEntity("Test", ((SpriteFont)Fonts["MACABRE"]).MeasureString(" ").ToString(), new Vector2(50, 50), 1f);
            Shell.UpdateQueue.Add(Test);
            Shell.RenderQueue.Add(Test);*/
            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer("INTRO_SNIFFER_UNIQUE", ScriptProcessor.RetrieveScriptByName("INTRO_PRELOAD"), "INTRO_PRELOAD"));
            WriteLine("Content load complete.");
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            WriteLine("Client closing...");
            WriteLine("[SHELL EXITING AT " + System.DateTime.Now.ToShortTimeString() + " " + System.DateTime.Now.ToShortDateString() + "]");
            SaveLoadModule.WriteFinals();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        [field: NonSerialized]
        public static event VoidDel MouseLeftClick;
        MouseState LastMouseState;
        KeyboardState LastKeyState;
        public static Boolean ExitOut = false;
        public static Boolean AllowEnter = true;
        public static Boolean DoNextShifter { get; set; }
        float LastCapturedVol = 0f;
        float LastCapturedText = 0f;
        protected override void Update(GameTime gameTime)
        {
            KeyboardState KCurrent = Keyboard.GetState();
            if (KCurrent.IsKeyDown(Keys.Escape) && !LastKeyState.IsKeyDown(Keys.Escape))
            {
                if (ScriptProcessor.ActiveGame())
                {
                    if (!ButtonScripts.Paused) { ButtonScripts.Pause(); }
                    else { ButtonScripts.Unpause(); }
                }
                else { ButtonScripts.Quit(); }
            }
            if (KCurrent.IsKeyDown(Keys.H) && !LastKeyState.IsKeyDown(Keys.H) && ScriptProcessor.ActiveGame())
            {
                ButtonScripts.RefreshUIHideState();
            }
            if (AutoCamera != null && LooseCamera)
            {
                if (KCurrent.IsKeyDown(Keys.R) && !LastKeyState.IsKeyDown(Keys.R))
                {
                    AutoCamera.CenterDefault();
                    AutoCamera.ResetZoom();
                }
                AutoCamera.MouseDragEnabled = KCurrent.IsKeyDown(Keys.F);
                if(!UpdateQueue.Contains(AutoCamera)) { UpdateQueue.Add(AutoCamera); }
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ExitOut)
            {
                Exit();
            }
            if (((KCurrent.IsKeyDown(Keys.Enter) && !LastKeyState.IsKeyDown(Keys.Enter)) || DoNextShifter) && AllowEnter)
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
            if (KCurrent.IsKeyDown(Keys.F11) && !LastKeyState.IsKeyDown(Keys.F11)) { ToggleFullscreen(); }
            LastKeyState = KCurrent;
            foreach (WorldEntity E in UpdateQueue)
            {
                E.Update();
            }
            foreach (WorldEntity E in DeleteQueue)
            {
                if (E.Clickable) { MouseLeftClick -= E.MLCOut; }
                if (UpdateQueue.Contains(E)) { UpdateQueue.Remove(E); }
                if (RenderQueue.Contains(E)) { RenderQueue.Remove(E); }
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
            if(Current.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton != ButtonState.Pressed && MouseLeftClick != null) { MouseLeftClick(); }
            LastMouseState = Current;
            if (GlobalVoid != null)
            {
                GlobalVoid();
                GlobalVoid = null;
            }
            base.Update(gameTime);
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
            //spriteBatch.Draw(((TAtlasInfo)AtlasDirectory["TESTBG"]).Atlas, new Rectangle(0, 0, 1280, 720), new Rectangle(0, 0, 1280, 720), new Color(0,255,255,255), 0f, new Vector2(), SpriteEffects.None, 0f);
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
