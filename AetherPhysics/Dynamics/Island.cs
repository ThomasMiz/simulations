﻿// Copyright (c) 2017 Kastellanos Nikolaos

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
using AetherPhysics.Common;
using AetherPhysics.Dynamics.Contacts;
using AetherPhysics.Dynamics.Joints;
using System.Numerics;

namespace AetherPhysics.Dynamics
{
    /// <summary>
    /// This is an internal class.
    /// </summary>
    public class Island
    {
        private ContactManager _contactManager;
        private ContactSolver _contactSolver = new ContactSolver();
        private Contact[] _contacts;
        private Joint[] _joints;

        private const float LinTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;
        private const float AngTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;
        private Stopwatch _watch = new Stopwatch();

        public Body[] Bodies;
        public int BodyCount;
        public int ContactCount;
        public int JointCount;

        internal SolverVelocity[] _velocities;
        internal SolverPosition[] _positions;
        internal int[] _locks;

        public int BodyCapacity;
        public int ContactCapacity;
        public int JointCapacity;
        public TimeSpan JointUpdateTime;

        public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
        {
            BodyCapacity = bodyCapacity;
            ContactCapacity = contactCapacity;
            JointCapacity = jointCapacity;
            BodyCount = 0;
            ContactCount = 0;
            JointCount = 0;

            _contactManager = contactManager;

            if (Bodies == null || Bodies.Length < bodyCapacity)
            {
                int newBodyBufferCapacity = Math.Max(bodyCapacity, 32);
                newBodyBufferCapacity = (newBodyBufferCapacity + 31) & (~31); // grow in chunks of 32.
                Bodies = new Body[newBodyBufferCapacity];
                _velocities = new SolverVelocity[newBodyBufferCapacity];
                _positions = new SolverPosition[newBodyBufferCapacity];
                _locks = new int[newBodyBufferCapacity];
            }

            if (_contacts == null || _contacts.Length < contactCapacity)
            {
                int newContactBufferCapacity = Math.Max(contactCapacity, 32);
                newContactBufferCapacity = newContactBufferCapacity + (newContactBufferCapacity*2 >> 4); // grow by x1.125f
                newContactBufferCapacity = (newContactBufferCapacity + 31) & (~31); // grow in chunks of 32.
                _contacts = new Contact[newContactBufferCapacity];
            }

            if (_joints == null || _joints.Length < jointCapacity)
            {
                int newJointBufferCapacity = Math.Max(jointCapacity, 32);
                newJointBufferCapacity = (newJointBufferCapacity + 31) & (~31); // grow in chunks of 32.
                _joints = new Joint[newJointBufferCapacity];
            }
        }

        public void Clear()
        {
            BodyCount = 0;
            ContactCount = 0;
            JointCount = 0;
        }

        internal void Solve(ref TimeStep step, ref Vector2 gravity)
        {
            float h = step.dt;

            // Integrate velocities and apply damping. Initialize the body state.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                Vector2 c = b._sweep.C;
                float a = b._sweep.A;
                Vector2 v = b._linearVelocity;
                float w = b._angularVelocity;

                // Store positions for continuous collision.
                b._sweep.C0 = b._sweep.C;
                b._sweep.A0 = b._sweep.A;

                if (b.BodyType == BodyType.Dynamic)
                {
                    // Integrate velocities.

                    // FPE: Only apply gravity if the body wants it.
                    if (b.IgnoreGravity)
                        v += h * (b._invMass * b._force);
                    else
                        v += h * (gravity + b._invMass * b._force);

                    w += h * b._invI * b._torque;

                    // Apply damping.
                    // ODE: dv/dt + c * v = 0
                    // Solution: v(t) = v0 * exp(-c * t)
                    // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                    // v2 = exp(-c * dt) * v1
                    // Taylor expansion:
                    // v2 = (1.0f - c * dt) * v1
                    v *= MathUtils.Clamp(1.0f - h * b.LinearDamping, 0.0f, 1.0f);
                    w *= MathUtils.Clamp(1.0f - h * b.AngularDamping, 0.0f, 1.0f);
                }

                _positions[i].c = c;
                _positions[i].a = a;
                _velocities[i].v = v;
                _velocities[i].w = w;
            }

