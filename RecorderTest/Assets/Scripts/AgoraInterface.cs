using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Android;

// Screen sharing doc: https://docs.agora.io/en/Video/screensharing_unity?platform=Unity
// Agora Unity Api reference: https://docs.agora.io/en/Video/API%20Reference/unity/index.html

public class AgoraInterface : MonoBehaviour
{
    //PUT YOUR AGORA APP ID HERE:
    public string m_appID = "APP ID, DO NOT LOSE";

    public GameObject m_videoDisplay_1;
    public GameObject m_videoDisplay_2;

    public bool isConnected = false;

    public Action OnConnectedToChannel = delegate { };
    public Action OnSupportConnected = delegate { };
    public Action OnSupportOffline = delegate { };

    //The magic variable that makes all the Agora stuff happen
    public IRtcEngine m_rtcEngine;

    private uint m_userId;
    private uint m_SupportId;

    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private ArrayList permissionList = new ArrayList();
    #endif

    private void Awake()
    {
    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            permissionList.Add(Permission.Microphone);
            permissionList.Add(Permission.Camera);
    #endif
    }

    private void Update()
    {
        CheckPermissions();
    }

    /// <summary>
    ///   Checks for platform dependent permissions.
    /// </summary>
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Debug.Log("Requesting permission for " + permission);
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    


    //Initialises the Agora Engine
    public void LoadEngine(string appID)
    {
        if (m_rtcEngine != null)
        {
            Debug.Log("Engine exists. Please unload it first!");
            return;
        }

        //Init engine
        m_rtcEngine = IRtcEngine.GetEngine(appID);
    }


    //Call this method to join a channel
    public void Join(string channel)
    {
        LoadEngine(m_appID);

        Debug.Log("Joining Channel: " + channel);

        //Setting callback methods
        m_rtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        m_rtcEngine.OnLeaveChannel = OnLeaveChannel;
        m_rtcEngine.OnUserJoined = OnUserJoined;
        m_rtcEngine.OnUserOffline = OnUserOffline;

        //Enable video
        m_rtcEngine.EnableVideo();
        m_rtcEngine.EnableVideoObserver();

        //Join channel
        m_rtcEngine.JoinChannel(channel, null, 0);
    }

    public void Leave()
    {
        Debug.Log("calling leave");

        if (m_rtcEngine == null)
            return;

        //DestroyVideoSurface(m_userId.ToString());
        //DestroyVideoSurface(m_SupportId.ToString());

        DestroyAllVideoSurface();

        // leave channel
        m_rtcEngine.LeaveChannel();
        // deregister video frame observers in native-c code
        m_rtcEngine.DisableVideoObserver();

        UnloadEngine();
    }

    private void DestroyAllVideoSurface()
    {
        VideoSurface[] obj1 = m_videoDisplay_1.GetComponentsInChildren<VideoSurface>();
        VideoSurface[] obj2 = m_videoDisplay_2.GetComponentsInChildren<VideoSurface>();

        foreach(VideoSurface v in obj1)
        {
            Destroy(v.gameObject);
        }
        foreach(VideoSurface v in obj2)
        {
            Destroy(v.gameObject);
        }
    }

    VideoSurface MakeRawImageSurface(string go_name, GameObject parent)
    {
        GameObject go = new GameObject();

        if (go == null)
        {
            return null;
        }

        go.name = go_name;
        go.transform.SetParent(parent.transform);

        //Set transform
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        
        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(0, 0, 180);
        go.transform.rotation = rotation;

        go.AddComponent<RawImage>();

        RectTransform copiedRectTransform = parent.GetComponent<RectTransform>();
        RectTransform rectTransform = go.GetComponent<RectTransform>();

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        //rectTransform.anchoredPosition = copiedRectTransform.anchoredPosition;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        
        

        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
        
    }


    //Disconnects Agora engine
    public void UnloadEngine()
    {
        Debug.Log("calling unloadEngine");

        // delete
        if (m_rtcEngine != null)
        {
            IRtcEngine.Destroy();
            m_rtcEngine = null;
        }
    }

    public void EnableVideo(bool pauseVideo)
    {
        if (m_rtcEngine != null)
        {
            if (!pauseVideo)
            {
                m_rtcEngine.EnableVideo();
            }
            else
            {
                m_rtcEngine.DisableVideo();
            }
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (!ReferenceEquals(m_rtcEngine, null))
        {
            EnableVideo(paused);
        }
    }


    //VERY IMPORTANT
    private void OnApplicationQuit()
    {
        UnloadEngine();
    }


    //CALLBACK METHODS
    //Gets called when YOU successfully join a channel
    private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        //Inform LiveSupportInterface that the user is joined to the channel
        OnConnectedToChannel?.Invoke();

        m_userId = uid;

        Debug.Log("Successfully joined channel " + uid.ToString());

        VideoSurface videoSurface = MakeRawImageSurface(uid.ToString(), m_videoDisplay_1);

        videoSurface.EnableFilpTextureApply(true, false);
        videoSurface.SetGameFps(60);

        //m_videoDisplay_1.AddComponent<VideoSurface>();

        SwitchCamera();
    }

    public void SwitchCamera()
    {
        m_rtcEngine.SwitchCamera();
    }

    private void OnLeaveChannel(RtcStats rtcStats)
    {
        Debug.Log("Successfully leaved channel: " + rtcStats);
        
    }


    //Gets called when ANOTHER user joins the SAME channel that you are in
    private void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log("User has joined, " + uid.ToString());

        m_SupportId = uid;

        //Inform LiveSupportInterface that the support is connected
        OnSupportConnected?.Invoke();

        VideoSurface videoSurface = MakeRawImageSurface(uid.ToString(), m_videoDisplay_2);
        if (!ReferenceEquals(videoSurface, null))
        {
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurface.SetGameFps(60);
        }
        else
        {
            Debug.Log("Make videoSurface failed");
        }
    }

    private void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        Debug.Log(string.Format("User {0} offline, reason: {1}", uid.ToString(), reason));
        DestroyVideoSurface(uid.ToString());
        OnSupportOffline?.Invoke();
    }

    private void DestroyVideoSurface(string name)
    {
        GameObject obj = GameObject.Find(name);

        if (!ReferenceEquals(obj, null))
        {
            Destroy(obj);
        }
    }

}
