// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine 3, based on Box2D.XNA port:
* Copyright (c) 2012 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using AetherPhysics.Collision;
using AetherPhysics.Collision.Shapes;
using AetherPhysics.Common;
using System.Numerics;
#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

namespace AetherPhysics.Dynamics.Contacts
{
    public sealed class ContactPositionConstraint
    {
        public Vector2[] localPoints = new Vector2[Settings.MaxManifoldPoints];
        public Vector2 localNormal;
        public Vector2 localPoint;
        public int indexA;
        public int indexB;
        public float invMassA, invMassB;
        public Vector2 localCenterA, localCenterB;
        public float invIA, invIB;
        public ManifoldType type;
        public float radiusA, radiusB;
        public int pointCount;
    }

    public sealed class VelocityConstraintPoint
    {
        public Vector2 rA;
        public Vector2 rB;
        public float normalImpulse;
        public float tangentImpulse;
        public float normalMass;
        public float tangentMass;
        public float velocityBias;
    }

    public sealed class ContactVelocityConstraint
    {
        public VelocityConstraintPoint[] points = new VelocityConstraintPoint[Settings.MaxManifoldPoints];
        public Vector2 normal;
        public Mat22 normalMass;
        public Mat22 K;
        public int indexA;
        public int indexB;
        public float invMassA, invMassB;
        public float invIA, invIB;
        public float friction;
        public float restitution;
        public float tangentSpeed;
        public int pointCount;
        public int contactIndex;

        public ContactVelocityConstraint()
        {
            for (int i = 0; i < Settings.MaxManifoldPoints; i++)
            {
                points[i] = new VelocityConstraintPoint();
            }
        }
    }

    public class ContactSolver
    {
        internal SolverPosition[] _positions;
        internal SolverVelocity[] _velocities;
        internal int[] _locks;
        public ContactPositionConstraint[] _positionConstraints;
        public ContactVelocityConstraint[] _velocityConstraints;
        public Contact[] _contacts;
        public int _count;
        int _velocityConstraintsMultithreadThreshold;
        int _positionConstraintsMultithreadThreshold;

        internal void Reset(ref TimeStep step, int count, Contact[] contacts, SolverPosition[] positions, SolverVelocity[] velocities,
            int[] locks, int velocityConstraintsMultithreadThreshold, int positionConstraintsMultithreadThreshold)
        {
            _count = count;
            _positions = positions;
            _velocities = velocities;
            _locks = locks;
            _contacts = contacts;
            _velocityConstraintsMultithreadThreshold = velocityConstraintsMultithreadThreshold;
            _positionConstraintsMultithreadThreshold = positionConstraintsMultithreadThreshold;

            // grow the array
            if (_velocityConstraints == null || _velocityConstraints.Length < count)
            {
                int newBufferCount = Math.Max(count, 32);
                newBufferCount = newBufferCount + (newBufferCount * 2 >> 4); // grow by x1.125f
                newBufferCount = (newBufferCount + 31) & (~31); // grow in chunks of 32.
                int oldBufferCount = (_velocityConstraints == null) ? 0 : _velocityConstraints.Length;
                Array.Resize(ref _velocityConstraints, newBufferCount);
                Array.Resize(ref _positionConstraints, newBufferCount);

                for (int i = oldBufferCount; i < newBufferCount; i++)
                {
                    _velocityConstraints[i] = new ContactVelocityConstraint();
                    _positionConstraints[i] = new ContactPositionConstraint();
                }
            }

            // Initialize position independent portions of the constraints.
            for (int i = 0; i < _count; ++i)
            {
                Contact contact = contacts[i];

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                Shape shapeA = fixtureA.Shape;
                Shape shapeB = fixtureB.Shape;
                float radiusA = shapeA.Radius;
                float radiusB = shapeB.Radius;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;
                Manifold manifold = contact.Manifold;

                int pointCount = manifold.PointCount;
                Debug.Assert(pointCount > 0);

                ContactVelocityConstraint vc = _velocityConstraints[i];
                vc.friction = contact.Friction;
                vc.restitution = contact.Restitution;
                vc.tangentSpeed = contact.TangentSpeed;
                vc.indexA = bodyA.IslandIndex;
                vc.indexB = bodyB.IslandIndex;
                vc.invMassA = bodyA._invMass;
                vc.invMassB = bodyB._invMass;
                vc.invIA = bodyA._invI;
                vc.invIB = bodyB._invI;
                vc.contactIndex = i;
                vc.pointCount = pointCount;
                vc.K.SetZero();
                vc.normalMass.SetZero();

                ContactPositionConstraint pc = _positionConstraints[i];
                pc.indexA = bodyA.IslandIndex;
                pc.indexB = bodyB.IslandIndex;
                pc.invMassA = bodyA._invMass;
                pc.invMassB = bodyB._invMass;
                pc.localCenterA = bodyA._sweep.LocalCenter;
                pc.localCenterB = bodyB._sweep.LocalCenter;
                pc.invIA = bodyA._invI;
                pc.invIB = bodyB._invI;
                pc.localNormal = manifold.LocalNormal;
                pc.localPoint = manifold.LocalPoint;
                pc.pointCount = pointCount;
                pc.radiusA = radiusA;
                pc.radiusB = radiusB;
                pc.type = manifold.Type;

                for (int j = 0; j < pointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    VelocityConstraintPoint vcp = vc.points[j];

                    if (step.warmStarting)
                    {
                        vcp.normalImpulse = step.dtRatio * cp.NormalImpulse;
                        vcp.tangentImpulse = step.dtRatio * cp.TangentImpulse;
                    }
                    else
                    {
                        vcp.normalImpulse = 0.0f;
                        vcp.tangentImpulse = 0.0f;
                    }

                    vcp.rA = Vector2.Zero;
                    vcp.rB = Vector2.Zero;
                    vcp.normalMass = 0.0f;
                    vcp.tangentMass = 0.0f;
                    vcp.velocityBias = 0.0f;

                    pc.localPoints[j] = cp.LocalPoint;
                }
            }
        }

