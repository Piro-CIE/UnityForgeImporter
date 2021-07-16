// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
using System.Collections.Generic;
using UnityEngine;
using PiroCIE.Utils;

namespace PiroCIE.SVF
{
    public class Fragments 
    {
        public static ISvfFragment[] parseFragments(byte[] buffer)
        {
            List<ISvfFragment> fragments = new List<ISvfFragment>();
            PackFileReader pfr = new PackFileReader(buffer);

            for(int i=0, len=pfr.numEntries(); i < len; i++)
            {
                ISvfManifestType? entryType = pfr.seekEntry(i);
                Debug.Assert(entryType != null);
                Debug.Assert(entryType?.version > 4);

                byte flags = pfr.getUint8();
                bool visible = (flags & 0x01) != 0;
                int materialID = pfr.getVarint();
                int geometryID = pfr.getVarint();
                ISvfTransform? transform = pfr.getTransform();
                float[] bbox = new float[6] {0, 0, 0, 0, 0, 0};
                float[] bboxOffset = new float[3]{0, 0, 0};
                if (entryType?.version > 3) {
                    if (transform != null && transform?.t != null ) {
                        bboxOffset[0] = (float)transform?.t.x;
                        bboxOffset[1] = (float)transform?.t.y;
                        bboxOffset[2] = (float)transform?.t.z;
                    }
                }
                for (int j = 0; j < 6; j++) {
                    bbox[j] = pfr.getFloat32() + bboxOffset[j % 3];
                }

                int dbID = pfr.getVarint();

                ISvfFragment fragment = new ISvfFragment(){
                    visible = visible,
                    materialID = materialID,
                    geometryID = geometryID,
                    dbID = dbID,
                    transform = transform,
                    bbox = bbox
                };
                fragments.Add(fragment);

            }

            return fragments.ToArray();


        }
    }

    
}

