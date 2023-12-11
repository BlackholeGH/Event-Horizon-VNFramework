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
    public static partial class GraphicsTools
    {
        /// <summary>
        /// VertexRenderable objects can be drawn to the world based on point vector graphics. This class provides the abstract definitions
        /// </summary>
        [Serializable]
        public abstract class VertexRenderable
        {
            /// <summary>
            /// Function to get the draw transforms for a null camera environment
            /// </summary>
            /// <returns>Draw transform matrices for vector rendering within MonoGame</returns>
            protected (Matrix, Matrix, Matrix) GetDrawTransforms()
            {
                return GetDrawTransforms(new Camera(""), null);
            }
            /// <summary>
            /// Function to get the draw transforms with respect to a camera position
            /// </summary>
            /// <param name="camera">Camera entity with respect to which the object is being viewed</param>
            /// <returns>Draw transform matrices for vector rendering within MonoGame</returns>
            protected (Matrix, Matrix, Matrix) GetDrawTransforms(Camera camera)
            {
                return GetDrawTransforms(camera, null);
            }
            /// <summary>
            /// Function to get the draw transforms aligned to a WorldEntity
            /// </summary>
            /// <param name="alignTo">WorldEntity with which the VertexRenderable should be aligned</param>
            /// <returns>Draw transform matrices for vector rendering within MonoGame</returns>
            protected (Matrix, Matrix, Matrix) GetDrawTransforms(WorldEntity alignTo)
            {
                return GetDrawTransforms(new Camera(""), alignTo);
            }
            /// <summary>
            /// Function to get the draw transforms with respect to a camera and a WorldEntity to align to
            /// </summary>
            /// <param name="camera">Camera entity with respect to which the object is being viewed</param>
            /// <param name="alignTo">WorldEntity with which the VertexRenderable should be aligned</param>
            /// <returns>Draw transform matrices for vector rendering within MonoGame</returns>
            protected virtual (Matrix, Matrix, Matrix) GetDrawTransforms(Camera camera, WorldEntity alignTo)
            {
                if (alignTo is null) //If there is no entity to align the camera to
                {
                    Matrix world = Matrix.CreateTranslation(0, 0, 0); //World translation is zero
                    Vector2 idealPosition = camera.Position * new Vector2(1, -1); //Position with respect to camera is calculated
                    Matrix view = Matrix.CreateLookAt(new Vector3(idealPosition, 1), new Vector3(idealPosition, 0), new Vector3(0, 1, 0)); //CreateLookAt above the scene
                    Matrix projection = Matrix.CreateOrthographic(Shell.Resolution.X / camera.ZoomFactor.X, Shell.Resolution.Y / camera.ZoomFactor.Y, 0.1f, 100f); //Create orthographic projection based on camera zoom and sceen resolution
                    return (world, view, projection);
                }
                else
                {
                    Matrix world = Matrix.CreateTranslation(0, 0, 0); //World translation is zero
                    //Rotation here is calculated with an inverted Y-axis to the one used by the world coordinate system.
                    Vector3 rot = new Vector3((float)Math.Sin(alignTo.RotationRads + Math.PI), (float)-Math.Cos(alignTo.RotationRads + Math.PI), 0); //Correct rotation with respect to alignment entity
                    Matrix doRot = Matrix.CreateRotationZ(alignTo.RotationRads + (float)Math.PI); //Create rotation matrix
                    Vector2 idealPosition = Vector2.Transform((camera.Position - alignTo.Position) * new Vector2(-1, 1), doRot); //Calculate ideal draw position including rotation
                    Matrix view = Matrix.CreateLookAt(new Vector3(idealPosition, 1), new Vector3(idealPosition, 0), rot); //CreateLookAt above the scene
                    Matrix projection = Matrix.CreateOrthographic(Shell.Resolution.X / (camera.ZoomFactor.X * alignTo.Size.X), Shell.Resolution.Y / (camera.ZoomFactor.Y * alignTo.Size.Y), 0.1f, 100f); //Create orthographic projection based on camera zoom and sceen resolution
                    return (world, view, projection);
                }
            }
            /// <summary>
            /// Abstract implementation of vertex calculation function
            /// </summary>
            /// <param name="gd">Application GraphicsDevice</param>
            public abstract void CalculateVertices(GraphicsDevice gd);
            /// <summary>
            /// Array of draw vertices for this object
            /// </summary>
            public IVertexType[] VertexArray
            {
                get;
                protected set;
            }
            /// <summary>
            /// Draw vertices to the world
            /// </summary>
            /// <param name="device">Applicable GraphicsDevice object</param>
            /// <param name="camera">Applicable draw Camera object</param>
            /// <param name="alignTo">WorldEntity that this draw operation should be aligned to</param>
            public virtual void DrawVertices(GraphicsDevice device, Camera camera, WorldEntity alignTo)
            {
                (Matrix, Matrix, Matrix) drawMatrices;
                if(camera is null) //Get matrix transforms for draw operation
                {
                    drawMatrices = GetDrawTransforms(alignTo);
                }
                else
                {
                    drawMatrices = GetDrawTransforms(camera, alignTo);
                }
                BasicEffect drawEffect = DrawEffect; //Create draw effect
                drawEffect.VertexColorEnabled = true;
                drawEffect.World = drawMatrices.Item1; //Apply world matrix
                drawEffect.View = drawMatrices.Item2; //Apply view matrix
                drawEffect.Projection = drawMatrices.Item3; //Apply projection matrix
                foreach(EffectPass pass in drawEffect.CurrentTechnique.Passes) //For each effect pass perform draw operation
                {
                    pass.Apply(); //Apply pass
                    Type generic = Type.MakeGenericMethodParameter(0);
                    MethodInfo me = typeof(GraphicsDevice).GetMethod("DrawUserPrimitives", new [] { typeof(PrimitiveType), generic.MakeArrayType(), typeof(int), typeof(int) }); //Get generic draw method
                    Type genType = VertexArray[0].GetType(); //Get vertex type
                    var arrayParam = Array.CreateInstance(genType, VertexArray.Length); //Create correct array instance
                    VertexArray.CopyTo(arrayParam, 0); //Copy in vertex type
                    me.MakeGenericMethod(genType).Invoke(device, new object[] { PrimitiveType, arrayParam, 0, VertexArray.Length / VerticesPerPrimitive }); //Invoke generic draw method
                }
            }
            /// <summary>
            /// Draw vertices with respect to an aligned world entity
            /// </summary>
            /// <param name="device">Applicable GraphicsDevice object</param>
            /// <param name="alignTo">WorldEntity that this draw operation should be aligned to</param>
            public virtual void DrawVertices(GraphicsDevice device, WorldEntity alignTo)
            {
                DrawVertices(device, null, alignTo);
            }
            /// <summary>
            /// Boolean property; should this be aligned to a specific WorldEntity?
            /// </summary>
            public Boolean AlignToEntity { get; set; }
            /// <summary>
            /// The BasicEffect to draw
            /// </summary>
            public abstract BasicEffect DrawEffect { get; }
            /// <summary>
            /// The primitive type to draw
            /// </summary>
            public abstract PrimitiveType PrimitiveType { get; }
            /// <summary>
            /// The number of vertices consisting each primitive
            /// </summary>
            public abstract int VerticesPerPrimitive { get; }
        }
        /// <summary>
        /// Perform a modulo operation on a radian value with respect to 2*Pi
        /// </summary>
        /// <param name="radians">Input value as radians</param>
        /// <returns>Mod(2Pi,radians)</returns>
        public static Double Mod2PI(Double radians)
        {
            while (radians >= Math.PI * 2) { radians -= Math.PI * 2; }
            while (radians < 0) { radians += Math.PI * 2; }
            return radians;
        }
        /// <summary>
        /// Normalizes angle comparisons to take into account the 0-2pi modulus circle
        /// </summary>
        /// <param name="a">Angle a</param>
        /// <param name="b">Angle b</param>
        /// <returns>Normalized angle difference</returns>
        public static Double AngleDifference(Double a, Double b)
        {
            Double flatDif = a - b;
            if (flatDif < -Math.PI) { return flatDif + (Math.PI * 2); }
            else if (flatDif >= Math.PI) { return flatDif - (Math.PI * 2); }
            return flatDif;
        }
        /// <summary>
        /// Calculates the bearing of a vector.
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <returns>Bearing of the vector with respect to the origin</returns>
        public static Double VectorToBearing(Vector2 a)
        {
            double y = a.Y;
            double x = a.X;
            double bearing = 0;
            if (x > 0)
            {
                bearing = Math.Acos(-y / a.Length());
            }
            else if (x < 0)
            {
                bearing = Math.Acos(y / a.Length()) + Math.PI;
            }
            else if (x == 0)
            {
                bearing = y > 0 ? Math.PI : 0;
            }
            return bearing;
        }
        /// <summary>
        /// Calculates the bearing of a point
        /// </summary>
        /// <param name="a">Input point</param>
        /// <returns>Bearing of the point with respect to the origin</returns>
        public static Double PointToBearing(Point a)
        {
            return VectorToBearing(new Vector2(a.X, a.Y));
        }
        /// <summary>
        /// Defines geometric polygons as a series of directed lines. Polygons are constructed clockwise; the righthand side of a line should be the shape interior.
        /// </summary>
        [Serializable]
        public class Polygon : ICollider, IEnumerable<Trace>
        {
            List<Trace> _lineTraces = new List<Trace>(); //Traces making up the polygon edges
            /// <summary>
            /// Enumerable Trace property
            /// </summary>
            /// <param name="index">Index of the line trace</param>
            /// <returns>An individual edge of the polygon</returns>
            public Trace this[int index]
            {
                get { return _lineTraces[index]; }
                set { _lineTraces[index] = value; }
            }
            /// <summary>
            /// Returns the trace enumerator.
            /// </summary>
            /// <returns>The trace enumerator.</returns>
            public IEnumerator<Trace> GetEnumerator()
            {
                return _lineTraces.GetEnumerator();
            }
            /// <summary>
            /// Returns the trace enumerator.
            /// </summary>
            /// <returns>The enumerator, which is the trace enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            /// <summary>
            /// Integer property representing the number of edges in the polygon.
            /// </summary>
            public int Count
            {
                get { return _lineTraces.Count; }
            }
            private double _maxDistanceFromLocalOrigin = 0;
            /// <summary>
            /// Double property representing the greatest distance the polygon has from its origin.
            /// </summary>
            /// <returns>The maximum distance extent</returns>
            public double GetMaximumExtent()
            {
                return _maxDistanceFromLocalOrigin;
            }
            /// <summary>
            /// Initialize a polygon from a Vector2 array.
            /// </summary>
            /// <param name="vectors">An array of Vector2</param>
            public void InstanceFromVectors(Vector2[] vectors)
            {
                InstanceFromVectors(vectors, new Vector2(), new Vector2());
            }
            /// <summary>
            /// Initialize a polygon from a Vector2 array.
            /// </summary>
            /// <param name="vectors">An array of Vector2</param>
            /// <param name="normalizingOrigin">Vector origin to normalize input vectors</param>
            /// <param name="distanceBoundsOrigin">Vector origin to normalize the maximum extent calculation</param>
            public void InstanceFromVectors(Vector2[] vectors, Vector2 normalizingOrigin, Vector2 distanceBoundsOrigin)
            {
                _maxDistanceFromLocalOrigin = 0;
                _lineTraces = new List<Trace>();
                for(int i = 0; i < vectors.Length; i++) //For each vector
                {
                    Vector2 origin = vectors[i] - normalizingOrigin; //Calculate normalized origin
                    Vector2 terminus = i < vectors.Length - 1 ? vectors[i + 1] : vectors[0]; //Calculate edge terminating point
                    terminus = terminus - normalizingOrigin; //Normalize terminating point
                    _lineTraces.Add(new Trace(origin, terminus)); //Create new edge trace and add to list
                    Vector2 distanceBoundsVector = origin - distanceBoundsOrigin; //Normalize origin with respect to distance bounds origin
                    if(distanceBoundsVector.Length() > _maxDistanceFromLocalOrigin) { _maxDistanceFromLocalOrigin = distanceBoundsVector.Length(); } //Update maximum extent length if it is greater than the previously recorded value.
                }
            }
            /// <summary>
            /// Initialize a polygon from a Point array.
            /// </summary>
            /// <param name="points">An array of Point</param>
            public void InstanceFromPoints(Point[] points)
            {
                InstanceFromVectors(points.Select(x => VNFUtils.ConvertPoint(x)).ToArray(), new Vector2(), new Vector2());
            }
            /// <summary>
            /// Initialize a polygon from a Point array.
            /// </summary>
            /// <param name="vectors">An array of Point</param>
            /// <param name="normalizingOrigin">Point origin to normalize input points</param>
            /// <param name="distanceBoundsOrigin">Point origin to normalize the maximum extent calculation</param>
            public void InstanceFromPoints(Point[] points, Point normalizingOrigin, Point distanceBoundsOrigin)
            {
                InstanceFromVectors(points.Select(x => VNFUtils.ConvertPoint(x)).ToArray(), VNFUtils.ConvertPoint(normalizingOrigin), VNFUtils.ConvertPoint(distanceBoundsOrigin));
            }
            /// <summary>
            /// Get the intersection points for this Polygon for an input Trace.
            /// </summary>
            /// <param name="trace">The intersecting trace.</param>
            /// <returns>A tuple representing information about each intersection, including its location, the trace being intersected, and its index within the polygon</returns>
            public (Vector2[], Trace[], int[]) GetTraceIntersections(Trace trace)
            {
                List<Trace> intersectionTraces = new List<Trace>();
                List<Vector2> intersections = new List<Vector2>();
                List<int> intersectionIndices = new List<int>();
                int index = 0;
                foreach(Trace selfTrace in _lineTraces) //Check each trace within the polygon
                {
                    bool edgeMatches = false;
                    Vector2? intersect = trace.GetIntersection(selfTrace, out edgeMatches); //Check for intersection
                    for (int i = 0; i < (edgeMatches ? 2 : 1); i++) //An adjustment is made so that edge matches don't throw off the calculator for whether a point is inside the polygon, which relies on an even number of intersections.
                    {
                        if (intersect != null)
                        {
                            intersectionTraces.Add(selfTrace); //Add to trace list
                            intersections.Add((Vector2)intersect); //Add to intersection list
                            intersectionIndices.Add(index); //Add to index list
                        }
                    }
                    index++;
                }
                return (intersections.ToArray(), intersectionTraces.ToArray(), intersectionIndices.ToArray()); //Return found intersections
            }
            /// <summary>
            /// Get the first intersection of a trace with the polygon
            /// </summary>
            /// <param name="trace">Trace to check for intersection</param>
            /// <returns>Null, or the intersection as a Vector2</returns>
            public Vector2? GetFirstIntersection(Trace trace)
            {
                Vector2 closest = new Vector2();
                int i = 0;
                Vector2[] intersections = GetTraceIntersections(trace).Item1; //Get the list of intersections if any are found
                if (intersections.Length == 0) { return null; } //Return null for no intersections
                else
                {
                    foreach (Vector2 intersect in intersections) //For each intersection
                    {
                        if (i == 0 || (intersect - trace.Origin).Length() < (closest - trace.Origin).Length()) //Check if this intersection is the closest
                        {
                            closest = intersect; //And set the output if it is the closest
                        }
                    }
                    return closest;
                }
            }
            /// <summary>
            /// Check whether a Vector2 coordinate is inside the polygon
            /// </summary>
            /// <param name="coord">Vector2 coordinate to check</param>
            /// <returns>A boolean representing whether it is in the polygon</returns>
            public Boolean Contains(Vector2 coord)
            {
                Trace sightline = new Trace(coord, new Vector2(1000000, coord.Y)); //Create a sightline
                var intersects = GetTraceIntersections(sightline); //Check for intersections with the sightline
                return intersects.Item1.Count() % 2 != 0; //If the intersection number is divisible by two, the point is outside the polygon.
            }
            /// <summary>
            /// Polygon constructor from a rectangle.
            /// </summary>
            /// <param name="rectangle">Rectangle to convert to a polygon</param>
            public Polygon(Rectangle rectangle)
            {
                InstanceFromVectors(new Vector2[] { new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Bottom) }, new Vector2(), new Vector2());
            }
            /// <summary>
            /// Polygon constructor from a rectangle.
            /// </summary>
            /// <param name="rectangle">Rectangle to convert to a polygon</param>
            /// <param name="normalizingOrigin">Origin to normalize coordinates</param>
            /// <param name="distanceBoundsOrigin">Distance bounds origin to normalize max extent</param>
            public Polygon(Rectangle rectangle, Vector2 normalizingOrigin, Vector2 distanceBoundsOrigin)
            {
                InstanceFromVectors(new Vector2[] { new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Bottom) }, normalizingOrigin, distanceBoundsOrigin);
            }
            /// <summary>
            /// Polygon constructor from a vertex array of Vector2
            /// </summary>
            /// <param name="vertices">Vector2 array to convert into a Polygon</param>
            public Polygon(Vector2[] vertices)
            {
                InstanceFromVectors(vertices, new Vector2(), new Vector2());
            }
            /// <summary>
            /// Polygon constructor from a vertex array of Vector2
            /// </summary>
            /// <param name="vertices">Vector2 array to convert into a Polygon</param>
            /// <param name="normalizingOrigin">Origin to normalize coordinates</param>
            /// <param name="distanceBoundsOrigin">Distance bounds origin to normalize max extent</param>
            public Polygon(Vector2[] vertices, Vector2 normalizingOrigin, Vector2 distanceBoundsOrigin)
            {
                InstanceFromVectors(vertices, normalizingOrigin, distanceBoundsOrigin);
            }
            /// <summary>
            /// Checks whether any edges of another polygon intersect with this one
            /// </summary>
            /// <param name="polygon">Other polygon to check</param>
            /// <returns>Boolean of whether or not any edges intersect</returns>
            public Boolean EdgesIntersect(Polygon polygon)
            {
                foreach(Trace edge in polygon) //For each edge of the other polygon
                {
                    var intersections = GetTraceIntersections(edge); //Check for intersections with this one
                    if(intersections.Item1.Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// Checks whether a trace intersects this polygon
            /// </summary>
            /// <param name="trace">Trace to check</param>
            /// <returns>A boolean value for whether it intersects</returns>
            public Boolean Intersects(Trace trace)
            {
                var intersections = GetTraceIntersections(trace); //Get any intersections
                if (intersections.Item1.Length > 0) //Return true if there are any intersections
                {
                    return true;
                }
                else { return false; }
            }
            /// <summary>
            /// Check whether this polygon collides with any ICollider
            /// </summary>
            /// <param name="collider">The ICollider to check against</param>
            /// <returns>Boolean value for whether there is a collision</returns>
            public Boolean Collides(ICollider collider)
            {
                if (collider is Polygon) { return EdgesIntersect((Polygon)collider); }
                else if (collider is RadialCollider) { return ((RadialCollider)collider).Collides(this); }
                return false;
            }
            /// <summary>
            /// Gets the collider impingement, which is the extent to which this polygon impinges upon another collider
            /// </summary>
            /// <param name="collider">The collider that is being impinged on</param>
            /// <returns>A trace representing the impingement line</returns>
            public Trace GetImpingementOn(ICollider collider)
            {
                if (collider is Polygon) { return null; } //Impingement calculations for two polygons are not yet implemented
                else if (collider is RadialCollider) { return ((RadialCollider)collider).GetImpingementOn(this).Flip(); } //For a RadialCollider, flip the impingement that the collider has on this polygon
                return null;
            }
            /// <summary>
            /// Collision resolution for a DynamicEntity with a polygonal collider is not yet implemented
            /// </summary>
            /// <param name="selfAttach">Attached Dynamic entity</param>
            /// <param name="remoteAttach">Remote collision entity</param>
            public void ResolveCollision(DynamicEntity selfAttach, WorldEntity remoteAttach)
            {
                return;
            }
            /// <summary>
            /// Scale this polygon as an ICollider
            /// </summary>
            /// <param name="origin">Origin for scale operation</param>
            /// <param name="scale">Scale factor</param>
            /// <returns>A newly scaled polygon</returns>
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
            /// <summary>
            /// Rotate this polygon as an ICollider
            /// </summary>
            /// <param name="origin">Center of rotation</param>
            /// <param name="angle">Rotation angle in radians</param>
            /// <returns>A newly rotated polygon</returns>
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
            /// <summary>
            /// Translate this polygon as an ICollider
            /// </summary>
            /// <param name="translation">Vector translation</param>
            /// <returns>A newly translated polygon</returns>
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
            /// <summary>
            /// The origin point of the trace, as a Vector2
            /// </summary>
            public Vector2 Origin
            {
                get
                {
                    return _origin;
                }
            }
            /// <summary>
            /// The terminating point of the trace, as a Vector2
            /// </summary>
            public Vector2 Terminus
            {
                get
                {
                    return _terminus;
                }
            }
            object _max = null;
            /// <summary>
            /// A vector representing the max X and Y values of this trace.
            /// </summary>
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
            /// <summary>
            /// A vector representing the minimum X and Y values of this trace.
            /// </summary>
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
            /// <summary>
            /// The length of this trace calculated as a double value using trigonometry.
            /// </summary>
            public Double Length
            {
                get
                {
                    if( _length == null)
                    {
                        _length = Math.Sqrt(Math.Pow(Math.Abs(_terminus.X - _origin.X), 2) + Math.Pow(Math.Abs(_terminus.Y - _origin.Y), 2));
                        if(Double.IsNaN((double)_length)) { _length = 0d; }
                    }
                    return (Double)_length;
                }
            }
            object _alignedVector;
            /// <summary>
            /// Vector2 property returning an aligned vector representation of this line trace.
            /// </summary>
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
            /// <summary>
            /// Double property representing the bearing of this Trace from the origin point to the terminating point.
            /// </summary>
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
            /// <summary>
            /// Double property representing the slope of the Trace line.
            /// </summary>
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
            /// <summary>
            /// The hypothetical Y-Intercept of this line trace.
            /// </summary>
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
            /// <summary>
            /// Multiplication operator for a trace with a double. Extends or diminishes the trace by the double factor.
            /// </summary>
            /// <param name="a">Trace operand.</param>
            /// <param name="b">Double operand.</param>
            /// <returns>Scaled line Trace</returns>
            public static Trace operator * (Trace a, Double b)
            {
                return new Trace(a.Origin, a.Bearing, a.Length * b);
            }
            /// <summary>
            /// Gets the component of this Trace that is perpendicular to another Trace.
            /// </summary>
            /// <param name="trace">Any other Trace.</param>
            /// <returns>The component of this Trace that is perpendicular to the Trace parameter.</returns>
            public Trace GetPerpendicularComponentTo(Trace trace)
            {
                Double recenteredBearing = Mod2PI(Bearing - trace.Bearing); //Calculate relative bearing
                Double component = Math.Sin(recenteredBearing) * Length; //Get component of the trace length with respect to the bearing
                Double componentMagnitude = Math.Abs(component); //Get component as an absolute magnitude
                Double componentBearing = Mod2PI(trace.Bearing + (Math.Sign(component) * (Math.PI / 2))); //Get component bearing
                Trace componentTrace = new Trace(Origin, componentBearing, componentMagnitude); //A new trace is created from the extracted component.
                return componentTrace;
            }
            /// <summary>
            /// Gets the component of a Vector that is perpendicular to another Vector.
            /// </summary>
            /// <param name="getComponentFrom">Vector2 from which the component is extracted</param>
            /// <param name="thePerpendicularOfThis">Vector2 that the component should be perpendicular to</param>
            /// <returns></returns>
            public static Vector2 GetPerpendicularComponent(Vector2 getComponentFrom, Vector2 thePerpendicularOfThis)
            {
                Trace from = new Trace(getComponentFrom);
                return from.GetPerpendicularComponentTo(new Trace(thePerpendicularOfThis)).AsAlignedVector;
            }
            /// <summary>
            /// Gets the component of this Trace that is parallel to another Trace.
            /// </summary>
            /// <param name="trace">Any other trace.</param>
            /// <returns>The component of this Trace that is parallel to the Trace parameter.</returns>
            public Trace GetAlignedComponentTo(Trace trace)
            {
                Double recenteredBearing = Mod2PI(Bearing - trace.Bearing); //Calculate the relative bearing
                Double component = Math.Cos(recenteredBearing) * Length; //Get component of trace length with respect to the bearing
                Double componentMagnitude = Math.Abs(component); //Get component as absolute magnitude
                Double componentBearing = Mod2PI(trace.Bearing + (component < 0 ? Math.PI : 0)); //Get component bearing
                Trace componentTrace = new Trace(Origin, componentBearing, componentMagnitude); //A new trace is created from the extracted component
                return componentTrace;
            }
            /// <summary>
            /// Gets the component of a Vector that is parallel to another Vector.
            /// </summary>
            /// <param name="getComponentFrom">Vector2 from which the component is extracted</param>
            /// <param name="theAlignmentOfThis">Vector2 that the component should be parallel to</param>
            /// <returns></returns>
            public static Vector2 GetAlignedComponent(Vector2 getComponentFrom, Vector2 theAlignmentOfThis)
            {
                Trace from = new Trace(getComponentFrom);
                return from.GetAlignedComponentTo(new Trace(theAlignmentOfThis)).AsAlignedVector;
            }
            /// <summary>
            /// Function to determine if a Vector2 is on the right hand side of this trace
            /// </summary>
            /// <param name="coord">Vector2 coordinate</param>
            /// <returns>A boolean value dependent on whether the coordinate is on the right hand side.</returns>
            public Boolean IsCoordOnRightHandSide(Vector2 coord)
            {
                Trace toCoord = new Trace(Origin, coord);
                Double normalizedBearing = toCoord.Bearing - Bearing; //The bearing of the coordinate from the Trace origin is compared to the Trace bearing
                if (normalizedBearing < 0) { normalizedBearing += (Math.PI * 2); }
                return normalizedBearing < Math.PI;
            }
            /// <summary>
            /// Reverses the origin and terminating points for this Trace.
            /// </summary>
            /// <returns>The reversed Trace.</returns>
            public Trace Flip()
            {
                return new Trace(Terminus, Origin);
            }
            private Vector2 _origin;
            private Vector2 _terminus;
            /// <summary>
            /// Trace constructor.
            /// </summary>
            /// <param name="origin">The start point of the line.</param>
            /// <param name="terminus">The end point of the line.</param>
            public Trace(Vector2 origin, Vector2 terminus)
            {
                _origin = origin;
                _terminus = terminus;
            }
            /// <summary>
            /// Trace constructor using bearing.
            /// </summary>
            /// <param name="origin">The start point of the line.</param>
            /// <param name="bearing">The line bearing.</param>
            /// <param name="length">The length of the line</param>
            public Trace(Vector2 origin, double bearing, double length)
            {
                _origin = origin;
                double x_additive = Math.Sin(bearing) * length;
                double y_additive = -Math.Cos(bearing) * length;
                _terminus = origin + new Vector2((float)x_additive, (float)y_additive);
            }
            /// <summary>
            /// Trace constructor using a default origin.
            /// </summary>
            /// <param name="terminus">The terminating point of the line.</param>
            public Trace(Vector2 terminus)
            {
                _origin = new Vector2(); //The origin is set to (0,0), the default origin.
                _terminus = terminus;
            }
            /// <summary>
            /// Get the intersection between this Trace and another Trace.
            /// </summary>
            /// <param name="trace">Another Trace</param>
            /// <returns>Null, or the location of an intersection as Vector2</returns>
            public Vector2? GetIntersection(Trace trace)
            {
                bool outBool = false;
                return GetIntersection(trace, out outBool);
            }
            /// <summary>
            /// Get the intersection between this Trace and another Trace.
            /// </summary>
            /// <param name="trace">Another trace</param>
            /// <param name="edgeMatches">Out parameter indicating whether edges overlap along a length</param>
            /// <returns></returns>
            public Vector2? GetIntersection(Trace trace, out bool edgeMatches)
            {
                edgeMatches = false;
                if (Double.IsNaN(YIntercept) && Double.IsNaN(trace.YIntercept)) //If both lines are vertical
                {
                    if (Origin.X == trace.Origin.X) //Check if they are at the same X coordinate
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
                else if (Double.IsNaN(YIntercept)) //If one line is vertical
                {
                    double passY = (trace.Slope * Origin.X) + trace.YIntercept;
                    if (Origin.X <= trace.Max.X && Origin.X >= trace.Min.X && passY >= Min.Y && passY <= Max.Y) { return new Vector2(Origin.X, (float)passY); }
                    else { return null; }
                }
                else if (Double.IsNaN(trace.YIntercept)) //If the other line is vertical
                {
                    double passY = (Slope * trace.Origin.X) + YIntercept;
                    if (trace.Origin.X <= Max.X && trace.Origin.X >= Min.X && passY >= trace.Min.Y && passY <= trace.Max.Y) { return new Vector2(trace.Origin.X, (float)passY); }
                    else { return null; }
                }
                else
                {
                    if (Slope.Equals(trace.Slope)) //Check if the lines have the same slope
                    {
                        if (YIntercept.Equals(trace.YIntercept)) //And the same Y Intercept
                        {
                            edgeMatches = true;
                            if (Origin.Y >= trace.Min.Y && Origin.Y <= trace.Max.Y) { return Origin; }
                            else if (Terminus.Y >= trace.Min.Y && Terminus.Y <= trace.Max.Y) { return Terminus; }
                            else { return null; }
                        }
                        else { return null; }
                    }
                    else //Apply standard straight line intersection solve
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
            /// <summary>
            /// Checks for intersections between this Trace and another Trace
            /// </summary>
            /// <param name="trace">Another Trace</param>
            /// <returns>Boolean value for whether they intersect</returns>
            public Boolean Intersects(Trace trace)
            {
                return GetIntersection(trace) != null;
            }
            /// <summary>
            /// Checks for intersections between this Trace and a collider
            /// </summary>
            /// <param name="collider">A collider implementing ICollider</param>
            /// <returns>Boolean value for whether they intersect</returns>
            public Boolean Intersects(ICollider collider)
            {
                return collider.Intersects(this);
            }
            /// <summary>
            /// Get the first intersection (if applicable) between this Trace and a Collider
            /// </summary>
            /// <param name="collider">A collider implementing ICollider</param>
            /// <returns>Null, or the first intersection between them</returns>
            public Vector2? GetFirstIntersection(ICollider collider)
            {
                return collider.GetFirstIntersection(this);
            }
            /// <summary>
            /// Finds the closest Trace to a given Vector2 coordinate
            /// </summary>
            /// <param name="coordinate">A coordinate as a Vector2</param>
            /// <param name="distanceLimit">The distance limit for the search</param>
            /// <returns>The nearest Trace to the coordinate point</returns>
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
            /// <summary>
            /// Get intersections between this Trace and a WorldEntity hitbox
            /// </summary>
            /// <param name="worldEntity">A WorldEntity with a hitbox</param>
            /// <returns>An array of Vector2 representing collision points</returns>
            public Vector2[] GetHitBoxIntersections(WorldEntity worldEntity)
            {
                Rectangle hitbox = worldEntity.Hitbox; //Get hitbox rectangle
                Trace[] edges = new Trace[4];
                //Rectangle edges are converted to Traces
                edges[0] = new Trace(new Vector2(hitbox.Left, hitbox.Top), new Vector2(hitbox.Right, hitbox.Top));
                edges[1] = new Trace(new Vector2(hitbox.Right, hitbox.Top), new Vector2(hitbox.Right, hitbox.Bottom));
                edges[2] = new Trace(new Vector2(hitbox.Right, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Bottom));
                edges[3] = new Trace(new Vector2(hitbox.Left, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Top));
                List<Vector2> intersections = new List<Vector2>(); 
                foreach(Trace edge in edges)
                {
                    Vector2? intersect = GetIntersection(edge); //Check intersection for each edge
                    if(intersect != null) { intersections.Add((Vector2)intersect); }
                }
                return intersections.ToArray();
            }
            /// <summary>
            /// Checks if this Trace intersects a WorldEntity hitbox
            /// </summary>
            /// <param name="worldEntity">A WorldEntity with a hitbox</param>
            /// <returns>A Boolean value representing whether or not there is an intersection</returns>
            public Boolean IntersectsHitBox(WorldEntity worldEntity)
            {
                Rectangle hitbox = worldEntity.Hitbox; //Get hitbox rectangle
                Trace[] edges = new Trace[4];
                //Rectangle edges are converted to Traces
                edges[0] = new Trace(new Vector2(hitbox.Left, hitbox.Top), new Vector2(hitbox.Right, hitbox.Top));
                edges[1] = new Trace(new Vector2(hitbox.Right, hitbox.Top), new Vector2(hitbox.Right, hitbox.Bottom));
                edges[2] = new Trace(new Vector2(hitbox.Right, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Bottom));
                edges[3] = new Trace(new Vector2(hitbox.Left, hitbox.Bottom), new Vector2(hitbox.Left, hitbox.Top));
                foreach (Trace edge in edges)
                {
                    if (Intersects(edge)) { return true; } //Return true for any intersection
                }
                return false;
            }
            /// <summary>
            /// Gets the first intersection with a WorldEntity texture if applicable.
            /// </summary>
            /// <param name="worldEntity">A WorldEntity with a texture</param>
            /// <param name="traceDivisions">The number of divisions of the trace to check</param>
            /// <returns>Null, or the first texture intersection</returns>
            public Vector2? GetFirstTextureIntersection(WorldEntity worldEntity, int traceDivisions)
            {
                Vector2[] hitBoxIntersects = GetHitBoxIntersections(worldEntity); //Check that the hitbox is intersected
                Rectangle hitbox = worldEntity.Hitbox;
                if(hitBoxIntersects.Length == 0) { return null; } //Return null if not
                else //Otherwise
                {
                    Vector2 origin = new Vector2();
                    Vector2 terminus = new Vector2();
                    //Based on hitbox intersection, the trace line to scan is defined
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
                    Trace scan = new Trace(origin, terminus); //The trace scan line is created
                    Vector2 divisionVector = scan.AsAlignedVector / traceDivisions; //It is divided into a number of divisions
                    Vector2[] checkPoints = new Vector2[traceDivisions + 1];
                    checkPoints[0] = origin;
                    for(int i = 1; i < traceDivisions; i++) //For each division a point to check is calculated
                    {
                        checkPoints[i] = origin + (divisionVector * i);
                    }
                    checkPoints[traceDivisions] = terminus;
                    foreach(Vector2 check in checkPoints)
                    {
                        if(worldEntity.TextureAwareInBounds(check)) //Each point is checked against the texture to see if there is an intersection
                        {
                            return check;
                        }
                    }
                    return null;
                }
            }
            /// <summary>
            /// A boolean function for if the Trace intersects a WorldEntity texture
            /// </summary>
            /// <param name="worldEntity">A WorldEntity with a texture</param>
            /// <param name="traceDivisions">The number of divisions of the trace to check</param>
            /// <returns>A boolean value representing whether or not there is a collision</returns>
            public Boolean IntersectsTexture(WorldEntity worldEntity, int traceDivisions)
            {
                return GetFirstTextureIntersection(worldEntity, traceDivisions) != null;
            }
            private BasicEffect _drawEffect = null;
            //Draw effect for rendering this trace
            public override BasicEffect DrawEffect
            {
                get { return _drawEffect; }
            }
            //Primitive type for rendering this trace
            public override PrimitiveType PrimitiveType
            {
                get
                {
                    return PrimitiveType.LineList;
                }
            }
            //Each trace has two vertices
            public override int VerticesPerPrimitive
            {
                get
                {
                    return 2;
                }
            }
            /// <summary>
            /// Method to calculate vertices to be drawn
            /// </summary>
            /// <param name="gd">Applicable GraphicsDevice</param>
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
            /// <summary>
            /// Property to get and set vertex draw colours
            /// </summary>
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
            /// <summary>
            /// Method to scale this Trace
            /// </summary>
            /// <param name="origin">Scale origin as Vector2</param>
            /// <param name="scale">Scale factor as Vector2</param>
            /// <returns>The scaled Trace</returns>
            public Trace Scale(Vector2 origin, Vector2 scale)
            {
                return new Trace(((Origin - origin) * scale) + origin, ((Terminus - origin) * scale) + origin);
            }
            /// <summary>
            /// Method to rotate this Trace
            /// </summary>
            /// <param name="origin">Center of rotation as Vector2</param>
            /// <param name="angle">Rotation angle as Double (radians)</param>
            /// <returns>The rotated Trace</returns>
            public Trace Rotate(Vector2 origin, Double angle)
            {
                Vector2 normOrigin = Origin - origin;
                Vector2 normTerminus = Terminus - origin;
                Matrix rot = Matrix.CreateRotationZ((float)angle);
                return new Trace((Vector2.Transform(normOrigin, rot)) + origin, (Vector2.Transform(normTerminus, rot)) + origin);
            }
            /// <summary>
            /// Method to translate this Trace
            /// </summary>
            /// <param name="translation">Desired translation as Vector2</param>
            /// <returns>The translated Trace</returns>
            public Trace Translate(Vector2 translation)
            {
                return new Trace(Origin + translation, Terminus + translation);
            }
        }
    }
}
