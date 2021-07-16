// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using PiroCIE.IMF;

namespace PiroCIE.SVF
{

    public class Scene : IScene
    {

        private ISvfContent svf;

        public Scene(ISvfContent _svf)
        {
            svf = _svf;

        }

        public override IMetadata getMetadata()
        {
            return new IMetadata() { key = this.svf.metadata.metadata };
        }

        public override int getNodeCount()
        {
            return this.svf.fragments.Length;
        }

        public override INode getNode(int id)
        {
            ISvfFragment frag = this.svf.fragments[id];
            IObjectNode node = new IObjectNode()
            {
                kind = NodeKind.Object,
                dbid = frag.dbID,
                geometry = frag.geometryID,
                material = frag.materialID
            };

            if (frag.transform != null)
            {
                if (frag.transform?.matrix != null)
                {
                    double[] matrix = frag.transform?.matrix;
                    Vector3 t = (Vector3)frag.transform?.t;
                    node.transform = new IMatrixTransform()
                    {
                        kind = TransformKind.Matrix,
                        elements = new List<double>()
                        {
                            matrix[0], matrix[3], matrix[6], 0,
                            matrix[1], matrix[4], matrix[7], 0,
                            matrix[2], matrix[5], matrix[8], 0,
                            t.x,t.y,t.z,1
                        }
                    };
                }
                else
                {
                    node.transform = new IDecomposedTransform()
                    {
                        rotation = new IQuat() { 
                            x = (float)frag.transform?.q.x,
                            y = (float)frag.transform?.q.y,
                            z = (float)frag.transform?.q.z,
                            w = (float)frag.transform?.q.w
                        },
                        scale = new IVec3() { 
                            x = (float)frag.transform?.s.x,
                            y = (float)frag.transform?.s.y,
                            z = (float)frag.transform?.s.z
                        },
                        translation = new IVec3() { 
                            x = (float)frag.transform?.t.x,
                            y = (float)frag.transform?.t.y,
                            z = (float)frag.transform?.t.z
                        }
                };
                    // if (frag.transform?.q != null)
                    // {
                    //     node.transform.rotation = frag.transform.q;
                    // }

                    // if (frag.transform?.s != null)
                    // {
                    //     node.transform.scale = frag.transform.s;
                    // }

                    // if (frag.transform?.t != null)
                    // {
                    //     node.transform.translation = frag.transform.t;
                    // }
                }
            }
            return node;
        }

        public override int getGeometryCount()
        {
            return this.svf.geometries.Length;
        }

        public override IGeometry getGeometry(int id)
        {
            ISvfGeometryMetadata meta = this.svf.geometries[id];
            IMeshPack mesh = this.svf.meshpacks[meta.packID][meta.entityID];
            if (mesh != null)
            {
                if (mesh is ISvfLines)
                {
                    ISvfLines svfLines = (ISvfLines)mesh;
                    ILineGeometry geom = new ILineGeometry()
                    {
                        kind = IMF.GeometryKind.Lines,
                        getIndices = () => svfLines.indices,
                        getVertices = () => svfLines.vertices,
                        getColors = () => svfLines.colors
                    };
                    return geom;
                }
                else if (mesh is ISvfPoints)
                {
                    ISvfPoints svfPoints = (ISvfPoints)mesh;
                    IPointGeometry geom = new IPointGeometry()
                    {
                        kind = IMF.GeometryKind.Points,
                        getVertices = () => svfPoints.vertices,
                        getColors = () => svfPoints.colors
                    };
                    return geom;
                }
                else
                {
                    ISvfMesh svfMesh = (ISvfMesh)mesh;
                    IMeshGeometry geom = new IMeshGeometry()
                    {
                        kind = IMF.GeometryKind.Mesh,
                        getIndices = () => svfMesh.indices,
                        getVertices = () => svfMesh.vertices,
                        getNormals = () => svfMesh.normals,
                        getColors = () => svfMesh.colors,
                        getUvChannelCount = () => svfMesh.uvcount,
                        getUvs = (channel) => svfMesh.uvmaps[channel].uvs
                    };
                    return geom;
                }
            }
            return new IEmptyGeometry();
        }

        public override int getMaterialCount()
        {
            return this.svf.materials.Length;
        }

        public override IMaterial getMaterial(int id)
        {
            ISvfMaterial? _mat = this.svf.materials[id];
            IPhysicalMaterial mat = new IPhysicalMaterial()
            {
                kind = IMF.MaterialKind.Physical,
                diffuse = new IVec3() { x = 0, y = 0, z = 0 },
                metallic = (bool)_mat?.metal ? 1.0f : 0.0f,
                opacity = _mat?.opacity ?? 1.0f,
                roughness = (_mat?.glossiness != null) ? (1.0f - _mat?.glossiness / 255.0f) : 1.0f, // TODO: how to map glossiness to roughness properly?
                scale = new IVec2()
                {
                    x = _mat?.maps?.diffuse?.scale.texture_UScale ?? 1.0f,
                    y = _mat?.maps?.diffuse?.scale.texture_VScale ?? 1.0f
                }
            };
            if (_mat?.diffuse != null)
            {
                mat.diffuse = new IVec3()
                {
                    x = (float)_mat?.diffuse[0],
                    y = (float)_mat?.diffuse[1],
                    z = (float)_mat?.diffuse[2]
                };
            }
            if (_mat?.maps?.diffuse != null)
            {
                mat.maps = mat.maps ?? null;
                mat.maps = new IMaps() { diffuse = _mat?.maps?.diffuse?.uri };
            }
            return mat;
        }

