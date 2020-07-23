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
using System.Reflection;

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
        protected Point pAtlasCoordinates = new Point(0, 0);
        public Point AtlasCoordinates
        {
            get { return pAtlasCoordinates; }
        }
        protected float pRotation = 0f;
        public float RotationRads { get { return pRotation; } }
        protected Vector2 pScale = new Vector2(1, 1);
        public Vector2 ScaleSize { get { return pScale; } }
        protected Vector2 pOrigin = new Vector2();
        protected Boolean pCO = false;
        protected ColourShift pColour = new ColourShift(255f, 255f, 255f, 255f);
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
            if (Coords.X < LocalAtlas.DivDimensions.X && Coords.Y < LocalAtlas.DivDimensions.Y)
            {
                pAtlasCoordinates = Coords;
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
                if (pCO == true)
                {
                    pOrigin = VNFUtils.ConvertPoint(HitBox.Size) / 2;
                }
                else
                {
                    pOrigin = new Vector2();
                }
            }
        }
        public Boolean SuppressClickable { get; set; }
        //It should be noted that the event subscription register only holds the entity name of the publishing entity, as entIDs are reassigned after deserialization.
        //Due to this, if a specific event subscription is required, the publisher should be ensured to have a unique name string.
        [Serializable]
        public struct EventSubRegister
        {
            public EventSubRegister(String PublisherEntName, EventNames EventName, MethodInfo EventHandler, object[] MethodArgs)
            {
                this.PublisherEntName = PublisherEntName;
                this.EventName = EventName;
                this.EventHandler = EventHandler;
                this.MethodArgs = MethodArgs;
            }
            public String PublisherEntName;
            public EventNames EventName;
            public MethodInfo EventHandler;
            public object[] MethodArgs;
        }
        protected List<EventSubRegister> pSubscribedEvents = new List<EventSubRegister>();
        public List<EventSubRegister> SubscribedEvents
        {
            get
            {
                return pSubscribedEvents;
            }
            set
            {
                pSubscribedEvents = value;
            }
        }
        public void SubscribeToEvent(EventNames EventName, MethodInfo EventHandler, object[] MethodArgs)
        {
            SubscribeToEvent(this, EventName, EventHandler, MethodArgs);
        }
        public void SubscribeToEvent(WorldEntity EventPublisher, EventNames EventName, MethodInfo EventHandler, object[] MethodArgs)
        {
            pSubscribedEvents.Add(new EventSubRegister(EventPublisher.Name, EventName, EventHandler, MethodArgs));
            EventSubscribeActual(EventPublisher, EventName, EventHandler, MethodArgs);
        }
        [field: NonSerialized]
        protected Dictionary<WorldEntity, ArrayList> TrueDetachers = new Dictionary<WorldEntity, ArrayList>();
        private void EventSubscribeActual(WorldEntity EventPublisher, EventNames EventName, MethodInfo EventHandler, object[] MethodArgs)
        {
            VoidDel ThisHandler = new VoidDel(delegate () {
                EventHandler.Invoke(this, MethodArgs);
            });
            if(!TrueDetachers.ContainsKey(EventPublisher))
            {
                TrueDetachers.Add(EventPublisher, new ArrayList());
            }
            ((ArrayList)TrueDetachers[EventPublisher]).Add(new object[] { EventName, ThisHandler });
            EventCoupleDecouple(EventPublisher, EventName, ThisHandler, true);
        }
        [Serializable]
        public enum EventNames { EntityClickFunction, ButtonPressFunction, ButtonHoverFunction, ButtonHoverReleaseFunction, SliderClickFunction, ScrollBarClickFunction, TextEnteredFunction };
        public void EventCoupleDecouple(WorldEntity EventPublisher, EventNames EventName, VoidDel Handler, Boolean Subscribe)
        {
            switch (EventName)
            {
                case EventNames.EntityClickFunction:
                    if (Subscribe) { EventPublisher.EntityClickFunction += Handler; }
                    else { EventPublisher.EntityClickFunction -= Handler; }
                    break;
                case EventNames.ButtonPressFunction:
                    if (EventPublisher is Button)
                    {
                        Button B = (Button)EventPublisher;
                        if (Subscribe) { B.ButtonPressFunction += Handler; }
                        else { B.ButtonPressFunction -= Handler; }
                    }
                    break;
                case EventNames.ButtonHoverFunction:
                    if (EventPublisher is Button)
                    {
                        Button B = (Button)EventPublisher;
                        if (Subscribe) { B.ButtonHoverFunction += Handler; }
                        else { B.ButtonHoverFunction -= Handler; }
                    }
                    break;
                case EventNames.ButtonHoverReleaseFunction:
                    if (EventPublisher is Button)
                    {
                        Button B = (Button)EventPublisher;
                        if (Subscribe) { B.ButtonHoverReleaseFunction += Handler; }
                        else { B.ButtonHoverReleaseFunction -= Handler; }
                    }
                    break;
                case EventNames.SliderClickFunction:
                    if (EventPublisher is Slider)
                    {
                        Slider S = (Slider)EventPublisher;
                        if (Subscribe) { S.SliderClickFunction += Handler; }
                        else { S.SliderClickFunction -= Handler; }
                    }
                    break;
                case EventNames.ScrollBarClickFunction:
                    if (EventPublisher is ScrollBar)
                    {
                        ScrollBar S = (ScrollBar)EventPublisher;
                        if (Subscribe) { S.ScrollBarClickFunction += Handler; }
                        else { S.ScrollBarClickFunction -= Handler; }
                    }
                    else if (EventPublisher is VerticalScrollPane)
                    {
                        VerticalScrollPane S = (VerticalScrollPane)EventPublisher;
                        if (Subscribe) { S.ScrollBarClickFunction += Handler; }
                        else { S.ScrollBarClickFunction -= Handler; }
                    }
                    break;
                case EventNames.TextEnteredFunction:
                    if (EventPublisher is TextInputField)
                    {
                        TextInputField T = (TextInputField)EventPublisher;
                        if (Subscribe) { T.TextEnteredFunction += Handler; }
                        else { T.TextEnteredFunction -= Handler; }
                    }
                    break;
            }
        }
        public virtual void AddEventTriggers()
        {
            Shell.MouseLeftClick += EntityClickFunctionTrigger;
        }
        public virtual void RemoveEventTriggers()
        {
            Shell.MouseLeftClick -= EntityClickFunctionTrigger;
        }
        public virtual void ClickTrigger()
        {
            EntityClickFunctionTrigger();
        }
        [field:NonSerialized]
        public event VoidDel EntityClickFunction;
        protected virtual void EntityClickFunctionTrigger()
        {
            if (EntityClickFunction != null && MouseInBounds() && !SuppressClickable) { EntityClickFunction?.Invoke(); }
        }
        private Texture2D SerializationBackup = null;
        public virtual void OnSerializeDo()
        {
            foreach (Animation A in AnimationQueue) { A.TimeHang(); }
            if (LocalAtlas.ReferenceHash == "" || LocalAtlas.ReferenceHash == null)
            {
                SerializationBackup = LocalAtlas.Atlas;
            }
        }
        public void ResubscribeEvents()
        {
            if(TrueDetachers != null && TrueDetachers.Count > 0)
            {
                UnsubscribeEvents();
            }
            List<EventSubRegister> LocalSubE = SubscribedEvents;
            SubscribedEvents = new List<EventSubRegister>();
            foreach (EventSubRegister ESR in LocalSubE)
            {
                WorldEntity Publisher = Shell.GetEntityByName(ESR.PublisherEntName);
                if(Publisher != null)
                {
                    SubscribeToEvent(Publisher, ESR.EventName, ESR.EventHandler, ESR.MethodArgs);
                }
            }
        }
        private void UnsubscribeEvents()
        {
            if (TrueDetachers != null)
            {
                foreach (WorldEntity E in TrueDetachers.Keys)
                {
                    foreach (object[] Pair in TrueDetachers[E])
                    {
                        EventCoupleDecouple(E, (EventNames)Pair[0], (VoidDel)Pair[1], false);
                    }
                }
            }
            TrueDetachers = new Dictionary<WorldEntity, ArrayList>();
        }
        public virtual void OnDeserializeDo()
        {
            AddEventTriggers();
            if(MyBehaviours is null) { MyBehaviours = new ArrayList(); }
            foreach (Animation A in AnimationQueue)
            {
                A.ReRegisterSelf();
                A.UnHang();
                if (A.Started && A.TimeElapsed > 100) { A.Jump(this); }
            }
            if (SerializationBackup != null)
            {
                LocalAtlas.Atlas = SerializationBackup;
                SerializationBackup = null;
            }
            else if (LocalAtlas.ReferenceHash != "" && LocalAtlas.ReferenceHash != null)
            {
                LocalAtlas.Atlas = ((TAtlasInfo)Shell.AtlasDirectory[LocalAtlas.ReferenceHash]).Atlas;
            }
            TrueDetachers = new Dictionary<WorldEntity, ArrayList>();
        }
        protected Rectangle pHitBox = new Rectangle(0, 0, 0, 0);
        public virtual Rectangle HitBox
        {
            get
            {
                Point Size = new Point((int)(LocalAtlas.FrameSize().X * pScale.X), (int)(LocalAtlas.FrameSize().Y * pScale.Y));
                if (!pCO) { pHitBox = new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y), Size); }
                else { pHitBox = new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y) - new Point(Size.X / 2, Size.Y / 2), Size); }
                return pHitBox;
            }
        }
        public ArrayList AnimationQueue { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is WorldEntity) { return Equals((WorldEntity)obj); }
            else { return false; }
        }
        public override int GetHashCode()
        {
            return (int)(EntityID % (UInt32.MaxValue - Int32.MaxValue));
        }
        public Boolean Equals(WorldEntity B)
        {
            if (B is null) { return false; }
            if (B.EntityID == pEntityID) { return true; }
            else { return false; }
        }
        public static Boolean operator ==(WorldEntity A, WorldEntity B)
        {
            if (A is null && B is null) { return true; }
            else if (A is null ^ B is null) { return false; }
            return A.Equals(B);
        }
        public static Boolean operator !=(WorldEntity A, WorldEntity B)
        {
            if (A is null && B is null) { return false; }
            else if (A is null ^ B is null) { return true; }
            return !A.Equals(B);
        }
        public Vector2 PseudoMouse { get; set; }
        public Boolean UsePseudoMouse { get; set; }
        public Boolean MouseInBounds()
        {
            if(LocalAtlas.Atlas == null) { return false; }
            var MouseState = Mouse.GetState();
            //return HitBox.Contains(new Vector2(MouseState.X, MouseState.Y));
            Vector2 NormalizedMouseVector;
            if (UsePseudoMouse)
            {
                NormalizedMouseVector = PseudoMouse;
            }
            else { NormalizedMouseVector = Shell.CoordNormalize(new Vector2(MouseState.X, MouseState.Y)); }
            return TextureAwareInBounds(NormalizedMouseVector);
        }
        public Boolean TextureAwareInBounds(Vector2 V)
        {
            Vector2 ZoomFactor = new Vector2(1, 1);
            if (!CameraImmune)
            {
                if (CustomCamera != null)
                {
                    V = CustomCamera.TranslateCoordsToEquivalent(V);
                }
                else if (Shell.AutoCamera != null)
                {
                    V = Shell.AutoCamera.TranslateCoordsToEquivalent(V);
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
                Vector2 AtlasConformity = new Vector2(((float)LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X) * pAtlasCoordinates.X, ((float)LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * pAtlasCoordinates.Y);
                LocalCoord += AtlasConformity;
                Color Comparitor = OrderedAtlas[(int)LocalCoord.X, (int)LocalCoord.Y];
                if (Comparitor.A != 0) { return true; }
                else { return false; }
            }
            return false;
        }
        public float LayerDepth { get; set; }
        public Boolean TransientAnimation { get; set; }
        public void ReissueID()
        {
            pEntityID = IDIterator;
            IDIterator++;
        }
        public WorldEntity(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth)
        {
            pEntityID = IDIterator;
            IDIterator++;
            TransientAnimation = false;
            ManualHorizontalFlip = false;
            pName = Name;
            pDrawCoords = Location;
            LayerDepth = Depth;
            CustomCamera = null;
            CameraImmune = false;
            InitStateHash();
            AddEventTriggers();
            PseudoMouse = new Vector2(float.NaN, float.NaN);
            if (Atlas != null)
            {
                LocalAtlas = (TAtlasInfo)Atlas;
                pHitBox = new Rectangle(new Point((int)Location.X, (int)Location.Y), LocalAtlas.FrameSize());
            }
            AnimationQueue = new ArrayList();
            Stickers = new ArrayList();
            MyBehaviours = new ArrayList();
        }
        ~WorldEntity()
        {
            ManualDispose();
        }
        public void ManualDispose()
        {
            RemoveEventTriggers();
            UnsubscribeEvents();
            if(Shell.NonSerializables.Contains(this)) { Shell.NonSerializables.Remove(this); }
            foreach (Animation A in AnimationQueue)
            {
                A.AutoWipe();
            }
            AnimationQueue = new ArrayList();
            foreach (Behaviours.IVNFBehaviour B in MyBehaviours)
            {
                B.Clear();
            }
            MyBehaviours = new ArrayList();
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
        /// <summary>
        /// The EntityStates hashtable contains information about different ways a WorldEntity is behaving, such as how it is moving or shifting in the world. This can be used as a reference for animation controllers.
        /// </summary>
        public Hashtable EntityStates
        {
            get
            {
                Hashtable StatesReal = new Hashtable();
                lock(pStateHash.SyncRoot)
                {
                    foreach(String K in pStateHash.Keys)
                    {
                        StatesReal.Add(K, ((object[])pStateHash[K])[0]);
                    }
                }
                return StatesReal;
            }
        }
        private Hashtable pStateHash = new Hashtable();
        private void InitStateHash()
        {
            String[] State = new String[] { "NORTHSOUTH", "EASTWEST", "ROTATION", "SCALEHORIZ", "SCALEVERT", "RED", "GREEN", "BLUE", "ALPHA" };
            lock (pStateHash.SyncRoot)
            {
                foreach (String S in State)
                {
                    pStateHash.Add(S, new object[] { 0f, 0 });
                }
            }
        }
        public ArrayList Stickers { get; set; }
        public void Move(Vector2 V)
        {
            pDrawCoords += V;
            lock (pStateHash.SyncRoot)
            {
                pStateHash["NORTHSOUTH"] = new object[] { V.Y, V.Y != 0 ? 2 : 0 };
                pStateHash["EASTWEST"] = new object[] { V.X, V.X != 0 ? 2 : 0 };
            }
            if (Stickers != null && Stickers.Count > 0)
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
            lock (pStateHash.SyncRoot)
            {
                pStateHash["ROTATION"] = new object[] { R, R != 0 ? 2 : 0 };
            }
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
            lock (pStateHash.SyncRoot)
            {
                pStateHash["SCALEVERT"] = new object[] { S.Y, S.Y != 0 ? 2 : 0 };
                pStateHash["SCALEHORIZ"] = new object[] { S.X, S.X != 0 ? 2 : 0 };
            }
            if (InvertXScaling && InvertYScaling) { S = new Vector2(-S.X, -S.Y); }
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
            lock (pStateHash.SyncRoot)
            {
                pStateHash["RED"] = new object[] { C.R, C.R != 0 ? 2 : 0 };
                pStateHash["GREEN"] = new object[] { C.G, C.G != 0 ? 2 : 0 };
                pStateHash["BLUE"] = new object[] { C.B, C.B != 0 ? 2 : 0 };
                pStateHash["ALPHA"] = new object[] { C.A, C.A != 0 ? 2 : 0 };
            }
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
        public ArrayList MyBehaviours { get; set; }
        public virtual void Update()
        {
            lock (pStateHash.SyncRoot)
            {
                foreach (object[] State in pStateHash.Values)
                {
                    if ((int)State[1] > 0)
                    {
                        State[1] = ((int)State[1]) - 1;
                        if((int)State[1] == 0) { State[0] = 0f; }
                    }
                }
            }
            foreach(Behaviours.IVNFBehaviour Component in MyBehaviours)
            {
                Component.UpdateFunctionality(this);
            }
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
            spriteBatch.Draw(LocalAtlas.Atlas, new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y), new Point((int)(LocalAtlas.FrameSize().X * pScale.X), (int)(LocalAtlas.FrameSize().Y * pScale.Y))), new Rectangle(new Point((LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X)*pAtlasCoordinates.X, (LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * pAtlasCoordinates.Y), LocalAtlas.FrameSize()), ColourValue, pRotation + FlipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth);
        }
        public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else { spriteBatch.Draw(LocalAtlas.Atlas, new Rectangle(VNFUtils.PointMultiply(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y) + camera.OffsetPoint, camera.ZoomFactor), VNFUtils.PointMultiply(new Point((int)(LocalAtlas.FrameSize().X * pScale.X), (int)(LocalAtlas.FrameSize().Y * pScale.Y)), camera.ZoomFactor)), new Rectangle(new Point((LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X) * pAtlasCoordinates.X, (LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * pAtlasCoordinates.Y), LocalAtlas.FrameSize()), ColourValue, pRotation + FlipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth); }
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
                return VNFUtils.ConvertVector(OffsetVector);
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
        public Vector2 TranslateCoordsToEquivalent(Vector2 GlobalCoords)
        {
            return (GlobalCoords / ZoomFactor) - OffsetVector;
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
                        LastMouseDragPos = Shell.CoordNormalize(VNFUtils.ConvertPoint(MyMouse.Position));
                        MouseDragging = true;
                    }
                    Vector2 CurrentMouseDragPos = Shell.CoordNormalize(VNFUtils.ConvertPoint(MyMouse.Position));
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
    [Serializable]
    public class Pane : WorldEntity
    {
        public Camera DefaultPaneCamera { get; set; }
        ArrayList UpdateQueue { get; set; }
        ArrayList RenderQueue { get; set; }
        ArrayList DeleteQueue { get; set; }
        public Color BackgroundColor { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        RenderTarget2D pRenderPane;
        public void AddUpdate(WorldEntity E)
        {
            E.CustomCamera = DefaultPaneCamera;
            E.UsePseudoMouse = true;
            UpdateQueue.Add(E);
        }
        public void AddRender(WorldEntity E)
        {
            E.CustomCamera = DefaultPaneCamera;
            E.UsePseudoMouse = true;
            RenderQueue.Add(E);
        }
        public void AddDelete(WorldEntity E)
        {
            DeleteQueue.Add(E);
        }
        public RenderTarget2D RenderPane
        {
            get { return pRenderPane; }
        }
        Point PaneBaseSize;
        public Pane(String Name, Vector2 Location, float Depth, Point PaneSize, Color BackgroundCol, GraphicsDevice MyGraphicsDevice) : base(Name, Location, null, Depth)
        {
            BackgroundColor = BackgroundCol;
            GraphicsDevice = MyGraphicsDevice;
            PaneBaseSize = PaneSize;
            DefaultPaneCamera = new Camera("CAMERA_PANE_" + Name);
            UpdateQueue = new ArrayList();
            RenderQueue = new ArrayList();
            DeleteQueue = new ArrayList();
            RenderAlways = true;
            AllowInternalInteracts = true;
            Render();
        }
        public Boolean AllowInternalInteracts { get; set; }
        protected override void EntityClickFunctionTrigger()
        {
            if(AllowInternalInteracts && MouseInBounds())
            {
                var MouseState = Mouse.GetState();
                Vector2 LocalPosition = LocalCoords(Shell.CoordNormalize(new Vector2(MouseState.X, MouseState.Y)));
                foreach (WorldEntity E in UpdateQueue)
                {
                    E.PseudoMouse = LocalPosition;
                    if (E.MouseInBounds()) { E.ClickTrigger(); }
                }
            }
            base.EntityClickFunctionTrigger();
        }
        ~Pane()
        {
            Clear();
        }
        public Vector2 LocalCoords(Vector2 GlobalCoords)
        {
            Vector2 V = GlobalCoords;
            Vector2 ZoomFactor = new Vector2(1, 1);
            if (!CameraImmune)
            {
                if (CustomCamera != null)
                {
                    V = CustomCamera.TranslateCoordsToEquivalent(V);
                }
                else if (Shell.AutoCamera != null)
                {
                    V = Shell.AutoCamera.TranslateCoordsToEquivalent(V);
                }
            }
            Vector2 Size = new Vector2((LocalAtlas.FrameSize().X * pScale.X), (LocalAtlas.FrameSize().Y * pScale.Y));
            Vector2 InternalOriginCoords = pCO ? pDrawCoords - (Size/2) : pDrawCoords;
            V = (V - InternalOriginCoords) / pScale;
            return V;
        }
        public void Clear()
        {
            foreach (WorldEntity E in UpdateQueue)
            {
                DeleteQueue.Add(E);
            }
            foreach (WorldEntity E in RenderQueue)
            {
                DeleteQueue.Add(E);
            }
            foreach (WorldEntity E in DeleteQueue)
            {
                if (UpdateQueue.Contains(E)) { UpdateQueue.Remove(E); }
                if (RenderQueue.Contains(E)) { RenderQueue.Remove(E); }
                E.ManualDispose();
            }
            DeleteQueue = new ArrayList();
        }
        Vector2 PanePseudoMouse = new Vector2(float.NaN, float.NaN);
        public Boolean RenderAlways { get; set; }
        public override void Update()
        {
            if (AllowInternalInteracts)
            {
                var MouseState = Mouse.GetState();
                PanePseudoMouse = LocalCoords(Shell.CoordNormalize(new Vector2(MouseState.X, MouseState.Y)));
            }
            else { PanePseudoMouse = new Vector2(float.NaN, float.NaN); }
            foreach(WorldEntity E in UpdateQueue)
            {
                E.PseudoMouse = PanePseudoMouse;
                E.Update();
            }
            foreach (WorldEntity E in DeleteQueue)
            {
                if (UpdateQueue.Contains(E)) { UpdateQueue.Remove(E); }
                if (RenderQueue.Contains(E)) { RenderQueue.Remove(E); }
                if (Shell.NonSerializables.Contains(E)) { Shell.NonSerializables.Remove(E); }
            }
            DeleteQueue = new ArrayList();
            base.Update();
        }
        public void Render()
        {
            if (pRenderPane == null || pRenderPane.Bounds.Size != PaneBaseSize)
            {
                if (pRenderPane != null) { pRenderPane.Dispose(); }
                pRenderPane = new RenderTarget2D(GraphicsDevice, PaneBaseSize.X, PaneBaseSize.Y, false,
                    GraphicsDevice.PresentationParameters.BackBufferFormat,
                    DepthFormat.Depth24);
            }
            GraphicsDevice.SetRenderTarget(pRenderPane);
            //Rectangle PreScissor = GraphicsDevice.ScissorRectangle;
            //GraphicsDevice.ScissorRectangle = new Rectangle(new Point(), PaneBaseSize/new Point(2, 2));
            GraphicsDevice.Clear(BackgroundColor);
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            foreach (WorldEntity E in RenderQueue)
            {
                if (E.Drawable)
                {
                    if (E.CustomCamera != null) { E.Draw(spriteBatch, E.CustomCamera); }
                    else { E.Draw(spriteBatch, DefaultPaneCamera); }
                }
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.ScissorRectangle = PreScissor;
            LocalAtlas = new TAtlasInfo();
            LocalAtlas.Atlas = RenderPane;
            LocalAtlas.DivDimensions = new Point(1, 1);
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
        public override Rectangle HitBox
        {
            get
            {
                pHitBox = new Rectangle(VNFUtils.ConvertVector(pDrawCoords), new Point(BufferLength, VerticalLength(true)));
                return pHitBox;
            }
        }
        protected RenderTarget2D[,] StaticTextures = null;
        protected Boolean pDrawAsStatic = false;
        public Boolean DrawAsStatic
        {
            get
            {
                return pDrawAsStatic;
            }
            set
            {
                if(pDrawAsStatic != value)
                {
                    if(!pDrawAsStatic)
                    {
                        StaticTextures = new RenderTarget2D[1, 1];
                        StaticTextures[0,0] = new RenderTarget2D(Shell.PubGD, 1000, 1000, false,
                            Shell.PubGD.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
                        StaticRender();
                    }
                    else
                    {
                        StaticTextures = null;
                    }
                    pDrawAsStatic = value;
                }
            }
        }
        public void StaticRender()
        {
            Point Dims = new Point((int)Math.Ceiling(HitBox.Width / 1000f), (int)Math.Ceiling(HitBox.Height / 1000f));
            RenderTarget2D[,] OutR = new RenderTarget2D[Dims.X, Dims.Y];
            for(int y = 0; y < Dims.Y; y++)
            {
                for(int x = 0; x < Dims.X; x++)
                {
                    if(x < StaticTextures.GetLength(0) && y < StaticTextures.GetLength(1) && StaticTextures[x, y] != null)
                    {
                        OutR[x, y] = StaticTextures[x, y];
                    }
                    else
                    {
                        OutR[x, y] = new RenderTarget2D(Shell.PubGD, 1000, 1000, false,
                            Shell.PubGD.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
                    }
                    GraphicsDevice TextGD = Shell.PubGD;
                    TextGD.SetRenderTarget(OutR[x, y]);
                    TextGD.Clear(Color.Transparent);
                    SpriteBatch spriteBatch = new SpriteBatch(TextGD);
                    spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                    DrawDynamic(spriteBatch, new Vector2(x * -1000, y * -1000));
                    spriteBatch.End();
                    Shell.PubGD.SetRenderTarget(null);
                }
            }
            StaticTextures = OutR;
        }
        public static TextChunk[] PreprocessText(String Text, Boolean ForceSplitUnchunkables)
        {
            return PreprocessText(Text, -1, ForceSplitUnchunkables);
        }
        public static TextChunk[] PreprocessText(String Text, int PixelBuffer, Boolean ForceSplitUnchunkables)
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
                                if (SSplit[1] == "SYSFONT")
                                {
                                    Font = (SpriteFont)Shell.SysFont;
                                }
                                else
                                {
                                    Font = (SpriteFont)Shell.Fonts[SSplit[1]];
                                }
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
            if(PixelBuffer != -1) { Out = LinebreakChunks(Out, PixelBuffer, ForceSplitUnchunkables); }
            return Out;
        }
        protected Boolean pForceSplitUnchunkables = false;
        public Boolean ForceSplitUnchunkables
        {
            get
            {
                return pForceSplitUnchunkables;
            }
            set
            {
                pForceSplitUnchunkables = value;
                TextChunkR = PreprocessText(Text, BufferLength, pForceSplitUnchunkables);
            }
        }
        public static TextChunk[] LinebreakChunks(TextChunk[] Initial, int MaxPixelLineLength, Boolean ForceSplitUnchunkables) //Function to insert linebreaks into text as required, based on a given length
        {
            int CurrentPixelTotal = 0;
            Vector2 RollingLocationMod = new Vector2(0, 0); //Vector representing the degree to which the next line should be shifted from the previous
            Vector2 StackableVector2 = new Vector2(0, 0); //Vector representing the degree to which all subsequent text chunks should be additionally shifted by auto-inserted line breaks
            Vector2 LinebreakAmmendment = new Vector2(); //Vector representing the degree to which all subsequent text chunks should be additionally shifted due to manual line breaks
            ArrayList RebuildChunks = new ArrayList(); //The new set of chunks broken up correctly to include line breaks
            ArrayList NewChunkRegistry = new ArrayList(); //Collection of new text chunks created by auto-inserted line breaks, that will be depopulated as they are processed
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
                    Boolean ByPix = false;
                    //ByPix: whether the next chunk will take the line length over the edge
                    if (CurrentPixelTotal + (int)TC.Font.MeasureString(TC.Text).X + PixFromNextTotal > MaxPixelLineLength && TC.Font.MeasureString(TC.Text).X + CurrentPixelTotal <= MaxPixelLineLength) { ByPix = true; }
                    if (TC.Text.Contains(' ')) //First we check for a space to see if a linebreak can be performed in the current text chunk
                    {
                        String FindLoc = TC.Text;
                        int FoundLocation = -1;
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
                            FoundLocation = TC.Text.IndexOf(' ');
                        }
                        //A new text chunk is created to represent the text after the line break
                        TextChunk New = TC;
                        New.TimeDelay = 0;
                        New.Text = New.Text.Remove(0, FoundLocation + 1);
                        if(New.Text.Length > 0 && New.Text[0] == ' ') { New.Text = New.Text.Remove(0, 1); }
                        int TCMeasure = 0;
                        TC.Text = TC.Text.Remove(FoundLocation);
                        TCMeasure = (int)TC.Font.MeasureString(TC.Text + " ").X; //The length of the first half of the new, broken line is measured
                        RollingLocationMod.X = -CurrentPixelTotal - TCMeasure; //RollingLocationMod is used to modify the position of the new text chunk, and also to later transfer this information to the SV2 vector
                        New.DrawLocation.X += TCMeasure; //The length is added back on, as the New text chunk is starting from the initial location of the old one (TC) before being shifted back by RLM, so it must be adjusted for the fact that it comes after TC
                        RollingLocationMod.Y = (int)TC.Font.MeasureString(" ").Y; //And the "new" chunk is also shifted down
                        CurrentPixelTotal = 0;
                        RebuildChunks.Add(TC); //The current text chunk is added to the new, final list of chunks                       
                        NewChunkRegistry.Add(New);
                    }
                    else //Else, if text cannot be split...
                    {
                        if (!ByPix && ForceSplitUnchunkables)
                        {
                            StringBuilder FindLoc = new StringBuilder(TC.Text);
                            //Similar to above, but spaces are not looked for
                            while (TC.Font.MeasureString(FindLoc).X + CurrentPixelTotal > MaxPixelLineLength)
                            {
                                FindLoc.Remove(FindLoc.Length - 1, 1);
                            }
                            int FoundLocation = FindLoc.Length;
                            //A new text chunk is created to represent the text after the line break
                            TextChunk New = TC;
                            New.TimeDelay = 0;
                            New.Text = New.Text.Remove(0, FoundLocation);
                            int TCMeasure = 0;
                            TC.Text = TC.Text.Remove(FoundLocation);
                            TCMeasure = (int)TC.Font.MeasureString(TC.Text).X; //The length of the first half of the new, broken line is measured
                            RollingLocationMod.X = -CurrentPixelTotal - TCMeasure; //RollingLocationMod is used to modify the position of the new text chunk, and also to later transfer this information to the SV2 vector
                            New.DrawLocation.X += TCMeasure; //The length is added back on, as the New text chunk is starting from the initial location of the old one (TC) before being shifted back by RLM, so it must be adjusted for the fact that it comes after TC
                            RollingLocationMod.Y = (int)TC.Font.MeasureString(" ").Y; //And the "new" chunk is also shifted down
                            CurrentPixelTotal = 0;
                            RebuildChunks.Add(TC); //The current text chunk is added to the new, final list of chunks                       
                            NewChunkRegistry.Add(New);
                        }
                        else
                        {
                            RebuildChunks.Add(TC); //Ignore split and proceed as normal as it is not possible to split, hoping that the next text chunk will be splitable.
                            CurrentPixelTotal += (int)TC.Font.MeasureString(TC.Text).X;
                        }
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
                if (pText != value)
                {
                    pText = value;
                    TextChunkR = new TextChunk[0];
                    IgnoreDelayOnThis = new ArrayList();
                    TextChunkR = PreprocessText(value, BufferLength, pForceSplitUnchunkables);
                    int L = 0;
                    foreach (TextChunk TCO in TextChunkR) { L += TCO.Text.Length; }
                    pLength = L;
                    if (pDrawAsStatic)
                    {
                        StaticRender();
                    }
                }
            }
        }
        public TextEntity(String Name, String TextIn, Vector2 Location, float Depth) : base(Name, Location, null, Depth)
        {
            BufferLength = 1000;
            TextChunkR = PreprocessText(TextIn, BufferLength, false);
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
            if (pDrawAsStatic) { StaticRender(); }
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
        public int ChunkCount
        {
            get { return TextChunkR.Length; }
        }
        protected float[] pChunkFontHeight = null;
        public float[] ChunkFontHeight
        {
            get
            {
                pChunkFontHeight = new float[TextChunkR.Length];
                int i = 0;
                foreach(TextChunk T in TextChunkR)
                {
                    pChunkFontHeight[i] = T.Font.MeasureString(" ").Y;
                    i++;
                }
                return pChunkFontHeight;
            }
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
                    if (pDrawAsStatic) { StaticRender(); }
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
            if(DrawAsStatic)
            {
                DrawStatic(spriteBatch);
            }
            else
            {
                DrawDynamic(spriteBatch, (Vector2?)null);
            }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (DrawAsStatic)
            {
                DrawStatic(spriteBatch, camera);
            }
            else
            {
                DrawDynamic(spriteBatch, camera);
            }
        }
        public void DrawDynamic(SpriteBatch spriteBatch, Vector2? ManualNormalizer)
        {
            if (!TypeWrite)
            {
                foreach (TextChunk TC in TextChunkR)
                {
                    if(TC.RainbowMode) { TC.Rainbow(); }
                    spriteBatch.DrawString(TC.Font, TC.Text, new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + (ManualNormalizer ?? DrawCoords - pOrigin), TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), 1f, SpriteEffects.None, LayerDepth);
                }
            }
            else
            {
                foreach (TextChunk TC in ProgressiveChunks)
                {
                    if (TC.RainbowMode) { TC.Rainbow(); }
                    spriteBatch.DrawString(TC.Font, TC.Text, new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + (ManualNormalizer ?? DrawCoords - pOrigin), TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), 1f, SpriteEffects.None, LayerDepth);
                }
            }
        }
        public void DrawDynamic(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                if (!TypeWrite)
                {
                    foreach (TextChunk TC in TextChunkR)
                    {
                        if (TC.RainbowMode) { TC.Rainbow(); }
                        spriteBatch.DrawString(TC.Font, TC.Text, (new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + DrawCoords - pOrigin + camera.OffsetVector) * camera.ZoomFactor, TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), camera.ZoomFactor.X, SpriteEffects.None, LayerDepth);
                    }
                }
                else
                {
                    foreach (TextChunk TC in ProgressiveChunks)
                    {
                        if (TC.RainbowMode) { TC.Rainbow(); }
                        spriteBatch.DrawString(TC.Font, TC.Text, (new Vector2(TC.DrawLocation.X, TC.DrawLocation.Y) + DrawCoords - pOrigin + camera.OffsetVector) * camera.ZoomFactor, TC.Colour * (pColour.A / 255f), 0f, new Vector2(0, 0), camera.ZoomFactor.X, SpriteEffects.None, LayerDepth);
                    }
                }
            }
        }
        public void DrawStatic(SpriteBatch spriteBatch)
        {
            if (StaticTextures != null)
            {
                for(int x = 0; x < StaticTextures.GetLength(0); x++)
                {
                    for (int y = 0; y < StaticTextures.GetLength(1); y++)
                    {
                        spriteBatch.Draw(StaticTextures[x, y], new Rectangle(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y) - VNFUtils.ConvertVector(pOrigin) + VNFUtils.PointMultiply(new Point(x * 1000, y * 1000), pScale), VNFUtils.PointMultiply(StaticTextures[x, y].Bounds.Size, pScale)), StaticTextures[x, y].Bounds, Color.White, 0f, new Vector2(), SpriteEffects.None, LayerDepth);
                    }
                }
            }
        }
        public void DrawStatic(SpriteBatch spriteBatch, Camera camera)
        {
            if (StaticTextures != null)
            {
                for (int x = 0; x < StaticTextures.GetLength(0); x++)
                {
                    for (int y = 0; y < StaticTextures.GetLength(1); y++)
                    {
                        spriteBatch.Draw(StaticTextures[x, y], new Rectangle(VNFUtils.PointMultiply((new Point((int)pDrawCoords.X, (int)pDrawCoords.Y) - VNFUtils.ConvertVector(pOrigin) + VNFUtils.PointMultiply(new Point(x * 1000, y * 1000), pScale) + VNFUtils.ConvertVector(camera.OffsetVector)), camera.ZoomFactor), VNFUtils.PointMultiply(VNFUtils.PointMultiply(StaticTextures[x, y].Bounds.Size, VNFUtils.ConvertVector(pScale)), camera.ZoomFactor)), StaticTextures[x, y].Bounds, Color.White, 0f, new Vector2(), SpriteEffects.None, LayerDepth);
                    }
                }
            }
        }
    }
}
