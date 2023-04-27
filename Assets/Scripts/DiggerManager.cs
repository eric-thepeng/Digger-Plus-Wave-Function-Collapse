using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiggerManager : MonoBehaviour
{
    static DiggerManager instance;
    public static DiggerManager i
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DiggerManager>();
            }
            return instance;
        }
    }

    public GameObject diggerPrefab;
    List<GameObject> allDiggers = new List<GameObject>();

    public enum GenerationState { NotStarted, InProcess, Complete }
    public GenerationState state = GenerationState.NotStarted;
    int maxNumFloors;

    public TMP_Text statusText;

    List<Vector2Int> generatedTiles = new List<Vector2Int>();

    public bool AddNewTile(Vector2Int toCheck)
    {
        return GenerationManager.i.AddNewPlaceholder(toCheck);
    }
    public void BeginGeneration(int manNumFloors)
    {
        this.maxNumFloors = manNumFloors;
        Digger.totalFloors = 0;
        generatedTiles.Clear();
        Instantiate(diggerPrefab, Vector3.zero, Quaternion.identity);
        state = GenerationState.InProcess;
    }

    void EndGeneration()
    {
        for (int i = allDiggers.Count - 1; i >= 0; i--)
        {
            if (allDiggers[i] != null) Destroy(allDiggers[i]);
        }
        state = GenerationState.Complete;
    }

    public void DiggerAdded(GameObject go)
    {
        allDiggers.Add(go);
    }

    void Update()
    {
        if (state == GenerationState.InProcess)
        {
            float percentDone = Digger.totalFloors / (float)maxNumFloors;
            statusText.text = "Generating ("
                + Mathf.FloorToInt(percentDone * 100)
                + "%) Diggers: " + Digger.totalDiggers
                + "\nPress R to Reload";

            // We're done generating after we exceed the max number of floors
            if (Digger.totalFloors >= maxNumFloors)
            {
                EndGeneration();
            }
        }
        else if (state == GenerationState.Complete)
        {
            statusText.text = "Digger Done \n Press T to Perform WFC;";
        }
        else if (state == GenerationState.NotStarted)
        {
            statusText.text = "Ready to Start \n Press R to Perform Digger";
        }
    }
}
