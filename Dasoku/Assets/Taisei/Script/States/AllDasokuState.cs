using UnityEngine;

[CreateAssetMenu(fileName = "AllDasoku", menuName = "ScriptableObjects/AllDasoku")]
public class AllDasokuState : ScriptableObject
{
    public DasokuStateObject[] dasokuKinds;
}
