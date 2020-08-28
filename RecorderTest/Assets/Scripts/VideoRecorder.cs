using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.ReplayKit;

public class VideoRecorder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_statusText;
    //[SerializeField] private Text m_RecordButtonText;

    public Action<string, string> OnVideoSaved = delegate { };

    #region Unity Lifecycle

    private void Start()
    {
        ReplayKitManager.Initialise();
    }

    private void OnEnable()
    {
        SetStatus("Registered for ReplayKit Callbacks");
        ReplayKitManager.DidInitialise += DidInitialiseEvent;
        ReplayKitManager.DidRecordingStateChange += DidRecordingStateChangeEvent;

        IsAPIAvailable();

        SaveSystem.Init();
    }

    private void OnDisable()
    {
        SetStatus("Unloaded ReplayKit Callbacks");
        ReplayKitManager.DidInitialise -= DidInitialiseEvent;
        ReplayKitManager.DidRecordingStateChange -= DidRecordingStateChangeEvent;
    }

    #endregion

    #region Functions

    public void StartRecording()
    {

        ReplayKitManager.StartRecording(enableMicrophone: true);
        SetStatus("Started recording");
        //m_RecordButtonText.text = "Stop Recording";
    }

    public void StopRecording()
    {
        ReplayKitManager.StopRecording();
        SetStatus("Stopped recording");

        //m_RecordButtonText.text = "Record Call";
    }

    public void Preview()
    {
        ReplayKitManager.Preview();
    }

    public void Discard()
    {
        bool success = ReplayKitManager.Discard();

        SetStatus("Discard  Recording: " + (success ? "Success" : "Failed"));
    }

    public void Save()
    {
        string filePath = GetPreviewFilePath();
        Debug.Log(filePath);

        string[] substrings = filePath.Split('/');
        string fileName = substrings[substrings.Length - 1];
        Debug.Log(fileName);

        string fileNewPath = SaveSystem.SAVE_FOLDER + fileName;
        Debug.Log(fileNewPath);

        //Save video to folder
        byte[] b = File.ReadAllBytes(filePath);
        File.WriteAllBytes(fileNewPath, b);
        if (File.Exists(fileNewPath))
        {
            Debug.Log("File exists");
        }
        else
        {
            Debug.Log("File doesn't exist");
        }

        string listFilePath = SaveSystem.SAVE_TXT;

        SaveSystem.Save(fileName, fileNewPath);

        if (File.Exists(fileNewPath))
        {
            Debug.Log("Successfully saved video to " + fileNewPath);
        }
        else
        {
            Debug.Log("Failed saving video");
        }

        OnVideoSaved?.Invoke(fileName, fileNewPath);
    }

    #endregion

    #region Query

    private string GetPreviewFilePath()
    {
        string filePath = ReplayKitManager.GetPreviewFilePath();
        SetStatus("Recorded video preview file path : " + filePath);

        return filePath;
    }

    public bool IsRecording()
    {
        bool isRecording = ReplayKitManager.IsRecording();
        SetStatus("Is currently recording : " + isRecording);
        return isRecording;
    }

    public void IsAPIAvailable()
    {
        bool isAvailable = ReplayKitManager.IsRecordingAPIAvailable();
        SetStatus("Recording API Available : " + isAvailable);
    }

    public void IsPreviewAvailable()
    {
        bool isPreviewAvailable = ReplayKitManager.IsPreviewAvailable();
        SetStatus("Is preview available : " + isPreviewAvailable);
    }

    #endregion

    #region Callbacks

    private void DidInitialiseEvent(ReplayKitInitialisationState state, string message)
    {
        Debug.Log("Received Event Callback : DidInitialiseEvent [State:" + state.ToString() + " " + "Message:" + message);

        switch (state)
        {
            case ReplayKitInitialisationState.Success:
                SetStatus("ReplayKitManager.DidInitialiseEvent : Initialisation Success");
                break;
            case ReplayKitInitialisationState.Failed:
                SetStatus("ReplayKitManager.DidInitialiseEvent : Initialisation Failed with message[" + message + "]");
                break;
            default:
                SetStatus("Unknown State");
                break;
        }
    }

    private void DidRecordingStateChangeEvent(ReplayKitRecordingState state, string message)
    {
        Debug.Log("Received Event Callback : DidRecordingStateChangeEvent [State:" + state.ToString() + " " + "Message:" + message);

        switch (state)
        {
            case ReplayKitRecordingState.Started:
                SetStatus("ReplayKitManager.DidRecordingStateChangeEvent : Video Recording Started");
                break;
            case ReplayKitRecordingState.Stopped:
                SetStatus("ReplayKitManager.DidRecordingStateChangeEvent : Video Recording Stopped");
                break;
            case ReplayKitRecordingState.Failed:
                SetStatus("ReplayKitManager.DidRecordingStateChangeEvent : Video Recording Failed with message [" + message + "]");
                break;
            case ReplayKitRecordingState.Available:
                SetStatus("ReplayKitManager.DidRecordingStateChangeEvent : Video Recording available for preview");

                IsPreviewAvailable();
                Save();

                break;

            default:
                SetStatus("Unknown State");
                break;
        }
    }

    #endregion

    #region UI

    private void SetStatus(string message)
    {
        Debug.Log("[ReplayKit] " + message);
        if (m_statusText != null)
        {
            m_statusText.text = message;
        }
    }

    #endregion

    

}
