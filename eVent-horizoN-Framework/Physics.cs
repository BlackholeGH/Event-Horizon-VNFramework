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
    }
    /// <summary>
    /// Dynamic extension of WorldEntity that holds physics model variables.
    /// </summary>
    [Serializable]
    public class DynamicEntity : WorldEntity
    {
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
        }
        public override void Update()
        {
            _velocity += _acceleration;
            Move(_velocity);
            _angularVelocity += _angularAcceleration;
            Rotate(_angularVelocity);
            _acceleration.X = 0;
            _acceleration.Y = 0;
            _angularAcceleration = 0f;
            CheckCollisions();
            base.Update();
        }
        private void CheckCollisions()
        {
            if(Collider is null) { return; }
            //Console.WriteLine("StartCollide");
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if(worldEntity == this) { continue; }
                if(worldEntity.Collider != null && worldEntity.TraceTo(Position).Length < (worldEntity.Collider.GetMaximumExtent() + Collider.GetMaximumExtent()))
                {
                    if(Collider.Collides(worldEntity.Collider))
                    {
                        //Console.WriteLine("Collided with " + worldEntity.Name);
                        if (worldEntity is DynamicEntity)
                        {
                            Trace impingementOnCollider = Collider.GetImpingementOn(worldEntity.Collider);
                            Move((impingementOnCollider.Flip().AsAlignedVector / 2) * 1.0001f);
                            Vector2 targetPerpendicularVelocity = Trace.GetPerpendicularComponent(Velocity, impingementOnCollider.AsAlignedVector);
                            Vector2 velocityDelta = targetPerpendicularVelocity - Velocity;
                            Acceleration += velocityDelta;

                            DynamicEntity remote = (DynamicEntity)worldEntity;
                            Vector2 totalAlignedVelocity = Trace.GetAlignedComponent(Velocity, impingementOnCollider.AsAlignedVector) - Trace.GetAlignedComponent(remote.Velocity, impingementOnCollider.AsAlignedVector);
                            Double proportionOfThis = Mass / (Mass + remote.Mass);
                            Trace alignedVelocityAsTrace = new Trace(totalAlignedVelocity);
                            Vector2 flipAligned = alignedVelocityAsTrace.Flip().AsAlignedVector;
                            Acceleration += new Vector2((float)(flipAligned.X * (1 - proportionOfThis)), (float)(flipAligned.Y * (1 - proportionOfThis)));
                        }
                        else
                        {
                            Trace impingementOnCollider = Collider.GetImpingementOn(worldEntity.Collider);
                            Move(impingementOnCollider.Flip().AsAlignedVector * 1.0001f);
                            Vector2 targetPerpendicularVelocity = Trace.GetPerpendicularComponent(Velocity, impingementOnCollider.AsAlignedVector);
                            Vector2 velocityDelta = targetPerpendicularVelocity - Velocity;
                            Acceleration += velocityDelta;

                            Vector2 totalAlignedVelocity = Trace.GetAlignedComponent(Velocity, impingementOnCollider.AsAlignedVector);
                            Trace alignedVelocityAsTrace = new Trace(totalAlignedVelocity);
                            Vector2 flipAligned = alignedVelocityAsTrace.Flip().AsAlignedVector;
                            Acceleration += flipAligned;
                        }
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
    }
}