        public void InitializeVelocityConstraints()
        {
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = _velocityConstraints[i];
                ContactPositionConstraint pc = _positionConstraints[i];

                float radiusA = pc.radiusA;
                float radiusB = pc.radiusB;
                Manifold manifold = _contacts[vc.contactIndex].Manifold;

                int indexA = vc.indexA;
                int indexB = vc.indexB;

                float mA = vc.invMassA;
                float mB = vc.invMassB;
                float iA = vc.invIA;
                float iB = vc.invIB;
                Vector2 localCenterA = pc.localCenterA;
                Vector2 localCenterB = pc.localCenterB;

                Vector2 cA = _positions[indexA].c;
                float aA = _positions[indexA].a;
                Vector2 vA = _velocities[indexA].v;
                float wA = _velocities[indexA].w;

                Vector2 cB = _positions[indexB].c;
                float aB = _positions[indexB].a;
                Vector2 vB = _velocities[indexB].v;
                float wB = _velocities[indexB].w;

                Debug.Assert(manifold.PointCount > 0);

                Transform xfA = new Transform(Vector2.Zero, aA);
                Transform xfB = new Transform(Vector2.Zero, aB);
                xfA.p = cA - ComplexF.Multiply(ref localCenterA, ref xfA.q);
                xfB.p = cB - ComplexF.Multiply(ref localCenterB, ref xfB.q);

                Vector2 normal;
                FixedArray2<Vector2> points;
                WorldManifold.Initialize(ref manifold, ref xfA, radiusA, ref xfB, radiusB, out normal, out points);

                vc.normal = normal;
                Vector2 tangent = MathUtils.Rot270(ref vc.normal);

                int pointCount = vc.pointCount;
                for (int j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.points[j];

                    vcp.rA = points[j] - cA;
                    vcp.rB = points[j] - cB;

                    float rnA = MathUtils.Cross(ref vcp.rA, ref vc.normal);
                    float rnB = MathUtils.Cross(ref vcp.rB, ref vc.normal);

                    float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    vcp.normalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;


                    float rtA = MathUtils.Cross(ref vcp.rA, ref tangent);
                    float rtB = MathUtils.Cross(ref vcp.rB, ref tangent);

                    float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

                    vcp.tangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

                    // Setup a velocity bias for restitution.
                    vcp.velocityBias = 0.0f;
                    float vRel = Vector2.Dot(vc.normal, vB + MathUtils.Cross(wB, ref vcp.rB) - vA - MathUtils.Cross(wA, ref vcp.rA));
                    if (vRel < -Settings.VelocityThreshold)
                    {
                        vcp.velocityBias = -vc.restitution * vRel;
                    }
                }

                // If we have two points, then prepare the block solver.
                if (vc.pointCount == 2)
                {
                    VelocityConstraintPoint vcp1 = vc.points[0];
                    VelocityConstraintPoint vcp2 = vc.points[1];

                    float rn1A = MathUtils.Cross(ref vcp1.rA, ref vc.normal);
                    float rn1B = MathUtils.Cross(ref vcp1.rB, ref vc.normal);
                    float rn2A = MathUtils.Cross(ref vcp2.rA, ref vc.normal);
                    float rn2B = MathUtils.Cross(ref vcp2.rB, ref vc.normal);

                    float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
                    float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
                    float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float k_maxConditionNumber = 1000.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        vc.K.ex = new Vector2(k11, k12);
                        vc.K.ey = new Vector2(k12, k22);
                        vc.normalMass = vc.K.Inverse;
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        vc.pointCount = 1;
                    }
                }
            }
        }

        public void WarmStart()
        {
            // Warm start.
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = _velocityConstraints[i];

                int indexA = vc.indexA;
                int indexB = vc.indexB;
                float mA = vc.invMassA;
                float iA = vc.invIA;
                float mB = vc.invMassB;
                float iB = vc.invIB;
                int pointCount = vc.pointCount;

                Vector2 vA = _velocities[indexA].v;
                float wA = _velocities[indexA].w;
                Vector2 vB = _velocities[indexB].v;
                float wB = _velocities[indexB].w;

                Vector2 normal = vc.normal;
                Vector2 tangent = MathUtils.Rot270(ref normal);

                for (int j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.points[j];
                    Vector2 P = vcp.normalImpulse * normal + vcp.tangentImpulse * tangent;
                    wA -= iA * MathUtils.Cross(ref vcp.rA, ref P);
                    vA -= mA * P;
                    wB += iB * MathUtils.Cross(ref vcp.rB, ref P);
                    vB += mB * P;
                }

                _velocities[indexA].v = vA;
                _velocities[indexA].w = wA;
                _velocities[indexB].v = vB;
                _velocities[indexB].w = wB;
            }
        }

        public void SolveVelocityConstraints()
        {
            if (_count >= _velocityConstraintsMultithreadThreshold && System.Environment.ProcessorCount > 1)
            {
                if (_count == 0) return;
                var batchSize = (int)Math.Ceiling((float)_count / System.Environment.ProcessorCount);
                var batches = (int)Math.Ceiling((float)_count / batchSize);

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
                SolveVelocityConstraintsWaitLock.Reset(batches);
                for (int i = 0; i < batches; i++)
                {
                    var start = i * batchSize;
                    var end = Math.Min(start + batchSize, _count);
                    ThreadPool.QueueUserWorkItem( SolveVelocityConstraintsCallback, SolveVelocityConstraintsState.Get(this, start,end));                    
                }
                // We avoid SolveVelocityConstraintsWaitLock.Wait(); because it spins a few milliseconds before going into sleep. Going into sleep(0) directly in a while loop is faster.
                while (SolveVelocityConstraintsWaitLock.CurrentCount > 0)
                    Thread.Sleep(0);
#else
                SolveVelocityConstraints(0, _count);
#endif
            }
            else
            {                
                SolveVelocityConstraints(0, _count);
            }

            return;
        }

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
        CountdownEvent SolveVelocityConstraintsWaitLock = new CountdownEvent(0);
        static void SolveVelocityConstraintsCallback(object state)
        {
            var svcState = (SolveVelocityConstraintsState)state;

            svcState.ContactSolver.SolveVelocityConstraints(svcState.Start, svcState.End);
            SolveVelocityConstraintsState.Return(svcState);
            svcState.ContactSolver.SolveVelocityConstraintsWaitLock.Signal();
        }

        private class SolveVelocityConstraintsState
        {
            private static System.Collections.Concurrent.ConcurrentQueue<SolveVelocityConstraintsState> _queue = new System.Collections.Concurrent.ConcurrentQueue<SolveVelocityConstraintsState>(); // pool

            public ContactSolver ContactSolver;
            public int Start { get; private set; }
            public int End { get; private set; }

            private SolveVelocityConstraintsState()
            {
            }

            internal static object Get(ContactSolver contactSolver, int start, int end)
            {
                SolveVelocityConstraintsState result;
                if (!_queue.TryDequeue(out result))
                    result = new SolveVelocityConstraintsState();

                result.ContactSolver = contactSolver;
                result.Start = start;
                result.End = end;

                return result;
            }

            internal static void Return(object state)
            {
                _queue.Enqueue((SolveVelocityConstraintsState)state);
            }
        }
