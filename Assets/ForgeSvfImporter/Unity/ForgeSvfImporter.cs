// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using PiroCIE.SVF;
using PiroCIE.Unity;

namespace PiroCIE.Unity
{
    public class ForgeSvfImporter
    {
        public static Reader ReadFromFileSystem(string svfPath)
        {
            string svfFolderPath = Path.GetDirectoryName(svfPath);

            System.Func<string, byte[]> resolve = (uri) => { 
                var buffer = File.ReadAllBytes(Path.Combine(svfFolderPath, uri));
                return buffer;
            };
            return new Reader(svfPath, resolve);

        }


        public void ReadSvf(string _svfModelPath)
        {
            
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            Reader reader = ReadFromFileSystem(_svfModelPath);
            ISvfContent scene = reader.read();

            UnityLoader unityLoader = new UnityLoader(scene);
            unityLoader.createScene();

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

            Debug.Log($"Svf imported in {elapsedTime}");
        }

    }
    
}