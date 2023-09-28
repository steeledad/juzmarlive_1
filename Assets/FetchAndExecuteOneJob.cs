using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;


public class DownloadAndPlayAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public string CurrentCharacter { get; private set; }

    public bool harmlessFetchJob = false;
    public string filePath = "file:///C:/Users/steel/Downloads/test.mp3";
    public Dictionary<string, GameObject> characterMap = new Dictionary<string, GameObject>();
    public Camera mainCamera;
    public Vector3 defaultRotation;

    private string fetchJobUrl;

    void Start()
    {
        InitializeCharacterMap();
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

    void Update()
    {
        if (!audioSource.isPlaying && captions.text != "")
        {
            captions.text = "";
        }
    }

    private void InitializeCharacterMap()
    {
        // Add your character GameObject references here, use names like "spongebob" as keys
        characterMap.Add("spongebob", GameObject.Find("SpongeBob"));
        characterMap.Add("patrick", GameObject.Find("patrick"));
        characterMap.Add("joe", GameObject.Find("rogan"));
        characterMap.Add("andrew", GameObject.Find("tate"));
        // Add more characters if needed...
    }
    IEnumerator RunCoroutinesSequentially()
    {
        string jobId = null;
        string[] conversations = new string[100];
        string[] characters = new string[100];
        string[] transcripts = new string[100];

        int conversationCount = 0;

        using (UnityWebRequest jobRequest = UnityWebRequest.Get(fetchJobUrl))
        {
            yield return jobRequest.SendWebRequest();

            if (jobRequest.result != UnityWebRequest.Result.Success)
            {
                // Debug.LogError(jobRequest.error);
                Debug.LogError("Job Request Error" + jobRequest.error);
            }
            else
            {
                string response = jobRequest.downloadHandler.text;
                JSONNode jsonNode = JSON.Parse(response);
                jobId = jsonNode["job_id"];
                conversationCount = jsonNode["conversation"].Count;
                for (int i = 0; i < conversationCount; i++)
                {
                    conversations[i] = jsonNode["conversation"][i]["content"];
                }
                int count = 0;
                Debug.Log("conversationCount: " + conversationCount);
                for (int i = 2; i < conversationCount; i += 2)
                {
                    Debug.Log("conversations[" + i + "]: " + conversations[i]);
                    bool conversation_formatting_correct = conversations[i].Contains(" to ");
                    Debug.Log("conversation_formatting_correct: " + conversation_formatting_correct);
                    if (conversation_formatting_correct)
                    {
                        characters[count] = conversations[i].Split(' ')[0].Substring(1);
                        if (conversations[i].Contains("]"))
                        {
                            transcripts[count] = conversations[i].Split(']')[1];
                        }
                        else
                        {
                            transcripts[count] = "BAD TEXT TRANSCRIPT";
                        }
                    }
                    else
                    {
                        characters[count] = "unknown";
                        transcripts[count] = conversations[i];
                    }
                    count++;
                }
                for (int i = 0; i < count; i++)
                {
                    Debug.Log("characters[" + i + "]: " + characters[i]);
                    Debug.Log("transcripts[" + i + "]: " + transcripts[i]);
                }
            }
        }

        if (jobId != null)
        {
            for (int i = 2; i < conversationCount; i += 2)
            {
                string downloadAudioUrl = $"http://34.227.81.173:3000/fetch_audio?job_id={jobId}&index={i}";
                yield return StartCoroutine(DownloadAndPlayAudioClip(downloadAudioUrl));
                Debug.Log("characters length: " + characters.Length);
                // Debug.log(characters[i / 2])
                Debug.Log("characters[" + (i / 2 - 1) + "]: " + characters[(i / 2 - 1)]);

                CurrentCharacter = characters[(i / 2 - 1)];
                // Debug.Log(CurrentCharacter);
                Debug.Log("CurrentCharacter: " + CurrentCharacter);


                yield return new WaitForSeconds(audioSource.clip.length + 0.5f);
            }
        }
    }

    IEnumerator DownloadAndPlayAudioClip(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            Debug.Log("url: " + url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                // Debug.LogError(www.error);
                Debug.LogError("Audio Request Error" + www.error);
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