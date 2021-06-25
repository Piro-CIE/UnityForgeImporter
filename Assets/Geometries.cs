
using System.Collections.Generic;
using UnityEngine;
using System;

public class Geometries {
    /**
    * Parses geometries from a binary buffer, typically stored in a file called 'GeometryMetadata.pf',
    * referenced in the SVF manifest as an asset of type 'Autodesk.CloudPlatform.GeometryMetadataList'.
    * @generator
    * @param {Buffer} buffer Binary buffer to parse.
    * @returns {Iterable<IGeometryMetadata>} Instances of parsed geometries.
    */
    public static ISvfGeometryMetadata[] parseGeometries(byte[] buffer) {

        List<ISvfGeometryMetadata> geometries = new List<ISvfGeometryMetadata>();

        PackFileReader pfr = new PackFileReader(buffer);
        for (int i = 0, len = pfr.numEntries(); i < len; i++) {
            ISvfManifestType? entry = pfr.seekEntry(i);
            Debug.Assert(entry != null);
            Debug.Assert(entry?.version >= 3);

            byte fragType = pfr.getUint8();
            // Skip past object space bbox -- we don't use that
            pfr.seek(pfr.offset + 24);
            ushort primCount = pfr.getUint16();
            string pID = pfr.getString(pfr.getVarint()).Replace(".pf", "");
            int packID = Int32.Parse(pID);
            int entityID = pfr.getVarint();
            // geometry.topoID = this.stream.getInt32(); //already removed from source code

            //yield { fragType, primCount, packID, entityID };
            ISvfGeometryMetadata geometry = new ISvfGeometryMetadata()
            {
                fragType = fragType,
                primCount = primCount,
                packID = packID,
                entityID = entityID
            };

            geometries.Add(geometry);
        }

        return geometries.ToArray();
    }

}

/**
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
*/