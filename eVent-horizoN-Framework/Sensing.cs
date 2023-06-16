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
using System.Reflection;
using System.Reflection.Metadata;

namespace VNFramework
{
    public static class Sensing
    {
        public class Trace
        {
            public Vector2 Origin
            {
                get
                {
                    return _origin;
                }
            }
            public Vector2 Terminus
            {
                get
                {
                    return _terminus;
                }
            }
            public Vector2 Max
            {
                get
                {
                    return new Vector2(Math.Max(_origin.X, _terminus.X), Math.Max(_origin.Y, _terminus.Y));
                }
            }
            public Vector2 Min
            {
                get
                {
                    return new Vector2(Math.Min(_origin.X, _terminus.X), Math.Min(_origin.Y, _terminus.Y));
                }
            }
            public Double Length
            {
                get
                {
                    return Math.Sqrt(Math.Pow(Math.Abs(_terminus.X - _origin.X), 2) + Math.Pow(Math.Abs(_terminus.Y - _origin.Y), 2));
                }
            }
            public Vector2 AsAlignedVector
            {
                get
                {
                    return new Vector2(_terminus.X - _origin.X, _terminus.Y - _origin.Y);
                }
            }
            public Double Bearing
            {
                get
                {
                    double y = _terminus.Y - _origin.Y;
                    double x = _terminus.X - _origin.X;
                    double bearing = 0;
                    if(x > 0)
                    {
                        bearing = Math.Acos(y / Length);
                    }
                    else if(x < 0)
                    {
                        bearing = Math.Acos((-y / Length)) + Math.PI;
                    }
                    else if(x == 0)
                    {
                        bearing = y < 0 ? Math.PI : 0;
                    }
                    return bearing;
                }
            }
            public Double Slope
            {
                get
                {
                    double dy = _terminus.Y - _origin.Y;
                    double dx = _terminus.X - _origin.X;
                    return dy / dx;
                }
            }
            public Double YIntercept
            {
                get
                {
                    if(Slope != Double.NaN && Slope != Double.PositiveInfinity && Slope != Double.NegativeInfinity)
                    {
                        return _origin.Y - (_origin.X * Slope);
                    }
                    else { return Double.NaN; }
                }
            }
            private Vector2 _origin;
            private Vector2 _terminus;
            public Trace(Vector2 origin, Vector2 terminus)
            {
                _origin = origin;
                _terminus = terminus;
            }
            public Trace(Vector2 origin, double bearing, double length)
            {
                _origin = origin;
                double x_additive = Math.Sin(bearing) * length;
                double y_additive = Math.Cos(bearing) * length;
                _terminus = origin + new Vector2((float)x_additive, (float)y_additive);
            }
            public Vector2? GetIntersection(Trace trace)
            {
                if (Double.IsNaN(YIntercept) && Double.IsNaN(trace.YIntercept))
                {
                    if (Origin.X == trace.Origin.X && !(Max.Y < trace.Min.Y || Min.Y > trace.Max.Y)) { return Origin; }
                    else { return null; }
                }
                else if (Double.IsNaN(YIntercept))
                {
                    double passY = (trace.Slope * Origin.X) + trace.YIntercept;
                    if (Origin.X <= trace.Max.X && Origin.X >= trace.Min.X && passY >= Min.Y && passY <= Max.Y) { return new Vector2(Origin.X, (float)passY); }
                    else { return null; }
                }
                else if (Double.IsNaN(trace.YIntercept))
                {
                    double passY = (Slope * trace.Origin.X) + YIntercept;
                    if (trace.Origin.X <= Max.X && trace.Origin.X >= Min.X && passY >= trace.Min.Y && passY <= trace.Max.Y) { return new Vector2(trace.Origin.X, (float)passY); }
                    else { return null; }
                }
                else
                {
                    if (Slope.Equals(trace.Slope))
                    {
                        if (YIntercept.Equals(trace.YIntercept))
                        {
                            if (Origin.Y >= trace.Min.Y && Origin.Y <= trace.Max.Y) { return Origin; }
                            else if (Terminus.Y >= trace.Min.Y && Terminus.Y >= trace.Max.Y) { return Terminus; }
                            else { return null; }
                        }
                        else { return null; }
                    }
                    else
                    {
                        /* mx + c = mtx + ct
                         * mx - mtx = ct - c
                         * x (m - mt) = ct - c
                         * x = (ct - c) / (m - mt)
                        */
                        double intersectX = (trace.YIntercept - YIntercept) / (Slope - trace.Slope);
                        if(intersectX >= Min.X && intersectX <= Max.X && intersectX >= trace.Min.X && intersectX <= trace.Max.X)
                        {
                            double intersectY = (Slope * intersectX) + YIntercept;
                            return new Vector2((float)intersectX, (float)intersectY);
                        }
                        else { return null; }
                    }
                }
            }
            public Boolean Intersects(Trace trace)
            {
                return GetIntersection(trace) != null;
            }
            public Vector2[] GetHitBoxIntersections(WorldEntity worldEntity)
            {
                Rectangle hitbox = worldEntity.HitBox;
                Trace[] edges = new Trace[4];
                edges[0] = new Trace(new Vector2(hitbox.Left, hitbox.Top), new Vector2(hitbox.Right, hitbox.Top));
                edges[1] = new Trace(new Vector2(hitbox.Right, hitbox.Top), new Vector2(hitbox.Right, hitbox.Bottom));
                edges[2] = new Trace(new Vector2(hitbox.Right, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Bottom));
                edges[3] = new Trace(new Vector2(hitbox.Left, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Top));
                List<Vector2> intersections = new List<Vector2>(); 
                foreach(Trace edge in edges)
                {
                    Vector2? intersect = GetIntersection(edge);
                    if(intersect != null) { intersections.Add((Vector2)intersect); }
                }
                return intersections.ToArray();
            }
            public Boolean IntersectsHitBox(WorldEntity worldEntity)
            {
                Rectangle hitbox = worldEntity.HitBox;
                Trace[] edges = new Trace[4];
                edges[0] = new Trace(new Vector2(hitbox.Left, hitbox.Top), new Vector2(hitbox.Right, hitbox.Top));
                edges[1] = new Trace(new Vector2(hitbox.Right, hitbox.Top), new Vector2(hitbox.Right, hitbox.Bottom));
                edges[2] = new Trace(new Vector2(hitbox.Right, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Bottom));
                edges[3] = new Trace(new Vector2(hitbox.Left, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Top));
                foreach (Trace edge in edges)
                {
                    if (Intersects(edge)) { return true; }
                }
                return false;
            }
            public Vector2? GetFirstTextureIntersection(WorldEntity worldEntity, int traceDivisions)
            {
                Vector2[] hitBoxIntersects = GetHitBoxIntersections(worldEntity);
                Rectangle hitbox = worldEntity.HitBox;
                if(hitBoxIntersects.Length == 0) { return null; }
                else
                {
                    Vector2 origin = new Vector2();
                    Vector2 terminus = new Vector2();
                    if(hitBoxIntersects.Length == 1)
                    {
                        if(hitbox.Contains((int)Math.Round(Origin.X), (int)Math.Round(Origin.Y)))
                        {
                            origin = Origin;
                            terminus = hitBoxIntersects[0];
                        }
                        else
                        {
                            origin = hitBoxIntersects[0];
                            terminus = Terminus;
                        }
                    }
                    else
                    {
                        origin = hitBoxIntersects[0];
                        terminus = hitBoxIntersects[1];
                        if(new Trace(Origin, origin).Length > new Trace(Origin, terminus).Length)
                        {
                            Vector2 a = origin;
                            origin = terminus;
                            terminus = a;
                        }
                    }
                    Trace scan = new Trace(origin, terminus);
                    Vector2 divisionVector = scan.AsAlignedVector / traceDivisions;
                    Vector2[] checkPoints = new Vector2[traceDivisions + 1];
                    checkPoints[0] = origin;
                    for(int i = 1; i < traceDivisions; i++)
                    {
                        checkPoints[i] = origin + (divisionVector * i);
                    }
                    checkPoints[traceDivisions] = terminus;
                    foreach(Vector2 check in checkPoints)
                    {
                        if(worldEntity.TextureAwareInBounds(check))
                        {
                            return check;
                        }
                    }
                    return null;
                }
            }
            public Boolean IntersectsTexture(WorldEntity worldEntity, int traceDivisions)
            {
                return GetFirstTextureIntersection(worldEntity, traceDivisions) != null;
            }
        }
    }
}