            // Solver data
            SolverData solverData = new SolverData();
            solverData.step = step;
            solverData.positions = _positions;
            solverData.velocities = _velocities;
            solverData.locks = _locks;

            _contactSolver.Reset(ref step, ContactCount, _contacts, _positions, _velocities,
                _locks, _contactManager.VelocityConstraintsMultithreadThreshold, _contactManager.PositionConstraintsMultithreadThreshold);
            _contactSolver.InitializeVelocityConstraints();

            if (step.warmStarting)
            {
                _contactSolver.WarmStart();
            }

            if (Settings.EnableDiagnostics)
                _watch.Start();

            for (int i = 0; i < JointCount; ++i)
            {
                if (_joints[i].Enabled)
                    _joints[i].InitVelocityConstraints(ref solverData);
            }

            if (Settings.EnableDiagnostics)
                _watch.Stop();

            // Solve velocity constraints.
            for (int i = 0; i < step.velocityIterations; ++i)
            {
                for (int j = 0; j < JointCount; ++j)
                {
                    Joint joint = _joints[j];

                    if (!joint.Enabled)
                        continue;

                    if (Settings.EnableDiagnostics)
                        _watch.Start();

                    joint.SolveVelocityConstraints(ref solverData);
                    joint.Validate(step.inv_dt);

                    if (Settings.EnableDiagnostics)
                        _watch.Stop();
                }

                _contactSolver.SolveVelocityConstraints();
            }

            // Store impulses for warm starting.
            _contactSolver.StoreImpulses();

            // Integrate positions
            for (int i = 0; i < BodyCount; ++i)
            {
                Vector2 c = _positions[i].c;
                float a = _positions[i].a;
                Vector2 v = _velocities[i].v;
                float w = _velocities[i].w;

                // Check for large velocities
                Vector2 translation = h * v;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    v *= ratio;
                }

                float rotation = h * w;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    w *= ratio;
                }

                // Integrate
                c += h * v;
                a += h * w;

