//this file is an intermediate class waiting for Unity serialization


public struct IMFScene
{

    // metadata: await this.getMetadata(),
    // fragments: [],
    // geometries: [],
    // meshpacks: [],
    // materials: [],
    // properties: null,
    // images: {}

    public ISvfMetadata metadata {get; set;}

    public ISvfFragment[] fragments { get; set; }

    public ISvfGeometryMetadata[] geometries { get; set; }

    public IMeshPack[] meshpacks { get; set; }

    public ISvfMaterial[] materials { get; set; }

    public string properties {get; set; } //check type

    public string images {get; set; } //check type


}