using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class ForceDefaultConverter : JsonConverter
{
    public override bool CanRead => false;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

[JsonConverter(typeof(ForceDefaultConverter))]
public enum AssetType
{
    [EnumMember(Value = "Autodesk.CloudPlatform.Image")]
    Image,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyViewables")]
    PropertyViewables,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyOffsets")]
    PropertyOffsets,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyAttributes")]
    PropertyAttributes,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyValues")]
    PropertyValues,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyIDs")]
    PropertyIDs,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyAVs")]
    PropertyAVs,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyRCVs")]
    PropertyRCVs,
    [EnumMember(Value = "ProteinMaterials")]
    ProteinMaterials,
    [EnumMember(Value = "Autodesk.CloudPlatform.PackFile")]
    PackFile,
    [EnumMember(Value = "Autodesk.CloudPlatform.FragmentList")]
    FragmentList,
    [EnumMember(Value = "Autodesk.CloudPlatform.GeometryMetadataList")]
    GeometryMetadataList,
    [EnumMember(Value = "Autodesk.CloudPlatform.InstanceTree")]
    InstanceTree,
    //Autodesk.CloudPlatform.ViewingMetadata
    [EnumMember(Value = "Autodesk.CloudPlatform.ViewingMetadata")]
    ViewingMetadata
}

public struct ISvfContent {
    // metadata SVF.ISvfMetadata;
    public ISvfMetadata metadata {get; set; }
    // fragments: SVF.IFragment[];
    public ISvfFragment[] fragments {get; set; }
    // geometries: SVF.IGeometryMetadata[];
    public ISvfGeometryMetadata[] geometries {get; set; }
    // meshpacks: (SVF.IMesh | SVF.ILines | SVF.IPoints | null)[][];
    public IMeshPack[][] meshpacks {get; set; }
    // materials: (SVF.IMaterial | null)[];
    public ISvfMaterial?[] materials {get; set;}
    // properties: PropDbReader;
    public PropDbReader properties {get; set; }
    // images: { [uri: string]: Buffer };
    public Dictionary<string, byte[]> images {get; set;}
}

/**
 * Parsed content of an actual *.svf file.
 */
public struct ISvfRoot {
    //manifest: ISvfManifest;
    public ISvfManifest manifest { get; set; }

    //metadata: ISvfMetadata;
    public ISvfMetadata metadata { get; set; }

    //embedded: { [key: string]: Buffer };
    public Dictionary<string, byte[]> embedded { get; set; }
}

/**
 * Top-level manifest containing URIs and types of all assets
 * referenced by or embedded in a specific SVF file.
 * The URIs are typically relative to the SVF file itself.
 */
public struct ISvfManifest
{
    // name: string;
    [JsonProperty("name")]
    public string name { get; set; }

    // manifestversion: number;
    [JsonProperty("manifestversion")]
    public int manifestversion { get; set; }

    // toolkitversion: string;
    [JsonProperty("toolkitversion")]
    public string toolkitversion { get; set; }

    // assets: ISvfManifestAsset[];
    [JsonProperty("assets")]
    public List<ISvfManifestAsset> assets { get; set; }

    // typesets: ISvfManifestTypeSet[];
    [JsonProperty("typesets")]
    public List<ISvfManifestTypeSet> typesets { get; set; }

}

/**
 * Description of a specific asset referenced by or embedded in an SVF,
 * including the URI, compressed and uncompressed size, type of the asset itself,
 * and types of all entities inside the asset.
 */
public struct ISvfManifestAsset
{
    // id: string;
    [JsonProperty("id")]
    public string id { get; set; }

    // type: AssetType;
    [JsonProperty("type")]
    public AssetType type { get; set; }

    // typeset?: string;
    [JsonProperty("typeset")]
    public string typeset { get; set; }

    // URI: string;
    [JsonProperty("URI")]
    public string URI { get; set; }

    // size: number;
    [JsonProperty("size")]
    public int size { get; set; }

    // usize: number;
    [JsonProperty("usize")]
    public int usize { get; set; }
}

/**
 * Collection of type definitions.
 */
public struct ISvfManifestTypeSet
{
    //id: string;
    [JsonProperty("id")]
    public string id { get; set; }

    //types: ISvfManifestType[];
    [JsonProperty("types")]
    public List<ISvfManifestType> types { get; set; } 
}

/**
 * Single type definition.
 */
public struct ISvfManifestType {
    //class: string;
    [JsonProperty("class")]
    public string typeClass { get; set; }

    //type: string;
    [JsonProperty("type")]
    public string type { get; set; }

    //version: number;
    [JsonProperty("version")]
    public int version { get; set; }
}

/**
 * Additional metadata for SVF such as the definition of "up" vector,
 * default background, etc.
 */
public struct ISvfMetadata {
    //version: string;
    [JsonProperty("version")]
    public string version { get; set; }

    //metadata: { [key: string]: any };
    [JsonProperty("metadata")]
    public Dictionary<string, object> metadata { get; set; }
}


/**
 * Fragment represents a single scene object,
 * linking together material, geometry, and database IDs,
 * and providing world transform and bounding box on top of that.
 */
public struct ISvfFragment {
    // visible: boolean;
    public bool visible { get; set; }
    // materialID: number;
    public int materialID { get; set; }
    // geometryID: number;
    public int geometryID { get; set; }
    // dbID: number;
    public int dbID { get; set; }
    // transform: Transform | null;
    public ISvfTransform? transform { get; set; }
    // bbox: number[];
    public float[] bbox { get; set; }
}

/**
 * Lightweight data structure pointing to a mesh in a specific packfile and entry.
 * Contains additional information about the type of mesh and its primitive count.
 */
public struct ISvfGeometryMetadata {
    // fragType: number;
    public byte fragType {get; set;}
    // primCount: number;
    public ushort primCount {get; set;}
    // packID: number;
    public int packID {get; set;}
    // entityID: number;
    public int entityID {get; set;}
    // topoID?: number;
    public int topoID {get; set;}
}

public struct ISvfMaterials {
    // name: string;
    public string name {get; set; }
    // version: string;
    public string version {get; set; }
    // scene: { [key: string]: any };
    public Dictionary<string, string> scene {get; set; } //scene type ???
    // materials: { [key: string]: IMaterialGroup };
    public Dictionary<string, ISvfMaterialGroup> materials {get; set; }
}

public struct ISvfMaterialGroup {
    // version: number;
    public int version {get; set; }
    // userassets: string[];
    public string[] userassets {get; set; }
    // materials: { [key: string]: IMaterial };
    public Dictionary<string, ISvfMaterial> materials {get; set; }
}


public struct ISvfMaterial {
    // diffuse?: number[];
    public float[] diffuse {get; set;}
    // specular?: number[];
    public float[] specular {get; set;}
    // ambient?: number[];
    public float[] ambient {get; set;}
    // emissive?: number[];
    public float[] emissive {get; set;}
    // glossiness?: number;
    public float glossiness {get; set;}
    // reflectivity?: number;
    public float reflectivity {get; set;}
    // opacity?: number;
    public float opacity {get; set;}
    // metal?: boolean;
    public bool metal { get; set;}

    public ISvfMaterialMaps maps { get; set;}

    //second part of the material defined in the material class

    // tag: string;
    public string tag {get; set; }
    // proteinType: string;
    public string proteinType {get; set; }
    // definition: string;
    public string definition {get; set; }
    // transparent: boolean;
    public bool transparent {get; set;}
    // keywords?: string[];
    public string[] keywords {get; set;}
    // categories?: string[];
    public string[] categories {get; set; }
    // properties: {}
    public ISvfMaterialProperties properties {get; set;}
    // textures?: { [key: string]: { connections: string[] }; };
    public Dictionary<string, StringConnections> textures {get; set; }
}

public struct ISvfMaterialProperties {
    // integers?: { [key: string]: number; };
    public Dictionary<string, int> integers {get; set; }
    // booleans?: { [key: string]: boolean; };
    public Dictionary<string, bool> booleans {get; set; }
    // strings?: { [key: string]: { values: string[] }; };
    public Dictionary<string, StringValues> strings {get; set ;}
    // uris?: { [key: string]: { values: string[] }; };
    public Dictionary<string, StringValues> uris {get; set; }
    // scalars?: { [key: string]: { units: string; values: number[] }; };
    public Dictionary<string, UnitsAndNumberValues> scalars {get; set; }
    // colors?: { [key: string]: { values: { r: number; g: number; b: number; a: number; }[]; connections?: string[]; }; };
    public Dictionary<string, RGBValuesAndStringConnections> colors {get; set; }
    // choicelists?: { [key: string]: { values: number[] }; };
    public Dictionary<string, IntValues> choicelists {get; set; }
    // uuids?: { [key: string]: { values: number[] }; };
    public Dictionary<string, IntValues> uuids {get; set; } 
    // references?: any; // TODO
    public string references {get; set; } //type TODO

}

public struct StringValues {
    public string[] values {get; set;}
}


public struct IntValues {
    public int[] values {get; set;}
}

public struct UnitsAndNumberValues {
    public string units {get; set; }
    public float[] values {get; set;}
}

public struct RGBValuesAndStringConnections {
    public List<RGBA> values {get; set; }
    public string[] connections {get; set;}
}

public struct RGBA {
    public float r {get; set; }
    public float g {get; set; }
    public float b {get; set; }
    public float a {get; set; }
}

public struct StringConnections {
    public string[] connections {get; set; }
}

public struct ISvfMaterialMaps {

    // maps?: {
    //     diffuse?: IMaterialMap;
    //     specular?: IMaterialMap;
    //     normal?: IMaterialMap;
    //     bump?: IMaterialMap;
    //     alpha?: IMaterialMap;
    // };
    public ISvfMaterialMap? diffuse {get; set;}
    public ISvfMaterialMap? specular {get; set;}
    public ISvfMaterialMap? normal {get; set;}
    public ISvfMaterialMap? bump {get; set;}
    public ISvfMaterialMap? alpha {get; set;}
}

public struct ISvfMaterialMap {
    // uri: string;
    public string uri { get; set;}

    public ISvfMaterialMapScale scale {get; set;}
}

public struct ISvfMaterialMapScale 
{
    public float texture_UScale {get; set;}
    public float texture_VScale {get; set;}
    // scale: {
    //     texture_UScale: number ,
    //     texture_VScale: number
    // }
}

public interface IMeshPack {}

/**
 * Triangular mesh data, including indices, vertices, optional normals and UVs.
 */
public struct ISvfMesh : IMeshPack {
    // vcount: number; // Num of vertices
    public int vcount { get; set; }
    // tcount: number; // Num of triangles
    public int tcount { get; set; }
    // uvcount: number; // Num of UV maps
    public int uvcount { get; set; }
    // attrs: number; // Number of attributes per vertex
    public int attrs { get; set; }
    // flags: number;
    public int flags {get; set; }
    // comment: string;
    public string comment {get; set; }
    // uvmaps: IUVMap[];
    public List<ISvfUVMap> uvmaps {get; set; }
    // indices: Uint16Array;
    public uint[] indices {get; set; }
    // vertices: Float32Array;
    public float[] vertices {get; set; }
    // normals?: Float32Array;
    public float[] normals {get; set; }
    // colors?: Float32Array;
    public float[] colors {get; set; }
    // min: IVector3;
    public Vector3 min {get; set; }
    // max: IVector3;
    public Vector3 max {get; set; }
}

/**
 * Line segment data.
 */
public struct ISvfLines : IMeshPack {
    // isLines: true;
    public bool isLines {get; set; }
    // vcount: number; // Number of vertices
    public int vcount { get; set; }
    // lcount: number; // Number of line segments
    public int lcount { get; set; }
    // vertices: Float32Array; // Vertex buffer (of length vcount*3)
    public float[] vertices {get; set; }
    // indices: Uint16Array; // Index buffer (of length lcount*2)
    public ushort[] indices {get; set; }
    // colors?: Float32Array; // Optional color buffer (of length vcount*3)
    public float[] colors {get; set; }
    // lineWidth: number;
    public float lineWidth {get; set; }
}

/**
 * Point cloud data.
 */
public struct ISvfPoints : IMeshPack {
    // isPoints: true;
    public bool isPoints {get; set; }
    // vcount: number; // Number of vertices/points
    public int vcount { get; set; } 
    // vertices: Float32Array; // Vertex buffer (of length vcount*3)
    public float[] vertices {get; set; }
    // colors?: Float32Array; // Optional color buffer (of length vcount*3)
    public float[] colors {get; set; }
    // pointSize: number;
    public float pointSize {get; set; }
}



/**
 * Single UV channel. {@link IMesh} can have more of these.
 */
public struct ISvfUVMap {
    // name: string;
    public string name {get; set; }
    // file: string;
    public string file {get; set; }
    // uvs: Float32Array;
    public float[] uvs {get; set; }
}

public struct ISvfTransform {
    public Vector3 t {get; set; }
    public Vector3 s {get; set; }
    public Quaternion q {get; set; }
    public double[] matrix {get; set; }

}

// public struct ISvfVector3 {
//     public double x {get; set; }
//     public double y {get; set; }
//     public double z {get; set; }
// }

public struct ValueObj {
    public object value {get; set; }
}

public struct XYZArray {
    public float[] XYZ {get; set; }
}

public class IScene {

    public GameObject root;
    private ISvfContent svfContent;
    public IScene(ISvfContent svf) {
        svfContent = svf;

        CreateNodes();
    }

    public void CreateNodes()
    {
        root = new GameObject("root");

    
        string upvectorRaw = svfContent.metadata.metadata["world up vector"].ToString();
        float[] upvector = JsonConvert.DeserializeObject<XYZArray>(upvectorRaw).XYZ;
        
        // root.transform.localScale = new Vector3(scale, scale, -scale);
        // root.transform.Rotate(new Vector3(90, 0, 0));

        if ( upvector[2] != 0f ) { // Z axis
            root.transform.Rotate (Vector3.right, -90) ;
            root.transform.localScale =new Vector3 (-1f, 1f, 1f) ;
        }

        string unitObject = svfContent.metadata.metadata["distance unit"].ToString();
        string unit = (string)JsonConvert.DeserializeObject<ValueObj>(unitObject).value;
        
        float unitsConvert = convertToMeter(unit);

        if ( unitsConvert != 1.0f )
            root.transform.localScale =new Vector3 (root.transform.localScale.x * unitsConvert, root.transform.localScale.y * unitsConvert, root.transform.localScale.z * unitsConvert) ;
        
        // Now move to final position
        root.transform.localPosition =Vector3.zero ;


        foreach(ISvfFragment frag in svfContent.fragments)
        {
            // if(frag.transform != null)
            // {
                //check if matrix
                if(frag.transform?.matrix != null)
                {
                    //dont process this case for now but TODO !!
                    continue;
                }

                bool t = frag.transform?.t != null;
                bool q = frag.transform?.q != null;
                bool s = frag.transform?.s != null;

                //create node
                GameObject node = new GameObject(frag.dbID.ToString());
                if(t) node.transform.position = frag.transform?.t ?? Vector3.zero;
                if(q) node.transform.rotation = frag.transform?.q ?? Quaternion.Euler(Vector3.zero);
                if(s) node.transform.localScale = frag.transform?.s ?? Vector3.one;

                node.transform.SetParent(root.transform, false);

                Mesh mesh = CreateGeometry(frag.geometryID);
                if(mesh != null)
                {
                    MeshFilter meshfilter = node.AddComponent<MeshFilter>();
                    meshfilter.sharedMesh = mesh;

                    Material mat = CreateMaterial(frag.materialID);

                    MeshRenderer meshRenderer = node.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = mat;

                }

                MeshCollider collider = node.AddComponent<MeshCollider>();

                var props = svfContent.properties.getPropertiesByCategory(frag.dbID);

                ForgeProperties forgeProps = node.AddComponent<ForgeProperties>();

                forgeProps.properties = props;

                //get properties
                Debug.Log($"Properties of dbID {frag.dbID} : " + props.Count);

 
//           }
        }


    }

    public static float convertToMeter (string units) {
        if ( units == "centimeter" || units == "cm" )
            return (0.01f) ;
        else if ( units == "millimeter" || units == "mm" )
            return (0.001f) ;
        else if ( units == "foot" || units == "ft" )
            return (0.3048f) ;
        else if ( units == "inch" || units == "in" )
            return (0.0254f) ;
        return (1.0f) ; // "meter" / "m"

        // 'decimal-ft'
        // 'ft-and-fractional-in'
        // 'ft-and-decimal-in'
        // 'decimal-in'
        // 'fractional-in'
        // 'm'
        // 'cm'
        // 'mm'
        // 'm-and-cm'
    }


    public Mesh CreateGeometry(int id)
    {
        ISvfGeometryMetadata meta = svfContent.geometries[id];
        IMeshPack meshpack = svfContent.meshpacks[meta.packID][meta.entityID];
        if(meshpack != null)
        {

            if(meshpack is ISvfLines)
            {
                //now we don't want to process these;
                Debug.Log("Lines not supported yet");
            }
            else if(meshpack is ISvfPoints)
            {
                //now we don't want to process these;
                Debug.Log("Points not supported yet");
            }
            else
            {
                //create a new UnityMesh
                ISvfMesh _svfMesh = (ISvfMesh)meshpack;
                
                Debug.Log($"SVF Mesh - Indices {_svfMesh.indices}");

                int[] triangles = new int[_svfMesh.indices.Length];

                for(int i=0; i < triangles.Length; i++)
                {
                    triangles[i] = (int)_svfMesh.indices[i];
                }

                Vector3[] vertices = new Vector3[_svfMesh.vertices.Length / 3];
                for(int j=0; j < vertices.Length; j++)
                {
                    vertices[j] = new Vector3(_svfMesh.vertices[j*3], 
                                            _svfMesh.vertices[(j*3)+1],
                                            _svfMesh.vertices[(j*3)+2]);
                }

                Vector3[] normals = new Vector3[_svfMesh.normals.Length / 3];
                for(int k=0; k < normals.Length; k++)
                {
                    normals[k] = new Vector3(_svfMesh.normals[k*3], 
                                            _svfMesh.normals[(k*3)+1],
                                            _svfMesh.normals[(k*3)+2]);
                } 

                //only handle one channel for now
                Vector2[] uvs = new Vector2[_svfMesh.uvmaps[0].uvs.Length / 2];
                for(int l=0; l < uvs.Length; l++)
                {
                    uvs[l] = new Vector2(_svfMesh.uvmaps[0].uvs[l*2],
                                        _svfMesh.uvmaps[0].uvs[(l*2)+1]);
                }                
                
                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.normals = normals;
                mesh.uv = uvs;

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();

                // GameObject newMesh = new GameObject("newMesh");
                // MeshFilter meshfilter = newMesh.AddComponent<MeshFilter>();
                // meshfilter.sharedMesh = mesh;

                // MeshRenderer meshRenderer = newMesh.AddComponent<MeshRenderer>();
                // meshRenderer.material = new Material(Shader.Find("Standard"));

                return mesh;
            }

        }

        return null;
  
    }


    public Material CreateMaterial(int id)
    {
#if UNITY_IOS && !UNITY_EDITOR
        Shader shader = Shader.Find("Standard");
#else
        Shader shader = Shader.Find("Standard (Specular setup)");
#endif
        
        Material newMat = new Material(shader); //use something better

        ISvfMaterial svfMat = (ISvfMaterial)svfContent.materials[id];

        //diffuse is the color? 
        if(svfMat.diffuse != null)
        {
            newMat.color = new Color(svfMat.diffuse[0], svfMat.diffuse[1], svfMat.diffuse[2], svfMat.opacity);
        }

        if(svfMat.opacity != 1.0f)
        {
            //change material for transparent mat
            newMat.SetOverrideTag("RenderType", "Transparent");
            newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            newMat.SetInt("_ZWrite", 0);
            newMat.DisableKeyword("_ALPHATEST_ON");
            newMat.DisableKeyword("_ALPHABLEND_ON");
            newMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            newMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        if(svfMat.specular != null)
        {
            newMat.SetColor("_SpecColor", new Color(svfMat.specular[0], svfMat.specular[1], svfMat.specular[2], svfMat.specular[3]));
        }

        if(svfMat.metal)
        {
            Debug.Log("Found a metal mat !");
        }

        newMat.SetFloat("_Glossiness", svfMat.glossiness / 255.0f);

        return newMat;

    }
}


