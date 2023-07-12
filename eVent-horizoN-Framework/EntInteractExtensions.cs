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
    /// a basic button object. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    public class Button : WorldEntity
    {
        public Boolean Enabled { get; set; }
        public Boolean IsActingAsObscured
        {
            get;
            protected set;
        }
        /// <summary>
        /// Custom method to allow DropMenus to suppress button presses on elements they obscure while the menu is showing.
        /// </summary>
        /// <param name="menu"></param>
        public void AssertObscuringState(DropMenu menu)
        {
            IsActingAsObscured = menu.Toggle;
        }
        public Boolean AutoUpdateFrameState { get; set; }
        public Boolean ViableClick
        {
            get
            {
                return (MouseInBounds() && Enabled && !IsActingAsObscured);
            }
        }
        public Button(String name, Vector2 location, TAtlasInfo? atlas, float depth) : base(name, location, atlas, depth)
        {
            Enabled = true;
            AutoUpdateFrameState = true;
            //OnClickDo += iOnClickDo;
            if (!(this is DropMenu)) { CenterOrigin = true; }
        }
        public override void AddEventTriggers()
        {
            base.AddEventTriggers();
            Shell.MouseLeftClick += ButtonPressFunctionTrigger;
        }
        public override void RemoveEventTriggers()
        {
            base.RemoveEventTriggers();
            Shell.MouseLeftClick -= ButtonPressFunctionTrigger;
        }
        [field: NonSerialized]
        public event VoidDel ButtonPressFunction;
        [field: NonSerialized]
        public event VoidDel ButtonHoverFunction;
        [field: NonSerialized]
        public event VoidDel ButtonHoverReleaseFunction;
        public override void ClickTrigger()
        {
            ButtonPressFunctionTrigger();
            base.ClickTrigger();
        }
        protected virtual void ButtonPressFunctionTrigger()
        {
            if (ButtonPressFunction != null && MouseInBounds() && Enabled && !IsActingAsObscured && _hoverActive) { ButtonPressFunction?.Invoke(); }
        }
        private Boolean _hoverActive = false;
        public Boolean HoverActive
        {
            get { return _hoverActive; }
            protected set { _hoverActive = value; }
        }
        //Maybe update these so it doesn't constantly check mouse bounds, same for other event triggers
        public override void Update()
        {
            if (MouseInBounds() && Enabled)
            {
                if (AutoUpdateFrameState) { AtlasCoordinates = new Point(1, AtlasCoordinates.Y); }
                if (!_hoverActive)
                {
                    ButtonHoverFunction?.Invoke();
                    _hoverActive = true;
                }
            }
            else
            {
                if (AutoUpdateFrameState) { AtlasCoordinates = new Point(0, AtlasCoordinates.Y); }
                if (_hoverActive && Enabled)
                {
                    ButtonHoverReleaseFunction?.Invoke();
                    _hoverActive = false;
                }
            }
            base.Update();
        }
        public override void OnDeserializeDo()
        {
            base.OnDeserializeDo();
            AutoUpdateFrameState = true;
        }
    }
    /// <summary>
    /// a checkbox button object. Extends the Button class.
    /// </summary>
    [Serializable]
    public class Checkbox : Button
    {
        private Boolean _toggle = false;
        public Boolean Toggle
        {
            get
            {
                return _toggle;
            }
            protected set { _toggle = value; }
        }
        public Checkbox(String name, Vector2 location, TAtlasInfo? atlas, float depth, Boolean initialToggle) : base(name, location, atlas, depth)
        {
            _toggle = initialToggle;
        }
        public void ForceState(Boolean state)
        {
            _toggle = state;
        }
        protected override void ButtonPressFunctionTrigger()
        {
            if (MouseInBounds() && Enabled) { _toggle = !_toggle; }
            base.ButtonPressFunctionTrigger();
        }
        public override void Update()
        {
            base.Update();
            if (_toggle)
            {
                AtlasCoordinates = new Point(AtlasCoordinates.X, 1);
            }
            else
            {
                AtlasCoordinates = new Point(AtlasCoordinates.X, 0);
            }
        }
    }
    /// <summary>
    /// a dropdown list menu entity that extends the checkbox button type.
    /// </summary>
    [Serializable]
    public class DropMenu : Checkbox
    {
        private Color[] _interfaceColours;
        public DropMenu(String name, Vector2 location, float depth, int width, String defaultText, String[] dropList, Boolean initialToggle) : base(name, location, null, depth, initialToggle)
        {
            BoxSize = new Vector2(width, -1);
            TextOffset = new Vector2(15, 15);
            TextRenderPrepend = "";
            _interfaceColours = new Color[] { new Color(138, 0, 255, 255), new Color(70, 70, 70, 255), new Color(255, 255, 255, 200), new Color(50, 50, 50, 255), new Color(255, 255, 255, 200) };
            TAtlasInfo customAtlas = new TAtlasInfo();
            customAtlas.Atlas = ButtonScripts.CreateDynamicTextCheckbox(TextRenderPrepend + defaultText, BoxSize, TextOffset, _interfaceColours[0], _interfaceColours[1], _interfaceColours[2], _interfaceColours[4]);
            customAtlas.DivDimensions = new Point(2, 2);
            Atlas = customAtlas;
            PopulateDropList(dropList);
        }
        public DropMenu(String name, Vector2 location, float depth, Vector2 dimensions, Vector2 textOffset, int dropElementSpacing, Color[] colours, String defaultText, String textRenderPrepend, String[] dropList, Boolean initialToggle) : base(name, location, null, depth, initialToggle)
        {
            BoxSize = dimensions;
            TextOffset = textOffset;
            _interfaceColours = colours;
            _dropElementSpacing = dropElementSpacing;
            TextRenderPrepend = textRenderPrepend;
            TAtlasInfo customAtlas = new TAtlasInfo();
            customAtlas.Atlas = ButtonScripts.CreateDynamicTextCheckbox(TextRenderPrepend + defaultText, BoxSize, TextOffset, _interfaceColours[0], _interfaceColours[1], _interfaceColours[2], _interfaceColours[4]);
            customAtlas.DivDimensions = new Point(2, 2);
            Atlas = customAtlas;
            PopulateDropList(dropList);
        }
        ~DropMenu()
        {
            DepopulateDropList();
        }
        Boolean _droppedDown = false;
        private int _dropElementSpacing = 10;
        public String TextRenderPrepend { get; set; }
        public Vector2 BoxSize { get; set; }
        public Vector2 TextOffset { get; set; }
        private String _outputText = "";
        public String OutputText { get { return _outputText; } }
        public void SetTopText(String text)
        {
            _outputText = text;
            TAtlasInfo CustomAtlas = new TAtlasInfo();
            CustomAtlas.Atlas = ButtonScripts.CreateDynamicTextCheckbox(TextRenderPrepend + text, BoxSize, TextOffset, _interfaceColours[0], _interfaceColours[1], _interfaceColours[2], _interfaceColours[4]);
            CustomAtlas.DivDimensions = new Point(2, 2);
            Atlas = CustomAtlas;
            DropMenuSelectFunction?.Invoke();
        }
        public void DepopulateDropList()
        {
            foreach (WorldEntity dropDownEntity in MyDropEntities)
            {
                if (MyStickers != null && MyStickers.Contains(dropDownEntity)) { MyStickers.Remove(dropDownEntity); }
                if (Shell.UpdateQueue.Contains(dropDownEntity)) { Shell.UpdateQueue.Remove(dropDownEntity); }
                if (Shell.RenderQueue.Contains(dropDownEntity)) { Shell.RenderQueue.Remove(dropDownEntity); }
            }
            MyDropEntities = new ArrayList();
        }
        public void AssignUniqueMenuClickFuncs(VoidDel function)
        {
            foreach (Button dropButton in MyDropEntities)
            {
                VoidDel newClickFunction = new VoidDel(delegate ()
                {
                    SetTopText(dropButton.Name.Remove(0, dropButton.Name.IndexOf("_DROPOPTION_") + 12));
                    function();
                });
                dropButton.ButtonPressFunction += newClickFunction;
            }
        }
        [field: NonSerialized]
        public event VoidDel DropMenuSelectFunction;
        Texture2D _dropBackingTexture = null;
        void PopulateDropList(String[] textList)
        {
            DepopulateDropList();
            float cumulativeY = Position.Y + (Atlas.Atlas.Bounds.Height / 2) + _dropElementSpacing;
            foreach (String label in textList)
            {
                TAtlasInfo buttonAtlas = new TAtlasInfo();
                buttonAtlas.Atlas = ButtonScripts.CreateCustomButton(TextRenderPrepend + label, BoxSize, TextOffset, _interfaceColours[0], _interfaceColours[1], _interfaceColours[2]);
                buttonAtlas.DivDimensions = new Point(2, 1);
                Button button = new Button(Name + "_DROPOPTION_" + label, new Vector2(Position.X, cumulativeY), buttonAtlas, LayerDepth - 0.001f);
                SubscribeToEvent(button, EventNames.ButtonPressFunction, typeof(DropMenu).GetMethod("SetTopText"), new object[] { label });
                button.CenterOrigin = false;
                button.Enabled = _droppedDown;
                button.IsUIElement = this.IsUIElement;
                button.CameraImmune = this.CameraImmune;
                MyDropEntities.Add(button);
                MyStickers.Add(button);
                Shell.UpdateQueue.Add(button);
                cumulativeY += (Atlas.Atlas.Bounds.Height / 2) + _dropElementSpacing;
                _dropBackingTexture = VNFUtils.GetNovelTextureOfColour(Shell.DefaultShell, _interfaceColours[3], new Point((int)BoxSize.X + 10, (int)(cumulativeY - (Position.Y + (Atlas.Atlas.Bounds.Height / 2) + 10))));
            }

        }
        private ArrayList MyDropEntities = new ArrayList();
        public override void Update()
        {
            base.Update();
            if (Toggle && !_droppedDown)
            {
                foreach (Button dropButton in MyDropEntities)
                {
                    dropButton.Enabled = true;
                    dropButton.IsUIElement = this.IsUIElement;
                    dropButton.CameraImmune = this.CameraImmune;
                    Shell.RenderQueue.Add(dropButton);
                }
                _droppedDown = true;
            }
            else if (!Toggle && _droppedDown)
            {
                foreach (Button dropButton in MyDropEntities)
                {
                    dropButton.Enabled = false;
                    if (Shell.RenderQueue.Contains(dropButton)) { Shell.RenderQueue.Remove(dropButton); }
                }
                _droppedDown = false;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_droppedDown) { spriteBatch.Draw(_dropBackingTexture, Position + new Vector2(0, (Atlas.Atlas.Bounds.Height / 2)), _dropBackingTexture.Bounds, Color.White, 0f, new Vector2(), new Vector2(1, 1), SpriteEffects.None, LayerDepth - 0.002f); }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                if (_droppedDown) { spriteBatch.Draw(_dropBackingTexture, Position + camera.OffsetVector + new Vector2(0, (Atlas.Atlas.Bounds.Height / 2)), _dropBackingTexture.Bounds, Color.White, 0f, new Vector2(), camera.Size, SpriteEffects.None, LayerDepth - 0.002f); }
                base.Draw(spriteBatch, camera);
            }
        }
    }
    /// <summary>
    /// a programable slider UI object. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    public class Slider : WorldEntity
    {
        private Boolean _engaged = false;
        public Boolean Enabled { get; set; }
        public Vector2 EndpointA { get; set; }
        public Vector2 EndpointB { get; set; }
        public Slider(String name, Vector2 location, TAtlasInfo? atlas, float depth, Vector2 endpointA, Vector2 endpointB, float initialValue) : base(name, location, atlas, depth)
        {
            _engaged = false;
            Enabled = true;
            EndpointA = endpointA;
            EndpointB = endpointB;
            CenterOrigin = true;
            ForceState(initialValue);
        }
        public static Vector2 CalculatePerpendicularIntersection(Vector2 a, Vector2 b, Vector2 c)
        {
            if (a.X != b.X && a.Y != b.Y)
            {
                float gradient = (b.Y - a.Y) / (b.X - a.X);
                float yIntercept = a.Y - (gradient * a.X);
                float inverseGradient = -1f / gradient;
                float perpendictularYIntercept = c.Y - (inverseGradient * c.X);
                Vector2 intersection = new Vector2((perpendictularYIntercept - yIntercept) / (gradient - inverseGradient), 0);
                intersection.Y = (inverseGradient * intersection.X) + perpendictularYIntercept;
                return intersection;
            }
            else if (a.X == b.X)
            {
                return new Vector2(a.X, c.Y);
            }
            else if (a.Y == b.Y)
            {
                return new Vector2(c.X, a.Y);
            }
            return new Vector2();
        }
        public float Output()
        {
            return (float)(VNFUtils.GetLinearDistance(EndpointA, Position) / VNFUtils.GetLinearDistance(EndpointA, EndpointB));
        }
        public void ForceState(float state)
        {
            Position = GetLocationByOutput(state, EndpointA, EndpointB);
        }
        public static Vector2 GetLocationByOutput(float output, Vector2 a, Vector2 b)
        {
            Vector2 extension = (b - a) * output;
            return a + extension;
        }
        public override void AddEventTriggers()
        {
            base.AddEventTriggers();
            Shell.MouseLeftClick += SliderClickFunctionTrigger;
        }
        public override void RemoveEventTriggers()
        {
            base.RemoveEventTriggers();
            Shell.MouseLeftClick -= SliderClickFunctionTrigger;
        }
        public override void ClickTrigger()
        {
            SliderClickFunctionTrigger();
            base.ClickTrigger();
        }
        [field: NonSerialized]
        public event VoidDel SliderClickFunction;
        protected virtual void SliderClickFunctionTrigger()
        {
            if (MouseInBounds() && Enabled)
            {
                if (!_engaged) { _engaged = true; }
                SliderClickFunction?.Invoke();
            }
        }
        public override void Update()
        {
            if (Enabled)
            {
                MouseState mouseState = Mouse.GetState();
                if (_engaged)
                {
                    AtlasCoordinates = new Point(2, AtlasCoordinates.Y);
                    Camera myCam = new Camera("");
                    Vector2 fullyAdjustedMouseCoords = new Vector2();
                    if (UsePseudoMouse)
                    {
                        fullyAdjustedMouseCoords = PseudoMouse;
                    }
                    else
                    {
                        fullyAdjustedMouseCoords = Shell.CoordNormalize(VNFUtils.ConvertPoint(mouseState.Position));
                    }
                    if (!CameraImmune)
                    {
                        if (CustomCamera != null)
                        {
                            myCam = CustomCamera;
                        }
                        else if (Shell.AutoCamera != null)
                        {
                            myCam = Shell.AutoCamera;
                        }
                        fullyAdjustedMouseCoords = myCam.TranslateCoordsToEquivalent(fullyAdjustedMouseCoords);
                    }
                    Vector2 mouseDerived = CalculatePerpendicularIntersection(EndpointA, EndpointB, fullyAdjustedMouseCoords);
                    float greatestX = EndpointB.X >= EndpointA.X ? EndpointB.X : EndpointA.X;
                    float leastX = EndpointB.X >= EndpointA.X ? EndpointA.X : EndpointB.X;
                    float greatestY = EndpointB.Y >= EndpointA.Y ? EndpointB.Y : EndpointA.Y;
                    float leastY = EndpointB.Y >= EndpointA.Y ? EndpointA.Y : EndpointB.Y;
                    if (mouseDerived.X > greatestX) { mouseDerived.X = greatestX; }
                    if (mouseDerived.X < leastX) { mouseDerived.X = leastX; }
                    if (mouseDerived.Y > greatestY) { mouseDerived.Y = greatestY; }
                    if (mouseDerived.Y < leastY) { mouseDerived.Y = leastY; }
                    Position = mouseDerived;
                    if (mouseState.LeftButton != ButtonState.Pressed)
                    {
                        _engaged = false;
                    }
                }
                else
                {
                    if (MouseInBounds()) { AtlasCoordinates = new Point(1, AtlasCoordinates.Y); }
                    else { AtlasCoordinates = new Point(0, AtlasCoordinates.Y); }
                }
            }
            else
            {
                AtlasCoordinates = new Point(0, AtlasCoordinates.Y);
                Enabled = false;
            }
            base.Update();
        }
    }
    public interface IScrollBar
    {
        Boolean Engaged { get; set; }
        Boolean Enabled { get; set; }
        Boolean HideBar { get; }
        int ScrollFrameHeight { get; }
        float TotalScrollHeight { get; }
        int MaxHeight { get; }
        int MinHeight { get; }
        Rectangle DetectScrollRectangle { get; }
        float ExtentPosition();
        void JumpTo(float Fraction);
    }
    /// <summary>
    /// a scrollbar object that renders an accompanying graphics frame. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    [Obsolete("ScrollBar is deprecated and inefficient; preferably, a VerticalScrollPane should be used.")]
    public class ScrollBar : WorldEntity, IScrollBar
    {
        public Boolean Engaged { get; set; }
        public Boolean Enabled { get; set; }
        public Boolean HideBar
        {
            get { return _hideBar; }
        }
        private int _minHeight;
        private int _maxHeight;
        public int MinHeight
        {
            get { return _minHeight; }
            protected set { _minHeight = value; }
        }
        public int MaxHeight
        {
            get { return _maxHeight; }
            protected set { _maxHeight = value; }
        }
        private Rectangle _detectScrollRectangle;
        public Rectangle DetectScrollRectangle
        {
            get { return _detectScrollRectangle; }
            protected set
            {
                _detectScrollRectangle = value;
            }
        }
        private Rectangle _displayRect = new Rectangle();
        public Rectangle DisplayRect
        {
            get { return _displayRect; }
            protected set
            {
                _displayRect = value;
            }
        }
        public Texture2D[] DisplayScrollR;
        public Texture2D DisplayScroll = null;
        private int _scrollFrameHeight;
        public int ScrollFrameHeight
        {
            get { return _scrollFrameHeight; }
            protected set
            {
                _scrollFrameHeight = value;
            }
        }
        public float ExtentPosition()
        {
            return ((Position.Y - _minHeight) / (_maxHeight - _minHeight));
        }
        public ScrollBar(String name, Vector2 location, TAtlasInfo? atlas, float depth, Texture2D[] scrollPlane, int scrollHeight) : base(name, location, atlas, depth)
        {
            Enabled = true;
            _minHeight = (int)Position.Y;
            _maxHeight = (int)(Position.Y + scrollHeight - Hitbox.Height);
            foreach (Texture2D textturePanel in scrollPlane)
            {
                _totalScrollHeight += textturePanel.Bounds.Height;
            }
            DisplayScrollR = scrollPlane;
            _scrollFrameHeight = scrollHeight;
            DisplayScroll = CalculateDisplayTexture(DisplayScrollR);
            if (TotalScrollHeight <= ScrollFrameHeight) { _hideBar = true; }
            CenterOrigin = true;
            _displayRect = new Rectangle(0, 0, DisplayScrollR[0].Width, ScrollFrameHeight);
            _detectScrollRectangle = new Rectangle((int)Position.X - 20 - _displayRect.Width, _minHeight - (Hitbox.Height / 2), _displayRect.Width + 20 + (Hitbox.Width / 2), _displayRect.Height);
            _lastMouseScroll = Mouse.GetState().ScrollWheelValue;
            MyBehaviours.Add(new Behaviours.ScrollBarControlBehaviour(_lastMouseScroll));
        }
        ~ScrollBar()
        {
            foreach (Texture2D displayScrollIterator in DisplayScrollR)
            {
                displayScrollIterator.Dispose();
            }
            DisplayScroll.Dispose();
        }
        public void HardDispose()
        {
            foreach (Texture2D displayScrollIterator in DisplayScrollR)
            {
                displayScrollIterator.Dispose();
            }
            DisplayScroll.Dispose();
            DisplayScrollR = null;
            DisplayScroll = null;
        }
        public override void AddEventTriggers()
        {
            base.AddEventTriggers();
            Shell.MouseLeftClick += ScrollBarClickFunctionTrigger;
        }
        public override void RemoveEventTriggers()
        {
            base.RemoveEventTriggers();
            Shell.MouseLeftClick -= ScrollBarClickFunctionTrigger;
        }
        public override void ClickTrigger()
        {
            ScrollBarClickFunctionTrigger();
            base.ClickTrigger();
        }
        [field: NonSerialized]
        public event VoidDel ScrollBarClickFunction;
        protected virtual void ScrollBarClickFunctionTrigger()
        {
            if (MouseInBounds() && Enabled)
            {
                if (!Engaged) { Engaged = true; }
                ScrollBarClickFunction?.Invoke();
            }
        }
        int _lastMouseScroll = 0;
        Boolean _hideBar = false;
        private float _totalScrollHeight = 0;
        public float TotalScrollHeight
        {
            get
            {
                return _totalScrollHeight;
            }
            protected set
            {
                _totalScrollHeight = value;
            }
        }
        public void JumpTo(float fraction)
        {
            float pos = fraction * (_maxHeight - _minHeight);
            Position = new Vector2(Position.X, _minHeight + pos);
        }
        public Texture2D CalculateDisplayTexture(Texture2D[] scrollSequence)
        {
            int drsh = (int)(((float)(Position.Y - _minHeight) / (float)(_maxHeight - _minHeight)) * (float)(TotalScrollHeight - ScrollFrameHeight));
            int startTIndex = (int)Math.Floor((double)drsh / 2000d);
            Texture2D one = DisplayScrollR[startTIndex];
            Texture2D two = null;
            int dIntoOne = drsh - (2000 * startTIndex);
            int slack = ScrollFrameHeight - (2000 - dIntoOne);
            if (slack <= 0) { slack = 0; }
            Rectangle onePull = new Rectangle(0, dIntoOne, one.Width, ScrollFrameHeight - slack);
            Rectangle twoPull = new Rectangle();
            Boolean drawTwo = false;
            if (onePull.Height < ScrollFrameHeight)
            {
                drawTwo = true;
                if (startTIndex < DisplayScrollR.Length - 1)
                {
                    two = DisplayScrollR[startTIndex + 1];
                }
                twoPull = new Rectangle(0, 0, one.Width, slack);
            }
            RenderTarget2D output = new RenderTarget2D(Shell.PubGD, one.Width, ScrollFrameHeight, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(output);
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(one, new Rectangle(0, 0, onePull.Width, onePull.Height), onePull, Color.White);
            if (drawTwo) { spriteBatch.Draw(two, new Rectangle(0, onePull.Height, twoPull.Width, twoPull.Height), twoPull, Color.White); }
            spriteBatch.End();
            Shell.PubGD.SetRenderTarget(null);
            return (Texture2D)output;
        }
        public override void Update()
        {
            base.Update();
            if (!_hideBar)
            {
                DisplayScroll.Dispose();
                DisplayScroll = CalculateDisplayTexture(DisplayScrollR);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(DisplayScroll, new Rectangle((int)Position.X - 20 - _displayRect.Width, _minHeight - (Hitbox.Height / 2), _displayRect.Width, _displayRect.Height), _displayRect, Color.White, 0f, new Vector2(), SpriteEffects.None, 0.97f);
            if (!_hideBar) { base.Draw(spriteBatch); }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                spriteBatch.Draw(DisplayScroll, new Rectangle((int)(((int)Position.X - 20 - _displayRect.Width + (int)camera.OffsetVector.X) * camera.ZoomFactor.X), (int)((_minHeight - (Hitbox.Height / 2) + (int)camera.OffsetVector.Y) * camera.ZoomFactor.Y), (int)(_displayRect.Width * camera.ZoomFactor.X), (int)(_displayRect.Height * camera.ZoomFactor.Y)), _displayRect, Color.White, 0f, new Vector2(), SpriteEffects.None, 0.97f);
                if (!_hideBar) { base.Draw(spriteBatch, camera); }
            }
        }
    }
    [Serializable]
    public class VerticalScrollPane : WorldEntity, IScrollBar
    {
        public Boolean Engaged { get; set; }
        public Boolean Enabled { get; set; }
        public Boolean HideBar
        {
            get { return _hideBar; }
            protected set { _hideBar = value; }
        }
        private int _minHeight;
        private int _maxHeight;
        public int MinHeight
        {
            get { return _minHeight; }
            protected set { _minHeight = value; }
        }
        public int MaxHeight
        {
            get { return _maxHeight; }
            protected set
            {
                _maxHeight = value;
            }
        }
        private Rectangle _detectScrollRectangle;
        public Rectangle DetectScrollRectangle
        {
            get { return _detectScrollRectangle; }
            protected set
            {
                _detectScrollRectangle = value;
            }
        }
        private int _scrollFrameHeight;
        public int ScrollFrameHeight
        {
            get { return _scrollFrameHeight; }
            protected set
            {
                _scrollFrameHeight = value;
            }
        }
        public float ExtentPosition()
        {
            return ((Position.Y - MinHeight) / (MaxHeight - MinHeight));
        }
        public Pane AssociatedPane
        {
            get
            {
                return _associatedPane;
            }
            set
            {
                _associatedPane = value;
            }
        }
        private Pane _associatedPane;
        private Point _paneDimensions;
        public VerticalScrollPane(String name, Vector2 location, TAtlasInfo? atlas, float depth, Point paneDimensions, Color backgroundColour) : base(name, location, atlas, depth)
        {
            Enabled = true;
            _minHeight = (int)Position.Y;
            _maxHeight = (int)Position.Y + paneDimensions.Y - Hitbox.Height;
            _scrollFrameHeight = (int)paneDimensions.Y;
            _paneDimensions = paneDimensions;
            CenterOrigin = true;
            _detectScrollRectangle = new Rectangle((int)Position.X - 20 - (int)paneDimensions.X, MinHeight - (Hitbox.Height / 2), (int)paneDimensions.X + 20 + (Hitbox.Width / 2), (int)paneDimensions.Y);
            _lastMouseScroll = Mouse.GetState().ScrollWheelValue;
            _associatedPane = new Pane(name + "_ATTACHED_PANE", new Vector2((int)Position.X - 20 - paneDimensions.X, MinHeight - (Hitbox.Height / 2)), depth, paneDimensions, backgroundColour, Shell.PubGD);
            MyBehaviours.Add(new Behaviours.ScrollBarControlBehaviour(_lastMouseScroll));
        }
        ~VerticalScrollPane()
        {
            _associatedPane.Clear();
        }
        public override void AddEventTriggers()
        {
            base.AddEventTriggers();
            Shell.MouseLeftClick += ScrollBarClickFunctionTrigger;
        }
        public override void RemoveEventTriggers()
        {
            base.RemoveEventTriggers();
            Shell.MouseLeftClick -= ScrollBarClickFunctionTrigger;
        }
        public override void ClickTrigger()
        {
            ScrollBarClickFunctionTrigger();
            base.ClickTrigger();
        }
        [field: NonSerialized]
        public event VoidDel ScrollBarClickFunction;
        protected virtual void ScrollBarClickFunctionTrigger()
        {
            if (MouseInBounds() && Enabled)
            {
                if (!Engaged) { Engaged = true; }
                ScrollBarClickFunction?.Invoke();
            }
        }
        int _lastMouseScroll = 0;
        Boolean _hideBar = false;
        private float _totalScrollHeight = 0;
        public float TotalScrollHeight
        {
            get
            {
                return _totalScrollHeight;
            }
            set
            {
                _totalScrollHeight = value;
                if (_totalScrollHeight <= ScrollFrameHeight) { _hideBar = true; }
                else { _hideBar = false; }
            }
        }
        public void JumpTo(float fraction)
        {
            float Pos = fraction * (MaxHeight - MinHeight);
            Position = new Vector2(Position.X, MinHeight + Pos);
        }
        private TextEntity _defaultTextPaneText = null;
        public void SetAsTextPane(String text, int newlineIndent)
        {
            _associatedPane.Clear();
            if (_defaultTextPaneText == null)
            {
                _defaultTextPaneText = new TextEntity(Name + "_SCROLL_TEXT_ENTITY", "", new Vector2(20, 0), 1f);
                _defaultTextPaneText.TypeWrite = false;
                _defaultTextPaneText.BufferLength = _paneDimensions.X - 40;
                _defaultTextPaneText.ForceSplitUnchunkables = true;
                _defaultTextPaneText.Text = text;
                _defaultTextPaneText.NewlineIndent = newlineIndent;
                _defaultTextPaneText.DrawAsStatic = true;
            }
            else
            {
                _defaultTextPaneText.BufferLength = _paneDimensions.X - 40;
                _defaultTextPaneText.Text = text;
                _defaultTextPaneText.NewlineIndent = newlineIndent;
            }
            float[] yBorder = new float[] { _defaultTextPaneText.ChunkFontHeight[0]/4, _defaultTextPaneText.ChunkFontHeight[_defaultTextPaneText.ChunkCount - 1]/4 };
            _defaultTextPaneText.QuickMoveTo(new Vector2(20, yBorder[0]));
            TotalScrollHeight = _defaultTextPaneText.VerticalLength() + yBorder[0] + yBorder[1];
            _associatedPane.AddUpdate(_defaultTextPaneText);
            _associatedPane.AddRender(_defaultTextPaneText);
            UpdatePaneCameraPos();
        }
        void UpdatePaneCameraPos()
        {
            int YDown = (int)(((float)(Position.Y - MinHeight) / (float)(MaxHeight - MinHeight)) * (float)(TotalScrollHeight - ScrollFrameHeight));
            _associatedPane.DefaultPaneCamera.QuickMoveTo(new Vector2(Shell.Resolution.X / 2, Shell.Resolution.Y / 2 + YDown));
            _associatedPane.Update();
        }
        public override void Update()
        {
            base.Update();
            if (!_hideBar)
            {
                UpdatePaneCameraPos();
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_hideBar) { base.Draw(spriteBatch); }
            _associatedPane.Draw(spriteBatch);
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                if (!_hideBar) { base.Draw(spriteBatch, camera); }
                _associatedPane.Draw(spriteBatch, camera);
            }
        }
    }
    public interface ITextInputReceiver
    {
        void DoTextInputActionable(Behaviours.TextInputBehaviour myTextInputBehaviour);
        public event VoidDel TextEnteredFunction;
        public Boolean Active { get; set; }
    }
    public class MonitoringTextInputField : TextEntity, ITextInputReceiver
    {
        protected Behaviours.TextInputBehaviour myTextInput { get; set; }
        public MonitoringTextInputField(String name, String initialText, Vector2 location, float depth) : base(name, initialText, location, depth)
        {
            TypeWrite = false;
            TextRenderParamString = "[F:SYSFONT]";
            Text = TextRenderParamString + initialText;
            myTextInput = new Behaviours.TextInputBehaviour(false, initialText);
            TIREnabled += () => { myTextInput.UpdateEnabled(this); };
            MyBehaviours.Add(myTextInput);
            Active = true;
        }
        public event VoidDel TIREnabled;
        Boolean _active = false;
        public virtual Boolean Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                TIREnabled.Invoke();
                if(_active)
                {
                    Shell.UsingKeyboardInputs = this;
                }
                else if(Shell.UsingKeyboardInputs == this)
                {
                    Shell.UsingKeyboardInputs = null;
                }
            }
        }
        Boolean _enabled = true;
        Boolean _activeOnDisabled = false;
        public Boolean Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if(_enabled != value)
                {
                    if (value)
                    {
                        Active = _activeOnDisabled;
                    }
                    else
                    {
                        _activeOnDisabled = Active;
                        Active = false;
                    }
                }
                _enabled = value;
            }
        }
        public override void ManualDispose()
        {
            if(Shell.UsingKeyboardInputs == this) { Shell.UsingKeyboardInputs = null; }
            base.ManualDispose();
        }
        private String _lastSentText = "";
        public String LastSentText
        {
            get { return _lastSentText; }
            protected set { _lastSentText = value; }
        }
        public String TextRenderParamString { get; set; }
        public virtual void DoTextInputActionable(Behaviours.TextInputBehaviour myTextInputBehaviour)
        {
            StringBuilder initText = new StringBuilder(myTextInputBehaviour.HeldString.Replace('[', '(').Replace(']', ')'));
            while(Shell.SysFont.MeasureString(initText).X > BufferLength)
            {
                initText.Remove(0, 1);
            }
            Text = TextRenderParamString + initText;
            if(myTextInputBehaviour.HeldStringChangedFlag)
            {
                myTextInputBehaviour.HeldStringChangedFlag = false;
                _lastSentText = myTextInputBehaviour.LastHeldString.Replace('[', '(').Replace(']', ')');
                TextEnteredFunction?.Invoke();
            }
        }
        public virtual void ManualSendEnterSignal()
        {
            myTextInput.TextEntryTriggerOnEnterPress();
        }
        [field: NonSerialized]
        public event VoidDel TextEnteredFunction;
    }
    public class ToggleableTextInputField : MonitoringTextInputField
    {
        public ToggleableTextInputField(String name, String initialText, Vector2 location, float depth) : base(name, initialText, location, depth)
        {
            DrawAtlasComponent = true;
            TypeWrite = false;
            TextRenderParamString = "[F:SYSFONT,L:5-5]";
            Text = TextRenderParamString + initialText;
            myTextInput.TriggerWithoutEnterPress = true;
            Active = false;
        }
        public override void AddEventTriggers()
        {
            base.AddEventTriggers();
            Shell.MouseLeftClick += ToggleCheckTextInputTrigger;
        }
        public override void RemoveEventTriggers()
        {
            base.RemoveEventTriggers();
            Shell.MouseLeftClick -= ToggleCheckTextInputTrigger;
        }
        public override void ClickTrigger()
        {
            ToggleCheckTextInputTrigger();
            base.ClickTrigger();
        }
        protected virtual void ToggleCheckTextInputTrigger()
        {
            if (MouseInBounds() && !Active && !Shell.ConsoleOpen && Enabled)
            {
                Active = true;
            }
            else if (Active)
            {
                Active = false;
            }
        }
        public override Boolean Active
        {
            get
            {
                return base.Active;
            }
            set
            {
                base.Active = value;
                if (value)
                {
                    AtlasCoordinates = new Point(1, AtlasCoordinates.Y);
                }
                else
                {
                    AtlasCoordinates = new Point(0, AtlasCoordinates.Y);
                }
            }
        }
        public override void ManualSendEnterSignal()
        {
            myTextInput.TextEntryTriggerOnAny();
        }
    }
}
