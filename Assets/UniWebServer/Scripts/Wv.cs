using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace UniWebServer
{
    public class Wv : MonoBehaviour
    {
        public TextAsset manifest;
        protected WebViewObject webViewObject;
        
        public IEnumerator StartWebView(string s)
        {
            
            yield return new WaitForSeconds(3);
            
            webViewObject = (new GameObject(this.GetType().Name)).AddComponent<WebViewObject>();
            
            webViewObject.Init( transparent:true,
                cb: (msg) =>
                {
                    Debug.Log($"message from WV: {msg}");
                },
                
                err: msg =>
                {
                    Debug.LogError($"webView error: {msg}");
                },
                // err: msg =>
                // {
                //     Debug.LogError($"webView error: {msg}");
                // },

                // ld: (msg) =>
                // {
                //     Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
                //     // NOTE: depending on the situation, you might prefer
                //     // the 'iframe' approach.
                //     // cf. https://github.com/gree/unity-webview/issues/189
                //
                //     //webViewObject.EvaluateJS($"UnityIncoming({dataJson})");
                //
                //     //webViewObject.SetVisibility(true);
                // },
                // //ua: "custom user agent string",
#if UNITY_EDITOR
                separated: false,
#endif
                enableWKWebView: true);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            webViewObject.bitmapRefreshCycle = 1;
#endif
            // cf. https://github.com/gree/unity-webview/pull/512
            // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
            //webViewObject.SetAlertDialogEnabled(false);

            // cf. https://github.com/gree/unity-webview/pull/550
            // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
            //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

            // cf. https://github.com/gree/unity-webview/pull/570
            // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
            //webViewObject.SetBasicAuthInfo("id", "password");

            webViewObject.SetMargins(0, 0, 0, 0);
            webViewObject.SetVisibility(true);
            
            webViewObject.LoadURL(s.Replace(" ", "%20"));
        }
        
        public void DoLoading(string folderPath)
        {
            // сначала загружаем манифест из StreamingAssets
            // var localCachedManifest =
            //     await LoadCachedManifest(FullPath(Path.Combine(folderPath, _manifestFileName)));
            ZipDataManifest localCachedManifest = JsonConvert.DeserializeObject<ZipDataManifest>(manifest.text);
            //CheckUpgradeMethod(localCachedManifest, _bundleTag, _directoryName, _archiveName);
            StartCoroutine(ShowFromStreamingAssets(localCachedManifest, folderPath));
            Debug.Log("COMPLETE!!!!!!");
        }
        
        protected IEnumerator ShowFromStreamingAssets(ZipDataManifest localManifest, string directoryName)
        {
            if (localManifest == null)
                throw new NullReferenceException(
                    $"[WebViewController.ShowFromStreamingAssets] local manifest is null!");
            IEnumerable<string> filePaths = localManifest.Files.Select(i => Path.Combine(directoryName, i.Key));
            yield return CopyAllFiles(filePaths);
            
        }

        protected IEnumerator CopyAllFiles(IEnumerable<string> paths)
        {
            foreach (var file in paths)
            {
                IEnumerator saveFile(byte[] contents)
                {
                    var path = PersistentPath(file);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, contents);
                    yield break;
                }

                var fullPath = FullPath(file);
                Debug.Log(fullPath);
                yield return readFile(fullPath, saveFile);
            }
        }

        
        private IEnumerator readFile(string src, Func<byte[], IEnumerator> onLoaded)
        {
            byte[] result = null;
            if (src.Contains("://"))
            {
                // for Android
#if UNITY_2018_9_OR_NEWER
                        // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                        var unityWebRequest = UnityWebRequest.Get(src);
                        yield return unityWebRequest.SendWebRequest();
                        result = unityWebRequest.downloadHandler.data;
#else
                var www = new WWW(src);
                yield return www;
                result = www.bytes;
#endif
            }
            else
            {
                result = File.ReadAllBytes(src);
            }

            yield return onLoaded(result);
        }
        
        public static string PersistentPath(string relativePath)
        {
            var path = Application.persistentDataPath;
            return $"{path}{relativePath}"; //Path.Combine(Application.persistentDataPath, relativePath);
        }

        protected static string FullPath(string relativePath)
        {
            var trr = Application.streamingAssetsPath;
            //var result = Path.Combine(trr, relativePath);
            var result = $"{trr}{relativePath}";
            Debug.Log($"{relativePath} => {result}");
            return result;
        }
    }
}