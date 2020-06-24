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
    public static class Sofia
    {
        public static void InitSofiaFlags()
        {
            KingFlag = 0;
            CrookedFlag = 0;
            MysticFlag = 0;
            ParanoidFlag = 0;
            CrookedKnowledgeFlag = 0;
            StoryFlag = 0;
        }
        public static byte KingFlag
        {
            get { return (byte)Shell.ReadFlag("KING"); }
            set { Shell.UpdateFlag("KING", value); }
        }
        public static byte CrookedFlag
        {
            get { return (byte)Shell.ReadFlag("CROOKED"); }
            set { Shell.UpdateFlag("CROOKED", value); }
        }
        public static byte ParanoidFlag
        {
            get { return (byte)Shell.ReadFlag("PARANOID"); }
            set { Shell.UpdateFlag("PARANOID", value); }
        }
        public static byte CrookedKnowledgeFlag
        {
            get { return (byte)Shell.ReadFlag("KNOWLEDGE"); }
            set { Shell.UpdateFlag("KNOWLEDGE", value); }
        }
        public static byte MysticFlag
        {
            get { return (byte)Shell.ReadFlag("MYSTIC"); }
            set { Shell.UpdateFlag("MYSTIC", value); }
        }
        public static byte StoryFlag
        {
            get { return (byte)Shell.ReadFlag("STORY"); }
            set { Shell.UpdateFlag("STORY", value); }
        }
        public static String GetContextualLocation()
        {
            String ScriptName = ScriptProcessor.SnifferSearch().Name.ToUpper();
            if (ScriptName.Contains("SOFIA_KING")) { return "KING"; }
            else if (ScriptName.Contains("SOFIA_CROOKED")) { return "CROOKED"; }
            else if (ScriptName.Contains("SOFIA_MYSTIC")) { return "MYSTIC"; }
            return "NULL";
        }
        public static String GetExitThrowLocation(String Name, String[] ShiftCondition)
        {
            String ExitScript = "";
            switch (Name)
            {
                case "SOFIA_KING_PRIMARY_SNIFFER":
                    ExitScript = "EXIT_KING_PRIMARY";
                    break;
                case "SOFIA_CROOKED_PRIMARY_FINALIZE_SNIFFER":
                    ExitScript = "EXIT_CROOKED_PRIMARY_FINALIZE";
                    break;
                default:
                    if (ShiftCondition.Contains<String>("GWS:impos_mapnavigate")) { ExitScript = "EXIT_DEFAULT_PROPER"; }
                    else
                    {
                        WorldEntity PotentialKing = Shell.GetEntityByName("KING SOFIA");
                        WorldEntity PotentialMystic = Shell.GetEntityByName("MYSTIC SOFIA");
                        WorldEntity PotentialCrooked = Shell.GetEntityByName("CROOKED SOFIA");
                        if (PotentialKing != null && PotentialKing.ColourValue.A >= 254f && PotentialKing.DrawCoords.X < 1280 && PotentialKing.DrawCoords.X > 0)
                        {
                            ExitScript = "EXIT_DEFAULT_KING";
                        }
                        else if (PotentialMystic != null && PotentialMystic.ColourValue.A >= 254f && PotentialMystic.DrawCoords.X < 1280 && PotentialMystic.DrawCoords.X > 0)
                        {
                            ExitScript = "EXIT_DEFAULT_MYSTIC";
                        }
                        else if (PotentialCrooked != null && PotentialCrooked.ColourValue.A >= 254f && PotentialCrooked.DrawCoords.X < 1280 && PotentialCrooked.DrawCoords.X > 0)
                        {
                            ExitScript = "EXIT_DEFAULT_CROOKED";
                        }
                        else { ExitScript = "EXIT_DEFAULT"; }
                    }
                    break;
            }
            return ExitScript;
        }
        public static String NavigateToMystic()
        {
            if (KingFlag < 3) { return "SOFIA_MYSTIC_NO_EDICT"; } //Done
            else if (MysticFlag == 0) { return "SOFIA_MYSTIC_EXPLANATION"; } //Done
            else if (MysticFlag == 1) { return "SOFIA_MYSTIC_EXPLANATION_RETURN"; } //Done
            else if (MysticFlag == 2 && CrookedFlag == 4) { return "SOFIA_MYSTIC_ESSENCE_RETURN_SUCCESS"; } //Done
            else if (MysticFlag == 2 && CrookedFlag != 4) { return "SOFIA_MYSTIC_ESSENCE_RETURN_FAILURE"; } //Done
            else if (MysticFlag == 3) { return "SOFIA_MYSTIC_PREFINAL_RETURN"; } //Done
            else { return "SOFIA_MYSTIC_EXPLANATION"; } //Done
        }
        public static String NavigateToKing()
        {
            if (KingFlag == 0) { return "SOFIA_KING_SECONDARY"; } //Done
            else if (KingFlag == 1) { return "SOFIA_KING_RETURN_POST_GOLEM_PRE_INTERIOR"; } //Done
            else if (KingFlag < 3) { return "SOFIA_KING_RETURN_NO_EDICT"; } //Done
            else if (MysticFlag < 2) { return "SOFIA_KING_RETURN_CLUELESS"; } //Done
            else if (MysticFlag == 2 && KingFlag == 3)
            {
                if (CrookedFlag != 4) { return "SOFIA_KING_RETURN_NO_ESSENCE"; } //Done
                else { return "SOFIA_KING_FINAL"; }
            }
            else if (MysticFlag == 2 && KingFlag == 4)
            {
                if (CrookedFlag != 4) { return "SOFIA_KING_RETURN_NO_ESSENCE_RETURN"; } //Done
                else { return "SOFIA_KING_RETURN_NO_ESSENSE_RETURN_WITH_ESSENCE"; } //Done
            }
            else if (MysticFlag == 2 && KingFlag == 5)
            {
                if (CrookedFlag != 4) { return "SOFIA_KING_RETURN_DURING_ESSENCE_SEEK"; } //Done
                else { return "SOFIA_KING_RETURN_ESSENCE_SEEK_SUCCESSFUL"; } //Done
            }
            else { return "SOFIA_KING_FINAL"; } //Done
        }
        /*
         * Crookedflag 1 = Met crooked
         * 2 = Knows crooked offers essence
         * 3 = Seen crooked previously, wants essence
         * 4 = Essence attained
         * 5 = Not seen Crooked at all, but knows essence wanted
         */
        public static String NavigateToCrooked()
        {
            if (CrookedFlag == 0)
            {
                if (MysticFlag != 0) { return "SOFIA_CROOKED_SECONDARY_KNOWS_MYSTIC"; } //Done
                else { return "SOFIA_CROOKED_SECONDARY_DOESNT_KNOW_MYSTIC"; } //Done
            }
            else if (MysticFlag < 2)
            {
                if (ParanoidFlag == 1) { return "SOFIA_CROOKED_RETURN_PARANOID"; } //Done
                else { return "SOFIA_CROOKED_RETURN_CLUELESS"; } //Done
            }
            else if (MysticFlag == 2 && CrookedFlag == 5) { return "SOFIA_CROOKED_SECONDARY_WANT_ESSENCE"; } //Done
            else if (MysticFlag == 2 && CrookedFlag == 3)
            {
                //IF Mystic mission known to get essense, Crooked known that sells essence but crooked path was NOT completed, therefore if player has been to Crooked SECONDARY but left prior to king explanation
                if (ParanoidFlag == 0) { return "SOFIA_CROOKED_RETURN_NO_ESSENCE"; } //Done
                //IF Mystic mission known to get essense, Crooked known that sells essence but crooked path was NOT completed, therefore if player has been to Crooked SECONDARY but stayed for king explanation
                else if (ParanoidFlag == 1) { return "SOFIA_CROOKED_RETURN_NO_ESSENCE_PARANOID"; } //Done
                //IF Mystic mission known to get essence, Crooked seen PRIMARY or secondary after but BEFORE essence mission given by Mystic. That is to say if the crooked KNOWS about the save the world mission
                else { return "SOFIA_CROOKED_RETURN_NO_ESSENCE_MISSION_KNOWLEDGE"; } //Done
            }
            else
            {
                if (CrookedKnowledgeFlag == 0) { return "SOFIA_CROOKED_NO_KNOWLEDGE"; } //Done
                else { return "SOFIA_CROOKED_FINAL"; } //Done
            }
        }
        public class ParticleFire : WorldEntity
        {
            public static Boolean Cease = false;
            public ParticleFire(String Name) : base(Name, new Vector2(), null, 0)
            {
                Cease = false;
            }
            int LastFireCountdown = 1;
            public override void Update()
            {
                if(Shell.Rnd.Next(0, 2) == 0 && !Cease)
                {
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        UpwardParticle Add = new UpwardParticle("PURPLESPRITE", new Vector2(Shell.Rnd.Next(140, 1140), Shell.Rnd.Next(200, 600)), (TAtlasInfo)Shell.AtlasDirectory["PURPLE"], 0.4f);
                        Add.CenterOrigin = true;
                        Add.ColourValue = new Color(0, 0, 0, 0);
                        Add.AnimationQueue.Add(Animation.Retrieve("FADEINOUT"));
                        Add.TransientAnimation = true;
                        Shell.UpdateQueue.Add(Add);
                        Shell.RenderQueue.Add(Add);
                    }));
                }
                LastFireCountdown--;
                if(LastFireCountdown < 1) { LastFireCountdown = 1; }
                if (Shell.Rnd.Next(0, LastFireCountdown) == 0 && !Cease)
                {
                    LastFireCountdown = 400;
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Double Degree = Shell.Rnd.NextDouble() * (2*Math.PI);
                        Vector2 SpawnPos = (new Vector2((float)Math.Sin(Degree), (float)Math.Cos(Degree))*1000f) + new Vector2(640, 360);
                        Transient Add = new Transient("MEME", SpawnPos, (TAtlasInfo)Shell.AtlasDirectory["MEMES"], ((float)Shell.Rnd.NextDouble() * 0.09f) + 0.405f);
                        Add.CenterOrigin = true;
                        Vector2 Target = new Vector2(Shell.Rnd.Next(400, 881), Shell.Rnd.Next(300, 420));
                        Vector2 Trajectory = (Target - Add.DrawCoords) * 2;
                        Animation A = new Animation("MEMEFLOATER");
                        Animation B = new Animation("MEMEFLOATER_TURN");
                        SortedList Moves = Animation.CreateVectorTween(Trajectory, 7000, 20);
                        A.WriteMovement(Moves);
                        SortedList Rots = Animation.CreateFloatTween(Shell.Rnd.Next(-10, 10) / 10f, 1000, 20);
                        B.WriteRotation(Rots);
                        A.Loop = true;
                        B.Loop = true;
                        Add.AnimationQueue.Add(A);
                        Add.AnimationQueue.Add(B);
                        Add.SetAtlasFrame(new Point(Shell.Rnd.Next(0, 4), Shell.Rnd.Next(0, 4)));
                        Add.Scale(new Vector2(-0.5f, -0.5f));
                        Shell.UpdateQueue.Add(Add);
                        Shell.RenderQueue.Add(Add);
                    }));
                }
                base.Update();
            }
        }
        public class UpwardParticle : WorldEntity
        {
            public UpwardParticle(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth) : base(Name, Location, Atlas, Depth)
            {

            }
            int XVelocity = 0;
            public override void Update()
            {
                XVelocity += Shell.Rnd.Next(-1, 2);
                if (XVelocity > 5) { XVelocity = 5; }
                if (XVelocity < -5) { XVelocity = -5; }
                this.Move(new Vector2(XVelocity, -2));
                base.Update();
            }
        }
        [Serializable]
        public class Transient : WorldEntity
        {
            public Transient(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth) : base(Name, Location, Atlas, Depth)
            {
                StartTime = Environment.TickCount;
                Lifespan = 8000;
            }
            public int Lifespan { get; set; }
            int StartTime;
            int PausedTimeElapsed = -1;
            public override void OnSerializeDo()
            {
                base.OnSerializeDo();
                PausedTimeElapsed = Environment.TickCount - StartTime;
            }
            public override void OnDeserializeDo()
            {
                base.OnDeserializeDo();
                StartTime = Environment.TickCount - PausedTimeElapsed;
                PausedTimeElapsed = -1;
            }
            public override void Update()
            {
                base.Update();
                if (ButtonScripts.Paused)
                {
                    if(PausedTimeElapsed == -1) { PausedTimeElapsed = Environment.TickCount - StartTime; }
                    return;
                }
                else if (PausedTimeElapsed >= 0)
                {
                    StartTime = Environment.TickCount - PausedTimeElapsed;
                    PausedTimeElapsed = -1;
                }
                if(Environment.TickCount - StartTime > Lifespan)
                {
                    Shell.DeleteQueue.Add(this);
                    ManualDispose();
                }
            }
        }
        [Serializable]
        public class EssenseGlow : WorldEntity
        {
            WorldEntity SofiaE = null;
            private Vector2 SCDist = new Vector2(10, 255);
            public EssenseGlow(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, String AlternateParentName) : base(Name, Location, Atlas, Depth)
            {
                pOrigin = (VNFUtils.ConvertPoint(HitBox.Size) / 2) + SCDist;
                FindSofia(AlternateParentName);
            }
            public EssenseGlow(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth) : base(Name, Location, Atlas, Depth)
            {
                pOrigin = (VNFUtils.ConvertPoint(HitBox.Size) / 2) + SCDist;
                FindSofia();
            }
            private void FindSofia()
            {
                FindSofia("SOFIA");
            }
            private void FindSofia(String SearchName)
            {
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E.Name.ToUpper() == SearchName.ToUpper())
                    {
                        SofiaE = E;
                        break;
                    }
                }
            }
            public override void OnDeserializeDo()
            {
                SofiaE = null;
                base.OnDeserializeDo();
            }
            private void Mirror(WorldEntity E)
            {
                pScale = E.ScaleSize;
                if (E is BigSofia) { QuickMoveTo(E.DrawCoords + new Vector2(35, 100)); }
                else { QuickMoveTo(E.DrawCoords); }
            }
            public override void Update()
            {
                Color R = TextEntity.GetRainbowColour();
                float Fade = pColour.A / 255f;
                pColour = new ColourShift(R.R * Fade, R.G * Fade, R.B * Fade, pColour.A);
                if(SofiaE != null) { Mirror(SofiaE); }
                else { FindSofia(); }
                base.Update();
            }
        }
        [Serializable]
        public class SofiaBoomer : WorldEntity
        {
            int Ticks = 0;
            int Number = 0;
            int Interval = 0;
            public SofiaBoomer(String Name, int myNumber, int myInterval) : base(Name, new Vector2(), null, 0f)
            {
                Number = myNumber;
                Interval = myInterval;
                Ticks = Environment.TickCount;
            }
            public override void Update()
            {
                if(ScriptProcessor.SnifferSearch().Skipping) { Number = 0; }
                if(Number > 0)
                {
                    if(Environment.TickCount - Ticks > Interval)
                    {
                        ScriptProcessor.ActivateScriptElement("S|DEEPBOOM");
                        foreach (WorldEntity E in Shell.RenderQueue)
                        {
                            if (ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                            E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                        }
                        Number--;
                        Ticks = Environment.TickCount;
                    }
                }
                else { Shell.DeleteQueue.Add(this); }
                base.Update();
            }
        }
        [Serializable]
        public class RuneGlow : WorldEntity
        {
            private int SpaceAdjust = 0;
            public double GetMySinable
            {
                get
                {
                    int Ticks = Environment.TickCount;
                    int TickMod = (Ticks - SpaceAdjust) % 10000;
                    return (((double)TickMod / 10000d) * 2d * Math.PI);
                }
            }
            private Boolean Nodraw = false;
            public RuneGlow(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth) : base(Name, Location, Atlas, Depth)
            {
                pColour = new ColourShift(0f, 0f, 0f, 0f);
                SpaceAdjust = Environment.TickCount % 10000;
                CenterOrigin = false;
                foreach(WorldEntity W in Shell.UpdateQueue)
                {
                    if(W is RuneGlow && !(W == this))
                    {
                        Nodraw = true;
                        Shell.DeleteQueue.Add(this);
                    }
                }
            }
            public override void Update()
            {
                pColour = new ColourShift(255f, 255f, 255f, 255f) * (float)((-Math.Cos(GetMySinable) / 2d) + 0.5);
                base.Update();
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                if(Nodraw) { return; }
                base.Draw(spriteBatch);
            }
        }
        [Serializable]
        public class SourceGlow : WorldEntity
        {
            public SourceGlow(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth) : base(Name, Location, Atlas, Depth)
            {

            }
            public override void Update()
            {
                Color R = TextEntity.GetRainbowColour();
                float Fade = (pColour.A / 255f);
                pColour = new ColourShift(((R.R * 0.4f) + (255f * 0.6f)) * Fade, ((R.G * 0.4f) + (255f * 0.6f)) * Fade, ((R.B * 0.4f) + (255f * 0.6f)) * Fade, pColour.A);
                base.Update();
            }
        }
        [Serializable]
        public class BigSofia : WorldEntity
        {
            public BigSofia(String Name, Vector2 Location, TAtlasInfo? Atlas, float Depth, ArrayList InitialStates) : base(Name, Location, Atlas, Depth)
            {
                AtlasCoordinates = new Point(3, 2);
                BasePosition = Location;
                States = InitialStates;
                SpewFreqencyOne = 2000;
                SpewIntervalOne = 50;
                SpewFreqencyTwo = 2000;
                SpewWavesTwo = 3;
            }
            ~BigSofia()
            {
                foreach(WorldEntity E in Dependents) { Shell.DeleteQueue.Add(E); }
            }
            private double AdjustSin = -100d;
            private Vector2 BasePosition = new Vector2();
            public void ResetBasePosition()
            {
                BasePosition = DrawCoords;
            }
            public ArrayList States
            {
                get;
                set;
            }
            public int SpewFreqencyOne { get; set; }
            public int SpewIntervalOne { get; set; }
            public int SpewFreqencyTwo { get; set; }
            public int SpewWavesTwo { get; set; }
            private ArrayList Dependents = new ArrayList();
            private EssenseGlow MyMask = null;
            int LastTime = Environment.TickCount;
            int MyTime = 0;
            Boolean SetForSpew1 = false;
            Boolean SetForSpew2 = false;
            int SpewCounter1 = 0;
            int SpewCounter2 = 0;
            Point SpewVar1 = new Point(1, 1);
            public override void OnDeserializeDo()
            {
                base.OnDeserializeDo();
                LastTime = Environment.TickCount;
            }
            public override void Update()
            {
                base.Update();
                if (Shell.DeleteQueue.Contains(this)) { foreach (WorldEntity E in Dependents) { Shell.DeleteQueue.Add(E); } }
                ArrayList CleanupR = new ArrayList();
                foreach(WorldEntity D in Dependents)
                {
                    if(!(Shell.UpdateQueue.Contains(D) || Shell.RenderQueue.Contains(D))) { CleanupR.Add(D); }
                }
                foreach(WorldEntity C in CleanupR) { Dependents.Remove(C); }
                CleanupR = new ArrayList();
                int LocalTimeElapsed = Environment.TickCount - LastTime;
                if (ButtonScripts.Paused) { LocalTimeElapsed = 0; }
                MyTime += LocalTimeElapsed;
                if (ButtonScripts.Paused)
                {
                    LastTime = Environment.TickCount;
                    return;
                }
                if(MyTime > 300000000) { MyTime = 0; }
                if(States.Contains("FLOATING"))
                {
                    if(AdjustSin == -100d)
                    {
                        AdjustSin = (MyTime / 1000f) - (Math.Floor((MyTime / 1000f) / (Math.PI * 2)) * (Math.PI * 2));
                    }
                    Move(new Vector2(0, (float)(Math.Sin(MyTime / 1000f - AdjustSin) - Math.Sin((MyTime / 1000f - AdjustSin) - 60))*0.75f));
                }
                if(States.Contains("GLOW"))
                {
                    if(MyMask == null)
                    {
                        MyMask = new EssenseGlow("BIGSOFIA_CHILD_ESSENCEGLOW", pDrawCoords + new Vector2(35, 100), (TAtlasInfo)Shell.AtlasDirectory["BIGJUDGINGGLOW"], LayerDepth + 0.1f, Name);
                        Dependents.Add(MyMask);
                        Shell.RunQueue.Add(new VoidDel(delegate ()
                        {
                            Shell.UpdateQueue.Add(MyMask);
                            Shell.RenderQueue.Add(MyMask);
                        }
                        ));
                    }
                    else
                    {
                        MyMask.QuickMoveTo(pDrawCoords + new Vector2(35, 100));
                    }
                }
                else if(MyMask != null)
                {
                    Shell.DeleteQueue.Add(MyMask);
                    MyMask = null;
                }
                if (States.Contains("SHIFTER"))
                {
                    if(Shell.Rnd.Next(0,20) == 0)
                    {
                        int AY = Shell.Rnd.Next(0, 3);
                        int AX = Shell.Rnd.Next(0, 4);
                        if (AY == 2)
                        {
                            AX = Shell.Rnd.Next(0, 2);
                        }
                        AtlasCoordinates = new Point(AX, AY);
                        if(MyMask != null && AtlasCoordinates != new Point(0, 0)) { MyMask.Drawable = false; }
                        else if (MyMask != null) { MyMask.Drawable = true; }
                    }
                }
                else { if (MyMask != null) { MyMask.Drawable = true; } }
                if (States.Contains("SPEW1"))
                {
                    if(MyTime % SpewFreqencyOne < 1000)
                    {
                        SetForSpew1 = true;
                        if (Shell.Rnd.Next(0, 2) == 1) { SpewVar1.X = 1; }
                        else { SpewVar1.X = -1; }
                        if (Shell.Rnd.Next(0, 2) == 1) { SpewVar1.Y = 1; }
                        else { SpewVar1.Y = -1; }
                    }
                    if(SetForSpew1 && (MyTime % SpewFreqencyOne) - 1000 > 0)
                    {
                        if(SpewCounter1 < Math.Floor(((MyTime % SpewFreqencyOne) - 1000)/50f) + 1)
                        {
                            Transient Add = new Transient("BIGSOFIA_CHILD_SPEW", pDrawCoords + new Vector2(0, -80), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], LayerDepth - 0.1f + (0.001f*SpewCounter1));
                            Dependents.Add(Add);
                            Add.Lifespan = 1500;
                            Add.CenterOrigin = true;
                            double SinNum = ((Math.PI / 6d) * SpewCounter1);
                            Vector2 Trajectory = new Vector2((float)Math.Sin(SinNum) * 1000f * SpewVar1.X, (float)Math.Cos(SinNum) * 1000f * SpewVar1.Y);
                            Animation A = new Animation("BIGSOFIASPEW");
                            Animation B = new Animation("BIGSOFIASPEW_TURN");
                            SortedList Moves = Animation.CreateVectorTween(Trajectory, 1000, 20);
                            A.WriteMovement(Moves);
                            SortedList Rots = Animation.CreateFloatTween(Shell.Rnd.Next(-10, 10) / 10f, 2000, 20);
                            B.WriteRotation(Rots);
                            A.Loop = true;
                            B.Loop = true;
                            Add.AnimationQueue.Add(A);
                            Add.AnimationQueue.Add(B);
                            int AY = Shell.Rnd.Next(0, 3);
                            int AX = Shell.Rnd.Next(0, 4);
                            if (AY == 2)
                            {
                                AX = Shell.Rnd.Next(0, 2);
                                if(Shell.Rnd.Next(0, 20) == 0) { AX = 2; }
                            }
                            Add.SetAtlasFrame(new Point(AX, AY));
                            Add.Scale(new Vector2(-0.6f, -0.6f));

                            Shell.RunQueue.Add(new VoidDel(delegate ()
                            {
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                            SpewCounter1++;
                            if(SpewCounter1 >= 12)
                            {
                                SpewCounter1 = 0;
                                SetForSpew1 = false;
                            }
                        }
                    }
                }
                else { SetForSpew1 = false; }
                if(States.Contains("SPEW2"))
                {
                    if (MyTime % SpewFreqencyTwo < 1000)
                    {
                        SetForSpew2 = true;
                    }
                    if (SetForSpew2 && (MyTime % SpewFreqencyTwo) - 1000 > 0)
                    {
                        if (SpewCounter2 < Math.Floor(((MyTime % SpewFreqencyTwo) - 1000) / 300f) + 1)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                Transient Add = new Transient("BIGSOFIA_CHILD_SPEW", pDrawCoords + new Vector2(0,-70), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], LayerDepth - 0.1f + (0.001f * SpewCounter1));
                                Dependents.Add(Add);
                                Add.Lifespan = 1500;
                                Add.CenterOrigin = true;
                                double SinNum = ((Math.PI / 6d) * i);
                                Vector2 Trajectory = new Vector2((float)Math.Sin(SinNum) * 1000f, (float)Math.Cos(SinNum) * 1000f);
                                Animation A = new Animation("BIGSOFIASPEW");
                                Animation B = new Animation("BIGSOFIASPEW_TURN");
                                SortedList Moves = Animation.CreateVectorTween(Trajectory, 1000, 20);
                                A.WriteMovement(Moves);
                                SortedList Rots = Animation.CreateFloatTween(Shell.Rnd.Next(-10, 10) / 10f, 2000, 20);
                                B.WriteRotation(Rots);
                                A.Loop = true;
                                B.Loop = true;
                                Add.AnimationQueue.Add(A);
                                Add.AnimationQueue.Add(B);
                                int AY = Shell.Rnd.Next(0, 3);
                                int AX = Shell.Rnd.Next(0, 4);
                                if (AY == 2)
                                {
                                    AX = Shell.Rnd.Next(0, 2);
                                    if (Shell.Rnd.Next(0, 20) == 0) { AX = 2; }
                                }
                                Add.SetAtlasFrame(new Point(AX, AY));
                                Add.Scale(new Vector2(-0.6f, -0.6f));

                                Shell.RunQueue.Add(new VoidDel(delegate ()
                                {
                                    Shell.UpdateQueue.Add(Add);
                                    Shell.RenderQueue.Add(Add);
                                }));
                            }
                            SpewCounter2++;
                            if (SpewCounter2 >= SpewWavesTwo)
                            {
                                SpewCounter2 = 0;
                                SetForSpew2 = false;
                            }
                        }
                    }
                }
                else { SetForSpew2 = false; }
                LastTime = Environment.TickCount;
            }
        }
    }
}