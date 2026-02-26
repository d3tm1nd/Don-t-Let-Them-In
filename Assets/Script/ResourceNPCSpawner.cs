using UnityEngine;
using System.Collections;

public class ResourceNPCSpawner : MonoBehaviour
{
    [Header("Outside NPC (ตัวแรก)")]
    public GameObject outsideNPCPrefab;
    public Transform outsideSpawnPoint;

    [Header("Next Outside NPC (ตัวใหม่)")]
    public GameObject nextOutsideNPCPrefab;

    [Header("Spawn Limits & Delays")]
    [Tooltip("จำนวนสูงสุดของ NPC ที่ spawn")]
    public int maxNPCs = 5;
    [Tooltip("Delay ก่อน spawn NPC ตัวใหม่")]
    public float spawnDelay = 3f;

    [Header("Audio")]
    public AudioClip knockSound;

    AudioSource audioSource;

    private int spawnedCount = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        StartCoroutine(SpawnOutside());
    }

    IEnumerator SpawnOutside()
    {
        yield return new WaitForSeconds(1f);

        GameObject npc = Instantiate(
            outsideNPCPrefab,
            outsideSpawnPoint.position,
            outsideSpawnPoint.rotation
        );

        ResourceDoorNPC doorNPC = npc.GetComponent<ResourceDoorNPC>();
        if (doorNPC != null)
        {
            doorNPC.spawner = this;
        }

        spawnedCount++;
        Debug.Log($"✅ Spawn Outside NPC (Initial) | Count: {spawnedCount}/{maxNPCs}");

        if (knockSound != null)
            audioSource.PlayOneShot(knockSound);
    }

    public IEnumerator SpawnNewOutsideNPC()
    {
        if (spawnedCount >= maxNPCs)
        {
            Debug.Log($"🚫 ถึง limit NPC ({maxNPCs}) แล้ว!");
            yield break;
        }

        yield return new WaitForSeconds(spawnDelay);

        GameObject prefabToSpawn = nextOutsideNPCPrefab != null ? nextOutsideNPCPrefab : outsideNPCPrefab;

        GameObject npc = Instantiate(
            prefabToSpawn,
            outsideSpawnPoint.position,
            outsideSpawnPoint.rotation
        );

        ResourceDoorNPC doorNPC = npc.GetComponent<ResourceDoorNPC>();
        if (doorNPC != null)
        {
            doorNPC.spawner = this;
        }

        spawnedCount++;
        Debug.Log($"✅ Spawn New Outside NPC | Count: {spawnedCount}/{maxNPCs}");

        if (knockSound != null)
            audioSource.PlayOneShot(knockSound);
    }
}