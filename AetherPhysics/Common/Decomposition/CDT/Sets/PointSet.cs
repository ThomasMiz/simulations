﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using AetherPhysics.Common.Decomposition.CDT.Delaunay;

namespace AetherPhysics.Common.Decomposition.CDT.Sets
{
    internal class PointSet : Triangulatable
    {
        public PointSet(List<TriangulationPoint> points)
        {
            Points = new List<TriangulationPoint>(points);
        }

        #region Triangulatable Members

        public IList<TriangulationPoint> Points { get; private set; }
        public IList<DelaunayTriangle> Triangles { get; private set; }

        public virtual TriangulationMode TriangulationMode
        {
            get { return TriangulationMode.Unconstrained; }
        }

        public void AddTriangle(DelaunayTriangle t)
        {
            Triangles.Add(t);
        }

        public void AddTriangles(IEnumerable<DelaunayTriangle> list)
        {
            foreach (DelaunayTriangle tri in list) Triangles.Add(tri);
        }

        public void ClearTriangles()
        {
            Triangles.Clear();
        }

        public virtual void PrepareTriangulation(TriangulationContext tcx)
        {
            if (Triangles == null)
            {
                Triangles = new List<DelaunayTriangle>(Points.Count);
            }
            else
            {
                Triangles.Clear();
            }
            tcx.Points.AddRange(Points);
        }

        #endregion
    }
}