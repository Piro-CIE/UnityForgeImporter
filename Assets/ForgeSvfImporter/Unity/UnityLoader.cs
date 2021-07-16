using System;
using System.Collections.Generic;
using UnityEngine;
using PiroCIE.SVF;
using Newtonsoft.Json;
using System.Linq;

namespace PiroCIE.Unity
{
    public class UnityLoader
    {
        public GameObject root;
        private GameObject node1;
        private ISvfContent svfContent;
        private List<string> mergedDbIds;
        private List<GameObject> createdNodes;

        public UnityLoader(ISvfContent _svfContent)
        {
            svfContent = _svfContent;
        }

        public void createScene(int[] _dbIds = null)
        {
            CreateNodes(_dbIds);

            root = new GameObject("root");
            GameObject node0 = new GameObject("node_0");

            string upvectorRaw = svfContent.metadata.metadata["world up vector"].ToString();
            float[] upvector = JsonConvert.DeserializeObject<XYZArray>(upvectorRaw).XYZ;

            if (upvector[2] != 0f)
            { // Z axis
                node0.transform.Rotate(Vector3.right, -90);
                node0.transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            string unitObject = svfContent.metadata.metadata["distance unit"].ToString();
            string unit = (string)JsonConvert.DeserializeObject<ValueObj>(unitObject).value;

            float unitsConvert = convertToMeter(unit);

            if (unitsConvert != 1.0f)
                node0.transform.localScale = new Vector3(node0.transform.localScale.x * unitsConvert, node0.transform.localScale.y * unitsConvert, node0.transform.localScale.z * unitsConvert);

            // Now move to final position
            node0.transform.localPosition = Vector3.zero;


            node1.transform.SetParent(node0.transform, false);
            node0.transform.SetParent(root.transform, true);

            Debug.Log("Scene Created");
        }

        public void CreateNodes(int[] _dbIds)
        {
            node1 = new GameObject("node_1");

            if (mergedDbIds != null)
            {
                mergedDbIds.Clear();
            }
            else
            {
                mergedDbIds = new List<string>();
            }

            if (createdNodes == null)
            {
                createdNodes = new List<GameObject>();
            }
            else
            {
                foreach (var node in createdNodes)
                {
                    UnityEngine.Object.DestroyImmediate(node);
                }
                createdNodes.Clear();
            }

            List<GameObject> objToDestroy = new List<GameObject>();

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            foreach (ISvfFragment frag in svfContent.fragments)
            {
                if (_dbIds != null && _dbIds.Length > 0 && !_dbIds.Contains(frag.dbID))
                {
                    //skip if not in filter dbIDs list
                    continue;
                }

                //check if matrix
                if (frag.transform?.matrix != null)
                {
                    //dont process this case for now !!
                    continue;
                }

                bool t = frag.transform?.t != null;
                bool q = frag.transform?.q != null;
                bool s = frag.transform?.s != null;

                //create node
                GameObject node = new GameObject(frag.dbID.ToString());
                if (t)
                {
                    node.transform.position = new Vector3(
                        (float)frag.transform?.t.x,
                        (float)frag.transform?.t.y,
                        (float)frag.transform?.t.z);
                }
                if (q)
                {
                    node.transform.rotation = new Quaternion(
                        (float)frag.transform?.q.x,
                        (float)frag.transform?.q.y,
                        (float)frag.transform?.q.z,
                        (float)frag.transform?.q.w);
                }
                if (s) node.transform.localScale = (frag.transform?.s != Vector3.zero) ? (Vector3)frag.transform?.s : Vector3.one;



                Mesh mesh = CreateGeometry(frag.geometryID);
                if (mesh != null)
                {
                    MeshFilter meshfilter = node.AddComponent<MeshFilter>();
                    meshfilter.sharedMesh = mesh;



                    Material mat = CreateMaterial(frag.materialID);

                    MeshRenderer meshRenderer = node.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = mat;

                    // ObjectSettings objSettings = node.AddComponent<ObjectSettings>();
                    // objSettings.maxOpacityValue = mat.color.a;
                }

                MeshCollider collider = node.AddComponent<MeshCollider>();

                node.transform.SetParent(node1.transform, true);

                createdNodes.Add(node);

            }

            stopwatch.Stop();

            Debug.Log($"Time to create all nodes from fragments {stopwatch.ElapsedMilliseconds} ms");

            foreach (GameObject node in createdNodes)
            {
                Transform parent = node.transform.parent;
                string str_dbId = node.name.Replace("sub_", "");
                if (mergedDbIds.Contains(str_dbId))
                    continue;

                //fidn all associated nodes to this dbId
                List<GameObject> associatedNodes = new List<GameObject>();
                associatedNodes = createdNodes.FindAll(n => n.name == node.name).ToList();

                GameObject rootNode = new GameObject(node.name);

                mergedDbIds.Add(node.name);

                foreach (var n in associatedNodes)
                {
                    n.transform.SetParent(rootNode.transform, true);
                    n.name = $"sub_{n.name}";
                }

                rootNode.transform.SetParent(parent, false);

                AddProperties(rootNode, int.Parse(str_dbId));

                // Option to merge nodes by dbID 
                // MergeNodes(rootNode);
            }


        }
        public static float convertToMeter(string units)
        {
            if (units == "centimeter" || units == "cm")
                return (0.01f);
            else if (units == "millimeter" || units == "mm")
                return (0.001f);
            else if (units == "foot" || units == "ft")
                return (0.3048f);
            else if (units == "inch" || units == "in")
                return (0.0254f);
            return (1.0f); // "meter" / "m"

        }

        public void MergeNodes(GameObject rootNode)
        {
            MeshFilter[] meshFilters = rootNode.GetComponentsInChildren<MeshFilter>();
            MeshRenderer[] meshRenderers = rootNode.GetComponentsInChildren<MeshRenderer>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            Material[] materials = new Material[meshRenderers.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }

            int j = 0;
            while (j < meshRenderers.Length)
            {
                materials[j] = meshRenderers[j].sharedMaterial;
                j++;
            }

            MeshFilter rootNodeMeshFilter = rootNode.GetComponent<MeshFilter>();

            if (rootNodeMeshFilter == null)
            {
                rootNodeMeshFilter = rootNode.AddComponent<MeshFilter>();
            }

            MeshRenderer rootNodeMeshRenderer = rootNode.GetComponent<MeshRenderer>();

            if (rootNodeMeshRenderer == null)
            {
                rootNodeMeshRenderer = rootNode.AddComponent<MeshRenderer>();
            }

            rootNodeMeshFilter.mesh = new Mesh();
            rootNodeMeshFilter.mesh.CombineMeshes(combine, false);

            rootNodeMeshFilter.mesh.Optimize();
            rootNodeMeshFilter.mesh.RecalculateNormals();
            rootNodeMeshFilter.mesh.RecalculateBounds();


            rootNodeMeshRenderer.materials = materials;

            //Add a mesh collider
            if (rootNode.GetComponent<MeshCollider>() == null)
            {
                rootNode.AddComponent<MeshCollider>();
            }

        }


        public void AddProperties(GameObject node, int dbID)
        {
            var props = svfContent.properties.getPropertiesByCategory(dbID);

            ForgeProperties forgeProps = node.AddComponent<ForgeProperties>();

            forgeProps.properties = props;
        }

        public Mesh CreateGeometry(int id)
        {
            ISvfGeometryMetadata meta = svfContent.geometries[id];
            IMeshPack meshpack = svfContent.meshpacks[meta.packID][meta.entityID];
            if (meshpack != null)
            {

                if (meshpack is ISvfLines)
                {
                    //now we don't want to process these;
                    Debug.Log("Lines not supported yet");
                }
                else if (meshpack is ISvfPoints)
                {
                    //now we don't want to process these;
                    Debug.Log("Points not supported yet");
                }
                else
                {
                    //create a new UnityMesh
                    ISvfMesh _svfMesh = (ISvfMesh)meshpack;

                    //Debug.Log($"SVF Mesh - Indices {_svfMesh.indices}");

                    int[] triangles = new int[_svfMesh.indices.Length];

                    for (int i = 0; i < triangles.Length; i++)
                    {
                        triangles[i] = (int)_svfMesh.indices[i];
                    }

                    Vector3[] vertices = new Vector3[_svfMesh.vertices.Length / 3];
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        vertices[j] = new Vector3(_svfMesh.vertices[j * 3],
                                                _svfMesh.vertices[(j * 3) + 1],
                                                _svfMesh.vertices[(j * 3) + 2]);
                    }

                    Vector3[] normals = new Vector3[_svfMesh.normals.Length / 3];
                    for (int k = 0; k < normals.Length; k++)
                    {
                        normals[k] = new Vector3(_svfMesh.normals[k * 3],
                                                _svfMesh.normals[(k * 3) + 1],
                                                _svfMesh.normals[(k * 3) + 2]);
                    }

                    Vector2[] uvs = new Vector2[0];
                    if(_svfMesh.uvcount > 0) 
                    {
                        //only handle one channel for now
                        uvs = new Vector2[_svfMesh.uvmaps[0].uvs.Length / 2];
                        for (int l = 0; l < uvs.Length; l++)
                        {
                            uvs[l] = new Vector2(_svfMesh.uvmaps[0].uvs[l * 2],
                                                _svfMesh.uvmaps[0].uvs[(l * 2) + 1]);
                        }

                    }

                    Mesh mesh = new Mesh();
                    mesh.vertices = vertices;
                    mesh.triangles = triangles;
                    mesh.normals = normals;
                    mesh.uv = uvs;

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    mesh.RecalculateTangents();


                    return mesh;
                }

            }

            return null;

        }

