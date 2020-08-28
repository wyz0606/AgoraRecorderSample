using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgoraTest : MonoBehaviour
{
    public GameObject m_MainPanel;
    public GameObject m_AgoraPanel;


    public Button m_ConnectButton;
    public Button m_EndCallButton;
    public Button m_RecordCall;
    public Button m_StopRecording;
    public Button m_IsRecording;
    public Button m_IsAPIAvailable;
    public Button m_IsPreviewAvailable;
    public Button m_Preview;

    public AgoraInterface m_Agora;
    public VideoRecorder m_videoRecorder;

    public GameObject m_VideoCallStep_1;
    public GameObject m_VideoCallStep_2;

    // Start is called before the first frame update
    void Start()
    {
        if (m_ConnectButton != null)
        {
            m_ConnectButton.onClick.AddListener(attemptToConnectLiveService);
        }
        if (m_EndCallButton != null)
        {
            m_EndCallButton.onClick.AddListener(endAgoraCall);
        }

        m_RecordCall.onClick.AddListener(RecordCall);
        m_StopRecording.onClick.AddListener(StopRecording);
        m_IsRecording.onClick.AddListener(CheckRecording);
        m_IsAPIAvailable.onClick.AddListener(CheckAPIAvaible);
        m_IsPreviewAvailable.onClick.AddListener(CheckPreviewAvailable);
        m_Preview.onClick.AddListener(Preview);
    }

    public void attemptToConnectLiveService()
    {
        //We try to connect, lets assume it works for now
        TurnOnAgora();
    }

    private void TurnOnAgora()
    {
        m_Agora.Join("Public Channel");
        turnOnAgoraScreen();
        OnSupportConnected();
        m_Agora.OnConnectedToChannel += SuccessfullyConnectedToChannel;
        m_Agora.OnSupportConnected += SuccessfullyConnectedToSupport;
        m_Agora.OnSupportOffline += OnSupportDisconnected;
    }

    private void TurnOffAgora()
    {
        m_Agora.Leave();
        m_Agora.OnConnectedToChannel -= SuccessfullyConnectedToChannel;
        m_Agora.OnSupportConnected -= SuccessfullyConnectedToSupport;
        m_Agora.OnSupportOffline -= OnSupportDisconnected;
    }

    private void SuccessfullyConnectedToChannel()
    {
        //m_VideoCallStep_1.SetActive(true);
        //m_VideoCallStep_2.SetActive(false);

        turnOnAgoraScreen();
    }

    private void SuccessfullyConnectedToSupport()
    {

        //OnSupportConnected();
        //m_RecordCall.onClick.AddListener(RecordCall);
    }

    private void OnSupportConnected()
    {
        m_VideoCallStep_1.SetActive(false);
        m_VideoCallStep_2.SetActive(true);
    }

    private void OnSupportDisconnected()
    {
        m_VideoCallStep_1.SetActive(true);
        m_VideoCallStep_2.SetActive(false);
    }

    private void endAgoraCall()
    {
        OnSupportDisconnected();

        TurnOffAgora();

        //Logic to end the call
        turnOffAgoraScreen();
    }

    private void turnOnAgoraScreen()
    {
        m_MainPanel.gameObject.SetActive(false);
        m_AgoraPanel.gameObject.SetActive(true);
    }

    private void turnOffAgoraScreen()
    {
        m_MainPanel.gameObject.SetActive(true);
        m_AgoraPanel.gameObject.SetActive(false);
    }

    private void RecordCall()
    {
        m_RecordCall.gameObject.SetActive(false);
        m_StopRecording.gameObject.SetActive(true);
        Debug.Log("Trying to start recording");
        m_videoRecorder.StartRecording();
    }

    private void StopRecording()
    {
        Debug.Log("Trying to stop recording");
        m_videoRecorder.StopRecording();
        m_RecordCall.gameObject.SetActive(true);
        m_StopRecording.gameObject.SetActive(false);
    }

    private void CheckRecording()
    {
        m_videoRecorder.IsRecording();
    }

    private void CheckAPIAvaible()
    {
        m_videoRecorder.IsAPIAvailable();
    }

    private void CheckPreviewAvailable()
    {
        m_videoRecorder.IsPreviewAvailable();
    }

    private void Preview()
    {
        m_videoRecorder.Preview();
    }
}
