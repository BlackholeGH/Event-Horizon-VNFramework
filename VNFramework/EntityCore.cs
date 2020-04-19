using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Runtime.Serialization;

namespace VNFramework
{
    
    /// <summary>
    /// The base entity class for all objects that can be added to the VNF game environment, and which function with the Shell's update and render queues.
    /// </summary>
    [Serializable]
    public class WorldEntity
    {
        public static ulong IDIterator = 0;
        protected ulong pEntityID;
        [field: NonSerialized]
        protected int Updatetime = Environment.TickCount;
        protected String pName;
        protected Boolean pDrawable = true;
        protected TAtlasInfo LocalAtlas;
        protected Vector2 pDrawCoords;
        protected Point AtlasCoordinates = new Point(0, 0);
        protected Boolean pClickable = false;
        protected float pRotation = 0f;
        public float RotationRads { get { return pRotation; } }
        protected Vector2 pScale = new Vector2(1, 1);
        public Vector2 ScaleSize { get { return pScale; } }
        public Boolean Clickable { get { return pClickable; } }
        protected Vector2 pOrigin = new Vector2();
        protected Boolean pCO = false;
        protected ColourShift pColour = new ColourShift(255f,255f,255f,255f);
        public Color ColourValue
        {
            get
            {
                return new Color((byte)Math.Round(pColour.R), (byte)Math.Round(pColour.G), (byte)Math.Round(pColour.B), (byte)Math.Round(pColour.A));
            }
            set
            {
                Color C = value;
                pColour = ColourShift.Constrain(new ColourShift(C.R, C.G, C.B, C.A));
            }
        }
        /// <summary>
        /// Represents a custom Camera entity that can be assigned to render this WorldEntity.
        /// </summary>
        public Camera CustomCamera { get; set; }
        public Boolean CameraImmune { get; set; }
        public TAtlasInfo Atlas { get { return LocalAtlas; } }
        public Boolean SetAtlasFrame(Point Coords)
        {
            if(Coords.X < LocalAtlas.DivDimensions.X && Coords.Y < LocalAtlas.DivDimensions.Y)
            {
                AtlasCoordinates = Coords;
                return true;
            }
            return false;
        }
        public void SetManualOrigin(Vector2 V)
        {
            pOrigin = V;
        }
        public Boolean CenterOrigin
        {
            get
            {
                return pCO;
            }
            set
            {
                pCO = value;
                if(pCO == true)
                {
                    pOrigin = Shell.ConvertPoint(HitBox.Size) / 2;
                }
                else
                {
                    pOrigin = new Vector2();
                }
            }
        }
        public Boolean SuppressClickable { get; set; }
        public void GiveClickFunction(VoidDel Func)
        {
            pClickable = true;
            MLC = delegate () { if (MouseInBounds() && !SuppressClickable) { Func(); }  };
            Shell.MouseLeftClick += MLC;
        }
        private Texture2D SerializationBackup = null;
        public virtual void OnSerializeDo()
        {
            foreach(Animation A in AnimationQueue) { A.TimeHang(); }
            if(LocalAtlas.ReferenceHash == "" || LocalAtlas.ReferenceHash == null)
            {
                SerializationBackup = LocalAtlas.Atlas;
            }
        }
        public virtual void OnDeserializeDo()
        {
            foreach (Animation A in AnimationQueue)
            {
                A.UnHang();
                if (A.Started && A.TimeElapsed > 100) { A.Jump(this); }
            }
            if (!(this is Button))
            {
                if(MLCRecord.Length > 0)
                {
                    GiveClickFunction(ButtonScripts.DelegateFetch(MLCRecord[0]));
                }
            }
            if(SerializationBackup != null)
            {
                LocalAtlas.Atlas = SerializationBackup;
                SerializationBackup = null;
            }
            else if(LocalAtlas.ReferenceHash != "" && LocalAtlas.ReferenceHash != null)
            {
                LocalAtlas.Atlas = ((TAtlasInfo)Shell.AtlasDirectory[LocalAtlas.ReferenceHash]).Atlas;
            }
        }
        protected Rectangle pHitBox = new Rectangle(0, 0, 0, 0);
        public Rectangle HitBox
        {
            get
            {
                Point Size = new Point((int)(LocalAtlas.FrameSize().X * pScale.X), (int)(LocalAtlas.FrameSize().Y * pScale.Y));
                if (!pCO) { pHitBox = new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y), Size); }
                else { pHitBox = new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y)-new Point(Size.X/2, Size.Y/2), Size); }
                return pHitBox;
            }
            set { pHitBox = value; }
        }
        public ArrayList AnimationQueue { get; set; }
        public Boolean Equals(WorldEntity B)
        {
            if(B is null) { return false; }
            if(B.EntityID == pEntityID) { return true; }
            else { return false; }
        }
        public static Boolean operator== (WorldEntity A, WorldEntity B)
        {
            if(A is null && B is null) { return true; }
            else if (A is null ^ B is null) { return false; }
            return A.Equals(B);
        }
        public static Boolean operator!= (WorldEntity A, WorldEntity B)
        {
            if (A is null && B is null) { return false; }
            else if (A is null ^ B is null) { return true; }
            return !A.Equals(B);
        }
        public virtual void MouseLeftClick()
        {
        
        }
        public Boolean MouseInBounds()
        {
            var MouseState = Mouse.GetState();
            //return HitBox.Contains(new Vector2(MouseState.X, MouseState.Y));
            Vector2 NormalizedMouseVector = Shell.CoordNormalize(new Vector2(MouseState.X, MouseState.Y));
            return TextureAwareInBounds(NormalizedMouseVector);
        }
        public Boolean TextureAwareInBounds(Vector2 V)
        {
            Vector2 ZoomFactor = new Vector2(1, 1);
            if (!CameraImmune)
            {
                if (CustomCamera != null)
                {
                    ZoomFactor = CustomCamera.ZoomFactor;
                    V = ((V / ZoomFactor) - (CustomCamera.OffsetVector));
                }
                else if (Shell.AutoCamera != null)
                {
                    ZoomFactor = Shell.AutoCamera.ZoomFactor;
                    V = ((V / ZoomFactor) - (Shell.AutoCamera.OffsetVector));
                }
            }
            if (HitBox.Contains(V))
            {
                Texture2D MyAtlas = Atlas.Atlas;
                Color[] RawAtlas = new Color[MyAtlas.Width * MyAtlas.Height];
                MyAtlas.GetData<Color>(RawAtlas);
                Color[,] OrderedAtlas = new Color[MyAtlas.Width, MyAtlas.Height];
                Point AddCoord = new Point(0, 0);
                foreach (Color C in RawAtlas)
                {
                    if (AddCoord.X >= MyAtlas.Width)
                    {
                        AddCoord = new Point(0, AddCoord.Y + 1);
                    }
                    OrderedAtlas[AddCoord.X, AddCoord.Y] = C;
                    AddCoord += new Point(1, 0);
                }
                Vector2 PublicCorner = new Vector2(HitBox.X, HitBox.Y);
                Vector2 LocalCoord = V - PublicCorner;
                Vector2 AtlasConformity = new Vector2(((float)LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X) * AtlasCoordinates.X, ((float)LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * AtlasCoordinates.Y);
                LocalCoord += AtlasConformity;
                Color Comparitor = OrderedAtlas[(int)LocalCoord.X, (int)LocalCoord.Y];
                if (Comparitor.A != 0) { return true; }
                else { return false; }
            }
            return false;
        }
        public float LayerDepth { get; set; }
        [field: NonSerialized]
        protected VoidDel MLC;
        public VoidDel MLCOut { get { return MLC; } }
        public String[] MLCRecord { get; set; }
        public Boolean TransientAnimation { get; set; }
        public void ReissueID()
        {
            pEntityID = IDIterator;
            IDIterator++;
        }
        public WorldEntity(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth)
        {
            pEntityID = IDIterator;
            TransientAnimation = false;
            ManualHorizontalFlip = false;
            IDIterator++;
            pName = Name;
            pDrawCoords = Location;
            LayerDepth = Depth;
            CustomCamera = null;
            CameraImmune = false;
            if (Atlas != null)
            {
                LocalAtlas = (TAtlasInfo)Atlas;
                pHitBox = new Rectangle(new Point((int)Location.X, (int)Location.Y), LocalAtlas.FrameSize());
            }
            MLC = delegate () { MouseLeftClick(); };
            MLCRecord = new string[0];
            AnimationQueue = new ArrayList();
            Stickers = new ArrayList();
        }
        ~WorldEntity()
        {
            if(Clickable) { Shell.MouseLeftClick -= MLC; }
        }
        public ulong EntityID { get { return pEntityID; } }
        public String Name { get { return pName; } }
        public Vector2 DrawCoords { get { return pDrawCoords; } }
        public void QuickMoveTo(Vector2 Coords)
        {
            pDrawCoords = Coords;
        }
        public Boolean Drawable
        {
            get
            {
                return pDrawable;
            }
            set
            {
                pDrawable = value;
            }
        }
        public ArrayList Stickers { get; set; }
        public void Move(Vector2 V)
        {
            pDrawCoords += V;
            if(Stickers != null && Stickers.Count > 0)
            {
                foreach(WorldEntity E in Stickers)
                {
                    E.Move(V);
                }
            }
        }
        public void Rotate(float R)
        {
            pRotation += R;
            if (Stickers != null && Stickers.Count > 0)
            {
                foreach (WorldEntity E in Stickers)
                {
                    E.Rotate(R);
                }
            }
        }
        public void Scale(Vector2 S)
        {
            if(InvertXScaling && InvertYScaling) { S = new Vector2(-S.X, -S.Y); }
            else if (InvertXScaling) { S = new Vector2(-S.X, S.Y); }
            else if (InvertYScaling) { S = new Vector2(S.X, -S.Y); }
            Vector2 ManualSet = new Vector2();
            if ((pScale + S).X < 0)
            {
                AutoHorizontalFlip = !AutoHorizontalFlip;
                InvertXScaling = !InvertXScaling;
                ManualSet = new Vector2(-(S.X + pScale.X), ManualSet.Y);
            }
            if ((pScale + S).Y < 0)
            {
                AutoVerticalFlip = !AutoVerticalFlip;
                InvertYScaling = !InvertYScaling;
                ManualSet = new Vector2(ManualSet.X, -(S.Y + pScale.Y));
            }
            if(ManualSet == new Vector2()) { pScale += S; }
            else
            {
                if (ManualSet.X != 0f) { pScale = new Vector2(ManualSet.X, pScale.Y); }
                else { pScale = new Vector2(pScale.X + S.X, pScale.Y); }
                if (ManualSet.Y != 0f) { pScale = new Vector2(pScale.X, ManualSet.Y); }
                else { pScale = new Vector2(pScale.X, pScale.Y + S.Y); }
            }
            //Shell.WriteLine(pName + ": Scale now set to: " + pScale.X + ", " + pScale.Y);
            if (Stickers != null && Stickers.Count > 0)
            {
                foreach (WorldEntity E in Stickers)
                {
                    E.Scale(S);
                }
            }
        }
        public void Colour(ColourShift C)
        {
            pColour = ColourShift.Constrain(pColour + C);
            if (Stickers != null && Stickers.Count > 0)
            {
                foreach (WorldEntity E in Stickers)
                {
                    E.Colour(C);
                }
            }
        }
        protected Boolean InvertXScaling = false;
        protected Boolean InvertYScaling = false;
        public Boolean[] CheckScaleInversions()
        {
            return new Boolean[] { InvertXScaling, InvertYScaling };
        }
        protected float MirrorOriginX
        {
            get
            {
                float XShift = -((HitBox.X / 2f) - pOrigin.X);
                return (HitBox.X / 2f) + XShift;
            }
        }
        protected float MirrorOriginY
        {
            get
            {
                float YShift = -((HitBox.Y / 2f) - pOrigin.Y);
                return (HitBox.Y / 2f) + YShift;
            }
        }
        public Vector2 AdjustedOrigin
        {
            get
            {
                Vector2 Out = pOrigin;
                if(InvertXScaling) { Out.X = MirrorOriginX; }
                if(InvertYScaling) { Out.Y = MirrorOriginY; }
                return Out;
            }
        }
        protected float FlipRotationAddit = 0f;
        public Boolean ManualHorizontalFlip { get; set; }
        public Boolean ManualVerticalFlip { get; set; }
        private Boolean AutoHorizontalFlip = false;
        private Boolean AutoVerticalFlip = false;
        private Boolean TrueHorizontalFlip { get { return ManualHorizontalFlip ^ AutoHorizontalFlip; } }
        private Boolean TrueVerticalFlip { get { return ManualVerticalFlip ^ AutoVerticalFlip; } }
        public virtual void Update()
        {
            foreach(Animation A in AnimationQueue)
            {
                if (!ButtonScripts.Paused)
                {
                    if(A.PlacedInPauseState)
                    {
                        A.UnHang();
                        A.PlacedInPauseState = false;
                    }
                    A.Step(this);
                }
                else
                {
                    if(!A.PlacedInPauseState)
                    {
                        A.TimeHang();
                        A.PlacedInPauseState = true;
                    }
                }
            }
            for(int i = 0; i < AnimationQueue.Count; i++)
            {
                Animation R = ((Animation)AnimationQueue[i]);
                if (R.Spent)
                {
                    R.AutoWipe();
                    AnimationQueue.Remove(R);
                    i--;
                    if(AnimationQueue.Count == 0 && TransientAnimation)
                    {
                        Shell.DeleteQueue.Add(this);
                    }
                }
            }
            if (TrueHorizontalFlip && TrueVerticalFlip)
            {
                LocalSpriteEffect = SpriteEffects.None;
                if(FlipRotationAddit == 0f) { FlipRotationAddit = (float)Math.PI; }
            }
            else
            {
                if (FlipRotationAddit != 0f) { FlipRotationAddit = 0f; }
                if (TrueHorizontalFlip && LocalSpriteEffect != SpriteEffects.FlipHorizontally) { LocalSpriteEffect = SpriteEffects.FlipHorizontally; }
                else if (TrueVerticalFlip && LocalSpriteEffect != SpriteEffects.FlipVertically) { LocalSpriteEffect = SpriteEffects.FlipVertically; }
                else if (!TrueHorizontalFlip && !TrueVerticalFlip && LocalSpriteEffect != SpriteEffects.None) { LocalSpriteEffect = SpriteEffects.None; }
            }
        }
        protected SpriteEffects LocalSpriteEffect = SpriteEffects.None;
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(LocalAtlas.Atlas, new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y), new Point((int)(LocalAtlas.FrameSize().X * pScale.X), (int)(LocalAtlas.FrameSize().Y * pScale.Y))), new Rectangle(new Point((LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X)*AtlasCoordinates.X, (LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * AtlasCoordinates.Y), LocalAtlas.FrameSize()), ColourValue, pRotation + FlipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth);
        }
        public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if(CameraImmune) { Draw(spriteBatch); }
            spriteBatch.Draw(LocalAtlas.Atlas, new Rectangle(Shell.PointMultiply(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y) + camera.OffsetPoint, camera.ZoomFactor), Shell.PointMultiply(new Point((int)(LocalAtlas.FrameSize().X * pScale.X), (int)(LocalAtlas.FrameSize().Y * pScale.Y)), camera.ZoomFactor)), new Rectangle(new Point((LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X) * AtlasCoordinates.X, (LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * AtlasCoordinates.Y), LocalAtlas.FrameSize()), ColourValue, pRotation + FlipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth);
        }
    }
    /// <summary>
    /// Camera objects can be placed in space and used to render objects with reference to their location. Supports mouse click-and-drag movement and a dynamic zoom function.
    /// </summary>
    [Serializable]
    public class Camera : WorldEntity
    {
        public Vector2 OffsetVector
        {
            get
            {
                return -(pDrawCoords - ((new Vector2(1280, 720) / ZoomFactor)/2));
            }
        }
        public Point OffsetPoint
        {
            get
            {
                return Shell.ConvertVector(OffsetVector);
            }
        }
        private double ZoomLevel = 0d;
        public Vector2 ZoomFactor
        {
            get
            {
                return (new Vector2((float)Math.Pow(2d, ZoomLevel), (float)Math.Pow(2d, ZoomLevel))) * pScale;
            }
        }
        public void SnapTo(WorldEntity WE)
        {
            QuickMoveTo(WE.DrawCoords);
        }
        public void CenterDefault()
        {
            QuickMoveTo((new Vector2(1280, 720) / 2));
        }
        public void Zoom(float Z)
        {
            ZoomLevel += Z;
        }
        public void ResetZoom()
        {
            ZoomLevel = 0d;
            pScale = new Vector2(1, 1);
        }
        public Camera(String Name) : base(Name, (new Vector2(1280, 720) / 2), null, 1)
        {
            MouseDragEnabled = false;
        }
        public Boolean MouseDragEnabled { get; set; }
        private Boolean MouseDragging = false;
        private Boolean ZoomOpen = false;
        private Vector2 LastMouseDragPos = new Vector2();
        private int LastMouseScroll = 0;
        public override void Update()
        {
            base.Update();
            if (MouseDragEnabled)
            {
                MouseState MyMouse = Mouse.GetState();
                if (MyMouse.LeftButton == ButtonState.Pressed)
                {
                    if (!MouseDragging)
                    {
                        LastMouseDragPos = Shell.CoordNormalize(Shell.ConvertPoint(MyMouse.Position));
                        MouseDragging = true;
                    }
                    Vector2 CurrentMouseDragPos = Shell.CoordNormalize(Shell.ConvertPoint(MyMouse.Position));
                    Vector2 DragDistance = CurrentMouseDragPos - LastMouseDragPos;
                    Move(-DragDistance / ZoomFactor);
                    LastMouseDragPos = CurrentMouseDragPos;
                }
                else if (MyMouse.LeftButton == ButtonState.Released)
                {
                    MouseDragging = false;
                }
                if (!ZoomOpen)
                {
                    LastMouseScroll = MyMouse.ScrollWheelValue;
                    ZoomOpen = true;
                }
                int CurrentMouseScroll = MyMouse.ScrollWheelValue;
                ZoomLevel += (CurrentMouseScroll - LastMouseScroll) / 1000d;
                LastMouseScroll = CurrentMouseScroll;
            }
            else
            {
                if (MouseDragging) { MouseDragging = false; }
                if (ZoomOpen) { ZoomOpen = false; }
            }
        }
    }
    /// <summary>
    /// An object that renders text to the screen. Text strings can be dynamically formatted and displayed based on the properties of the TextEntity and user provided text markup. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    public class TextEntity : WorldEntity
    {
        public static int TickWriteInterval = 30;
        /// <summary>
        /// Data structure for a single contiguous text string chunk used within a TextEntity.
        /// </summary>
        [Serializable]
        public struct TextChunk
        {
            public String Text;
            public String FontName;
            [field: NonSerialized]
            public SpriteFont Font;
            public Color Colour;
            public Vector2 DrawLocation;
            public Boolean RainbowMode;
            public Boolean Linebreak;
            public int TimeDelay;
            public void Rainbow()
            {
                Colour = GetRainbowColour();
            }
        }
        public static int GetTicksFromSliderValue(float SliderValue)
        {
            return (int)Math.Round(1000 / (Math.Pow(10, (3 * SliderValue))));
        }
        public static float GetSliderValueFromTicks(int Ticks)
        {
            return (float)(Math.Log10(1000 / Ticks) / 3);
        }
        public static Color GetRainbowColour()
        {
            double[] ScrollColour = new double[3];
            ScrollColour[0] = ((Environment.TickCount / 10) % 300) * (double)(Math.PI / 150);
            ScrollColour[1] = (((Environment.TickCount / 10) + 100) % 300) * (double)(Math.PI / 150);
            ScrollColour[2] = (((Environment.TickCount / 10) + 200) % 300) * (double)(Math.PI / 150);
            if (ScrollColour[0] >= Math.PI * 2) { ScrollColour[0] = 0; }
            if (ScrollColour[1] >= Math.PI * 2) { ScrollColour[1] = 0; }
            if (ScrollColour[2] >= Math.PI * 2) { ScrollColour[2] = 0; }
            Color Out = new Color((byte)(122.5 * (Math.Sin(ScrollColour[0]) + 1)), (byte)(122.5 * (Math.Sin(ScrollColour[1]) + 1)), (byte)(122.5 * (Math.Sin(ScrollColour[2]) + 1)), (byte)255);
            return Out;
        }
        public static String GetRainbowColourCode()
        {
            Color TrueColour = GetRainbowColour();
            return "[C:" + TrueColour.R + "-" + TrueColour.G + "-" + TrueColour.B + "-255]";
        }
        protected int pLength = 0;
        public int Length { get { return pLength; } }
        public static TextChunk[] PreprocessText(String Text)
        {
            return PreprocessText(Text, -1);
        }
        public static TextChunk[] PreprocessText(String Text, int PixelBuffer)
        {
            Text = Text.Replace("\n", "[N]");
            if (Text.Length > 0 && Text[Text.Length-1] == ']') { Text += " "; }
            ArrayList ChunkStore = new ArrayList();
            Vector2 Location = new Vector2();
            Boolean First = true;
            while (Text.Contains('[') || First)
            {
                First = false;
                SpriteFont Font = Shell.Default;
                String FontName = "DEFAULT";
                Color Colour = Color.White;
                Boolean Linebreak = false;
                Boolean RainbowMode = false;
                int TimeDelay = 0;
                if (Text.IndexOf('[') == 0)
                {
                    String Formatting = Text.Remove(Text.IndexOf(']') + 1);
                    Formatting = Formatting.Replace("[", "").Replace("]", "").ToUpper();
                    String[] Specs = Formatting.Split(',');
                    foreach (String S in Specs)
                    {
                        String[] SSplit = S.Split(':');
                        switch(SSplit[0])
                        {
                            case "C":
                                switch(SSplit[1])
                                {
                                    case "WHITE":
                                        Colour = Color.White;
                                        break;
                                    case "PURPLE":
                                        Colour = new Color(138, 0, 255, 255);
                                        break;
                                    default:
                                        byte[] RGBA = SSplit[1].Split('-').Select(x => Convert.ToByte(x)).ToArray();
                                        Colour.R = RGBA[0];
                                        Colour.G = RGBA[1];
                                        Colour.B = RGBA[2];
                                        Colour.A = RGBA[3];
                                        break;
                                }
                                break;
                            case "F":
                                Font = (SpriteFont)Shell.Fonts[SSplit[1]];
                                FontName = SSplit[1];
                                break;
                            case "L":
                                Vector2 Adjustment = new Vector2(0,0);
                                Adjustment.X = SSplit[1].Split('-').Select(x => Convert.ToInt32(x)).ToArray()[0];
                                Adjustment.Y = SSplit[1].Split('-').Select(x => Convert.ToInt32(x)).ToArray()[1];
                                Location += Adjustment;
                                break;
                            case "R":
                                RainbowMode = true;
                                break;
                            case "N":
                                Linebreak = true;
                                break;
                            case "T":
                                TimeDelay = Convert.ToInt32(SSplit[1]);
                                break;
                            default:
                                break;
                        }
                    }
                    Text = Text.Remove(0, Formatting.Length + 2);
                }
                TextChunk Temp = new TextChunk();
                Temp.Font = Font;
                Temp.FontName = FontName;
                Temp.Colour = Colour;
                Temp.DrawLocation = Location;
                Temp.RainbowMode = RainbowMode;
                Temp.Linebreak = Linebreak;
                Temp.TimeDelay = TimeDelay;
                if (Text.Contains('['))
                {
                    Temp.Text = Text.Remove(Text.IndexOf('['));
                }
                else { Temp.Text = Text; }
                ChunkStore.Add(Temp);
                Location.X += (int)Font.MeasureString(Temp.Text).X;
                Text = Text.Remove(0, Temp.Text.Length);
            }
            TextChunk[] Out = ChunkStore.ToArray().Select(x => (TextChunk)x).ToArray();
            if(PixelBuffer != -1) { Out = LinebreakChunks(Out, PixelBuffer); }
            return Out;
        }
        public static TextChunk[] LinebreakChunks(TextChunk[] Initial, int MaxPixelLineLength) //Function to insert linebreaks into text as required, based on a given length
        {
            int CurrentPixelTotal = 0;
            Vector2 RollingLocationMod = new Vector2(0, 0); //Vector representing the degree to which the next line should be shifted from the previous
            Vector2 StackableVector2 = new Vector2(0, 0); //Vector representing the degree to which all subsequent text chunks should be additionally shifted by auto-inserted line breaks
            Vector2 LinebreakAmmendment = new Vector2(); //Vector representing the degree to which all subsequent text chunks should be additionally shifted due to manual line breaks
            ArrayList RebuildChunks = new ArrayList(); //The new set of chunks broken up correctly to include line breaks
            ArrayList NewChunkRegistry = new ArrayList(); //Collection of new text chunks created by auto-inserted line breaks, that will be depopulated as they are processed
            Boolean TCOn = false;
            int i = 0;
            while(i < Initial.Length || NewChunkRegistry.Count > 0)
            {
                TextChunk TC; //TC is the current text chunk being processed
                if (NewChunkRegistry.Count > 0) //If new chunks have been created by automated linebreaking, they are dealt with before the next input text chunk
                {
                    i--;
                    TC = (TextChunk)NewChunkRegistry[0];
                    NewChunkRegistry.RemoveAt(0);
                    StackableVector2 += RollingLocationMod;
                }
                else //If there are none, then the next chunk in the list is selected
                {
                    TC = Initial[i];
                    if (i > 0)
                    {
                        TC.DrawLocation += (LinebreakAmmendment + StackableVector2 - RollingLocationMod); //Its initial location is modified from the input location (determined in PreprocessText()) using the three vectors
                        //RollingLocationMod is subtracted because StackableVector2 will contain its value, if this is a new chunk after an autolinebreak, and it is always added back on regardless, so this nullifies that
                    }
                }
                if(TC.Linebreak)
                {
                    //Manual linebreaks cause RLM to be updated, but the change is also stored in LinebreakAmmendment for future chunks
                    RollingLocationMod = new Vector2(RollingLocationMod.X - CurrentPixelTotal, RollingLocationMod.Y + (int)TC.Font.MeasureString(" ").Y);
                    LinebreakAmmendment += new Vector2(-CurrentPixelTotal, (int)TC.Font.MeasureString(" ").Y);
                    CurrentPixelTotal = 0;
                    TCOn = true;
                    TC.Linebreak = false;
                    Initial[i].Linebreak = false;
                }
                TC.DrawLocation += RollingLocationMod; //The TC draw location is modified by the RLM vector
                int PixFromNextTotal = 0;
                if(i + 1 < Initial.Length) //This section of code determines how many pixels the text chunk after this would take up before a line break is possible, for later use
                {
                    int Forward = 1;
                    while (i + Forward < Initial.Length)
                    {
                        TextChunk NTC = Initial[i + Forward];
                        if(NTC.Linebreak) { break; }
                        if (NTC.Text.Contains(' '))
                        {
                            PixFromNextTotal += (int)TC.Font.MeasureString(NTC.Text.Remove(NTC.Text.IndexOf(' '))).X;
                            break;
                        }
                        else { PixFromNextTotal += (int)TC.Font.MeasureString(NTC.Text).X; }
                        Forward++;
                    }
                }
                if(CurrentPixelTotal + (int)TC.Font.MeasureString(TC.Text).X + PixFromNextTotal > MaxPixelLineLength) //If the current length plus the next chunk's pre-space pixels break the line limit...
                {
                    if (TC.Text.Contains(' ')) //First we check for a space to see if a linebreak can be performed in the current text chunk
                    {
                        String FindLoc = TC.Text;
                        int FoundLocation = -1;
                        Boolean ByPix = false;
                        //ByPix: whether the next chunk will take the line length over the edge
                        if(CurrentPixelTotal + (int)TC.Font.MeasureString(TC.Text).X + PixFromNextTotal > MaxPixelLineLength && TC.Font.MeasureString(TC.Text).X + CurrentPixelTotal <= MaxPixelLineLength) { ByPix = true; }
                        //ByPix is used to cause a linebreak on the last possible space to avoid overflow next text chunk
                        //Otherwise, each space is checked until one is found that allows the line to be shortened to within the limit
                        while ((TC.Font.MeasureString(FindLoc).X + CurrentPixelTotal > MaxPixelLineLength || ByPix) && FindLoc.Contains(' '))
                        {
                            ByPix = false;
                            int CI = FindLoc.LastIndexOf(' ');
                            FindLoc = FindLoc.Remove(CI);
                            if (TC.Font.MeasureString(FindLoc).X + CurrentPixelTotal <= MaxPixelLineLength)
                            {
                                FoundLocation = CI;
                            }
                        }
                        //If no workable space is found, the first one is picked as the best compromise
                        if (FoundLocation == -1)
                        {
                            /*if (CurrentPixelTotal != 0)
                            {
                                FoundLocation = -1;
                            }
                            else { FoundLocation = TC.Text.IndexOf(' '); }*/
                            FoundLocation = TC.Text.IndexOf(' ');
                        }
                        //A new text chunk is created to represent the text after the line break
                        TextChunk New = TC;
                        New.TimeDelay = 0;
                        New.Text = New.Text.Remove(0, FoundLocation + 1);
                        if(New.Text.Length > 0 && New.Text[0] == ' ') { New.Text = New.Text.Remove(0, 1); }
                        int TCMeasure = 0;
                        if (FoundLocation >= 0)
                        {
                            TC.Text = TC.Text.Remove(FoundLocation);
                            TCMeasure = (int)TC.Font.MeasureString(TC.Text + " ").X; //The length of the first half of the new, broken line is measured
                        }
                        else { TC.Text = ""; }
                        RollingLocationMod.X = -CurrentPixelTotal - TCMeasure; //RollingLocationMod is used to modify the position of the new text chunk, and also to later transfer this information to the SV2 vector
                        New.DrawLocation.X += TCMeasure; //The length is added back on, as the New text chunk is starting from the initial location of the old one (TC) before being shifted back by RLM, so it must be adjusted for the fact that it comes after TC
                        if (TC.Text.Length != 0) { RebuildChunks.Add(TC); } //The current text chunk is added to the new, final list of chunks
                        NewChunkRegistry.Add(New);
                        CurrentPixelTotal = 0;
                        RollingLocationMod.Y = (int)TC.Font.MeasureString(" ").Y; //And the "new" chunk is also shifted down
                    }
                    else //Else, if text cannot be split...
                    {
                        /* Unsure what I was doing here... function to move overflowing continuous text onto a new line that was unneccessary
                         * RollingLocationMod.X = -CurrentPixelTotal - (int)TC.Font.MeasureString(TC.Text).X;
                        RollingLocationMod.Y = (int)TC.Font.MeasureString(" ").Y;
                        if (!(i == 0 && TC.Text == Initial[0].Text) && CurrentPixelTotal > 0)
                        {
                            //TC.DrawLocation.X -= (int)TC.Font.MeasureString(TC.Text).X;
                            TC.DrawLocation.Y += (int)TC.Font.MeasureString(TC.Text).Y;
                            RollingLocationMod.Y += (int)TC.Font.MeasureString(TC.Text).Y;
                            if (TC.Text.Length > 0 && TC.Text[0] == ' ') { TC.Text = TC.Text.Remove(0, 1); }
                        }*/
                        RebuildChunks.Add(TC); //Ignore split and proceed as normal as it is not possible to split, hoping that the next text chunk will be splitable.
                        CurrentPixelTotal += (int)TC.Font.MeasureString(TC.Text).X;
                        //CurrentPixelTotal = 0;
                    }
                }
                else //If no linebreak is required...
                {
                    RebuildChunks.Add(TC); //Simply add TC into the new chunk list as is
                    CurrentPixelTotal += (int)TC.Font.MeasureString(TC.Text).X; //And increment the CurrentPixelTotal to keep a record of how far we are towards the length limit
                }
                i++;
            }
            return RebuildChunks.ToArray().Select(x => (TextChunk)x).ToArray(); //The new list of text chunks is returned as an array
        }
        ArrayList IgnoreDelayOnThis = new ArrayList();
        public TextChunk[] RevealXChars(TextChunk[] Initial, int CharCount)
        {
            TextChunk[] Copy = (TextChunk[])Initial.Clone();
            int CharR = 0;
            for (int i = 0; i < Copy.Length; i++)
            {
                TextChunk TC = Copy[i];
                if(CharR >= CharCount) { TC.Text = ""; }
                for (int ii = 0; ii < TC.Text.Length; ii++)
                {
                    if (TC.TimeDelay != 0 && !IgnoreDelayOnThis.Contains(i) && WriteProgress < pLength)
                    {
                        Updatetime += TC.TimeDelay - 30;
                        IgnoreDelayOnThis.Add(i);
                        ii--;
                    }
                    CharR++;
                    if(CharR >= CharCount && ii+1 < TC.Text.Length)
                    {
                        TC.Text = TC.Text.Remove(ii+1);
                        break;
                    }
                }
                Copy[i] = TC;
            }
            return Copy;
        }
        String pText = "";
        TextChunk[] TextChunkR;
        public override void OnDeserializeDo()
        {
            base.OnDeserializeDo();
            for(int i = 0; i < TextChunkR.Length; i++)
            {
                TextChunk TC = TextChunkR[i];
                String FName = (TC.FontName != null ? TC.FontName : "DEFAULT");
                TC.Font = (SpriteFont)Shell.Fonts[FName];
                TextChunkR[i] = TC;
            }
            for (int i = 0; i < ProgressiveChunks.Length; i++)
            {
                TextChunk TC = ProgressiveChunks[i];
                String FName = (TC.FontName != null ? TC.FontName : "DEFAULT");
                TC.Font = (SpriteFont)Shell.Fonts[FName];
                ProgressiveChunks[i] = TC;
            }
        }
        public int BufferLength { get; set; }
        public String Text
        {
            get { return pText; }
            set
            {
                pText = value;
                TextChunkR = new TextChunk[0];
                IgnoreDelayOnThis = new ArrayList();
                TextChunkR = PreprocessText(value, BufferLength);
                int L = 0;
                foreach (TextChunk TCO in TextChunkR) { L += TCO.Text.Length; }
                pLength = L;
            }
        }
        public TextEntity(String Name, String TextIn, Vector2 Location, float Depth) : base(Name, Location, null, Depth)
        {
            BufferLength = 1000;
            TextChunkR = PreprocessText(TextIn, BufferLength);
            if(TypeWrite) { ProgressiveChunks = RevealXChars(TextChunkR, 0); }
            int L = 0;
            foreach (TextChunk TCO in TextChunkR) { L += TCO.Text.Length; }
            pLength = L;
            pText = TextIn;
            TypeWrite = true;
        }
        public void ReWrite()
        {
            ProgressiveChunks = new TextChunk[0];
            WriteProgress = -1;
        }
        public void SkipWrite()
        {
            WriteProgress = pLength;
            ProgressiveChunks = RevealXChars(TextChunkR, WriteProgress);
        }
        public int VerticalLength()
        {
            return VerticalLength(false);
        }
        public int VerticalLength(Boolean CheckDisplacements)
        {
            int Out = 0;
            if(CheckDisplacements)
            {
                float CC = 0;
                Out = (int)(TextChunkR[0].Font.MeasureString(" ").Y);
                foreach (TextChunk T in TextChunkR)
                {
                    if(T.DrawLocation.Y > CC)
                    {
                        CC = T.DrawLocation.Y;
                        Out = (int)(CC + T.Font.MeasureString(" ").Y);
                    }
                }
                foreach (TextChunk T in TextChunkR)
                {
                    if (T.DrawLocation.Y < CC)
                    {
                        CC = T.DrawLocation.Y;
                    }
                }
                Out -= (int)CC;
            }
            else
            {
                Out = ((int)TextChunkR[TextChunkR.Length - 1].DrawLocation.Y + (int)TextChunkR[TextChunkR.Length - 1].Font.MeasureString(" ").Y);
            }
            return Out;
        }
        int WriteProgress = -1;
        public Boolean TypeWrite { get; set; }
        private TextChunk[] ProgressiveChunks = new TextChunk[] { };
        public Boolean WrittenAll()
        {
            return !(WriteProgress < pLength) || !TypeWrite;
        }
        public static void PlayTick()
        {
            if (!Shell.Mute)
            {
                int Tn = Shell.Rnd.Next(1, 4);
                SoundEffectInstance Tick = ((SoundEffect)Shell.SFXDirectory["TYPE_" + Tn]).CreateInstance();
                Tick.Volume = Shell.GlobalVolume;
                Tick.Play();
                Shell.ActiveSounds.Add(Tick);
            }
        }
        Boolean Sounder = false;
        public override void Update()
        {
            base.Update();
            if (TypeWrite && WriteProgress < pLength && Updatetime < Environment.TickCount - TickWriteInterval && !Shell.HoldRender)
            {
                Updatetime = Environment.TickCount;
                if (!ButtonScripts.Paused && !ButtonScripts.Navigating)
                {
                    WriteProgress++;
                    ProgressiveChunks = RevealXChars(TextChunkR, WriteProgress);
                    if (Sounder)
                    {
                        if (Name == "TEXT_MAIN" && pLength > 0)
                        {
                            PlayTick();
                        }
                        Sounder = false;
                    }
                    else { Sounder = true; }
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!TypeWrite)
            {
                foreach (TextChunk TC in TextChunkR)
                {
                    if(TC.RainbowMode) { TC.Rainbow(); }
                    spriteBatch.DrawString(TC.Font, TC.Text, new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + DrawCoords, TC.Colour * (pColour.A / 255f), 0f, new Vector2(0,0), 1f, SpriteEffects.None, LayerDepth);
                }
            }
            else
            {
                foreach (TextChunk TC in ProgressiveChunks)
                {
                    if (TC.RainbowMode) { TC.Rainbow(); }
                    spriteBatch.DrawString(TC.Font, TC.Text, new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + DrawCoords, TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), 1f, SpriteEffects.None, LayerDepth);
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            if (!TypeWrite)
            {
                foreach (TextChunk TC in TextChunkR)
                {
                    if (TC.RainbowMode) { TC.Rainbow(); }
                    spriteBatch.DrawString(TC.Font, TC.Text, (new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + DrawCoords + camera.OffsetVector) * camera.ZoomFactor, TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), camera.ZoomFactor.X, SpriteEffects.None, LayerDepth);
                }
            }
            else
            {
                foreach (TextChunk TC in ProgressiveChunks)
                {
                    if (TC.RainbowMode) { TC.Rainbow(); }
                    spriteBatch.DrawString(TC.Font, TC.Text, (new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + DrawCoords + camera.OffsetVector) * camera.ZoomFactor, TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), camera.ZoomFactor.X, SpriteEffects.None, LayerDepth);
                }
            }
        }
    }
}
