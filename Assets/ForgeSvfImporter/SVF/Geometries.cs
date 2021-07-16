// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
using System.Collections.Generic;
using UnityEngine;
using System;
using PiroCIE.Utils;

namespace PiroCIE.SVF
{
    public class Geometries {

        /// <summary>
        /// Parses geometries from a binary buffer, typically stored in a file called 'GeometryMetadata.pf',
        /// referenced in the SVF manifest as an asset of type 'Autodesk.CloudPlatform.GeometryMetadataList'.
        /// </summary>
        /// <param name="buffer">Binary buffer to parse.</param>
        /// <returns>Instances of parsed geometries.</returns>
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
    
}
