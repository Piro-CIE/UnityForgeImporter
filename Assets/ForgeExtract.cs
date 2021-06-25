using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;


public class ForgeExtract : MonoBehaviour
{
    //BureauRevit
    static string urnBureauRevit = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6aWFqaGhreGJ6MTVyaG5xb295bnA5c2c0aXZjaHZ2bjItcGVyc2lzdGVudGJ1Y2tldC9CdXJlYXUucnZ0";
    static string guidBureauRevit = "f5b75801-9bb3-4921-7ec7-06ab73f6c94d";
    static string filenameBureauRevit = "BureauRevit";

    //ProjetAddin
    static string urnProjetAddin = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6aWFqaGhreGJ6MTVyaG5xb295bnA5c2c0aXZjaHZ2bjItcGVyc2lzdGVudGJ1Y2tldC9Qcm9qZXRfQWRkaW5fQWxsXzIwMTgucnZ0";
    static string guidProjetAddin = "61a0c07a-032a-d40e-871f-d96cc3a82305";
    static string filenameProjetAddin = "ProjetAddin";
    
    static string[][] models = new string[2][]{
        new string[3] {
            urnBureauRevit, guidBureauRevit, filenameBureauRevit
        },
        new string[3]{
            urnProjetAddin, guidProjetAddin, filenameProjetAddin
        }
    };
    
    private GameObject root;

    public static Reader ReadFromFileSystem(int modelIndex)
    {
        //
        string svfFolderPath = Path.Combine(Application.streamingAssetsPath, models[modelIndex][2], models[modelIndex][0], models[modelIndex][1]);
        string svfPath = Path.Combine(svfFolderPath, "output.svf");

        Func<string, byte[]> resolve = (uri) => { 
            var buffer = File.ReadAllBytes(Path.Combine(svfFolderPath, uri));
            return buffer;
        };
        return new Reader(svfPath, resolve);

    }


