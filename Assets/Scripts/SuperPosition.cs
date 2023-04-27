using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SuperPosition<T>
{
    List<T> possibleValues= new List<T>();
    bool observed { get { return possibleValues.Count == 1; } }

    public SuperPosition(List<T> possibleValues)
    {
        foreach(T value in possibleValues) { this.possibleValues.Add(value); }
    }

    public bool isObserved() { return observed; }
    public bool isImpossible() { return possibleValues.Count == 0;}
    public T GetObservedValue() { return possibleValues[0]; }
    public T Observe()
    {
        
        float totalWeight = 0;
        foreach(T value in possibleValues)
        {
            totalWeight += ((Proto.IWeighted)value).GetWeight();
        }
        float chooseWeight = Random.Range(0f,totalWeight);
        float countWeight = 0;
        T chosenValue = default(T);
        for (int i = 0; i < possibleValues.Count; i++)
        {
            countWeight += ((Proto.IWeighted)possibleValues[i]).GetWeight();
            if (countWeight >= chooseWeight)
            {
                chosenValue = possibleValues[i];
                possibleValues = new List<T> { chosenValue };
                return GetObservedValue();
            }
        }
        //T chosenValue = possibleValues[possibleValues.Count-1];
        //T chosenValue = possibleValues[Random.Range(0,possibleValues.Count)];
        possibleValues = new List<T> { chosenValue };
        return GetObservedValue();
    }
    public int NumValues()
    {
        return possibleValues.Count;
    }
    public void RemoveValue(T value)
    {
        possibleValues.Remove(value);
    }

    public List<T> GetPossibleValues() { return possibleValues; }
}
