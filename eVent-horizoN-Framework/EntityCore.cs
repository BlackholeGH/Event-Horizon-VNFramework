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
using static VNFramework.GraphicsTools;

namespace VNFramework
{

    /// <summary>
    /// The base entity class for all objects that can be added to the VNF game environment, and which function with the Shell's update and render queues.
    /// </summary>
    [Serializable]
    public class WorldEntity
    {
        private Boolean _overlayUtility = false;
        public Boolean OverlayUtility
        {
            get
            {
                return _overlayUtility;
            }
            set
            {
                _overlayUtility = value;
                if(_overlayUtility && !Shell.NonSerializables.Contains(this)) { Shell.NonSerializables.Add(this); }
                else if (!_overlayUtility && Shell.NonSerializables.Contains(this)) { Shell.NonSerializables.Remove(this); }
            }
        }
        private Boolean _isUIElement = false;
        public Boolean IsUIElement
        {
            get
            {
                return _isUIElement;
            }
            set
            {
                _isUIElement = value;
                if (_isUIElement) { CameraImmune = true; }
            }
        }
        public static ulong IDIterator = 0;
        private ulong _entityID;
        [field: NonSerialized]
        private int _updatetime = Environment.TickCount;
        protected int Updatetime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }
        private String _name;
        private Boolean _drawable = true;
        private TAtlasInfo _localAtlas;
        private Vector2 _position;
        private Point _atlasCoordinates = new Point(0, 0);
        public Point AtlasCoordinates
        {
            get { return _atlasCoordinates; }
            protected set { _atlasCoordinates = value; }
        }
        private float _rotation = 0f;
        public float RotationRads
        {
            get { return _rotation; }
            protected set { _rotation = value; }
        }
        public GraphicsTools.Trace ForwardTrace
        {
            get
            {
                return new GraphicsTools.Trace(new Vector2(), RotationRads, 1);
            }
        }
        private Vector2 _size = new Vector2(1, 1);
        public Vector2 Size {
            get { return _size; }
            protected set { _size = value; }
        }
        private Vector2 _origin = new Vector2();
        private Boolean _centerOrigin = false;
        private ColourShift _colour = new ColourShift(255f, 255f, 255f, 255f);
        public Color ColourValue
        {
            get
            {
                return new Color((byte)Math.Round(_colour.R), (byte)Math.Round(_colour.G), (byte)Math.Round(_colour.B), (byte)Math.Round(_colour.A));
            }
            set
            {
                Color colour = value;
                _colour = ColourShift.Constrain(new ColourShift(colour.R, colour.G, colour.B, colour.A));
            }
        }
        /// <summary>
        /// Represents a custom Camera entity that can be assigned to render this WorldEntity.
        /// </summary>
        public Camera CustomCamera { get; set; }
        public Boolean CameraImmune { get; set; }
        public TAtlasInfo Atlas
        {
            get { return _localAtlas; }
            protected set { _localAtlas = value; }
        }
        public Boolean SetAtlasFrame(Point coords)
        {
            if (coords.X < Atlas.DivDimensions.X && coords.Y < Atlas.DivDimensions.Y)
            {
                AtlasCoordinates = coords;
                return true;
            }
            return false;
        }
        public void SetManualOrigin(Vector2 origin)
        {
            _origin = origin;
        }
        public Boolean CenterOrigin
        {
            get
            {
                return _centerOrigin;
            }
            set
            {
                _centerOrigin = value;
                if (_centerOrigin == true)
                {
                    _origin = VNFUtils.ConvertPoint(Hitbox.Size) / 2;
                }
                else
                {
                    _origin = new Vector2();
                }
            }
        }
        public Boolean SuppressClickable { get; set; }
        //It should be noted that the event subscription register only holds the entity name of the publishing entity, as entIDs are reassigned after deserialization.
        //Due to this, if a specific event subscription is required, the publisher should be ensured to have a unique name string.
        [Serializable]
        public struct EventSubRegister
        {
            public EventSubRegister(String publisherEntName, EventNames eventName, MethodInfo eventHandler, object[] methodArgs)
            {
                this.PublisherEntName = publisherEntName;
                this.EventName = eventName;
                this.EventHandler = eventHandler;
                this.MethodArgs = methodArgs;
            }
            public String PublisherEntName;
            public EventNames EventName;
            public MethodInfo EventHandler;
            public object[] MethodArgs;
        }
        private List<EventSubRegister> _subscribedEvents = new List<EventSubRegister>();
        public List<EventSubRegister> SubscribedEvents
        {
            get
            {
                return _subscribedEvents;
            }
            set
            {
                _subscribedEvents = value;
            }
        }
        public void SubscribeToEvent(EventNames eventName, MethodInfo eventHandler, object[] methodArgs)
        {
            SubscribeToEvent(this, eventName, eventHandler, methodArgs);
        }
        public void SubscribeToEvent(WorldEntity eventPublisher, EventNames eventName, MethodInfo eventHandler, object[] methodArgs)
        {
            SubscribedEvents.Add(new EventSubRegister(eventPublisher.Name, eventName, eventHandler, methodArgs));
            EventSubscribeActual(eventPublisher, eventName, eventHandler, methodArgs);
        }
        [field: NonSerialized]
        private Dictionary<WorldEntity, ArrayList> _trueDetachers = new Dictionary<WorldEntity, ArrayList>();
        private void EventSubscribeActual(WorldEntity eventPublisher, EventNames eventName, MethodInfo eventHandler, object[] methodArgs)
        {
            VoidDel thisHandler = new VoidDel(delegate () {
                eventHandler.Invoke(this, methodArgs);
            });
            if(!_trueDetachers.ContainsKey(eventPublisher))
            {
                _trueDetachers.Add(eventPublisher, new ArrayList());
            }
            ((ArrayList)_trueDetachers[eventPublisher]).Add(new object[] { eventName, thisHandler });
            EventCoupleDecouple(eventPublisher, eventName, thisHandler, true);
        }
        [Serializable]
        public enum EventNames { EntityClickFunction, ButtonPressFunction, ButtonHoverFunction, ButtonHoverReleaseFunction, SliderClickFunction, ScrollBarClickFunction, TextEnteredFunction };
        public void EventCoupleDecouple(WorldEntity eventPublisher, EventNames eventName, VoidDel handler, Boolean subscribe)
        {
            switch (eventName)
            {
                case EventNames.EntityClickFunction:
                    if (subscribe) { eventPublisher.EntityClickFunction += handler; }
                    else { eventPublisher.EntityClickFunction -= handler; }
                    break;
                case EventNames.ButtonPressFunction:
                    if (eventPublisher is Button)
                    {
                        Button button = (Button)eventPublisher;
                        if (subscribe) { button.ButtonPressFunction += handler; }
                        else { button.ButtonPressFunction -= handler; }
                    }
                    break;
                case EventNames.ButtonHoverFunction:
                    if (eventPublisher is Button)
                    {
                        Button button = (Button)eventPublisher;
                        if (subscribe) { button.ButtonHoverFunction += handler; }
                        else { button.ButtonHoverFunction -= handler; }
                    }
                    break;
                case EventNames.ButtonHoverReleaseFunction:
                    if (eventPublisher is Button)
                    {
                        Button button = (Button)eventPublisher;
                        if (subscribe) { button.ButtonHoverReleaseFunction += handler; }
                        else { button.ButtonHoverReleaseFunction -= handler; }
                    }
                    break;
                case EventNames.SliderClickFunction:
                    if (eventPublisher is Slider)
                    {
                        Slider slider = (Slider)eventPublisher;
                        if (subscribe) { slider.SliderClickFunction += handler; }
                        else { slider.SliderClickFunction -= handler; }
                    }
                    break;
                case EventNames.ScrollBarClickFunction:
                    if (eventPublisher is ScrollBar)
                    {
                        ScrollBar scrollbar = (ScrollBar)eventPublisher;
                        if (subscribe) { scrollbar.ScrollBarClickFunction += handler; }
                        else { scrollbar.ScrollBarClickFunction -= handler; }
                    }
                    else if (eventPublisher is VerticalScrollPane)
                    {
                        VerticalScrollPane scrollpane = (VerticalScrollPane)eventPublisher;
                        if (subscribe) { scrollpane.ScrollBarClickFunction += handler; }
                        else { scrollpane.ScrollBarClickFunction -= handler; }
                    }
                    break;
                case EventNames.TextEnteredFunction:
                    if (eventPublisher is ITextInputReceiver)
                    {
                        ITextInputReceiver textfield = (ITextInputReceiver)eventPublisher;
                        if (subscribe) { textfield.TextEnteredFunction += handler; }
                        else { textfield.TextEnteredFunction -= handler; }
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
        private Texture2D _serializationBackup = null;
        public virtual void OnSerializeDo()
        {
            foreach (Animation A in AnimationQueue) { A.TimeHang(); }
            if (Atlas.ReferenceHash == "" || Atlas.ReferenceHash == null)
            {
                _serializationBackup = Atlas.Atlas;
            }
        }
        public void ResubscribeEvents()
        {
            if(_trueDetachers != null && _trueDetachers.Count > 0)
            {
                UnsubscribeEvents();
            }
            List<EventSubRegister> localSubscribedEvents = SubscribedEvents;
            SubscribedEvents = new List<EventSubRegister>();
            foreach (EventSubRegister esr in localSubscribedEvents)
            {
                WorldEntity publisher = Shell.GetEntityByName(esr.PublisherEntName);
                if(publisher != null)
                {
                    SubscribeToEvent(publisher, esr.EventName, esr.EventHandler, esr.MethodArgs);
                }
            }
        }
        private void UnsubscribeEvents()
        {
            if (_trueDetachers != null)
            {
                foreach (WorldEntity worldEntity in _trueDetachers.Keys)
                {
                    foreach (object[] pair in _trueDetachers[worldEntity])
                    {
                        EventCoupleDecouple(worldEntity, (EventNames)pair[0], (VoidDel)pair[1], false);
                    }
                }
            }
            _trueDetachers = new Dictionary<WorldEntity, ArrayList>();
        }
        public virtual void OnDeserializeDo()
        {
            AddEventTriggers();
            if(MyBehaviours is null) { MyBehaviours = new List<Behaviours.IVNFBehaviour>(); }
            foreach (Animation animation in AnimationQueue)
            {
                animation.ReRegisterSelf();
                animation.UnHang();
                if (animation.Started && animation.TimeElapsed > 100) { animation.Jump(this); }
            }
            if (_serializationBackup != null)
            {
                TAtlasInfo atlas = Atlas;
                atlas.Atlas = _serializationBackup;
                Atlas = atlas;
                _serializationBackup = null;
            }
            else if (Atlas.ReferenceHash != "" && Atlas.ReferenceHash != null)
            {
                TAtlasInfo atlas = Atlas;
                atlas.Atlas = Shell.AtlasDirectory[Atlas.ReferenceHash].Atlas;
                Atlas = atlas;
            }
            _trueDetachers = new Dictionary<WorldEntity, ArrayList>();
        }
        private Rectangle _hitbox = new Rectangle(0, 0, 0, 0);
        public virtual Rectangle Hitbox
        {
            get
            {
                Point atlasSize = new Point((int)(Atlas.FrameSize().X * Size.X), (int)(Atlas.FrameSize().Y * Size.Y));
                if (!CenterOrigin) { _hitbox = new Rectangle(new Point((int)Position.X, (int)Position.Y), atlasSize); }
                else { _hitbox = new Rectangle(new Point((int)Position.X, (int)Position.Y) - new Point(atlasSize.X / 2, atlasSize.Y / 2), atlasSize); }
                return _hitbox;
            }
            protected set { _hitbox = value; }
        }
        private ICollider _collider = null;
        public ICollider Collider
        {
            get
            {
                ICollider adjustedCollider = _collider != null ? _collider.Scale(new Vector2(), Size).Rotate(new Vector2(), RotationRads).Translate(Position) : null;
                return adjustedCollider;
            }
            protected set { _collider = value; }
        }
        public List<Animation> AnimationQueue { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is WorldEntity) { return Equals((WorldEntity)obj); }
            else { return false; }
        }
        public override int GetHashCode()
        {
            return (int)(EntityID % (UInt32.MaxValue - Int32.MaxValue));
        }
        public Boolean Equals(WorldEntity b)
        {
            if (b is null) { return false; }
            if (b.EntityID == EntityID) { return true; }
            else { return false; }
        }
        public static Boolean operator ==(WorldEntity a, WorldEntity b)
        {
            if (a is null && b is null) { return true; }
            else if (a is null ^ b is null) { return false; }
            return a.Equals(b);
        }
        public static Boolean operator !=(WorldEntity a, WorldEntity b)
        {
            if (a is null && b is null) { return false; }
            else if (a is null ^ b is null) { return true; }
            return !a.Equals(b);
        }
        public Vector2 PseudoMouse { get; set; }
        public Boolean UsePseudoMouse { get; set; }
        public Boolean MouseInBounds()
        {
            if(Atlas.Atlas == null) { return false; }
            var mouseState = Mouse.GetState();
            //return HitBox.Contains(new Vector2(MouseState.X, MouseState.Y));
            Vector2 normalizedMouseVector;
            if (UsePseudoMouse)
            {
                normalizedMouseVector = PseudoMouse;
            }
            else { normalizedMouseVector = Shell.CoordNormalize(new Vector2(mouseState.X, mouseState.Y)); }
            return TextureAwareInBounds(normalizedMouseVector);
        }
        public Boolean TextureAwareInBounds(Vector2 vector)
        {
            if (!CameraImmune)
            {
                if (CustomCamera != null)
                {
                    vector = CustomCamera.TranslateCoordsToEquivalent(vector);
                }
                else if (Shell.AutoCamera != null)
                {
                    vector = Shell.AutoCamera.TranslateCoordsToEquivalent(vector);
                }
            }
            if (Hitbox.Contains(vector))
            {
                Texture2D myAtlas = Atlas.Atlas;
                Color[] rawAtlas = new Color[myAtlas.Width * myAtlas.Height];
                myAtlas.GetData<Color>(rawAtlas);
                Color[,] orderedAtlas = new Color[myAtlas.Width, myAtlas.Height];
                Point addCoord = new Point(0, 0);
                foreach (Color colour in rawAtlas)
                {
                    if (addCoord.X >= myAtlas.Width)
                    {
                        addCoord = new Point(0, addCoord.Y + 1);
                    }
                    orderedAtlas[addCoord.X, addCoord.Y] = colour;
                    addCoord += new Point(1, 0);
                }
                Vector2 publicCorner = new Vector2(Hitbox.X, Hitbox.Y);
                Vector2 localCoord = (vector - publicCorner) / Size;
                Vector2 atlasConformity = new Vector2(((float)Atlas.SourceRect.Width / Atlas.DivDimensions.X) * AtlasCoordinates.X, ((float)Atlas.SourceRect.Height / Atlas.DivDimensions.Y) * AtlasCoordinates.Y);
                localCoord += atlasConformity;
                Color comparitor = orderedAtlas[(int)localCoord.X, (int)localCoord.Y];
                if (comparitor.A != 0) { return true; }
                else { return false; }
            }
            return false;
        }
        public float LayerDepth { get; set; }
        public Boolean TransientAnimation { get; set; }
        public void ReissueID()
        {
            EntityID = IDIterator;
            IDIterator++;
        }
        public WorldEntity(String name, Vector2 location, TAtlasInfo? atlas, float depth)
        {
            EntityID = IDIterator;
            IDIterator++;
            TransientAnimation = false;
            ManualHorizontalFlip = false;
            Name = name;
            Position = location;
            LayerDepth = depth;
            CustomCamera = null;
            CameraImmune = false;
            InitStateHash();
            AddEventTriggers();
            PseudoMouse = new Vector2(float.NaN, float.NaN);
            if (atlas != null)
            {
                Atlas = (TAtlasInfo)atlas;
                Hitbox = new Rectangle(new Point((int)location.X, (int)location.Y), Atlas.FrameSize());
            }
            AnimationQueue = new List<Animation>();
            MyStickers = new List<WorldEntity>();
            MyBehaviours = new List<Behaviours.IVNFBehaviour>();
            MyVertexRenderables = new List<VertexRenderable>();
        }
        ~WorldEntity()
        {
            ManualDispose();
        }
        public virtual void ManualDispose()
        {
            RemoveEventTriggers();
            UnsubscribeEvents();
            if(Shell.NonSerializables.Contains(this)) { Shell.NonSerializables.Remove(this); }
            foreach (Animation animation in AnimationQueue)
            {
                animation.AutoWipe();
            }
            AnimationQueue = new List<Animation>();
            foreach (Behaviours.IVNFBehaviour behaviour in MyBehaviours)
            {
                behaviour.Clear();
            }
            MyBehaviours = new List<Behaviours.IVNFBehaviour>();
            MyVertexRenderables = new List<VertexRenderable>();
        }
        public ulong EntityID
        {
            get { return _entityID; }
            protected set { _entityID = value; }
        }
        public String Name
        {
            get { return _name; }
            protected set { _name = value; }
        }
        public Vector2 Position
        {
            get { return _position; }
            protected set { _position = value; }
        }
        public Trace TraceTo(Vector2 coords)
        {
            return new Trace(Position, coords);
        }
        public void QuickMoveTo(Vector2 coords)
        {
            Position = coords;
        }
        public Boolean Drawable
        {
            get
            {
                return _drawable;
            }
            set
            {
                _drawable = value;
            }
        }
        /// <summary>
        /// The EntityStates hashtable contains information about different ways a WorldEntity is behaving, such as how it is moving or shifting in the world. This can be used as a reference for animation controllers.
        /// </summary>
        public Dictionary<string, object> EntityStates
        {
            get
            {
                Dictionary<string, object> statesReal = new Dictionary<string, object>();
                lock(((ICollection)_stateHash).SyncRoot)
                {
                    foreach(String k in _stateHash.Keys)
                    {
                        statesReal.Add(k, _stateHash[k][0]);
                    }
                }
                return statesReal;
            }
        }
        private Dictionary<string, object[]> _stateHash = new Dictionary<string, object[]>();
        private void InitStateHash()
        {
            
            String[] State = new String[] { "NORTHSOUTH", "EASTWEST", "ROTATION", "SCALEHORIZ", "SCALEVERT", "RED", "GREEN", "BLUE", "ALPHA" };
            lock (((ICollection)_stateHash).SyncRoot)
            {
                foreach (String S in State)
                {
                    _stateHash.Add(S, new object[] { 0f, 0 });
                }
            }
        }
        public List<WorldEntity> MyStickers { get; set; }
        public void Move(Vector2 vector)
        {
            Position += vector;
            lock (((ICollection)_stateHash).SyncRoot)
            {
                _stateHash["NORTHSOUTH"] = new object[] { vector.Y, vector.Y != 0 ? 2 : 0 };
                _stateHash["EASTWEST"] = new object[] { vector.X, vector.X != 0 ? 2 : 0 };
            }
            if (MyStickers != null && MyStickers.Count > 0)
            {
                foreach(WorldEntity sticker in MyStickers)
                {
                    sticker.Move(vector);
                }
            }
        }
        public void Rotate(float rotation)
        {
            RotationRads += rotation;
            lock (((ICollection)_stateHash).SyncRoot)
            {
                _stateHash["ROTATION"] = new object[] { rotation, rotation != 0 ? 2 : 0 };
            }
            if (MyStickers != null && MyStickers.Count > 0)
            {
                foreach (WorldEntity sticker in MyStickers)
                {
                    sticker.Rotate(rotation);
                }
            }
        }
        public void Scale(Vector2 scale)
        {
            lock (((ICollection)_stateHash).SyncRoot)
            {
                _stateHash["SCALEVERT"] = new object[] { scale.Y, scale.Y != 0 ? 2 : 0 };
                _stateHash["SCALEHORIZ"] = new object[] { scale.X, scale.X != 0 ? 2 : 0 };
            }
            if (_invertXScaling && _invertYScaling) { scale = new Vector2(-scale.X, -scale.Y); }
            else if (_invertXScaling) { scale = new Vector2(-scale.X, scale.Y); }
            else if (_invertYScaling) { scale = new Vector2(scale.X, -scale.Y); }
            Vector2 manualSet = new Vector2();
            if ((Size + scale).X < 0)
            {
                _autoHorizontalFlip = !_autoHorizontalFlip;
                _invertXScaling = !_invertXScaling;
                manualSet = new Vector2(-(scale.X + Size.X), manualSet.Y);
            }
            if ((Size + scale).Y < 0)
            {
                _autoVerticalFlip = !_autoVerticalFlip;
                _invertYScaling = !_invertYScaling;
                manualSet = new Vector2(manualSet.X, -(scale.Y + Size.Y));
            }
            if(manualSet == new Vector2()) { Size += scale; }
            else
            {
                if (manualSet.X != 0f) { Size = new Vector2(manualSet.X, Size.Y); }
                else { Size = new Vector2(Size.X + scale.X, Size.Y); }
                if (manualSet.Y != 0f) { Size = new Vector2(Size.X, manualSet.Y); }
                else { Size = new Vector2(Size.X, Size.Y + scale.Y); }
            }
            //Shell.WriteLine(pName + ": Scale now set to: " + _scale.X + ", " + _scale.Y);
            if (MyStickers != null && MyStickers.Count > 0)
            {
                foreach (WorldEntity sticker in MyStickers)
                {
                    sticker.Scale(scale);
                }
            }
        }
        public void Colour(ColourShift colourShift)
        {
            _colour = ColourShift.Constrain(_colour + colourShift);
            lock (((ICollection)_stateHash).SyncRoot)
            {
                _stateHash["RED"] = new object[] { colourShift.R, colourShift.R != 0 ? 2 : 0 };
                _stateHash["GREEN"] = new object[] { colourShift.G, colourShift.G != 0 ? 2 : 0 };
                _stateHash["BLUE"] = new object[] { colourShift.B, colourShift.B != 0 ? 2 : 0 };
                _stateHash["ALPHA"] = new object[] { colourShift.A, colourShift.A != 0 ? 2 : 0 };
            }
            if (MyStickers != null && MyStickers.Count > 0)
            {
                foreach (WorldEntity sticker in MyStickers)
                {
                    sticker.Colour(colourShift);
                }
            }
        }
        private Boolean _invertXScaling = false;
        private Boolean _invertYScaling = false;
        public Boolean[] CheckScaleInversions()
        {
            return new Boolean[] { _invertXScaling, _invertYScaling };
        }
        protected float MirrorOriginX
        {
            get
            {
                float xShift = -((Hitbox.X / 2f) - _origin.X);
                return (Hitbox.X / 2f) + xShift;
            }
        }
        protected float MirrorOriginY
        {
            get
            {
                float yShift = -((Hitbox.Y / 2f) - _origin.Y);
                return (Hitbox.Y / 2f) + yShift;
            }
        }
        protected Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }
        public Vector2 AdjustedOrigin
        {
            get
            {
                Vector2 originOut = _origin;
                if(_invertXScaling) { originOut.X = MirrorOriginX; }
                if(_invertYScaling) { originOut.Y = MirrorOriginY; }
                return originOut;
            }
        }
        private float _flipRotationAddit = 0f;
        public Boolean ManualHorizontalFlip { get; set; }
        public Boolean ManualVerticalFlip { get; set; }
        private Boolean _autoHorizontalFlip = false;
        private Boolean _autoVerticalFlip = false;
        private Boolean TrueHorizontalFlip { get { return ManualHorizontalFlip ^ _autoHorizontalFlip; } }
        private Boolean TrueVerticalFlip { get { return ManualVerticalFlip ^ _autoVerticalFlip; } }
        public List<Behaviours.IVNFBehaviour> MyBehaviours { get; set; }
        public virtual void Update()
        {
            lock (((ICollection)_stateHash).SyncRoot)
            {
                foreach (object[] State in _stateHash.Values)
                {
                    if ((int)State[1] > 0)
                    {
                        State[1] = ((int)State[1]) - 1;
                        if((int)State[1] == 0) { State[0] = 0f; }
                    }
                }
            }
            foreach(Behaviours.IVNFBehaviour behaviour in MyBehaviours)
            {
                behaviour.UpdateFunctionality(this);
            }
            foreach(Animation animation in AnimationQueue)
            {
                if (!ButtonScripts.Paused)
                {
                    if(animation.PlacedInPauseState)
                    {
                        animation.UnHang();
                        animation.PlacedInPauseState = false;
                    }
                    animation.Step(this);
                }
                else
                {
                    if(!animation.PlacedInPauseState)
                    {
                        animation.TimeHang();
                        animation.PlacedInPauseState = true;
                    }
                }
            }
            for(int i = 0; i < AnimationQueue.Count; i++)
            {
                Animation removeAnim = AnimationQueue[i];
                if (removeAnim.Spent)
                {
                    removeAnim.AutoWipe();
                    AnimationQueue.Remove(removeAnim);
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
                if(_flipRotationAddit == 0f) { _flipRotationAddit = (float)Math.PI; }
            }
            else
            {
                if (_flipRotationAddit != 0f) { _flipRotationAddit = 0f; }
                if (TrueHorizontalFlip && LocalSpriteEffect != SpriteEffects.FlipHorizontally) { LocalSpriteEffect = SpriteEffects.FlipHorizontally; }
                else if (TrueVerticalFlip && LocalSpriteEffect != SpriteEffects.FlipVertically) { LocalSpriteEffect = SpriteEffects.FlipVertically; }
                else if (!TrueHorizontalFlip && !TrueVerticalFlip && LocalSpriteEffect != SpriteEffects.None) { LocalSpriteEffect = SpriteEffects.None; }
            }
        }
        public List<GraphicsTools.VertexRenderable> MyVertexRenderables { get; set; }
        protected SpriteEffects LocalSpriteEffect = SpriteEffects.None;
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Atlas.Atlas, new Rectangle(new Point((int)Position.X, (int)Position.Y), new Point((int)(Atlas.FrameSize().X * Size.X), (int)(Atlas.FrameSize().Y * Size.Y))), new Rectangle(new Point((Atlas.SourceRect.Width / Atlas.DivDimensions.X)*AtlasCoordinates.X, (Atlas.SourceRect.Height / Atlas.DivDimensions.Y) * AtlasCoordinates.Y), Atlas.FrameSize()), ColourValue, RotationRads + _flipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth);
        }
        public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else { spriteBatch.Draw(Atlas.Atlas, new Rectangle(VNFUtils.PointMultiply(new Point((int)Position.X, (int)Position.Y) + camera.OffsetPoint, camera.ZoomFactor), VNFUtils.PointMultiply(new Point((int)(Atlas.FrameSize().X * Size.X), (int)(Atlas.FrameSize().Y * Size.Y)), camera.ZoomFactor)), new Rectangle(new Point((Atlas.SourceRect.Width / Atlas.DivDimensions.X) * AtlasCoordinates.X, (Atlas.SourceRect.Height / Atlas.DivDimensions.Y) * AtlasCoordinates.Y), Atlas.FrameSize()), ColourValue, RotationRads + _flipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth); }
          //else { spriteBatch.Draw(LocalAtlas.Atlas, new Rectangle(VNFUtils.PointMultiply(new Point((int)pDrawCoords.X, (int)pDrawCoords.Y) + camera.OffsetPoint, camera.ZoomFactor), VNFUtils.PointMultiply(new Point((int)(LocalAtlas.FrameSize().X * _scale.X), (int)(LocalAtlas.FrameSize().Y * _scale.Y)), camera.ZoomFactor)), new Rectangle(new Point((LocalAtlas.SourceRect.Width / LocalAtlas.DivDimensions.X) * pAtlasCoordinates.X, (LocalAtlas.SourceRect.Height / LocalAtlas.DivDimensions.Y) * pAtlasCoordinates.Y), LocalAtlas.FrameSize()), ColourValue, pRotation + FlipRotationAddit, AdjustedOrigin, LocalSpriteEffect, LayerDepth); }

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
                return -(Position - ((new Vector2(Shell.Resolution.X, Shell.Resolution.Y) / ZoomFactor)/2));
            }
        }
        public Point OffsetPoint
        {
            get
            {
                return VNFUtils.ConvertVector(OffsetVector);
            }
        }
        private double _zoomLevel = 0d;
        public Vector2 ZoomFactor
        {
            get
            {
                return (new Vector2((float)Math.Pow(2d, _zoomLevel), (float)Math.Pow(2d, _zoomLevel))) * Size;
            }
        }
        public void SnapTo(WorldEntity worldEntity)
        {
            QuickMoveTo(worldEntity.Position);
        }
        String _autoSnapToOnResetEntityName = "";
        public String AutoSnapToOnResetEntityName
        {
            get
            {
                return _autoSnapToOnResetEntityName;
            }
            set
            {
                _autoSnapToOnResetEntityName = value;
            }
        }
        Vector2 _recenterPosition = (Shell.Resolution / 2);
        public Vector2 RecenterPosition
        {
            get
            {
                return _recenterPosition;
            }
            set
            {
                _recenterPosition = value;
            }
        }
        public void RecenterCamera()
        {
            if (_autoSnapToOnResetEntityName.Length > 0)
            {
                WorldEntity snapEntity = Shell.GetEntityByName(AutoSnapToOnResetEntityName);
                if(snapEntity != null)
                {
                    SnapTo(snapEntity);
                    return;
                }
            }
            QuickMoveTo(_recenterPosition);
        }
        public void Zoom(float zoomDelta)
        {
            _zoomLevel += zoomDelta;
        }
        public void ResetZoom()
        {
            _zoomLevel = 0d;
            Size = new Vector2(1, 1);
        }
        public Vector2 TranslateCoordsToEquivalent(Vector2 globalCoords)
        {
            return (globalCoords / ZoomFactor) - OffsetVector;
        }
        public Camera(String Name) : base(Name, (Shell.Resolution / 2), null, 1)
        {
            MouseDragEnabled = false;
        }
        public Boolean MouseDragEnabled { get; set; }
        private Boolean _mouseDragging = false;
        private Boolean _zoomOpen = false;
        private Vector2 _lastMouseDragPos = new Vector2();
        private int _lastMouseScroll = 0;
        public override void Update()
        {
            base.Update();
            if (MouseDragEnabled)
            {
                MouseState myMouse = Mouse.GetState();
                if (myMouse.LeftButton == ButtonState.Pressed)
                {
                    if (!_mouseDragging)
                    {
                        _lastMouseDragPos = Shell.CoordNormalize(VNFUtils.ConvertPoint(myMouse.Position));
                        _mouseDragging = true;
                    }
                    Vector2 currentMouseDragPos = Shell.CoordNormalize(VNFUtils.ConvertPoint(myMouse.Position));
                    Vector2 dragDistance = currentMouseDragPos - _lastMouseDragPos;
                    Move(-dragDistance / ZoomFactor);
                    _lastMouseDragPos = currentMouseDragPos;
                }
                else if (myMouse.LeftButton == ButtonState.Released)
                {
                    _mouseDragging = false;
                }
                if (!_zoomOpen)
                {
                    _lastMouseScroll = myMouse.ScrollWheelValue;
                    _zoomOpen = true;
                }
                int currentMouseScroll = myMouse.ScrollWheelValue;
                _zoomLevel += (currentMouseScroll - _lastMouseScroll) / 1000d;
                _lastMouseScroll = currentMouseScroll;
            }
            else
            {
                if (_mouseDragging) { _mouseDragging = false; }
                if (_zoomOpen) { _zoomOpen = false; }
            }
        }
    }
    [Serializable]
    public class Pane : WorldEntity
    {
        public Camera DefaultPaneCamera { get; set; }
        List<WorldEntity> UpdateQueue { get; set; }
        List<WorldEntity> RenderQueue { get; set; }
        List<WorldEntity> DeleteQueue { get; set; }
        public Color BackgroundColor { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        RenderTarget2D _renderPane;
        public void AddUpdate(WorldEntity worldEntity)
        {
            worldEntity.CustomCamera = DefaultPaneCamera;
            worldEntity.UsePseudoMouse = true;
            UpdateQueue.Add(worldEntity);
        }
        public void AddRender(WorldEntity worldEntity)
        {
            worldEntity.CustomCamera = DefaultPaneCamera;
            worldEntity.UsePseudoMouse = true;
            RenderQueue.Add(worldEntity);
        }
        public void AddDelete(WorldEntity worldEntity)
        {
            DeleteQueue.Add(worldEntity);
        }
        public RenderTarget2D RenderPane
        {
            get { return _renderPane; }
        }
        Point _paneBaseSize;
        public Pane(String name, Vector2 location, float depth, Point paneSize, Color backgroundCol, GraphicsDevice myGraphicsDevice) : base(name, location, null, depth)
        {
            BackgroundColor = backgroundCol;
            GraphicsDevice = myGraphicsDevice;
            _paneBaseSize = paneSize;
            DefaultPaneCamera = new Camera("CAMERA_PANE_" + name);
            UpdateQueue = new List<WorldEntity>();
            RenderQueue = new List<WorldEntity>();
            DeleteQueue = new List<WorldEntity>();
            RenderAlways = true;
            AllowInternalInteracts = true;
            Render();
        }
        public Boolean AllowInternalInteracts { get; set; }
        protected override void EntityClickFunctionTrigger()
        {
            if(AllowInternalInteracts && MouseInBounds())
            {
                var mouseState = Mouse.GetState();
                Vector2 localPosition = LocalCoords(Shell.CoordNormalize(new Vector2(mouseState.X, mouseState.Y)));
                foreach (WorldEntity updateEntity in UpdateQueue)
                {
                    updateEntity.PseudoMouse = localPosition;
                    if (updateEntity.MouseInBounds()) { updateEntity.ClickTrigger(); }
                }
            }
            base.EntityClickFunctionTrigger();
        }
        ~Pane()
        {
            Clear();
        }
        public Vector2 LocalCoords(Vector2 globalCoords)
        {
            Vector2 vectorOut = globalCoords;
            Vector2 zoomFactor = new Vector2(1, 1);
            if (!CameraImmune)
            {
                if (CustomCamera != null)
                {
                    vectorOut = CustomCamera.TranslateCoordsToEquivalent(vectorOut);
                }
                else if (Shell.AutoCamera != null)
                {
                    vectorOut = Shell.AutoCamera.TranslateCoordsToEquivalent(vectorOut);
                }
            }
            Vector2 size = new Vector2((Atlas.FrameSize().X * Size.X), (Atlas.FrameSize().Y * Size.Y));
            Vector2 InternalOriginCoords = CenterOrigin ? Position - (size/2) : Position;
            vectorOut = (vectorOut - InternalOriginCoords) / Size;
            return vectorOut;
        }
        public void Clear()
        {
            foreach (WorldEntity worldEntity in UpdateQueue)
            {
                DeleteQueue.Add(worldEntity);
            }
            foreach (WorldEntity worldEntity in RenderQueue)
            {
                DeleteQueue.Add(worldEntity);
            }
            foreach (WorldEntity worldEntity in DeleteQueue)
            {
                if (UpdateQueue.Contains(worldEntity)) { UpdateQueue.Remove(worldEntity); }
                if (RenderQueue.Contains(worldEntity)) { RenderQueue.Remove(worldEntity); }
                worldEntity.ManualDispose();
            }
            DeleteQueue = new List<WorldEntity>();
        }
        Vector2 _panePseudoMouse = new Vector2(float.NaN, float.NaN);
        public Boolean RenderAlways { get; set; }
        public override void Update()
        {
            if (AllowInternalInteracts)
            {
                var mouseState = Mouse.GetState();
                _panePseudoMouse = LocalCoords(Shell.CoordNormalize(new Vector2(mouseState.X, mouseState.Y)));
            }
            else { _panePseudoMouse = new Vector2(float.NaN, float.NaN); }
            foreach(WorldEntity worldEntity in UpdateQueue)
            {
                worldEntity.PseudoMouse = _panePseudoMouse;
                worldEntity.Update();
            }
            foreach (WorldEntity worldEntity in DeleteQueue)
            {
                if (UpdateQueue.Contains(worldEntity)) { UpdateQueue.Remove(worldEntity); }
                if (RenderQueue.Contains(worldEntity)) { RenderQueue.Remove(worldEntity); }
                if (Shell.NonSerializables.Contains(worldEntity)) { Shell.NonSerializables.Remove(worldEntity); }
            }
            DeleteQueue = new List<WorldEntity>();
            base.Update();
        }
        public void Render()
        {
            if (_renderPane == null || _renderPane.Bounds.Size != _paneBaseSize)
            {
                if (_renderPane != null) { _renderPane.Dispose(); }
                _renderPane = new RenderTarget2D(GraphicsDevice, _paneBaseSize.X, _paneBaseSize.Y, false,
                    GraphicsDevice.PresentationParameters.BackBufferFormat,
                    DepthFormat.Depth24);
            }
            GraphicsDevice.SetRenderTarget(_renderPane);
            //Rectangle PreScissor = GraphicsDevice.ScissorRectangle;
            //GraphicsDevice.ScissorRectangle = new Rectangle(new Point(), PaneBaseSize/new Point(2, 2));
            GraphicsDevice.Clear(BackgroundColor);
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            foreach (WorldEntity worldEntity in RenderQueue)
            {
                if (worldEntity.Drawable)
                {
                    if (worldEntity.CustomCamera != null) { worldEntity.Draw(spriteBatch, worldEntity.CustomCamera); }
                    else { worldEntity.Draw(spriteBatch, DefaultPaneCamera); }
                }
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.ScissorRectangle = PreScissor;
            TAtlasInfo localAtlas = new TAtlasInfo();
            localAtlas.Atlas = RenderPane;
            localAtlas.DivDimensions = new Point(1, 1);
            Atlas = localAtlas;
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
            public Boolean IgnoreLinebreak;
            public int TimeDelay;
            public void Rainbow()
            {
                Colour = GetRainbowColour();
            }
        }
        public static int GetTicksFromSliderValue(float sliderValue)
        {
            return (int)Math.Round(1000 / (Math.Pow(10, (3 * sliderValue))));
        }
        public static float GetSliderValueFromTicks(int ticks)
        {
            return (float)(Math.Log10(1000 / ticks) / 3);
        }
        public static Color GetRainbowColour()
        {
            double[] scrollColour = new double[3];
            scrollColour[0] = ((Environment.TickCount / 10) % 300) * (double)(Math.PI / 150);
            scrollColour[1] = (((Environment.TickCount / 10) + 100) % 300) * (double)(Math.PI / 150);
            scrollColour[2] = (((Environment.TickCount / 10) + 200) % 300) * (double)(Math.PI / 150);
            if (scrollColour[0] >= Math.PI * 2) { scrollColour[0] = 0; }
            if (scrollColour[1] >= Math.PI * 2) { scrollColour[1] = 0; }
            if (scrollColour[2] >= Math.PI * 2) { scrollColour[2] = 0; }
            Color outColour = new Color((byte)(122.5 * (Math.Sin(scrollColour[0]) + 1)), (byte)(122.5 * (Math.Sin(scrollColour[1]) + 1)), (byte)(122.5 * (Math.Sin(scrollColour[2]) + 1)), (byte)255);
            return outColour;
        }
        public static String GetRainbowColourCode()
        {
            Color trueColour = GetRainbowColour();
            return "[C:" + trueColour.R + "-" + trueColour.G + "-" + trueColour.B + "-255]";
        }
        private int _length = 0;
        public int Length
        {
            get { return _length; }
            protected set { _length = value; }
        }
        public override Rectangle Hitbox
        {
            get
            {
                if (!DrawAtlasComponent)
                {
                    Rectangle hitbox = new Rectangle(VNFUtils.ConvertVector(Position), new Point(_bufferLength, VerticalLength(true)));
                    base.Hitbox = hitbox;
                    return hitbox;
                }
                else
                {
                    return base.Hitbox;
                }
            }
        }
        private int _newlineIndent = 0;
        public int NewlineIndent
        {
            get
            {
                return _newlineIndent;
            }
            set
            {
                if (value != _newlineIndent)
                {
                    _newlineIndent = value;
                    ignoreDelayOnThis = new List<int>();
                    _textChunkR = PreprocessText(_text, _bufferLength, _forceSplitUnchunkables, _newlineIndent);
                }
            }
        }
        private RenderTarget2D[,] _staticTextures = null;
        private Boolean _drawAsStatic = false;
        public Boolean DrawAsStatic
        {
            get
            {
                return _drawAsStatic;
            }
            set
            {
                if(_drawAsStatic != value)
                {
                    if(!_drawAsStatic)
                    {
                        _staticTextures = new RenderTarget2D[1, 1];
                        _staticTextures[0,0] = new RenderTarget2D(Shell.PubGD, 1000, 1000, false,
                            Shell.PubGD.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
                        StaticRender();
                    }
                    else
                    {
                        _staticTextures = null;
                    }
                    _drawAsStatic = value;
                }
            }
        }
        public void StaticRender()
        {
            Point dims = new Point((int)Math.Ceiling(Hitbox.Width / 1000f), (int)Math.Ceiling(Hitbox.Height / 1000f));
            RenderTarget2D[,] outArray = new RenderTarget2D[dims.X, dims.Y];
            for(int y = 0; y < dims.Y; y++)
            {
                for(int x = 0; x < dims.X; x++)
                {
                    if(x < _staticTextures.GetLength(0) && y < _staticTextures.GetLength(1) && _staticTextures[x, y] != null)
                    {
                        outArray[x, y] = _staticTextures[x, y];
                    }
                    else
                    {
                        outArray[x, y] = new RenderTarget2D(Shell.PubGD, 1000, 1000, false,
                            Shell.PubGD.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
                    }
                    GraphicsDevice textGD = Shell.PubGD;
                    textGD.SetRenderTarget(outArray[x, y]);
                    textGD.Clear(Color.Transparent);
                    SpriteBatch spriteBatch = new SpriteBatch(textGD);
                    spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                    DrawDynamic(spriteBatch, new Vector2(x * -1000, y * -1000));
                    spriteBatch.End();
                    Shell.PubGD.SetRenderTarget(null);
                }
            }
            _staticTextures = outArray;
        }
        public static TextChunk[] PreprocessText(String text, Boolean forceSplitUnchunkables, int newlineIndent)
        {
            return PreprocessText(text, -1, forceSplitUnchunkables, newlineIndent);
        }
        public static TextChunk[] PreprocessText(String text, int pixelBuffer, Boolean forceSplitUnchunkables, int newlineIndent)
        {
            text = text.Replace("\n", "[N]");
            text = text.Replace("][", ",");
            if (text.Length > 0 && text[text.Length-1] == ']') { text += " "; }
            List<TextChunk> chunkStore = new List<TextChunk>();
            Vector2 location = new Vector2();
            Boolean first = true;
            while (text.Contains('[') || first)
            {
                first = false;
                SpriteFont font = Shell.Default;
                String fontName = "DEFAULT";
                Color colour = Color.White;
                Boolean linebreak = false;
                Boolean ignoreLinebreak = false;
                Boolean rainbowMode = false;
                int timeDelay = 0;
                if (text.IndexOf('[') == 0)
                {
                    String formatting = text.Remove(text.IndexOf(']') + 1);
                    formatting = formatting.Replace("[", "").Replace("]", "").ToUpper();
                    String[] specs = formatting.Split(',');
                    foreach (String specString in specs)
                    {
                        String[] strSplit = specString.Split(':');
                        /*
                         * [C:] Set colour
                         * [F:] Set font to spritefont
                         * [L:-] Manually adjust textchunk location
                         * [R] Render in rainbow text
                         * [N] Insert newline
                         * [I] Ignore auto-insertion of linebreaks for this textchunk
                         * [T:] Delay typewrite by given millisecond amount
                         */
                        switch(strSplit[0])
                        {
                            case "C":
                                switch(strSplit[1])
                                {
                                    case "WHITE":
                                        colour = Color.White;
                                        break;
                                    case "PURPLE":
                                        colour = new Color(138, 0, 255, 255);
                                        break;
                                    default:
                                        byte[] rgba = strSplit[1].Split('-').Select(x => Convert.ToByte(x)).ToArray();
                                        colour.R = rgba[0];
                                        colour.G = rgba[1];
                                        colour.B = rgba[2];
                                        colour.A = rgba[3];
                                        break;
                                }
                                break;
                            case "F":
                                if (strSplit[1] == "SYSFONT")
                                {
                                    font = (SpriteFont)Shell.SysFont;
                                }
                                else
                                {
                                    font = (SpriteFont)Shell.Fonts[strSplit[1]];
                                }
                                fontName = strSplit[1];
                                break;
                            case "L":
                                Vector2 adjustment = new Vector2(0,0);
                                adjustment.X = strSplit[1].Split('-').Select(x => Convert.ToInt32(x)).ToArray()[0];
                                adjustment.Y = strSplit[1].Split('-').Select(x => Convert.ToInt32(x)).ToArray()[1];
                                location += adjustment;
                                break;
                            case "R":
                                rainbowMode = true;
                                break;
                            case "N":
                                linebreak = true;
                                break;
                            case "I":
                                ignoreLinebreak = true;
                                break;
                            case "T":
                                timeDelay = Convert.ToInt32(strSplit[1]);
                                break;
                            default:
                                break;
                        }
                    }
                    text = text.Remove(0, formatting.Length + 2);
                }
                TextChunk temp = new TextChunk();
                temp.Font = font;
                temp.FontName = fontName;
                temp.Colour = colour;
                temp.DrawLocation = location;
                temp.RainbowMode = rainbowMode;
                temp.Linebreak = linebreak;
                temp.IgnoreLinebreak = ignoreLinebreak;
                temp.TimeDelay = timeDelay;
                if (text.Contains('['))
                {
                    temp.Text = text.Remove(text.IndexOf('['));
                }
                else { temp.Text = text; }
                chunkStore.Add(temp);
                location.X += (int)font.MeasureString(temp.Text).X;
                text = text.Remove(0, temp.Text.Length);
            }
            TextChunk[] outChunkR = chunkStore.ToArray().Select(x => (TextChunk)x).ToArray();
            if(pixelBuffer != -1) { outChunkR = LinebreakChunks(outChunkR, pixelBuffer, forceSplitUnchunkables, newlineIndent); }
            return outChunkR;
        }
        private Boolean _forceSplitUnchunkables = false;
        public Boolean ForceSplitUnchunkables
        {
            get
            {
                return _forceSplitUnchunkables;
            }
            set
            {
                if (value != _forceSplitUnchunkables)
                {
                    _forceSplitUnchunkables = value;
                    ignoreDelayOnThis = new List<int>();
                    _textChunkR = PreprocessText(_text, _bufferLength, _forceSplitUnchunkables, NewlineIndent);
                }
            }
        }
        public static TextChunk[] LinebreakChunks(TextChunk[] initial, int maxPixelLineLength, Boolean forceSplitUnchunkables, int newlineIndent) //Function to insert linebreaks into text as required, based on a given length
        {
            int currentPixelTotal = 0;
            Vector2 rollingLocationMod = new Vector2(0, 0); //Vector representing the degree to which the next line should be shifted from the previous
            Vector2 stackableVector2 = new Vector2(0, 0); //Vector representing the degree to which all subsequent text chunks should be additionally shifted by auto-inserted line breaks
            Vector2 linebreakAmmendment = new Vector2(); //Vector representing the degree to which all subsequent text chunks should be additionally shifted due to manual line breaks
            ArrayList rebuildChunks = new ArrayList(); //The new set of chunks broken up correctly to include line breaks
            ArrayList newChunkRegistry = new ArrayList(); //Collection of new text chunks created by auto-inserted line breaks, that will be depopulated as they are processed
            int i = 0;
            while(i < initial.Length || newChunkRegistry.Count > 0)
            {
                TextChunk currentTC; //currentTC is the current text chunk being processed
                if (newChunkRegistry.Count > 0) //If new chunks have been created by automated linebreaking, they are dealt with before the next input text chunk
                {
                    i--;
                    currentTC = (TextChunk)newChunkRegistry[0];
                    newChunkRegistry.RemoveAt(0);
                    stackableVector2 += rollingLocationMod;
                }
                else //If there are none, then the next chunk in the list is selected
                {
                    currentTC = initial[i];
                    if (i > 0)
                    {
                        currentTC.DrawLocation += (linebreakAmmendment + stackableVector2 - rollingLocationMod); //Its initial location is modified from the input location (determined in PreprocessText()) using the three vectors
                        //rollingLocationMod is subtracted because stackableVector2 will contain its value, if this is a new chunk after an autolinebreak, and it is always added back on regardless, so this nullifies that
                    }
                }
                if(currentTC.Linebreak)
                {
                    //Manual linebreaks cause RLM to be updated, but the change is also stored in linebreakAmmendment for future chunks
                    rollingLocationMod = new Vector2(rollingLocationMod.X - currentPixelTotal, rollingLocationMod.Y + (int)currentTC.Font.MeasureString(" ").Y);
                    linebreakAmmendment += new Vector2(-currentPixelTotal, (int)currentTC.Font.MeasureString(" ").Y);
                    currentPixelTotal = 0;
                    currentTC.Linebreak = false;
                    initial[i].Linebreak = false;
                }
                currentTC.DrawLocation += rollingLocationMod; //The currentTC draw location is modified by the RLM vector
                int pixFromNextTotal = 0;
                if(i + 1 < initial.Length) //This section of code determines how many pixels the text chunk after this would take up before a line break is possible, for later use
                {
                    int forward = 1;
                    while (i + forward < initial.Length)
                    {
                        TextChunk nextTextChunk = initial[i + forward];
                        if(nextTextChunk.Linebreak) { break; }
                        if (nextTextChunk.Text.Contains(' '))
                        {
                            pixFromNextTotal += (int)currentTC.Font.MeasureString(nextTextChunk.Text.Remove(nextTextChunk.Text.IndexOf(' '))).X;
                            break;
                        }
                        else { pixFromNextTotal += (int)currentTC.Font.MeasureString(nextTextChunk.Text).X; }
                        forward++;
                    }
                }
                if(currentPixelTotal + (int)currentTC.Font.MeasureString(currentTC.Text).X + pixFromNextTotal > maxPixelLineLength && !currentTC.IgnoreLinebreak) //If the current length plus the next chunk's pre-space pixels break the line limit...
                {
                    Boolean overByPix = false;
                    //overByPix: whether the next chunk will take the line length over the edge
                    if (currentPixelTotal + (int)currentTC.Font.MeasureString(currentTC.Text).X + pixFromNextTotal > maxPixelLineLength && currentTC.Font.MeasureString(currentTC.Text).X + currentPixelTotal <= maxPixelLineLength) { overByPix = true; }
                    if (currentTC.Text.Contains(' ')) //First we check for a space to see if a linebreak can be performed in the current text chunk
                    {
                        String findLoc = currentTC.Text;
                        int foundLocation = -1;
                        //overByPix is used to cause a linebreak on the last possible space to avoid overflow next text chunk
                        //Otherwise, each space is checked until one is found that allows the line to be shortened to within the limit
                        while ((currentTC.Font.MeasureString(findLoc).X + currentPixelTotal > maxPixelLineLength || overByPix) && findLoc.Contains(' '))
                        {
                            overByPix = false;
                            int currentIndex = findLoc.LastIndexOf(' ');
                            findLoc = findLoc.Remove(currentIndex);
                            if (currentTC.Font.MeasureString(findLoc).X + currentPixelTotal <= maxPixelLineLength)
                            {
                                foundLocation = currentIndex;
                            }
                        }
                        //If no workable space is found, the first one is picked as the best compromise
                        if (foundLocation == -1)
                        {
                            foundLocation = currentTC.Text.IndexOf(' ');
                        }
                        //A new text chunk is created to represent the text after the line break
                        TextChunk newTC = currentTC;
                        newTC.TimeDelay = 0;
                        newTC.Text = newTC.Text.Remove(0, foundLocation + 1);
                        if(newTC.Text.Length > 0 && newTC.Text[0] == ' ') { newTC.Text = newTC.Text.Remove(0, 1); }
                        int TCMeasure = 0;
                        currentTC.Text = currentTC.Text.Remove(foundLocation);
                        TCMeasure = (int)currentTC.Font.MeasureString(currentTC.Text + " ").X; //The length of the first half of the new, broken line is measured
                        rollingLocationMod.X = -currentPixelTotal - TCMeasure + newlineIndent; //rollingLocationMod is used to modify the position of the new text chunk, and also to later transfer this information to the SV2 vector
                        newTC.DrawLocation.X += TCMeasure; //The length is added back on, as the newTC text chunk is starting from the initial location of the old one (currentTC) before being shifted back by RLM, so it must be adjusted for the fact that it comes after currentTC
                        rollingLocationMod.Y = (int)currentTC.Font.MeasureString(" ").Y; //And the "new" chunk is also shifted down
                        currentPixelTotal = newlineIndent;
                        rebuildChunks.Add(currentTC); //The current text chunk is added to the new, final list of chunks                       
                        newChunkRegistry.Add(newTC);
                    }
                    else //Else, if text cannot be split...
                    {
                        if (!overByPix && forceSplitUnchunkables)
                        {
                            StringBuilder findLoc = new StringBuilder(currentTC.Text);
                            //Similar to above, but spaces are not looked for
                            while (currentTC.Font.MeasureString(findLoc).X + currentPixelTotal > maxPixelLineLength)
                            {
                                findLoc.Remove(findLoc.Length - 1, 1);
                            }
                            int FoundLocation = findLoc.Length;
                            //A new text chunk is created to represent the text after the line break
                            TextChunk newTC = currentTC;
                            newTC.TimeDelay = 0;
                            newTC.Text = newTC.Text.Remove(0, FoundLocation);
                            int tcMeasure = 0;
                            currentTC.Text = currentTC.Text.Remove(FoundLocation);
                            tcMeasure = (int)currentTC.Font.MeasureString(currentTC.Text).X; //The length of the first half of the new, broken line is measured
                            rollingLocationMod.X = -currentPixelTotal - tcMeasure + newlineIndent; //rollingLocationMod is used to modify the position of the new text chunk, and also to later transfer this information to the SV2 vector
                            newTC.DrawLocation.X += tcMeasure; //The length is added back on, as the newTC text chunk is starting from the initial location of the old one (currentTC) before being shifted back by RLM, so it must be adjusted for the fact that it comes after currentTC
                            rollingLocationMod.Y = (int)currentTC.Font.MeasureString(" ").Y; //And the "new" chunk is also shifted down
                            currentPixelTotal = newlineIndent;
                            rebuildChunks.Add(currentTC); //The current text chunk is added to the new, final list of chunks                       
                            newChunkRegistry.Add(newTC);
                        }
                        else
                        {
                            rebuildChunks.Add(currentTC); //Ignore split and proceed as normal as it is not possible to split, hoping that the next text chunk will be splitable.
                            currentPixelTotal += (int)currentTC.Font.MeasureString(currentTC.Text).X;
                        }
                    }
                }
                else //If no linebreak is required...
                {
                    rebuildChunks.Add(currentTC); //Simply add currentTC into the new chunk list as is
                    currentPixelTotal += (int)currentTC.Font.MeasureString(currentTC.Text).X; //And increment the CurrentPixelTotal to keep a record of how far we are towards the length limit
                }
                i++;
            }
            return rebuildChunks.ToArray().Select(x => (TextChunk)x).ToArray(); //The new list of text chunks is returned as an array
        }
        List<int> ignoreDelayOnThis = new List<int>();
        public TextChunk[] RevealXChars(TextChunk[] Initial, int CharCount)
        {
            TextChunk[] copyTC = (TextChunk[])Initial.Clone();
            int charR = 0;
            for (int i = 0; i < copyTC.Length; i++)
            {
                TextChunk textChunk = copyTC[i];
                if(charR >= CharCount) { textChunk.Text = ""; }
                for (int ii = 0; ii < textChunk.Text.Length; ii++)
                {
                    if (textChunk.TimeDelay != 0 && !ignoreDelayOnThis.Contains(i) && writeProgress < _length)
                    {
                        Updatetime += textChunk.TimeDelay - 30;
                        ignoreDelayOnThis.Add(i);
                        ii--;
                    }
                    charR++;
                    if(charR >= CharCount && ii+1 < textChunk.Text.Length)
                    {
                        textChunk.Text = textChunk.Text.Remove(ii+1);
                        break;
                    }
                }
                copyTC[i] = textChunk;
            }
            return copyTC;
        }
        String _text = "";
        TextChunk[] _textChunkR;
        public override void OnDeserializeDo()
        {
            base.OnDeserializeDo();
            for(int i = 0; i < _textChunkR.Length; i++)
            {
                TextChunk textChunk = _textChunkR[i];
                String fontName = (textChunk.FontName != null ? textChunk.FontName : "DEFAULT");
                textChunk.Font = (SpriteFont)Shell.Fonts[fontName];
                _textChunkR[i] = textChunk;
            }
            for (int i = 0; i < _progressiveChunks.Length; i++)
            {
                TextChunk textChunk = _progressiveChunks[i];
                String fontName = (textChunk.FontName != null ? textChunk.FontName : "DEFAULT");
                textChunk.Font = (SpriteFont)Shell.Fonts[fontName];
                _progressiveChunks[i] = textChunk;
            }
        }
        private int _bufferLength;
        public int BufferLength
        {
            get
            {
                return _bufferLength;
            }
            set
            {
                if (value != _bufferLength)
                {
                    _bufferLength = value;
                    ignoreDelayOnThis = new List<int>();
                    _textChunkR = PreprocessText(_text, _bufferLength, _forceSplitUnchunkables, NewlineIndent);
                }
            }
        }
        public String Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    _textChunkR = new TextChunk[0];
                    ignoreDelayOnThis = new List<int>();
                    _textChunkR = PreprocessText(value, _bufferLength, _forceSplitUnchunkables, NewlineIndent);
                    int len = 0;
                    foreach (TextChunk textChunk in _textChunkR) { len += textChunk.Text.Length; }
                    _length = len;
                    if (_drawAsStatic)
                    {
                        StaticRender();
                    }
                }
            }
        }
        public TextEntity(String name, String textIn, Vector2 location, float depth) : base(name, location, null, depth)
        {
            DrawAtlasComponent = false;
            _bufferLength = 1000;
            _textChunkR = PreprocessText(textIn, _bufferLength, false, NewlineIndent);
            if(TypeWrite) { _progressiveChunks = RevealXChars(_textChunkR, 0); }
            int len = 0;
            foreach (TextChunk textChunk in _textChunkR) { len += textChunk.Text.Length; }
            _length = len;
            _text = textIn;
            TypeWrite = false;
        }
        public void ReWrite()
        {
            _progressiveChunks = new TextChunk[0];
            writeProgress = -1;
        }
        public void SkipWrite()
        {
            writeProgress = _length;
            _progressiveChunks = RevealXChars(_textChunkR, writeProgress);
            if (_drawAsStatic) { StaticRender(); }
        }
        public int VerticalLength()
        {
            return VerticalLength(false);
        }
        public int VerticalLength(Boolean checkDisplacements)
        {
            int output = 0;
            if(checkDisplacements)
            {
                float dispChecker = 0;
                output = (int)(_textChunkR[0].Font.MeasureString(" ").Y);
                foreach (TextChunk T in _textChunkR)
                {
                    if(T.DrawLocation.Y > dispChecker)
                    {
                        dispChecker = T.DrawLocation.Y;
                        output = (int)(dispChecker + T.Font.MeasureString(" ").Y);
                    }
                }
                foreach (TextChunk T in _textChunkR)
                {
                    if (T.DrawLocation.Y < dispChecker)
                    {
                        dispChecker = T.DrawLocation.Y;
                    }
                }
                output -= (int)dispChecker;
            }
            else
            {
                output = ((int)_textChunkR[_textChunkR.Length - 1].DrawLocation.Y + (int)_textChunkR[_textChunkR.Length - 1].Font.MeasureString(" ").Y);
            }
            return output;
        }
        public int ChunkCount
        {
            get { return _textChunkR.Length; }
        }
        private float[] _chunkFontHeight = null;
        public float[] ChunkFontHeight
        {
            get
            {
                _chunkFontHeight = new float[_textChunkR.Length];
                int i = 0;
                foreach(TextChunk T in _textChunkR)
                {
                    _chunkFontHeight[i] = T.Font.MeasureString(" ").Y;
                    i++;
                }
                return _chunkFontHeight;
            }
            protected set { _chunkFontHeight = value; }
        }
        int writeProgress = -1;
        public Boolean TypeWrite { get; set; }
        private TextChunk[] _progressiveChunks = new TextChunk[] { };
        public Boolean WrittenAll()
        {
            return !(writeProgress < _length) || !TypeWrite;
        }
        public static void PlayTick()
        {
            if (!Shell.Mute)
            {
                int tickNum = Shell.Rnd.Next(1, 4);
                SoundEffectInstance Tick = ((SoundEffect)Shell.SFXDirectory["TYPE_" + tickNum]).CreateInstance();
                Tick.Volume = Shell.GlobalVolume;
                Tick.Play();
                Shell.ActiveSounds.Add(Tick);
            }
        }
        Boolean _sounder = false;
        public override void Update()
        {
            base.Update();
            if (TypeWrite && writeProgress < _length && Updatetime < Environment.TickCount - TickWriteInterval && !Shell.HoldRender)
            {
                Updatetime = Environment.TickCount;
                if (!ButtonScripts.Paused && !ButtonScripts.Navigating)
                {
                    writeProgress++;
                    _progressiveChunks = RevealXChars(_textChunkR, writeProgress);
                    if (_drawAsStatic) { StaticRender(); }
                    if (_sounder)
                    {
                        if (Name == "TEXT_MAIN" && _length > 0)
                        {
                            PlayTick();
                        }
                        _sounder = false;
                    }
                    else { _sounder = true; }
                }
            }
        }
        public Boolean DrawAtlasComponent
        {
            get; set;
        }
        public void AssignTextureAtlas(TAtlasInfo? atlas)
        {
            if(atlas != null)
            {
                Atlas = (TAtlasInfo)atlas;
                _drawAtlasTextDifferential = 0.00001f;
                DrawAtlasComponent = true;
            }
            else
            {
                Atlas = new TAtlasInfo();
                _drawAtlasTextDifferential = 0f;
                DrawAtlasComponent = false;
            }
        }
        private float _drawAtlasTextDifferential = 0f;
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
            if(DrawAtlasComponent) { base.Draw(spriteBatch); }
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
            if (DrawAtlasComponent) { base.Draw(spriteBatch, camera); }
        }
        public void DrawDynamic(SpriteBatch spriteBatch, Vector2? manualNormalizer)
        {
            if (!TypeWrite)
            {
                foreach (TextChunk textChunk in _textChunkR)
                {
                    if(textChunk.RainbowMode) { textChunk.Rainbow(); }
                    spriteBatch.DrawString(textChunk.Font, textChunk.Text, new Vector2(textChunk.DrawLocation.X, textChunk.DrawLocation.Y) + (manualNormalizer ?? Position - Origin), textChunk.Colour * (ColourValue.A / 255f), 0f, new Vector2(0, 0), 1f, SpriteEffects.None, LayerDepth + _drawAtlasTextDifferential);
                }
            }
            else
            {
                foreach (TextChunk textChunk in _progressiveChunks)
                {
                    if (textChunk.RainbowMode) { textChunk.Rainbow(); }
                    spriteBatch.DrawString(textChunk.Font, textChunk.Text, new Vector2(textChunk.DrawLocation.X, textChunk.DrawLocation.Y) + (manualNormalizer ?? Position - Origin), textChunk.Colour * (ColourValue.A / 255f), 0f, new Vector2(0, 0), 1f, SpriteEffects.None, LayerDepth + _drawAtlasTextDifferential);
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
                    foreach (TextChunk textChunk in _textChunkR)
                    {
                        if (textChunk.RainbowMode) { textChunk.Rainbow(); }
                        spriteBatch.DrawString(textChunk.Font, textChunk.Text, (new Vector2(textChunk.DrawLocation.X, textChunk.DrawLocation.Y) + Position - Origin + camera.OffsetVector) * camera.ZoomFactor, textChunk.Colour * (ColourValue.A / 255f), 0f, new Vector2(0, 0), camera.ZoomFactor.X, SpriteEffects.None, LayerDepth + _drawAtlasTextDifferential);
                    }
                }
                else
                {
                    foreach (TextChunk textChunk in _progressiveChunks)
                    {
                        if (textChunk.RainbowMode) { textChunk.Rainbow(); }
                        spriteBatch.DrawString(textChunk.Font, textChunk.Text, (new Vector2(textChunk.DrawLocation.X, textChunk.DrawLocation.Y) + Position - Origin + camera.OffsetVector) * camera.ZoomFactor, textChunk.Colour * (ColourValue.A / 255f), 0f, new Vector2(0, 0), camera.ZoomFactor.X, SpriteEffects.None, LayerDepth + _drawAtlasTextDifferential);
                    }
                }
            }
        }
        public void DrawStatic(SpriteBatch spriteBatch)
        {
            if (_staticTextures != null)
            {
                for(int x = 0; x < _staticTextures.GetLength(0); x++)
                {
                    for (int y = 0; y < _staticTextures.GetLength(1); y++)
                    {
                        spriteBatch.Draw(_staticTextures[x, y], new Rectangle(new Point((int)Position.X, (int)Position.Y) - VNFUtils.ConvertVector(Origin) + VNFUtils.PointMultiply(new Point(x * 1000, y * 1000), Size), VNFUtils.PointMultiply(_staticTextures[x, y].Bounds.Size, Size)), _staticTextures[x, y].Bounds, Color.White, 0f, new Vector2(), SpriteEffects.None, LayerDepth + _drawAtlasTextDifferential);
                    }
                }
            }
        }
        public void DrawStatic(SpriteBatch spriteBatch, Camera camera)
        {
            if (_staticTextures != null)
            {
                for (int x = 0; x < _staticTextures.GetLength(0); x++)
                {
                    for (int y = 0; y < _staticTextures.GetLength(1); y++)
                    {
                        spriteBatch.Draw(_staticTextures[x, y], new Rectangle(VNFUtils.PointMultiply((new Point((int)Position.X, (int)Position.Y) - VNFUtils.ConvertVector(Origin) + VNFUtils.PointMultiply(new Point(x * 1000, y * 1000), Size) + VNFUtils.ConvertVector(camera.OffsetVector)), camera.ZoomFactor), VNFUtils.PointMultiply(VNFUtils.PointMultiply(_staticTextures[x, y].Bounds.Size, VNFUtils.ConvertVector(Size)), camera.ZoomFactor)), _staticTextures[x, y].Bounds, Color.White, 0f, new Vector2(), SpriteEffects.None, LayerDepth + _drawAtlasTextDifferential);
                    }
                }
            }
        }
    }
}
