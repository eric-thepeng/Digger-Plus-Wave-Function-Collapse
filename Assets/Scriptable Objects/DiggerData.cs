using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiggerData", menuName = "ScriptableObjects/DiggerData", order = 1)]
public class DiggerData : ScriptableObject
{
    public float stepWidth;
    public float stepDepth;

    public int maxNumberOfDiggers;

    public float turnLeftProbability;
    public float turnRightProbability;
    public float turnAroundProbability;

    public float roomProbability;
    public int roomWidthMax;
    public int roomWidthMin;
    public int roomHeightMax;
    public int roomHeightMin;

    public float spawnNewDiggerBaseProbability;
    public float deleteDiggerBaseProbability;
    public AnimationCurve spawnNewDiggerProbabilityCurve;
    public AnimationCurve deleteDiggerProbabilityCurve;
}
