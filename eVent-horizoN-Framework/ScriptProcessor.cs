using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Assimp;
using SharpFont.Cache;
using static Assimp.Metadata;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Xml.Linq;

namespace VNFramework
{
    public static class ScriptProcessor
    {
        public static Dictionary<string, object[]> ScriptCache { get; set; }
        public static object[] RetrieveScriptByName(String scriptName)
        {
            if (ScriptCache.ContainsKey(scriptName.ToUpper())) { return ScriptCache[scriptName.ToUpper()]; }
            object[] script = new object[0];
            return script;
        }
        public static int CountApplicableRollbacks { get; set; }
        public static Boolean AllowScriptShift { get; set; }
        public static Stack PastStates = new Stack();
        public static String SongCom = "";
        static String s_textArchive = "";
        public static Boolean AllowScriptExit { get; set; }
        public static Boolean AssertGameRunningWithoutScript { get; set; }
        public static Boolean ActiveGame()
        {
            foreach(WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if(worldEntity is ScriptSniffer && worldEntity.Name != "INTRO_SNIFFER_UNIQUE") { return true; }
            }
            return AssertGameRunningWithoutScript;
        }
        public static ScriptSniffer SnifferSearch()
        {
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity is ScriptSniffer)
                {
                    return (ScriptSniffer)worldEntity;
                }
            }
            return null;
        }
        public static void SearchAndThrowForExit()
        {
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity is ScriptSniffer)
                {
                    ((ScriptSniffer)worldEntity).ThrowForInstantExit();
                    break;
                }
            }
        }
        public static void WriteArchive(String text, String label)
        {
            if (text != "" || label != "")
            {
                CountApplicableRollbacks++;
                if (s_textArchive != "") { s_textArchive += "[TADM][N] [N]"; }
                if (label != "") { s_textArchive += ("[C:PURPLE]" + label + "[N]"); }
                s_textArchive += text;
            }
            //Shell.WriteLine(Label + ": " + Text);
        }
        static Stack<int> s_applicableRollbackArchive = new Stack<int>();
        public static void RollbackArchive()
        {
            //What even is this? Check it actually works right
            int modifierNumber = s_applicableRollbackArchive.Pop() - 1;
            int realRollbacks = CountApplicableRollbacks + Math.Max(modifierNumber, 0);
            Shell.WriteLine("Received archive rollback request. Wiping " + realRollbacks + " line(s).");
            for (int i = realRollbacks; i > 0; i--)
            {
                if (s_textArchive.Contains("[TADM]"))
                {
                    s_textArchive = s_textArchive.Remove(s_textArchive.LastIndexOf("[TADM]"));
                }
                else { s_textArchive = ""; }
            }
            //CountApplicableRollbacks = ApplicableRollbackArchive.Pop();
        }
        public static void WipeArchive()
        {
            s_textArchive = "";
            Shell.WriteLine("Dialogue archive wiped.");
        }
        public static String PullArchiveText()
        {
            return s_textArchive;
        }
        public static Texture2D[] PullArchive()
        {
            return ButtonScripts.CreateDynamicScroll(s_textArchive, 1000);
        }
        public static void ClearNonUIEntities()
        {
            foreach(WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if(!Shell.DeleteQueue.Contains(worldEntity) && !(worldEntity is ScriptSniffer || worldEntity.Name == "UIBOX" || worldEntity.Name == "BLACK" || worldEntity.Name == "BUTTON_ARCHIVE" || worldEntity.Name == "BUTTON_SKIP" || worldEntity.Name == "BUTTON_PAUSEMENU" || worldEntity.Name == "BUTTON_ROLLBACK" || worldEntity.Name == "BUTTON_NAVSCREEN" || worldEntity.Name == "BUTTON_HIDE_UI"))
                {
                    Shell.DeleteQueue.Add(worldEntity);
                }
            }
            foreach (WorldEntity worldEntity in Shell.RenderQueue)
            {
                if (!Shell.DeleteQueue.Contains(worldEntity) && !(worldEntity is ScriptSniffer || worldEntity.Name == "UIBOX" || worldEntity.Name == "BLACK" || worldEntity.Name == "BUTTON_ARCHIVE" || worldEntity.Name == "BUTTON_SKIP" || worldEntity.Name == "BUTTON_PAUSEMENU" || worldEntity.Name == "BUTTON_ROLLBACK" || worldEntity.Name == "BUTTON_NAVSCREEN" || worldEntity.Name == "BUTTON_HIDE_UI"))
                {
                    Shell.DeleteQueue.Add(worldEntity);
                }
            }
        }
        public static Dictionary<string, object[]> ExtractEventScriptArchive(String scriptArchiveContent)
        {
            scriptArchiveContent = String.Join("\n", scriptArchiveContent.Split('\n').Select(x => ((String)x).StartsWith("//") ? "" : x));
            scriptArchiveContent = VNFUtils.Strings.ReplaceExclosed(scriptArchiveContent, "{{", "^", '\"');
            scriptArchiveContent = VNFUtils.Strings.ReplaceExclosed(scriptArchiveContent, "}}", "^", '\"');
            scriptArchiveContent = VNFUtils.Strings.RemoveExclosed(scriptArchiveContent, '\n', '^');
            scriptArchiveContent = VNFUtils.Strings.ReplaceEnclosedExclosed(scriptArchiveContent, "{", "£", '^', '\"');
            scriptArchiveContent = VNFUtils.Strings.ReplaceEnclosedExclosed(scriptArchiveContent, "}", "$", '^', '\"');
            scriptArchiveContent = scriptArchiveContent.Replace("\r", "");
            List<String> individualScripts = new List<String>();
            int nextStart = VNFUtils.Strings.IndexOfExclosed(scriptArchiveContent, "declare_script", '\"');
            scriptArchiveContent = scriptArchiveContent.Remove(0, nextStart + 14);
            do
            {
                nextStart = VNFUtils.Strings.IndexOfExclosed(scriptArchiveContent, "declare_script", '\"');
                if(nextStart > 0)
                {
                    individualScripts.Add(scriptArchiveContent.Remove(nextStart));
                    scriptArchiveContent = scriptArchiveContent.Remove(0, nextStart + 14);
                }
                else { individualScripts.Add(scriptArchiveContent); }
            }
            while (nextStart > 0);
            Dictionary<string, string> namedScripts = new Dictionary<string, string>();
            foreach(String script in individualScripts)
            {
                String nameless = script.Remove(0, script.IndexOf('\"') + 1);
                String name = nameless.Remove(nameless.IndexOf('\"'));
                nameless = nameless.Remove(0, nameless.IndexOf('\"'));
                nameless = nameless.Remove(0, nameless.IndexOf(':'));
                nameless = nameless.TrimEnd(' ');
                nameless = nameless.TrimEnd('\n');
                if (nameless.EndsWith("}")) { nameless = nameless + ","; }
                namedScripts.Add(name, nameless);
            }
            Dictionary<string,object[]> eventScripts = new Dictionary<string, object[]>();
            foreach(String scriptKey in namedScripts.Keys)
            {
                eventScripts.Add(scriptKey.ToUpper(), AssembleEventScript((String)namedScripts[scriptKey]));
            }
            return eventScripts;
        }
        /// <summary>
        /// Extract a factory command string and assembles the delegate, either running or returning it.
        /// </summary>
        /// <param name="command">Command string containing the factory schema.</param>
        /// <param name="assemblyMode">1: Runs in regular WorldEntity assembly mode. 2: Runs in "RUN" assembly mode. Other: Returns null.</param>
        /// <param name="executeInstantly">Boolean flag for whether the delegate should be returned or put on the run queue.</param>
        /// <returns></returns>
        private static VoidDel? RunFactoryCommand(string command, int assemblyMode, Boolean executeInstantly)
        {
            String factoryBlueprint = command.Remove(0, command.IndexOf('^') + 1);
            factoryBlueprint = VNFUtils.Strings.RemoveExclosed(factoryBlueprint, '^', '\"');
            factoryBlueprint = VNFUtils.Strings.ReplaceExclosed(factoryBlueprint, "£", "{", '\"');
            factoryBlueprint = VNFUtils.Strings.ReplaceExclosed(factoryBlueprint, "$", "}", '\"');
            factoryBlueprint = factoryBlueprint.Trim('\n');
            VoidDel factoryDelegate;
            if (assemblyMode == 1)
            {
                factoryDelegate = new VoidDel(delegate ()
                {
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        EntityFactory.Assemble(factoryBlueprint);
                    }));
                });
            }
            else if(assemblyMode == 2)
            {
                factoryDelegate = new VoidDel(delegate ()
                {
                    Shell.RunQueue.Add(EntityFactory.AssembleVoidDelegate(factoryBlueprint));
                });
            }
            else { return null; }
            if(executeInstantly)
            {
                factoryDelegate();
                return null;
            }
            else
            {
                return factoryDelegate;
            }
        }
        public static Object[] AssembleEventScript(String stringFormatScript)
        {
            stringFormatScript = VNFUtils.Strings.RemoveExclosed(stringFormatScript, ' ', '\"');
            ArrayList eventScriptAssembled = new ArrayList();
            while (VNFUtils.Strings.ContainsExclosed(stringFormatScript, '{', '\"'))
            {
                stringFormatScript = stringFormatScript.Remove(0, stringFormatScript.IndexOf('{') + 1);
                int endIndex = VNFUtils.Strings.IndexOfExclosed(stringFormatScript, "},", '\"');
                String foundScriptShift = stringFormatScript.Remove(endIndex);
                stringFormatScript = stringFormatScript.Remove(0, endIndex + 2);
                String[] scriptCommands = VNFUtils.Strings.SplitAtExclosed(foundScriptShift, ',', new char[] { '\"', '^' });
                Object[] thisTrueShift = new Object[scriptCommands.Length];
                int commandIndex = 0;
                foreach(String command in scriptCommands)
                {
                    if (command[0] == '\"' && command[command.Length - 1] == '\"') { thisTrueShift[commandIndex] = command.Remove(0, 1).Remove(command.Length - 2); }
                    else if (command.StartsWith("FACTORY"))
                    {
                        thisTrueShift[commandIndex] = RunFactoryCommand(command, 1, false);
                    }
                    else if (command.StartsWith("RUN"))
                    {
                        thisTrueShift[commandIndex] = RunFactoryCommand(command, 2, false);
                    }
                    else if (command.StartsWith("MERGE_IN"))
                    {
                        String mergeName = command.Replace("MERGE_IN=\"", "");
                        mergeName = mergeName.Remove(mergeName.Length - 1);
                        object[] mergeScript = ScriptProcessor.RetrieveScriptByName(mergeName);
                        for(int i = 0; i < mergeScript.Length; i++)
                        {
                            eventScriptAssembled.Add(mergeScript[i]);
                        }
                        commandIndex--;
                    }
                    commandIndex++;
                }
                eventScriptAssembled.Add(thisTrueShift);
            }
            return eventScriptAssembled.ToArray();
        }
        public class ScriptParseException : Exception
        {
            public ScriptParseException(String arg) : base(arg)
            { }
        }
        [Serializable]
        public class ScriptSniffer : WorldEntity
        {
            Boolean _skipAll = false;
            public static String[] ShiftCondition = new String[0];
            private String[] _localShiftCondition = new String[0];
            int _shiftStartTime = Environment.TickCount;
            int _scriptIndex = 0;
            public int Index { get { return _scriptIndex; } }
            public void ThrowForInstantExit()
            {
                String exitScript = Sofia.GetExitThrowLocation(Name, ShiftCondition);
                AllowScriptShift = false;
                ActivateScriptElement("D|#CBUTTONS");
                CeaseSkipping();
                _scriptIndex = 0;
                _myScript = RetrieveScriptByName(exitScript);
                _scriptName = exitScript;
                Name = exitScript + "_SNIFFER";
                _loadMode = false;
                InitScriptShift();
                AllowScriptShift = true;
            }
            public Boolean PlacedInPauseState { get; set; }
            private int _hungTime = -1;
            public void TimeHang()
            {
                _hungTime = ShiftTimeElapsed();
            }
            public void UnHang()
            {
                if (_hungTime != -1)
                {
                    _shiftStartTime = Environment.TickCount - _hungTime;
                    _hungTime = -1;
                }
            }
            int ShiftTimeElapsed()
            {
                return Environment.TickCount - _shiftStartTime;
            }
            [field: NonSerialized]
            object[] _myScript = new object[0][];
            public object[] MyScript { get { return _myScript; } }
            public String ScriptThrowTarget { get; set; }
            private Boolean _loadMode = false;
            public ScriptSniffer(String name, object[] script, String scriptIndex) : base(name, new Vector2(), null, 0)
            {
                ScriptThrowTarget = "#MAINMENU";
                _scriptName = scriptIndex;
                Shell.GlobalWorldState = "SCRIPTSNIFFER_FIRST_INITIALIZED";
                AllowScriptShift = true;
                _myScript = script;
                InitScriptShift();
            }
            public ScriptSniffer(String name, object[] script, String scriptIndex, Boolean loading) : base(name, new Vector2(), null, 0)
            {
                ScriptThrowTarget = "#MAINMENU";
                _scriptName = scriptIndex;
                _loadMode = loading;
                Shell.GlobalWorldState = "SCRIPTSNIFFER_FIRST_INITIALIZED";
                AllowScriptShift = true;
                _myScript = script;
                InitScriptShift();
            }
            private String _scriptName = "";
            public void ReassignScript()
            {
                _myScript = RetrieveScriptByName(_scriptName);
            }
            public static void GlobalCeaseSkipping()
            {
                foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                {
                    if (worldEntity is ScriptProcessor.ScriptSniffer) { ((ScriptProcessor.ScriptSniffer)worldEntity).CeaseSkipping(); }
                }
            }
            public Boolean Skipping { get { return _skipAll; } }
            public void CeaseSkipping()
            {
                _skipAll = false;
            }
            public override void OnDeserializeDo()
            {
                base.OnDeserializeDo();
                _skipAll = false;
                ReassignScript();
                ShiftCondition = _localShiftCondition;
                _shiftStartTime = Environment.TickCount;
                lastTime = Environment.TickCount;
                CountApplicableRollbacks = RollbacksOnReturn;
                AllowScriptShift = true;
                /*
                 * This code was meant to re-write the current Script Shift text to the dialogue archive on load, I think.
                foreach (object O in (object[])_myScript[ScriptIndex])
                {
                    if (O is String)
                    {
                        String[] TS = ((String)O).Split('|');
                        if(TS[0].ToUpper() == "T")
                        {
                            WriteArchive(TS[2], TS[1]);
                        }
                    }
                }*/
            }
            Boolean lastCheck = false;
            int lastTime = Environment.TickCount;
            public void Skip()
            {
                _skipAll = true;
            }
            public void SetLoadBreaker(int Index)
            {
                _loadBreaker = Index;
            }
            private int _loadBreaker = -1;
            private Boolean _loadInit = true;
            private Queue _forceScriptInsertionQueue = new Queue();
            public void ForceInsertScriptElement(object[] OneScriptShift, Boolean ClearSCs)
            {
                if(_forceScriptInsertionQueue is null) { _forceScriptInsertionQueue = new Queue(); }
                _forceScriptInsertionQueue.Enqueue(OneScriptShift);
                PopForceInsertion(ClearSCs);
            }
            public void ForceInsertMultipleScriptElements(object[] multipleScriptShifts)
            {
                if (_forceScriptInsertionQueue is null) { _forceScriptInsertionQueue = new Queue(); }
                foreach (object[] oneScriptShift in multipleScriptShifts)
                {
                    _forceScriptInsertionQueue.Enqueue(oneScriptShift);
                }
                PopForceInsertion(true);
            }
            public void PopForceInsertion(Boolean clearShiftCondition)
            {
                if (_forceScriptInsertionQueue == null || _forceScriptInsertionQueue.Count == 0)
                {
                    throw new ScriptParseException("Error during forced script element insertion: The forced insertion queue was empty or null.");
                }
                object[] oneScriptShift = (object[])_forceScriptInsertionQueue.Dequeue();
                Shell.GlobalWorldState = "LOADED FORCED SHIFT...";
                Shell.WriteLine("Force inserting script element shift.");
                _shiftStartTime = Environment.TickCount;
                if (clearShiftCondition) { ShiftCondition = new String[0]; }
                if (_scriptIndex >= _myScript.Length - 1 && _forceScriptInsertionQueue.Count == 0) { _scriptIndex--; }
                PushScriptShift(oneScriptShift, false);
                lastTime = Environment.TickCount;
            }
            public override void Update()
            {
                base.Update();
                if(ButtonScripts.Paused)
                {
                    if(!PlacedInPauseState)
                    {
                        TimeHang();
                        PlacedInPauseState = true;
                    }
                }
                else if (PlacedInPauseState)
                {
                    UnHang();
                    PlacedInPauseState = false;
                }
                if(_localShiftCondition != ShiftCondition) { _localShiftCondition = ShiftCondition; }
                if (!_loadMode)
                {
                    if (Shell.DeleteQueue.Contains(this)) { return; }
                    KeyboardState keyboard = Keyboard.GetState();
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Tab) && !lastCheck) { _skipAll = !_skipAll; }
                    lastCheck = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Tab);
                    if (CheckForShiftCondition() && (Environment.TickCount - lastTime > 60 || Name == "INTRO_SNIFFER_UNIQUE") && AllowScriptShift)
                    {
                        if ((Shell.GlobalWorldState == "CONTINUE" || Shell.GlobalWorldState == "NEXT") && Name != "INTRO_SNIFFER_UNIQUE") { Shell.GlobalWorldState = "LOADED NEXT SHIFT..."; }
                        if(_skipAll)
                        {
                            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                            {
                                foreach (Animation entityAnimation in worldEntity.AnimationQueue)
                                {
                                    entityAnimation.Jump(worldEntity);
                                }
                            }
                        }
                        if (_forceScriptInsertionQueue != null && _forceScriptInsertionQueue.Count > 0)
                        {
                            PopForceInsertion(true);
                        }
                        else
                        {
                            _shiftStartTime = Environment.TickCount;
                            ShiftCondition = new String[0];
                            _scriptIndex++;
                            InitScriptShift();
                            lastTime = Environment.TickCount;
                        }
                    }
                }
                else if (_loadBreaker > -1)
                {
                    if(_loadInit)
                    {
                        Shell.DefaultShell.IsFixedTimeStep = false;
                        _loadInit = false;
                        return;
                    }
                    if (_scriptIndex < _loadBreaker)
                    {
                        foreach (WorldEntity E in Shell.UpdateQueue)
                        {
                            foreach (Animation A in E.AnimationQueue)
                            {
                                A.Jump(E);
                            }
                        }
                        _shiftStartTime = Environment.TickCount;
                        ShiftCondition = new String[0];
                        _scriptIndex++;
                        InitScriptShift();
                        lastTime = Environment.TickCount;
                    }
                    else
                    {
                        Shell.GlobalWorldState = "LOADED SHIFT VIA LOADMODE...";
                        Shell.HoldRender = false;
                        Shell.DefaultShell.IsFixedTimeStep = true;
                        _loadMode = false;
                    }
                }
            }
            int PushScriptShift(object[] currentShift, Boolean expectSerialization)
            {
                int prevApplicableRollbacks = CountApplicableRollbacks;
                if (expectSerialization)
                {
                    s_applicableRollbackArchive.Push(CountApplicableRollbacks);
                    CountApplicableRollbacks = 0;
                }
                foreach (object obj in currentShift)
                {
                    int returnCode = ActivateScriptElement(obj, _skipAll);
                    if(returnCode == 2) { break; }
                    if (_skipAll && obj is string && ((string)obj).Split('|')[0].ToUpper() == "T")
                    {
                        foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                        {
                            if (worldEntity is TextEntity && worldEntity.Name == "TEXT_MAIN")
                            {
                                ((TextEntity)worldEntity).SkipWrite();
                                TextEntity.PlayTick();
                                break;
                            }
                        }
                    }
                }
                _localShiftCondition = ShiftCondition;
                if (expectSerialization)
                {
                    foreach (String conditionString in ShiftCondition)
                    {
                        String[] conditions = conditionString.ToUpper().Split(':');
                        if (conditions[0] == "TIME" && Convert.ToInt32(conditions[1]) <= 20)
                        {
                            s_applicableRollbackArchive.Pop();
                            CountApplicableRollbacks = CountApplicableRollbacks + prevApplicableRollbacks;
                            break;
                        }
                    }
                }
                return CountApplicableRollbacks;
            }
            public int RollbacksOnReturn { get; set; }
            void InitScriptShift()
            {
                RollbacksOnReturn = 0;
                Shell.WriteLine("Initiating script shift.");
                if(_scriptIndex >= _myScript.Length)
                {
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        ButtonScripts.BackToMainMenu();
                    }));
                    return;
                }
                PushScriptShift((object[])_myScript[_scriptIndex], true);
                Boolean rollbackAble = true;
                foreach (String conditionString in ShiftCondition)
                {
                    String[] conditions = conditionString.ToUpper().Split(':');
                    if (conditions[0] == "TIME" && Convert.ToInt32(conditions[1]) <= 20)
                    {
                        rollbackAble = false;
                        break;
                    }
                }
                if (!Shell.DeleteQueue.Contains(this) && ShiftCondition.Length > 0)
                {
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Shell.GlobalVoid = new VoidDel(delegate ()
                        {
                            foreach (WorldEntity E in Shell.UpdateQueue)
                            {
                                if (E is ScriptSniffer) { ((ScriptSniffer)E).RollbacksOnReturn = CountApplicableRollbacks; }
                            }
                            if (rollbackAble) { PastStates.Push(Shell.SerializeState()); }
                        });
                    }));
                }
                if (PastStates.Count > 200)
                {
                    Stack tempStack = new Stack();
                    for (int i = 0; i < 200; i++) { tempStack.Push(PastStates.Pop()); }
                    PastStates.Clear();
                    for (int i = 0; i < 200; i++) { PastStates.Push(tempStack.Pop()); }
                    tempStack.Clear();
                }
            }
            Boolean CheckForShiftCondition()
            {
                Boolean outBool = true;
                foreach(String conditionString in ShiftCondition)
                {
                    String[] conditions = conditionString.ToUpper().Split(':');
                    switch(conditions[0])
                    {
                        case "TIME":
                            if(_skipAll) { return true; }
                            if(!(ShiftTimeElapsed() >= Convert.ToInt32(conditions[1])))
                            {
                                outBool = false;
                            }
                            if(conditions.Length > 2 && conditions[2] == "ORSKIP" && Shell.GlobalWorldState == "CONTINUE")
                            {
                                outBool = true;
                            }
                            break;
                        case "GWS":
                            if(_skipAll && conditions[1] == "CONTINUE") { return true; }
                            else if(_skipAll && conditions[1] != "CONTINUE") { _skipAll = false; }
                            if(Shell.GlobalWorldState != conditions[1])
                            {
                                outBool = false;
                            }
                            break;
                    }
                    if(outBool == false) { break; }
                }
                return outBool;
            }
        }
        /*
         * T|| specifies text to write, and the character caption if applicable.
         * C| specifies conditions to move to the next script shift.
         * B| breaks with the current script, and starts a new script if applicable. |#MAINMENU to return to the main menu.
         * D| deletes the entity with the specified entity name, all entities via |#ALL, or all custom buttons via |#CBUTTONS. ||IFPRESENT causes the function not to throw an error for a missing object.
         * A|| plays an animation on a named entity by animation name. #DISMISS clears the entity's animation queue.
         * A|||||| plays an animation on a named entity by defined tween parameters.
         * F||| sets the atlas frame of a specified entity to the given coordinates, if possible.
         * F|| sets the atlas frame of a specified entity to the given named frame state, if possible.
         * G| sets the GlobalWorldState.
         * U|| updates a given game flag to the specified value.
         * R||| reads a game flag and then performs the following command if it matches a specified value, or if an optional comparison operator is true.
         * H halts script skipping upon occurrence.
         * S|| plays a named sound effect, or stops all sound effects via |#CLOSEALL. Second parameter sets looping.
         * M||| switches the music track to a named song, or stops the song via |#NULL|. Second parameter sets looping. Third if set to "INSTANT" skips the auto fadeout.
        */
        public static String LabelEntity = "";
        static public void SetFocus(String label)
        {
            if(label.ToUpper() == LabelEntity) { return; }
            Boolean foundNew = false;
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.Name == label.ToUpper() && LabelEntity != label.ToUpper())
                {
                    foundNew = true;
                    foreach (WorldEntity worldEntity2 in Shell.UpdateQueue)
                    {
                        if (worldEntity2.Name == LabelEntity)
                        {
                            Boolean[] scaleChecks = worldEntity2.CheckScaleInversions();
                            Animation focusShrink = Animation.Retrieve("FOCUSSHRINK");
                            if(scaleChecks[0] || scaleChecks[1]) { focusShrink.AutoInvertScaling(scaleChecks[0], scaleChecks[1]); }
                            worldEntity2.AnimationQueue.Add(focusShrink);
                            break;
                        }
                    }
                    LabelEntity = label.ToUpper();
                    Boolean[] scaleChecks2 = worldEntity.CheckScaleInversions();
                    Animation focusGrow = Animation.Retrieve("FOCUSGROW");
                    if (scaleChecks2[0] || scaleChecks2[1]) { focusGrow.AutoInvertScaling(scaleChecks2[0], scaleChecks2[1]); }
                    worldEntity.AnimationQueue.Add(focusGrow);
                    break;
                }
            }
            if (!foundNew && LabelEntity != label.ToUpper())
            {
                foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                {
                    if (worldEntity.Name == LabelEntity)
                    {
                        Boolean[] scaleChecks = worldEntity.CheckScaleInversions();
                        Animation focusShrink = Animation.Retrieve("FOCUSSHRINK");
                        if (scaleChecks[0] || scaleChecks[1]) { focusShrink.AutoInvertScaling(scaleChecks[0], scaleChecks[1]); }
                        worldEntity.AnimationQueue.Add(focusShrink);
                        break;
                    }
                }
                LabelEntity = "";
            }
        }
        static public int ActivateScriptElement(object element)
        {
            return ActivateScriptElement(element, false);
        }
        static public int ActivateScriptElement(object element, Boolean snifferSkipping)
        {
            if (element is String)
            {
                if (!(("activate " + ((String)element)).ToUpper() == Shell.LastManualConsoleInput.ToUpper()))
                {
                    SortedDictionary<int, Color> colours = new SortedDictionary<int, Color>();
                    colours.Add(0, Color.LightBlue);
                    Shell.WriteLine((String)element, colours);
                }
                String elementStr = (String)element;
                if(elementStr.StartsWith("FACTORY"))
                {
                    elementStr = VNFUtils.Strings.ReplaceExclosed(elementStr, "{{", "^", '\"');
                    elementStr = VNFUtils.Strings.ReplaceExclosed(elementStr, "}}", "^", '\"');
                    RunFactoryCommand(elementStr, 1, true);
                }
                else if(elementStr.StartsWith("RUN"))
                {
                    elementStr = VNFUtils.Strings.ReplaceExclosed(elementStr, "{{", "^", '\"');
                    elementStr = VNFUtils.Strings.ReplaceExclosed(elementStr, "}}", "^", '\"');
                    RunFactoryCommand(elementStr, 2, true);
                }
                String[] parts = elementStr.Split('|');
                if (parts[0].ToUpper() == "T")
                {
                    T(parts, snifferSkipping);
                }
                else if (parts[0].ToUpper() == "C")
                {
                    C(parts);
                }
                else if (parts[0].ToUpper() == "B")
                {
                    B(parts);
                    return 2;
                }
                else if (parts[0].ToUpper() == "D")
                {
                    D(parts);
                }
                else if (parts[0].ToUpper() == "A")
                {
                    A(parts);
                }
                else if (parts[0].ToUpper() == "F")
                {
                    F(parts);
                }
                else if (parts[0].ToUpper() == "S")
                {
                    S(parts);
                }
                else if (parts[0].ToUpper() == "M")
                {
                    M(parts, elementStr);
                }
                else if (parts[0].ToUpper() == "H")
                {
                    ScriptSniffer.GlobalCeaseSkipping();
                }
                else if (parts[0].ToUpper() == "G")
                {
                    Shell.GlobalWorldState = parts[1];
                }
                else if (parts[0].ToUpper() == "U")
                {
                    U(parts);
                }
                else if (parts[0].ToUpper() == "R")
                {
                    return R(parts);
                }
            }
            else if (element is VoidDel)
            {
                Shell.WriteLine("Executing anonymous method from script.");
                VoidDel elementDelegate = (VoidDel)element;
                elementDelegate();
            }
            return 1;
        }
        /// <summary>
        /// T|| specifies text to write, and the character caption if applicable.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="snifferSkipping"></param>
        private static void T(String[] parts, Boolean snifferSkipping)
        {
            //Label change text bounce...
            TextEntity textLabel = null;
            WorldEntity nameBacking = null;
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.Name == "TEXT_LABEL" && worldEntity is TextEntity)
                {
                    textLabel = (TextEntity)worldEntity;
                    if (((TextEntity)worldEntity).Text.Remove(0, ((TextEntity)worldEntity).Text.LastIndexOf(']') + 1) != parts[1])
                    {
                        if (!snifferSkipping) { worldEntity.AnimationQueue.Add(Animation.Retrieve("BOUNCE_3")); }
                        SetFocus(parts[1]);
                    }
                }
                if (worldEntity.Name == "NAMELABELBACKING")
                {
                    nameBacking = worldEntity;
                }
                if (textLabel != null && nameBacking != null) { break; }
            }
            WriteArchive(parts[2], parts[1]);
            TextEntity main = new TextEntity("", "", new Vector2(), 0);
            Boolean found = false;
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.Name == "TEXT_MAIN" && worldEntity is TextEntity)
                {
                    main = (TextEntity)worldEntity;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                main = new TextEntity("TEXT_MAIN", parts[2], new Vector2(150, 500), 0.96f);
                main.TypeWrite = true;
                main.IsUIElement = true;
                if(ButtonScripts.UIHideEnabled) { main.Drawable = false; }
                Shell.RunQueue.Add(new VoidDel(delegate ()
                {
                    Shell.UpdateQueue.Add(main);
                    Shell.RenderQueue.Add(main);
                }));
            }
            main.Text = parts[2];
            main.ReWrite();
            if (parts[1] != "")
            {
                TextEntity label = textLabel;
                if (textLabel is null)
                {
                    label = new TextEntity("TEXT_LABEL", parts[1], new Vector2(140, 420), 0.96f);
                    label.IsUIElement = true;
                    if (!snifferSkipping) { label.AnimationQueue.Add(Animation.Retrieve("BOUNCE_3")); }
                    SetFocus(parts[1]);
                    if (ButtonScripts.UIHideEnabled) { label.Drawable = false; }
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Shell.UpdateQueue.Add(label);
                        Shell.RenderQueue.Add(label);
                    }));
                }
                if (nameBacking is null)
                {
                    nameBacking = new WorldEntity("NAMELABELBACKING", new Vector2(90, 388), (TAtlasInfo)Shell.AtlasDirectory["NAMEBACKING"], 0.9f);
                    nameBacking.ColourValue = new Color(255, 255, 255, 200);
                    nameBacking.IsUIElement = true;
                    if (ButtonScripts.UIHideEnabled) { nameBacking.Drawable = false; }
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Shell.UpdateQueue.Add(nameBacking);
                        Shell.RenderQueue.Add(nameBacking);
                    }));
                }
                label.Text = "[C:PURPLE]" + parts[1];
            }
            else
            {
                if (textLabel != null)
                {
                    Shell.DeleteQueue.Add(textLabel);
                    textLabel.Text = "";
                }
                if (nameBacking != null)
                {
                    Shell.DeleteQueue.Add(nameBacking);
                }
            }
        }
        public static object ParseLiteralValue(String input)
        {
            float f;
            if (float.TryParse(input.TrimEnd(new char[] { 'f', 'd' }), out f))
            {
                if (input.Contains("f") || (input.Contains(".") && !input.Contains("d"))) { return float.Parse(input.Replace("f", "")); }
                else if (input.Contains("d")) { return double.Parse(input.Replace("d", "")); }
                else { return int.Parse(input); }
            }
            else if (input.ToUpper() == "TRUE" || input.ToUpper() == "FALSE")
            {
                return input.ToUpper() == "TRUE";
            }
            else if (input.ToUpper() == "NULL")
            {
                return null;
            }
            else { return input; }
        }
        /// <summary>
        /// U|| updates a given game flag to the specified value.
        /// </summary>
        /// <param name="parts"></param>
        private static void U(String[] parts)
        {
            String flagName = parts[1].ToUpper();
            String textFlagVal = parts[2];
            object trueVal = ParseLiteralValue(textFlagVal);
            Shell.UpdateFlag(flagName, trueVal);
        }
        /// <summary>
        /// R||| reads a game flag and then performs the following command if it matches a specified value, or if an optional comparison operator is true.
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        private static int R(String[] parts)
        {
            String flagName = parts[1].ToUpper();
            String textFlagComparisonVal = parts[2];
            String mode = "=";
            if (textFlagComparisonVal.Contains(":"))
            {
                String[] modeSplit = textFlagComparisonVal.Split(':');
                textFlagComparisonVal = modeSplit[1];
                mode = modeSplit[0];
            }
            object trueComparisonVal = ParseLiteralValue(textFlagComparisonVal);
            Boolean activateCond = false;
            switch (mode)
            {
                case "=":
                    activateCond = Shell.ReadFlag(flagName).Equals(trueComparisonVal);
                    break;
                case "!=":
                    activateCond = !Shell.ReadFlag(flagName).Equals(trueComparisonVal);
                    break;
                case ">":
                    activateCond = Convert.ToDecimal(Shell.ReadFlag(flagName)) > Convert.ToDecimal(trueComparisonVal);
                    break;
                case ">=":
                    activateCond = Convert.ToDecimal(Shell.ReadFlag(flagName)) >= Convert.ToDecimal(trueComparisonVal);
                    break;
                case "<":
                    activateCond = Convert.ToDecimal(Shell.ReadFlag(flagName)) < Convert.ToDecimal(trueComparisonVal);
                    break;
                case "<=":
                    activateCond = Convert.ToDecimal(Shell.ReadFlag(flagName)) <= Convert.ToDecimal(trueComparisonVal);
                    break;
            }
            if (activateCond && parts.Length > 3)
            {
                String conditionalCom = "";
                for (int i = 3; i < parts.Length; i++)
                {
                    conditionalCom += parts[i] + "|";
                }
                conditionalCom = conditionalCom.TrimEnd('|');
                return ActivateScriptElement(conditionalCom);
            }
            return 1;
        }
        /// <summary>
        /// C| specifies conditions to move to the next script shift.
        /// </summary>
        /// <param name="parts"></param>
        private static void C(String[] parts)
        {
            ArrayList sConditions = new ArrayList();
            Boolean first = true;
            foreach (String str in parts)
            {
                if (!first) { sConditions.Add(str); }
                first = false;
            }
            ScriptSniffer.ShiftCondition = sConditions.ToArray().Select(x => (String)x).ToArray();
        }
        /// <summary>
        /// B| breaks with the current script, and starts a new script if applicable. |#MAINMENU to return to the main menu.
        /// </summary>
        /// <param name="parts"></param>
        private static void B(String[] parts)
        {
            Boolean skipping = false;
            for (int i = 0; i < Shell.UpdateQueue.Count; i++)
            {
                if (Shell.UpdateQueue[i] is ScriptSniffer)
                {
                    if(((ScriptSniffer)Shell.UpdateQueue[i]).Skipping) { skipping = true; }
                    Shell.DeleteQueue.Add(Shell.UpdateQueue[i]);
                    break;
                }
            }
            if (parts.Length > 1)
            {
                if (parts[1] != "")
                {
                    if (parts[1].ToUpper() == "#MAINMENU")
                    {
                        Shell.RunQueue.Add(new VoidDel(delegate ()
                        {
                            ButtonScripts.BackToMainMenu();
                        }));
                    }
                    else if (parts[1].ToUpper() == "#SCRIPTTHROWTARGET")
                    {
                        Boolean found = false;
                        foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                        {
                            if (worldEntity is ScriptSniffer)
                            {
                                found = true;
                                ActivateScriptElement("B|" + ((ScriptSniffer)worldEntity).ScriptThrowTarget);
                                break;
                            }
                        }
                        if (!found)
                        {
                            throw (new ScriptParseException("Script parsing error during script throw target activation; unable to find an active ScriptSniffer."));
                        }
                    }
                    else
                    {
                        ScriptSniffer newScriptSniffer = new ScriptSniffer(parts[1].ToUpper() + "_SNIFFER", RetrieveScriptByName(parts[1]), parts[1]);
                        if(skipping) { newScriptSniffer.Skip(); }
                        Shell.RunQueue.Add(new VoidDel(delegate ()
                        {
                            Shell.UpdateQueue.Add(newScriptSniffer);
                        }));
                    }
                }
            }
        }
        /// <summary>
        /// D| deletes the entity with the specified entity name, all entities via |#ALL, or all custom buttons via |#CBUTTONS. ||IFPRESENT causes the function not to throw an error for a missing object.
        /// </summary>
        /// <param name="parts"></param>
        private static void D(String[] parts)
        {
            Boolean customButtonMode = false;
            Boolean found = false;
            Boolean all = false;
            if (parts[1].ToUpper() == "#CBUTTONS")
            {
                customButtonMode = true;
                found = true;
            }
            if (parts[1].ToUpper() == "#ALL")
            {
                all = true;
                found = true;
            }
            List<WorldEntity> companionRemoves = new List<WorldEntity>();
            for (int i = 0; i < Shell.UpdateQueue.Count; i++)
            {
                if ((all && !((Shell.UpdateQueue[i]) is ScriptSniffer)) || (!customButtonMode && (Shell.UpdateQueue[i]).Name.ToUpper() == parts[1]) || (customButtonMode && (Shell.UpdateQueue[i]).Name.ToUpper().Contains("BUTTON_CUSTOM_")))
                {
                    Shell.DeleteQueue.Add(Shell.UpdateQueue[i]);
                    if (Shell.UpdateQueue[i] is DropMenu) { companionRemoves.Add(Shell.UpdateQueue[i]); }
                    if (!customButtonMode && !all)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if(companionRemoves.Count > 0)
            {
                foreach(WorldEntity worldEntity in companionRemoves)
                {
                    if (worldEntity is DropMenu)
                    {
                        Shell.RunQueue.Add(new VoidDel(delegate () { ((DropMenu)worldEntity).DepopulateDropList(); }));
                    }
                }
            }
            if (!found && (parts.Length < 3 || parts[2].ToUpper() != "IFPRESENT"))
            {
                throw (new ScriptParseException("Entity delete command could not locate the specified entity: " + parts[1]));
            }
        }
        /// <summary>
        /// A|| plays an animation on a named entity by animation name. #DISMISS clears the entity's animation queue.
        /// A|||||| plays an animation on a named entity by defined tween parameters.
        /// </summary>
        /// <param name="parts"></param>
        private static void A(String[]parts)
        {
            List<WorldEntity> addAnimatees = new List<WorldEntity>();
            if(parts[1].ToUpper() == "#ALL")
            {
                foreach (WorldEntity worldEntity in Shell.UpdateQueue) { if (!(worldEntity is ScriptSniffer) && !(worldEntity is Camera)) { addAnimatees.Add(worldEntity); } }
            }
            else if(parts[1].ToUpper() == "#ALL-NON-UI")
            {
                foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                {
                    if (!(worldEntity is ScriptSniffer) && !(worldEntity is Camera) && !ButtonScripts.DefaultUINames.Contains(worldEntity.Name) && !(worldEntity is TextEntity) && !(worldEntity.Name == "WHITE-SHEET")) { addAnimatees.Add(worldEntity); }
                }
            }
            else if (parts[1].ToUpper() == "#ALL-NON-UI-SOFIA")
            {
                foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                {
                    if (!(worldEntity is ScriptSniffer) && !(worldEntity is Camera) && !ButtonScripts.DefaultUINames.Contains(worldEntity.Name) && !(worldEntity is TextEntity) && !(worldEntity is Sofia.BigSofia) && !(worldEntity.Name == "WHITE-SHEET")) { addAnimatees.Add(worldEntity); }
                }
            }
            else if (parts[1].ToUpper() == "#GLOBAL_END_LOOPS")
            {
                Animation.GlobalEndLoops();
            }
            else if (parts[1].ToUpper() == "#GLOBAL_MANUAL_TRIGGER")
            {
                Animation.GlobalManualTrigger(parts[2]);
            }
            else { addAnimatees.Add(Shell.GetEntityByName(parts[1])); }
            WorldEntity[] animatees = addAnimatees.ToArray().Select(x => (WorldEntity)x).ToArray();
            if (animatees.Length > 0 && animatees[0] != null)
            {
                if (parts.Length == 3 || parts.Length == 4)
                {
                    if (parts[2] == "#DISMISS")
                    {
                        for (int i = 0; i < animatees.Length; i++)
                        {
                            WorldEntity animatedEntity = animatees[i];
                            animatedEntity.AnimationQueue.Clear();
                        }
                    }
                    else
                    {
                        Animation animationRetrieved = Animation.Retrieve(parts[2]);
                        if (parts.Length == 4 && animationRetrieved != null)
                        {
                            if (parts[3].ToUpper() == "LOOP" || parts[3].ToUpper() == "TRUE") { animationRetrieved.Loop = true; }
                            else { animationRetrieved.Loop = false; }
                        }
                        for (int i = 0; i < animatees.Length; i++)
                        {
                            WorldEntity animatedEntity = animatees[i];
                            if (animationRetrieved.AnimName.ToUpper() != "NULL") { animatedEntity.AnimationQueue.Add(animationRetrieved.Clone()); }
                        }
                        animationRetrieved.AutoWipe();
                    }
                }
                else if (parts.Length == 7)
                {
                    //Parameters are seperated by commas, parameters within parameters are seperated by "="
                    //A looped vector tween: A|[entityname]|50=50,1000,20||||loop
                    Animation newAnimation = new Animation(animatees[0].Name + "_animation_scriptdefined");
                    if (parts[2].Length > 0)
                    {
                        String[] movementTrack = parts[2].Split(',');
                        String[] vectorMotionTerms = movementTrack[0].Split('=');
                        Vector2 motionTween = new Vector2((float)Convert.ToDouble(vectorMotionTerms[0]), (float)Convert.ToDouble(vectorMotionTerms[1]));
                        newAnimation.WriteMovement(Animation.CreateVectorTween(motionTween, Convert.ToInt32(movementTrack[1]), Convert.ToInt32(movementTrack[2])));
                    }
                    if (parts[3].Length > 0)
                    {
                        String[] movementTrack = parts[3].Split(',');
                        float rotationTween = (float)Convert.ToDouble(movementTrack[0]);
                        newAnimation.WriteRotation(Animation.CreateFloatTween(rotationTween, Convert.ToInt32(movementTrack[1]), Convert.ToInt32(movementTrack[2])));
                    }
                    if (parts[4].Length > 0)
                    {
                        String[] movementTrack = parts[4].Split(',');
                        String[] scalingTerms = movementTrack[0].Split('=');
                        Vector2 scaleTween = new Vector2((float)Convert.ToDouble(scalingTerms[0]), (float)Convert.ToDouble(scalingTerms[1]));
                        newAnimation.WriteScaling(Animation.CreateVectorTween(scaleTween, Convert.ToInt32(movementTrack[1]), Convert.ToInt32(movementTrack[2])));
                    }
                    if (parts[5].Length > 0)
                    {
                        String[] movementTrack = parts[5].Split(',');
                        String[] colourTerms = movementTrack[0].Split('=');
                        ColourShift colourTween = new ColourShift(Convert.ToInt32(colourTerms[0]), Convert.ToInt32(colourTerms[1]), Convert.ToInt32(colourTerms[2]), Convert.ToInt32(colourTerms[3]));
                        newAnimation.WriteColouring(Animation.CreateColourTween(colourTween, Convert.ToInt32(movementTrack[1]), Convert.ToInt32(movementTrack[2])));
                    }
                    if (parts[6].Length > 0 && parts[6].ToUpper() == "LOOP" || parts[6].ToUpper() == "TRUE") { newAnimation.Loop = true; }
                    for (int i = 0; i < animatees.Length; i++)
                    {
                        WorldEntity animatedEntity = animatees[i];
                        animatedEntity.AnimationQueue.Add(newAnimation.Clone());
                    }
                    newAnimation.AutoWipe();
                }
                else { throw (new ScriptParseException("The animation command does not take the specified number of parameters: " + (parts.Length - 1))); }
            }
            else if (animatees.Length > 0) { throw (new ScriptParseException("Entity animation command could not locate the specified entity: " + parts[1])); }
        }
        /// <summary>
        /// F||| sets the atlas frame of a specified entity to the given coordinates, if possible.
        /// F|| sets the atlas frame of a specified entity to the given named frame state, if possible.
        /// </summary>
        /// <param name="parts"></param>
        private static void F(String[] parts)
        {
            WorldEntity frameEntity = Shell.GetEntityByName(parts[1]);
            if (frameEntity != null)
            {
                if (parts.Length == 3)
                {
                    Hashtable lookup = frameEntity.Atlas.FrameLookup;
                    if (lookup.ContainsKey(parts[2].ToUpper()))
                    {
                        Point frame = (Point)lookup[parts[2].ToUpper()];
                        frameEntity.SetAtlasFrame(frame);
                    }
                }
                else if (parts.Length == 4)
                {
                    Point frame = new Point(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[3]));
                    frameEntity.SetAtlasFrame(frame);
                }
                else { throw (new ScriptParseException("The atlas frame shift command does not take the specified number of parameters: " + (parts.Length - 1))); }
            }
            else { throw (new ScriptParseException("Atlas frame shift command could not locate the specified entity: " + parts[1])); }
        }
        /// <summary>
        /// S|| plays a named sound effect, or stops all sound effects via |#CLOSEALL. Second parameter sets looping.
        /// </summary>
        /// <param name="parts"></param>
        private static void S(String[] parts)
        {
            if (parts[1].ToUpper() != "#CLOSEALL")
            {
                if (!Shell.Mute)
                {
                    SoundEffectInstance localSound = (Shell.SFXDirectory[parts[1]]).CreateInstance();
                    localSound.Volume = Shell.GlobalVolume;
                    if (parts.Length > 2 && parts[2].ToUpper() != "")
                    {
                        if (parts[2].ToUpper() == "TRUE")
                        {
                            localSound.IsLooped = true;
                        }
                        else if (parts[2].ToUpper() == "FALSE")
                        {
                            localSound.IsLooped = false;
                        }
                    }
                    localSound.Play();
                    Shell.ActiveSounds.Add(localSound);
                }
            }
            else
            {
                foreach (SoundEffectInstance soundEffectInst in Shell.ActiveSounds)
                {
                    soundEffectInst.Stop();
                }
            }
        }
        /// <summary>
        /// M||| switches the music track to a named song, or stops the song via |#NULL|. Second parameter sets looping. Third if set to "INSTANT" skips the auto fadeout.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="MCommand"></param>
        private static void M(String[] parts, String MCommand)
        {
            SongCom = MCommand;
            Boolean instant = false;
            if (parts.Length > 3 && parts[3].ToUpper() != "")
            {
                if (parts[3].ToUpper() == "INSTANT")
                {
                    instant = true;
                }
            }
            if (parts[1].ToUpper() != "#NULL")
            {
                Song localSong = ((Song)Shell.SongDirectory[parts[1]]);
                if (instant) { Shell.QueueInstantTrack(localSong, 1f); }
                else { Shell.QueueInstantTrack(localSong); }
            }
            else
            {
                if (instant) { Shell.OneFadeout(1f); }
                else { Shell.OneFadeout(); }
            }
            if (parts.Length > 2 && parts[2].ToUpper() != "")
            {
                if (parts[2].ToUpper() == "TRUE")
                {
                    MediaPlayer.IsRepeating = true;
                }
                else if (parts[2].ToUpper() == "FALSE")
                {
                    MediaPlayer.IsRepeating = false;
                }
            }
        }
    }
}
