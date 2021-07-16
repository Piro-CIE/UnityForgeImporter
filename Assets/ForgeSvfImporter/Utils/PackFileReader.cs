// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
// This code is made from the package Forge-Convert-Utils by Petr Broz (Autodesk)
// Most parts of this code are Typescript -> C# adaptation

using System.IO;
using UnityEngine;
using ICSharpCode.SharpZipLib.GZip;
using PiroCIE.SVF;

namespace PiroCIE.Utils
{
    public class PackFileReader : InputStream 
    {
        public string _type {get; set; }
        protected int _version {get; set; }
        protected int[] _entries { get; set;}
        protected ISvfManifestType[] _types {get; set; }

        public static byte[] DecompressBuffer(byte[] inputBuffer)
        {
            if(inputBuffer[0] == 31 && inputBuffer[1]== 139)
            {
                using (var compressedStream = new MemoryStream(inputBuffer))
                using (var resultStream = new MemoryStream())
                {
                    GZip.Decompress(compressedStream, resultStream, true);
                    return resultStream.ToArray();
                }
            }
            
            return inputBuffer;
        }

        public PackFileReader(byte[] _buffer) : base(DecompressBuffer(_buffer)) 
        {
            this._type = this.getString(this.getVarint());
            this._version = this.getInt32();
            this.parseContents();
        }

        protected void parseContents() {
            // Get offsets to TOC and type sets from the end of the file
            int originalOffset = this.offset;
            this.seek(this.length - 8);
            uint entriesOffset = this.getUint32();
            uint typesOffset = this.getUint32();

            // Populate entries
            this.seek((int)entriesOffset);
            int entriesCount = this.getVarint();
            this._entries = new int[entriesCount]; 
            for (int i = 0; i < entriesCount; i++) {
                this._entries[i] = (int)this.getUint32();
            }

            // Populate type sets
            this.seek((int)typesOffset);
            int typesCount = this.getVarint();
            this._types = new ISvfManifestType[typesCount];
            for (int i = 0; i < typesCount; i++) {
                string _class = this.getString(this.getVarint());
                string _type = this.getString(this.getVarint());

                this._types[i] = new ISvfManifestType(){
                    typeClass = _class,
                    type = _type,
                    version = this.getVarint()    
                };

            }

            // Restore offset
            this.seek(originalOffset);
        }

        public int numEntries() {
            return this._entries.Length;
        }

        public ISvfManifestType? seekEntry(int i) {
            if (i >= this.numEntries()) {
                return null;
            }

            // Read the type index and populate the entry data
            int offset = this._entries[i];
            this.seek(offset);
            uint type = this.getUint32();
            if (type >= this._types.Length) {
                return null;
            }
            return this._types[type];
        }

        // explicit cast of double into float to fit the unity format
        protected Vector3 getVector3D() {
            return new Vector3(
                (float)this.getFloat64(),
                (float)this.getFloat64(),
                (float)this.getFloat64());
        }

        protected Quaternion getQuaternion() {
            return new Quaternion(
                this.getFloat32(),
                this.getFloat32(),
                this.getFloat32(),
                this.getFloat32());
        }

        protected double[] getMatrix3x3() {
        double[] elements = new double[9];
        int count = 0;
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                elements[count] = this.getFloat32();
                count += 1;
            }
        }
        return elements;
        }

        public ISvfTransform? getTransform() {
            byte xformType = this.getUint8();
            Quaternion q;
            Vector3 t;
            Vector3 s;
            double[] matrix;

            switch (xformType) {
                case 0: // translation
                    return new ISvfTransform() { t= this.getVector3D()};
                case 1: // rotation & translation
                    q = this.getQuaternion();
                    t = this.getVector3D();
                    s = Vector3.one;//{ x: 1, y: 1, z: 1 };
                    return new ISvfTransform() { t=t, q=q, s=s};
                case 2: // uniform scale & rotation & translation
                    float scale = this.getFloat32();
                    q = this.getQuaternion();
                    t = this.getVector3D();
                    s = new Vector3(scale, scale, scale);//{ x: scale, y: scale, z: scale };
                    return new ISvfTransform() { t=t, q=q, s=s};
                case 3: // affine matrix
                    matrix = this.getMatrix3x3();
                    t = this.getVector3D();
                    return new ISvfTransform() { t=t, matrix=matrix};
            }
            return null;
        }

    }
    
}
