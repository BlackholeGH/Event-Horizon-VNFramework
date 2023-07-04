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
using System.CodeDom;
using System.Windows.Forms.VisualStyles;

namespace VNFramework
{
    /// <summary>
    /// Classes and functions facillitating more complex graphical operations between entities
    /// </summary>
    public static class GraphicsTools
    {
        [Serializable]
        public abstract class VertexRenderable
        {
            protected (Matrix, Matrix, Matrix) GetDrawTransforms()
            {
                return GetDrawTransforms(new Camera(""), null);
            }
            protected (Matrix, Matrix, Matrix) GetDrawTransforms(Camera camera)
            {
                return GetDrawTransforms(camera, null);
            }
            protected (Matrix, Matrix, Matrix) GetDrawTransforms(WorldEntity alignTo)
            {
                return GetDrawTransforms(new Camera(""), alignTo);
            }
            protected virtual (Matrix, Matrix, Matrix) GetDrawTransforms(Camera camera, WorldEntity alignTo)
            {
                if (alignTo is null)
                {
                    Matrix world = Matrix.CreateTranslation(0, 0, 0);
                    Vector2 idealPosition = camera.Position * new Vector2(1, -1);
                    Matrix view = Matrix.CreateLookAt(new Vector3(idealPosition, 1), new Vector3(idealPosition, 0), new Vector3(0, 1, 0));
                    Matrix projection = Matrix.CreateOrthographic(Shell.Resolution.X / camera.ZoomFactor.X, Shell.Resolution.Y / camera.ZoomFactor.Y, 0.1f, 100f);
                    return (world, view, projection);
                }
                else
                {
                    Matrix world = Matrix.CreateTranslation(0, 0, 0);
                    //Rotation is all weird because the Y-axis is inverted. This works but can maybe be simplified...
                    Vector3 rot = new Vector3((float)Math.Sin(alignTo.RotationRads + Math.PI), (float)-Math.Cos(alignTo.RotationRads + Math.PI), 0);
                    Matrix doRot = Matrix.CreateRotationZ(alignTo.RotationRads + (float)Math.PI);
                    Vector2 idealPosition = Vector2.Transform((camera.Position - alignTo.Position) * new Vector2(-1, 1), doRot);
                    Matrix view = Matrix.CreateLookAt(new Vector3(idealPosition, 1), new Vector3(idealPosition, 0), rot);
                    Matrix projection = Matrix.CreateOrthographic(Shell.Resolution.X / (camera.ZoomFactor.X * alignTo.Size.X), Shell.Resolution.Y / (camera.ZoomFactor.Y * alignTo.Size.Y), 0.1f, 100f);
                    return (world, view, projection);
                }
            }
            public abstract void CalculateVertices(GraphicsDevice gd);
            public IVertexType[] VertexArray
            {
                get;
                protected set;
            }
            public virtual void DrawVertices(GraphicsDevice device, Camera camera, WorldEntity alignTo)
            {
                (Matrix, Matrix, Matrix) drawMatrices;
                if(camera is null)
                {
                    drawMatrices = GetDrawTransforms(alignTo);
                }
                else
                {
                    drawMatrices = GetDrawTransforms(camera, alignTo);
                }
                BasicEffect drawEffect = DrawEffect;
                drawEffect.VertexColorEnabled = true;
                drawEffect.World = drawMatrices.Item1;
                drawEffect.View = drawMatrices.Item2;
                drawEffect.Projection = drawMatrices.Item3;
                foreach(EffectPass pass in drawEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Type generic = Type.MakeGenericMethodParameter(0);
                    MethodInfo me = typeof(GraphicsDevice).GetMethod("DrawUserPrimitives", new [] { typeof(PrimitiveType), generic.MakeArrayType(), typeof(int), typeof(int) });
                    Type genType = VertexArray[0].GetType();
                    var arrayParam = Array.CreateInstance(genType, VertexArray.Length);
                    VertexArray.CopyTo(arrayParam, 0);
                    me.MakeGenericMethod(genType).Invoke(device, new object[] { PrimitiveType, arrayParam, 0, VertexArray.Length / VerticesPerPrimitive });
                }
            }
            public virtual void DrawVertices(GraphicsDevice device, WorldEntity alignTo)
            {
                DrawVertices(device, null, alignTo);
            }
            public Boolean AlignToEntity { get; set; }
            public abstract BasicEffect DrawEffect { get; }
            public abstract PrimitiveType PrimitiveType { get; }
            public abstract int VerticesPerPrimitive { get; }
        }
        public static Double Mod2PI(Double radians)
        {
            while (radians >= Math.PI * 2) { radians -= Math.PI * 2; }
            while (radians < 0) { radians += Math.PI * 2; }
            return radians;
        }
        /// <summary>
        /// Normalizes angle comparisons to take into account the 0-2pi modulus circle
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Double AngleDifference(Double a, Double b)
        {
            Double flatDif = a - b;
            if (flatDif < -Math.PI) { return flatDif + (Math.PI * 2); }
            else if (flatDif >= Math.PI) { return flatDif - (Math.PI * 2); }
            return flatDif;
        }
        /// <summary>
        /// Defines geometric polygons as a series of directed lines. Polygons are constructed clockwise; the righthand side of a line should be the shape interior.
        /// </summary>
        [Serializable]
        public class Polygon : ICollider, IEnumerable<Trace>
        {
            List<Trace> _lineTraces = new List<Trace>();
            public Trace this[int index]
            {
                get { return _lineTraces[index]; }
                set { _lineTraces[index] = value; }
            }
            public IEnumerator<Trace> GetEnumerator()
            {
                return _lineTraces.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            public int Count
            {
                get { return _lineTraces.Count; }
            }
            private double _maxDistanceFromLocalOrigin = 0;
            public double GetMaximumExtent()
            {
                return _maxDistanceFromLocalOrigin;
            }
            public void InstanceFromVectors(Vector2[] vectors)
            {
                InstanceFromVectors(vectors, new Vector2(), new Vector2());
            }
            public void InstanceFromVectors(Vector2[] vectors, Vector2 normalizingOrigin, Vector2 distanceBoundsOrigin)
            {
                _maxDistanceFromLocalOrigin = 0;
                _lineTraces = new List<Trace>();
                for(int i = 0; i < vectors.Length; i++)
                {
                    Vector2 origin = vectors[i] - normalizingOrigin;
                    Vector2 terminus = i < vectors.Length - 1 ? vectors[i + 1] : vectors[0];
                    terminus = terminus - normalizingOrigin;
                    _lineTraces.Add(new Trace(origin, terminus));
                    Vector2 distanceBoundsVector = origin - distanceBoundsOrigin;
                    if(distanceBoundsVector.Length() > _maxDistanceFromLocalOrigin) { _maxDistanceFromLocalOrigin = distanceBoundsVector.Length(); }
                }
            }
            public void InstanceFromPoints(Point[] points)
            {
                InstanceFromVectors(points.Select(x => VNFUtils.ConvertPoint(x)).ToArray(), new Vector2(), new Vector2());
            }
            public void InstanceFromPoints(Point[] points, Point normalizingOrigin, Point distanceBoundsOrigin)
            {
                InstanceFromVectors(points.Select(x => VNFUtils.ConvertPoint(x)).ToArray(), VNFUtils.ConvertPoint(normalizingOrigin), VNFUtils.ConvertPoint(distanceBoundsOrigin));
            }
            public (Vector2[], Trace[], int[]) GetTraceIntersections(Trace trace)
            {
                List<Trace> intersectionTraces = new List<Trace>();
                List<Vector2> intersections = new List<Vector2>();
                List<int> intersectionIndices = new List<int>();
                int index = 0;
                foreach(Trace selfTrace in _lineTraces)
                {
                    bool edgeMatches = false;
                    Vector2? intersect = trace.GetIntersection(selfTrace, out edgeMatches);
                    for (int i = 0; i < (edgeMatches ? 2 : 1); i++) //so that edge matches don't throw off the calculator for whether a point is inside the polygon, which relies on an even number of intersections.
                    {
                        if (intersect != null)
                        {
                            intersectionTraces.Add(selfTrace);
                            intersections.Add((Vector2)intersect);
                            intersectionIndices.Add(index);
                        }
                    }
                    index++;
                }
                return (intersections.ToArray(), intersectionTraces.ToArray(), intersectionIndices.ToArray());
            }
            public Vector2? GetFirstIntersection(Trace trace)
            {
                Vector2 closest = new Vector2();
                int i = 0;
                Vector2[] intersections = GetTraceIntersections(trace).Item1;
                if (intersections.Length == 0) { return null; }
                else
                {
                    foreach (Vector2 intersect in intersections)
                    {

                        if (i == 0 || (intersect - trace.Origin).Length() < (closest - trace.Origin).Length())
                        {
                            closest = intersect;
                        }
                    }
                    return closest;
                }
            }
            public Boolean Contains(Vector2 coord)
            {
                Trace sightline = new Trace(coord, new Vector2(1000000, coord.Y));
                var intersects = GetTraceIntersections(sightline);
                return intersects.Item1.Count() % 2 != 0;
            }
            public Polygon(Rectangle rectangle)
            {
                InstanceFromVectors(new Vector2[] { new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Bottom) }, new Vector2(), new Vector2());
            }
            public Polygon(Rectangle rectangle, Vector2 normalizingOrigin, Vector2 distanceBoundsOrigin)
            {
                InstanceFromVectors(new Vector2[] { new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Bottom) }, normalizingOrigin, distanceBoundsOrigin);
            }
            public Polygon(Vector2[] vertices)
            {
                InstanceFromVectors(vertices, new Vector2(), new Vector2());
            }
            public Polygon(Vector2[] vertices, Vector2 normalizingOrigin, Vector2 distanceBoundsOrigin)
            {
                InstanceFromVectors(vertices, normalizingOrigin, distanceBoundsOrigin);
            }
            public Boolean EdgesIntersect(Polygon polygon)
            {
                foreach(Trace edge in polygon)
                {
                    var intersections = GetTraceIntersections(edge);
                    if(intersections.Item1.Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            public Boolean Intersects(Trace trace)
            {
                var intersections = GetTraceIntersections(trace);
                if (intersections.Item1.Length > 0)
                {
                    return true;
                }
                else { return false; }
            }
            public Boolean Collides(ICollider collider)
            {
                if (collider is Polygon) { return EdgesIntersect((Polygon)collider); }
                else if (collider is RadialCollider) { return ((RadialCollider)collider).Collides(this); }
                return false;
            }
            /// <summary>
            /// Gets the collider impingement
            /// </summary>
            /// <param name="collider"></param>
            /// <returns></returns>
            public Trace GetImpingementOn(ICollider collider)
            {
                if (collider is Polygon) { return null; } //Implement a proper solve for this later if we do a proper collider mesh physics engine
                else if (collider is RadialCollider) { return ((RadialCollider)collider).GetImpingementOn(this).Flip(); }
                return null;
            }
            public void ResolveCollision(DynamicEntity selfAttach, WorldEntity remoteAttach)
            {
                return;
            }
            public ICollider Scale(Vector2 origin, Vector2 scale) 
            {
                List<Vector2> newCoords = new List<Vector2>();
                foreach(Trace trace in _lineTraces)
                {
                    Vector2 dif = trace.Origin - origin;
                    dif = dif * scale;
                    newCoords.Add(dif + origin);
                }
                return new Polygon(newCoords.ToArray());
            }
            public ICollider Rotate(Vector2 origin, Double angle)
            {
                List<Vector2> newCoords = new List<Vector2>();
                foreach (Trace trace in _lineTraces)
                {
                    Vector2 dif = trace.Origin - origin;
                    Trace norm = new Trace(new Vector2(), dif);
                    Double normBearing = norm.Bearing;
                    Vector2 newNorm = new Vector2((float)Math.Sin(normBearing + angle), -(float)Math.Cos(normBearing + angle)) * (float)norm.Length;
                    newCoords.Add(newNorm + origin);
                }
                return new Polygon(newCoords.ToArray());
            }
            public ICollider Translate(Vector2 translation)
            {
                List<Vector2> newCoords = new List<Vector2>();
                foreach (Trace trace in _lineTraces)
                {
                    newCoords.Add(trace.Origin + translation);
                }
                return new Polygon(newCoords.ToArray(), new Vector2(), translation);
            }
        }
        /// <summary>
        /// Represents a directed line with a beginning and end point, for calculating intersections
        /// </summary>
        [Serializable]
        public class Trace : VertexRenderable
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
            object _max = null;
            public Vector2 Max
            {
                get
                {
                    if(_max == null)
                    {
                        _max = new Vector2(Math.Max(_origin.X, _terminus.X), Math.Max(_origin.Y, _terminus.Y));
                    }
                    return (Vector2)_max;
                }
            }
            object _min = null;
            public Vector2 Min
            {
                get
                {
                    if(_min == null)
                    {
                        _min = new Vector2(Math.Min(_origin.X, _terminus.X), Math.Min(_origin.Y, _terminus.Y));
                    }
                    return (Vector2)_min;
                }
            }
            object _length = null;
            public Double Length
            {
                get
                {
                    if( _length == null)
                    {
                        _length = Math.Sqrt(Math.Pow(Math.Abs(_terminus.X - _origin.X), 2) + Math.Pow(Math.Abs(_terminus.Y - _origin.Y), 2));
                    }
                    return (Double)_length;
                }
            }
            object _alignedVector;
            public Vector2 AsAlignedVector
            {
                get
                {
                    if( _alignedVector == null )
                    {
                        _alignedVector = new Vector2(_terminus.X - _origin.X, _terminus.Y - _origin.Y);
                    }
                    return (Vector2)_alignedVector;
                }
            }
            object _bearing = null;
            public Double Bearing
            {
                get
                {
                    if (_bearing == null)
                    {
                        double y = _terminus.Y - _origin.Y;
                        double x = _terminus.X - _origin.X;
                        double bearing = 0;
                        if (x > 0)
                        {
                            bearing = Math.Acos(-y / Length);
                        }
                        else if (x < 0)
                        {
                            bearing = Math.Acos(y / Length) + Math.PI;
                        }
                        else if (x == 0)
                        {
                            bearing = y > 0 ? Math.PI : 0;
                        }
                        _bearing = bearing;
                    }
                    return (Double)_bearing;
                }
            }
            object _slope = null;
            public Double Slope
            {
                get
                {
                    if (_slope == null)
                    {
                        double dy = _terminus.Y - _origin.Y;
                        double dx = _terminus.X - _origin.X;
                        _slope = dy / dx;
                    }
                    return (Double)_slope;
                }
            }
            object _yIntercept = null;
            public Double YIntercept
            {
                get
                {
                    if (_yIntercept == null)
                    {
                        if (Slope != Double.NaN && Slope != Double.PositiveInfinity && Slope != Double.NegativeInfinity)
                        {
                            _yIntercept = _origin.Y - (_origin.X * Slope);
                        }
                        else { _yIntercept = Double.NaN; }
                    }
                    return (Double)_yIntercept;
                }
            }
            public static Trace operator * (Trace a, Double b)
            {
                return new Trace(a.Origin, a.Bearing, a.Length * b);
            }
            public Trace GetPerpendicularComponentTo(Trace trace)
            {
                Double recenteredBearing = Mod2PI(Bearing - trace.Bearing);
                Double component = Math.Sin(recenteredBearing) * Length;
                Double componentMagnitude = Math.Abs(component);
                Double componentBearing = Mod2PI(trace.Bearing + (Math.Sign(component) * (Math.PI / 2)));
                Trace componentTrace = new Trace(Origin, componentBearing, componentMagnitude);
                return componentTrace;
            }
            public static Vector2 GetPerpendicularComponent(Vector2 getComponentFrom, Vector2 thePerpendicularOfThis)
            {
                Trace from = new Trace(getComponentFrom);
                return from.GetPerpendicularComponentTo(new Trace(thePerpendicularOfThis)).AsAlignedVector;
            }
            public Trace GetAlignedComponentTo(Trace trace)
            {
                Double recenteredBearing = Mod2PI(Bearing - trace.Bearing);
                Double component = Math.Cos(recenteredBearing) * Length;
                Double componentMagnitude = Math.Abs(component);
                Double componentBearing = Mod2PI(trace.Bearing + (component < 0 ? Math.PI : 0));
                Trace componentTrace = new Trace(Origin, componentBearing, componentMagnitude);
                return componentTrace;
            }
            public static Vector2 GetAlignedComponent(Vector2 getComponentFrom, Vector2 theAlignmentOfThis)
            {
                Trace from = new Trace(getComponentFrom);
                return from.GetAlignedComponentTo(new Trace(theAlignmentOfThis)).AsAlignedVector;
            }
            public Boolean IsCoordOnRightHandSide(Vector2 coord)
            {
                Trace toCoord = new Trace(Origin, coord);
                Double normalizedBearing = toCoord.Bearing - Bearing;
                if (normalizedBearing < 0) { normalizedBearing += (Math.PI * 2); }
                return normalizedBearing < Math.PI;
            }
            public Trace Flip()
            {
                return new Trace(Terminus, Origin);
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
                double y_additive = -Math.Cos(bearing) * length;
                _terminus = origin + new Vector2((float)x_additive, (float)y_additive);
            }
            public Trace(Vector2 terminus)
            {
                _origin = new Vector2();
                _terminus = terminus;
            }
            public Vector2? GetIntersection(Trace trace)
            {
                bool outBool = false;
                return GetIntersection(trace, out outBool);
            }
            public Vector2? GetIntersection(Trace trace, out bool edgeMatches)
            {
                edgeMatches = false;
                if (Double.IsNaN(YIntercept) && Double.IsNaN(trace.YIntercept))
                {
                    if (Origin.X == trace.Origin.X)
                    {
                        edgeMatches = true;
                        if(Origin.Y >= trace.Min.Y && Origin.Y <= trace.Max.Y) { return Origin; }
                        else if(trace.Origin.Y >= Min.Y && trace.Origin.Y <= Max.Y && !(trace.Terminus.Y >= Min.Y && trace.Terminus.Y <= Max.Y)) { return trace.Origin; }
                        else if(!(trace.Origin.Y >= Min.Y && trace.Origin.Y <= Max.Y) && trace.Terminus.Y >= Min.Y && trace.Terminus.Y <= Max.Y) { return trace.Terminus; }
                        else if(trace.Origin.Y >= Min.Y && trace.Origin.Y <= Max.Y && trace.Terminus.Y >= Min.Y && trace.Terminus.Y <= Max.Y)
                        {
                            Trace toTraceOrigin = new Trace(Origin, trace.Origin);
                            Trace toTraceTerminus = new Trace(Origin, trace.Terminus);
                            return toTraceOrigin.Length < toTraceTerminus.Length ? trace.Origin : trace.Terminus;
                        }
                        return null;
                    }
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
                            edgeMatches = true;
                            if (Origin.Y >= trace.Min.Y && Origin.Y <= trace.Max.Y) { return Origin; }
                            else if (Terminus.Y >= trace.Min.Y && Terminus.Y <= trace.Max.Y) { return Terminus; }
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
                        if (intersectX >= Min.X && intersectX <= Max.X && intersectX >= trace.Min.X && intersectX <= trace.Max.X)
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
            public Boolean Intersects(ICollider collider)
            {
                return collider.Intersects(this);
            }
            public Vector2? GetFirstIntersection(ICollider collider)
            {
                return collider.GetFirstIntersection(this);
            }
            public Trace GetClosestTraceFrom(Vector2 coordinate, Double distanceLimit)
            {
                double bearing1 = Bearing + (Math.PI / 2);
                double bearing2 = Bearing - (Math.PI / 2);
                if (bearing1 > Math.PI * 2) { bearing1 = bearing1 - (Math.PI * 2); }
                else if(bearing1 < 0) { bearing1 = bearing1 + (Math.PI * 2); }
                if (bearing2 > Math.PI * 2) { bearing2 = bearing2 - (Math.PI * 2); }
                else if (bearing2 < 0) { bearing2 = bearing2 + (Math.PI * 2); }
                Vector2? intersect1 = GetIntersection(new Trace(coordinate, bearing1, distanceLimit));
                Vector2? intersect2 = GetIntersection(new Trace(coordinate, bearing2, distanceLimit));
                if (intersect1 != null) { return new Trace(coordinate, (Vector2)intersect1); }
                else if (intersect2 != null) { return new Trace(coordinate, (Vector2)intersect2); }
                else
                {
                    Trace one = new Trace(coordinate, Origin);
                    Trace two = new Trace(coordinate, Terminus);
                    return one.Length < two.Length ? one : two;
                }
            }
            public Vector2[] GetHitBoxIntersections(WorldEntity worldEntity)
            {
                Rectangle hitbox = worldEntity.Hitbox;
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
                Rectangle hitbox = worldEntity.Hitbox;
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
                Rectangle hitbox = worldEntity.Hitbox;
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
            private BasicEffect _drawEffect = null;
            public override BasicEffect DrawEffect
            {
                get { return _drawEffect; }
            }
            public override PrimitiveType PrimitiveType
            {
                get
                {
                    return PrimitiveType.LineList;
                }
            }
            public override int VerticesPerPrimitive
            {
                get
                {
                    return 2;
                }
            }
            public override void CalculateVertices(GraphicsDevice gd)
            {
                _drawEffect = new BasicEffect(gd);
                VertexArray = new IVertexType[2];
                VertexPositionColor vPosCol = new VertexPositionColor();
                vPosCol.Color = DrawColour;
                vPosCol.Position = new Vector3(Origin * new Vector2(1, -1), 0);
                VertexArray[0] = vPosCol;
                vPosCol = new VertexPositionColor();
                vPosCol.Color = DrawColour;
                vPosCol.Position = new Vector3(Terminus * new Vector2(1, -1), 0);
                VertexArray[1] = vPosCol;
            }
            private Color _drawColour = Color.Black;
            public Color DrawColour
            {
                get
                {
                    return _drawColour;
                }
                set
                {
                    _drawColour = value;
                    if (VertexArray != null)
                    {
                        for (int i = 0; i < VertexArray.Length; i++)
                        {
                            VertexPositionColor vpc = (VertexPositionColor)VertexArray[i];
                            vpc.Color = _drawColour;
                            VertexArray[i] = vpc;
                        }
                    }
                }
            }
            public Trace Scale(Vector2 origin, Vector2 scale)
            {
                return new Trace(((Origin - origin) * scale) + origin, ((Terminus - origin) * scale) + origin);
            }
            public Trace Rotate(Vector2 origin, Double angle)
            {
                Vector2 normOrigin = Origin - origin;
                Vector2 normTerminus = Terminus - origin;
                Matrix rot = Matrix.CreateRotationZ((float)angle);
                return new Trace((Vector2.Transform(normOrigin, rot)) + origin, (Vector2.Transform(normTerminus, rot)) + origin);
            }
            public Trace Translate(Vector2 translation)
            {
                return new Trace(Origin + translation, Terminus + translation);
            }
        }
    }
}
