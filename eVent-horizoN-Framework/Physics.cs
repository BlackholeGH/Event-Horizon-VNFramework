﻿using System;
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
    /// <summary>
    /// Interface for a Collider objects that can be attached to entities
    /// </summary>
    public interface ICollider
    {
        public GraphicsTools.Trace GetImpingementOn(ICollider collider);
        public Boolean Collides(ICollider collider);
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
        public RadialCollider(double radius, Vector2 location)
        {
            Radius = radius;
            CenterPoint = location;
        }
        public Double GetMaximumExtent()
        {
            return Radius + CenterPoint.Length();
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
            return new RadialCollider(Radius, CenterPoint + translation);
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
        private Boolean Intersects(Polygon polygon)
        {
            if(polygon.Contains(CenterPoint)) { return true; }
            else
            {
                foreach(Trace trace in polygon)
                {
                    if (trace.GetClosestTraceFrom(CenterPoint, 1000000).Length < Radius)
                    {
                        Trace test = trace.GetClosestTraceFrom(CenterPoint, 1000000);
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
                Console.WriteLine("Remote entity " + remote.Name + " total ke: " + (Math.Pow(remote.Velocity.Length(), 2) * selfAttach.Mass));
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
                /*Console.WriteLine();
                Console.WriteLine("Wall collision for " + selfAttach.Name + "! At " + Shell.DefaultShell.LastUpdateGameTime.TotalGameTime);*/
                /*
                 * Simply flip velocity component aligned with collision.
                 */
                Vector2 totalAlignedVelocity = Trace.GetAlignedComponent(selfAttach.Velocity, impingementOnCollider.AsAlignedVector);
                Trace alignedVelocityAsTrace = new Trace(totalAlignedVelocity);
                reboundDelta = alignedVelocityAsTrace.Flip().AsAlignedVector;
            }
            //selfAttach.Accelerate(reboundDelta);
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
            //Console.WriteLine("Local entity " + selfAttach.Name + " total ke: " + (Math.Pow(selfAttach.Velocity.Length(), 2) * selfAttach.Mass));
        }
    }
    /// <summary>
    /// Dynamic extension of WorldEntity that holds physics model variables.
    /// </summary>
    [Serializable]
    public class DynamicEntity : WorldEntity
    {
        public static readonly float ColliderPushbackBuffer = 0.01f;
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
        }
        public override void Update()
        {
            AlreadyCollidedWithThisIteration.Clear();
            _velocity += _acceleration;
            Move(_velocity);
            _angularVelocity += _angularAcceleration;
            Rotate(_angularVelocity);
            /*Console.WriteLine();
            Console.WriteLine("Update block for " + Name + ":");
            Console.WriteLine("Acceleration applied was X: " + _acceleration.X + ", Y: " + _acceleration.Y);
            Console.WriteLine("Velocity (dPos) was X: " + _velocity.X + ", Y: " + _velocity.Y);
            Console.WriteLine("Kinetic energy factor was: " + Math.Pow(Velocity.Length(), 2) * Mass);*/
            _acceleration.X = 0;
            _acceleration.Y = 0;
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

            base.Update();
        }
        public List<WorldEntity> AlreadyCollidedWithThisIteration { get; protected set; }
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
    }
}
