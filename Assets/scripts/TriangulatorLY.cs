﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AsImpL.MathUtil;

namespace ObjLoaderLY
{
    /// <summary>
    /// Implements triangulation of a face of a data set.
    /// </summary>
    public static class TriangulatorLY
    {
        /// <summary>
        /// Triangulate a face of the given dataset.
        /// </summary>
        /// <param name="dataSet">Input data set.</param>
        /// <param name="face">Face to be triangulated (with more than 3 vertices)</param>
        public static void Triangulate(DataSetLY dataSet, FaceIndices[] face, SubMeshInfo subMeshInfo)
        {
            int numVerts = face.Length;
            //Debug.LogFormat("Triangulating a face with {0} vertices of {1}...", numVerts, dataSet.CurrGroupName);
            Vector3 planeNormal = FindPlaneNormal(dataSet, face);

            // setup the data structure used for triangluation
            List<Vertex> poly = new List<Vertex>();
            for (int i = 0; i < numVerts; i++)
            {
                int idx = face[i].vertIdx;
                poly.Add(new Vertex(i, dataSet.VertList[idx]));
            }

            // use the ear clipping triangulation
            List<Triangle> newTris = Triangulation.TriangulateByEarClipping(poly, planeNormal, "");

            // copy the data structure used for triangluation back to the data set
            SubMeshInfo.FaceTriangleList newFace = new SubMeshInfo.FaceTriangleList();
            for (int t = 0; t < newTris.Count; t++)
            {
                int idx1 = newTris[t].v1.OriginalIndex;
                int idx2 = newTris[t].v2.OriginalIndex;
                int idx3 = newTris[t].v3.OriginalIndex;
                dataSet.AddFaceIndices(face[idx1]);
                dataSet.AddFaceIndices(face[idx3]);
                dataSet.AddFaceIndices(face[idx2]);
                newFace.AddTriangle(face[idx1], face[idx3], face[idx2]);
            }
            subMeshInfo.SubMeshLists.Add(newFace);
        }

        /// <summary>
        /// Get a normal of a plane used for polygon projection.
        /// </summary>
        /// <param name="dataSet">Input data set.</param>
        /// <param name="face">Face to be triangulated</param>
        /// <returns>The mean of the normals if available or a vector perpendicular to the first triangle</returns>
        public static Vector3 FindPlaneNormal(DataSetLY dataSet, FaceIndices[] face)
        {
            int vertCount = face.Length;
            bool hasNormals = dataSet.NormalList.Count > 0;
            Vector3 planeNormal = Vector3.zero;
            if (hasNormals)
            {
                // if it has normals use the mean of the normals of the vertices
                for (int i = 0; i < vertCount; i++)
                {
                    int normalIdx = face[i].normIdx;
                    planeNormal += dataSet.NormalList[normalIdx];
                }
                planeNormal.Normalize();
            }
            else
            {
                // else calculate a vector perpendicular to the first triangle
                Vector3 v0 = dataSet.VertList[face[0].vertIdx];
                Vector3 v1 = dataSet.VertList[face[1].vertIdx];
                Vector3 vn = dataSet.VertList[face[vertCount - 1].vertIdx];
                planeNormal = MathUtility.ComputeNormal(v0, v1, vn);
            }
            return planeNormal;
        }

    }
}
