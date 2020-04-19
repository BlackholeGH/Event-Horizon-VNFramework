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

namespace VNFramework
{
    partial class Animation
    {
        public static Animation Retrieve(String Name)
        {
            Animation Out = new Animation("null");
            Animation Temp;
            SortedList BFrames;
            SortedList TempFrames;
            int Velocity;
            int Accel;
            int Frame;
            switch (Name.ToUpper())
            {
                case "BOUNCE_1":
                    Temp = new Animation("bounce_1");
                    BFrames = new SortedList();
                    Velocity = -10;
                    Accel = 1;
                    Frame = 0;
                    do
                    {
                        BFrames.Add(Frame, new Vector2(0, (float)Velocity));
                        Frame += 20;
                        Velocity += Accel;
                    }
                    while (Velocity <= 10);
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "BOUNCE_2":
                    Temp = new Animation("bounce_2");
                    BFrames = new SortedList();
                    Velocity = -10;
                    Accel = 1;
                    Frame = 0;
                    do
                    {
                        BFrames.Add(Frame, new Vector2(0, (float)Velocity));
                        Frame += 20;
                        Velocity += Accel;
                    }
                    while (Velocity <= 10);
                    Velocity = -6;
                    do
                    {
                        BFrames.Add(Frame, new Vector2(0, (float)Velocity));
                        Frame += 20;
                        Velocity += Accel;
                    }
                    while (Velocity <= 6);
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "BOUNCE_3":
                    Temp = new Animation("bounce_3");
                    BFrames = new SortedList();
                    Velocity = -5;
                    Accel = 1;
                    Frame = 0;
                    do
                    {
                        BFrames.Add(Frame, new Vector2(0, (float)Velocity));
                        Frame += 20;
                        Velocity += Accel;
                    }
                    while (Velocity <= 5);
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "FALLSHOCK":
                    Temp = new Animation("fallshock");
                    BFrames = Animation.CreateVectorTween(new Vector2(0, -20), 1500, 20);
                    TempFrames = Animation.CreateVectorTween(new Vector2(0, 200), 300, 20);
                    BFrames = Animation.MergeFrames(BFrames, TempFrames);
                    BFrames.Add(2500, new Vector2(0, 0));
                    TempFrames = Animation.CreateVectorTween(new Vector2(0, -180), 700, 20);
                    BFrames = Animation.MergeFrames(BFrames, TempFrames);
                    Temp.WriteMovement(BFrames);
                    BFrames = new SortedList();
                    BFrames.Add(0, new Point(1, 1));
                    BFrames.Add(1800, new Point(3, 0));
                    Temp.WriteFrames(BFrames);
                    Out = Temp;
                    break;
                case "SHAKEMINOR":
                    Temp = new Animation("shakeminor");
                    BFrames = new SortedList();
                    Velocity = -4;
                    Accel = 1;
                    Frame = 0;
                    int FCount = 0;
                    do
                    {
                        BFrames.Add(Frame, new Vector2((float)Velocity, 0));
                        Frame += 20;
                        Velocity += Accel;
                        if(Velocity == -4 || Velocity == 4)
                        {
                            FCount++;
                            Accel = Velocity / -4;
                        }
                    }
                    while (FCount <= 4);
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "SHAKEMAJOR":
                    Temp = new Animation("shakemajor");
                    BFrames = new SortedList();
                    float FVelocity = -40;
                    float FAccel = 20;
                    Frame = 0;
                    float TrueX = 0;
                    Boolean CenterPass = false;
                    do
                    {
                        CenterPass = false;
                        BFrames.Add(Frame, new Vector2((float)FVelocity, 0));
                        TrueX += FVelocity;
                        Frame += 20;
                        FVelocity += FAccel;
                        if (TrueX * FAccel > 0)
                        {
                            FVelocity = 0.5f * FVelocity;
                            FAccel = -FAccel * 0.9f;
                            CenterPass = true;
                        }
                    }
                    while (Frame < 5000 && (Math.Sqrt(FVelocity*FVelocity) > 4 || !CenterPass));
                    BFrames.Add(Frame, new Vector2(-TrueX, 0));
                    BFrames.Add(Frame + 1000, new Vector2(0, 0));
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "SHAKEQUAKE":
                    Temp = new Animation("shakequake");
                    BFrames = new SortedList();
                    Frame = 0;
                    float LastX = 0;
                    float Mult = 1;
                    float NowVal = 0;
                    do
                    {
                        NowVal = (float)Math.Sin((((float)Frame % 1000f) / 1000f) * Math.PI * 16f) * Mult;
                        Mult = 100 - (Frame / 10f);
                        BFrames.Add(Frame, new Vector2(NowVal - LastX, 0));
                        LastX = NowVal;
                        Frame += 20;
                    }
                    while (Frame < 5000 && Mult > 1);
                    BFrames.Add(Frame, new Vector2(-NowVal, 0));
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "LASTINGQUAKE":
                    Temp = new Animation("lastingquake");
                    BFrames = new SortedList();
                    Frame = 0;
                    LastX = 0;
                    Mult = 1;
                    NowVal = 0;
                    do
                    {
                        NowVal = (float)Math.Sin((((float)Frame % 1000f) / 1000f) * Math.PI * 16f) * Mult;
                        Mult = Shell.Rnd.Next(5, 20);
                        BFrames.Add(Frame, new Vector2(NowVal - LastX, 0));
                        LastX = NowVal;
                        Frame += 20;
                    }
                    while (Frame < 5000 && Mult > 1);
                    BFrames.Add(Frame, new Vector2(-NowVal, 0));
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "SPIRALOUT":
                    Temp = new Animation("spiralout");
                    BFrames = new SortedList();
                    SortedList MyScaleFrames = new SortedList();
                    SortedList MyRotFrames = new SortedList();
                    Frame = 0;
                    Vector2 LastVec = new Vector2();
                    Vector2 NowVec = new Vector2();
                    float ProgVal = 0f;
                    float ScaleAmount = 0f;
                    do
                    {
                        ProgVal = (float)((((float)Frame % 1000f) / 1000f) * Math.PI * 2f);
                        NowVec = new Vector2((float)Math.Cos((double)ProgVal), (float)Math.Sin((double)ProgVal)) * (300f * (Frame/6500f));
                        BFrames.Add(Frame, NowVec - LastVec);
                        ScaleAmount = ((Frame / 200f) * (Frame / 200f)) / 10000f;
                        MyScaleFrames.Add(Frame, new Vector2(ScaleAmount, ScaleAmount));
                        MyRotFrames.Add(Frame, 0.01f);
                        LastVec = NowVec;
                        Frame += 20;
                    }
                    while (Frame < 6500);
                    Temp.WriteMovement(BFrames);
                    Temp.WriteScaling(MyScaleFrames);
                    Temp.WriteRotation(MyRotFrames);
                    Out = Temp;
                    break;
                case "BLINKIN":
                    Temp = new Animation("blinkin");
                    BFrames = new SortedList();
                    BFrames.Add(20, new ColourShift(255, 255, 255, 255));
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "BLINKOUT":
                    Temp = new Animation("blinkout");
                    BFrames = new SortedList();
                    BFrames.Add(20, new ColourShift(-255, -255, -255, -255));
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FLASH1":
                    Temp = new Animation("flash1");
                    BFrames = new SortedList();
                    BFrames.Add(2700, new ColourShift(255, 255, 255, 255));
                    BFrames.Add(2800, new ColourShift(-255, -255, -255, -255));
                    BFrames.Add(2900, new ColourShift(255, 255, 255, 255));
                    BFrames.Add(3000, new ColourShift(-255, -255, -255, -255));
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FLASH2":
                    Temp = new Animation("flash2");
                    BFrames = new SortedList();
                    BFrames.Add(2750, new ColourShift(255, 255, 255, 255));
                    BFrames.Add(2950, new ColourShift(-255, -255, -255, -255));
                    BFrames.Add(3000, new ColourShift(0,0,0,0));
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FLOATER":
                    Temp = new Animation("floater");
                    BFrames = new SortedList();
                    Frame = 0;
                    float Last = 0f;
                    do
                    {
                        float Current = (float)Math.Sin((Frame / 4000d) * Math.PI * 2d) * 45;
                        BFrames.Add(Frame, new Vector2(0, Current - Last));
                        Last = Current;
                        Frame += 20;
                    }
                    while (Frame < 4000);
                    Temp.WriteMovement(BFrames);
                    Out = Temp;
                    break;
                case "FADEIN":
                    Temp = new Animation("fadein");
                    BFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 1500, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEINMED":
                    Temp = new Animation("fadeinmed");
                    BFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 2000, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEINLONG":
                    Temp = new Animation("fadeinlong");
                    BFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 3000, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEOUT":
                    Temp = new Animation("fadeout");
                    BFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 1500, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEOUTMED":
                    Temp = new Animation("fadeoutmed");
                    BFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 2000, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEOUTLONG":
                    Temp = new Animation("fadeoutlong");
                    BFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 3000, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEOUTRAPID":
                    Temp = new Animation("fadeoutrapid");
                    BFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 300, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEOUTCOLOURPRESERVE":
                    Temp = new Animation("fadeoutcolourpreserve");
                    BFrames = Animation.CreateColourTween(new ColourShift(0, 0, 0, -255), 2000, 20);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEINOUT":
                    Temp = new Animation("fadeinout");
                    BFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 1500, 20);
                    TempFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 1500, 20);
                    BFrames = Animation.MergeFrames(BFrames, TempFrames);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "FADEINOUTLONG":
                    Temp = new Animation("fadeinoutlong");
                    BFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 5000, 20);
                    TempFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 5000, 20);
                    BFrames = Animation.MergeFrames(BFrames, TempFrames);
                    Temp.WriteColouring(BFrames);
                    Out = Temp;
                    break;
                case "01OSCILLATE":
                    Temp = new Animation("01oscillate");
                    BFrames = new SortedList();
                    BFrames.Add(0, new Point(0, 0));
                    BFrames.Add(50, new Point(1, 0));
                    BFrames.Add(100, new Point(1, 0));
                    Temp.WriteFrames(BFrames);
                    Temp.Loop = true;
                    Out = Temp;
                    break;
                case "SOFIASLIDESHOW":
                    Temp = new Animation("sofiaslideshow");
                    BFrames = new SortedList();
                    int ChangeFrame = 0;
                    for(int ii = 0; ii < 3; ii++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            BFrames.Add(ChangeFrame, new Point(i, ii));
                            ChangeFrame += 600;
                            if(ii == 2 && i == 1)
                            {
                                BFrames.Add(ChangeFrame, new Point(0, 0));
                                break;
                            }
                        }
                    }
                    Temp.WriteFrames(BFrames);
                    Temp.Loop = true;
                    Out = Temp;
                    break;
                case "GOLEMSLIDESHOW":
                    Temp = new Animation("golemslideshow");
                    BFrames = new SortedList();
                    BFrames = new SortedList();
                    BFrames.Add(0, new Point(0, 0));
                    BFrames.Add(600, new Point(1, 0));
                    BFrames.Add(1200, new Point(2, 0));
                    BFrames.Add(1800, new Point(0, 0));
                    Temp.WriteFrames(BFrames);
                    Temp.Loop = true;
                    Out = Temp;
                    break;
                case "SLOWOSCILLATE":
                    Temp = new Animation("slowoscillate");
                    BFrames = new SortedList();
                    for (int i = 0; i < 9; i++)
                    {
                        BFrames.Add(500*i, new Point(Math.Floor(((float)500 * i) /1000f) == ((float)500 * i) / 1000f ? 0 : 1, 0));
                    }
                    Temp.WriteFrames(BFrames);
                    Out = Temp;
                    break;
                case "FOCUSGROW":
                    Temp = new Animation("focusgrow");
                    BFrames = Animation.CreateVectorTween(new Vector2(0.06f, 0.06f), 200, 20);
                    Temp.WriteScaling(BFrames);
                    Out = Temp;
                    break;
                case "FOCUSSHRINK":
                    Temp = new Animation("focusshrink");
                    TempFrames = new SortedList();
                    TempFrames = Animation.CreateVectorTween(new Vector2(0.06f, 0.06f), 200, 20);
                    BFrames = new SortedList();
                    foreach (int K in TempFrames.Keys)
                    {
                        BFrames.Add(K, (Vector2)TempFrames[K] * -1);
                    }
                    Temp.WriteScaling(BFrames);
                    Out = Temp;
                    break;
                case "WIGGLE":
                    Temp = new Animation("wiggle");
                    BFrames = Animation.CreateFloatTween(0.1f, 1000, 20);
                    TempFrames = Animation.CreateFloatTween(-0.2f, 2000, 20);
                    BFrames = Animation.MergeFrames(BFrames, TempFrames);
                    TempFrames = Animation.CreateFloatTween(0.1f, 1000, 20);
                    BFrames = Animation.MergeFrames(BFrames, TempFrames);
                    Temp.WriteRotation(BFrames);
                    Out = Temp;
                    break;
            }
            return Out;
        }
    }
}