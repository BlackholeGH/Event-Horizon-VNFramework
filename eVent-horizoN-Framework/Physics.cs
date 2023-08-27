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
using static VNFramework.GraphicsTools;

namespace VNFramework
{
    //IVNFBehaviours are partially implemented within the physics context.
    public static partial class Behaviours
    {
        /// <summary>
        /// DragPhysicsBehaviour implements drag physics behaviour for the physics medium.
        /// </summary>
        [Serializable]
        public class DragPhysicsBehaviour : IVNFBehaviour
        {
            /// <summary>
            /// Mass fluid density for the drag simulation
            /// </summary>
            public static readonly Double MassFluidDensity = 0.000002d;
            public DragPhysicsBehaviour()
            {

            }
            /// <summary>
            /// Update functionality for the drag physics behaviour
            /// </summary>
            /// <param name="worldEntity">The WorldEntity that owns this behaviour</param>
            public void UpdateFunctionality(WorldEntity worldEntity)
            {
                if(worldEntity is DynamicEntity)
                {
                    DynamicEntity dynamicEntity = (DynamicEntity)worldEntity;
                    if(dynamicEntity.ImpulseKilledByActiveCollision) { return; } //A collision kills additional impulse that might otherwise be added by drag
                    Double dragMagnitude = (Math.Pow(dynamicEntity.Velocity.Length(), 2) * Math.Pow(dynamicEntity.Collider.GetMaximumExtent(), 2)) * MassFluidDensity; //Magnitude is dependent on entity velocity, size, and the mass fluid density
                    Double magnitudeLimit = dynamicEntity.Velocity.Length() * dynamicEntity.Mass; //The drag is limited to a mass dependent value
                    if(magnitudeLimit == 0) { magnitudeLimit = 1; }
                    if(dragMagnitude >= magnitudeLimit) { dragMagnitude = magnitudeLimit - 0.001d; } //Check for limit excession
                    Vector2 dragForce = new Trace(new Vector2(), new Trace(dynamicEntity.Velocity).Flip().Bearing, dragMagnitude).AsAlignedVector; //Calculate drag force
                    dynamicEntity.ApplyForce(dragForce); //Apply drag force to DynamicEntity
                }
            }
            public void Clear() { }
        }
        /// <summary>
        /// DynamicWASDControlBehaviour allows a user to move a DynamicEntity using the keyboard.
        /// </summary>
        [Serializable]
        public class DynamicWASDControlBehaviour : IVNFBehaviour
        {
            public static Double Speed = 8d; //Set movement speed
            public DynamicWASDControlBehaviour()
            {

            }
            /// <summary>
            /// Clear keyboard state
            /// </summary>
            public void Clear()
            {
                _lastState = new KeyboardState();
            }
            KeyboardState _lastState = new KeyboardState();
            /// <summary>
            /// Update functionality for this behaviour
            /// </summary>
            /// <param name="worldEntity">The behaviour owner</param>
            public void UpdateFunctionality(WorldEntity worldEntity)
            {
                if (worldEntity is DynamicEntity && !(Shell.UsingKeyboardInputs != null && Shell.UsingKeyboardInputs != worldEntity && !Shell.DefaultShell.IsActive)) //If WASD movement is applicable
                {
                    DynamicEntity dynamicEntity = (DynamicEntity)worldEntity;
                    KeyboardState kState = Keyboard.GetState(); //Get keyboard state
                    if (kState.IsKeyDown(Keys.W)) //Check and apply forward motion
                    {
                        Vector2 targetForwardTrace = new Trace(new Vector2(), dynamicEntity.RotationRads, Speed).AsAlignedVector;
                        Vector2 forwardDelta = targetForwardTrace - dynamicEntity.Velocity;
                        dynamicEntity.Accelerate(forwardDelta);
                    }
                    if (kState.IsKeyDown(Keys.S)) //Check and apply backwards motion
                    {
                        Vector2 targetBackwardTrace = new Trace(new Vector2(), dynamicEntity.RotationRads, Speed).Flip().AsAlignedVector;
                        Vector2 backwardDelta = targetBackwardTrace - dynamicEntity.Velocity;
                        dynamicEntity.Accelerate(backwardDelta);
                    }
                    if (kState.IsKeyDown(Keys.A)) //Check and apply left turning
                    {
                        dynamicEntity.Rotate(-0.1f);
                    }
                    if (kState.IsKeyDown(Keys.D)) //Check and apply right turning
                    {
                        dynamicEntity.Rotate(0.1f);
                    }
                    if (kState.IsKeyDown(Keys.T) && !_lastState.IsKeyDown(Keys.T)) //Toggle whether the camera follows this entity by adding/removing it from Stickers
                    {
                        if(!worldEntity.MyStickers.Contains(Shell.AutoCamera))
                        {
                            worldEntity.MyStickers.Add(Shell.AutoCamera);
                            Shell.WriteLine("Shell autocamera now attached to " + worldEntity.Name + ".");
                        }
                        else
                        {
                            worldEntity.MyStickers.Remove(Shell.AutoCamera);
                            Shell.WriteLine("Shell autocamera detached from " + worldEntity.Name + ".");
                        }
                    }
                    _lastState = kState;
                }
            }
        }
    }
    /// <summary>
    /// Interface for a Collider objects that can be attached to entities
    /// </summary>
    public interface ICollider
    {
        public GraphicsTools.Trace GetImpingementOn(ICollider collider); //Get impingement Trace on another collider
        public Boolean Collides(ICollider collider); //Check whether the Collider collides another Collider
        public Boolean Intersects(Trace trace); //Check whether the Collider intersects a Trace
        public Vector2? GetFirstIntersection(Trace trace); //Get first intersection with a Trace if applicable0
        public double GetMaximumExtent(); //Get the maximum extent of the Collider from its origin
        public ICollider Scale(Vector2 origin, Vector2 scale); //Scale the collider
        public ICollider Rotate(Vector2 origin, double rotation); //Rotate the collider
        public ICollider Translate(Vector2 translation); //Translate the collider
        public void ResolveCollision(DynamicEntity selfAttach, WorldEntity remoteAttach); //Resolve a collision between the DynamicEntity of the collider and another WorldEntity
    }
    /// <summary>
    /// A radial collider object with a point center.
    /// </summary>
    [Serializable]
    public class RadialCollider : ICollider
    {
        /// <summary>
        /// Radius of the collider
        /// </summary>
        public double Radius { get; protected set; }
        /// <summary>
        /// Center point of the collider as a Vector2
        /// </summary>
        public Vector2 CenterPoint { get; protected set; }
        Boolean _translatedCenterPoint = false;
        /// <summary>
        /// RadialCollider constructor
        /// </summary>
        /// <param name="radius">Collider radius as double</param>
        /// <param name="location">Collider location as Vector2</param>
        public RadialCollider(double radius, Vector2 location)
        {
            Radius = radius;
            CenterPoint = location;
        }
        /// <summary>
        /// RadialCollider constructor
        /// </summary>
        /// <param name="radius">Collider radius as double</param>
        /// <param name="location">Collider location as Vector2</param>
        /// <param name="location">Boolean value for whether to use a translated center point in extent calculations</param>
        public RadialCollider(double radius, Vector2 location, Boolean translatedCenterPoint)
        {
            Radius = radius;
            CenterPoint = location;
            _translatedCenterPoint = translatedCenterPoint;
        }
        /// <summary>
        /// Get the maximum extent of the Collider
        /// </summary>
        /// <returns>The maximum extent of the Collider</returns>
        public Double GetMaximumExtent()
        {
            return Radius + (_translatedCenterPoint ? 0 : CenterPoint.Length());
        }
        /// <summary>
        /// Scale this RadialCollider as an ICollider
        /// </summary>
        /// <param name="origin">The origin of the scale operation as Vector2</param>
        /// <param name="scale">The scale factor as Vector2</param>
        /// <returns>The scaled RadialCollider</returns>
        public ICollider Scale(Vector2 origin, Vector2 scale)
        {
            Vector2 scaledCenter = CenterPoint + ((CenterPoint - origin) * scale);
            double scaledRadius = Radius * ((scale.X + scale.Y) / 2);
            return new RadialCollider(scaledRadius, scaledCenter);
        }
        /// <summary>
        /// Rotate this RadialCollider as an ICollider
        /// </summary>
        /// <param name="origin">The origin of rotation as Vector2</param>
        /// <param name="rotation">The rotation angle in radians as double</param>
        /// <returns>The rotated RadialCollider</returns>
        public ICollider Rotate(Vector2 origin, double rotation)
        {
            return new RadialCollider(Radius, CenterPoint); //As a radial point collider cannot be meaningfully rotated, the collider is just cloned
        }
        /// <summary>
        /// Translate this RadialCollider as an ICollider
        /// </summary>
        /// <param name="translation">The translation as a Vector2</param>
        /// <returns>The translated RadialCollider</returns>
        public ICollider Translate(Vector2 translation)
        {
            return new RadialCollider(Radius, CenterPoint + translation, true);
        }
        /// <summary>
        /// Checks whether this RadialCollider collides with another collider
        /// </summary>
        /// <param name="collider">Another collider</param>
        /// <returns>A boolean value for whether or not they collide</returns>
        public Boolean Collides(ICollider collider)
        {
            if(collider is RadialCollider) { return Intersects((RadialCollider)collider); }
            else if(collider is Polygon) { return Intersects((Polygon)collider); }
            return false;
        }
        /// <summary>
        /// Returns a Trace representing calculated impingement on another Collider
        /// </summary>
        /// <param name="collider">Another Collider</param>
        /// <returns>A Trace representing any calculated impingement</returns>
        public Trace GetImpingementOn(ICollider collider)
        {
            if (collider is RadialCollider) { return ImpingementOn((RadialCollider)collider); } //For another RadialCollider
            else if (collider is Polygon) { return ImpingementOn((Polygon)collider); } //For a Polygon
            return null;
        }
        /// <summary>
        /// Boolean value for checking whether this RadialCollider intersects another RadialCollider
        /// </summary>
        /// <param name="radialCollider">Another RadialCollider</param>
        /// <returns>A boolean value for whether they collide</returns>
        private Boolean Intersects(RadialCollider radialCollider)
        {
            return new Trace(CenterPoint, radialCollider.CenterPoint).Length < Radius + radialCollider.Radius; //Calculate using centerpoint distance
        }
        /// <summary>
        /// Gets as a Trace the impingement of this RadialCollider on another RadialCollider
        /// </summary>
        /// <param name="radialCollider">Another RadialCollider</param>
        /// <returns>A Trace representing the extent and direction of collider impingement.</returns>
        private Trace ImpingementOn(RadialCollider radialCollider)
        {
            Trace thisToThat = new Trace(CenterPoint, radialCollider.CenterPoint); //Get Trace from this collider to the other
            Trace thatToThis = new Trace(radialCollider.CenterPoint, CenterPoint); //Get Trace from the other collider to this
            Trace thisFurthestImpingement = new Trace(CenterPoint, thisToThat.Bearing, Radius); //Get Trace along bearing for this
            Trace thatFurthestImpingement = new Trace(radialCollider.CenterPoint, thatToThis.Bearing, radialCollider.Radius); //Get trace along bearing for that
            return new Trace(thatFurthestImpingement.Terminus, thisFurthestImpingement.Terminus); //Get impingement from terminating points
        }
        /// <summary>
        /// Returns an array of intersections for a Trace with this RadialCollider
        /// </summary>
        /// <param name="trace">A Trace</param>
        /// <returns>An array of intersections as applicable</returns>
        public Vector2[] GetIntersections(Trace trace)
        {
            //This function works by solving straight line/circle intersection equations geometricly
            Boolean invert = false;
            if(trace.Slope == Double.PositiveInfinity || trace.Slope == Double.NegativeInfinity)
            {
                invert = true;
                trace = new Trace(new Vector2(trace.Origin.Y, trace.Origin.X), new Vector2(trace.Terminus.Y, trace.Terminus.X)); //For vertical lines, rotate the coordinate space so that the calculation can be performed
            }
            Vector2 centerPoint = invert ? new Vector2(CenterPoint.Y, CenterPoint.X) : CenterPoint;
            //The equation used is (m2+1)x2+2(mc−mq−p)x+(q2−r2+p2−2cq+c2)=0.
            //This is then solved
            //For more information, look up how to solve for straight line collisions with a circle
            Double a = Math.Pow(trace.Slope, 2) + 1;
            Double b = 2 * ((trace.Slope * trace.YIntercept) - (trace.Slope * centerPoint.Y) - centerPoint.X);
            Double c = Math.Pow(centerPoint.Y, 2) - Math.Pow(Radius, 2) + Math.Pow(centerPoint.X, 2) - (2 * trace.YIntercept * centerPoint.Y) + Math.Pow(trace.YIntercept, 2);
            List<Vector2> validOut = new List<Vector2>();
            Double x1 = (-b + Math.Sqrt(Math.Pow(b, 2) - (4 * a * c))) / (2 * a);
            if (!Double.IsNaN(x1))
            {
                Double y1 = (trace.Slope * x1) + trace.YIntercept;
                Vector2 first = new Vector2((float)x1, (float)y1);
                if (first.X <= trace.Max.X && first.X >= trace.Min.X && first.Y <= trace.Max.Y && first.Y >= trace.Min.Y)
                {
                    validOut.Add(invert ? new Vector2(first.Y, first.X) : first);
                }
            }
            Double x2 = (-b - Math.Sqrt(Math.Pow(b, 2) - (4 * a * c))) / (2 * a);
            if (!Double.IsNaN(x2))
            {
                Double y2 = (trace.Slope * x2) + trace.YIntercept;
                Vector2 second = new Vector2((float)x2, (float)y2);
                if (second.X <= trace.Max.X && second.X >= trace.Min.X && second.Y <= trace.Max.Y && second.Y >= trace.Min.Y)
                {
                    validOut.Add(invert ? new Vector2(second.Y, second.X) : second);
                }
            }
            return validOut.ToArray();
        }
        /// <summary>
        /// Retrieves the first intersection of the Trace with this RadialCollider
        /// </summary>
        /// <param name="trace">The Trace to check</param>
        /// <returns>Null, or the first intersection of the Trace with this Collider</returns>
        public Vector2? GetFirstIntersection(Trace trace)
        {
            Vector2[] intersections = GetIntersections(trace); //Get all intersections
            if(intersections.Length > 0)
            {
                if(intersections.Length == 1) { return intersections[0]; }
                else
                {
                    return (intersections[0] - trace.Origin).Length() <= (intersections[1] - trace.Origin).Length() ? intersections[0] : intersections[1]; //Return the closest one
                }
            }
            return null;
        }
        /// <summary>
        /// Checks whether a Trace intersects this collider
        /// </summary>
        /// <param name="trace">A Trace</param>
        /// <returns>A boolean value reglecting whether there is an intersection</returns>
        public Boolean Intersects(Trace trace)
        {
            if (trace.GetClosestTraceFrom(CenterPoint, 1000000).Length < Radius) //Uses GetClosestTraceFrom and checks it against collider radius
            {
                return true;
            }
            else { return false; }
        }
        /// <summary>
        /// Checks whether this RadialCollider intersects a Polygon
        /// </summary>
        /// <param name="polygon">A Polygon entity</param>
        /// <returns>A boolean value reflecting whether there is an intersection</returns>
        private Boolean Intersects(Polygon polygon)
        {
            if(polygon.Contains(CenterPoint)) { return true; } //return troe if the Polygon contains the collider center point
            else
            {
                foreach(Trace trace in polygon) //Else check for each Trace
                {
                    if (trace.GetClosestTraceFrom(CenterPoint, 1000000).Length < Radius) //Whether its closest point is within the radius
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Calculates the impingement of this RadialCollider on a Polygon
        /// </summary>
        /// <param name="polygon">The colliding Polygon</param>
        /// <returns>A Trace representing the RadialCollider's impingement on the Polygon</returns>
        private Trace ImpingementOn(Polygon polygon)
        {
            Trace closestTrace = null;
            foreach(Trace trace in polygon) //Each Trace is checked to find the closest Polygon edge to the RadialCollider center
            {
                Trace measure = trace.GetClosestTraceFrom(CenterPoint, 1000000);
                if(closestTrace is null || measure.Length < closestTrace.Length) { closestTrace = measure; }
            }
            //The Trace is then calculated in reference to the RadialColldier center based on the closest Polygon edge
            if(polygon.Contains(CenterPoint))
            {
                return new Trace(closestTrace.Terminus, closestTrace.Flip().Bearing, closestTrace.Length + Radius);
            }
            else
            {
                return new Trace(closestTrace.Terminus, closestTrace.Bearing, Radius - closestTrace.Length);
            }
        }
        /// <summary>
        /// Move out of intersection with remote collider. If both objects can move, then each does half the movement.
        /// </summary>
        /// <param name="selfAttach">The DynamicEntity that this RadialColldier is attached to</param>
        /// <param name="remoteAttach">The colliding entity</param>
        public void ResolveCollision(DynamicEntity selfAttach, WorldEntity remoteAttach)
        {
            Trace impingementOnCollider = selfAttach.Collider.GetImpingementOn(remoteAttach.Collider); //Calculate impingement on the remote collider
            Vector2 bufferVector = new Vector2();
            if (DynamicEntity.ColliderPushbackBuffer > 0) { bufferVector = new Trace(new Vector2(), impingementOnCollider.Flip().Bearing, DynamicEntity.ColliderPushbackBuffer).AsAlignedVector; } //Account for the pushback extra buffer
            Vector2 baseUnintersectVector = impingementOnCollider.Flip().AsAlignedVector; //Flip impingement to get vector for undoing the intersection
            selfAttach.Move((baseUnintersectVector / (remoteAttach is DynamicEntity ? 2 : 1)) + bufferVector); //If the remote entity is dynamic, move half the distance. If not, move the full distance

            Vector2 targetPerpendicularVelocity = Trace.GetPerpendicularComponent(selfAttach.Velocity, impingementOnCollider.AsAlignedVector); //Calculate the target velocity perpendicular to the collision
            Vector2 velocityDelta = targetPerpendicularVelocity - selfAttach.Velocity; //Calculate the correct velocity delta

            Vector2 reboundDelta = new Vector2();
            if (remoteAttach is DynamicEntity) //For Dynamic remote entities
            {
                /*
                 * Using standard momentum and kinetic energy conservation equations:
                 * Elastic bounce occurs using components of velocity aligned with collision.
                 */
                DynamicEntity remote = (DynamicEntity)remoteAttach;

                Vector2 remoteBufferVector = new Vector2();
                if (DynamicEntity.ColliderPushbackBuffer > 0) { remoteBufferVector = new Trace(new Vector2(), impingementOnCollider.Bearing, DynamicEntity.ColliderPushbackBuffer).AsAlignedVector; } //Account for pushback extra buffer
                Vector2 remoteUnintersectVector = impingementOnCollider.AsAlignedVector; //Calculate unintersect vector
                remote.Move((remoteUnintersectVector / 2) + remoteBufferVector); //Apply the other half of the un-intersection motion
                Vector2 remoteTargetPerpendicularVelocity = Trace.GetPerpendicularComponent(remote.Velocity, impingementOnCollider.AsAlignedVector); //Calculate remote object target velocity
                Vector2 remoteVelocityDelta = remoteTargetPerpendicularVelocity - remote.Velocity; //Calculate delta to target velocity

                Trace thisVelocityTrace = new Trace(new Vector2(), selfAttach.Velocity); //Get velocity of local entity as Trace
                Trace remoteVelocityTrace = new Trace(new Vector2(), remote.Velocity); //Get velocity of remote entity as Trace
                Trace thisAlignedVelocityTrace = thisVelocityTrace.GetAlignedComponentTo(impingementOnCollider); //Get aligned velocity to impingement locally
                Trace remoteAlignedVelocityTrace = remoteVelocityTrace.GetAlignedComponentTo(impingementOnCollider); //Get aligned velocity to impingement for the remote entity

                //Velocity bearing is checked to see whether or not the entity is moving away from or towards the collision point, and the collision scalar values are set based on this
                Double thisAngleDifference = GraphicsTools.AngleDifference(thisAlignedVelocityTrace.Bearing, impingementOnCollider.Bearing);
                Double thisScalarVelocity = thisAlignedVelocityTrace.Length * (Math.Abs(thisAngleDifference) <= Math.PI / 2 ? 1 : -1);
                Double remoteAngleDifference = GraphicsTools.AngleDifference(remoteAlignedVelocityTrace.Bearing, impingementOnCollider.Bearing);
                Double remoteScalarVelocity = remoteAlignedVelocityTrace.Length * (Math.Abs(remoteAngleDifference) <= Math.PI / 2 ? 1 : -1);

                //The local target speed is calculated from momentum equations
                Double thisTargetSpeedAfterCollision = (((selfAttach.Mass - remote.Mass) / (selfAttach.Mass + remote.Mass)) * thisScalarVelocity) + (((remote.Mass * 2) / (selfAttach.Mass + remote.Mass)) * remoteScalarVelocity);
                Trace targetVelocityTrace = new Trace(new Vector2(), impingementOnCollider.Bearing, thisTargetSpeedAfterCollision); //The target speed is converted into a total target Trace
                reboundDelta = targetVelocityTrace.AsAlignedVector; //And converted to a vector

                //The remote target speed is calculated from momentum equations
                Double remoteTargetSpeedAfterCollision = (((selfAttach.Mass * 2) / (selfAttach.Mass + remote.Mass)) * thisScalarVelocity) + (((remote.Mass - selfAttach.Mass) / (selfAttach.Mass + remote.Mass)) * remoteScalarVelocity);
                Trace remoteTargetVelocityTrace = new Trace(new Vector2(), impingementOnCollider.Bearing, remoteTargetSpeedAfterCollision); //Convert to Trace
                Vector2 remoteReboundDelta = remoteTargetVelocityTrace.AsAlignedVector; //Then to vector

                remote.ShuntVelocity(remoteVelocityDelta); //The remote velocity delta is applied as a shunt
                if (Math.Abs(GraphicsTools.AngleDifference(new Trace(remoteReboundDelta).Bearing, impingementOnCollider.Flip().Bearing)) > Math.PI / 2) //The rebound delta is applied via comparison of the rebound and impingement bearing
                {
                    remote.ShuntVelocity(remoteReboundDelta);
                }
                else
                {
                    remote.ShuntVelocity(-remoteReboundDelta);
                }

                remote.KillImpulse(); //Any additional impulse on the remote object is removed

                if (Shell.ActiveSounds.Count < 16 && DynamicEntity.EnableCollisionChimes) //Collision chimes, when enabled, plays audio corresponding to collision intensity
                {
                    Double ke = (Math.Pow(remoteScalarVelocity, 2) * remote.Mass) + (Math.Pow(thisScalarVelocity, 2) * selfAttach.Mass);
                    Shell.PlaySoundInstant("GLOCKEN", false, (float)(1 - ((4 * Math.Atan(ke / 10) / Math.PI))));
                }
            }
            else //For a non-Dynamic remote entity
            {
                /*
                 * Simply flip velocity component aligned with collision.
                 */
                Vector2 totalAlignedVelocity = Trace.GetAlignedComponent(selfAttach.Velocity, impingementOnCollider.AsAlignedVector);
                Trace alignedVelocityAsTrace = new Trace(totalAlignedVelocity);
                reboundDelta = alignedVelocityAsTrace.Flip().AsAlignedVector;

                selfAttach.KillImpulse(); //Kill any additional impulse on the local object

                if (Shell.ActiveSounds.Count < 16 && DynamicEntity.EnableCollisionChimes) //Collision chimes, when enabled, plays audio corresponding to collision intensity
                {
                    Double ke = (Math.Pow(totalAlignedVelocity.Length(), 2) * selfAttach.Mass);
                    Shell.PlaySoundInstant("GLOCKEN", false, (float)(1 - ((4 * Math.Atan(ke / 10) / Math.PI))));
                }
            }
            selfAttach.ShuntVelocity(velocityDelta); //Apply collision velocity for the local entity
            //Flip acceleration if it is somehow already coming out the other side of the collider.
            if(Math.Abs(GraphicsTools.AngleDifference(new Trace(reboundDelta).Bearing, impingementOnCollider.Bearing)) > Math.PI / 2)
            {
                selfAttach.ShuntVelocity(reboundDelta);
            }
            else
            {
                selfAttach.ShuntVelocity(-reboundDelta);
            }

            selfAttach.KillImpulse(); //Kill any additional impulse on the local object
        }
    }
    /// <summary>
    /// Dynamic extension of WorldEntity that holds physics model variables.
    /// </summary>
    [Serializable]
    public class DynamicEntity : WorldEntity
    {
        /// <summary>
        /// Defines whether this object should play collision sounds
        /// </summary>
        public static Boolean EnableCollisionChimes { get; set; }
        private static float s_globalGravity = 0f;
        /// <summary>
        /// Property reflecting the global dynamic gravity value to be applied
        /// </summary>
        public static float GlobalGravity
        {
            get { return s_globalGravity; }
            set
            {
                s_globalGravity = value;
                Shell.WriteLine("Global physics gravity was set to " + s_globalGravity.ToString() + ".");
            }
        }
        public static readonly float ColliderPushbackBuffer = 0.0f; //Optional physics collision buffer
        private float _angularAcceleration = 0f;
        /// <summary>
        /// Float property representing the current angular acceleration of the DynamicEntity
        /// </summary>
        public float AngularAcceleration
        {
            get
            {
                return _angularAcceleration;
            }
            protected set
            {
                _angularAcceleration = value;
            }
        }
        private float _angularVelocity = 0f;
        /// <summary>
        /// Float property representing the current angular velocity of the DynamicEntity
        /// </summary>
        public float AngularVelocity
        {
            get
            {
                return _angularVelocity;
            }
            protected set
            {
                _angularVelocity = value;
            }
        }
        private Vector2 _velocity = new Vector2();
        /// <summary>
        /// Vector2 property representing the current vector velocity of the DynamicEntity
        /// </summary>
        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
            protected set
            {
                _velocity = value;
            }
        }
        private Vector2 _acceleration = new Vector2();
        /// <summary>
        /// Vector2 property representing the current acceleration vector of the DynamicEntity
        /// </summary>
        public Vector2 Acceleration
        {
            get
            {
                return _acceleration;
            }
            protected set
            {
                _acceleration = value;
            }
        }
        /// <summary>
        /// Method to instantly halt motion of the DynamicEntity
        /// </summary>
        public void Halt()
        {
            Halt(true);
        }
        /// <summary>
        /// Method to instantly halt motion of the DynamicEntity
        /// </summary>
        /// <param name="haltAngularMovement">Boolean flag for whether rotary motion should be halted</param>
        public void Halt(Boolean haltAngularMovement)
        {
            _acceleration.X = 0;
            _acceleration.Y = 0;
            _velocity.X = 0;
            _velocity.Y = 0;
            if (haltAngularMovement)
            {
                _angularAcceleration = 0f;
                _angularVelocity = 0f;
            }
        }
        /// <summary>
        /// Double property corresponding to simulated mass
        /// </summary>
        public Double Mass
        {
            get;
            protected set;
        }
        /// <summary>
        /// Vector2 property corresponding to the coordinate mass center
        /// </summary>
        public Vector2 CenterOfMass
        {
            get;
            protected set;
        }
        /// <summary>
        /// DynamicEntity constructor
        /// </summary>
        /// <param name="name">WorldEntity name</param>
        /// <param name="location">Initial location as a vector</param>
        /// <param name="atlas">Draw texture atlas</param>
        /// <param name="depth">Depth within 2D scene</param>
        /// <param name="mass">Simulated object mass</param>
        public DynamicEntity(String name, Vector2 location, TAtlasInfo? atlas, float depth, Double mass) : base(name, location, atlas, depth)
        {
            Mass = mass;
            CenterOfMass = Origin;
            //Set parameters for tracking collisions
            AlreadyCollidedWithThisIteration = new List<WorldEntity>();
            PreviousFrameCollides = new List<WorldEntity>();
            ImpulseKilledByActiveCollision = false;
        }
        public override void Update()
        {
            base.Update(); //Run the base WorldEntity update
            PreviousFrameCollides.Clear(); //Clear any collisions from the previous frame
            AlreadyCollidedWithThisIteration.Clear(); //Clear any record of previous entity collisions
            ImpulseKilledByActiveCollision = false; //Set impulse killed flag to false
            _velocity += _acceleration; //Update velocity with acceleration
            /*
             * Note:
             * If under constant acceleration (or more accurately, if accelerating into a collision), then hitting a surface will cause an energy decrease;
             * I believe due to the acceleration not being applied for the distance it travels on the frame it makes the collision.
             * If the Move() function is changed to occur before the acceleration application, then the effect is reversed and energy will be gained.
             * Not sure how to easily correct this without performing custom inference calculatons - regardless the effect is fairly slight for any individual collision
             * UPDATE: Seems to be mostly solved now as a byproduct of the KillImpulse mechanism. Apply gravity to acceleration on accel reset so it can be killed by a collision and not applied on the rebound frame.
             */
            Move(_velocity); //Move by velocity
            _angularVelocity += _angularAcceleration; //Update angular velocity with angular acceleration
            Rotate(_angularVelocity); //Rotate by angular velocity

            //Set acceleration to zero, plus global gravity
            _acceleration.X = 0;
            _acceleration.Y = 0 + GlobalGravity;
            _angularAcceleration = 0f; //Set angular acceleration to zero
            Vector2 oldPos = Position; //Record "old" position
            Vector2 oldAccel = Acceleration; //Record "old" accelertation
        }
        private List<WorldEntity> AlreadyCollidedWithThisIteration { get; set; } //List of entities for which collisions have already been processed this iteration
        public List<WorldEntity> PreviousFrameCollides { get; private set; } //List of entities which were collided with in the previous frame
        public Boolean ImpulseKilledByActiveCollision { get; protected set; } //Flag for if an active collision has already killed object impulse
        /* Note:
         * appears to be some sort of error with overlapping polygon colliders that causes an entity to be shunted out the other side of the polygon when interacting with a joint created by the clipping. Investigate/fix later.
         * UPDATE: May have actually been due to surface area resizing bug? Check to see if it reoccurs.
         * UPDATE 2: Spotted again in relation to strange lag - potentially due to excessive collision checks on spawn-in?
         * FIXED: Drag calculation was wrong for low mass objects.
         */
        /// <summary>
        /// Function to check and resolve DynamicObject collisions
        /// </summary>
        public void CheckAndResolveCollisions()
        {
            if(Collider is null) { return; } //Return if there is no active collider
            SortedList<float, List<WorldEntity>> sortedCollides = new System.Collections.Generic.SortedList<float, List<WorldEntity>>(); //Create sorted list for collision objects in order of distance
            foreach (WorldEntity worldEntity in Shell.UpdateQueue) //For each world entity
            {
                if (worldEntity == this || (worldEntity is DynamicEntity && ((DynamicEntity)worldEntity).AlreadyCollidedWithThisIteration.Contains(this))) { continue; } //Ignore this object and any collisions that have already been processed
                if (worldEntity.Collider != null && worldEntity.TraceTo(Position).Length < (worldEntity.Collider.GetMaximumExtent() + Collider.GetMaximumExtent())) //Only check for collisions with valid colliders taht are in range
                {
                    if (Collider.Collides(worldEntity.Collider)) //Check for a collision
                    {
                        float trueImpingement = Collider.GetImpingementOn(worldEntity.Collider).AsAlignedVector.Length(); //Get collision impingement
                        if (sortedCollides.ContainsKey(trueImpingement)) { sortedCollides[trueImpingement].Add(worldEntity); } //If there is already a collision with that exact distance add to the existing list
                        else
                        {
                            sortedCollides.Add(trueImpingement, new List<WorldEntity>(new WorldEntity[] { worldEntity })); //Otherwise record the new collision
                        }
                    }
                }
            }
            for(int i = sortedCollides.Count - 1; i >= 0; i--) //In order of distance to the collision
            {
                foreach (WorldEntity colliderEntity in sortedCollides.Values[i])
                {
                    if (Collider.Collides(colliderEntity.Collider)) //Check that the collision still applies
                    {
                        Collider.ResolveCollision(this, colliderEntity); //Resolve the collision
                        AlreadyCollidedWithThisIteration.Add(colliderEntity); //Record that the collision has already been resolved
                        PreviousFrameCollides.Add(colliderEntity); //Add to the previous frame collision list
                        if(colliderEntity is DynamicEntity)
                        {
                            ((DynamicEntity)colliderEntity).PreviousFrameCollides.Add(this); //If the remote entity is Dynamic, record that it has already collided with this object
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Method to apply a force in Newtons, specified as Vector2
        /// </summary>
        /// <param name="newtons">Vector force to apply</param>
        public void ApplyForce(Vector2 newtons)
        {
            Acceleration = Acceleration + new Vector2((float)(newtons.X / Mass), (float)(newtons.Y / Mass));
        }
        /// <summary>
        /// Method to accelerate this object
        /// </summary>
        /// <param name="acceleration">Acceleration to apply as a Vector2</param>
        public void Accelerate(Vector2 acceleration)
        {
            Acceleration += acceleration;
        }
        /// <summary>
        /// Method to instantly apply velocity
        /// </summary>
        /// <param name="acceleration">Acceleration to shunt as velocity as a Vector2</param>
        public void ShuntVelocity(Vector2 acceleration)
        {
            Velocity += acceleration;
        }
        /// <summary>
        /// Method to immediately kill acceleration impulse on this object
        /// </summary>
        public void KillImpulse()
        {
            _acceleration.X = 0f;
            _acceleration.Y = 0f;
            ImpulseKilledByActiveCollision = true;
        }
    }
}
