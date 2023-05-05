using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GenerationManager : MonoBehaviour
{
    static GenerationManager instance;
    public static GenerationManager i
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GenerationManager>();
            }
            return instance;
        }
    }

    public TMP_Text statusText;

    public DiggerData diggerData;
    public GameObject placeholderPrefab;
    public float tileSize = 5;
    public int mapSize;

    public Dictionary<Vector2Int, GameObject> allTilesMap = new Dictionary<Vector2Int, GameObject>();

    public ProtoList origionalPortos;

    public bool visualizeWFC = true;
    public int visualizeSpeed = 1;

    private List<Vector2Int> buildCompleted = new List<Vector2Int>();
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            DiggerManager.i.BeginGeneration(mapSize);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Digger.numOfDiggers = 1;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            WFCManager.i.BeginGeneration(new List<Vector2Int>(allTilesMap.Keys));
        }
    }

    public bool AddNewPlaceholder(Vector2Int newCoord)
    {
        if (allTilesMap.ContainsKey(newCoord)) return false;
        allTilesMap[newCoord] = Instantiate(placeholderPrefab, new Vector3(tileSize * newCoord.x, 0, tileSize * newCoord.y), Quaternion.identity);
        allTilesMap[newCoord].transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        allTilesMap[newCoord].transform.position -= new Vector3(0, 1, 0);
        return true;
    }

    public void AddedNewTile(Vector2Int newCoord, float choicesLeft, int totalChoices, GameObject newGO)
    {
        if ((int)choicesLeft == 1 )
        {
            GameObject toDestroy = allTilesMap[newCoord];
                Destroy(toDestroy.gameObject);
                allTilesMap[newCoord] = newGO;
                newGO.transform.parent = this.transform;
        }
        /*
        if ((int)choicesLeft == 1 )
        {
            if (!buildCompleted.Contains(newCoord))
            {
                GameObject toDestroy = allTilesMap[newCoord];
                Destroy(toDestroy);
                allTilesMap[newCoord] = newGO;
                buildCompleted.Add(newCoord);
            }
        }
        else
        {
            allTilesMap[newCoord].gameObject.transform.localScale = new Vector3(tileSize, tileSize, tileSize) * (choicesLeft/totalChoices);
        }*/

        //allTilesMap[newCoord].transform.position -= new Vector3(0, 1, 0);
    }

    public void ChangeCurrentTile(Vector2Int newCoord, float choicesLeft, int totalChoices, GameObject newGO)
    {
        if ((int)choicesLeft == 1)
        {
            GameObject toDestroy = allTilesMap[newCoord];
            Destroy(toDestroy);
            allTilesMap[newCoord] = newGO;
        }
        else
        {
            allTilesMap[newCoord].gameObject.transform.localScale = new Vector3(tileSize, tileSize, tileSize) * (choicesLeft/totalChoices);
        }

        //allTilesMap[newCoord].transform.position -= new Vector3(0, 1, 0);
    }

    /*
    IEnumerator GenerationProcess()
    {
        if(DiggerManager.i.state == DiggerManager.GenerationState.NotStarted)
        {
            if(Input.GetKeyDown(KeyCode.R)) { }
        }
        DiggerManager.i.BeginGeneration(200);
        yield return new WaitForEndOfFrame();
    }*/

    public void UpdateText(string newText)
    {
        statusText.text = newText;
    }

    public void BuildNavMesh()
    { 
        GetComponent<AIArea>().BuildNavMesh();
    }
}
