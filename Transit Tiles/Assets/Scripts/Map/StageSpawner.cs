using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSpawner : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] Transform initialSpawnPoint;
    //[SerializeField] Transform spawnerStageSectionSpawnPoint;

    [Header("Prefabs")]
    [SerializeField] public GameObject stageSectionPrefab;
    [SerializeField] public List<GameObject> stationPrefabs = new List<GameObject>();

    [Header("Floats")]
    [SerializeField] public float destroySectionDelay = 3f;
    //[SerializeField] float stagePositionX = -2.595f;

    [Header("Integers")]
    [SerializeField] public int stageSectionsPassed = 0;
    [SerializeField] public int maxStageSectionsToPass = 3;

    [Header("Vectors")]
    [SerializeField] private Vector3 stagePosition = new Vector3(-29.2f, -2.2f, -0.7f);

    private void Start()
    {
        GameManager.instance.StageSpawner = this;

/*        if (!spawnerStageSectionSpawnPoint)
            spawnerStageSectionSpawnPoint = initialSpawnPoint;*/

        StartStage();
    }

    public IEnumerator DestroyStageSection(GameObject stageObj)
    {
        Debug.Log("Destroying: " + stageObj.name);

        yield return new WaitForSeconds(destroySectionDelay);

        Destroy(stageObj);
        Debug.Log("Stage has been destroyed");
    }

    public void SpawnStagePrefab(StageSectionEnd stageSectionEnd, Transform spawnPoint, GameObject stageSection)
    {
        GameObject newStageSection = Instantiate(stageSection, spawnPoint.position, Quaternion.Euler(-89.98f, spawnPoint.rotation.eulerAngles.y, spawnPoint.rotation.eulerAngles.z));

        stageSectionEnd = newStageSection.GetComponentInChildren<StageSectionEnd>(); //Gets the StageSectionEnd from the newStageSection
        if (stageSectionEnd != null)
        {
            //spawnerStageSectionSpawnPoint = stageSectionEnd.GetNextSpawnPoint();
        }
        else
        {
            Debug.LogWarning("Spawned stage section has no StageSectionEnd component!");
        }

        Debug.Log("Spawned Next Stage Section");
    }

    public void StartStage()
    {
        GameObject startingStageSection = Instantiate(stationPrefabs[0] /*the first station*/, new Vector3(stagePosition.x, stagePosition.y, stagePosition.z), Quaternion.Euler(-89.98f, initialSpawnPoint.rotation.eulerAngles.y, initialSpawnPoint.rotation.eulerAngles.z));

        //spawnerStageSectionSpawnPoint = startingStageSection.GetComponentInChildren<StageSectionEnd>().GetNextSpawnPoint();

        Debug.Log("Spawned Starting Stage");
    }
}
