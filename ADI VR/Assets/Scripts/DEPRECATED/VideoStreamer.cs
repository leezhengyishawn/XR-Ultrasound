using agora_gaming_rtc;
using UnityEngine;
using UnityEngine.UI;

public class VideoStreamer : MonoBehaviour
{
    public static VideoStreamer Instance { get { return FindObjectOfType<VideoStreamer>(); } }

    private IRtcEngine mRtcEngine;

    private string token;

    private int lastError;

    private uint localUserId;

    public uint LocalUserId
    {
        get => localUserId;
    }

    public void LoadEngine(string appId, string token = null)
    {
        this.token = token;

        if (mRtcEngine != null)
            return;

        mRtcEngine = IRtcEngine.GetEngine(appId);
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
        Debug.Log("Video Stream Loaded");

        mRtcEngine.EnableAudio();
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();        
    }

    public void Join(string channel)
    {
        if (mRtcEngine == null) return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        mRtcEngine.OnUserJoined = OnUserJoinedHandler;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnWarning = (int warn, string msg) =>
        {
            Debug.LogWarning($"Warning code:{warn} msg:{IRtcEngine.GetErrorDescription(warn)}");
        };

        mRtcEngine.OnError = HandleError;

        /*  This API Accepts AppID with token; by default omiting info and use 0 as the local user id */
        int i = mRtcEngine.JoinChannelByKey(token, channel);
        Debug.Log(i);
        Debug.Log("Video Stream Joined");
    }

    public void Leave()
    {
        if (mRtcEngine == null) return;

        mRtcEngine.LeaveChannel();

        // deregister video frame observers in native-c code
        mRtcEngine.DisableVideoObserver();

        GameObject go = GameObject.Find($"{localUserId}");
        if (go != null) Destroy(go);
    }


    public void EnableVideo(bool pauseVideo)
    {
        if (mRtcEngine != null)
        {
            if (!pauseVideo)
                mRtcEngine.EnableVideo();
            else
                mRtcEngine.DisableVideo();
        }
    }

    // Implement engine callbacks
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        Debug.Log($"OnJoinChannelSuccess: uid = {uid}");

        localUserId = uid;

        GameObject childVideo = GetChildVideoLocation(uid);

        VideoSurface videoSurface = MakeImageVideoSurface(childVideo);
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        Debug.Log("OnUserJoined");
        GameObject childVideo = GetChildVideoLocation(uid);

        // create a GameObject and assign to this new user
        VideoSurface videoSurface = MakeImageVideoSurface(childVideo);

        if (videoSurface != null)
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        }
    }

    private static GameObject GetChildVideoLocation(uint uid)
    {
        // find a game object to render video stream from 'uid'
        GameObject canvas = GameObject.Find("VideoCanvas");
        GameObject childVideo = canvas.transform.Find($"{uid}")?.gameObject;

        if (childVideo == null)
        {
            childVideo = new GameObject($"{uid}");
            childVideo.transform.parent = canvas.transform;
        }

        return childVideo;
    }

    public VideoSurface MakeImageVideoSurface(GameObject go)
    {
        go.AddComponent<RawImage>();
        go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        var rectTransform = go.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(VideoStreamSetup.Instance.windowHeight, VideoStreamSetup.Instance.windowWidth);
        rectTransform.localPosition = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);

        //rectTransform.localRotation = new Quaternion(0, rectTransform.localRotation.y,
        //    -180f, rectTransform.localRotation.w);
        rectTransform.localEulerAngles = new Vector3(0, 0, -90f);
        return go.AddComponent<VideoSurface>();
    }

    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    private void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (go != null) Destroy(go);
    }

    private void HandleError(int error, string msg)
    {
        if (error == lastError) return;

        if (string.IsNullOrEmpty(msg))
        {
            msg = string.Format($"Error code:{error} msg:{IRtcEngine.GetErrorDescription(error)}");
        }

        switch (error)
        {
            case 101:
                msg += "\nPlease make sure your AppId is valid and it does not require a certificate for this demo.";
                break;
        }

        lastError = error;
    }


    // unload agora engine
    public void UnloadEngine()
    {
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  // Place this call in ApplicationQuit
            mRtcEngine = null;
        }
    }
}
