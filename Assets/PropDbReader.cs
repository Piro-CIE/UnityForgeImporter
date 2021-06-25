using System;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class PropDbReader
{
    public string[] ids { get; set; }
    public int[] offsets { get; set; }
    public int[] avs { get; set; }

    public object[] attrs { get; set; }
    public string[] vals { get; set; }

    public static byte[] Unzip(byte[] input)
    {
        using (var compressedStream = new MemoryStream(input))
        using (var resultStream = new MemoryStream())
        {
            GZip.Decompress(compressedStream, resultStream, true);
            return resultStream.ToArray();
        }
    }

    public PropDbReader(byte[] _ids, byte[] _offsets, byte[] _avs, byte[] _attrs, byte[] _vals)
    {
        ids = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(_ids)));
        offsets = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(_offsets)));
        avs = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(_avs)));
        attrs = JsonConvert.DeserializeObject<object[]>(Encoding.UTF8.GetString(Unzip(_attrs)));
        vals = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(_vals)));
    }

    /**
 * Enumerates all properties (including internal ones such as "__child__" property
 * establishing the parent-child relationships) of given object.
 * @generator
 * @param {number} id Object ID.
 * @returns {Iterable<{ name: string; category: string; value: any }>} Name, category, and value of each property.
 */
    public Property[] enumerateProperties(int id)
    {
        List<Property> properties = new List<Property>();

        if (id > 0 && id < offsets.Length)
        {
            Debug.Log("ID : " + id);
            int avStart = 2 * offsets[id];
            int avEnd = (id == offsets.Length-1) ? avs.Length : 2 * offsets[id + 1];
            for (int i = avStart; i < avEnd; i += 2)
            {
                int attrOffset = avs[i];
                int valOffset = avs[i + 1];
                var attrObj = attrs[attrOffset];
                string[] attr = new string[0];
                if(!(attrObj is int))
                {
                    attr = JsonConvert.DeserializeObject<string[]>(attrObj.ToString());
                }
                var value = vals[valOffset];
                //yield { name: attr[0], category: attr[1], value };
                properties.Add(new Property(){
                    name = attr[0],
                    category = attr[1],
                    value = value
                });
            }
        }

        return properties.ToArray();
    }

    /**
     * Finds "public" properties of given object.
     * Additional properties like parent-child relationships are not included in the output.
     * @param {number} id Object ID.
     * @returns {{ [name: string]: any }} Dictionary of property names and values.
     */
    public Dictionary<string, string> getProperties(int id) {
        Dictionary<string, string> props = new Dictionary<string, string>();
        Regex rg = new Regex(@"^__\w+__$");
        foreach(var prop in enumerateProperties(id)) {
            if (prop.category != null && rg.IsMatch(prop.category)) {
                // Skip internal attributes
            } else {
                props[prop.name] = prop.value;
            }
        }
        return props;
    }


    /**
     * Finds all properties of given object
     * Create a list of properties key / value
     */
    public Dictionary<string, List<KeyValuePair<string, object>>> getPropertiesByCategory(int id) {
        Dictionary<string, List<KeyValuePair<string, object>>> properties = new Dictionary<string, List<KeyValuePair<string, object>>>();
        Regex rg = new Regex(@"^__\w+__$");

        //create the list of categories
        List<string> categories = new List<string>();

        List<Property> props = enumerateProperties(id).ToList();
        foreach(var prop in props) {
            if (prop.category != null && rg.IsMatch(prop.category)) {
                // Skip internal attributes
            } else {
                if (!categories.Exists(x => x.Contains(prop.category)))
                {
                    categories.Add(prop.category);
                }
            }
        }

        for(int j = 0; j< categories.Count; j ++)
        {
            string CategoryPropKey = categories[j];

            var propResult = props.FindAll(x => x.category == CategoryPropKey);

            List<KeyValuePair<string, object>> propDictonnary = new List<KeyValuePair<string, object>>();

            propResult.ForEach((prop) =>
            {
                //string propKey = prop.DisplayName ?? prop.PropName;
                string propKey = prop.name;
                propDictonnary.Add(new KeyValuePair<string, object>(propKey, prop.value));
            });

            properties.Add(CategoryPropKey, propDictonnary);
        }


        return properties;
    }

    /**
     * Finds IDs of all children of given object.
     * @param {number} id Object ID.
     * @returns {number[]} Children IDs.
     */
    public int[] getChildren(int id) {
        List<int> children = new List<int>();
        foreach(var prop in enumerateProperties(id)) {
            if (prop.category == "__child__") {
                children.Add(int.Parse(prop.value));
            }
        }
        return children.ToArray();
    }


}


public struct Property
{
    //{ name: string; category: string; value: any }
    public string name { get; set; }
    public string category { get; set; }
    public string value { get; set; } // maybe change for only strings values
}
