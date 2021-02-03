using System;
using System.Collections;
using System.IO;
using UnityEngine;


namespace UniWebServer
{
    [RequireComponent(typeof(EmbeddedWebServerComponent))]
    public class FileServer : MonoBehaviour, IWebResource
    {
        public string folderPath = "/hextris";
        EmbeddedWebServerComponent server;
        
        public string url;
        public Wv wv;
        
        
        IEnumerator Start()
        {
           
            wv.DoLoading(folderPath);
            yield return new WaitForSeconds(5);
            server = GetComponent<EmbeddedWebServerComponent>();
            server.AddResource(folderPath, this);
            
            Debug.Log("Loading complete");
            StartCoroutine(wv.StartWebView(url));
        }

        
        
        
            

        public void HandleRequest(Request request, Response response)
        {
            //Debug.Log("REQUEST: " + request.method  + " " + request.uri);
            // check if file exist at folder (need to assume a base local root)
            string folderRoot = Application.persistentDataPath;
            string fullPath = folderRoot + Uri.UnescapeDataString(request.uri.LocalPath);
            //Debug.Log($"file: {fullPath}\n exists: {File.Exists(fullPath)}\n exists2: {File.Exists(fullPath.Replace("/fb_003", ""))}");
            // get file extension to add to header
            string fileExt = Path.GetExtension(fullPath);
            // not found
            if (!File.Exists(fullPath)) {
                response.statusCode = 404;
                response.message = "Not Found";
                return;
            }

            // serve the file
            response.statusCode = 200;
            response.message = "OK";
            response.headers.Add("Content-Type", MimeTypeMap.GetMimeType(fileExt));

            // read file and set bytes
            using (FileStream fs = File.OpenRead(fullPath))
            {
                int length = (int)fs.Length;
                byte[] buffer;

                // add content length
                response.headers.Add("Content-Length", length.ToString());

                // use binary for mostly all except text
                using (BinaryReader br = new BinaryReader(fs))
                {
                    buffer = br.ReadBytes(length);
                }
                response.SetBytes(buffer);

            }
        }

    }
}