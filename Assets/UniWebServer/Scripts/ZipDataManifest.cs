using System.Collections.Generic;
using Newtonsoft.Json;

namespace UniWebServer
{
    public class ZipDataManifest
    {
        /// <summary>
        /// A dictionary of files, indexed by file name
        /// </summary>
        [JsonProperty("files")]
        public Dictionary<string, FileDescriptor> Files { get; private set; }
        
        // /// <summary>
        // /// Manifest File Version
        // /// </summary>
        // [JsonProperty("v")]
        // public string V { get; set; }
        
        /// <summary>
        /// Package version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; private set; }
        
        /// <summary>
        /// Package name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }
    }
    
    public class FileDescriptor
    {
        [JsonProperty("hash")]
        public string Hash { get; private set; }

        [JsonProperty("size")]
        public long Size { get; private set; }
    }
}