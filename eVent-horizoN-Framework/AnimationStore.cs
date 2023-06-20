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
using System.Windows.Forms;
using System.Reflection;

namespace VNFramework
{
    public partial class Animation
    {
        public static Animation Retrieve(String name)
        {
            Animation animOut = null;
            Animation animTemp = null;
            SortedList<int, Vector2> vectorBFrames = new SortedList<int, Vector2>();
            SortedList<int, Vector2> vectorTempFrames = new SortedList<int, Vector2>();
            SortedList<int, Point> pointBFrames = new SortedList<int, Point>();
            SortedList<int, Point> pointTempFrames = new SortedList<int, Point>(); ;
            SortedList<int, ColourShift> colourBFrames = new SortedList<int, ColourShift>();
            SortedList<int, ColourShift> colourTempFrames = new SortedList<int, ColourShift>();
            SortedList<int, float> floatBFrames = new SortedList<int, float>();
            SortedList<int, float> floatTempFrames = new SortedList<int, float>();
            int velocity = 0;
            int accel = 0;
            int frame = 0;
            switch (name.ToUpper())
            {
                case "BOUNCE_1":
                    animTemp = new Animation("bounce_1");
                    vectorBFrames = new SortedList<int, Vector2>();
                    velocity = -10;
                    accel = 1;
                    frame = 0;
                    do
                    {
                        vectorBFrames.Add(frame, new Vector2(0, (float)velocity));
                        frame += 20;
                        velocity += accel;
                    }
                    while (velocity <= 10);
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "BOUNCE_2":
                    animTemp = new Animation("bounce_2");
                    vectorBFrames = new SortedList<int, Vector2>();
                    velocity = -10;
                    accel = 1;
                    frame = 0;
                    do
                    {
                        vectorBFrames.Add(frame, new Vector2(0, (float)velocity));
                        frame += 20;
                        velocity += accel;
                    }
                    while (velocity <= 10);
                    velocity = -6;
                    do
                    {
                        vectorBFrames.Add(frame, new Vector2(0, (float)velocity));
                        frame += 20;
                        velocity += accel;
                    }
                    while (velocity <= 6);
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "BOUNCE_3":
                    animTemp = new Animation("bounce_3");
                    vectorBFrames = new SortedList<int, Vector2>();
                    velocity = -5;
                    accel = 1;
                    frame = 0;
                    do
                    {
                        vectorBFrames.Add(frame, new Vector2(0, (float)velocity));
                        frame += 20;
                        velocity += accel;
                    }
                    while (velocity <= 5);
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "MOVEUPIN":
                    animTemp = new Animation("moveupin");
                    animTemp.WriteMovement(Animation.CreateVectorTween(new Vector2(0, -600), 1500, 20));
                    animOut = animTemp;
                    break;
                case "FALLSHOCK":
                    animTemp = new Animation("fallshock");
                    vectorBFrames = Animation.CreateVectorTween(new Vector2(0, -20), 1500, 20);
                    vectorTempFrames = Animation.CreateVectorTween(new Vector2(0, 200), 300, 20);
                    vectorBFrames = Animation.MergeFrames(vectorBFrames, vectorTempFrames);
                    vectorBFrames.Add(2500, new Vector2(0, 0));
                    vectorTempFrames = Animation.CreateVectorTween(new Vector2(0, -180), 700, 20);
                    vectorBFrames = Animation.MergeFrames(vectorBFrames, vectorTempFrames);
                    animTemp.WriteMovement(vectorBFrames);
                    pointBFrames = new SortedList<int, Point>();
                    pointBFrames.Add(0, new Point(1, 1));
                    pointBFrames.Add(1800, new Point(3, 0));
                    animTemp.WriteFrames(pointBFrames);
                    animOut = animTemp;
                    break;
                case "SHAKEMINOR":
                    animTemp = new Animation("shakeminor");
                    vectorBFrames = new SortedList<int, Vector2>();
                    velocity = -4;
                    accel = 1;
                    frame = 0;
                    int FCount = 0;
                    do
                    {
                        vectorBFrames.Add(frame, new Vector2((float)velocity, 0));
                        frame += 20;
                        velocity += accel;
                        if(velocity == -4 || velocity == 4)
                        {
                            FCount++;
                            accel = velocity / -4;
                        }
                    }
                    while (FCount <= 4);
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "SHAKEMAJOR":
                    animTemp = new Animation("shakemajor");
                    vectorBFrames = new SortedList<int, Vector2>();
                    float fVelocity = -40;
                    float fAccel = 20;
                    frame = 0;
                    float trueX = 0;
                    Boolean centerPass = false;
                    do
                    {
                        centerPass = false;
                        vectorBFrames.Add(frame, new Vector2((float)fVelocity, 0));
                        trueX += fVelocity;
                        frame += 20;
                        fVelocity += fAccel;
                        if (trueX * fAccel > 0)
                        {
                            fVelocity = 0.5f * fVelocity;
                            fAccel = -fAccel * 0.9f;
                            centerPass = true;
                        }
                    }
                    while (frame < 5000 && (Math.Sqrt(fVelocity*fVelocity) > 4 || !centerPass));
                    vectorBFrames.Add(frame, new Vector2(-trueX, 0));
                    vectorBFrames.Add(frame + 1000, new Vector2(0, 0));
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "SHAKEQUAKE":
                    animTemp = new Animation("shakequake");
                    vectorBFrames = new SortedList<int, Vector2>();
                    frame = 0;
                    float lastX = 0;
                    float mult = 1;
                    float nowVal = 0;
                    do
                    {
                        nowVal = (float)Math.Sin((((float)frame % 1000f) / 1000f) * Math.PI * 16f) * mult;
                        mult = 100 - (frame / 10f);
                        vectorBFrames.Add(frame, new Vector2(nowVal - lastX, 0));
                        lastX = nowVal;
                        frame += 20;
                    }
                    while (frame < 5000 && mult > 1);
                    vectorBFrames.Add(frame, new Vector2(-nowVal, 0));
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "LASTINGQUAKE":
                    animTemp = new Animation("lastingquake");
                    vectorBFrames = new SortedList<int, Vector2>();
                    frame = 0;
                    lastX = 0;
                    mult = 1;
                    nowVal = 0;
                    do
                    {
                        nowVal = (float)Math.Sin((((float)frame % 1000f) / 1000f) * Math.PI * 16f) * mult;
                        mult = Shell.Rnd.Next(5, 20);
                        vectorBFrames.Add(frame, new Vector2(nowVal - lastX, 0));
                        lastX = nowVal;
                        frame += 20;
                    }
                    while (frame < 5000 && mult > 1);
                    vectorBFrames.Add(frame, new Vector2(-nowVal, 0));
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "CREDITSROLL":
                    animTemp = new Animation("creditsroll");
                    SortedList<int, Vector2> creditsMovement = Animation.CreateVectorTween(new Vector2(0, -11300), 100800, 20);
                    animTemp.WriteMovement(creditsMovement);
                    animTemp.AutoTrigger = false;
                    animOut = animTemp;
                    break;
                case "SPIRALOUT":
                    animTemp = new Animation("spiralout");
                    vectorBFrames = new SortedList<int, Vector2>();
                    SortedList<int, Vector2> myScaleFrames = new SortedList<int, Vector2>();
                    SortedList<int, float> myRotFrames = new SortedList<int, float>();
                    frame = 0;
                    Vector2 lastVec = new Vector2();
                    Vector2 nowVec = new Vector2();
                    float progVal = 0f;
                    float scaleAmount = 0f;
                    do
                    {
                        progVal = (float)((((float)frame % 1000f) / 1000f) * Math.PI * 2f);
                        nowVec = new Vector2((float)Math.Cos((double)progVal), (float)Math.Sin((double)progVal)) * (300f * (frame/6500f));
                        vectorBFrames.Add(frame, nowVec - lastVec);
                        scaleAmount = ((frame / 200f) * (frame / 200f)) / 10000f;
                        myScaleFrames.Add(frame, new Vector2(scaleAmount, scaleAmount));
                        myRotFrames.Add(frame, 0.01f);
                        lastVec = nowVec;
                        frame += 20;
                    }
                    while (frame < 6500);
                    animTemp.WriteMovement(vectorBFrames);
                    animTemp.WriteScaling(myScaleFrames);
                    animTemp.WriteRotation(myRotFrames);
                    animOut = animTemp;
                    break;
                case "BLINKIN":
                    animTemp = new Animation("blinkin");
                    colourBFrames = new SortedList<int, ColourShift>();
                    colourBFrames.Add(20, new ColourShift(255, 255, 255, 255));
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "BLINKOUT":
                    animTemp = new Animation("blinkout");
                    colourBFrames = new SortedList<int, ColourShift>();
                    colourBFrames.Add(20, new ColourShift(-255, -255, -255, -255));
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FLASH1":
                    animTemp = new Animation("flash1");
                    colourBFrames = new SortedList<int, ColourShift>();
                    colourBFrames.Add(2700, new ColourShift(255, 255, 255, 255));
                    colourBFrames.Add(2800, new ColourShift(-255, -255, -255, -255));
                    colourBFrames.Add(2900, new ColourShift(255, 255, 255, 255));
                    colourBFrames.Add(3000, new ColourShift(-255, -255, -255, -255));
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FLASH2":
                    animTemp = new Animation("flash2");
                    colourBFrames = new SortedList<int, ColourShift>();
                    colourBFrames.Add(2750, new ColourShift(255, 255, 255, 255));
                    colourBFrames.Add(2950, new ColourShift(-255, -255, -255, -255));
                    colourBFrames.Add(3000, new ColourShift(0,0,0,0));
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FLOATER":
                    animTemp = new Animation("floater");
                    vectorBFrames = new SortedList<int, Vector2>();
                    frame = 0;
                    float last = 0f;
                    do
                    {
                        float current = (float)Math.Sin((frame / 4000d) * Math.PI * 2d) * 45;
                        vectorBFrames.Add(frame, new Vector2(0, current - last));
                        last = current;
                        frame += 20;
                    }
                    while (frame < 4000);
                    animTemp.WriteMovement(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "FADEIN":
                    animTemp = new Animation("fadein");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 1500, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEINMED":
                    animTemp = new Animation("fadeinmed");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 2000, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEINLONG":
                    animTemp = new Animation("fadeinlong");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 3000, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEOUT":
                    animTemp = new Animation("fadeout");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 1500, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEOUTMED":
                    animTemp = new Animation("fadeoutmed");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 2000, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEOUTLONG":
                    animTemp = new Animation("fadeoutlong");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 3000, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEOUTRAPID":
                    animTemp = new Animation("fadeoutrapid");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 300, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEOUTCOLOURPRESERVE":
                    animTemp = new Animation("fadeoutcolourpreserve");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(0, 0, 0, -255), 2000, 20);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEINOUT":
                    animTemp = new Animation("fadeinout");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 1500, 20);
                    colourTempFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 1500, 20);
                    colourBFrames = Animation.MergeFrames(colourBFrames, colourTempFrames);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "FADEINOUTLONG":
                    animTemp = new Animation("fadeinoutlong");
                    colourBFrames = Animation.CreateColourTween(new ColourShift(255, 255, 255, 255), 5000, 20);
                    colourTempFrames = Animation.CreateColourTween(new ColourShift(-255, -255, -255, -255), 5000, 20);
                    colourBFrames = Animation.MergeFrames(colourBFrames, colourTempFrames);
                    animTemp.WriteColouring(colourBFrames);
                    animOut = animTemp;
                    break;
                case "01OSCILLATE":
                    animTemp = new Animation("01oscillate");
                    pointBFrames = new SortedList<int, Point>();
                    pointBFrames.Add(0, new Point(0, 0));
                    pointBFrames.Add(50, new Point(1, 0));
                    pointBFrames.Add(100, new Point(1, 0));
                    animTemp.WriteFrames(pointBFrames);
                    animTemp.Loop = true;
                    animOut = animTemp;
                    break;
                case "SOFIASLIDESHOW":
                    animTemp = new Animation("sofiaslideshow");
                    pointBFrames = new SortedList<int, Point>();
                    int ChangeFrame = 0;
                    for(int ii = 0; ii < 3; ii++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            pointBFrames.Add(ChangeFrame, new Point(i, ii));
                            ChangeFrame += 600;
                            if(ii == 2 && i == 1)
                            {
                                pointBFrames.Add(ChangeFrame, new Point(0, 0));
                                break;
                            }
                        }
                    }
                    animTemp.WriteFrames(pointBFrames);
                    animTemp.Loop = true;
                    animOut = animTemp;
                    break;
                case "GOLEMSLIDESHOW":
                    animTemp = new Animation("golemslideshow");
                    pointBFrames = new SortedList<int, Point>();
                    pointBFrames.Add(0, new Point(0, 0));
                    pointBFrames.Add(600, new Point(1, 0));
                    pointBFrames.Add(1200, new Point(2, 0));
                    pointBFrames.Add(1800, new Point(0, 0));
                    animTemp.WriteFrames(pointBFrames);
                    animTemp.Loop = true;
                    animOut = animTemp;
                    break;
                case "SLOWOSCILLATE":
                    animTemp = new Animation("slowoscillate");
                    pointBFrames = new SortedList<int, Point>();
                    for (int i = 0; i < 9; i++)
                    {
                        pointBFrames.Add(500*i, new Point(Math.Floor(((float)500 * i) /1000f) == ((float)500 * i) / 1000f ? 0 : 1, 0));
                    }
                    animTemp.WriteFrames(pointBFrames);
                    animOut = animTemp;
                    break;
                case "FOCUSGROW":
                    animTemp = new Animation("focusgrow");
                    vectorBFrames = Animation.CreateVectorTween(new Vector2(0.06f, 0.06f), 200, 20);
                    animTemp.WriteScaling(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "FOCUSSHRINK":
                    animTemp = new Animation("focusshrink");
                    vectorTempFrames = new SortedList<int, Vector2>();
                    vectorTempFrames = Animation.CreateVectorTween(new Vector2(0.06f, 0.06f), 200, 20);
                    vectorBFrames = new SortedList<int, Vector2>();
                    foreach (int key in vectorTempFrames.Keys)
                    {
                        vectorBFrames.Add(key, (Vector2)vectorTempFrames[key] * -1);
                    }
                    animTemp.WriteScaling(vectorBFrames);
                    animOut = animTemp;
                    break;
                case "WIGGLE":
                    animTemp = new Animation("wiggle");
                    floatBFrames = Animation.CreateFloatTween(0.1f, 1000, 20);
                    floatTempFrames = Animation.CreateFloatTween(-0.2f, 2000, 20);
                    floatBFrames = Animation.MergeFrames(floatBFrames, floatTempFrames);
                    floatTempFrames = Animation.CreateFloatTween(0.1f, 1000, 20);
                    floatBFrames = Animation.MergeFrames(floatBFrames, floatTempFrames);
                    animTemp.WriteRotation(floatBFrames);
                    animOut = animTemp;
                    break;
                case "RIGHTLEFT":
                    animTemp = new Animation("rightleft");
                    vectorBFrames = Animation.CreateVectorTween(new Vector2(200, 0), 2000, 20);
                    vectorTempFrames = Animation.CreateVectorTween(new Vector2(-200, 0), 2000, 20);
                    vectorBFrames = Animation.MergeFrames(vectorBFrames, vectorTempFrames);
                    animTemp.WriteMovement(vectorBFrames);
                    animTemp.Loop = true;
                    animOut = animTemp;
                    break;
                case "MYSTICMOVEMENT":
                    Animation mysticMovement = new Animation("mysticmovement");
                    SortedList<int, Vector2> a = Animation.CreateVectorTween(new Vector2(-530, 0), 800, 20);
                    mysticMovement.WriteMovement(a);
                    a = Animation.CreateVectorTween(new Vector2(-0.06f, -0.06f), 800, 20);
                    SortedList<int, Vector2> b = Animation.CreateVectorTween(new Vector2(-1.76f, 0), 20, 20);
                    a = Animation.MergeFrames(a, b);
                    mysticMovement.WriteScaling(a);
                    animOut = mysticMovement;
                    break;
                case "SOFIAMOVEMENT":
                    Animation sofiaMovement = new Animation("sofiamovement");
                    a = Animation.CreateVectorTween(new Vector2(-100, 0), 200, 20);
                    sofiaMovement.WriteMovement(a);
                    animOut = sofiaMovement;
                    break;
                case "COOLMOVEMENT":
                    Animation coolMovement = new Animation("coolmovement");
                    a = Animation.CreateVectorTween(new Vector2(0, 0), 800, 20);
                    b = Animation.CreateVectorTween(new Vector2(-550, 0), 800, 20);
                    a = Animation.MergeFrames(a, b);
                    coolMovement.WriteMovement(a);
                    animOut = coolMovement;
                    break;
                case "KINGMOVEMENT":
                    Animation kingMovement = new Animation("kingmovement");
                    a = Animation.CreateVectorTween(new Vector2(0, 0), 1000, 20);
                    b = Animation.CreateVectorTween(new Vector2(-550, 0), 800, 20);
                    a = Animation.MergeFrames(a, b);
                    kingMovement.WriteMovement(a);
                    animOut = kingMovement;
                    break;
            }
            return animOut;
        }
    }
}