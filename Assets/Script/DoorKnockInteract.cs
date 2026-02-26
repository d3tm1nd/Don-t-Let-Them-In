using UnityEngine;
using System.Collections;

public class DoorKnockInteract : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    public AudioSource voiceSource;
    public AudioClip knockSound;
    public AudioClip[] voiceClips;

    [Header("Timing")]
    public float voiceDelay = 1.5f;
    public float knockCooldown = 2.0f;

    private bool isBusy = false;

    public void Interact()
    {
        if (isBusy) return;

        StartCoroutine(KnockSequence());
    }

    IEnumerator KnockSequence()
    {
        isBusy = true;

        // เคาะ
        if (knockSound != null)
        {
            voiceSource.PlayOneShot(knockSound);
            Debug.Log("เคาะประตู");
        }

        // หน่วงก่อนพูด
        yield return new WaitForSeconds(voiceDelay);

        // เสียงพูด
        if (voiceClips.Length > 0)
        {
            AudioClip clip =
                voiceClips[Random.Range(0, voiceClips.Length)];

            voiceSource.PlayOneShot(clip);
            Debug.Log("มีเสียงพูดจากหลังประตู");
        }

        // คูลดาวน์
        yield return new WaitForSeconds(knockCooldown);

        isBusy = false;
    }
}
