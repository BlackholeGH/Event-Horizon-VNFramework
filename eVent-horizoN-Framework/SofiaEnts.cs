using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        public static void SofiaReturnScript()
        {
            WorldEntity ourSofia = Shell.GetEntityByName("SOFIA");
            ourSofia.AnimationQueue.Clear();
            Animation freshTween = new Animation("sofia_return_tween");
            SortedList<int, Vector2> TweenFrames = Animation.CreateVectorTween(new Vector2(0, 405 - ourSofia.DrawCoords.Y), 1000, 20);
            freshTween.WriteMovement(TweenFrames);
            ourSofia.AnimationQueue.Add(freshTween);
        }
        public static void EndQuakes()
        {
            foreach (WorldEntity worldEntity in Shell.RenderQueue)
            {
                foreach (Animation animation in worldEntity.AnimationQueue)
                {
                    if (animation.AnimName == "lastingquake" || animation.AnimName == "shakequake") { animation.Jump(worldEntity); }
                }
            }
        }
        public static Vector2 EndTextPosition()
        {
            String textContent = System.DateTime.Now.DayOfWeek.ToString().ToUpper() + ". GILLETTE RESIDENCE. " + System.DateTime.Now.ToString("hh:mm tt") + "...";
            return new Vector2(640 - (Shell.Default.MeasureString(textContent).X / 2), 500);
        }
        public static int KingFlag
        {
            get { return (int)Shell.ReadFlag("KING"); }
            set { Shell.UpdateFlag("KING", value); }
        }
        public static int CrookedFlag
        {
            get { return (int)Shell.ReadFlag("CROOKED"); }
            set { Shell.UpdateFlag("CROOKED", value); }
        }
        public static int ParanoidFlag
        {
            get { return (int)Shell.ReadFlag("PARANOID"); }
            set { Shell.UpdateFlag("PARANOID", value); }
        }
        public static int CrookedKnowledgeFlag
        {
            get { return (int)Shell.ReadFlag("KNOWLEDGE"); }
            set { Shell.UpdateFlag("KNOWLEDGE", value); }
        }
        public static int MysticFlag
        {
            get { return (int)Shell.ReadFlag("MYSTIC"); }
            set { Shell.UpdateFlag("MYSTIC", value); }
        }
        public static int StoryFlag
        {
            get { return (int)Shell.ReadFlag("STORY"); }
            set { Shell.UpdateFlag("STORY", value); }
        }
        public static String GetContextualLocation()
        {
            String scriptName = ScriptProcessor.SnifferSearch().Name.ToUpper();
            if (scriptName.Contains("SOFIA_KING")) { return "KING"; }
            else if (scriptName.Contains("SOFIA_CROOKED")) { return "CROOKED"; }
            else if (scriptName.Contains("SOFIA_MYSTIC")) { return "MYSTIC"; }
            return "NULL";
        }
        public static String GetExitThrowLocation(String name, String[] shiftCondition)
        {
            String exitScript = "";
            switch (name)
            {
                case "SOFIA_KING_PRIMARY_SNIFFER":
                    exitScript = "EXIT_KING_PRIMARY";
                    break;
                case "SOFIA_CROOKED_PRIMARY_FINALIZE_SNIFFER":
                    exitScript = "EXIT_CROOKED_PRIMARY_FINALIZE";
                    break;
                default:
                    if (shiftCondition.Contains<String>("GWS:impos_mapnavigate")) { exitScript = "EXIT_DEFAULT_PROPER"; }
                    else
                    {
                        WorldEntity PotentialKing = Shell.GetEntityByName("KING SOFIA");
                        WorldEntity PotentialMystic = Shell.GetEntityByName("MYSTIC SOFIA");
                        WorldEntity PotentialCrooked = Shell.GetEntityByName("CROOKED SOFIA");
                        if (PotentialKing != null && PotentialKing.ColourValue.A >= 254f && PotentialKing.DrawCoords.X < 1280 && PotentialKing.DrawCoords.X > 0)
                        {
                            exitScript = "EXIT_DEFAULT_KING";
                        }
                        else if (PotentialMystic != null && PotentialMystic.ColourValue.A >= 254f && PotentialMystic.DrawCoords.X < 1280 && PotentialMystic.DrawCoords.X > 0)
                        {
                            exitScript = "EXIT_DEFAULT_MYSTIC";
                        }
                        else if (PotentialCrooked != null && PotentialCrooked.ColourValue.A >= 254f && PotentialCrooked.DrawCoords.X < 1280 && PotentialCrooked.DrawCoords.X > 0)
                        {
                            exitScript = "EXIT_DEFAULT_CROOKED";
                        }
                        else { exitScript = "EXIT_DEFAULT"; }
                    }
                    break;
            }
            return exitScript;
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
                if (CrookedKnowledgeFlag == 0) { return "SOFIA_CROOKED_RETURN_NO_KNOWLEDGE"; } //Done
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
            int _lastFireCountdown = 1;
            public override void Update()
            {
                if(Shell.Rnd.Next(0, 2) == 0 && !Cease)
                {
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        UpwardParticle add = new UpwardParticle("PURPLESPRITE", new Vector2(Shell.Rnd.Next(140, 1140), Shell.Rnd.Next(200, 600)), (TAtlasInfo)Shell.AtlasDirectory["PURPLE"], 0.4f);
                        add.CenterOrigin = true;
                        add.ColourValue = new Color(0, 0, 0, 0);
                        add.AnimationQueue.Add(Animation.Retrieve("FADEINOUT"));
                        add.TransientAnimation = true;
                        Shell.UpdateQueue.Add(add);
                        Shell.RenderQueue.Add(add);
                    }));
                }
                _lastFireCountdown--;
                if(_lastFireCountdown < 1) { _lastFireCountdown = 1; }
                if (Shell.Rnd.Next(0, _lastFireCountdown) == 0 && !Cease)
                {
                    _lastFireCountdown = 400;
                    Shell.RunQueue.Add(new VoidDel(delegate ()
                    {
                        Double degree = Shell.Rnd.NextDouble() * (2*Math.PI);
                        Vector2 spawnPos = (new Vector2((float)Math.Sin(degree), (float)Math.Cos(degree))*1000f) + new Vector2(640, 360);
                        Transient add = new Transient("MEME", spawnPos, (TAtlasInfo)Shell.AtlasDirectory["MEMES"], ((float)Shell.Rnd.NextDouble() * 0.09f) + 0.405f);
                        add.CenterOrigin = true;
                        Vector2 target = new Vector2(Shell.Rnd.Next(400, 881), Shell.Rnd.Next(300, 420));
                        Vector2 trajectory = (target - add.DrawCoords) * 2;
                        Animation a = new Animation("MEMEFLOATER");
                        Animation b = new Animation("MEMEFLOATER_TURN");
                        SortedList<int, Vector2> moves = Animation.CreateVectorTween(trajectory, 7000, 33);
                        a.WriteMovement(moves);
                        SortedList<int, float> rots = Animation.CreateFloatTween(Shell.Rnd.Next(-10, 10) / 10f, 1000, 33);
                        b.WriteRotation(rots);
                        a.Loop = true;
                        b.Loop = true;
                        add.AnimationQueue.Add(a);
                        add.AnimationQueue.Add(b);
                        add.SetAtlasFrame(new Point(Shell.Rnd.Next(0, 4), Shell.Rnd.Next(0, 4)));
                        add.Scale(new Vector2(-0.5f, -0.5f));
                        Shell.UpdateQueue.Add(add);
                        Shell.RenderQueue.Add(add);
                    }));
                }
                base.Update();
            }
        }
        public class UpwardParticle : WorldEntity
        {
            public UpwardParticle(String name, Vector2 location, TAtlasInfo? atlas, float depth) : base(name, location, atlas, depth)
            {

            }
            int xVelocity = 0;
            public override void Update()
            {
                xVelocity += Shell.Rnd.Next(-1, 2);
                if (xVelocity > 5) { xVelocity = 5; }
                if (xVelocity < -5) { xVelocity = -5; }
                this.Move(new Vector2(xVelocity, -2));
                base.Update();
            }
        }
        [Serializable]
        public class Transient : WorldEntity
        {
            public Transient(String name, Vector2 location, TAtlasInfo? atlas, float depth) : base(name, location, atlas, depth)
            {
                _startTime = Environment.TickCount;
                Lifespan = 8000;
            }
            public int Lifespan { get; set; }
            int _startTime;
            int _pausedTimeElapsed = -1;
            public override void OnSerializeDo()
            {
                base.OnSerializeDo();
                _pausedTimeElapsed = Environment.TickCount - _startTime;
            }
            public override void OnDeserializeDo()
            {
                base.OnDeserializeDo();
                _startTime = Environment.TickCount - _pausedTimeElapsed;
                _pausedTimeElapsed = -1;
            }
            public override void Update()
            {
                base.Update();
                if (ButtonScripts.Paused)
                {
                    if(_pausedTimeElapsed == -1) { _pausedTimeElapsed = Environment.TickCount - _startTime; }
                    return;
                }
                else if (_pausedTimeElapsed >= 0)
                {
                    _startTime = Environment.TickCount - _pausedTimeElapsed;
                    _pausedTimeElapsed = -1;
                }
                if(Environment.TickCount - _startTime > Lifespan)
                {
                    Shell.DeleteQueue.Add(this);
                    ManualDispose();
                }
            }
        }
        [Serializable]
        public class EssenseGlow : WorldEntity
        {
            WorldEntity sofiaE = null;
            private Vector2 _scDist = new Vector2(10, 255);
            public EssenseGlow(String name, Vector2 location, TAtlasInfo? atlas, float depth, String alternateParentName) : base(name, location, atlas, depth)
            {
                Origin = (VNFUtils.ConvertPoint(Hitbox.Size) / 2) + _scDist;
                FindSofia(alternateParentName);
            }
            public EssenseGlow(String name, Vector2 location, TAtlasInfo? atlas, float depth) : base(name, location, atlas, depth)
            {
                Origin = (VNFUtils.ConvertPoint(Hitbox.Size) / 2) + _scDist;
                FindSofia();
            }
            private void FindSofia()
            {
                FindSofia("SOFIA");
            }
            private void FindSofia(String searchName)
            {
                foreach (WorldEntity worldEntity in Shell.UpdateQueue)
                {
                    if (worldEntity.Name.ToUpper() == searchName.ToUpper())
                    {
                        sofiaE = worldEntity;
                        break;
                    }
                }
            }
            public override void OnDeserializeDo()
            {
                sofiaE = null;
                base.OnDeserializeDo();
            }
            private void Mirror(WorldEntity worldEntity)
            {
                Size = worldEntity.Size;
                if (worldEntity is BigSofia) { QuickMoveTo(worldEntity.DrawCoords + new Vector2(35, 100)); }
                else { QuickMoveTo(worldEntity.DrawCoords); }
            }
            public override void Update()
            {
                Color rainbow = TextEntity.GetRainbowColour();
                float fade = ColourValue.A / 255f;
                ColourValue = new ColourShift(rainbow.R * fade, rainbow.G * fade, rainbow.B * fade, ColourValue.A).AsColour();
                if(sofiaE != null) { Mirror(sofiaE); }
                else { FindSofia(); }
                base.Update();
            }
        }
        [Serializable]
        public class SofiaBoomer : WorldEntity
        {
            int _ticks = 0;
            int _number = 0;
            int _interval = 0;
            public SofiaBoomer(String name, int myNumber, int myInterval) : base(name, new Vector2(), null, 0f)
            {
                _number = myNumber;
                _interval = myInterval;
                _ticks = Environment.TickCount;
            }
            public override void Update()
            {
                if(ScriptProcessor.SnifferSearch().Skipping) { _number = 0; }
                if(_number > 0)
                {
                    if(Environment.TickCount - _ticks > _interval)
                    {
                        ScriptProcessor.ActivateScriptElement("S|DEEPBOOM");
                        foreach (WorldEntity worldEntity in Shell.RenderQueue)
                        {
                            if (ButtonScripts.DefaultUINames.Contains(worldEntity.Name) || worldEntity is TextEntity) { continue; }
                            worldEntity.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                        }
                        _number--;
                        _ticks = Environment.TickCount;
                    }
                }
                else { Shell.DeleteQueue.Add(this); }
                base.Update();
            }
        }
        [Serializable]
        public class RuneGlow : WorldEntity
        {
            private int _spaceAdjust = 0;
            public double GetMySinable
            {
                get
                {
                    int ticks = Environment.TickCount;
                    int tickMod = (ticks - _spaceAdjust) % 10000;
                    return (((double)tickMod / 10000d) * 2d * Math.PI);
                }
            }
            private Boolean _nodraw = false;
            public RuneGlow(String name, Vector2 location, TAtlasInfo? atlas, float depth) : base(name, location, atlas, depth)
            {
                ColourValue = new ColourShift(0f, 0f, 0f, 0f).AsColour();
                _spaceAdjust = Environment.TickCount % 10000;
                CenterOrigin = false;
                foreach(WorldEntity worldEntity in Shell.UpdateQueue)
                {
                    if(worldEntity is RuneGlow && !(worldEntity == this))
                    {
                        _nodraw = true;
                        Shell.DeleteQueue.Add(this);
                    }
                }
            }
            public override void Update()
            {
                ColourValue = (new ColourShift(255f, 255f, 255f, 255f) * (float)((-Math.Cos(GetMySinable) / 2d) + 0.5)).AsColour();
                base.Update();
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                if(_nodraw) { return; }
                base.Draw(spriteBatch);
            }
        }
        [Serializable]
        public class SourceGlow : WorldEntity
        {
            public SourceGlow(String name, Vector2 location, TAtlasInfo? atlas, float depth) : base(name, location, atlas, depth)
            {

            }
            public override void Update()
            {
                Color rainbow = TextEntity.GetRainbowColour();
                float fade = (ColourValue.A / 255f);
                ColourValue = new ColourShift(((rainbow.R * 0.4f) + (255f * 0.6f)) * fade, ((rainbow.G * 0.4f) + (255f * 0.6f)) * fade, ((rainbow.B * 0.4f) + (255f * 0.6f)) * fade, ColourValue.A).AsColour();
                base.Update();
            }
        }
        [Serializable]
        public class BigSofia : WorldEntity
        {
            public BigSofia(String name, Vector2 location, TAtlasInfo? atlas, float depth, ArrayList initialStates) : base(name, location, atlas, depth)
            {
                AtlasCoordinates = new Point(3, 2);
                _basePosition = location;
                States = initialStates;
                SpewFreqencyOne = 2000;
                SpewIntervalOne = 50;
                SpewFreqencyTwo = 2000;
                SpewWavesTwo = 3;
            }
            ~BigSofia()
            {
                foreach(WorldEntity worldEntity in _dependents) { Shell.DeleteQueue.Add(worldEntity); }
            }
            private double _adjustSin = -100d;
            private Vector2 _basePosition = new Vector2();
            public void ResetBasePosition()
            {
                _basePosition = DrawCoords;
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
            private List<WorldEntity> _dependents = new List<WorldEntity>();
            private EssenseGlow _myMask = null;
            int _lastTime = Environment.TickCount;
            int _myTime = 0;
            Boolean _setForSpew1 = false;
            Boolean _setForSpew2 = false;
            int _spewCounter1 = 0;
            int _spewCounter2 = 0;
            Point _spewVar1 = new Point(1, 1);
            public override void OnDeserializeDo()
            {
                base.OnDeserializeDo();
                _lastTime = Environment.TickCount;
            }
            public override void Update()
            {
                base.Update();
                if (Shell.DeleteQueue.Contains(this)) { foreach (WorldEntity E in _dependents) { Shell.DeleteQueue.Add(E); } }
                ArrayList cleanupR = new ArrayList();
                foreach(WorldEntity dependentWE in _dependents)
                {
                    if(!(Shell.UpdateQueue.Contains(dependentWE) || Shell.RenderQueue.Contains(dependentWE))) { cleanupR.Add(dependentWE); }
                }
                foreach(WorldEntity cleanupWE in cleanupR) { _dependents.Remove(cleanupWE); }
                cleanupR = new ArrayList();
                int localTimeElapsed = Environment.TickCount - _lastTime;
                if (ButtonScripts.Paused) { localTimeElapsed = 0; }
                _myTime += localTimeElapsed;
                if (ButtonScripts.Paused)
                {
                    _lastTime = Environment.TickCount;
                    return;
                }
                if(_myTime > 300000000) { _myTime = 0; }
                if(States.Contains("FLOATING"))
                {
                    if(_adjustSin == -100d)
                    {
                        _adjustSin = (_myTime / 1000f) - (Math.Floor((_myTime / 1000f) / (Math.PI * 2)) * (Math.PI * 2));
                    }
                    Move(new Vector2(0, (float)(Math.Sin(_myTime / 1000f - _adjustSin) - Math.Sin((_myTime / 1000f - _adjustSin) - 60))*0.75f));
                }
                if(States.Contains("GLOW"))
                {
                    if(_myMask == null)
                    {
                        _myMask = new EssenseGlow("BIGSOFIA_CHILD_ESSENCEGLOW", DrawCoords + new Vector2(35, 100), (TAtlasInfo)Shell.AtlasDirectory["BIGJUDGINGGLOW"], LayerDepth + 0.1f, Name);
                        _dependents.Add(_myMask);
                        Shell.RunQueue.Add(new VoidDel(delegate ()
                        {
                            Shell.UpdateQueue.Add(_myMask);
                            Shell.RenderQueue.Add(_myMask);
                        }
                        ));
                    }
                    else
                    {
                        _myMask.QuickMoveTo(DrawCoords + new Vector2(35, 100));
                    }
                }
                else if(_myMask != null)
                {
                    Shell.DeleteQueue.Add(_myMask);
                    _myMask = null;
                }
                if (States.Contains("SHIFTER"))
                {
                    if(Shell.Rnd.Next(0,20) == 0)
                    {
                        int aY = Shell.Rnd.Next(0, 3);
                        int aX = Shell.Rnd.Next(0, 4);
                        if (aY == 2)
                        {
                            aX = Shell.Rnd.Next(0, 2);
                        }
                        AtlasCoordinates = new Point(aX, aY);
                        if(_myMask != null && AtlasCoordinates != new Point(0, 0)) { _myMask.Drawable = false; }
                        else if (_myMask != null) { _myMask.Drawable = true; }
                    }
                }
                else { if (_myMask != null) { _myMask.Drawable = true; } }
                if (States.Contains("SPEW1"))
                {
                    if(_myTime % SpewFreqencyOne < 1000)
                    {
                        _setForSpew1 = true;
                        if (Shell.Rnd.Next(0, 2) == 1) { _spewVar1.X = 1; }
                        else { _spewVar1.X = -1; }
                        if (Shell.Rnd.Next(0, 2) == 1) { _spewVar1.Y = 1; }
                        else { _spewVar1.Y = -1; }
                    }
                    if(_setForSpew1 && (_myTime % SpewFreqencyOne) - 1000 > 0)
                    {
                        if(_spewCounter1 < Math.Floor(((_myTime % SpewFreqencyOne) - 1000)/50f) + 1)
                        {
                            Transient add = new Transient("BIGSOFIA_CHILD_SPEW", DrawCoords + new Vector2(0, -80), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], LayerDepth - 0.1f + (0.001f*_spewCounter1));
                            _dependents.Add(add);
                            add.Lifespan = 1500;
                            add.CenterOrigin = true;
                            double sinNum = ((Math.PI / 6d) * _spewCounter1);
                            Vector2 Trajectory = new Vector2((float)Math.Sin(sinNum) * 1000f * _spewVar1.X, (float)Math.Cos(sinNum) * 1000f * _spewVar1.Y);
                            Animation a = new Animation("BIGSOFIASPEW");
                            Animation b = new Animation("BIGSOFIASPEW_TURN");
                            SortedList<int, Vector2> moves = Animation.CreateVectorTween(Trajectory, 1000, 20);
                            a.WriteMovement(moves);
                            SortedList<int, float> rots = Animation.CreateFloatTween(Shell.Rnd.Next(-10, 10) / 10f, 2000, 20);
                            b.WriteRotation(rots);
                            a.Loop = true;
                            b.Loop = true;
                            add.AnimationQueue.Add(a);
                            add.AnimationQueue.Add(b);
                            int aY = Shell.Rnd.Next(0, 3);
                            int aX = Shell.Rnd.Next(0, 4);
                            if (aY == 2)
                            {
                                aX = Shell.Rnd.Next(0, 2);
                                if(Shell.Rnd.Next(0, 20) == 0) { aX = 2; }
                            }
                            add.SetAtlasFrame(new Point(aX, aY));
                            add.Scale(new Vector2(-0.6f, -0.6f));

                            Shell.RunQueue.Add(new VoidDel(delegate ()
                            {
                                Shell.UpdateQueue.Add(add);
                                Shell.RenderQueue.Add(add);
                            }));
                            _spewCounter1++;
                            if(_spewCounter1 >= 12)
                            {
                                _spewCounter1 = 0;
                                _setForSpew1 = false;
                            }
                        }
                    }
                }
                else { _setForSpew1 = false; }
                if(States.Contains("SPEW2"))
                {
                    if (_myTime % SpewFreqencyTwo < 1000)
                    {
                        _setForSpew2 = true;
                    }
                    if (_setForSpew2 && (_myTime % SpewFreqencyTwo) - 1000 > 0)
                    {
                        if (_spewCounter2 < Math.Floor(((_myTime % SpewFreqencyTwo) - 1000) / 300f) + 1)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                Transient add = new Transient("BIGSOFIA_CHILD_SPEW", DrawCoords + new Vector2(0,-70), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], LayerDepth - 0.1f + (0.001f * _spewCounter1));
                                _dependents.Add(add);
                                add.Lifespan = 1500;
                                add.CenterOrigin = true;
                                double sinNum = ((Math.PI / 6d) * i);
                                Vector2 trajectory = new Vector2((float)Math.Sin(sinNum) * 1000f, (float)Math.Cos(sinNum) * 1000f);
                                Animation a = new Animation("BIGSOFIASPEW");
                                Animation b = new Animation("BIGSOFIASPEW_TURN");
                                SortedList<int, Vector2> moves = Animation.CreateVectorTween(trajectory, 1000, 20);
                                a.WriteMovement(moves);
                                SortedList<int, float> rots = Animation.CreateFloatTween(Shell.Rnd.Next(-10, 10) / 10f, 2000, 20);
                                b.WriteRotation(rots);
                                a.Loop = true;
                                b.Loop = true;
                                add.AnimationQueue.Add(a);
                                add.AnimationQueue.Add(b);
                                int aY = Shell.Rnd.Next(0, 3);
                                int aX = Shell.Rnd.Next(0, 4);
                                if (aY == 2)
                                {
                                    aX = Shell.Rnd.Next(0, 2);
                                    if (Shell.Rnd.Next(0, 20) == 0) { aX = 2; }
                                }
                                add.SetAtlasFrame(new Point(aX, aY));
                                add.Scale(new Vector2(-0.6f, -0.6f));

                                Shell.RunQueue.Add(new VoidDel(delegate ()
                                {
                                    Shell.UpdateQueue.Add(add);
                                    Shell.RenderQueue.Add(add);
                                }));
                            }
                            _spewCounter2++;
                            if (_spewCounter2 >= SpewWavesTwo)
                            {
                                _spewCounter2 = 0;
                                _setForSpew2 = false;
                            }
                        }
                    }
                }
                else { _setForSpew2 = false; }
                _lastTime = Environment.TickCount;
            }
        }
    }
}