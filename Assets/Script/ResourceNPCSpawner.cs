using UnityEngine;
using System.Collections;

public class ResourceNPCSpawner : MonoBehaviour
{
    [Header("Outside NPC")]
    public GameObject outsideNPCPrefab;
    public Transform outsideSpawnPoint;

    [Header("Next Outside NPC")]
    public GameObject nextOutsideNPCPrefab;

    [Header("Spawn Settings")]
    public int maxNPCs = 5;
    public float spawnDelay = 3f;

    [Header("Stranger Settings")]
    public GameObject strangerPrefab;   // 🔥 FIX: เพิ่ม field นี้เพื่อแก้ CS0103
    [Range(0f, 100f)]
    public float strangerChance = 30f;

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

        GameObject npc = Instantiate(outsideNPCPrefab, outsideSpawnPoint.position, outsideSpawnPoint.rotation);
        npc.GetComponent<ResourceDoorNPC>().spawner = this;

        spawnedCount++;
        if (knockSound != null) audioSource.PlayOneShot(knockSound);
    }

    public IEnumerator SpawnNewOutsideNPC()
    {
        if (spawnedCount >= maxNPCs) yield break;

        yield return new WaitForSeconds(spawnDelay);

        GameObject prefabToSpawn = nextOutsideNPCPrefab != null ? nextOutsideNPCPrefab : outsideNPCPrefab;  // 🔥 FIX: เปลี่ยนชื่อเป็น prefabToSpawn

        // Stranger Logic
        if (Random.value * 100f < strangerChance && strangerPrefab != null)
        {
            prefabToSpawn = strangerPrefab;
        }

        GameObject npc = Instantiate(prefabToSpawn, outsideSpawnPoint.position, outsideSpawnPoint.rotation);
        npc.GetComponent<ResourceDoorNPC>().spawner = this;

        spawnedCount++;
        if (knockSound != null) audioSource.PlayOneShot(knockSound);
    }
}