using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PiroCIE.Unity;
using System.IO;

public class ForgeLoaderSample : MonoBehaviour
{
    public string SvfModelPath = "";
    public bool streamingAssets = true;

    // Start is called before the first frame update
    void Start()
    {
        if(SvfModelPath != null && SvfModelPath != "")
        {
            string filepath = Path.Combine((streamingAssets ? Application.streamingAssetsPath : ""),SvfModelPath);
            LoadSvf(filepath);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSvf(string modelPath)
    {
        ForgeSvfImporter svfImporter = new ForgeSvfImporter();
        svfImporter.ReadSvf(modelPath);
    }
}
