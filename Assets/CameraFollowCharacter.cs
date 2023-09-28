using UnityEngine;

public class CameraFollowCharacter : MonoBehaviour
{
    public DownloadAndPlayAudio downloadAndPlayAudioScript;
    public float smoothSpeed = 5f;
    private GameObject targetCharacter;

    void Update()
    {
        if (downloadAndPlayAudioScript != null)
        {
            string characterName = downloadAndPlayAudioScript.CurrentCharacter;

            if (characterName != null &&
                downloadAndPlayAudioScript.characterMap.ContainsKey(characterName) &&
                downloadAndPlayAudioScript.characterMap[characterName] != null)
            {
                targetCharacter = downloadAndPlayAudioScript.characterMap[characterName];
            }
        }

        if (targetCharacter != null)
        {
            Vector3 targetPosition = targetCharacter.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
    }
}