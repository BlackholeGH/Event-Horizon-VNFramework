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
        [field: NonSerialized]
        public event VoidDel OnClickDo;
        public event VoidDel OnHoverDo;
        public event VoidDel OnHoverReleaseDo;
        protected String[] OHDRecord;
        protected String[] OHRDRecord;
        public Boolean Enabled { get; set; }
        public Boolean AutoUpdateFrameState { get; set; }
        public Boolean ViableClick
        {
            get
            {
                return (MouseInBounds() && Enabled);
            }
        }
        public Button(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, VoidDel iOnClickDo) : base(Name, Location, Atlas, Depth)
        {
            Enabled = true;
            AutoUpdateFrameState = true;
            pClickable = true;
            OnClickDo += iOnClickDo;
            Shell.MouseLeftClick += MLC;
            if (!(this is DropMenu)) { CenterOrigin = true; }
        }
        public Button(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, VoidDel iOnClickDo, VoidDel iOnHoverDo, VoidDel iOnHoverReleaseDo) : base(Name, Location, Atlas, Depth)
        {
            Enabled = true;
            AutoUpdateFrameState = true;
            pClickable = true;
            OnClickDo += iOnClickDo;
            OnHoverDo += iOnHoverDo;
            OnHoverReleaseDo += iOnHoverReleaseDo;
            Shell.MouseLeftClick += MLC;
            CenterOrigin = true;
        }
        public override void MouseLeftClick()
        {
            if (MouseInBounds() && Enabled && (OnHoverDo == null || pHoverActive)) { OnClickDo(); }
        }
        protected Boolean pHoverActive = false;
        public Boolean HoverActive { get { return pHoverActive; } }
        public override void Update()
        {
            if (MouseInBounds() && Enabled)
            {
                if (AutoUpdateFrameState) { AtlasCoordinates.X = 1; }
                if (OnHoverDo != null && !pHoverActive)
                {
                    OnHoverDo();
                    pHoverActive = true;
                }
            }
            else
            {
                if (AutoUpdateFrameState) { AtlasCoordinates.X = 0; }
                if (OnHoverReleaseDo != null && pHoverActive && Enabled)
                {
                    OnHoverReleaseDo();
                    pHoverActive = false;
                }
            }
            base.Update();
        }
        public override void OnDeserializeDo()
        {
            base.OnDeserializeDo();
            AutoUpdateFrameState = true;
            if (MLCRecord != null && MLCRecord.Length > 0)
            {
                foreach (String S in MLCRecord)
                {
                    OnClickDo += ButtonScripts.DelegateFetch(S);
                }
            }
            if (OHDRecord != null && OHDRecord.Length > 0)
            {
                foreach (String S in OHDRecord)
                {
                    OnHoverDo += ButtonScripts.DelegateFetch(S);
                }
            }
            if (OHRDRecord != null && OHRDRecord.Length > 0)
            {
                foreach (String S in OHRDRecord)
                {
                    OnHoverReleaseDo += ButtonScripts.DelegateFetch(S);
                }
            }
            MLC = new VoidDel(MouseLeftClick);
            Shell.MouseLeftClick += MLC;
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
        public Checkbox(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, Boolean InitialToggle, VoidDel iOnClickDo) : base(Name, Location, Atlas, Depth, iOnClickDo)
        {
            pToggle = InitialToggle;
        }
        public void ForceState(Boolean State)
        {
            pToggle = State;
        }
        public override void MouseLeftClick()
        {
            if (MouseInBounds() && Enabled) { pToggle = !pToggle; }
            base.MouseLeftClick();
        }
        public override void Update()
        {
            base.Update();
            if (pToggle)
            {
                AtlasCoordinates.Y = 1;
            }
            else
            {
                AtlasCoordinates.Y = 0;
            }
        }
    }
    /// <summary>
    /// A dropdown list menu entity that extends the checkbox button type.
    /// </summary>
    [Serializable]
    public class DropMenu : Checkbox
    {
        public DropMenu(String Name, Vector2 Location, float Depth, int Width, String DefaultText, String[] DropList, Boolean InitialToggle, VoidDel iOnClickDo) : base(Name, Location, null, Depth, InitialToggle, iOnClickDo)
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
                B.OnClickDo += NewClickFunction;
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
                Button B = new Button(Name + "_DROPOPTION_" + Label, new Vector2(pDrawCoords.X, CumulativeY), ButtonAtlas, LayerDepth - 0.001f,
                    new VoidDel(delegate ()
                    {
                        SetTopText(Label);
                    }));
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
            if (DroppedDown) { spriteBatch.Draw(DropBackingTexture, pDrawCoords + camera.OffsetVector + new Vector2(0, (LocalAtlas.Atlas.Bounds.Height / 2)), DropBackingTexture.Bounds, Color.White, 0f, new Vector2(), camera.ScaleSize, SpriteEffects.None, LayerDepth - 0.002f); }
            base.Draw(spriteBatch, camera);
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
            pClickable = true;
            Shell.MouseLeftClick += MLC;
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
        public override void MouseLeftClick()
        {
            if (MouseInBounds() && Enabled)
            {
                if (!Engaged) { Engaged = true; }
            }
        }
        public override void Update()
        {
            if (Enabled)
            {
                MouseState M = Mouse.GetState();
                if (Engaged)
                {
                    AtlasCoordinates.X = 2;
                    Vector2 COffsetV = new Vector2();
                    Vector2 CZoomFactor = new Vector2(1, 1);
                    if (!CameraImmune)
                    {
                        if (CustomCamera != null)
                        {
                            COffsetV = CustomCamera.OffsetVector;
                            CZoomFactor = CustomCamera.ZoomFactor;
                        }
                        else if (Shell.AutoCamera != null)
                        {
                            COffsetV = Shell.AutoCamera.OffsetVector;
                            CZoomFactor = Shell.AutoCamera.ZoomFactor;
                        }
                    }
                    Vector2 FullyAdjustedMouseCoords = ((Shell.CoordNormalize(VNFUtils.ConvertPoint(M.Position) / CZoomFactor) - COffsetV));
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
                    if (MouseInBounds()) { AtlasCoordinates.X = 1; }
                    else { AtlasCoordinates.X = 0; }
                }
            }
            else
            {
                AtlasCoordinates.X = 0;
                Enabled = false;
            }
            base.Update();
        }
    }
    /// <summary>
    /// A scrollbar object that renders an accompanying graphics frame. Extends the WorldEntity class.
    /// </summary>
    [Serializable]
    public class ScrollBar : WorldEntity
    {
        protected Boolean Engaged = false;
        public Boolean Enabled { get; set; }
        protected int MinHeight;
        protected int MaxHeight;
        protected Rectangle pDisplayRect = new Rectangle();
        public Rectangle DisplayRect { get { return pDisplayRect; } }
        public Texture2D[] DisplayScrollR;
        public Texture2D DisplayScroll = null;
        protected int ScrollFrameHeight;
        protected Rectangle DetectScrollRectange;
        public float ExtentPosition()
        {
            return ((pDrawCoords.Y - MinHeight) / (MaxHeight - MinHeight));
        }
        public ScrollBar(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, Texture2D[] ScrollPlane, int ScrollHeight) : base(Name, Location, Atlas, Depth)
        {
            Enabled = true;
            MinHeight = (int)pDrawCoords.Y;
            MaxHeight = (int)(pDrawCoords.Y + ScrollHeight - HitBox.Height);
            foreach (Texture2D T in ScrollPlane)
            {
                pTotalScrollHeight += T.Bounds.Height;
            }
            DisplayScrollR = ScrollPlane;
            ScrollFrameHeight = ScrollHeight;
            DisplayScroll = CalculateDisplayTexture(DisplayScrollR);
            if (TotalScrollHeight <= ScrollFrameHeight) { HideBar = true; }
            pClickable = true;
            Shell.MouseLeftClick += MLC;
            CenterOrigin = true;
            pDisplayRect = new Rectangle(0, 0, DisplayScrollR[0].Width, ScrollFrameHeight);
            DetectScrollRectange = new Rectangle((int)pDrawCoords.X - 20 - pDisplayRect.Width, MinHeight - (HitBox.Height / 2), pDisplayRect.Width + 20 + (HitBox.Width / 2), pDisplayRect.Height);
            LastMouseScroll = Mouse.GetState().ScrollWheelValue;
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
        public override void MouseLeftClick()
        {
            if (MouseInBounds() && Enabled)
            {
                if (!Engaged) { Engaged = true; }
            }
        }
        int LastMouseScroll = 0;
        Boolean HideBar = false;
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
            float Pos = Fraction * (MaxHeight - MinHeight);
            pDrawCoords = new Vector2(pDrawCoords.X, MinHeight + Pos);
        }
        public Texture2D CalculateDisplayTexture(Texture2D[] ScrollSequence)
        {
            int DRSH = (int)(((float)(pDrawCoords.Y - MinHeight) / (float)(MaxHeight - MinHeight)) * (float)(TotalScrollHeight - ScrollFrameHeight));
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
            if (!HideBar)
            {
                MouseState M = Mouse.GetState();
                if (Enabled)
                {
                    Vector2 COffsetV = new Vector2();
                    Vector2 CZoomFactor = new Vector2(1, 1);
                    if (!CameraImmune)
                    {
                        if (CustomCamera != null)
                        {
                            COffsetV = CustomCamera.OffsetVector;
                            CZoomFactor = CustomCamera.ZoomFactor;
                        }
                        else if (Shell.AutoCamera != null)
                        {
                            COffsetV = Shell.AutoCamera.OffsetVector;
                            CZoomFactor = Shell.AutoCamera.ZoomFactor;
                        }
                    }
                    Vector2 FullyAdjustedMouseCoords = ((Shell.CoordNormalize(VNFUtils.ConvertPoint(M.Position) / CZoomFactor) - COffsetV));
                    int MY = (int)FullyAdjustedMouseCoords.Y;
                    if (M.ScrollWheelValue != LastMouseScroll && DetectScrollRectange.Contains(FullyAdjustedMouseCoords) && !Engaged)
                    {
                        if (pDrawCoords.Y >= MinHeight && pDrawCoords.Y <= MaxHeight) { pDrawCoords = new Vector2(pDrawCoords.X, pDrawCoords.Y + -(int)(((float)(M.ScrollWheelValue - LastMouseScroll) * (float)(ScrollFrameHeight)) / (2 * (float)DisplayScroll.Height))); }
                        if (pDrawCoords.Y < MinHeight) { pDrawCoords = new Vector2(pDrawCoords.X, MinHeight); }
                        else if (pDrawCoords.Y > MaxHeight) { pDrawCoords = new Vector2(pDrawCoords.X, MaxHeight); }
                    }
                    LastMouseScroll = M.ScrollWheelValue;
                    if (Engaged)
                    {
                        AtlasCoordinates.X = 2;
                        if (MY < MinHeight) { pDrawCoords = new Vector2(pDrawCoords.X, MinHeight); }
                        else if (MY > MaxHeight) { pDrawCoords = new Vector2(pDrawCoords.X, MaxHeight); }
                        else if (MY >= MinHeight && MY <= MaxHeight) { pDrawCoords = new Vector2(pDrawCoords.X, MY); }
                        if (M.LeftButton != ButtonState.Pressed) { Engaged = false; }
                    }
                    else
                    {
                        if (MouseInBounds()) { AtlasCoordinates.X = 1; }
                        else { AtlasCoordinates.X = 0; }
                    }
                }
                else
                {
                    AtlasCoordinates.X = 0;
                    Engaged = false;
                    LastMouseScroll = M.ScrollWheelValue;
                }
                DisplayScroll.Dispose();
                DisplayScroll = CalculateDisplayTexture(DisplayScrollR);
            }
            base.Update();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(DisplayScroll, new Rectangle((int)pDrawCoords.X - 20 - pDisplayRect.Width, MinHeight - (HitBox.Height / 2), pDisplayRect.Width, pDisplayRect.Height), pDisplayRect, Color.White, 0f, new Vector2(), SpriteEffects.None, 0.97f);
            if (!HideBar) { base.Draw(spriteBatch); }
        }
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (CameraImmune) { Draw(spriteBatch); }
            spriteBatch.Draw(DisplayScroll, new Rectangle((int)(((int)pDrawCoords.X - 20 - pDisplayRect.Width + (int)camera.OffsetVector.X) * camera.ZoomFactor.X), (int)((MinHeight - (HitBox.Height / 2) + (int)camera.OffsetVector.Y) * camera.ZoomFactor.Y), (int)(pDisplayRect.Width * camera.ZoomFactor.X), (int)(pDisplayRect.Height * camera.ZoomFactor.Y)), pDisplayRect, Color.White, 0f, new Vector2(), SpriteEffects.None, 0.97f);
            if (!HideBar) { base.Draw(spriteBatch, camera); }
        }
    }
}