                _positions[i].c = c;
                _positions[i].a = a;
                _velocities[i].v = v;
                _velocities[i].w = w;
            }


            // Solve position constraints
            bool positionSolved = false;
            for (int i = 0; i < step.positionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints();

                bool jointsOkay = true;
                for (int j = 0; j < JointCount; ++j)
                {
                    Joint joint = _joints[j];

                    if (!joint.Enabled)
                        continue;

                    if (Settings.EnableDiagnostics)
                        _watch.Start();

                    bool jointOkay = joint.SolvePositionConstraints(ref solverData);

                    if (Settings.EnableDiagnostics)
                        _watch.Stop();

                    jointsOkay = jointsOkay && jointOkay;
                }

                if (contactsOkay && jointsOkay)
                {
                    // Exit early if the position errors are small.
                    positionSolved = true;
                    break;
                }
            }

            if (Settings.EnableDiagnostics)
            {
                JointUpdateTime = TimeSpan.FromTicks(_watch.ElapsedTicks);
                _watch.Reset();
            }

            // Copy state buffers back to the bodies
            for (int i = 0; i < BodyCount; ++i)
            {
                Body body = Bodies[i];
                body._sweep.C = _positions[i].c;
                body._sweep.A = _positions[i].a;
                body._linearVelocity = _velocities[i].v;
                body._angularVelocity = _velocities[i].w;
                body.SynchronizeTransform();
            }

            Report(_contactSolver._velocityConstraints);

            if (Settings.AllowSleep)
            {
                float minSleepTime = Settings.MaxFloat;

                for (int i = 0; i < BodyCount; ++i)
                {
                    Body b = Bodies[i];

                    if (b.BodyType == BodyType.Static)
                        continue;

                    if (!b.SleepingAllowed || b._angularVelocity * b._angularVelocity > AngTolSqr || Vector2.Dot(b._linearVelocity, b._linearVelocity) > LinTolSqr)
                    {
                        b._sleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }
                    else
                    {
                        b._sleepTime += h;
                        minSleepTime = Math.Min(minSleepTime, b._sleepTime);
                    }
                }

                if (minSleepTime >= Settings.TimeToSleep && positionSolved)
                {
                    for (int i = 0; i < BodyCount; ++i)
                    {
                        Body b = Bodies[i];
                        b.Awake = false;
                    }
                }
            }
        }

        internal void SolveTOI(ref TimeStep subStep, int toiIndexA, int toiIndexB)
        {
            Debug.Assert(toiIndexA < BodyCount);
            Debug.Assert(toiIndexB < BodyCount);

            // Initialize the body state.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];
                _positions[i].c = b._sweep.C;
                _positions[i].a = b._sweep.A;
                _velocities[i].v = b._linearVelocity;
                _velocities[i].w = b._angularVelocity;
            }

            _contactSolver.Reset(ref subStep, ContactCount, _contacts, _positions, _velocities,
                _locks, _contactManager.VelocityConstraintsMultithreadThreshold, _contactManager.PositionConstraintsMultithreadThreshold);

            // Solve position constraints.
            for (int i = 0; i < subStep.positionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB);
                if (contactsOkay)
                {
                    break;
                }
            }

            // Leap of faith to new safe state.
            Bodies[toiIndexA]._sweep.C0 = _positions[toiIndexA].c;
            Bodies[toiIndexA]._sweep.A0 = _positions[toiIndexA].a;
            Bodies[toiIndexB]._sweep.C0 = _positions[toiIndexB].c;
            Bodies[toiIndexB]._sweep.A0 = _positions[toiIndexB].a;

            // No warm starting is needed for TOI events because warm
            // starting impulses were applied in the discrete solver.
            _contactSolver.InitializeVelocityConstraints();

            // Solve velocity constraints.
            for (int i = 0; i < subStep.velocityIterations; ++i)
            {
                _contactSolver.SolveVelocityConstraints();
            }

            // Don't store the TOI contact forces for warm starting
            // because they can be quite large.

            float h = subStep.dt;

            // Integrate positions.
            for (int i = 0; i < BodyCount; ++i)
            {
                Vector2 c = _positions[i].c;
                float a = _positions[i].a;
                Vector2 v = _velocities[i].v;
                float w = _velocities[i].w;

                // Check for large velocities
                Vector2 translation = h * v;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    v *= ratio;
                }

                float rotation = h * w;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    w *= ratio;
                }

                // Integrate
                c += h * v;
                a += h * w;

                _positions[i].c = c;
                _positions[i].a = a;
                _velocities[i].v = v;
                _velocities[i].w = w;

                // Sync bodies
                Body body = Bodies[i];
                body._sweep.C = c;
                body._sweep.A = a;
                body._linearVelocity = v;
                body._angularVelocity = w;
                body.SynchronizeTransform();
            }

            Report(_contactSolver._velocityConstraints);
        }

        public void Add(Body body)
        {
            Debug.Assert(BodyCount < BodyCapacity);
            body.IslandIndex = BodyCount;
            Bodies[BodyCount++] = body;
        }

        public void Add(Contact contact)
        {
            Debug.Assert(ContactCount < ContactCapacity);
            _contacts[ContactCount++] = contact;
        }

        public void Add(Joint joint)
        {
            Debug.Assert(JointCount < JointCapacity);
            _joints[JointCount++] = joint;
        }

        private void Report(ContactVelocityConstraint[] constraints)
        {
            if (_contactManager == null)
                return;

            for (int i = 0; i < ContactCount; ++i)
            {
                Contact c = _contacts[i];

                //FPE optimization: We don't store the impulses and send it to the delegate. We just send the whole contact.
                //FPE feature: added after collision
                var afterCollisionHandlerA = c.FixtureA.AfterCollision;
                if (afterCollisionHandlerA != null)
                    afterCollisionHandlerA(c.FixtureA, c.FixtureB, c, constraints[i]);

                var afterCollisionHandlerB = c.FixtureB.AfterCollision;
                if (afterCollisionHandlerB != null)
                    afterCollisionHandlerB(c.FixtureB, c.FixtureA, c, constraints[i]);

                var postSolveHandler = _contactManager.PostSolve;
                if (postSolveHandler != null)
                    postSolveHandler(c, constraints[i]);
            }
        }
    }
}