using System;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Text;

public class InstanceTree {
    public static void parseInstanceTree(byte[] buffer)
    {
       
        //PackFileReader pfr = new PackFileReader(buffer);

        // no num entries ??

        var newBuffer = Decompress(buffer);

        var result = Encoding.UTF8.GetString(newBuffer);

        Debug.Log("result " + result);
    }

    public static byte[] Decompress (byte[] gzip) {
        // Create a GZIP stream with decompression mode.
        // ... Then create a buffer and write into while reading from the GZIP stream.
        using ( GZipStream stream =new GZipStream (new MemoryStream (gzip), CompressionMode.Decompress) ) {
            const int size =4096 ;
            byte [] buffer =new byte [size] ;
            using ( MemoryStream memory =new MemoryStream () ) {
                int count =0 ;
                do {
                    count =stream.Read (buffer, 0, size) ;
                    if ( count > 0 )
                        memory.Write (buffer, 0, count) ;
                }
                while ( count > 0 ) ;
                return (memory.ToArray ()) ;
            }
        }
    }
}