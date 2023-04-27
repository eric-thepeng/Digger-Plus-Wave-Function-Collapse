using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Digger : MonoBehaviour {

    //Static variables ------------------
    public static int totalDiggers = 0;     // Static variable to keep track of the number of diggers in the scene.
    public static int totalFloors = 0;      // How many floor tiles that have been spawned (note: needs to be reset manually, usually by the a manager script).
    public static int numOfDiggers = 1;

    //Default ----------------
    private Vector2Int currentCoord = new Vector2Int(0, 0);
    private Vector2Int currentDirection = new Vector2Int(0, 1);
    private DiggerData diggerData { get { return GenerationManager.i.diggerData; } }

    void Awake() {
        totalDiggers++;
        DiggerManager.i.DiggerAdded(this.gameObject);
    }

    void OnDestroy() {
        totalDiggers--;
    }


    public void StampCorridor(Vector2Int floorCoord)
    {
        MaybeStampFloor(floorCoord);
        MaybeStampFloor(floorCoord + TurnClockWise(currentDirection,1));
        MaybeStampFloor(floorCoord + TurnClockWise(currentDirection, 3));
    }
    
    public void MaybeStampFloor(Vector2Int floorCoord) {
        if (DiggerManager.i.AddNewTile(floorCoord)) {
            totalFloors++;
        }
    }

    void Update() {
        // Stamp or Corridor
        if (Random.value <= diggerData.roomProbability)
        {
            for (int xOffset = 0; xOffset < diggerData.roomWidth; xOffset++)
            {
                for (int yOffset = 0; yOffset < diggerData.roomDepth; yOffset++)
                {
                    Vector2Int floorPosition = currentCoord + new Vector2Int(xOffset, yOffset);
                    MaybeStampFloor(floorPosition);
                }
            }
        }
        else
        {
            StampCorridor(currentCoord);
        }

        //move
        currentCoord += currentDirection;

        //rotate
        float randomNumber = Random.value;
        if (randomNumber <= diggerData.turnLeftProbability) { //turn left
            currentDirection = TurnClockWise(currentDirection, 3);
        }
        else if (randomNumber <= diggerData.turnLeftProbability + diggerData.turnRightProbability) { //turn right
            currentDirection = TurnClockWise(currentDirection, 2);
        }
        else if (randomNumber <= diggerData.turnLeftProbability + diggerData.turnRightProbability + diggerData.turnAroundProbability) { //turn around
            currentDirection = TurnClockWise(currentDirection, 1);
        }
        SpawnOrDeleteDiggers();
    }

    public Vector2Int TurnClockWise(Vector2Int orgCoord, int amount)
    {
        while (amount > 0)
        {
            if (orgCoord == new Vector2Int(0, 1)) orgCoord += new Vector2Int(1, -1);
            else if (orgCoord == new Vector2Int(1, 0)) orgCoord += new Vector2Int(-1, -1);
            else if (orgCoord == new Vector2Int(0, -1)) orgCoord += new Vector2Int(-1, 1); 
            else if (orgCoord == new Vector2Int(-1, 0)) orgCoord += new Vector2Int(1, 1); 
            amount--;
         }
        return orgCoord;
    }


    public void SpawnOrDeleteDiggers() {
        float roll = Random.Range(0.01f, 1f);
         if( numOfDiggers != diggerData.maxNumberOfDiggers && roll < diggerData.spawnNewDiggerProbabilityCurve.Evaluate(0.1f * numOfDiggers)) //spawn digger
        {
            numOfDiggers++;
            GameObject go = Instantiate(this.gameObject);
            print("spawn digger, now have " + numOfDiggers);
            print("roll is: " + roll + " chance is " + diggerData.spawnNewDiggerProbabilityCurve.Evaluate(0.1f * numOfDiggers));
        }
        else if (roll < diggerData.spawnNewDiggerProbabilityCurve.Evaluate(0.1f * numOfDiggers) + diggerData.deleteDiggerProbabilityCurve.Evaluate(0.1f * numOfDiggers)) //delete digger
        {
            numOfDiggers--;
            print("delete digger, now have " + numOfDiggers);
            Destroy(this.gameObject);
        }
    }
}
