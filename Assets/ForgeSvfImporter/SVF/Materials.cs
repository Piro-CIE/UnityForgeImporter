// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PiroCIE.Utils;

namespace PiroCIE.SVF
{
    public class Materials
    {

        /// <summary>
        /// Parses materials from a binary buffer, typically stored in a file called 'Materials.json.gz',
        /// referenced in the SVF manifest as an asset of type 'ProteinMaterials'.
        /// </summary>
        /// <param name="buffer">Binary buffer to parse.</param>
        /// <returns>Instances of parsed materials, or null if there are none (or are not supported).</returns>
        public static ISvfMaterial?[] parseMaterials(byte[] buffer)
        {
            List<ISvfMaterial?> materials = new List<ISvfMaterial?>();

            buffer = PackFileReader.DecompressBuffer(buffer);

            if (Buffer.ByteLength(buffer) > 0)
            {
                string json = System.Text.Encoding.Default.GetString(buffer);
                ISvfMaterials svfMat = JsonConvert.DeserializeObject<ISvfMaterials>(json);
                foreach (var item in svfMat.materials)
                {
                    ISvfMaterialGroup group = item.Value;
                    ISvfMaterial material = group.materials[group.userassets[0]];
                    switch (material.definition)
                    {
                        case "SimplePhong":
                            materials.Add(parseSimplePhongMaterial(group));
                            break;
                        default:
                            Debug.LogWarning("Unsupported material definition " + material.definition);
                            break;
                    }
                }

            }

            return materials.ToArray();
        }

        private static ISvfMaterial parseSimplePhongMaterial(ISvfMaterialGroup group)
        {
            ISvfMaterial result = new ISvfMaterial();
            ISvfMaterial material = group.materials[group.userassets[0]];

            result.diffuse = parseColorProperty(material, "generic_diffuse", new float[4] {0, 0, 0, 1});
            result.specular = parseColorProperty(material, "generic_specular", new float[4]{0, 0, 0, 1});
            result.ambient = parseColorProperty(material, "generic_ambient", new float[4]{0, 0, 0, 1});
            result.emissive = parseColorProperty(material, "generic_emissive", new float[4] {0, 0, 0, 1});

            result.glossiness = parseScalarProperty(material, "generic_glossiness", 30);
            result.reflectivity = parseScalarProperty(material, "generic_reflectivity_at_0deg", 0);
            result.opacity = 1.0f - parseScalarProperty(material, "generic_transparency", 0);

            result.metal = parseBooleanProperty(material, "generic_is_metal", false);

            if (material.textures.Count > 0) {
                ISvfMaterialMaps maps = new ISvfMaterialMaps();
                ISvfMaterialMap? diffuse = parseTextureProperty(material, group, "generic_diffuse");
                if (diffuse != null) {
                    maps.diffuse = diffuse;
                }
                ISvfMaterialMap? specular = parseTextureProperty(material, group, "generic_specular");
                if (specular != null) {
                    maps.specular = specular;
                }
                ISvfMaterialMap? alpha = parseTextureProperty(material, group, "generic_alpha");
                if (alpha != null) {
                    maps.alpha = alpha;
                }
                ISvfMaterialMap? bump = parseTextureProperty(material, group, "generic_bump");
                if (bump != null) {
                    if (parseBooleanProperty(material, "generic_bump_is_normal", false)) {
                        maps.normal = bump;
                    } else {
                        maps.bump = bump;
                    }
                }
                result.maps = maps;
            }

        return result;
        }


        private static bool parseBooleanProperty(ISvfMaterial material, string prop, bool defaultValue)
        {
            if (material.properties.booleans != null && material.properties.booleans.ContainsKey(prop))
            {
                return material.properties.booleans[prop];
            }
            else
            {
                return defaultValue;
            }
        }


        private static float parseScalarProperty(ISvfMaterial material, string prop, float defaultValue)
        {
            if (material.properties.scalars != null && material.properties.scalars.ContainsKey(prop))
            {
                return material.properties.scalars[prop].values[0];
            }
            else
            {
                return defaultValue;
            }
        }

        private static float[] parseColorProperty(ISvfMaterial material, string prop, float[] defaultValue)
        {
            if (material.properties.colors != null && material.properties.colors.ContainsKey(prop))
            {
                var color = material.properties.colors[prop].values[0];
                return new float[4] { color.r, color.g, color.b, color.a };
            }
            else
            {
                return defaultValue;
            }
        }


        private static ISvfMaterialMap? parseTextureProperty(ISvfMaterial material, ISvfMaterialGroup group, string prop)
        {
            if(material.textures != null && material.textures.ContainsKey(prop))
            {
                string connection = material.textures[prop].connections[0];
                ISvfMaterial texture = group.materials[connection];
                if(texture.properties.uris.ContainsKey("unifiedbitmap_Bitmap"))
                {
                    string uri = texture.properties.uris["unifiedbitmap_Bitmap"].values[0];
                    float texture_UScale = 0.0f, texture_VScale = 0.0f;

                    if(texture.properties.scalars != null 
                        && texture.properties.scalars.ContainsKey("texture_UScale")
                        && texture.properties.scalars.ContainsKey("texture_VScale"))
                    {
                        texture_UScale = texture.properties.scalars["texture_UScale"].values[0];
                        texture_VScale = texture.properties.scalars["texture_VScale"].values[0];      
                    }

                    if(uri != null)
                    {
                        return new ISvfMaterialMap() {
                            uri = uri,
                            scale = new ISvfMaterialMapScale()
                            {
                                texture_UScale = texture_UScale != 0 ? texture_UScale : 1.0f,
                                texture_VScale = texture_VScale != 0 ? texture_VScale : 1.0f,
                            }
                        };
                    }
                }
            }
            return null;
        }


    }    
}
