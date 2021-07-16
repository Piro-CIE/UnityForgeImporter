using System;
using System.Collections.Generic;


// Intermediate format added to follow the Forge-Convert-Utils logic
// Not used for now
// The Unity loader import from the ISvfContent
namespace PiroCIE.IMF
{
    public abstract class IScene 
    {
        public abstract IMetadata getMetadata();
        public abstract int getNodeCount();
        public abstract INode getNode(int id);
        public abstract int getGeometryCount();
        public abstract IGeometry getGeometry(int id);
        public abstract int getMaterialCount();
        public abstract IMaterial getMaterial(int id);
        public abstract byte[] getImage(string uri);
    }

    public struct IMetadata 
    {
        public Dictionary<string, object> key { get; set; }
    }

    public struct IVec2 
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public struct IVec3 
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public struct IQuat 
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }
    }

    public enum TransformKind 
    {
        Matrix,
        Decomposed
    }

    public interface ITransform {}
    public interface INode {}

    // public interface NodeID {}
    // public interface GeometryID {}
    // public interface MaterialID {}
    // public interface CameraID {}
    // public interface LightID {}

    public class IMatrixTransform : ITransform 
    {
        public TransformKind kind { get; set; } = TransformKind.Matrix;
        public List<double> elements { get; set; }
    }


    public class IDecomposedTransform : ITransform 
    {
        public TransformKind kind { get; set; } = TransformKind.Decomposed;
        public IVec3? translation { get; set; }
        public IQuat? rotation { get; set; }
        public IVec3? scale { get; set; }
    }

    public enum NodeKind 
    {
        Group,
        Object,
        Camera,
        Light
    }

    public class IGroupNode : INode
    {
        public NodeKind kind { get; set; } = NodeKind.Group;
        public int dbid { get; set; }
        public ITransform transform { get; set; }
        public int[] children { get; set; }
    }

    public class IObjectNode : INode
    {
        public NodeKind kind { get; set; } = NodeKind.Object;
        public int dbid { get; set; }
        public ITransform transform { get; set; }
        public int geometry { get; set; }
        public int material { get; set; }
    }


    public class ICameraNode : INode
    {
        public NodeKind kind { get; set; } = NodeKind.Camera; 
        public ITransform transform { get; set; }
        public int camera { get; set; }
    }


    public class ILightNode : INode
    {
        public NodeKind kind { get; set; } = NodeKind.Light;
        public ITransform transform { get; set; }
        public int light { get; set; }
    }

    public interface IGeometry {}

    public enum GeometryKind
    {
        Mesh,
        Lines,
        Points,
        Empty
    }

    public class IMeshGeometry : IGeometry {
        public GeometryKind kind { get; set; } = GeometryKind.Mesh;
        public Func<uint[]> getIndices { get; set; }
        public Func<float[]> getVertices {get; set; }
        public Func<float[]> getNormals {get; set; }
        public Func<float[]> getColors {get; set; }
        public Func<int> getUvChannelCount {get; set; }
        public Func<int, float[]> getUvs {get; set; }
    }


    public class ILineGeometry : IGeometry {
        public GeometryKind kind { get; set; }
        public Func<ushort[]> getIndices { get; set; }
        public Func<float[]> getVertices { get; set; }
        public Func<float[]> getColors { get; set; }
    }

    public class IPointGeometry : IGeometry {
        public GeometryKind kind { get; set; } = GeometryKind.Points;
        public Func<float[]> getVertices {get; set; }
        public Func<float[]> getColors {get; set; }
    }

    public class IEmptyGeometry : IGeometry {
        public GeometryKind kind { get; set; } = GeometryKind.Empty;
    }

    public enum MaterialKind {
        Physical
    }

    public interface IMaterial {}

    public class IPhysicalMaterial : IMaterial 
    {
        public MaterialKind kind { get; set; } = MaterialKind.Physical;
        public IVec3 diffuse { get; set; }
        public float? metallic { get; set; }
        public float? roughness { get; set; }
        public float? opacity { get; set; }
        public IMaps? maps { get; set; }
        public IVec2 scale { get; set; }
    }

    public struct IMaps {
        public string diffuse { get; set; }
    }


}