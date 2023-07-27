using System.Collections;
using agora_gaming_rtc;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using static agora_gaming_rtc.ExternalVideoFrame;

public class VideoStreamer : MonoBehaviour
{
    public static VideoStreamer Instance { get { return FindObjectOfType<VideoStreamer>(); } }

    private IRtcEngine mRtcEngine;

    //Video streaming
    public ARCameraManager cameraManager;
    MonoBehaviour monoProxy;
    public static TextureFormat ConvertFormat = TextureFormat.BGRA32;
    public static VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;
    int i = 0; // monotonic timestamp counter

    //Agora login
    private string token;
    private int lastError;
    private uint localUserId;
    public uint LocalUserId
    {
        get => localUserId;
    }
    bool streamingVid = false;

    public void LoadEngine(string appId, string token = null)
    {
        this.token = token;

        if (mRtcEngine != null)
            return;

        mRtcEngine = IRtcEngine.GetEngine(appId);
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
        Debug.Log("Video Stream Loaded");
    }

    public void Join(string channel)
    {
        if (mRtcEngine == null) return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnWarning = (int warn, string msg) =>
        {
            Debug.LogWarning($"Warning code:{warn} msg:{IRtcEngine.GetErrorDescription(warn)}");
        };

        mRtcEngine.OnError = HandleError;
        mRtcEngine.EnableVideo();

        // allow camera output callback
        mRtcEngine.EnableVideoObserver();


        CameraCapturerConfiguration config = new CameraCapturerConfiguration();
        config.preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_AUTO;
#if !UNITY_EDITOR
        config.cameraDirection = CAMERA_DIRECTION.CAMERA_REAR;
#endif
        mRtcEngine.SetCameraCapturerConfiguration(config);
        mRtcEngine.SetExternalVideoSource(true, false);


        /*  This API Accepts AppID with token; by default omiting info and use 0 as the local user id */
        mRtcEngine.JoinChannelByKey(channelKey: token, channelName: channel);
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
    private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log($"OnJoinChannelSuccess: uid = {uid}");

        localUserId = uid;
        /*
        GameObject go = GameObject.Find("AR Camera");
        if (go != null)
        {
            cameraManager = go.GetComponent<ARCameraManager>();
        }*/
        monoProxy = cameraManager.gameObject.GetComponent<MonoBehaviour>();

        GameObject childVideo = GetChildVideoLocation(uid);

        VideoSurface videoSurface = MakeImageVideoSurface(childVideo);
#if !UNITY_EDITOR
        childVideo.transform.localScale = Vector3.zero;
#endif
        streamingVid = true;
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    private void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log("OnUserJoined");/*
        GameObject childVideo = GetChildVideoLocation(uid);

        // create a GameObject and assign to this new user
        VideoSurface videoSurface = MakeImageVideoSurface(childVideo);

        if (videoSurface != null)
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        }*/
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

        rectTransform.sizeDelta = new Vector2(VideoStreamSetup.Instance.windowWidth, VideoStreamSetup.Instance.windowHeight);
        rectTransform.localPosition = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);

        rectTransform.localRotation = new Quaternion(0, rectTransform.localRotation.y,
            -180.0f, rectTransform.localRotation.w);

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

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
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
        StartCoroutine(PushFrame(bytes, image.width, image.height,() => { image.Dispose(); buffer.Dispose(); }));
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
            externalVideoFrame.cropLeft = 10;
            externalVideoFrame.cropTop = 10;
            externalVideoFrame.cropRight = 10;
            externalVideoFrame.cropBottom = 10;
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