        // Combine method work well but 

        // private Mesh CreateGeometryFromMultipleIDs(int[] ids, Transform[] transforms)
        // {
        //     List<CombineInstance> totalMesh = new List<CombineInstance>();
        //     for (int i = 0; i < ids.Length; i++)
        //     {
        //         Mesh meshToCombine = CreateGeometry(ids[i]);
        //         if(meshToCombine != null)
        //         {
        //             CombineInstance combine = new CombineInstance();
        //             combine.mesh = meshToCombine;
        //             combine.transform = transforms[i].localToWorldMatrix;

        //             totalMesh.Add(combine);
        //         }

        //     }

        //     Mesh newMesh = new Mesh();     
        //     newMesh.CombineMeshes(totalMesh.ToArray(), false);

        //     // newMesh.RecalculateNormals();
        //     // newMesh.RecalculateBounds();
        //     // newMesh.RecalculateTangents();

        //     return newMesh;
        // }

        //TODO : Use another shader more compatible with mobile (espacially iOS)
        public Material CreateMaterial(int id)
        {
#if UNITY_IOS && !UNITY_EDITOR
        Shader shader = Shader.Find("Standard");
#else
            Shader shader = Shader.Find("Standard (Specular setup)");
            // Shader shader = Shader.Find("Standard");
#endif
            Material newMat = new Material(shader);

            ISvfMaterial svfMat = (ISvfMaterial)svfContent.materials[id];

            if (svfMat.opacity != 1.0f)
            {
                //change material for transparent mat
                newMat.SetFloat("_Mode", 2f); //fade ?
                newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                newMat.SetInt("_ZWrite", 1);
                newMat.DisableKeyword("_ALPHATEST_ON");
                newMat.EnableKeyword("_ALPHABLEND_ON");
                newMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                newMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            //diffuse is the color? 
            if (svfMat.diffuse != null)
            {
                newMat.color = new Color(svfMat.diffuse[0], svfMat.diffuse[1], svfMat.diffuse[2], (float)svfMat.opacity);
                newMat.SetColor("_BaseColor", new Color(svfMat.diffuse[0], svfMat.diffuse[1], svfMat.diffuse[2], (float)svfMat.opacity));
            }


            if (svfMat.specular != null)
            {
                newMat.SetFloat("_WorkflowMode", 0f);
                newMat.SetColor("_SpecColor", new Color(svfMat.specular[0], svfMat.specular[1], svfMat.specular[2], svfMat.specular[3]));
            }

            if ((bool)svfMat.metal)
            {
                //newMat.SetFloat("_WorkflowMode", 1f);
                Debug.Log("Found a metal mat !");
            }

            newMat.SetFloat("_Glossiness", (float)svfMat.glossiness / 255.0f);

            return newMat;

        }

        private Material[] createMaterialsFromMultipleIds(int[] ids)
        {
            List<Material> materials = new List<Material>();
            foreach (int id in ids)
            {
                Material newMat = CreateMaterial(id);
                materials.Add(newMat);
            }

            return materials.ToArray();
        }

    }
}