using UnityEngine;

[CreateAssetMenu(fileName = "AllDasoku", menuName = "ScriptableObjects/AllDasoku")]
public class AllDasokuState : ScriptableObject
{
    public DasokuStateObject[] dasokuKinds;

    [System.Serializable]
    public struct DASOKUFORMS
    {
        public PlayerData.PLAYER_MODE kind; //形態の種類
        [Tooltip("出現確率")]
        public int probability;             //出現確率
    }

    public DASOKUFORMS[] dasokuForms;
}
