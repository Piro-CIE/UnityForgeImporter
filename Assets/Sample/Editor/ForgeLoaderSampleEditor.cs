using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ForgeLoaderSample))]
public class ForgeLoaderSampleEditor : Editor {

    private string BureauRevitPath = Path.Combine(Application.streamingAssetsPath,
        "BureauRevit",
        "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6aWFqaGhreGJ6MTVyaG5xb295bnA5c2c0aXZjaHZ2bjItcGVyc2lzdGVudGJ1Y2tldC9CdXJlYXUucnZ0",
        "f5b75801-9bb3-4921-7ec7-06ab73f6c94d",
        "output.svf");

    private string BB8Path = Path.Combine(Application.streamingAssetsPath,
        "BB8", "0", "1", "Design.svf");
    
    private string OfficePath = Path.Combine(Application.streamingAssetsPath,
        "Office", "Resource", "3D View", "{3D} 713157", "{3D}.svf");

    private string AnalyzePath = Path.Combine(Application.streamingAssetsPath,
        "Analyze", "c674dc18-aac0-bfc5-259f-78d3f0e145f7", "0.svf");

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        ForgeLoaderSample f = target as ForgeLoaderSample;
        if(GUILayout.Button("Load Bureau Revit"))
        {
            f.LoadSvf(BureauRevitPath);
        }

        if(GUILayout.Button("Load BB8"))
        {
            f.LoadSvf(BB8Path);
        }

        if(GUILayout.Button("Load Office"))
        {
            f.LoadSvf(OfficePath);
        }

        if(GUILayout.Button("Load Analyze"))
        {
            f.LoadSvf(AnalyzePath);
        }
    }
}