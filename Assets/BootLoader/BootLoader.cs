using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BootLoader : MonoBehaviour {
    public static BootLoader instance { get; private set; }
    public string Error = string.Empty;
    public string uri = string.Empty;
    UnityWebRequest www = null;
    AssetBundleRequest loadRequest = null;
    public float downloadProgress {
        get {
            if (www == null) {
                return 0;
            }
            return www.downloadProgress;
        }
    }
    public float loadingProgress {
        get {
            if (loadRequest == null) {
                return 0;
            }
            return loadRequest.progress;
        }
    }
    private void Awake() {
        instance = this;
        StartCoroutine(Download());
    }
    IEnumerator Download() {
        www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbGET) {
            downloadHandler = new DownloadHandlerAssetBundle(uri, 0)
        };
        yield return www.SendWebRequest();
        if(www.isNetworkError) {
            Error = www.error;
            Debug.LogWarning(Error);
            yield break;
        }
        if (www.isHttpError) {
            Error = string.Format("HTTP Error:{0}", www.responseCode);
            Debug.LogWarning(Error);
            yield break;
        }
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
        loadRequest = bundle.LoadAllAssetsAsync<GameObject>();
        yield return loadRequest;
        while(!loadRequest.isDone) {
            yield return null;
        }
        foreach(var prefab in loadRequest.allAssets) {
            if (prefab as GameObject != null) {
                var go = Instantiate(prefab);
                go.name = go.name.Replace("(Clone)", "");
            }
        }
    }

    public Object[] GetAssets() {
        if (loadRequest == null || loadRequest.isDone) {
            return null;
        }
        return loadRequest.allAssets;
    }
}