        public override byte[] getImage(string uri)
        {
            return this.svf.images[uri];
        }
    }

    public class Reader
    {
        public GameObject root;
        ISvfRoot svf;
        Func<string, byte[]> resolve;

        public Reader(string svfPath, Func<string, byte[]> _resolve)
        {
            resolve = _resolve;

            var zip = new ZipFile(svfPath);
            var _embedded = new Dictionary<string, byte[]>();

            foreach (ZipEntry zipEntry in zip)
            {
                if (zipEntry.Name == "manifest.json")
                {
                    using (var s = zip.GetInputStream(zipEntry))
                    {
                        StreamReader reader = new StreamReader(s);
                        string text = reader.ReadToEnd();
                        svf.manifest = JsonConvert.DeserializeObject<ISvfManifest>(text);

                        continue;
                    }
                }

                if (zipEntry.Name == "metadata.json")
                {
                    using (var s = zip.GetInputStream(zipEntry))
                    {

                        StreamReader reader = new StreamReader(s);
                        string text = reader.ReadToEnd();
                        svf.metadata = JsonConvert.DeserializeObject<ISvfMetadata>(text);

                        continue;
                        //Debug.Log(metadata);
                    }
                }

                using (var s = zip.GetInputStream(zipEntry))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        s.CopyTo(ms);
                        _embedded[zipEntry.Name] = ms.ToArray();
                    }
                }

            }

