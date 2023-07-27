using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using agora_gaming_rtc;
using agora_utilities;
using System; //Action
using UnityEngine;
using UnityEngine.Networking;
public class AgoraVideoStreamer : MonoBehaviour
{
    public enum RenderWhichUser
    {
        AR,
        VR //Use the VR mode to test your own local camera. By right we only want to render the iPhone's video
    }

    //Agora stream setup variables
    public string APP_ID = "3ea96cd2c2e84aa7a91c3f5d86cafa82";
    public string TOKEN = "";
    public string CHANNEL_NAME = "ADIXR";
    public AgoraVideoSurfaceType SURFACE_TYPE = AgoraVideoSurfaceType.Renderer; //Sets whether it as a 3D screen or on the UI
    public RenderWhichUser renderUser = RenderWhichUser.AR;

    public uint localUserID;

    private IRtcEngine mRtcEngine = null;

    //Screen Size Variables
    private const float Offset = 100;

    private void Start()
    {
        //The mesh renderer component is just there to position the screen correctly so we can disable it
        GetComponent<MeshRenderer>().enabled = false;

        InitEngine();
        StartCoroutine(FetchToken());
    }

    void InitEngine()
    {
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
       // mRtcEngine.SetLogFile("log.txt");  //Can uncomment this if you want to create a log.txt file
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);

        if (renderUser == RenderWhichUser.VR)
        {
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }
        else if (renderUser == RenderWhichUser.AR)
        {
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
        }

        mRtcEngine.EnableAudio();
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();

        //Callback Handlers
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnUserJoined += OnUserJoinedHandler;
        mRtcEngine.OnUserOffline += OnUserOfflineHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
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
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
    }




    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        Debug.Log(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
        localUserID = uid;
        if (renderUser == RenderWhichUser.VR) 
            CreateVideoView(localUserID);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        Debug.Log("OnLeaveChannelSuccess");
        if (renderUser == RenderWhichUser.VR)
            DestroyVideoView(localUserID);
    }

    //For users other than us
    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        Debug.Log(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        if (renderUser == RenderWhichUser.AR && uid != localUserID)
            CreateVideoView(uid);
    }

    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        Debug.Log(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int)reason));
        DestroyVideoView(uid);
    }

    void OnSDKWarningHandler(int warn, string msg)
    {
        Debug.LogWarning(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }

    void OnSDKErrorHandler(int error, string msg)
    {
        Debug.LogError(string.Format("OnSDKError error: {0}, msg: " + msg, error, msg));
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

    /// <summary>
    /// When the use successfully logs to the channel. It will create an empty GO with the user id as the name
    /// We set the transform of that id to this object
    /// </summary>
    /// <param name="uid"></param>
    void CreateVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null)) //Check if the object exists
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        VideoSurface videoSurface;

        if (SURFACE_TYPE == AgoraVideoSurfaceType.Renderer)
            videoSurface = CreatePlaneSurface(uid.ToString());
        else
            videoSurface = CreateImageSurface(uid.ToString());

        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(SURFACE_TYPE);
        }

        videoSurface.transform.parent = this.transform;
        videoSurface.transform.localPosition = Vector3.zero;
        if (uid == localUserID)
            videoSurface.transform.localRotation = Quaternion.identity;
        else
            videoSurface.transform.localRotation = Quaternion.Euler(0, -90, 0);
        videoSurface.transform.localScale = Vector3.one;
    }

    // VIDEO TYPE 1: 3D Object
    public VideoSurface CreatePlaneSurface(string goName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        if (go == null)
        {
            return null;
        }
        go.name = goName;
        // set up transform
        //go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        //float yPos = Random.Range(3.0f, 5.0f);
        //float xPos = Random.Range(-2.0f, 2.0f);
        //go.transform.position = new Vector3(xPos, yPos, 0f);
        //go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

        // configure videoSurface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    // Video TYPE 2: RawImage
    public VideoSurface CreateImageSurface(string goName)
    {
        GameObject go = new GameObject();

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        // to be renderered onto
        go.AddComponent<RawImage>();

        GameObject canvas = GameObject.Find("VideoCanvas");
        if (canvas != null)
        {
            go.transform.SetParent(canvas.transform);
            Debug.Log("add video view");
        }
        else
        {
            Debug.Log("Canvas is null video view");
        }
        // set up transform
        go.transform.localPosition = new Vector3(0f, 0f, 0f);
        go.transform.localScale = new Vector3(3f, 4f, 1f);

        // configure videoSurface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    void DestroyVideoView(uint uid)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Destroy(go);
        }
    }
}
