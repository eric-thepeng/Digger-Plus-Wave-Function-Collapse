using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WFCManager : MonoBehaviour
{
    static WFCManager instance;
    public static WFCManager i
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WFCManager>();
            }
            return instance;
        }
    }
    Vector2Int LEFT { get { return new Vector2Int(-1, 0); } }
    Vector2Int RIGHT { get { return new Vector2Int(1, 0); } }
    Vector2Int FRONT { get { return new Vector2Int(0, -1); } }
    Vector2Int BACK { get { return new Vector2Int(0, 1); } }

    Dictionary<Vector2Int, SuperPosition<Proto.ProtoData>> map = new Dictionary<Vector2Int, SuperPosition<Proto.ProtoData>>();
    List<Proto.ProtoData> allProtoData = new List<Proto.ProtoData>();
    private int protoDataCount = 1;
    bool generating = false;
    private List<KeyValuePair<Vector2Int, int>> recordTrack = new List<KeyValuePair<Vector2Int, int>>();

    public void BeginGeneration(List<Vector2Int> allCoords)
    {
        GenerateProtos();
        int tries = 0;
        bool result = false;
        do
        {
            recordTrack.Clear();
            tries++;
            InitializeMap(allCoords);
            PropogateWalls();
            result = RunWFC();
        }
        while (result == false && tries < 50);
        if (result == false)
        {
            print("Unable to solve wave function collapse after " + tries + " tries.");
            BuildCurrent();
        }
        else
        {
            print("run amount " + tries);
            if (GenerationManager.i.visualizeWFC)
            {
                StartCoroutine(VisualizedBuilding());
            }
            else
            {
                BuildCurrent();
            }
        }
    }

    IEnumerator VisualizedBuilding()
    {
        int count = 0;
        int changePerFrame = GenerationManager.i.visualizeSpeed;
        while (count <= recordTrack.Count)
        {
            for (int i = count; (i < count+changePerFrame) && (i < recordTrack.Count); i++)
            {
                Vector2Int coord = recordTrack[i].Key;
                GameObject newGO = Instantiate(map[coord].GetObservedValue().prefab);
                newGO.transform.position = new Vector3(2 * coord.x, 0, 2 * coord.y);
                newGO.transform.Rotate(new Vector3(0, 1, 0), 90 * map[coord].GetObservedValue().rotationIndex);
                GenerationManager.i.AddedNewTile(recordTrack[i].Key, recordTrack[i].Value, protoDataCount, newGO);// map[recordTrack[i].Key].GetObservedValue().prefab);
            }

            count += changePerFrame;
            yield return new WaitForEndOfFrame();
        }
        //GenerationManager.i.BuildNavMesh();
    }

    void BuildCurrent()
    {
        foreach (KeyValuePair<Vector2Int, SuperPosition<Proto.ProtoData>> kvp in map)
        {
            GameObject newGO = null;
            if (kvp.Value.isObserved()) {
                newGO = Instantiate(kvp.Value.GetObservedValue().prefab);
                newGO.transform.position = new Vector3(2 * kvp.Key.x, 0, 2 * kvp.Key.y);
                newGO.transform.Rotate(new Vector3(0, 1, 0), 90 * kvp.Value.GetObservedValue().rotationIndex);
                GenerationManager.i.AddedNewTile(kvp.Key, kvp.Value.NumValues(),protoDataCount,newGO);
                //= newGO.transform.localEulerAngles + new Vector3(0, 90, 0) * kvp.Value.GetObservedValue().rotationIndex;                 newGO.transform.Rotate(0,90 * kvp.Value.GetObservedValue().rotationIndex, 0,Space.Self);//= newGO.transform.localEulerAngles + new Vector3(0, 90, 0) * kvp.Value.GetObservedValue().rotationIndex;
            } 
            else if (kvp.Value.isImpossible())
            {
                newGO = Instantiate(GenerationManager.i.placeholderPrefab);
                newGO.transform.position = new Vector3(2 * kvp.Key.x, 1, 2 * kvp.Key.y);
                newGO.transform.Rotate(new Vector3(0, 1, 0), 45);
            }
        }
    }

    bool RunWFC()
    {
        while (DoUnobservedNodesExist())
        {
            Vector2Int node = GetNextUnobservedNode();
            //PrintStatus();
            if (map[node].NumValues() == 0) return false;
            map[node].Observe();
            recordTrack.Add(new KeyValuePair<Vector2Int, int>(node,1));
            PropogateNeighbors(node);
        }
        return true;
    }

    bool DoUnobservedNodesExist()
    {
        foreach (KeyValuePair<Vector2Int, SuperPosition<Proto.ProtoData>> kvp in map)
        {
            if (!kvp.Value.isObserved()) return true;
        }
        return false;
    }

    Vector2Int GetNextUnobservedNode()
    {
        int minNumValues = allProtoData.Count+1;
        List<Vector2Int> unobservedNodes =  new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, SuperPosition<Proto.ProtoData>> kvp in map)
        {
            if (kvp.Value.isObserved()) continue;
            if (kvp.Value.NumValues() > minNumValues) continue;
            if (kvp.Value.NumValues() < minNumValues)
            {
                minNumValues= kvp.Value.NumValues();
                unobservedNodes.Clear();
            }
            unobservedNodes.Add(kvp.Key);
        }
        return unobservedNodes[Random.Range(0, unobservedNodes.Count)];
    }

    public void GenerateProtos()
    {
        allProtoData.Clear();
        foreach (Proto proto in GenerationManager.i.origionalPortos.protos)
        {
            foreach(Proto.ProtoData ppd in proto.GetAllProtoDataVariations())
            {
                allProtoData.Add(ppd);
            }
        }

        protoDataCount = allProtoData.Count;
    }

    private void PrintStatus()
    {
        foreach (KeyValuePair<Vector2Int, SuperPosition<Proto.ProtoData>> kvp in map)
        {
            print(kvp.Key);
            if(kvp.Value.isObserved()) print("=== observed === " + kvp.Value.GetObservedValue());
            else
            {
                print("--- not observed --- has " + kvp.Value.NumValues());
                foreach(Proto.ProtoData ppd in kvp.Value.GetPossibleValues())
                {
                    print(ppd);
                }
            }
        }
     }

    public void InitializeMap(List<Vector2Int> allCoords)
    {
        map.Clear();
        foreach(Vector2Int coord in allCoords)
        {
            map.Add(coord, new SuperPosition<Proto.ProtoData>(allProtoData));
        }
    }

     void PropogateWalls()
    {
        Proto.AdjacencySet wallASet = new Proto.AdjacencySet(new List<Proto.Adjacency>() { Proto.Adjacency.Wall }, new List<Proto.Adjacency>() { Proto.Adjacency.Wall });
        foreach (KeyValuePair<Vector2Int,SuperPosition<Proto.ProtoData>> kvp in map)
        {
            if (!map.ContainsKey(kvp.Key + LEFT)) {
                Propogate(wallASet, map[kvp.Key], LEFT,kvp.Key);
            }

            if (!map.ContainsKey(kvp.Key + RIGHT))
            {
                Propogate(wallASet, map[kvp.Key], RIGHT,kvp.Key);
            }

            if (!map.ContainsKey(kvp.Key + FRONT))
            {
                Propogate(wallASet, map[kvp.Key], FRONT,kvp.Key);
            }

            if (!map.ContainsKey(kvp.Key + BACK))
            {
                Propogate(wallASet, map[kvp.Key], BACK,kvp.Key);
            }

            if (kvp.Value.isObserved())
            {
                print(kvp.Key + " JUST OBSERVED" + kvp.Value.GetObservedValue());
                PropogateNeighbors(kvp.Key);
            }


        }
        
     }

    Vector2Int GetOppositeDir(Vector2Int inputDir)
    {
        if (inputDir == LEFT) return RIGHT;
        if (inputDir == RIGHT) return LEFT;
        if (inputDir == FRONT) return BACK;
        return FRONT;
    }

     void PropogateNeighbors(Vector2Int orgCoord)
    {
        PropogateTo(orgCoord, LEFT);
        PropogateTo(orgCoord, RIGHT);
        PropogateTo(orgCoord, FRONT);
        PropogateTo(orgCoord, BACK);
    }

    void PropogateTo(Vector2Int orgCoord, Vector2Int direction)
    {
        Vector2Int targetCoord = orgCoord + direction;
        if (!map.ContainsKey(targetCoord)) return;
        if (map[targetCoord].isObserved()) return;
        Propogate(map[orgCoord].GetObservedValue().GetAdjacencySetByDirection(direction),   map[targetCoord],   GetOppositeDir(direction), targetCoord) ;
        if (map[targetCoord].isObserved())
        {
            PropogateNeighbors(targetCoord);
        }
    }

    private void Propogate(Proto.AdjacencySet orgASet, SuperPosition<Proto.ProtoData> tarSP, Vector2Int tarDir, Vector2Int tarCoord)
    {
        int removeCount = 0;
        for (int i = tarSP.GetPossibleValues().Count - 1; i >= 0; i--)
        {
            removeCount = 0;
            if (!Proto.AdjacencySetMatch(orgASet, tarSP.GetPossibleValues()[i].GetAdjacencySetByDirection(tarDir)))
            {
                tarSP.RemoveValue(tarSP.GetPossibleValues()[i]);
                removeCount++;
            }
            //if(removeCount!= 0)recordTrack.Add(new KeyValuePair<Vector2Int, int>(tarCoord,tarSP.NumValues()));
            if (tarSP.isObserved()) recordTrack.Add(new KeyValuePair<Vector2Int, int>(tarCoord, tarSP.NumValues()));
        }
    }

}
