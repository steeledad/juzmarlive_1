using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class DownloadAndPlayAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public bool harmlessFetchJob = false;
    public string filePath = "file:///C:/Users/steel/Downloads/test.mp3";

    private string fetchJobUrl;

    void Start()
    {
        if (harmlessFetchJob)
        {
            fetchJobUrl = "http://34.227.81.173:3000/harmless_fetch_job";
        }
        else
        {
            fetchJobUrl = "http://34.227.81.173:3000/fetch_job";
        }

        StartCoroutine(RunCoroutinesSequentially());
    }

    IEnumerator RunCoroutinesSequentially()
    {
        string jobId = null;
        int conversationCount = 0;

        using (UnityWebRequest jobRequest = UnityWebRequest.Get(fetchJobUrl))
        {
            yield return jobRequest.SendWebRequest();

            if (jobRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(jobRequest.error);
            }
            else
            {
                string response = jobRequest.downloadHandler.text;
                JSONNode jsonNode = JSON.Parse(response);
                jobId = jsonNode["job_id"];
                conversationCount = jsonNode["conversation"].Count;
            }
        }

        if (jobId != null)
        {
            for (int i = 2; i < conversationCount; i += 2)
            {
                string downloadAudioUrl = $"http://34.227.81.173:3000/fetch_audio?job_id={jobId}&index={i}";
                yield return StartCoroutine(DownloadAndPlayAudioClip(downloadAudioUrl));
                yield return new WaitForSeconds(audioSource.clip.length + 0.5f);
            }
        }
    }

    IEnumerator DownloadAndPlayAudioClip(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string localPath = filePath.Substring(8);
                Debug.Log("localPath: " + localPath);

                File.WriteAllBytes(localPath, www.downloadHandler.data);
                Debug.Log("Audio saved at: " + localPath);

                string audioFilePath = "file://" + localPath.Replace('\\', '/');
                Debug.Log("audioFilePath: " + audioFilePath);
                yield return StartCoroutine(LoadAndPlayAudio(audioFilePath));
            }
        }
    }

    IEnumerator LoadAndPlayAudio(string path)
    {
        using (WWW www = new WWW(path))
        {
            yield return www;
            audioSource.clip = www.GetAudioClip(false, true, AudioType.MPEG);
            audioSource.Play();
        }
    }
}