            svf.embedded = _embedded;

        }

        public ISvfContent read()
        {

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            Task[] taskList = new Task[6];

            ISvfFragment[] fragments = new ISvfFragment[0];
            taskList[0] = Task.Factory.StartNew(() => { fragments = this.readFragments(); });

            ISvfGeometryMetadata[] geometries = new ISvfGeometryMetadata[0];
            taskList[1] = Task.Factory.StartNew(() => { geometries = this.readGeometries(); });

            ISvfMaterial?[] materials = new ISvfMaterial?[0];
            taskList[2] = Task.Factory.StartNew(() => { materials = this.readMaterials(); });


            PropDbReader properties = new PropDbReader();
            taskList[3] = Task.Factory.StartNew(() => { properties = this.getPropertyDb(); });

            int meshPackCount = 0;
            IMeshPack[][] meshPacks = new IMeshPack[0][];

            taskList[4] = Task.Factory.StartNew(() =>
            {
                meshPackCount = this.getMeshPackCount();

                meshPacks = new IMeshPack[meshPackCount][];

                Parallel.For(0, meshPackCount, i =>
                {
                    meshPacks[i] = this.readMeshPack(i);
                });

            });

            Dictionary<string, byte[]> images = new Dictionary<string, byte[]>();
            taskList[5] = Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(this.listImages(), uri =>
                {
                    string normalizedUri = String.Join(Path.DirectorySeparatorChar.ToString(), uri.ToLower().Split('[', '\\', '/', ']'));
                    byte[] imageData = null;
                    try
                    {
                        imageData = this.getAsset(uri);
                    }
                    catch (System.Exception) { }

                    if (imageData == null)
                    {
                        Debug.Log($"Could not find image {uri}, trying a lower-cased version of the URI...");

                        try
                        {
                            imageData = this.getAsset(uri.ToLower());
                        }
                        catch (System.Exception) { }

                    }

                    if (imageData == null)
                    {
                        Debug.Log($"Still could not find image {uri}; will default to a single black pixel placeholder image...");
                    }

                    images[normalizedUri] = imageData;
                });

            });

            Task.WaitAll(taskList);

            s.Stop();
            Debug.Log($"Reading files time : {s.ElapsedMilliseconds} ms");

            ISvfContent svfContent = new ISvfContent()
            {
                metadata = this.getMetadata(),
                fragments = fragments,
                geometries = geometries,
                meshpacks = meshPacks,
                materials = materials,
                properties = properties,
                images = images
            };

            // return new Scene(svfContent);

            //Return the svfContent instead of the IMF Format
            return svfContent;

        }

        /// <summary>
        /// Retrieves raw binary data of a specific SVF asset.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private byte[] getAsset(string uri)
        {
            return resolve(uri);
        }

        /// <summary>
        /// Retrieves parsed SVF metadata.
        /// </summary>
        /// <returns></returns>
        private ISvfMetadata getMetadata()
        {
            return svf.metadata;
        }

        /// <summary>
        /// Retrieves parsed SVF manifest.
        /// </summary>
        /// <returns></returns>
        private ISvfManifest getManifest()
        {
            return svf.manifest;
        }

        protected ISvfManifestAsset? findAsset(AssetType _type)
        {
            return this.svf.manifest.assets.Where(asset => asset.type == _type).First();
        }

        protected ISvfManifestAsset? findAsset(string uri)
        {
            return this.svf.manifest.assets.Where(asset => asset.URI == uri).First();
        }

        protected ISvfManifestAsset? findAsset(AssetType _type, string uri)
        {
            return this.svf.manifest.assets.Where(asset => asset.type == _type).Where(asset => asset.URI == uri).First();
        }


        /// <summary>
        /// Retrieves, parses, and collects all SVF fragments.
        /// </summary>
        /// <returns>List of parsed fragments.</returns>
        protected ISvfFragment[] readFragments()
        {
            ISvfManifestAsset? fragmentAsset = this.findAsset(AssetType.FragmentList);
            if (fragmentAsset == null)
            {
                throw new Exception("Fragment list not found.");
            }
            byte[] buffer = this.getAsset(fragmentAsset?.URI);
            return Fragments.parseFragments(buffer);
        }


        /// <summary>
        /// Retrieves, parses, and collects all SVF geometry metadata.
        /// </summary>
        /// <returns>List of parsed geometry metadata.</returns>
        protected ISvfGeometryMetadata[] readGeometries()
        {
            ISvfManifestAsset? geometryAsset = this.findAsset(AssetType.GeometryMetadataList);
            if (geometryAsset == null)
            {
                throw new Exception("Geometry metadata not found.");
            }
            byte[] buffer = this.getAsset(geometryAsset?.URI);
            return Geometries.parseGeometries(buffer);
        }


        /// <summary>
        /// Retrieves, parses, and collects all SVF geometry metadata.
        /// </summary>
        /// <returns>List of parsed geometry metadata.</returns>
        protected ISvfMaterial?[] readMaterials()
        {
            ISvfManifestAsset? materialAsset = this.findAsset(AssetType.ProteinMaterials, "Materials.json.gz");
            if (materialAsset == null)
            {
                throw new Exception("Materials not found.");
            }
            byte[] buffer = this.getAsset(materialAsset?.URI);
            return Materials.parseMaterials(buffer);
        }

        /// <summary>
        /// Gets the number of available mesh packs.
        /// </summary>
        /// <returns></returns>
        protected int getMeshPackCount()
        {
            int count = 0;
            Regex rg = new Regex(@"^\d+\.pf$");
            foreach (ISvfManifestAsset asset in this.svf.manifest.assets)
            {
                if (asset.type == AssetType.PackFile && rg.IsMatch(asset.URI))
                {
                    count++;
                }
            }
            return count;
        }


        /// <summary>
        /// Retrieves, parses, and collects all meshes, lines, or points in a specific SVF meshpack.
        /// </summary>
        /// <param name="packNumber"></param>
        /// <returns>List of parsed meshes, lines, or points (or null values for unsupported mesh types).</returns>
        /// <remarks>IMeshPack is a generic type for ISvfMesh, ISvfLines and IsvfPoints <remarks>
        protected IMeshPack[] readMeshPack(int packNumber)
        {
            ISvfManifestAsset? meshPackAsset = this.findAsset(AssetType.PackFile, $"{packNumber}.pf");
            if (meshPackAsset == null)
            {
                throw new Exception($"Mesh packfile {packNumber}.pf not found.");
            }
            byte[] buffer = this.getAsset(meshPackAsset?.URI);
            return Meshes.parseMeshes(buffer);
        }


        /**
         * 
         * These can then be retrieved using {@link getAsset}.
         * @returns {string[]} Image asset URIs.
         */
        /// <summary>
        /// Finds URIs of all image assets referenced in the SVF.
        /// These can then be retrieved using <c>getAsset()</c>.
        /// </summary>
        /// <returns></returns>
        protected string[] listImages()
        {
            return this.svf.manifest.assets
                .Where(asset => asset.type == AssetType.Image)
                .Select(asset => asset.URI)
                .ToArray<string>();
        }

        protected PropDbReader getPropertyDb()
        {
            ISvfManifestAsset? idsAsset = this.findAsset(AssetType.PropertyIDs);
            ISvfManifestAsset? offsetsAsset = this.findAsset(AssetType.PropertyOffsets);
            ISvfManifestAsset? avsAsset = this.findAsset(AssetType.PropertyAVs);
            ISvfManifestAsset? attrsAsset = this.findAsset(AssetType.PropertyAttributes);
            ISvfManifestAsset? valsAsset = this.findAsset(AssetType.PropertyValues);

            if (idsAsset == null || offsetsAsset == null || avsAsset == null || attrsAsset == null || valsAsset == null)
            {
                throw new Exception("Could not parse property database. Some of the database assets are missing.");
            }

            byte[][] buffers = new byte[5][];

            buffers[0] = this.getAsset(idsAsset?.URI);
            buffers[1] = this.getAsset(offsetsAsset?.URI);
            buffers[2] = this.getAsset(avsAsset?.URI);
            buffers[3] = this.getAsset(attrsAsset?.URI);
            buffers[4] = this.getAsset(valsAsset?.URI);

            return new PropDbReader(buffers[0], buffers[1], buffers[2], buffers[3], buffers[4]);

        }
    }



}
