﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace VNFramework
{
    public static class Behaviours
    {
        public interface IVNFBehaviour
        {
            void UpdateFunctionality(WorldEntity BehaviourOwner);
            void Clear();
        }
        public class TextInputBehaviour : IVNFBehaviour
        {
            public TextInputBehaviour()
            {
                InputUpdated = false;
                Shell.DefaultShell.Window.TextInput += HandleTextInputEvent;
            }
            public void Clear()
            {
                Shell.DefaultShell.Window.TextInput -= HandleTextInputEvent;
            }
            void HandleTextInputEvent(object EventSender, TextInputEventArgs e)
            {
                if(e.Key != Keys.Enter && e.Key != Keys.Back)
                {
                    if(e.Character != '\0') { ConstructHeldString.Append(e.Character.ToString()[0]); }
                    InputUpdated = true;
                }
                else if(e.Key == Keys.Back)
                {
                    ConstructHeldString.Remove(ConstructHeldString.Length - 2, 1);
                    InputUpdated = true;
                }
                else if(e.Key == Keys.Enter)
                {
                    pLastHeldString = HeldString;
                    ConstructHeldString = new StringBuilder();
                    InputUpdated = true;
                }
            }
            private Boolean InputUpdated;
            private StringBuilder ConstructHeldString = new StringBuilder();
            private String pLastHeldString = "";
            public String HeldString
            {
                get
                {
                    return ConstructHeldString.ToString();
                }
            }
            public String LastHeldString { get { return pLastHeldString; } }
            public void UpdateFunctionality(WorldEntity BehaviourOwner)
            {
                if(BehaviourOwner is ITextInputReceiver && InputUpdated)
                {
                    ((ITextInputReceiver)BehaviourOwner).DoTextInputActionable(this);
                    InputUpdated = false;
                }
            }
        }
        public class ScrollBarControlBehaviour : IVNFBehaviour
        {
            int LastMouseScroll;
            public ScrollBarControlBehaviour(int InLastMouseScroll)
            {
                LastMouseScroll = InLastMouseScroll;
            }
            public void Clear()
            {

            }
            public void UpdateFunctionality(WorldEntity BehaviourOwner)
            {
                IScrollBar SB = (IScrollBar)BehaviourOwner;
                if (!SB.HideBar)
                {
                    MouseState M = Mouse.GetState();
                    if (SB.Enabled)
                    {
                        Vector2 COffsetV = new Vector2();
                        Vector2 CZoomFactor = new Vector2(1, 1);
                        if (!((WorldEntity)SB).CameraImmune)
                        {
                            if (((WorldEntity)SB).CustomCamera != null)
                            {
                                COffsetV = ((WorldEntity)SB).CustomCamera.OffsetVector;
                                CZoomFactor = ((WorldEntity)SB).CustomCamera.ZoomFactor;
                            }
                            else if (Shell.AutoCamera != null)
                            {
                                COffsetV = Shell.AutoCamera.OffsetVector;
                                CZoomFactor = Shell.AutoCamera.ZoomFactor;
                            }
                        }
                        Vector2 FullyAdjustedMouseCoords = ((Shell.CoordNormalize(VNFUtils.ConvertPoint(M.Position) / CZoomFactor) - COffsetV));
                        int MY = (int)FullyAdjustedMouseCoords.Y;
                        if (M.ScrollWheelValue != LastMouseScroll && SB.DetectScrollRectangle.Contains(FullyAdjustedMouseCoords) && !SB.Engaged)
                        {
                            if (((WorldEntity)SB).DrawCoords.Y >= SB.MinHeight && ((WorldEntity)SB).DrawCoords.Y <= SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).DrawCoords.X, ((WorldEntity)SB).DrawCoords.Y + -(int)(((float)(M.ScrollWheelValue - LastMouseScroll) * (float)(SB.ScrollFrameHeight)) / (2 * (float)SB.TotalScrollHeight)))); }
                            if (((WorldEntity)SB).DrawCoords.Y < SB.MinHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).DrawCoords.X, SB.MinHeight)); }
                            else if (((WorldEntity)SB).DrawCoords.Y > SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).DrawCoords.X, SB.MaxHeight)); }
                        }
                        LastMouseScroll = M.ScrollWheelValue;
                        if (SB.Engaged)
                        {
                            ((WorldEntity)SB).SetAtlasFrame(new Point(2, ((WorldEntity)SB).AtlasCoordinates.Y));
                            if (MY < SB.MinHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).DrawCoords.X, SB.MinHeight)); }
                            else if (MY > SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).DrawCoords.X, SB.MaxHeight)); }
                            else if (MY >= SB.MinHeight && MY <= SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).DrawCoords.X, MY)); }
                            if (M.LeftButton != ButtonState.Pressed) { SB.Engaged = false; }
                        }
                        else
                        {
                            if (((WorldEntity)SB).MouseInBounds()) { ((WorldEntity)SB).SetAtlasFrame(new Point(1, ((WorldEntity)SB).AtlasCoordinates.Y)); }
                            else { ((WorldEntity)SB).SetAtlasFrame(new Point(0, ((WorldEntity)SB).AtlasCoordinates.Y)); }
                        }
                    }
                    else
                    {
                        ((WorldEntity)SB).SetAtlasFrame(new Point(0, ((WorldEntity)SB).AtlasCoordinates.Y));
                        SB.Engaged = false;
                        LastMouseScroll = M.ScrollWheelValue;
                    }
                }
            }
        }
    }
}