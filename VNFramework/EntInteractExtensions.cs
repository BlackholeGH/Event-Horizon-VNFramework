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
    /// A basic button object. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    public class Button : WorldEntity
    {
        public Boolean Enabled { get; set; }
        public Boolean AutoUpdateFrameState { get; set; }
        public Boolean ViableClick
        {
            get
            {
                return (MouseInBounds() && Enabled);
            }
        }
        public Button(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth) : base(Name, Location, Atlas, Depth)
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
            if (MouseInBounds() && Enabled && pHoverActive) { ButtonPressFunction?.Invoke(); }
        }
        protected Boolean pHoverActive = false;
        public Boolean HoverActive { get { return pHoverActive; } }
        public override void Update()
        {
            if (MouseInBounds() && Enabled)
            {
                if (AutoUpdateFrameState) { pAtlasCoordinates.X = 1; }
                if (!pHoverActive)
                {
                    ButtonHoverFunction?.Invoke();
                    pHoverActive = true;
                }
            }
            else
            {
                if (AutoUpdateFrameState) { pAtlasCoordinates.X = 0; }
                if (pHoverActive && Enabled)
                {
                    ButtonHoverReleaseFunction?.Invoke();
                    pHoverActive = false;
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
    /// A checkbox button object. Extends the Button class.
    /// </summary>
    [Serializable]
    public class Checkbox : Button
    {
        protected Boolean pToggle = false;
        public Boolean Toggle
        {
            get
            {
                return pToggle;
            }
        }
        public Checkbox(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, Boolean InitialToggle) : base(Name, Location, Atlas, Depth)
        {
            pToggle = InitialToggle;
        }
        public void ForceState(Boolean State)
        {
            pToggle = State;
        }
        protected override void ButtonPressFunctionTrigger()
        {
            if (MouseInBounds() && Enabled) { pToggle = !pToggle; }
            base.ButtonPressFunctionTrigger();
        }
        public override void Update()
        {
            base.Update();
            if (pToggle)
            {
                pAtlasCoordinates.Y = 1;
            }
            else
            {
                pAtlasCoordinates.Y = 0;
            }
        }
    }
    /// <summary>
    /// A dropdown list menu entity that extends the checkbox button type.
    /// </summary>
    [Serializable]
    public class DropMenu : Checkbox
    {
        public DropMenu(String Name, Vector2 Location, float Depth, int Width, String DefaultText, String[] DropList, Boolean InitialToggle) : base(Name, Location, null, Depth, InitialToggle)
        {
            BoxWidth = Width;
            TAtlasInfo CustomAtlas = new TAtlasInfo();
            CustomAtlas.Atlas = ButtonScripts.CreateDynamicTextCheckbox(DefaultText, BoxWidth);
            CustomAtlas.DivDimensions = new Point(2, 2);
            LocalAtlas = CustomAtlas;
            PopulateDropList(DropList);
        }
        ~DropMenu()
        {
            DepopulateDropList();
        }
        Boolean DroppedDown = false;
        public int BoxWidth { get; set; }
        private String pOutputText = "";
        public String OutputText { get { return pOutputText; } }
        public void SetTopText(String Text)
        {
            pOutputText = Text;
            TAtlasInfo CustomAtlas = new TAtlasInfo();
            CustomAtlas.Atlas = ButtonScripts.CreateDynamicTextCheckbox(Text, BoxWidth);
            CustomAtlas.DivDimensions = new Point(2, 2);
            LocalAtlas = CustomAtlas;
        }
        public void DepopulateDropList()
        {
            foreach (WorldEntity E in MyDropEntities)
            {
                if (Stickers != null && Stickers.Contains(E)) { Stickers.Remove(E); }
                if (Shell.UpdateQueue.Contains(E)) { Shell.UpdateQueue.Remove(E); }
                if (Shell.RenderQueue.Contains(E)) { Shell.RenderQueue.Remove(E); }
            }
            MyDropEntities = new ArrayList();
        }
        public void AssignMenuClickFuncs(VoidDel function)
        {
            foreach (Button B in MyDropEntities)
            {
                VoidDel NewClickFunction = new VoidDel(delegate ()
                {
                    SetTopText(B.Name.Remove(0, B.Name.IndexOf("_DROPOPTION_") + 12));
                    function();
                });
                B.ButtonPressFunction += NewClickFunction;
            }
        }
        Texture2D DropBackingTexture = null;
        void PopulateDropList(String[] TextList)
        {
            DepopulateDropList();
            float CumulativeY = pDrawCoords.Y + (LocalAtlas.Atlas.Bounds.Height / 2) + 10;
            foreach (String Label in TextList)
            {
                TAtlasInfo ButtonAtlas = new TAtlasInfo();
                ButtonAtlas.Atlas = ButtonScripts.CreateDynamicCustomButton(Label, BoxWidth);
                ButtonAtlas.DivDimensions = new Point(2, 1);
                Button B = new Button(Name + "_DROPOPTION_" + Label, new Vector2(pDrawCoords.X, CumulativeY), ButtonAtlas, LayerDepth - 0.001f);
                B.SubscribeToEvent(EventNames.ButtonPressFunction, typeof(Button).GetMethod("SetTopText"), new object[] { Label });
                B.CenterOrigin = false;
                B.Enabled = DroppedDown;
                MyDropEntities.Add(B);
                Stickers.Add(B);
                Shell.UpdateQueue.Add(B);
                CumulativeY += (LocalAtlas.Atlas.Bounds.Height / 2) + 10;
                DropBackingTexture = VNFUtils.GetNovelTextureOfColour(Shell.DefaultShell, new Color(50, 50, 50, 255), new Point(BoxWidth + 10, (int)(CumulativeY - (pDrawCoords.Y + (LocalAtlas.Atlas.Bounds.Height / 2) + 10))));
            }

        }
        private ArrayList MyDropEntities = new ArrayList();
        public override void Update()
        {
            base.Update();
            if (pToggle && !DroppedDown)
            {
                foreach (Button B in MyDropEntities)
                {
                    B.Enabled = true;
                    Shell.RenderQueue.Add(B);
                }
                DroppedDown = true;
            }
            else if (!pToggle && DroppedDown)
            {
                foreach (Button B in MyDropEntities)
                {
                    B.Enabled = false;
                    if (Shell.RenderQueue.Contains(B)) { Shell.RenderQueue.Remove(B); }
                }
                DroppedDown = false;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (DroppedDown) { spriteBatch.Draw(DropBackingTexture, pDrawCoords + new Vector2(0, (LocalAtlas.Atlas.Bounds.Height / 2)), DropBackingTexture.Bounds, Color.White, 0f, new Vector2(), new Vector2(1, 1), SpriteEffects.None, LayerDepth - 0.002f); }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                if (DroppedDown) { spriteBatch.Draw(DropBackingTexture, pDrawCoords + camera.OffsetVector + new Vector2(0, (LocalAtlas.Atlas.Bounds.Height / 2)), DropBackingTexture.Bounds, Color.White, 0f, new Vector2(), camera.ScaleSize, SpriteEffects.None, LayerDepth - 0.002f); }
                base.Draw(spriteBatch, camera);
            }
        }
    }
    /// <summary>
    /// A programable slider UI object. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    public class Slider : WorldEntity
    {
        protected Boolean Engaged = false;
        public Boolean Enabled { get; set; }
        public Vector2 EndpointA { get; set; }
        public Vector2 EndpointB { get; set; }
        public Slider(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, Vector2 endpointA, Vector2 endpointB, float InitialValue) : base(Name, Location, Atlas, Depth)
        {
            Engaged = false;
            Enabled = true;
            EndpointA = endpointA;
            EndpointB = endpointB;
            CenterOrigin = true;
            ForceState(InitialValue);
        }
        public static Vector2 CalculatePerpendicularIntersection(Vector2 A, Vector2 B, Vector2 C)
        {
            if (A.X != B.X && A.Y != B.Y)
            {
                float Gradient = (B.Y - A.Y) / (B.X - A.X);
                float YIntercept = A.Y - (Gradient * A.X);
                float InverseGradient = -1f / Gradient;
                float PerpendictularYIntercept = C.Y - (InverseGradient * C.X);
                Vector2 Intersection = new Vector2((PerpendictularYIntercept - YIntercept) / (Gradient - InverseGradient), 0);
                Intersection.Y = (InverseGradient * Intersection.X) + PerpendictularYIntercept;
                return Intersection;
            }
            else if (A.X == B.X)
            {
                return new Vector2(A.X, C.Y);
            }
            else if (A.Y == B.Y)
            {
                return new Vector2(C.X, A.Y);
            }
            return new Vector2();
        }
        public float Output()
        {
            return (float)(VNFUtils.GetLinearDistance(EndpointA, pDrawCoords) / VNFUtils.GetLinearDistance(EndpointA, EndpointB));
        }
        public void ForceState(float State)
        {
            pDrawCoords = GetLocationByOutput(State, EndpointA, EndpointB);
        }
        public static Vector2 GetLocationByOutput(float Output, Vector2 A, Vector2 B)
        {
            Vector2 Extension = (B - A) * Output;
            return A + Extension;
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
                if (!Engaged) { Engaged = true; }
                SliderClickFunction?.Invoke();
            }
        }
        public override void Update()
        {
            if (Enabled)
            {
                MouseState M = Mouse.GetState();
                if (Engaged)
                {
                    pAtlasCoordinates.X = 2;
                    Camera MyCam = new Camera("");
                    Vector2 FullyAdjustedMouseCoords = Shell.CoordNormalize(VNFUtils.ConvertPoint(M.Position));
                    if (!CameraImmune)
                    {
                        if (CustomCamera != null)
                        {
                            MyCam = CustomCamera;
                        }
                        else if (Shell.AutoCamera != null)
                        {
                            MyCam = Shell.AutoCamera;
                        }
                        FullyAdjustedMouseCoords = MyCam.TranslateCoordsToEquivalent(FullyAdjustedMouseCoords);
                    }
                    Vector2 MouseDerived = CalculatePerpendicularIntersection(EndpointA, EndpointB, FullyAdjustedMouseCoords);
                    float GreatestX = EndpointB.X >= EndpointA.X ? EndpointB.X : EndpointA.X;
                    float LeastX = EndpointB.X >= EndpointA.X ? EndpointA.X : EndpointB.X;
                    float GreatestY = EndpointB.Y >= EndpointA.Y ? EndpointB.Y : EndpointA.Y;
                    float LeastY = EndpointB.Y >= EndpointA.Y ? EndpointA.Y : EndpointB.Y;
                    if (MouseDerived.X > GreatestX) { MouseDerived.X = GreatestX; }
                    if (MouseDerived.X < LeastX) { MouseDerived.X = LeastX; }
                    if (MouseDerived.Y > GreatestY) { MouseDerived.Y = GreatestY; }
                    if (MouseDerived.Y < LeastY) { MouseDerived.Y = LeastY; }
                    pDrawCoords = MouseDerived;
                    if (M.LeftButton != ButtonState.Pressed)
                    {
                        Engaged = false;
                    }
                }
                else
                {
                    if (MouseInBounds()) { pAtlasCoordinates.X = 1; }
                    else { pAtlasCoordinates.X = 0; }
                }
            }
            else
            {
                pAtlasCoordinates.X = 0;
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
    /// A scrollbar object that renders an accompanying graphics frame. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    [Obsolete("ScrollBar is deprecated and inefficient; preferably, a VerticalScrollPane should be used.")]
    public class ScrollBar : WorldEntity, IScrollBar
    {
        public Boolean Engaged { get; set; }
        public Boolean Enabled { get; set; }
        public Boolean HideBar
        {
            get { return pHideBar; }
        }
        protected int pMinHeight;
        protected int pMaxHeight;
        public int MinHeight
        {
            get { return pMinHeight; }
        }
        public int MaxHeight
        {
            get { return pMaxHeight; }
        }
        protected Rectangle pDetectScrollRectangle;
        public Rectangle DetectScrollRectangle
        {
            get { return pDetectScrollRectangle; }
        }
        protected Rectangle pDisplayRect = new Rectangle();
        public Rectangle DisplayRect { get { return pDisplayRect; } }
        public Texture2D[] DisplayScrollR;
        public Texture2D DisplayScroll = null;
        protected int pScrollFrameHeight;
        public int ScrollFrameHeight
        {
            get { return pScrollFrameHeight; }
        }
        public float ExtentPosition()
        {
            return ((pDrawCoords.Y - pMinHeight) / (pMaxHeight - pMinHeight));
        }
        public ScrollBar(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, Texture2D[] ScrollPlane, int ScrollHeight) : base(Name, Location, Atlas, Depth)
        {
            Enabled = true;
            pMinHeight = (int)pDrawCoords.Y;
            pMaxHeight = (int)(pDrawCoords.Y + ScrollHeight - HitBox.Height);
            foreach (Texture2D T in ScrollPlane)
            {
                pTotalScrollHeight += T.Bounds.Height;
            }
            DisplayScrollR = ScrollPlane;
            pScrollFrameHeight = ScrollHeight;
            DisplayScroll = CalculateDisplayTexture(DisplayScrollR);
            if (TotalScrollHeight <= ScrollFrameHeight) { pHideBar = true; }
            CenterOrigin = true;
            pDisplayRect = new Rectangle(0, 0, DisplayScrollR[0].Width, ScrollFrameHeight);
            pDetectScrollRectangle = new Rectangle((int)pDrawCoords.X - 20 - pDisplayRect.Width, pMinHeight - (HitBox.Height / 2), pDisplayRect.Width + 20 + (HitBox.Width / 2), pDisplayRect.Height);
            LastMouseScroll = Mouse.GetState().ScrollWheelValue;
            MyBehaviours.Add(new Behaviours.ScrollBarControlBehaviour(LastMouseScroll));
        }
        ~ScrollBar()
        {
            foreach (Texture2D DisplayScrolli in DisplayScrollR)
            {
                DisplayScrolli.Dispose();
            }
            DisplayScroll.Dispose();
        }
        public void HardDispose()
        {
            foreach (Texture2D DisplayScrolli in DisplayScrollR)
            {
                DisplayScrolli.Dispose();
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
        int LastMouseScroll = 0;
        Boolean pHideBar = false;
        protected float pTotalScrollHeight = 0;
        public float TotalScrollHeight
        {
            get
            {
                return pTotalScrollHeight;
            }
        }
        public void JumpTo(float Fraction)
        {
            float Pos = Fraction * (pMaxHeight - pMinHeight);
            pDrawCoords = new Vector2(pDrawCoords.X, pMinHeight + Pos);
        }
        public Texture2D CalculateDisplayTexture(Texture2D[] ScrollSequence)
        {
            int DRSH = (int)(((float)(pDrawCoords.Y - pMinHeight) / (float)(pMaxHeight - pMinHeight)) * (float)(TotalScrollHeight - ScrollFrameHeight));
            int StartTIndex = (int)Math.Floor((double)DRSH / 2000d);
            Texture2D One = DisplayScrollR[StartTIndex];
            Texture2D Two = null;
            int DIntoOne = DRSH - (2000 * StartTIndex);
            int Slack = ScrollFrameHeight - (2000 - DIntoOne);
            if (Slack <= 0) { Slack = 0; }
            Rectangle OnePull = new Rectangle(0, DIntoOne, One.Width, ScrollFrameHeight - Slack);
            Rectangle TwoPull = new Rectangle();
            Boolean DrawTwo = false;
            if (OnePull.Height < ScrollFrameHeight)
            {
                DrawTwo = true;
                if (StartTIndex < DisplayScrollR.Length - 1)
                {
                    Two = DisplayScrollR[StartTIndex + 1];
                }
                TwoPull = new Rectangle(0, 0, One.Width, Slack);
            }
            RenderTarget2D Output = new RenderTarget2D(Shell.PubGD, One.Width, ScrollFrameHeight, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(Output);
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(One, new Rectangle(0, 0, OnePull.Width, OnePull.Height), OnePull, Color.White);
            if (DrawTwo) { spriteBatch.Draw(Two, new Rectangle(0, OnePull.Height, TwoPull.Width, TwoPull.Height), TwoPull, Color.White); }
            spriteBatch.End();
            Shell.PubGD.SetRenderTarget(null);
            return (Texture2D)Output;
        }
        public override void Update()
        {
            base.Update();
            if (!pHideBar)
            {
                DisplayScroll.Dispose();
                DisplayScroll = CalculateDisplayTexture(DisplayScrollR);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(DisplayScroll, new Rectangle((int)pDrawCoords.X - 20 - pDisplayRect.Width, pMinHeight - (HitBox.Height / 2), pDisplayRect.Width, pDisplayRect.Height), pDisplayRect, Color.White, 0f, new Vector2(), SpriteEffects.None, 0.97f);
            if (!pHideBar) { base.Draw(spriteBatch); }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                spriteBatch.Draw(DisplayScroll, new Rectangle((int)(((int)pDrawCoords.X - 20 - pDisplayRect.Width + (int)camera.OffsetVector.X) * camera.ZoomFactor.X), (int)((pMinHeight - (HitBox.Height / 2) + (int)camera.OffsetVector.Y) * camera.ZoomFactor.Y), (int)(pDisplayRect.Width * camera.ZoomFactor.X), (int)(pDisplayRect.Height * camera.ZoomFactor.Y)), pDisplayRect, Color.White, 0f, new Vector2(), SpriteEffects.None, 0.97f);
                if (!pHideBar) { base.Draw(spriteBatch, camera); }
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
            get { return pHideBar; }
        }
        protected int pMinHeight;
        protected int pMaxHeight;
        public int MinHeight
        {
            get { return pMinHeight; }
        }
        public int MaxHeight
        {
            get { return pMaxHeight; }
        }
        protected Rectangle pDetectScrollRectangle;
        public Rectangle DetectScrollRectangle
        {
            get { return pDetectScrollRectangle; }
        }
        protected int pScrollFrameHeight;
        public int ScrollFrameHeight
        {
            get { return pScrollFrameHeight; }
        }
        public float ExtentPosition()
        {
            return ((pDrawCoords.Y - MinHeight) / (MaxHeight - MinHeight));
        }
        public Pane AssociatedPane
        {
            get
            {
                return pAssociatedPane;
            }
            set
            {
                pAssociatedPane = value;
            }
        }
        protected Pane pAssociatedPane;
        protected Point pPaneDimensions;
        public VerticalScrollPane(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, Point PaneDimensions, Color BackgroundColour) : base(Name, Location, Atlas, Depth)
        {
            Enabled = true;
            pMinHeight = (int)pDrawCoords.Y;
            pMaxHeight = (int)(pDrawCoords.Y + PaneDimensions.Y - HitBox.Height);
            pScrollFrameHeight = (int)PaneDimensions.Y;
            pPaneDimensions = PaneDimensions;
            CenterOrigin = true;
            pDetectScrollRectangle = new Rectangle((int)pDrawCoords.X - 20 - (int)PaneDimensions.X, MinHeight - (HitBox.Height / 2), (int)PaneDimensions.X + 20 + (HitBox.Width / 2), (int)PaneDimensions.Y);
            LastMouseScroll = Mouse.GetState().ScrollWheelValue;
            pAssociatedPane = new Pane(Name + "_ATTACHED_PANE", new Vector2((int)pDrawCoords.X - 20 - PaneDimensions.X, MinHeight - (HitBox.Height / 2)), Depth, PaneDimensions, BackgroundColour, Shell.PubGD);
            MyBehaviours.Add(new Behaviours.ScrollBarControlBehaviour(LastMouseScroll));
        }
        ~VerticalScrollPane()
        {
            pAssociatedPane.Clear();
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
        int LastMouseScroll = 0;
        Boolean pHideBar = false;
        protected float pTotalScrollHeight = 0;
        public float TotalScrollHeight
        {
            get
            {
                return pTotalScrollHeight;
            }
            set
            {
                pTotalScrollHeight = value;
                if (pTotalScrollHeight <= ScrollFrameHeight) { pHideBar = true; }
                else { pHideBar = false; }
            }
        }
        public void JumpTo(float Fraction)
        {
            float Pos = Fraction * (MaxHeight - MinHeight);
            pDrawCoords = new Vector2(pDrawCoords.X, MinHeight + Pos);
        }
        public void SetAsTextPane(String Text)
        {
            pAssociatedPane.Clear();
            TextEntity T = new TextEntity(Name + "_SCROLL_TEXT_ENTITY", "", new Vector2(20, 20), 1f);
            T.TypeWrite = false;
            T.BufferLength = pPaneDimensions.X - 40;
            T.Text = Text;
            pAssociatedPane.AddUpdate(T);
            pAssociatedPane.AddRender(T);
            TotalScrollHeight = T.VerticalLength() + 40;
            UpdatePaneCameraPos();
        }
        void UpdatePaneCameraPos()
        {
            int YDown = (int)(((float)(pDrawCoords.Y - MinHeight) / (float)(MaxHeight - MinHeight)) * (float)(TotalScrollHeight - ScrollFrameHeight));
            pAssociatedPane.DefaultPaneCamera.QuickMoveTo(new Vector2(640, YDown + 360));
            pAssociatedPane.Update();
        }
        public override void Update()
        {
            base.Update();
            if (!pHideBar)
            {
                UpdatePaneCameraPos();
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!pHideBar) { base.Draw(spriteBatch); }
            pAssociatedPane.Draw(spriteBatch);
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            else
            {
                if (!pHideBar) { base.Draw(spriteBatch, camera); }
                pAssociatedPane.Draw(spriteBatch, camera);
            }
        }
    }
    public interface ITextInputReceiver
    {
        void DoTextInputActionable(Behaviours.TextInputBehaviour MyTextInputBehaviour);
    }
}
