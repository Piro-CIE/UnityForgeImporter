// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PiroCIE.Utils;

namespace PiroCIE.SVF
{    
    public class Meshes
    {
        /// <summary>
        /// Parses meshes from a binary buffer, typically stored in files called '<number>.pf',
        /// referenced in the SVF manifest as an asset of type 'Autodesk.CloudPlatform.PackFile'.
        /// </summary>
        /// <param name="buffer">Binary buffer to parse.</param>
        /// <returns>Instances of parsed meshes, or null values if the mesh cannot be parsed (and to maintain the indices used in <c>IGeometry</c>).
        /// </returns>
        public static IMeshPack[] parseMeshes(byte[] buffer)
        {
            List<IMeshPack> meshPacks = new List<IMeshPack>();
            PackFileReader pfr = new PackFileReader(buffer);

            for (int i = 0, len = pfr.numEntries(); i < len; i++)
            {
                ISvfManifestType? entry = pfr.seekEntry(i);
                Debug.Assert(entry != null);
                Debug.Assert(entry?.version >= 1);

                switch (entry?.type)
                {
                    case "Autodesk.CloudPlatform.OpenCTM":
                        meshPacks.Add(parseMeshOCTM(pfr));
                        break;
                    case "Autodesk.CloudPlatform.Lines":
                        meshPacks.Add(parseLines(pfr, (int)entry?.version));
                        break;
                    case "Autodesk.CloudPlatform.Points":
                        meshPacks.Add(parsePoints(pfr, (int)entry?.version));
                        break;
                }
            }

            return meshPacks.ToArray();

        }

        private static ISvfMesh? parseMeshOCTM(PackFileReader pfr)
        {
            string fourcc = pfr.getString(4);
            Debug.Assert(fourcc == "OCTM");

            int version = pfr.getInt32();
            Debug.Assert(version == 5);

            string method = pfr.getString(3);
            pfr.getUint8(); // Read the last 0 char of the RAW or MG2 fourCC

            switch (method)
            {
                case "RAW":
                    return parseMeshRaw(pfr);
                default:
                    Debug.LogWarning("Unsupported OpenCTM method " + method);
                    return null;
            }
        }

        private static ISvfMesh parseMeshRaw(PackFileReader pfr)
        {
            // We will create a single ArrayBuffer to back both the vertex and index buffers.
            // The indices will be places after the vertex information, because we need alignment of 4 bytes.

            int vcount = pfr.getInt32(); // Num of vertices
            int tcount = pfr.getInt32(); // Num of triangles
            int uvcount = pfr.getInt32(); // Num of UV maps
            int attrs = pfr.getInt32(); // Number of custom attributes per vertex
            int flags = pfr.getInt32(); // Additional flags (e.g., whether normals are present)
            string comment = pfr.getString(pfr.getInt32());

            // Indices
            string name = pfr.getString(4);
            Debug.Assert(name == "INDX");
            uint[] indices = new uint[tcount * 3];
            for (int i = 0; i < tcount * 3; i++)
            {
                indices[i] = pfr.getUint32();
            }

            // Vertices
            name = pfr.getString(4);
            Debug.Assert(name == "VERT");
            float[] vertices = new float[vcount * 3];
            Vector3 min = new Vector3() { x = float.MaxValue, y = float.MaxValue, z =float.MaxValue };
            Vector3 max = new Vector3() { x = float.MinValue, y = float.MinValue, z = float.MinValue };
            for (int i = 0; i < vcount * 3; i += 3)
            {
                float x = vertices[i] = pfr.getFloat32();
                float y = vertices[i + 1] = pfr.getFloat32();
                float z = vertices[i + 2] = pfr.getFloat32();

                min.x = Math.Min(min.x, x);
                max.x = Math.Max(max.x, x);
                min.y = Math.Min(min.y, y);
                max.y = Math.Max(max.y, y);
                min.z = Math.Min(min.z, z);
                max.z = Math.Max(max.z, z);
            }


            // Normals
            float[] normals = null;
            if ((flags & 1) != 0)
            {
                name = pfr.getString(4);
                Debug.Assert(name == "NORM");
                normals = new float[vcount * 3];
                for (int i = 0; i < vcount; i++)
                {
                    float x = pfr.getFloat32();
                    float y = pfr.getFloat32();
                    float z = pfr.getFloat32();
                    // Make sure the normals have unit length
                    float dot = x * x + y * y + z * z;
                    if (dot != 1.0)
                    {
                        float len = (float)Math.Sqrt(dot);
                        x /= len;
                        y /= len;
                        z /= len;
                    }
                    normals[i * 3] = x;
                    normals[i * 3 + 1] = y;
                    normals[i * 3 + 2] = z;
                }
            }


            // Parse zero or more UV maps
            ISvfUVMap[] uvmaps = new ISvfUVMap[uvcount];
            for (int i = 0; i < uvcount; i++)
            {
                name = pfr.getString(4);
                Debug.Assert(name == "TEXC");

                ISvfUVMap uvmap = new ISvfUVMap()
                {
                    name = pfr.getString(pfr.getInt32()),
                    file = pfr.getString(pfr.getInt32()),
                    uvs = new float[vcount * 2]
                };
                for (int j = 0; j < vcount; j++)
                {
                    uvmap.uvs[j * 2] = pfr.getFloat32();
                    uvmap.uvs[j * 2 + 1] = 1.0f - pfr.getFloat32();
                }
                uvmaps[i] = uvmap;
            }


            // Parse custom attributes (currently we only support "Color" attrs)
            float[] colors = null;
            if (attrs > 0)
            {
                name = pfr.getString(4);
                Debug.Assert(name == "ATTR");
                for (int i = 0; i < attrs; i++)
                {
                    string attrName = pfr.getString(pfr.getInt32());
                    if (attrName == "Color")
                    {
                        colors = new float[vcount * 4];
                        for (int j = 0; j < vcount; j++)
                        {
                            colors[j * 4] = pfr.getFloat32();
                            colors[j * 4 + 1] = pfr.getFloat32();
                            colors[j * 4 + 2] = pfr.getFloat32();
                            colors[j * 4 + 3] = pfr.getFloat32();
                        }
                    }
                    else
                    {
                        pfr.seek(pfr.offset + vcount * 4);
                    }
                }
            }


            ISvfMesh mesh = new ISvfMesh()
            {
                vcount = vcount,
                tcount = tcount,
                uvcount = uvcount,
                attrs = attrs,
                flags = flags,
                comment = comment,
                uvmaps = uvmaps.ToList(),
                indices = indices,
                vertices = vertices,
                min = min,
                max = max
            };

            if (normals != null)
            {
                mesh.normals = normals;
            }
            if (colors != null)
            {
                mesh.colors = colors;
            }
            return mesh;

        }

