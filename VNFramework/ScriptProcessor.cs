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
using System.Collections;
using System.Text.RegularExpressions;

namespace VNFramework
{
    public static partial class ScriptProcessor
    {
        public static int CountApplicableRollbacks { get; set; }
        public static Boolean AllowScriptShift { get; set; }
        public static Stack PastStates = new Stack();
        public static String SongCom = "";
        static String TextArchive = "";
        public static Boolean AllowScriptExit { get; set; }
        public static Boolean ActiveGame()
        {
            foreach(WorldEntity E in Shell.UpdateQueue)
            {
                if(E is ScriptSniffer && E.Name != "INTRO_SNIFFER_UNIQUE") { return true; }
            }
            return false;
        }
        public static ScriptSniffer SnifferSearch()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is ScriptSniffer)
                {
                    return (ScriptSniffer)E;
                }
            }
            return null;
        }
        public static void SearchAndThrowForExit()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is ScriptSniffer)
                {
                    ((ScriptSniffer)E).ThrowForInstantExit();
                    break;
                }
            }
        }
        public static void WriteArchive(String Text, String Label)
        {
            if (Text != "" || Label != "")
            {
                CountApplicableRollbacks++;
                if (TextArchive != "") { TextArchive += "[TADM][N][N]"; }
                if (Label != "") { TextArchive += ("[C:PURPLE]" + Label + "[N]"); }
                TextArchive += Text;
            }
            Shell.WriteLine(Label + ": " + Text);
        }
        public static void RollbackArchive()
        {
            Shell.WriteLine("Received archive rollback request. Wiping " + CountApplicableRollbacks + " line(s).");
            for (int i = CountApplicableRollbacks; i > 0; i--)
            {
                if (TextArchive.Contains("[TADM]"))
                {
                    TextArchive = TextArchive.Remove(TextArchive.LastIndexOf("[TADM]"));
                }
                else { TextArchive = ""; }
            }
        }
        public static void WipeArchive()
        {
            TextArchive = "";
            Shell.WriteLine("Dialogue archive wiped.");
        }
        public static Texture2D[] PullArchive()
        {
            return ButtonScripts.CreateDynamicScroll(TextArchive, 1000);
        }
        public static void ClearNonUIEntities()
        {
            foreach(WorldEntity W in Shell.UpdateQueue)
            {
                if(!Shell.DeleteQueue.Contains(W) && !(W is ScriptSniffer || W.Name == "UIBOX" || W.Name == "BLACK" || W.Name == "BUTTON_ARCHIVE" || W.Name == "BUTTON_SKIP" || W.Name == "BUTTON_PAUSEMENU" || W.Name == "BUTTON_ROLLBACK" || W.Name == "BUTTON_NAVSCREEN" || W.Name == "BUTTON_HIDE_UI"))
                {
                    Shell.DeleteQueue.Add(W);
                }
            }
            foreach (WorldEntity W in Shell.RenderQueue)
            {
                if (!Shell.DeleteQueue.Contains(W) && !(W is ScriptSniffer || W.Name == "UIBOX" || W.Name == "BLACK" || W.Name == "BUTTON_ARCHIVE" || W.Name == "BUTTON_SKIP" || W.Name == "BUTTON_PAUSEMENU" || W.Name == "BUTTON_ROLLBACK" || W.Name == "BUTTON_NAVSCREEN" || W.Name == "BUTTON_HIDE_UI"))
                {
                    Shell.DeleteQueue.Add(W);
                }
            }
        }
        public static Hashtable ExtractEventScriptArchive(String ScriptArchiveContent)
        {
            String SCA = String.Join("\n", ScriptArchiveContent.Split('\n').Select(x => ((String)x).StartsWith("//") ? "" : x));
            SCA = VNFUtils.Strings.ReplaceExclosed(SCA, "{{", ">", '\"');
            SCA = VNFUtils.Strings.ReplaceExclosed(SCA, "}}", ">", '\"');
            SCA = VNFUtils.Strings.RemoveExclosed(SCA, '\n', '>');
            SCA = SCA.Replace("\r", "");
            ArrayList IndivScripts = new ArrayList();
            int NextStart = VNFUtils.Strings.IndexOfExclosed(SCA, "declare_script", '\"');
            SCA = SCA.Remove(0, NextStart + 14);
            do
            {
                NextStart = VNFUtils.Strings.IndexOfExclosed(SCA, "declare_script", '\"');
                if(NextStart > 0)
                {
                    IndivScripts.Add(SCA.Remove(NextStart));
                    SCA = SCA.Remove(0, NextStart + 14);
                }
                else { IndivScripts.Add(SCA); }
            }
            while (NextStart > 0);
            Hashtable TrueIndivScripts = new Hashtable();
            foreach(String S in IndivScripts)
            {
                String Nameless = S.Remove(0, S.IndexOf('\"') + 1);
                String Name = Nameless.Remove(Nameless.IndexOf('\"'));
                Nameless = Nameless.Remove(0, Nameless.IndexOf('\"'));
                Nameless = Nameless.Remove(0, Nameless.IndexOf(':'));
                if (Nameless.EndsWith("}")) { Nameless = Nameless + ","; }
                TrueIndivScripts.Add(Name, Nameless);
            }
            Hashtable EventScripts = new Hashtable();
            foreach(String S in TrueIndivScripts.Keys)
            {
                EventScripts.Add(S.ToUpper(), AssembleEventScript((String)TrueIndivScripts[S]));
            }
            return EventScripts;
        }
        public static Object[] AssembleEventScript(String StringFormatScript)
        {
            StringFormatScript = VNFUtils.Strings.RemoveExclosed(StringFormatScript, ' ', '\"');
            ArrayList SSAssembled = new ArrayList();
            while (VNFUtils.Strings.ContainsExclosed(StringFormatScript, '{', '\"'))
            {
                StringFormatScript = StringFormatScript.Remove(0, StringFormatScript.IndexOf('{') + 1);
                int EndIndex = VNFUtils.Strings.IndexOfExclosed(StringFormatScript, "},", '\"');
                String FoundScriptShift = StringFormatScript.Remove(EndIndex);
                StringFormatScript = StringFormatScript.Remove(0, EndIndex + 2);
                String[] SCommands = VNFUtils.Strings.SplitAtExclosed(FoundScriptShift, ',', new char[] { '>', '\"' });
                Object[] ThisTrueShift = new Object[SCommands.Length];
                int CIndex = 0;
                foreach(String S in SCommands)
                {
                    if (S[0] == '\"' && S[S.Length - 1] == '\"') { ThisTrueShift[CIndex] = S.Remove(0, 1).Remove(S.Length - 2); }
                    else if (S.StartsWith("FACTORY"))
                    {
                        String FBlueprint = S.Remove(0, S.IndexOf('>') + 1);
                        FBlueprint = VNFUtils.Strings.RemoveExclosed(FBlueprint, '>', '\"');
                        FBlueprint = FBlueprint.Trim('\n');
                        ThisTrueShift[CIndex] = new VoidDel(delegate ()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate ()
                            {
                                EntityFactory.Assemble(FBlueprint);
                            }));
                        });
                    }
                    else if (S.StartsWith("RUN"))
                    {
                        String VBlueprint = S.Remove(0, S.IndexOf('>') + 1);
                        VBlueprint = VNFUtils.Strings.RemoveExclosed(VBlueprint, '>', '\"');
                        VBlueprint = VBlueprint.Trim('\n');
                        VoidDel VD = EntityFactory.AssembleVoidDelegate(VBlueprint);
                        ThisTrueShift[CIndex] = new VoidDel(delegate ()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate ()
                            {
                                VD();
                            }));
                        });
                    }
                    CIndex++;
                }
                SSAssembled.Add(ThisTrueShift);
            }
            return SSAssembled.ToArray();
        }
        public static Boolean VineGot = false;
        public static String GetVineName()
        {
            ArrayList Vinny = new ArrayList();
            Vinny.Add("Binsnort");
            Vinny.Add("Shipyard");
            Vinny.Add("Blimpnaut");
            Vinny.Add("Binyot");
            Vinny.Add("Thotyot");
            Vinny.Add("Nesblart");
            Vinny.Add("Angstcart");
            Vinny.Add("Orbslot");
            Vinny.Add("Woodrot");
            Vinny.Add("Vineyacht");
            Vinny.Add("Kidstot");
            Vinny.Add("Brainyot");
            Vinny.Add("Brynnjolf");
            VineGot = true;
            return (String)Vinny[Shell.Rnd.Next(0, Vinny.Count)];
        }
        public class ScriptParseException : Exception
        {
            public ScriptParseException(String Arg) : base(Arg)
            { }
        }
        [Serializable]
        public class ScriptSniffer : WorldEntity
        {
            Boolean SkipAll = false;
            public static String[] ShiftCondition = new String[0];
            private String[] LocalSC = new String[0];
            int ShiftStartTime = Environment.TickCount;
            int ScriptIndex = 0;
            public int Index { get { return ScriptIndex; } }
            public void ThrowForInstantExit()
            {
                String ExitScript = Sofia.GetExitThrowLocation(Name, ShiftCondition);
                AllowScriptShift = false;
                ActivateScriptElement("D|#CBUTTONS");
                CeaseSkipping();
                ScriptIndex = 0;
                pMyScript = RetrieveScriptByName(ExitScript);
                ScriptName = ExitScript;
                pName = ExitScript + "_SNIFFER";
                LoadMode = false;
                InitScriptShift();
                AllowScriptShift = true;
            }
            public Boolean PlacedInPauseState { get; set; }
            private int HungTime = -1;
            public void TimeHang()
            {
                HungTime = ShiftTimeElapsed();
            }
            public void UnHang()
            {
                if (HungTime != -1)
                {
                    ShiftStartTime = Environment.TickCount - HungTime;
                    HungTime = -1;
                }
            }
            int ShiftTimeElapsed()
            {
                return Environment.TickCount - ShiftStartTime;
            }
            [field: NonSerialized]
            object[] pMyScript = new object[0][];
            public object[] MyScript { get { return pMyScript; } }
            public String ScriptThrowTarget { get; set; }
            private Boolean LoadMode = false;
            public ScriptSniffer(String Name, object[] Script, String ScriptIndex) : base(Name, new Vector2(), null, 0)
            {
                ScriptThrowTarget = "#MAINMENU";
                ScriptName = ScriptIndex;
                Shell.GlobalWorldState = "SCRIPTSNIFFER_FIRST_INITIALIZED";
                AllowScriptShift = true;
                pMyScript = Script;
                InitScriptShift();
            }
            public ScriptSniffer(String Name, object[] Script, String ScriptIndex, Boolean Loading) : base(Name, new Vector2(), null, 0)
            {
                ScriptThrowTarget = "#MAINMENU";
                ScriptName = ScriptIndex;
                LoadMode = Loading;
                Shell.GlobalWorldState = "SCRIPTSNIFFER_FIRST_INITIALIZED";
                AllowScriptShift = true;
                pMyScript = Script;
                InitScriptShift();
            }
            private String ScriptName = "";
            public void ReassignScript()
            {
                pMyScript = RetrieveScriptByName(ScriptName);
            }
            public static void GlobalCeaseSkipping()
            {
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E is ScriptProcessor.ScriptSniffer) { ((ScriptProcessor.ScriptSniffer)E).CeaseSkipping(); }
                }
            }
            public Boolean Skipping { get { return SkipAll; } }
            public void CeaseSkipping()
            {
                SkipAll = false;
            }
            public override void OnDeserializeDo()
            {
                base.OnDeserializeDo();
                SkipAll = false;
                ReassignScript();
                ShiftCondition = LocalSC;
                ShiftStartTime = Environment.TickCount;
                LastTime = Environment.TickCount;
                CountApplicableRollbacks = RollbacksOnReturn;
                AllowScriptShift = true;
                /*foreach (object O in (object[])pMyScript[ScriptIndex])
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
            Boolean LastCheck = false;
            int LastTime = Environment.TickCount;
            public void Skip()
            {
                SkipAll = true;
            }
            public void SetLoadBreaker(int Index)
            {
                LoadBreaker = Index;
            }
            private int LoadBreaker = -1;
            private Boolean LoadInit = true;
            private Queue ForceScriptInsertionQueue = new Queue();
            public void ForceInsertScriptElement(object[] OneScriptShift)
            {
                if(ForceScriptInsertionQueue is null) { ForceScriptInsertionQueue = new Queue(); }
                ForceScriptInsertionQueue.Enqueue(OneScriptShift);
                PopForceInsertion();
            }
            public void ForceInsertMultipleScriptElements(object[] MultipleScriptShifts)
            {
                if (ForceScriptInsertionQueue is null) { ForceScriptInsertionQueue = new Queue(); }
                foreach (object[] OneScriptShift in MultipleScriptShifts)
                {
                    ForceScriptInsertionQueue.Enqueue(OneScriptShift);
                }
                PopForceInsertion();
            }
            public void PopForceInsertion()
            {
                if (ForceScriptInsertionQueue == null || ForceScriptInsertionQueue.Count == 0)
                {
                    throw new ScriptParseException("Error during forced script element insertion: The forced insertion queue was empty or null.");
                }
                object[] OneScriptShift = (object[])ForceScriptInsertionQueue.Dequeue();
                Shell.GlobalWorldState = "LOADED FORCED SHIFT...";
                Shell.WriteLine("Force inserting script element shift.");
                ShiftStartTime = Environment.TickCount;
                ShiftCondition = new String[0];
                if (ScriptIndex >= pMyScript.Length - 1 && ForceScriptInsertionQueue.Count == 0) { ScriptIndex--; }
                PushScriptShift(OneScriptShift);
                LastTime = Environment.TickCount;
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
                if(LocalSC != ShiftCondition) { LocalSC = ShiftCondition; }
                if (!LoadMode)
                {
                    if (Shell.DeleteQueue.Contains(this)) { return; }
                    KeyboardState K = Keyboard.GetState();
                    if (K.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Tab) && !LastCheck) { SkipAll = !SkipAll; }
                    LastCheck = K.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Tab);
                    if (CheckForShiftCondition() && (Environment.TickCount - LastTime > 60 || Name == "INTRO_SNIFFER_UNIQUE") && AllowScriptShift)
                    {
                        if ((Shell.GlobalWorldState == "CONTINUE" || Shell.GlobalWorldState == "NEXT") && Name != "INTRO_SNIFFER_UNIQUE") { Shell.GlobalWorldState = "LOADED NEXT SHIFT..."; }
                        if(SkipAll)
                        {
                            foreach (WorldEntity E in Shell.UpdateQueue)
                            {
                                foreach (Animation A in E.AnimationQueue)
                                {
                                    A.Jump(E);
                                }
                            }
                        }
                        if (ForceScriptInsertionQueue != null && ForceScriptInsertionQueue.Count > 0)
                        {
                            PopForceInsertion();
                        }
                        else
                        {
                            ShiftStartTime = Environment.TickCount;
                            ShiftCondition = new String[0];
                            ScriptIndex++;
                            InitScriptShift();
                            LastTime = Environment.TickCount;
                        }
                    }
                }
                else if (LoadBreaker > -1)
                {
                    if(LoadInit)
                    {
                        Shell.DefaultShell.IsFixedTimeStep = false;
                        LoadInit = false;
                        return;
                    }
                    if (ScriptIndex < LoadBreaker)
                    {
                        foreach (WorldEntity E in Shell.UpdateQueue)
                        {
                            foreach (Animation A in E.AnimationQueue)
                            {
                                A.Jump(E);
                            }
                        }
                        ShiftStartTime = Environment.TickCount;
                        ShiftCondition = new String[0];
                        ScriptIndex++;
                        InitScriptShift();
                        LastTime = Environment.TickCount;
                    }
                    else
                    {
                        Shell.GlobalWorldState = "LOADED SHIFT VIA LOADMODE...";
                        Shell.HoldRender = false;
                        Shell.DefaultShell.IsFixedTimeStep = true;
                        LoadMode = false;
                    }
                }
            }
            int PushScriptShift(object[] CurrentShift)
            {
                CountApplicableRollbacks = 0;
                foreach (object O in CurrentShift)
                {
                    ActivateScriptElement(O, SkipAll);
                    if (SkipAll && O is String && ((String)O).Split('|')[0].ToUpper() == "T")
                    {
                        foreach (WorldEntity E in Shell.UpdateQueue)
                        {
                            if (E is TextEntity && E.Name == "TEXT_MAIN")
                            {
                                ((TextEntity)E).SkipWrite();
                                TextEntity.PlayTick();
                                break;
                            }
                        }
                    }
                }
                LocalSC = ShiftCondition;
                return CountApplicableRollbacks;
            }
            public int RollbacksOnReturn { get; set; }
            void InitScriptShift()
            {
                RollbacksOnReturn = 0;
                Shell.WriteLine("Initiating script shift.");
                if(ScriptIndex >= pMyScript.Length)
                {
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        ButtonScripts.BackToMainMenu();
                    }));
                    return;
                }
                PushScriptShift((object[])pMyScript[ScriptIndex]);
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
                            PastStates.Push(Shell.SerializeState());
                        });
                    }));
                }
                if (PastStates.Count > 200)
                {
                    Stack TempStack = new Stack();
                    for (int i = 0; i < 200; i++) { TempStack.Push(PastStates.Pop()); }
                    PastStates.Clear();
                    for (int i = 0; i < 200; i++) { PastStates.Push(TempStack.Pop()); }
                    TempStack.Clear();
                }
            }
            Boolean CheckForShiftCondition()
            {
                Boolean Out = true;
                foreach(String C in ShiftCondition)
                {
                    String[] Conditions = C.ToUpper().Split(':');
                    switch(Conditions[0])
                    {
                        case "TIME":
                            if(SkipAll) { return true; }
                            if(!(ShiftTimeElapsed() >= Convert.ToInt32(Conditions[1])))
                            {
                                Out = false;
                            }
                            if(Conditions.Length > 2 && Conditions[2] == "ORSKIP" && Shell.GlobalWorldState == "CONTINUE")
                            {
                                Out = true;
                            }
                            break;
                        case "GWS":
                            if(SkipAll && Conditions[1] == "CONTINUE") { return true; }
                            else if(SkipAll && Conditions[1] != "CONTINUE") { SkipAll = false; }
                            if(Shell.GlobalWorldState != Conditions[1])
                            {
                                Out = false;
                            }
                            break;
                    }
                    if(Out == false) { break; }
                }
                return Out;
            }
        }
        /*
         * T|| specifies text to write, and the character caption if applicable.
         * C| specifies conditions to move to the next script shift.
         * B| breaks with the current script, and starts a new script if applicable. |#MAINMENU to return to the main menu.
         * D| deletes the entity with the specified entity name, or all custom buttons via |#CBUTTONS. ||IFPRESENT causes the function not to throw an error for a missing object.
         * A|| plays an animation on a named entity by animation name. #DISMISS clears the entity's animation queue.
         * A|||||| plays an animation on a named entity by defined tween parameters.
         * F||| sets the atlas frame of a specified entity to the given coordinates, if possible.
         * F|| sets the atlas frame of a specified entity to the given named frame state, if possible.
         * G| sets the GlobalWorldState.
         * U|| updates a given game flag to the specified value.
         * R||| reads a game flag and then performs the following command if it matches a specified value.
         * H halts script skipping upon occurrence.
         * S|| plays a named sound effect, or stops all sound effects via |#CLOSEALL. Second parameter sets looping.
         * M||| switches the music track to a named song, or stops the song via |#NULL|. Second parameter sets looping. Third if set to "INSTANT" skips the auto fadeout.
        */
        public static String LabelEntity = "";
        static public void SetFocus(String Label)
        {
            if(Label.ToUpper() == LabelEntity) { return; }
            Boolean FoundNew = false;
            foreach (WorldEntity WE2 in Shell.UpdateQueue)
            {
                if (WE2.Name == Label.ToUpper() && LabelEntity != Label.ToUpper())
                {
                    FoundNew = true;
                    foreach (WorldEntity WE3 in Shell.UpdateQueue)
                    {
                        if (WE3.Name == LabelEntity)
                        {
                            Boolean[] ScaleChecks = WE3.CheckScaleInversions();
                            Animation A = Animation.Retrieve("FOCUSSHRINK");
                            if(ScaleChecks[0] || ScaleChecks[1]) { A.AutoInvertScaling(ScaleChecks[0], ScaleChecks[1]); }
                            WE3.AnimationQueue.Add(A);
                            break;
                        }
                    }
                    LabelEntity = Label.ToUpper();
                    Boolean[] ScaleChecks2 = WE2.CheckScaleInversions();
                    Animation A2 = Animation.Retrieve("FOCUSGROW");
                    if (ScaleChecks2[0] || ScaleChecks2[1]) { A2.AutoInvertScaling(ScaleChecks2[0], ScaleChecks2[1]); }
                    WE2.AnimationQueue.Add(A2);
                    break;
                }
            }
            if (!FoundNew && LabelEntity != Label.ToUpper())
            {
                foreach (WorldEntity WE3 in Shell.UpdateQueue)
                {
                    if (WE3.Name == LabelEntity)
                    {
                        Boolean[] ScaleChecks = WE3.CheckScaleInversions();
                        Animation A = Animation.Retrieve("FOCUSSHRINK");
                        if (ScaleChecks[0] || ScaleChecks[1]) { A.AutoInvertScaling(ScaleChecks[0], ScaleChecks[1]); }
                        WE3.AnimationQueue.Add(A);
                        break;
                    }
                }
                LabelEntity = "";
            }
        }
        static public void ActivateScriptElement(object Element)
        {
            ActivateScriptElement(Element, false);
        }
        static public void ActivateScriptElement(object Element, Boolean SnifferSkipping)
        {
            if (Element is String)
            {
                Shell.WriteLine((String)Element);
                String E = (String)Element;
                String[] Parts = E.Split('|');
                if (Parts[0].ToUpper() == "T")
                {
                    T(Parts, SnifferSkipping);
                }
                else if (Parts[0].ToUpper() == "C")
                {
                    C(Parts);
                }
                else if (Parts[0].ToUpper() == "B")
                {
                    B(Parts);
                }
                else if (Parts[0].ToUpper() == "D")
                {
                    D(Parts);
                }
                else if (Parts[0].ToUpper() == "A")
                {
                    A(Parts);
                }
                else if (Parts[0].ToUpper() == "F")
                {
                    F(Parts);
                }
                else if (Parts[0].ToUpper() == "S")
                {
                    S(Parts);
                }
                else if (Parts[0].ToUpper() == "M")
                {
                    M(Parts, E);
                }
                else if (Parts[0].ToUpper() == "H")
                {
                    ScriptSniffer.GlobalCeaseSkipping();
                }
                else if (Parts[0].ToUpper() == "G")
                {
                    Shell.GlobalWorldState = Parts[1];
                }
                else if (Parts[0].ToUpper() == "U")
                {
                    U(Parts);
                }
                else if (Parts[0].ToUpper() == "R")
                {
                    R(Parts);
                }
            }
            else if (Element is VoidDel)
            {
                Shell.WriteLine("Executing anonymous method from script.");
                VoidDel E = (VoidDel)Element;
                E();
            }
        }
        private static void T(String[] Parts, Boolean SnifferSkipping)
        {
            //Label change text bounce...
            TextEntity TL = null;
            WorldEntity NBacking = null;
            foreach (WorldEntity WE in Shell.UpdateQueue)
            {
                if (WE.Name == "TEXT_LABEL" && WE is TextEntity)
                {
                    TL = (TextEntity)WE;
                    if (((TextEntity)WE).Text.Remove(0, ((TextEntity)WE).Text.LastIndexOf(']') + 1) != Parts[1] && !(VineGot && Parts[1].ToUpper() == "VINNY"))
                    {
                        if (!SnifferSkipping) { WE.AnimationQueue.Add(Animation.Retrieve("BOUNCE_3")); }
                        SetFocus(Parts[1]);
                    }
                }
                if (WE.Name == "NAMELABELBACKING")
                {
                    NBacking = WE;
                }
                if (TL != null && NBacking != null) { break; }
            }
            VineGot = false;
            WriteArchive(Parts[2], Parts[1]);
            TextEntity Main = new TextEntity("", "", new Vector2(), 0);
            Boolean Found = false;
            foreach (WorldEntity WE in Shell.UpdateQueue)
            {
                if (WE.Name == "TEXT_MAIN" && WE is TextEntity)
                {
                    Main = (TextEntity)WE;
                    Found = true;
                    break;
                }
            }
            if (!Found)
            {
                Main = new TextEntity("TEXT_MAIN", Parts[2], new Vector2(150, 500), 0.96f);
                if(ButtonScripts.UIHideEnabled) { Main.Drawable = false; }
                Shell.RunQueue.Add(new VoidDel(delegate ()
                {
                    Shell.UpdateQueue.Add(Main);
                    Shell.RenderQueue.Add(Main);
                }));
            }
            Main.Text = Parts[2];
            Main.ReWrite();
            if (Parts[1] != "")
            {
                if (Parts[1].ToUpper() == "VINNY" && Shell.Rnd.Next(0, 50) == 0)
                {
                    Parts[1] = GetVineName();
                }
                TextEntity Label = TL;
                if (TL is null)
                {
                    Label = new TextEntity("TEXT_LABEL", Parts[1], new Vector2(140, 420), 0.96f);
                    if (!SnifferSkipping) { Label.AnimationQueue.Add(Animation.Retrieve("BOUNCE_3")); }
                    SetFocus(Parts[1]);
                    if (ButtonScripts.UIHideEnabled) { Label.Drawable = false; }
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Shell.UpdateQueue.Add(Label);
                        Shell.RenderQueue.Add(Label);
                    }));
                }
                if (NBacking is null)
                {
                    NBacking = new WorldEntity("NAMELABELBACKING", new Vector2(90, 388), (TAtlasInfo)Shell.AtlasDirectory["NAMEBACKING"], 0.9f);
                    NBacking.ColourValue = new Color(255, 255, 255, 200);
                    if (ButtonScripts.UIHideEnabled) { NBacking.Drawable = false; }
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Shell.UpdateQueue.Add(NBacking);
                        Shell.RenderQueue.Add(NBacking);
                    }));
                }
                Label.TypeWrite = false;
                Label.Text = "[C:PURPLE]" + Parts[1];
            }
            else
            {
                if (TL != null)
                {
                    Shell.DeleteQueue.Add(TL);
                    TL.Text = "";
                }
                if (NBacking != null)
                {
                    Shell.DeleteQueue.Add(NBacking);
                }
            }
        }
        public static object ParseLiteralValue(String Input)
        {
            float f;
            if (float.TryParse(Input.TrimEnd(new char[] { 'f', 'd' }), out f))
            {
                if (Input.Contains("f") || (Input.Contains(".") && !Input.Contains("d"))) { return float.Parse(Input.Replace("f", "")); }
                else if (Input.Contains("d")) { return double.Parse(Input.Replace("d", "")); }
                else { return int.Parse(Input); }
            }
            else if (Input.ToUpper() == "TRUE" || Input.ToUpper() == "FALSE")
            {
                return Input.ToUpper() == "TRUE";
            }
            else { return Input; }
        }
        private static void U(String[] Parts)
        {
            String FlagName = Parts[1].ToUpper();
            String TextFlagVal = Parts[2];
            object TrueVal = ParseLiteralValue(TextFlagVal);
            Shell.UpdateFlag(FlagName, TrueVal);
        }
        private static void R(String[] Parts)
        {
            String FlagName = Parts[1].ToUpper();
            String TextFlagComparisonVal = Parts[2];
            object TrueComparisonVal = ParseLiteralValue(TextFlagComparisonVal);
            if(Shell.ReadFlag(FlagName).Equals(TrueComparisonVal) && Parts.Length > 3)
            {
                String ConditionalCom = "";
                for(int i = 3; i < Parts.Length; i++)
                {
                    ConditionalCom += Parts[i] + "|";
                }
                ConditionalCom = ConditionalCom.TrimEnd('|');
                ActivateScriptElement(ConditionalCom);
            }
        }
        private static void C(String[] Parts)
        {
            ArrayList SConditions = new ArrayList();
            Boolean First = true;
            foreach (String S in Parts)
            {
                if (!First) { SConditions.Add(S); }
                First = false;
            }
            ScriptSniffer.ShiftCondition = SConditions.ToArray().Select(x => (String)x).ToArray();
        }
        private static void B(String[] Parts)
        {
            Boolean Skipping = false;
            for (int i = 0; i < Shell.UpdateQueue.Count; i++)
            {
                if (Shell.UpdateQueue[i] is ScriptSniffer)
                {
                    if(((ScriptSniffer)Shell.UpdateQueue[i]).Skipping) { Skipping = true; }
                    Shell.DeleteQueue.Add(Shell.UpdateQueue[i]);
                    break;
                }
            }
            if (Parts.Length > 1)
            {
                if (Parts[1] != "")
                {
                    if (Parts[1].ToUpper() == "#MAINMENU")
                    {
                        Shell.RunQueue.Add(new VoidDel(delegate ()
                        {
                            ButtonScripts.BackToMainMenu();
                        }));
                    }
                    else if (Parts[1].ToUpper() == "#SCRIPTTHROWTARGET")
                    {
                        Boolean Found = false;
                        foreach (WorldEntity W in Shell.UpdateQueue)
                        {
                            if (W is ScriptSniffer)
                            {
                                Found = true;
                                ActivateScriptElement("B|" + ((ScriptSniffer)W).ScriptThrowTarget);
                                break;
                            }
                        }
                        if (!Found)
                        {
                            throw (new ScriptParseException("Script parsing error during script throw target activation; unable to find an active ScriptSniffer."));
                        }
                    }
                    else
                    {
                        ScriptSniffer New = new ScriptSniffer(Parts[1].ToUpper() + "_SNIFFER", RetrieveScriptByName(Parts[1]), Parts[1]);
                        if(Skipping) { New.Skip(); }
                        Shell.RunQueue.Add(new VoidDel(delegate ()
                        {
                            Shell.UpdateQueue.Add(New);
                        }));
                    }
                }
            }
        }
        private static void D(String[] Parts)
        {
            Boolean CBMode = false;
            Boolean Found = false;
            Boolean All = false;
            if (Parts[1].ToUpper() == "#CBUTTONS")
            {
                CBMode = true;
                Found = true;
            }
            if (Parts[1].ToUpper() == "#ALL")
            {
                All = true;
                Found = true;
            }
            ArrayList CompanionRemoves = new ArrayList();
            for (int i = 0; i < Shell.UpdateQueue.Count; i++)
            {
                if ((All && !(((WorldEntity)Shell.UpdateQueue[i]) is ScriptSniffer)) || (!CBMode && ((WorldEntity)Shell.UpdateQueue[i]).Name.ToUpper() == Parts[1]) || (CBMode && ((WorldEntity)Shell.UpdateQueue[i]).Name.ToUpper().Contains("BUTTON_CUSTOM_")))
                {
                    Shell.DeleteQueue.Add(Shell.UpdateQueue[i]);
                    if ((WorldEntity)Shell.UpdateQueue[i] is DropMenu) { CompanionRemoves.Add(Shell.UpdateQueue[i]); }
                    if (!CBMode && !All)
                    {
                        Found = true;
                        break;
                    }
                }
            }
            if(CompanionRemoves.Count > 0)
            {
                foreach(WorldEntity E in CompanionRemoves)
                {
                    if (E is DropMenu)
                    {
                        Shell.RunQueue.Add(new VoidDel(delegate () { ((DropMenu)E).DepopulateDropList(); }));
                    }
                }
            }
            if (!Found && (Parts.Length < 3 || Parts[2].ToUpper() != "IFPRESENT"))
            {
                throw (new ScriptParseException("Entity delete command could not locate the specified entity: " + Parts[1]));
            }
        }
        private static void A(String[]Parts)
        {
            WorldEntity O = Shell.GetEntityByName(Parts[1]);
            if (O != null)
            {
                WorldEntity AnimatedObject = (WorldEntity)O;
                if (Parts.Length == 3 || Parts.Length == 4)
                {
                    if (Parts[2] == "#DISMISS") { O.AnimationQueue.Clear(); }
                    else
                    {
                        Animation R = Animation.Retrieve(Parts[2]);
                        if (R.AnimName.ToUpper() != "NULL") { AnimatedObject.AnimationQueue.Add(R); }
                        if (Parts.Length == 4)
                        {
                            if (Parts[3].ToUpper() == "LOOP") { R.Loop = true; }
                            else { R.Loop = false; }
                        }
                    }
                }
                else if (Parts.Length == 7)
                {
                    //Parameters are seperated by commas, parameters within parameters are seperated by "="
                    //A looped vector tween: A|[entityname]|50=50,1000,20||||loop
                    Animation New = new Animation(AnimatedObject.Name + "_animation_scriptdefined");
                    if (Parts[2].Length > 0)
                    {
                        String[] MV = Parts[2].Split(',');
                        String[] MS = MV[0].Split('=');
                        Vector2 T = new Vector2((float)Convert.ToDouble(MS[0]), (float)Convert.ToDouble(MS[1]));
                        New.WriteMovement(Animation.CreateVectorTween(T, Convert.ToInt32(MV[1]), Convert.ToInt32(MV[2])));
                    }
                    if (Parts[3].Length > 0)
                    {
                        String[] MV = Parts[3].Split(',');
                        float T = (float)Convert.ToDouble(MV[0]);
                        New.WriteRotation(Animation.CreateFloatTween(T, Convert.ToInt32(MV[1]), Convert.ToInt32(MV[2])));
                    }
                    if (Parts[4].Length > 0)
                    {
                        String[] MV = Parts[4].Split(',');
                        String[] SS = MV[0].Split('=');
                        Vector2 T = new Vector2((float)Convert.ToDouble(SS[0]), (float)Convert.ToDouble(SS[1]));
                        New.WriteScaling(Animation.CreateVectorTween(T, Convert.ToInt32(MV[1]), Convert.ToInt32(MV[2])));
                    }
                    if (Parts[5].Length > 0)
                    {
                        String[] MV = Parts[5].Split(',');
                        String[] CS = MV[0].Split('=');
                        ColourShift T = new ColourShift(Convert.ToInt32(CS[0]), Convert.ToInt32(CS[1]), Convert.ToInt32(CS[2]), Convert.ToInt32(CS[3]));
                        New.WriteColouring(Animation.CreateColourTween(T, Convert.ToInt32(MV[1]), Convert.ToInt32(MV[2])));
                    }
                    if (Parts[6].Length > 0 && Parts[6].ToUpper() == "LOOP") { New.Loop = true; }
                    AnimatedObject.AnimationQueue.Add(New);
                }
                else { throw (new ScriptParseException("The animation command does not take the specified number of parameters: " + (Parts.Length - 1))); }
            }
            else { throw (new ScriptParseException("Entity animation command could not locate the specified entity: " + Parts[1])); }
        }
        private static void F(String[] Parts)
        {
            WorldEntity O = Shell.GetEntityByName(Parts[1]);
            if (O != null)
            {
                if (Parts.Length == 3)
                {
                    Hashtable Lookup = O.Atlas.FrameLookup;
                    if (Lookup.ContainsKey(Parts[2].ToUpper()))
                    {
                        Point Frame = (Point)Lookup[Parts[2].ToUpper()];
                        O.SetAtlasFrame(Frame);
                    }
                }
                else if (Parts.Length == 4)
                {
                    Point Frame = new Point(Convert.ToInt32(Parts[2]), Convert.ToInt32(Parts[3]));
                    O.SetAtlasFrame(Frame);
                }
                else { throw (new ScriptParseException("The atlas frame shift command does not take the specified number of parameters: " + (Parts.Length - 1))); }
            }
            else { throw (new ScriptParseException("Atlas frame shift command could not locate the specified entity: " + Parts[1])); }
        }
        private static void S(String[] Parts)
        {
            if (Parts[1].ToUpper() != "#CLOSEALL")
            {
                if (!Shell.Mute)
                {
                    SoundEffectInstance LocalSound = ((SoundEffect)Shell.SFXDirectory[Parts[1]]).CreateInstance();
                    LocalSound.Volume = Shell.GlobalVolume;
                    if (Parts.Length > 2 && Parts[2].ToUpper() != "")
                    {
                        if (Parts[2].ToUpper() == "TRUE")
                        {
                            LocalSound.IsLooped = true;
                        }
                        else if (Parts[2].ToUpper() == "FALSE")
                        {
                            LocalSound.IsLooped = false;
                        }
                    }
                    LocalSound.Play();
                    Shell.ActiveSounds.Add(LocalSound);
                }
            }
            else
            {
                foreach (SoundEffectInstance SEI in Shell.ActiveSounds)
                {
                    SEI.Stop();
                }
            }
        }
        private static void M(String[] Parts, String MCommand)
        {
            ButtonScripts.SpoonsTrip = true;
            SongCom = MCommand;
            Boolean Instant = false;
            if (Parts.Length > 3 && Parts[3].ToUpper() != "")
            {
                if (Parts[3].ToUpper() == "INSTANT")
                {
                    Instant = true;
                }
            }
            if (Parts[1].ToUpper() != "#NULL")
            {
                Song LocalSong = ((Song)Shell.SongDirectory[Parts[1]]);
                if (Instant) { Shell.QueueInstantTrack(LocalSong, 1f); }
                else { Shell.QueueInstantTrack(LocalSong); }
            }
            else
            {
                if (Instant) { Shell.OneFadeout(1f); }
                else { Shell.OneFadeout(); }
            }
            if (Parts.Length > 2 && Parts[2].ToUpper() != "")
            {
                if (Parts[2].ToUpper() == "TRUE")
                {
                    MediaPlayer.IsRepeating = true;
                }
                else if (Parts[2].ToUpper() == "FALSE")
                {
                    MediaPlayer.IsRepeating = false;
                }
            }
        }
    }
}
