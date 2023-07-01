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
    public static partial class Behaviours
    {
        public class DragPhysicsBehaviour : IVNFBehaviour
        {
            public static readonly Double MassFluidDensity = 0.000002d;
            public DragPhysicsBehaviour()
            {

            }
            public void UpdateFunctionality(WorldEntity worldEntity)
            {
                if(worldEntity is DynamicEntity)
                {
                    DynamicEntity dynamicEntity = (DynamicEntity)worldEntity;
                    if(dynamicEntity.ImpulseKilledByActiveCollision) { return; }
                    Double dragMagnitude = (Math.Pow(dynamicEntity.Velocity.Length(), 2) * Math.Pow(dynamicEntity.Collider.GetMaximumExtent(), 2)) * MassFluidDensity;
                    Double magnitudeLimit = dynamicEntity.Velocity.Length() * dynamicEntity.Mass;
                    if(magnitudeLimit == 0) { magnitudeLimit = 1; }
                    //dragMagnitude = (Math.Atan(dragMagnitude / magnitudeLimit) / (Math.PI / 2)) * dragMagnitude * 0.75;
                    if(dragMagnitude >= magnitudeLimit) { dragMagnitude = magnitudeLimit - 0.001d; }
                    Vector2 dragForce = new Trace(new Vector2(), new Trace(dynamicEntity.Velocity).Flip().Bearing, dragMagnitude).AsAlignedVector;
                    dynamicEntity.ApplyForce(dragForce);
                    /*if (worldEntity.Name == "IMEMBOT_1")
                    {
                        Console.WriteLine(dynamicEntity.Collider.GetMaximumExtent());
                        Console.WriteLine("Applied drag force for " + worldEntity.Name + ": " + dragForce);
                    }*/
                }
            }
            public void Clear() { }
        }
        public class DynamicWASDControlBehaviour : IVNFBehaviour
        {
            public static Double Speed = 8d;
            public DynamicWASDControlBehaviour()
            {

            }
            public void Clear()
            {

            }
            public void UpdateFunctionality(WorldEntity worldEntity)
            {
                if (worldEntity is DynamicEntity)
                {
                    DynamicEntity dynamicEntity = (DynamicEntity)worldEntity;
                    KeyboardState kState = Keyboard.GetState();
                    if (kState.IsKeyDown(Keys.W))
                    {
                        Vector2 targetForwardTrace = new Trace(new Vector2(), dynamicEntity.RotationRads, Speed).AsAlignedVector;
                        //Console.WriteLine("Velocity at application " + dynamicEntity.Velocity);
                        //Console.WriteLine("targetForwardTrace " + targetForwardTrace);
                        Vector2 forwardDelta = targetForwardTrace - dynamicEntity.Velocity;
                        dynamicEntity.Accelerate(forwardDelta);
                        //dynamicEntity.ApplyForce(targetForwardTrace);
                        //Console.WriteLine("Forward delta " + forwardDelta);
                    }
                    if (kState.IsKeyDown(Keys.S))
                    {
                        Vector2 targetBackwardTrace = new Trace(new Vector2(), dynamicEntity.RotationRads, Speed).Flip().AsAlignedVector;
                        Vector2 backwardDelta = targetBackwardTrace - dynamicEntity.Velocity;
                        dynamicEntity.Accelerate(backwardDelta);
                    }
                    if (kState.IsKeyDown(Keys.A))
                    {
                        dynamicEntity.Rotate(-0.1f);
                    }
                    if (kState.IsKeyDown(Keys.D))
                    {
                        dynamicEntity.Rotate(0.1f);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Interface for a Collider objects that can be attached to entities
    /// </summary>
    public interface ICollider
    {
        public GraphicsTools.Trace GetImpingementOn(ICollider collider);
        public Boolean Collides(ICollider collider);
        public Boolean Intersects(Trace trace);
        public Vector2? GetFirstIntersection(Trace trace);
        public double GetMaximumExtent();
        public ICollider Scale(Vector2 origin, Vector2 scale);
        public ICollider Rotate(Vector2 origin, double rotation);
        public ICollider Translate(Vector2 translation);
        public void ResolveCollision(DynamicEntity selfAttach, WorldEntity remoteAttach);
    }
    /// <summary>
    /// A radial collider object with a point center.
    /// </summary>
    public class RadialCollider : ICollider
    {
        public double Radius { get; protected set; }
        public Vector2 CenterPoint { get; protected set; }
        Boolean _translatedCenterPoint = false;
        public RadialCollider(double radius, Vector2 location)
        {
            Radius = radius;
            CenterPoint = location;
        }
        public RadialCollider(double radius, Vector2 location, Boolean translatedCenterPoint)
        {
            Radius = radius;
            CenterPoint = location;
            _translatedCenterPoint = translatedCenterPoint;
        }
        public Double GetMaximumExtent()
        {
            return Radius + (_translatedCenterPoint ? 0 : CenterPoint.Length());
        }
        public ICollider Scale(Vector2 origin, Vector2 scale)
        {
            Vector2 scaledCenter = CenterPoint + ((CenterPoint - origin) * scale);
            double scaledRadius = Radius * ((scale.X + scale.Y) / 2);
            return new RadialCollider(scaledRadius, scaledCenter);
        }
        public ICollider Rotate(Vector2 origin, double rotation)
        {
            return new RadialCollider(Radius, CenterPoint);
        }
        public ICollider Translate(Vector2 translation)
        {
            return new RadialCollider(Radius, CenterPoint + translation, true);
        }
        public Boolean Collides(ICollider collider)
        {
            if(collider is RadialCollider) { return Intersects((RadialCollider)collider); }
            else if(collider is Polygon) { return Intersects((Polygon)collider); }
            return false;
        }
        public Trace GetImpingementOn(ICollider collider)
        {
            if (collider is RadialCollider) { return ImpingementOn((RadialCollider)collider); }
            else if (collider is Polygon) { return ImpingementOn((Polygon)collider); }
            return null;
        }
        private Boolean Intersects(RadialCollider radialCollider)
        {
            return new Trace(CenterPoint, radialCollider.CenterPoint).Length < Radius + radialCollider.Radius;
        }
        private Trace ImpingementOn(RadialCollider radialCollider)
        {
            Trace thisToThat = new Trace(CenterPoint, radialCollider.CenterPoint);
            Trace thatToThis = new Trace(radialCollider.CenterPoint, CenterPoint);
            Trace thisFurthestImpingement = new Trace(CenterPoint, thisToThat.Bearing, Radius);
            Trace thatFurthestImpingement = new Trace(radialCollider.CenterPoint, thatToThis.Bearing, radialCollider.Radius);
            return new Trace(thatFurthestImpingement.Terminus, thisFurthestImpingement.Terminus);
        }
        public Vector2[] GetIntersections(Trace trace)
        {
            Boolean invert = false;
            if(trace.Slope == Double.PositiveInfinity || trace.Slope == Double.NegativeInfinity)
            {
                invert = true;
                trace = new Trace(new Vector2(trace.Origin.Y, trace.Origin.X), new Vector2(trace.Terminus.Y, trace.Terminus.X));
            }
            //(m2+1)x2+2(mc−mq−p)x+(q2−r2+p2−2cq+c2)=0.
            Double a = Math.Pow(trace.Slope, 2) + 1;
            Double b = 2 * ((trace.Slope * trace.YIntercept) - (trace.Slope * CenterPoint.Y) - CenterPoint.X);
            Double c = Math.Pow(CenterPoint.Y, 2) - Math.Pow(Radius, 2) + Math.Pow(CenterPoint.X, 2) - (2 * trace.YIntercept * CenterPoint.Y) + Math.Pow(trace.YIntercept, 2);
            Double x1 = (-b + Math.Sqrt((2 * b) - (4 * a * c))) / (2 * a);
            Double x2 = (-b - Math.Sqrt((2 * b) - (4 * a * c))) / (2 * a);
            Double y1 = (trace.Slope * x1) + trace.YIntercept;
            Double y2 = (trace.Slope * x2) + trace.YIntercept;
            Vector2 first = new Vector2((float)x1, (float)y1);
            Vector2 second = new Vector2((float)x2, (float)y2);
            List<Vector2> validOut = new List<Vector2>();
            if (first.X <= trace.Max.X && first.X >= trace.Min.X && first.Y <= trace.Max.Y && first.Y >= trace.Min.Y)
            {
                validOut.Add(invert ? new Vector2(first.Y, first.X) : first);
            }
            if (second.X <= trace.Max.X && second.X >= trace.Min.X && second.Y <= trace.Max.Y && second.Y >= trace.Min.Y)
            {
                validOut.Add(invert ? new Vector2(second.Y, second.X) : second);
            }
            return validOut.ToArray();
        }
        public Vector2? GetFirstIntersection(Trace trace)
        {
            Vector2[] intersections = GetIntersections(trace);
            if(intersections.Length > 0)
            {
                if(intersections.Length == 1) { return intersections[0]; }
                else
                {
                    return (intersections[0] - trace.Origin).Length() <= (intersections[1] - trace.Origin).Length() ? intersections[0] : intersections[1];
                }
            }
            return null;
        }
        public Boolean Intersects(Trace trace)
        {
            if (trace.GetClosestTraceFrom(CenterPoint, 1000000).Length < Radius)
            {
                return true;
            }
            else { return false; }
        }
        private Boolean Intersects(Polygon polygon)
        {
            if(polygon.Contains(CenterPoint)) { return true; }
            else
            {
                foreach(Trace trace in polygon)
                {
                    if (trace.GetClosestTraceFrom(CenterPoint, 1000000).Length < Radius)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private Trace ImpingementOn(Polygon polygon)
        {
            Trace closestTrace = null;
            foreach(Trace trace in polygon)
            {
                Trace measure = trace.GetClosestTraceFrom(CenterPoint, 1000000);
                if(closestTrace is null || measure.Length < closestTrace.Length) { closestTrace = measure; }
            }
            if(polygon.Contains(CenterPoint))
            {
                return new Trace(closestTrace.Terminus, closestTrace.Flip().Bearing, closestTrace.Length + Radius);
            }
            else
            {
                return new Trace(closestTrace.Terminus, closestTrace.Bearing, Radius - closestTrace.Length);
            }
        }
        public void ResolveCollision(DynamicEntity selfAttach, WorldEntity remoteAttach)
        {
            if(selfAttach.Name == "IMEMBOT_1")
            {
                int testpoint = 0;
            }
            /*
            * Move out of intersection with remote collider. If both objects can move, then each does half the movement.
            */
            Trace impingementOnCollider = selfAttach.Collider.GetImpingementOn(remoteAttach.Collider);
            Vector2 bufferVector = new Vector2();
            if (DynamicEntity.ColliderPushbackBuffer > 0) { bufferVector = new Trace(new Vector2(), impingementOnCollider.Flip().Bearing, DynamicEntity.ColliderPushbackBuffer).AsAlignedVector; }
            Vector2 baseUnintersectVector = impingementOnCollider.Flip().AsAlignedVector;
            selfAttach.Move((baseUnintersectVector / (remoteAttach is DynamicEntity ? 2 : 1)) + bufferVector);

            //selfAttach.Move(baseUnintersectVector + bufferVector);
            Vector2 targetPerpendicularVelocity = Trace.GetPerpendicularComponent(selfAttach.Velocity, impingementOnCollider.AsAlignedVector);
            Vector2 velocityDelta = targetPerpendicularVelocity - selfAttach.Velocity;

            Vector2 reboundDelta = new Vector2();
            if (remoteAttach is DynamicEntity)
            {
                /*
                 * Using standard momentum and kinetic energy conservation equations:
                 * Elastic bounce occurs using components of velocity aligned with collision.
                 */
                DynamicEntity remote = (DynamicEntity)remoteAttach;

                Vector2 remoteBufferVector = new Vector2();
                if (DynamicEntity.ColliderPushbackBuffer > 0) { remoteBufferVector = new Trace(new Vector2(), impingementOnCollider.Bearing, DynamicEntity.ColliderPushbackBuffer).AsAlignedVector; }
                Vector2 remoteUnintersectVector = impingementOnCollider.AsAlignedVector;
                remote.Move((remoteUnintersectVector / 2) + remoteBufferVector);
                Vector2 remoteTargetPerpendicularVelocity = Trace.GetPerpendicularComponent(remote.Velocity, impingementOnCollider.AsAlignedVector);
                Vector2 remoteVelocityDelta = remoteTargetPerpendicularVelocity - remote.Velocity;

                /*Console.WriteLine();
                Console.WriteLine("New dynamic collision! At " + Shell.DefaultShell.LastUpdateGameTime.TotalGameTime);
                Console.WriteLine("Collision start!");
                Console.WriteLine("Local entity " + selfAttach.Name + " was unintersected by X: " + ((baseUnintersectVector / 2) + bufferVector).X + ", Y: " + ((baseUnintersectVector / 2) + bufferVector).Y);*/

                Trace thisVelocityTrace = new Trace(new Vector2(), selfAttach.Velocity);
                Trace remoteVelocityTrace = new Trace(new Vector2(), remote.Velocity);
                Trace thisAlignedVelocityTrace = thisVelocityTrace.GetAlignedComponentTo(impingementOnCollider);
                Trace remoteAlignedVelocityTrace = remoteVelocityTrace.GetAlignedComponentTo(impingementOnCollider);

                Double thisAngleDifference = GraphicsTools.AngleDifference(thisAlignedVelocityTrace.Bearing, impingementOnCollider.Bearing);
                Double thisScalarVelocity = thisAlignedVelocityTrace.Length * (Math.Abs(thisAngleDifference) <= Math.PI / 2 ? 1 : -1);
                Double remoteAngleDifference = GraphicsTools.AngleDifference(remoteAlignedVelocityTrace.Bearing, impingementOnCollider.Bearing);
                Double remoteScalarVelocity = remoteAlignedVelocityTrace.Length * (Math.Abs(remoteAngleDifference) <= Math.PI / 2 ? 1 : -1);

                /*Console.WriteLine("Local entity " + selfAttach.Name + " ke component: " + (Math.Pow(thisScalarVelocity, 2) * selfAttach.Mass));
                Console.WriteLine("Local entity " + selfAttach.Name + " velocity component: " + thisScalarVelocity);
                Console.WriteLine("Remote entity " + remoteAttach.Name + " was unintersected by X: " + ((remoteUnintersectVector / 2) + remoteBufferVector).X + ", Y: " + ((remoteUnintersectVector / 2) + remoteBufferVector).Y);
                Console.WriteLine("Remote entity  " + remoteAttach.Name + " ke component: " + (Math.Pow(remoteScalarVelocity, 2) * remote.Mass));
                Console.WriteLine("Remote entity  " + remoteAttach.Name + " velocity component: " + remoteScalarVelocity);
                Console.WriteLine();
                Console.WriteLine("Remote entity " + remote.Name + " total ke: " + (Math.Pow(remote.Velocity.Length(), 2) * remote.Mass));
                Console.WriteLine("Local entity " + selfAttach.Name + " total ke: " + (Math.Pow(selfAttach.Velocity.Length(), 2) * selfAttach.Mass));*/

                Double thisTargetSpeedAfterCollision = (((selfAttach.Mass - remote.Mass) / (selfAttach.Mass + remote.Mass)) * thisScalarVelocity) + (((remote.Mass * 2) / (selfAttach.Mass + remote.Mass)) * remoteScalarVelocity);
                Trace targetVelocityTrace = new Trace(new Vector2(), impingementOnCollider.Bearing, thisTargetSpeedAfterCollision);
                reboundDelta = targetVelocityTrace.AsAlignedVector;

                Double remoteTargetSpeedAfterCollision = (((selfAttach.Mass * 2) / (selfAttach.Mass + remote.Mass)) * thisScalarVelocity) + (((remote.Mass - selfAttach.Mass) / (selfAttach.Mass + remote.Mass)) * remoteScalarVelocity);
                Trace remoteTargetVelocityTrace = new Trace(new Vector2(), impingementOnCollider.Bearing, remoteTargetSpeedAfterCollision);
                Vector2 remoteReboundDelta = remoteTargetVelocityTrace.AsAlignedVector;

                remote.ShuntVelocity(remoteVelocityDelta);
                if (Math.Abs(GraphicsTools.AngleDifference(new Trace(remoteReboundDelta).Bearing, impingementOnCollider.Flip().Bearing)) > Math.PI / 2)
                {
                    remote.ShuntVelocity(remoteReboundDelta);
                }
                else
                {
                    remote.ShuntVelocity(-remoteReboundDelta);
                }

                remote.KillImpulse();

                if (Shell.ActiveSounds.Count < 16 && DynamicEntity.EnableCollisionChimes)
                {
                    Double ke = (Math.Pow(remoteScalarVelocity, 2) * remote.Mass) + (Math.Pow(thisScalarVelocity, 2) * selfAttach.Mass);
                    Shell.PlaySoundInstant("GLOCKEN", false, (float)(1 - ((4 * Math.Atan(ke / 10) / Math.PI))));
                }

                /*Console.WriteLine("Collision End!");
                Console.WriteLine("Local entity " + selfAttach.Name + " ke component: " + (Math.Pow(reboundDelta.Length(), 2) * selfAttach.Mass));
                Console.WriteLine("Local entity " + selfAttach.Name + " velocity component: " + thisTargetSpeedAfterCollision);
                Console.WriteLine("Remote entity  " + remoteAttach.Name + " ke component: " + (Math.Pow(remoteReboundDelta.Length(), 2) * remote.Mass));
                Console.WriteLine("Remote entity  " + remoteAttach.Name + " velocity component: " + remoteTargetSpeedAfterCollision);
                Console.WriteLine();
                Console.WriteLine("Remote entity " + remote.Name + " total ke: " + (Math.Pow(remote.Velocity.Length(), 2) * selfAttach.Mass));*/
            }
            else
            {
                //Console.WriteLine();
                //Console.WriteLine("Wall collision for " + selfAttach.Name + "! At " + Shell.DefaultShell.LastUpdateGameTime.TotalGameTime);
                /*
                 * Simply flip velocity component aligned with collision.
                 */
                Vector2 totalAlignedVelocity = Trace.GetAlignedComponent(selfAttach.Velocity, impingementOnCollider.AsAlignedVector);
                Trace alignedVelocityAsTrace = new Trace(totalAlignedVelocity);
                reboundDelta = alignedVelocityAsTrace.Flip().AsAlignedVector;

                /* Console.WriteLine("Detected impingement: " + impingementOnCollider.AsAlignedVector);
                Console.WriteLine("Rebound delta: " + reboundDelta); */

                selfAttach.KillImpulse();

                if (Shell.ActiveSounds.Count < 16 && DynamicEntity.EnableCollisionChimes)
                {
                    Double ke = (Math.Pow(totalAlignedVelocity.Length(), 2) * selfAttach.Mass);
                    Shell.PlaySoundInstant("GLOCKEN", false, (float)(1 - ((4 * Math.Atan(ke / 10) / Math.PI))));
                }
            }
            selfAttach.ShuntVelocity(velocityDelta);
            //Flip acceleration if it is somehow already coming out the other side of the collider.
            if(Math.Abs(GraphicsTools.AngleDifference(new Trace(reboundDelta).Bearing, impingementOnCollider.Bearing)) > Math.PI / 2)
            {
                selfAttach.ShuntVelocity(reboundDelta);
            }
            else
            {
                selfAttach.ShuntVelocity(-reboundDelta);
            }

            selfAttach.KillImpulse();

            //Console.WriteLine("Local entity " + selfAttach.Name + " total ke: " + (Math.Pow(selfAttach.Velocity.Length(), 2) * selfAttach.Mass));
        }
    }
    /// <summary>
    /// Dynamic extension of WorldEntity that holds physics model variables.
    /// </summary>
    [Serializable]
    public class DynamicEntity : WorldEntity
    {
        public static Boolean EnableCollisionChimes { get; set; }
        private static float s_globalGravity = 0f;
        public static float GlobalGravity
        {
            get { return s_globalGravity; }
            set
            {
                s_globalGravity = value;
                Shell.WriteLine("Global physics gravity was set to " + s_globalGravity.ToString() + ".");
            }
        }
        public static readonly float ColliderPushbackBuffer = 0.0f;
        private float _angularAcceleration = 0f;
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
        public Double Mass
        {
            get;
            protected set;
        }
        public Vector2 CenterOfMass
        {
            get;
            protected set;
        }
        public DynamicEntity(String name, Vector2 location, TAtlasInfo? atlas, float depth, Double mass) : base(name, location, atlas, depth)
        {
            Mass = mass;
            CenterOfMass = Origin;
            AlreadyCollidedWithThisIteration = new List<WorldEntity>();
            ImpulseKilledByActiveCollision = false;
        }
        public override void Update()
        {
            //base.Update();
            base.Update();
            AlreadyCollidedWithThisIteration.Clear();
            ImpulseKilledByActiveCollision = false;
            _velocity += _acceleration;
            /*
             * If under constant acceleration (or more accurately, if accelerating into a collision), then hitting a surface will cause an energy decrease;
             * I believe due to the acceleration not being applied for the distance it travels on the frame it makes the collision.
             * If the Move() function is changed to occur before the acceleration application, then the effect is reversed and energy will be gained.
             * Not sure how to easily correct this without performing custom inference calculatons - regardless the effect is fairly slight for any individual collision
             * UPDATE: Seems to be mostly solved now as a byproduct of the KillImpulse mechanism. Apply gravity to acceleration on accel reset so it can be killed by a collision and not applied on the rebound frame.
             */
            Move(_velocity);
            _angularVelocity += _angularAcceleration;
            Rotate(_angularVelocity);
            /*if (Name == "IMEMBOT_0_0")
            {
                Console.WriteLine();
                Console.WriteLine("Update block for " + Name + ":");
                Console.WriteLine("Acceleration applied was X: " + _acceleration.X + ", Y: " + _acceleration.Y);
                Console.WriteLine("Velocity (dPos) was X: " + _velocity.X + ", Y: " + _velocity.Y);
                Console.WriteLine("Kinetic energy factor was: " + Math.Pow(Velocity.Length(), 2) * Mass);
            }*/
            _acceleration.X = 0;
            _acceleration.Y = 0 + GlobalGravity;
            _angularAcceleration = 0f;
            Vector2 oldPos = Position;
            Vector2 oldAccel = Acceleration;
            /*if((_velocity + _acceleration).Length() > _velocity.Length())
            {
                Position = oldPos;
                Acceleration = oldAccel;
                Shell.DefaultShell.PauseUpdates = true;
                CheckCollisions();
            }*/
        }
        public List<WorldEntity> AlreadyCollidedWithThisIteration { get; protected set; }
        public Boolean ImpulseKilledByActiveCollision { get; protected set; }
        /* appears to be some sort of error with overlapping polygon colliders that causes an entity to be shunted out the other side of the polygon when interacting with a joint created by the clipping. Investigate/fix later.
         * UPDATE: May have actually been due to surface area resizing bug? Check to see if it reoccurs.
         * UPDATE 2: Spotted again in relation to strange lag - potentially due to excessive collision checks on spawn-in?
         * FIXED: Drag calculation was wrong for low mass objects.
         */
        public void CheckAndResolveCollisions()
        {
            if(Collider is null) { return; }
            //Console.WriteLine("StartCollide");
            SortedList<float, List<WorldEntity>> sortedCollides = new System.Collections.Generic.SortedList<float, List<WorldEntity>>();
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity == this || (worldEntity is DynamicEntity && ((DynamicEntity)worldEntity).AlreadyCollidedWithThisIteration.Contains(this))) { continue; }
                if (worldEntity.Collider != null && worldEntity.TraceTo(Position).Length < (worldEntity.Collider.GetMaximumExtent() + Collider.GetMaximumExtent()))
                {
                    if (Collider.Collides(worldEntity.Collider))
                    {
                        float trueImpingement = Collider.GetImpingementOn(worldEntity.Collider).AsAlignedVector.Length();
                        if (sortedCollides.ContainsKey(trueImpingement)) { sortedCollides[trueImpingement].Add(worldEntity); }
                        else
                        {
                            sortedCollides.Add(trueImpingement, new List<WorldEntity>(new WorldEntity[] { worldEntity }));
                        }
                    }
                }
            }
            for(int i = sortedCollides.Count - 1; i >= 0; i--)
            {
                foreach (WorldEntity colliderEntity in sortedCollides.Values[i])
                {
                    if (Collider.Collides(colliderEntity.Collider))
                    {
                        //Console.WriteLine("Collided with " + worldEntity.Name);
                        Collider.ResolveCollision(this, colliderEntity);
                        AlreadyCollidedWithThisIteration.Add(colliderEntity);
                    }
                }
            }
        }
        public void ApplyForce(Vector2 newtons)
        {
            Acceleration = Acceleration + new Vector2((float)(newtons.X / Mass), (float)(newtons.Y / Mass));
        }
        public void Accelerate(Vector2 acceleration)
        {
            Acceleration += acceleration;
        }
        public void ShuntVelocity(Vector2 acceleration)
        {
            Velocity += acceleration;
        }
        public void KillImpulse()
        {
            _acceleration.X = 0f;
            _acceleration.Y = 0f;
            ImpulseKilledByActiveCollision = true;
            /*Console.WriteLine("Impulse killed " + Acceleration.X + " " + Acceleration.Y);
            Console.WriteLine("Current velocity " + Velocity.X + " " + Velocity.Y);*/
        }
    }
}
