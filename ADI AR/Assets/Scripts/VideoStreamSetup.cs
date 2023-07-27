using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
/// <summary>
/// This class controls or spawns the video streamer
/// </summary>
public class VideoStreamSetup : MonoBehaviour
{
    public static VideoStreamSetup Instance { get { return FindObjectOfType<VideoStreamSetup>(); } }

    public float windowWidth = 640;
    public float windowHeight = 480;

    [SerializeField]
    private string appId = "785b190e877c4981a1d2de7d3f0f46d6";

    [SerializeField]
    private string channelName = "ADIXR";

    [SerializeField]
    private string token = "your_token";

    private ArrayList permissionList = new ArrayList();

    private void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        permissionList.Add(Permission.Microphone);
        permissionList.Add(Permission.Camera);
#endif
    }

    public uint GetAgoraUserId() => VideoStreamer.Instance.LocalUserId;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayedStartVideoStream());
        //FindObjectOfType<ARCameraManager>().frameReceived += V;
    }

    public IEnumerator DelayedStartVideoStream()
    {
        yield return new WaitForSeconds(2f);
        CheckPermissions();
        VideoStreamer.Instance.LoadEngine(appId, token);
        VideoStreamer.Instance.Join(channelName);

    }
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    void OnApplicationQuit()
    {
        VideoStreamer.Instance.UnloadEngine();
    }
}