        private static float checkValue(float inputValue) {
            if(Mathf.Abs(inputValue) > 0 && Mathf.Abs(inputValue) < 0.00000001f)
            {
                Debug.Log("Value replaced");
                return 0f;
            }
            return inputValue;
        }

        private static ISvfLines parseLines(PackFileReader pfr, int entryVersion)
        {
            Debug.Assert(entryVersion >= 2);

            ushort vertexCount = pfr.getUint16();
            ushort indexCount = pfr.getUint16();
            ushort boundsCount = pfr.getUint16(); // Ignoring for now
            float lineWidth = (entryVersion > 2) ? pfr.getFloat32() : 1.0f;
            bool hasColors = pfr.getUint8() != 0;

            ISvfLines lines = new ISvfLines()
            {
                isLines = true,
                vcount = vertexCount,
                lcount = indexCount / 2,
                vertices = new float[vertexCount * 3],
                indices = new ushort[indexCount],
                lineWidth = lineWidth
            };

            // Parse vertices
            for (int i = 0, len = vertexCount * 3; i < len; i++)
            {
                lines.vertices[i] = pfr.getFloat32();
            }

            // Parse colors
            if (hasColors)
            {
                lines.colors = new float[vertexCount * 3];
                for (int i = 0, len = vertexCount * 3; i < len; i++)
                {
                    lines.colors[i] = pfr.getFloat32();
                }
            }

            // Parse indices
            for (int i = 0, len = indexCount; i < len; i++)
            {
                lines.indices[i] = pfr.getUint16();
            }

            // TODO: Parse polyline bounds

            return lines;
        }

        private static ISvfPoints parsePoints(PackFileReader pfr, int entryVersion){
            Debug.Assert(entryVersion >= 2);

            ushort vertexCount = pfr.getUint16();
            ushort indexCount = pfr.getUint16();
            float pointSize = pfr.getFloat32();
            bool hasColors = pfr.getUint8() != 0;
            ISvfPoints points = new ISvfPoints(){
                isPoints = true,
                vcount = vertexCount,
                vertices = new float[vertexCount * 3],
                pointSize = pointSize
            };

            // Parse vertices
            for (int i = 0, len = vertexCount * 3; i < len; i++) {
                points.vertices[i] = pfr.getFloat32();
            }

            // Parse colors
            if (hasColors) {
                points.colors = new float[vertexCount * 3];
                for (int i = 0, len = vertexCount * 3; i < len; i++) {
                    points.colors[i] = pfr.getFloat32();
                }
            }

            return points;
        }

    }
}

