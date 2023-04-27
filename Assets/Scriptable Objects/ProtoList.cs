using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProtoList", menuName = "ScriptableObjects/ProtoList", order = 1)]
public class ProtoList : ScriptableObject
{
    public List<Proto> protos;
}