    public void ReadSvf(int modelIndex)
    {
        if(root != null)
            Destroy(root);
        
        Reader r = ReadFromFileSystem(modelIndex);
        r.read();

        root = r.root;
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
                    // do something with ZipInputStream
                    StreamReader reader = new StreamReader(s);
                    string text = reader.ReadToEnd();
                    //Debug.Log(text);
                    svf.manifest = JsonConvert.DeserializeObject<ISvfManifest>(text);

                    continue;
                }
            }

            if (zipEntry.Name == "metadata.json")
            {
                using (var s = zip.GetInputStream(zipEntry))
                {
                    // do something with ZipInputStream
                    StreamReader reader = new StreamReader(s);
                    string text = reader.ReadToEnd();
                    //Debug.Log(text);
                    svf.metadata = JsonConvert.DeserializeObject<ISvfMetadata>(text);

                    continue;
                    //Debug.Log(metadata);
                }
            }

            using(var s = zip.GetInputStream(zipEntry))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    _embedded[zipEntry.Name] =  ms.ToArray();
                }
            }

        }

        svf.embedded = _embedded;

    }

    public void read()
    {
        //  var output = {
        //     metadata: getMetadata(),
        //     fragments: [],
        //     geometries: [],
        //     meshpacks: [],
        //     materials: [],
        //     properties: null,
        //     images: {}
        // };
        // IMFScene output = new IMFScene() {
        //     metadata = getMetadata()
        // };


        this.readInstanceTree();

        ISvfFragment[] fragments = this.readFragments();
        ISvfGeometryMetadata[] geometries = this.readGeometries();
        ISvfMaterial?[] materials = this.readMaterials();
        Dictionary<string, byte[]> images = new Dictionary<string, byte[]>();
        
        PropDbReader properties = this.getPropertyDb();

        int meshPackCount = this.getMeshPackCount();
        IMeshPack[][] meshPacks = new IMeshPack[meshPackCount][];
        for (int i = 0, len = meshPackCount; i < len; i++) 
        {
            meshPacks[i] = this.readMeshPack(i);
        }

        foreach(string uri in this.listImages())
        {
            string normalizedUri = String.Join(Path.DirectorySeparatorChar.ToString(), uri.ToLower().Split('[', '\\', '/', ']'));
            byte[] imageData = null;
            try
            {
                imageData = this.getAsset(uri);
            }
            catch (System.Exception){}

            if(imageData == null)
            {
                Debug.Log($"Could not find image {uri}, trying a lower-cased version of the URI...");

                try
                {
                    imageData = this.getAsset(uri.ToLower());   
                }
                catch (System.Exception){}
         
            }

            if(imageData == null)
            {
                Debug.Log($"Still could not find image {uri}; will default to a single black pixel placeholder image...");
            }

            images[normalizedUri] = imageData;
        }


        Debug.Log("Fragments founded " + fragments.Length);
        Debug.Log("Geometries founded " + geometries.Length);
        Debug.Log("Materials founded " + materials.Length);
        Debug.Log("MeshPack founded : " + meshPackCount);
        Debug.Log("Images founded : " + images.Count);

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

        IScene newScene = new IScene(svfContent);

        root = newScene.root;

    }

    /**
     * Retrieves raw binary data of a specific SVF asset.
     * @async
     * @param {string} uri Asset URI.
     * @returns {Promise<Buffer>} Asset content.
     */
    private byte[] getAsset(string uri) 
    {
        return resolve(uri);
    }

    /**
     * Retrieves parsed SVF metadata.
     * @async
     * @returns {Promise<SVF.ISvfMetadata>} SVF metadata.
     */
    private ISvfMetadata getMetadata()
    {
        return svf.metadata;
    }

    /**
     * Retrieves parsed SVF manifest.
     * @async
     * @returns {Promise<SVF.ISvfManifest>} SVF manifest.
     */
    private ISvfManifest getManifest() 
    {
        return svf.manifest;
    }

    protected ISvfManifestAsset? findAsset(AssetType _type) {
        return this.svf.manifest.assets.Where(asset => asset.type == _type).First();
    }

    protected ISvfManifestAsset? findAsset(string uri) {
        return this.svf.manifest.assets.Where(asset => asset.URI == uri).First();
    }

    protected ISvfManifestAsset? findAsset(AssetType _type, string uri) {
        return this.svf.manifest.assets.Where(asset => asset.type ==_type).Where(asset => asset.URI == uri).First();
    }
    /**
     * Retrieves, parses, and collects all SVF fragments.
     * @async
     * @returns {Promise<IFragment[]>} List of parsed fragments.
     */
    protected ISvfFragment[] readFragments() {
        ISvfManifestAsset? fragmentAsset = this.findAsset(AssetType.FragmentList);
        if (fragmentAsset == null) {
            throw new Exception("Fragment list not found.");
        }
        byte[] buffer = this.getAsset(fragmentAsset?.URI);
        return Fragments.parseFragments(buffer);
    }


    /**
     * Retrieves, parses, and collects all SVF geometry metadata.
     * @async
     * @returns {Promise<SVF.IGeometryMetadata[]>} List of parsed geometry metadata.
     */
     
    protected ISvfGeometryMetadata[] readGeometries() {
        ISvfManifestAsset? geometryAsset = this.findAsset(AssetType.GeometryMetadataList);
        if (geometryAsset == null) {
            throw new Exception("Geometry metadata not found.");
        }
        byte[] buffer = this.getAsset(geometryAsset?.URI);
        return Geometries.parseGeometries(buffer);
    }


    /**
     * Retrieves, parses, and collects all SVF geometry metadata.
     * @async
     * @returns {Promise<SVF.IGeometryMetadata[]>} List of parsed geometry metadata.
     */
     
    protected ISvfMaterial?[] readMaterials() {
        ISvfManifestAsset? materialAsset = this.findAsset(AssetType.ProteinMaterials, "Materials.json.gz");
        if (materialAsset == null) {
            throw new Exception("Materials not found.");
        }
        byte[] buffer = this.getAsset(materialAsset?.URI);
        return Materials.parseMaterials(buffer);
    }

    /**
     * Gets the number of available mesh packs.
     */
    protected int getMeshPackCount(){
        int count = 0;
        // this.svf.manifest.assets.forEach(asset => {
        //     if (asset.type === SVF.AssetType.PackFile && asset.URI.match(/^\d+\.pf$/)) {
        //         count++;
        //     }
        // });
        Regex rg = new Regex(@"^\d+\.pf$");
        foreach (ISvfManifestAsset asset in this.svf.manifest.assets)
        {
            if (asset.type == AssetType.PackFile && rg.IsMatch(asset.URI)) {
                count++;
            }
        }
        return count;
    }

    /**
     * Retrieves, parses, and collects all meshes, lines, or points in a specific SVF meshpack.
     * @async
     * @param {number} packNumber Index of mesh pack file.
     * @returns {Promise<(SVF.IMesh | SVF.ILines | SVF.IPoints | null)[]>} List of parsed meshes,
     * lines, or points (or null values for unsupported mesh types).
     */
    protected IMeshPack[] readMeshPack(int packNumber) {
        ISvfManifestAsset? meshPackAsset = this.findAsset(AssetType.PackFile, $"{packNumber}.pf");
        if (meshPackAsset == null) {
            throw new Exception($"Mesh packfile {packNumber}.pf not found.");
        }
        byte[] buffer = this.getAsset(meshPackAsset?.URI);
        return Meshes.parseMeshes(buffer);
    }

    /**
    * Try to parse the instance tree but it seems not to be in the same format as others packfile ...
    **/
    protected void readInstanceTree() {
        var assets = this.svf.manifest.assets;
        ISvfManifestAsset? instanceTreeAsset = this.findAsset(AssetType.InstanceTree);
        if (instanceTreeAsset == null) {
            throw new Exception($"Instance tree not found.");
        }
        byte[] buffer = this.getAsset(instanceTreeAsset?.URI);
        /*return*/ InstanceTree.parseInstanceTree(buffer);
    }

    /**
     * Finds URIs of all image assets referenced in the SVF.
     * These can then be retrieved using {@link getAsset}.
     * @returns {string[]} Image asset URIs.
     */
    protected string[] listImages(){
        // return this.svf.manifest.assets
        //     .filter(asset => asset.type === SVF.AssetType.Image)
        //     .map(asset => asset.URI);
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

        if (idsAsset==null || offsetsAsset==null || avsAsset==null || attrsAsset==null || valsAsset==null) {
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

