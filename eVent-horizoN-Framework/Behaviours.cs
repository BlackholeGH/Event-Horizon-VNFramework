using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace VNFramework
{
    public static partial class Behaviours
    {
        public interface IVNFBehaviour
        {
            public void UpdateFunctionality(WorldEntity BehaviourOwner);
            public void Clear();
        }
        [Serializable]
        public class TextInputBehaviour : IVNFBehaviour
        {
            private Boolean _triggerWithoutEnterPress = false;
            public Boolean TriggerWithoutEnterPress
            {
                get
                {
                    return _triggerWithoutEnterPress;
                }
                set
                {
                    _triggerWithoutEnterPress = value;
                    if(_triggerWithoutEnterPress && Scrollers.Count == 0)
                    {
                        Scrollers.Add(HeldString);
                    }
                }
            }
            public TextInputBehaviour(bool triggerWithoutEnterPress, String initialText)
            {
                InputUpdated = false;
                ConstructHeldString.Append(initialText);
                Shell.DefaultShell.Window.TextInput += HandleTextInputEvent;
                Shell.DefaultShell.Window.KeyDown += Up;
                Shell.DefaultShell.Window.KeyDown += Down;
                TriggerWithoutEnterPress = triggerWithoutEnterPress;
                if(TriggerWithoutEnterPress)
                {
                    Scrollers.Add(initialText);
                }
            }
            public void Clear()
            {
                try
                {
                    Shell.DefaultShell.Window.KeyDown -= Up;
                    Shell.DefaultShell.Window.KeyDown -= Down;
                    Shell.DefaultShell.Window.TextInput -= HandleTextInputEvent;
                }
                catch (NullReferenceException) { }
            }
            void HandleTextInputEvent(object EventSender, TextInputEventArgs e)
            {
                if (!_active || (Shell.UsingKeyboardInputs != null && Shell.UsingKeyboardInputs != _myLastOwner)) { return; }
                if (e.Key != Keys.Enter && e.Key != Keys.Back && e.Key != Keys.Escape && e.Key != Keys.OemTilde)
                {
                    if(e.Character != '\0') { ConstructHeldString.Append(e.Character.ToString()[0]); }
                    InputUpdated = true;
                }
                else if(e.Key == Keys.Back && ConstructHeldString.Length > 0)
                {
                    ConstructHeldString.Remove(ConstructHeldString.Length - 1, 1);
                    InputUpdated = true;
                }
                if(TriggerWithoutEnterPress && InputUpdated)
                {
                    TextEntryTriggerOnAny();
                }
                else if(e.Key == Keys.Enter)
                {
                    TextEntryTriggerOnEnterPress();
                }
            }
            public void TextEntryTriggerOnAny()
            {
                pLastHeldString = HeldString;
                //Scrollers.RemoveRange(ScrollIndex, Scrollers.Count - ScrollIndex);
                Scrollers.Add(HeldString);
                ScrollIndex = Scrollers.Count - 1;
                HeldStringChangedFlag = true;
            }
            public void TextEntryTriggerOnEnterPress()
            {
                pLastHeldString = HeldString;
                //Scrollers.RemoveRange(ScrollIndex, Scrollers.Count - ScrollIndex);
                Scrollers.Add(HeldString);
                ScrollIndex = Scrollers.Count;
                ConstructHeldString = new StringBuilder();
                InputUpdated = true;
                HeldStringChangedFlag = true;
            }
            public void Up(object EventSender, InputKeyEventArgs e)
            {
                if (!_active || (Shell.UsingKeyboardInputs != null && Shell.UsingKeyboardInputs != _myLastOwner)) { return; }
                if (ScrollIndex > 0 && e.Key == Microsoft.Xna.Framework.Input.Keys.Up)
                {
                    ScrollIndex--;
                    ConstructHeldString = new StringBuilder(Scrollers[ScrollIndex]);
                    InputUpdated = true;
                    if (TriggerWithoutEnterPress)
                    {
                        pLastHeldString = HeldString;
                        HeldStringChangedFlag = true;
                    }
                }
            }
            public void Down(object EventSender, InputKeyEventArgs e)
            {
                if (!_active || (Shell.UsingKeyboardInputs != null && Shell.UsingKeyboardInputs != _myLastOwner)) { return; }
                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Down)
                {
                    if (ScrollIndex < Scrollers.Count - 1)
                    {
                        ScrollIndex++;
                        ConstructHeldString = new StringBuilder(Scrollers[ScrollIndex]);
                        InputUpdated = true;
                        if (TriggerWithoutEnterPress)
                        {
                            pLastHeldString = HeldString;
                            HeldStringChangedFlag = true;
                        }
                    }
                    else if (ScrollIndex == Scrollers.Count - 1 && !TriggerWithoutEnterPress)
                    {
                        ScrollIndex++;
                        ConstructHeldString = new StringBuilder();
                        InputUpdated = true;
                    }
                }
            }
            private int ScrollIndex = 0;
            private List<String> Scrollers = new List<string>();
            public Boolean HeldStringChangedFlag { get; set; }
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
            private Boolean _active = false;
            public void UpdateEnabled(ITextInputReceiver textInputReceiver)
            {
                if (_active != textInputReceiver.Active) { _active = textInputReceiver.Active; }
            }
            WorldEntity _myLastOwner = null;
            public void UpdateFunctionality(WorldEntity behaviourOwner)
            {
                if(_myLastOwner != behaviourOwner) { _myLastOwner = behaviourOwner; }
                if(behaviourOwner is ITextInputReceiver && InputUpdated)
                {
                    ((ITextInputReceiver)behaviourOwner).DoTextInputActionable(this);
                    InputUpdated = false;
                }
            }
        }
        [Serializable]
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
                        Vector2 FullyAdjustedMouseCoords = new Vector2();
                        if (BehaviourOwner.UsePseudoMouse)
                        {
                            FullyAdjustedMouseCoords = BehaviourOwner.PseudoMouse - COffsetV;
                        }
                        else
                        {
                            FullyAdjustedMouseCoords = ((Shell.CoordNormalize(VNFUtils.ConvertPoint(M.Position) / CZoomFactor) - COffsetV));
                        }
                        int MY = (int)FullyAdjustedMouseCoords.Y;
                        if (M.ScrollWheelValue != LastMouseScroll && SB.DetectScrollRectangle.Contains(FullyAdjustedMouseCoords) && !SB.Engaged)
                        {
                            if (((WorldEntity)SB).Position.Y >= SB.MinHeight && ((WorldEntity)SB).Position.Y <= SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).Position.X, ((WorldEntity)SB).Position.Y + -(int)(((float)(M.ScrollWheelValue - LastMouseScroll) * (float)(SB.ScrollFrameHeight)) / (2 * (float)SB.TotalScrollHeight)))); }
                            if (((WorldEntity)SB).Position.Y < SB.MinHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).Position.X, SB.MinHeight)); }
                            else if (((WorldEntity)SB).Position.Y > SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).Position.X, SB.MaxHeight)); }
                        }
                        LastMouseScroll = M.ScrollWheelValue;
                        if (SB.Engaged)
                        {
                            ((WorldEntity)SB).SetAtlasFrame(new Point(2, ((WorldEntity)SB).AtlasCoordinates.Y));
                            if (MY < SB.MinHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).Position.X, SB.MinHeight)); }
                            else if (MY > SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).Position.X, SB.MaxHeight)); }
                            else if (MY >= SB.MinHeight && MY <= SB.MaxHeight) { ((WorldEntity)SB).QuickMoveTo(new Vector2(((WorldEntity)SB).Position.X, MY)); }
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
        [Serializable]
        public class ConsoleReaderBehaviour : IVNFBehaviour
        {
            String ConsoleText = null;
            public void HandleConsoleUpdateEvent(String Text)
            {
                ConsoleText = Shell.PullInternalConsoleData;
            }
            public ConsoleReaderBehaviour()
            {
                Shell.ConsoleWrittenTo += HandleConsoleUpdateEvent;
            }
            public void Clear()
            {
                try
                {
                    Shell.ConsoleWrittenTo -= HandleConsoleUpdateEvent;
                }
                catch (NullReferenceException) { }
            }
            public void UpdateFunctionality(WorldEntity BehaviourOwner)
            {
                if(ConsoleText != null && BehaviourOwner is VerticalScrollPane)
                {
                    ((VerticalScrollPane)BehaviourOwner).SetAsTextPane(ConsoleText, 100);
                    ((VerticalScrollPane)BehaviourOwner).JumpTo(1f);
                    ConsoleText = null;
                }
            }
        }
    }
}
