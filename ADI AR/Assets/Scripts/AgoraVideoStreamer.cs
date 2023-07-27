using System.Collections;
using agora_gaming_rtc;
using agora_utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static agora_gaming_rtc.ExternalVideoFrame;

public class AgoraVideoStreamer : MonoBehaviour
{
    //Agora stream setup variables
    public string APP_ID = "3ea96cd2c2e84aa7a91c3f5d86cafa82";
    public string TOKEN = "";
    public string CHANNEL_NAME = "ADIXR";

    private IRtcEngine mRtcEngine = null;
    public uint localUserId;
    public bool streamingVid = false;


    //Video streaming
    public ARCameraManager cameraManager;
    MonoBehaviour monoProxy;
    public static TextureFormat ConvertFormat = TextureFormat.BGRA32;
    public static VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;
    int i = 0; // monotonic timestamp counter

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void Start()
    {
        InitEngine();
        StartCoroutine(FetchToken());
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
        }
    }

    public IEnumerator FetchToken()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://adixr-tokenservice.herokuapp.com/rtc/ADIXR/publisher/uid/0/");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("CANNOT CONNECT TO TOKEN SERVER");
            yield break;
        }

        TokenObject tokenInfo = JsonUtility.FromJson<TokenObject>(request.downloadHandler.text);

        Debug.Log("FetchToken Success: " + tokenInfo.rtcToken.ToString());
        SetAgoraToken(tokenInfo.rtcToken);
    }

    public void SetAgoraToken(string value)
    {
        TOKEN = value;
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME);
    }

    public void InitEngine()
    {
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        mRtcEngine.EnableAudio();
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        
        CameraCapturerConfiguration config = new CameraCapturerConfiguration();
        config.preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_AUTO;
#if !UNITY_EDITOR
        config.cameraDirection = CAMERA_DIRECTION.CAMERA_REAR;
#endif
        mRtcEngine.SetCameraCapturerConfiguration(config);
        mRtcEngine.SetExternalVideoSource(true, false);
        
        //Callback Handlers
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnUserJoined = OnUserJoinedHandler;
        mRtcEngine.OnUserOffline = OnUserOfflineHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
    }



    #region CALLBACKS
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        Debug.Log($"OnJoinChannelSuccess: uid = {uid}");

        localUserId = uid;

        monoProxy = cameraManager.gameObject.GetComponent<MonoBehaviour>();

        GameObject childVideo = GetChildVideoLocation(uid);
        childVideo.transform.localScale = Vector3.zero;

        VideoSurface videoSurface = MakeImageVideoSurface();
        streamingVid = true;
    }

    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        Debug.Log("OnUserJoined");
    }

    private void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (go != null)
            Destroy(go);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        Debug.Log("OnLeaveChannelSuccess");
        GameObject go = GameObject.Find(localUserId.ToString());
        if (go != null)
            Destroy(go);
    }

    void OnSDKWarningHandler(int warn, string msg)
    {
        Debug.LogWarning(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }

    void OnSDKErrorHandler(int error, string msg)
    {
        Debug.LogError(string.Format("OnSDKError error: {0}, msg: " + msg, error, msg));
    }
    #endregion








    private static GameObject GetChildVideoLocation(uint uid)
    {
        // find a game object to render video stream from 'uid'
        GameObject childVideo = GameObject.Find(uid.ToString());
        //GameObject childVideo = canvas.transform.Find($"{uid}")?.gameObject;

        if (childVideo == null)
        {
            childVideo = new GameObject($"{uid}");
            childVideo.transform.parent = GameObject.Find("VideoCanvas").transform;
        }

        return childVideo;
    }

    public VideoSurface MakeImageVideoSurface()
    {
        GameObject go = new GameObject(localUserId.ToString());
        go.AddComponent<RawImage>();
        go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        var rectTransform = go.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(100f,100f);
        rectTransform.localPosition = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);

        rectTransform.localRotation = new Quaternion(0, rectTransform.localRotation.y,
            -180.0f, rectTransform.localRotation.w);

        return go.AddComponent<VideoSurface>();
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (streamingVid == false)
            return;

        CaptureARBuffer();
    }

    // Get Image from the AR Camera, extract the raw data from the image 
    private unsafe void CaptureARBuffer()
    {
        // Get the image in the ARSubsystemManager.cameraFrameReceived callback

        XRCpuImage image;
        if (!cameraManager.TryAcquireLatestCpuImage(out image))
        {
            Debug.LogWarning("Capture AR Buffer returns nothing!!!!!!");
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the full image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2
            outputDimensions = new Vector2Int(image.width, image.height),

            // Color image format
            outputFormat = ConvertFormat,

            // Flip across the x axis
            transformation = XRCpuImage.Transformation.MirrorX

            // Call ProcessImage when the async operation completes
        };
        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        Debug.Log("OnCameraFrameReceived, size == " + size + "w:" + image.width + " h:" + image.height + " planes=" + image.planeCount);


        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so we can dispose of the CameraImage. We must do this or it will leak resources.

        byte[] bytes = buffer.ToArray();
        StartCoroutine(PushFrame(bytes, image.width, image.height, () => { image.Dispose(); buffer.Dispose(); }));
    }

    // Push frame to the remote client
    IEnumerator PushFrame(byte[] bytes, int width, int height, System.Action onFinish)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Zero bytes found!!!!");
            yield break;
        }

        IRtcEngine rtc = IRtcEngine.QueryEngine();
        //if the engine is present
        if (rtc != null)
        {
            //Create a new external video frame
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();

            //Set the buffer type of the video frame
            externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;

            // Set the video pixel format
            //externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;
            externalVideoFrame.format = PixelFormat;

            //apply raw data you are pulling from the rectangle you created earlier to the video frame
            externalVideoFrame.buffer = bytes;

            //Set the width of the video frame (in pixels)
            externalVideoFrame.stride = width;

            //Set the height of the video frame
            externalVideoFrame.height = height;

            //Remove pixels from the sides of the frame
            //externalVideoFrame.cropLeft = 10;
            //externalVideoFrame.cropTop = 10;
            //externalVideoFrame.cropRight = 10;
            //externalVideoFrame.cropBottom = 10;

            //Rotate the video frame (0, 90, 180, or 270)
            externalVideoFrame.rotation = 90;

            // increment i with the video timestamp
            externalVideoFrame.timestamp = i++;

            //Push the external video frame with the frame we just created
            int a = rtc.PushVideoFrame(externalVideoFrame);
            Debug.Log(" pushVideoFrame(" + i + ") size:" + bytes.Length + " => " + a);

        }
        yield return null;
        onFinish();
    }
}
