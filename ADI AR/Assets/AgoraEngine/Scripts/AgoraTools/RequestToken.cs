using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class TokenObject
{
    public string rtcToken;
}

namespace agora_utilities
{
    public static class HelperClass
    {
        public static IEnumerator FetchToken(string url, string channel, int userId, Action<string> callback = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(string.Format("{0}/rtc/{1}/publisher/uid/{2}/", url, channel, userId));
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogWarning("FetchToken: url = " + url + " error:" + request.error);
                callback(null);
                yield break;
            }

            TokenObject tokenInfo = JsonUtility.FromJson<TokenObject>(
              request.downloadHandler.text
            );

            Debug.Log("FetchToken Success: " + tokenInfo.rtcToken.ToString());
            callback(tokenInfo.rtcToken);
        }
    }
}
