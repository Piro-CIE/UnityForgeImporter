using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ForgeExtract))]
public class ForgeExtractEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(GUILayout.Button("ReadFromFileSystem"))
        {
            ForgeExtract.ReadFromFileSystem(0);
        }

        ForgeExtract f = target as ForgeExtract;
        if(GUILayout.Button("Read Svf"))
        {
            f.ReadSvf(0);
        }
    }
}