#endif

        private void SolveVelocityConstraints(int start, int end)
        {
            for (int i = start; i < end; ++i)
            {
                ContactVelocityConstraint vc = _velocityConstraints[i];

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
                // find lower order item
                int orderedIndexA = vc.indexA;
                int orderedIndexB = vc.indexB;
                if (orderedIndexB < orderedIndexA)
                {
                    orderedIndexA = vc.indexB;
                    orderedIndexB = vc.indexA;
                }
                
                for (; ; )
                {
                    if (Interlocked.CompareExchange(ref _locks[orderedIndexA], 1, 0) == 0)
                    {
                        if (Interlocked.CompareExchange(ref _locks[orderedIndexB], 1, 0) == 0)
                            break;
                        System.Threading.Interlocked.Exchange(ref _locks[orderedIndexA], 0);
                    }
                    Thread.Sleep(0);
                }
#endif

                int indexA = vc.indexA;
                int indexB = vc.indexB;
                float mA = vc.invMassA;
                float iA = vc.invIA;
                float mB = vc.invMassB;
                float iB = vc.invIB;
                int pointCount = vc.pointCount;

                Vector2 vA = _velocities[indexA].v;
                float wA = _velocities[indexA].w;
                Vector2 vB = _velocities[indexB].v;
                float wB = _velocities[indexB].w;

                Vector2 normal = vc.normal;
                Vector2 tangent = MathUtils.Rot270(ref normal);
                float friction = vc.friction;

                Debug.Assert(pointCount == 1 || pointCount == 2);

                // Solve tangent constraints first because non-penetration is more important
                // than friction.
                for (int j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.points[j];

                    // Relative velocity at contact
                    Vector2 dv = vB + MathUtils.Cross(wB, ref vcp.rB) - vA - MathUtils.Cross(wA, ref vcp.rA);

                    // Compute tangent force
                    float vt = Vector2.Dot(dv, tangent) - vc.tangentSpeed;
                    float lambda = vcp.tangentMass * (-vt);

                    // b2Clamp the accumulated force
                    float maxFriction = friction * vcp.normalImpulse;
                    float newImpulse = MathUtils.Clamp(vcp.tangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - vcp.tangentImpulse;
                    vcp.tangentImpulse = newImpulse;

                    // Apply contact impulse
                    Vector2 P = lambda * tangent;

                    vA -= mA * P;
                    wA -= iA * MathUtils.Cross(ref vcp.rA, ref P);

                    vB += mB * P;
                    wB += iB * MathUtils.Cross(ref vcp.rB, ref P);
                }

                // Solve normal constraints
                if (vc.pointCount == 1)
                {
                    VelocityConstraintPoint vcp = vc.points[0];

                    // Relative velocity at contact
                    Vector2 dv = vB + MathUtils.Cross(wB, ref vcp.rB) - vA - MathUtils.Cross(wA, ref vcp.rA);

                    // Compute normal impulse
                    float vn = Vector2.Dot(dv, normal);
                    float lambda = -vcp.normalMass * (vn - vcp.velocityBias);

                    // b2Clamp the accumulated impulse
                    float newImpulse = Math.Max(vcp.normalImpulse + lambda, 0.0f);
                    lambda = newImpulse - vcp.normalImpulse;
                    vcp.normalImpulse = newImpulse;

                    // Apply contact impulse
                    Vector2 P = lambda * normal;
                    vA -= mA * P;
                    wA -= iA * MathUtils.Cross(ref vcp.rA, ref P);

                    vB += mB * P;
                    wB += iB * MathUtils.Cross(ref vcp.rB, ref P);
                }
                else
                {
                    // Block solver developed in collaboration with Dirk Gregorius (back in 01/07 on Box2D_Lite).
                    // Build the mini LCP for this contact patch
                    //
                    // vn = A * x + b, vn >= 0, , vn >= 0, x >= 0 and vn_i * x_i = 0 with i = 1..2
                    //
                    // A = J * W * JT and J = ( -n, -r1 x n, n, r2 x n )
                    // b = vn0 - velocityBias
                    //
                    // The system is solved using the "Total enumeration method" (s. Murty). The complementary constraint vn_i * x_i
                    // implies that we must have in any solution either vn_i = 0 or x_i = 0. So for the 2D contact problem the cases
                    // vn1 = 0 and vn2 = 0, x1 = 0 and x2 = 0, x1 = 0 and vn2 = 0, x2 = 0 and vn1 = 0 need to be tested. The first valid
                    // solution that satisfies the problem is chosen.
                    // 
                    // In order to account of the accumulated impulse 'a' (because of the iterative nature of the solver which only requires
                    // that the accumulated impulse is clamped and not the incremental impulse) we change the impulse variable (x_i).
                    //
                    // Substitute:
                    // 
                    // x = a + d
                    // 
                    // a := old total impulse
                    // x := new total impulse
                    // d := incremental impulse 
                    //
                    // For the current iteration we extend the formula for the incremental impulse
                    // to compute the new total impulse:
                    //
                    // vn = A * d + b
                    //    = A * (x - a) + b
                    //    = A * x + b - A * a
                    //    = A * x + b'
                    // b' = b - A * a;

                    VelocityConstraintPoint cp1 = vc.points[0];
                    VelocityConstraintPoint cp2 = vc.points[1];

                    Vector2 a = new Vector2(cp1.normalImpulse, cp2.normalImpulse);
                    Debug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

                    // Relative velocity at contact
                    Vector2 dv1 = vB + MathUtils.Cross(wB, ref cp1.rB) - vA - MathUtils.Cross(wA, ref cp1.rA);
                    Vector2 dv2 = vB + MathUtils.Cross(wB, ref cp2.rB) - vA - MathUtils.Cross(wA, ref cp2.rA);

                    // Compute normal velocity
                    float vn1 = Vector2.Dot(dv1, normal);
                    float vn2 = Vector2.Dot(dv2, normal);

                    Vector2 b = new Vector2();
                    b.X = vn1 - cp1.velocityBias;
                    b.Y = vn2 - cp2.velocityBias;

                    // Compute b'
                    b -= MathUtils.Mul(ref vc.K, ref a);

                    const float k_errorTol = 1e-3f;
                    //B2_NOT_USED(k_errorTol);

                    for (; ; )
                    {
                        //
                        // Case 1: vn = 0
                        //
                        // 0 = A * x + b'
                        //
                        // Solve for x:
                        //
                        // x = - inv(A) * b'
                        //
                        Vector2 x = -MathUtils.Mul(ref vc.normalMass, ref b);

                        if (x.X >= 0.0f && x.Y >= 0.0f)
                        {
                            // Get the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(ref cp1.rA, ref P1) + MathUtils.Cross(ref cp2.rA, ref P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(ref cp1.rB, ref P1) + MathUtils.Cross(ref cp2.rB, ref P2));

                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER 
					// Postconditions
					dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
					dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

					// Compute normal velocity
					vn1 = Vector2.Dot(dv1, normal);
					vn2 = Vector2.Dot(dv2, normal);

					b2Assert(b2Abs(vn1 - cp1.velocityBias) < k_errorTol);
					b2Assert(b2Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 2: vn1 = 0 and x2 = 0
                        //
                        //   0 = a11 * x1 + a12 * 0 + b1' 
                        // vn2 = a21 * x1 + a22 * 0 + b2'
                        //
                        x.X = -cp1.normalMass * b.X;
                        x.Y = 0.0f;
                        vn1 = 0.0f;
                        vn2 = vc.K.ex.Y * x.X + b.Y;

                        if (x.X >= 0.0f && vn2 >= 0.0f)
                        {
                            // Get the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(ref cp1.rA, ref P1) + MathUtils.Cross(ref cp2.rA, ref P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(ref cp1.rB, ref P1) + MathUtils.Cross(ref cp2.rB, ref P2));

                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER
					// Postconditions
					dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);

					// Compute normal velocity
					vn1 = Vector2.Dot(dv1, normal);

					b2Assert(b2Abs(vn1 - cp1.velocityBias) < k_errorTol);
#endif
                            break;
                        }


                        //
                        // Case 3: vn2 = 0 and x1 = 0
                        //
                        // vn1 = a11 * 0 + a12 * x2 + b1' 
                        //   0 = a21 * 0 + a22 * x2 + b2'
                        //
                        x.X = 0.0f;
                        x.Y = -cp2.normalMass * b.Y;
                        vn1 = vc.K.ey.X * x.Y + b.X;
                        vn2 = 0.0f;

                        if (x.Y >= 0.0f && vn1 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(ref cp1.rA, ref P1) + MathUtils.Cross(ref cp2.rA, ref P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(ref cp1.rB, ref P1) + MathUtils.Cross(ref cp2.rB, ref P2));

                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER
					// Postconditions
					dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

					// Compute normal velocity
					vn2 = Vector2.Dot(dv2, normal);

					b2Assert(b2Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 4: x1 = 0 and x2 = 0
                        // 
                        // vn1 = b1
                        // vn2 = b2;
                        x.X = 0.0f;
                        x.Y = 0.0f;
                        vn1 = b.X;
                        vn2 = b.Y;

                        if (vn1 >= 0.0f && vn2 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(ref cp1.rA, ref P1) + MathUtils.Cross(ref cp2.rA, ref P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(ref cp1.rB, ref P1) + MathUtils.Cross(ref cp2.rB, ref P2));

                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

                            break;
                        }

                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }
                }

                _velocities[indexA].v = vA;
                _velocities[indexA].w = wA;
                _velocities[indexB].v = vB;
                _velocities[indexB].w = wB;

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
                System.Threading.Interlocked.Exchange(ref _locks[orderedIndexB], 0);
                System.Threading.Interlocked.Exchange(ref _locks[orderedIndexA], 0);
#endif
            }
        }

        public void StoreImpulses()
        {
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = _velocityConstraints[i];
                Manifold manifold = _contacts[vc.contactIndex].Manifold;

                for (int j = 0; j < vc.pointCount; ++j)
                {
                    ManifoldPoint point = manifold.Points[j];
                    point.NormalImpulse = vc.points[j].normalImpulse;
                    point.TangentImpulse = vc.points[j].tangentImpulse;
                    manifold.Points[j] = point;
                }

                _contacts[vc.contactIndex].Manifold = manifold;
            }
        }

        public bool SolvePositionConstraints()
        {
            bool contactsOkay = false;

            if (_count >= _positionConstraintsMultithreadThreshold && System.Environment.ProcessorCount > 1)
            {
                if (_count == 0) return true;
                var batchSize = (int)Math.Ceiling((float)_count / System.Environment.ProcessorCount);
                var batches = (int)Math.Ceiling((float)_count / batchSize);

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
                Parallel.For(0, batches, (i) =>
                {
                    var start = i * batchSize;
                    var end = Math.Min(start + batchSize, _count);
                    var res = SolvePositionConstraints(start, end);
                    lock (this)
                    {
                        contactsOkay = contactsOkay || res;
                    }
                });
#else            
                contactsOkay = SolvePositionConstraints(0, _count);
#endif
            }
            else
            {
                contactsOkay = SolvePositionConstraints(0, _count);
            }
            
            return contactsOkay;
        }

        private bool SolvePositionConstraints(int start, int end)
        {
            float minSeparation = 0.0f;

            for (int i = start; i < end; ++i)
            {
                ContactPositionConstraint pc = _positionConstraints[i];

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
                // Find lower order item.
                int orderedIndexA = pc.indexA;
                int orderedIndexB = pc.indexB;
                if (orderedIndexB < orderedIndexA)
                {
                    orderedIndexA = pc.indexB;
                    orderedIndexB = pc.indexA;
                }
                
                // Lock bodies.
                for (; ; )
                {
                    if (Interlocked.CompareExchange(ref _locks[orderedIndexA], 1, 0) == 0)
                    {
                        if (Interlocked.CompareExchange(ref _locks[orderedIndexB], 1, 0) == 0)
                            break;
                        System.Threading.Interlocked.Exchange(ref _locks[orderedIndexA], 0);
                    }
                    Thread.Sleep(0);
                }
#endif


                int indexA = pc.indexA;
                int indexB = pc.indexB;
                Vector2 localCenterA = pc.localCenterA;
                float mA = pc.invMassA;
                float iA = pc.invIA;
                Vector2 localCenterB = pc.localCenterB;
                float mB = pc.invMassB;
                float iB = pc.invIB;
                int pointCount = pc.pointCount;

                Vector2 cA = _positions[indexA].c;
                float aA = _positions[indexA].a;
                Vector2 cB = _positions[indexB].c;
                float aB = _positions[indexB].a;

                // Solve normal constraints
                for (int j = 0; j < pointCount; ++j)
                {
                    Transform xfA = new Transform(Vector2.Zero, aA);
                    Transform xfB = new Transform(Vector2.Zero, aB);
                    xfA.p = cA - ComplexF.Multiply(ref localCenterA, ref xfA.q);
                    xfB.p = cB - ComplexF.Multiply(ref localCenterB, ref xfB.q);

                    Vector2 normal;
                    Vector2 point;
                    float separation;

                    PositionSolverManifold.Initialize(pc, ref xfA, ref xfB, j, out normal, out point, out separation);

                    Vector2 rA = point - cA;
                    Vector2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(Settings.Baumgarte * (separation + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(ref rA, ref normal);
                    float rnB = MathUtils.Cross(ref rB, ref normal);
                    float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    Vector2 P = impulse * normal;

                    cA -= mA * P;
                    aA -= iA * MathUtils.Cross(ref rA, ref P);

                    cB += mB * P;
                    aB += iB * MathUtils.Cross(ref rB, ref P);
                }

                _positions[indexA].c = cA;
                _positions[indexA].a = aA;
                _positions[indexB].c = cB;
                _positions[indexB].a = aB;

#if NET40 || NET45 || NETSTANDARD2_0_OR_GREATER
                // Unlock bodies.
                System.Threading.Interlocked.Exchange(ref _locks[orderedIndexB], 0);
                System.Threading.Interlocked.Exchange(ref _locks[orderedIndexA], 0);
#endif
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -3.0f * Settings.LinearSlop;
        }

        // Sequential position solver for position constraints.
        public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _count; ++i)
            {
                ContactPositionConstraint pc = _positionConstraints[i];

                int indexA = pc.indexA;
                int indexB = pc.indexB;
                Vector2 localCenterA = pc.localCenterA;
                Vector2 localCenterB = pc.localCenterB;
                int pointCount = pc.pointCount;

                float mA = 0.0f;
                float iA = 0.0f;
                if (indexA == toiIndexA || indexA == toiIndexB)
                {
                    mA = pc.invMassA;
                    iA = pc.invIA;
                }

                float mB = 0.0f;
                float iB = 0.0f;
                if (indexB == toiIndexA || indexB == toiIndexB)
                {
                    mB = pc.invMassB;
                    iB = pc.invIB;
                }

                Vector2 cA = _positions[indexA].c;
                float aA = _positions[indexA].a;

                Vector2 cB = _positions[indexB].c;
                float aB = _positions[indexB].a;

                // Solve normal constraints
                for (int j = 0; j < pointCount; ++j)
                {
                    Transform xfA = new Transform(Vector2.Zero, aA);
                    Transform xfB = new Transform(Vector2.Zero, aB);
                    xfA.p = cA - ComplexF.Multiply(ref localCenterA, ref xfA.q);
                    xfB.p = cB - ComplexF.Multiply(ref localCenterB, ref xfB.q);

                    Vector2 normal;
                    Vector2 point;
                    float separation;

                    PositionSolverManifold.Initialize(pc, ref xfA, ref xfB, j, out normal, out point, out separation);

                    Vector2 rA = point - cA;
                    Vector2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(Settings.Baumgarte * (separation + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(ref rA, ref normal);
                    float rnB = MathUtils.Cross(ref rB, ref normal);
                    float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    Vector2 P = impulse * normal;

                    cA -= mA * P;
                    aA -= iA * MathUtils.Cross(ref rA, ref P);

                    cB += mB * P;
                    aB += iB * MathUtils.Cross(ref rB, ref P);
                }

                _positions[indexA].c = cA;
                _positions[indexA].a = aA;

                _positions[indexB].c = cB;
                _positions[indexB].a = aB;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }

        public static class WorldManifold
        {
            /// <summary>
            /// Evaluate the manifold with supplied transforms. This assumes
            /// modest motion from the original state. This does not change the
            /// point count, impulses, etc. The radii must come from the Shapes
            /// that generated the manifold.
            /// </summary>
            /// <param name="manifold">The manifold.</param>
            /// <param name="xfA">The transform for A.</param>
            /// <param name="radiusA">The radius for A.</param>
            /// <param name="xfB">The transform for B.</param>
            /// <param name="radiusB">The radius for B.</param>
            /// <param name="normal">World vector pointing from A to B</param>
            /// <param name="points">Torld contact point (point of intersection).</param>
            public static void Initialize(ref Manifold manifold, ref Transform xfA, float radiusA, ref Transform xfB, float radiusB, out Vector2 normal, out FixedArray2<Vector2> points)
            {
                normal = Vector2.Zero;
                points = new FixedArray2<Vector2>();

                if (manifold.PointCount == 0)
                {
                    return;
                }

                switch (manifold.Type)
                {
                    case ManifoldType.Circles:
                        {
                            normal = new Vector2(1.0f, 0.0f);
                            Vector2 pointA = Transform.Multiply(ref manifold.LocalPoint, ref xfA);
                            Vector2 pointB = Transform.Multiply(manifold.Points[0].LocalPoint, ref xfB);
                            if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                            {
                                normal = pointB - pointA;
                                normal = Vector2.Normalize(normal);
                            }

                            Vector2 cA = pointA + radiusA * normal;
                            Vector2 cB = pointB - radiusB * normal;
                            points[0] = 0.5f * (cA + cB);
                        }
                        break;

                    case ManifoldType.FaceA:
                        {
                            normal = ComplexF.Multiply(ref manifold.LocalNormal, ref xfA.q);
                            Vector2 planePoint = Transform.Multiply(ref manifold.LocalPoint, ref xfA);

                            for (int i = 0; i < manifold.PointCount; ++i)
                            {
                                Vector2 clipPoint = Transform.Multiply(manifold.Points[i].LocalPoint, ref xfB);
                                Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
                                Vector2 cB = clipPoint - radiusB * normal;
                                points[i] = 0.5f * (cA + cB);
                            }
                        }
                        break;

                    case ManifoldType.FaceB:
                        {
                            normal = ComplexF.Multiply(ref manifold.LocalNormal, ref xfB.q);
                            Vector2 planePoint = Transform.Multiply(ref manifold.LocalPoint, ref xfB);

                            for (int i = 0; i < manifold.PointCount; ++i)
                            {
                                Vector2 clipPoint = Transform.Multiply(manifold.Points[i].LocalPoint, ref xfA);
                                Vector2 cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
                                Vector2 cA = clipPoint - radiusA * normal;
                                points[i] = 0.5f * (cA + cB);
                            }

                            // Ensure normal points from A to B.
                            normal = -normal;
                        }
                        break;
                }
            }
        }

        private static class PositionSolverManifold
        {
            public static void Initialize(ContactPositionConstraint pc, ref Transform xfA, ref Transform xfB, int index, out Vector2 normal, out Vector2 point, out float separation)
            {
                Debug.Assert(pc.pointCount > 0);

                switch (pc.type)
                {
                    case ManifoldType.Circles:
                        {
                            Vector2 pointA = Transform.Multiply(ref pc.localPoint, ref xfA);
                            Vector2 pointB = Transform.Multiply(pc.localPoints[0], ref xfB);
                            normal = pointB - pointA;

                            // Handle zero normalization
                            if (normal != Vector2.Zero)
                                normal = Vector2.Normalize(normal);

                            point = 0.5f * (pointA + pointB);
                            separation = Vector2.Dot(pointB - pointA, normal) - pc.radiusA - pc.radiusB;
                        }
                        break;

                    case ManifoldType.FaceA:
                        {
                            ComplexF.Multiply(ref pc.localNormal, ref xfA.q, out normal);
                            Vector2 planePoint = Transform.Multiply(ref pc.localPoint, ref xfA);

                            Vector2 clipPoint = Transform.Multiply(pc.localPoints[index], ref xfB);
                            separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.radiusA - pc.radiusB;
                            point = clipPoint;
                        }
                        break;

                    case ManifoldType.FaceB:
                        {
                            ComplexF.Multiply(ref pc.localNormal, ref xfB.q, out normal);
                            Vector2 planePoint = Transform.Multiply(ref pc.localPoint, ref xfB);

                            Vector2 clipPoint = Transform.Multiply(pc.localPoints[index], ref xfA);
                            separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.radiusA - pc.radiusB;
                            point = clipPoint;

                            // Ensure normal points from A to B
                            normal = -normal;
                        }
                        break;
                    default:
                        normal = Vector2.Zero;
                        point = Vector2.Zero;
                        separation = 0;
                        break;

                }
            }
        }
    }


}