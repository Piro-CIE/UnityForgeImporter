// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System;

//Little hack to avoid some errors (detected on iOS) while deserializing enums
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
namespace PiroCIE.SVF
{
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
        [EnumMember(Value = "Autodesk.CloudPlatform.ViewingMetadata")]
        ViewingMetadata,
        [EnumMember(Value = "Topology")]
        Topology
    }

    public struct ISvfContent
    {
        public ISvfMetadata metadata { get; set; }
        public ISvfFragment[] fragments { get; set; }
        public ISvfGeometryMetadata[] geometries { get; set; }
        public IMeshPack[][] meshpacks { get; set; }
        public ISvfMaterial?[] materials { get; set; }
        public PropDbReader properties { get; set; }
        public Dictionary<string, byte[]> images { get; set; }
    }


    /// <summary>
    /// Parsed content of an actual *.svf file.
    /// </summary>
    public struct ISvfRoot
    {
        public ISvfManifest manifest { get; set; }
        public ISvfMetadata metadata { get; set; }
        public Dictionary<string, byte[]> embedded { get; set; }
    }


    /// <summary>
    /// Top-level manifest containing URIs and types of all assets
    /// referenced by or embedded in a specific SVF file. 
    /// The URIs are typically relative to the SVF file itself.
    /// </summary>
    public struct ISvfManifest
    {
        
        [JsonProperty("name")]
        public string name { get; set; }
        
        [JsonProperty("manifestversion")]
        public int manifestversion { get; set; }
        
        [JsonProperty("toolkitversion")]
        public string toolkitversion { get; set; }
        
        [JsonProperty("assets")]
        public List<ISvfManifestAsset> assets { get; set; }
        
        [JsonProperty("typesets")]
        public List<ISvfManifestTypeSet> typesets { get; set; }

    }

    /// <summary>
    /// Description of a specific asset referenced by or embedded in an SVF,
    /// including the URI, compressed and uncompressed size, type of the asset itself,
    /// and types of all entities inside the asset.   
    /// </summary> 
    public struct ISvfManifestAsset
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("type")]
        public AssetType type { get; set; }

        [JsonProperty("typeset")]
        public string typeset { get; set; }

        [JsonProperty("URI")]
        public string URI { get; set; }

        [JsonProperty("size")]
        public int size { get; set; }

        [JsonProperty("usize")]
        public int usize { get; set; }
    }


    /// <summary>
    /// Collection of type definitions.
    /// </summary>
    public struct ISvfManifestTypeSet
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("types")]
        public List<ISvfManifestType> types { get; set; }
    }

    /// <summary>
    /// Single type definition
    /// </summary>
    public struct ISvfManifestType
    {
        [JsonProperty("class")]
        public string typeClass { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("version")]
        public int version { get; set; }
    }

    /// <summary>
    /// Additional metadata for SVF such as the definition of "up" vector,
    /// default background, etc.
    /// </summary>
    public struct ISvfMetadata
    {
        [JsonProperty("version")]
        public string version { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> metadata { get; set; }
    }


    /// <summary>
    /// Fragment represents a single scene object,
    /// linking together material, geometry, and database IDs,
    /// and providing world transform and bounding box on top of that.
    /// </summary>
    public struct ISvfFragment
    {
        public bool visible { get; set; }
        public int materialID { get; set; }
        public int geometryID { get; set; }
        public int dbID { get; set; }
        public ISvfTransform? transform { get; set; }
        public float[] bbox { get; set; }
    }


    /// <summary>
    /// Lightweight data structure pointing to a mesh in a specific packfile and entry.
    /// Contains additional information about the type of mesh and its primitive count.
    /// </summary>
    public struct ISvfGeometryMetadata
    {
        public byte fragType { get; set; }
        public ushort primCount { get; set; }
        public int packID { get; set; }
        public int entityID { get; set; }
        public int topoID { get; set; }
    }

    public struct ISvfMaterials
    {
        public string name { get; set; }
        public string version { get; set; }
        public Dictionary<string, string> scene { get; set; }
        public Dictionary<string, ISvfMaterialGroup> materials { get; set; }
    }

    public struct ISvfMaterialGroup
    {
        public int version { get; set; }
        public string[] userassets { get; set; }
        public Dictionary<string, ISvfMaterial> materials { get; set; }
    }


    public struct ISvfMaterial
    {
        public float[] diffuse { get; set; }
        public float[] specular { get; set; }
        public float[] ambient { get; set; }
        public float[] emissive { get; set; }
        public float? glossiness { get; set; }
        public float? reflectivity { get; set; }
        public float? opacity { get; set; }
        public bool? metal { get; set; }

        public ISvfMaterialMaps? maps { get; set; }

        //second part of the material defined in the material class
        public string tag { get; set; }
        public string proteinType { get; set; }
        public string definition { get; set; }
        public bool transparent { get; set; }
        public string[] keywords { get; set; }
        public string[] categories { get; set; }
        public ISvfMaterialProperties properties { get; set; }
        public Dictionary<string, StringConnections> textures { get; set; }
    }

    public struct ISvfMaterialProperties
    {
        public Dictionary<string, int> integers { get; set; }
        public Dictionary<string, bool> booleans { get; set; }
        public Dictionary<string, StringValues> strings { get; set; }
        public Dictionary<string, StringValues> uris { get; set; }
        public Dictionary<string, UnitsAndNumberValues> scalars { get; set; }
        public Dictionary<string, RGBValuesAndStringConnections> colors { get; set; }
        public Dictionary<string, IntValues> choicelists { get; set; }
        public Dictionary<string, IntValues> uuids { get; set; }
        public string references { get; set; } //type TODO

    }

    public struct ISvfMaterialMaps
    {
        public ISvfMaterialMap? diffuse { get; set; }
        public ISvfMaterialMap? specular { get; set; }
        public ISvfMaterialMap? normal { get; set; }
        public ISvfMaterialMap? bump { get; set; }
        public ISvfMaterialMap? alpha { get; set; }
    }

    public struct ISvfMaterialMap
    {
        public string uri { get; set; }
        public ISvfMaterialMapScale scale { get; set; }
    }

    public struct ISvfMaterialMapScale
    {
        public float texture_UScale { get; set; }
        public float texture_VScale { get; set; }
    }

    /// <summary>
    ///  Interface to group ISvfMesh, ISvfLines and ISvfPoint
    /// </summary>
    public interface IMeshPack { }

    /// <summary>
    /// Triangular mesh data, including indices, vertices, optional normals and UVs.
    /// </summary>
    public struct ISvfMesh : IMeshPack
    {
        public int vcount { get; set; }
        public int tcount { get; set; }
        public int uvcount { get; set; }
        public int attrs { get; set; }
        public int flags { get; set; }
        public string comment { get; set; }
        public List<ISvfUVMap> uvmaps { get; set; }
        public uint[] indices { get; set; }
        public float[] vertices { get; set; }
        public float[] normals { get; set; }
        public float[] colors { get; set; }
        public Vector3 min { get; set; }
        public Vector3 max { get; set; }
    }

    /// <summary>
    /// Line segment data. 
    /// </summary>
    public struct ISvfLines : IMeshPack
    {
        public bool isLines { get; set; }
        public int vcount { get; set; }
        public int lcount { get; set; }
        public float[] vertices { get; set; }
        public ushort[] indices { get; set; }
        public float[] colors { get; set; }
        public float lineWidth { get; set; }
    }


    /// <summary>
    /// Point cloud data.
    /// </summary>
    public struct ISvfPoints : IMeshPack
    {
        public bool isPoints { get; set; }
        public int vcount { get; set; }
        public float[] vertices { get; set; }
        public float[] colors { get; set; }
        public float pointSize { get; set; }
    }


    /// <summary>
    /// Single UV channel. IMesh can have more of these.
    /// </summary>
    public struct ISvfUVMap
    {
        public string name { get; set; }
        public string file { get; set; }
        public float[] uvs { get; set; }
    }

    //using Unity types
    
    // public struct IVector3 {
    //     public float x { get; set; }
    //     public float y { get; set; }
    //     public float z { get; set; }
    // }


    // public struct IQuaternion {
    //     public float x { get; set; }
    //     public float y { get; set; }
    //     public float z { get; set; }
    //     public float w { get; set; }
    // }

    public struct ISvfTransform
    {
        public Vector3 t { get; set; }
        public Vector3 s { get; set; }
        public Quaternion q { get; set; }
        public double[] matrix { get; set; }
    }

    /// <summary>
    /// A few predefined types to easily deserialize the json using classes
    /// </summary>
    
    public struct StringValues
    {
        public string[] values { get; set; }
    }


    public struct IntValues
    {
        public int[] values { get; set; }
    }

    public struct UnitsAndNumberValues
    {
        public string units { get; set; }
        public float[] values { get; set; }
    }

    public struct RGBValuesAndStringConnections
    {
        public List<RGBA> values { get; set; }
        public string[] connections { get; set; }
    }

    public struct RGBA
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }
    }

    public struct StringConnections
    {
        public string[] connections { get; set; }
    }

    public struct ValueObj
    {
        public object value { get; set; }
    }

    public struct XYZArray
    {
        public float[] XYZ { get; set; }
    }
}

