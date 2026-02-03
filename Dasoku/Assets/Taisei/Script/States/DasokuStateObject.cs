using UnityEngine;

[CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/DasokuState")]
public class DasokuStateObject : ScriptableObject
{
    /// <summary>
    /// 状態変化の種類
    /// </summary>
    public enum DASOKUSTATE_KINDS
    {
        addKey,                 //カギ追加
        dasokuDelete,           //蛇足(状態変化)解除
        speedChange,            //移動速度倍率変更
        meterSpeedChenge,       //蛇足ゲージの増加速度変更
        addBody,                //胴体プラス
        teleport,               //ランダムワープ
        fanfare,                //ファンファーレ追加
        changeBGM,              //BGM変化
        decoration,             //デコレーション追加
        addEffect,              //エフェクト追加
        miss,                   //はずれ
    }
    
    /// <summary>
    /// 蛇足解除の種類
    /// </summary>
    public enum DASOKUSTATE_DELETE
    {
        one,    //1つ
        all     //全部
    }

    /// <summary>
    /// 移動速度倍率の種類
    /// </summary>
    public enum DASOKUSTATE_MOVESPEED
    {
        up,
        up_more,
        up_most,
        down,
        down_more
    }

    /// <summary>
    /// メーター速度倍率の種類
    /// </summary>
    public enum DASOKUSTATE_METERSPEED
    {
        up,
        up_more,
        down
    }

    /// <summary>
    /// 胴体追加の種類
    /// </summary>
    public enum DASOKUSTATE_ADDBODY
    {
        add_1,
        add_2,
        add_3
    }

    [System.Serializable]
    /// <summary>
    /// 共通変数
    /// </summary>
    public struct COMMONS
    {
        [Tooltip("状態変化の種類")]
        public DASOKUSTATE_KINDS kinds; //状態変化の種類
        [Tooltip("状態変化の名前")]
        public string name;             //状態変化の名前
        [TextArea(3, 5)]
        [Tooltip("状態変化の説明")]
        public string explanation;      //状態変化の説明
        [Tooltip("1度出現したかどうか")]
        public bool isAppearance;       //1度出現したかどうか
        [Tooltip("出現確率")]
        public int probability;         //出現確率
    }
    [Header("共通項目")]
    //共通変数の構造体
    public COMMONS commons;

    [Header("個別項目"), Tooltip("カギプレハブ")]
    //カギブレハブ
    public GameObject KeyPrefab;

    [Header("個別項目"), Tooltip("蛇足解除の種類")]
    //蛇足解除の種類
    public DASOKUSTATE_DELETE deleteType;

    [Header("個別項目"), Tooltip("移動速度倍率の種類")]
    public DASOKUSTATE_MOVESPEED moveType;

    [Header("個別項目"), Tooltip("メーター速度倍率の種類")]
    public DASOKUSTATE_METERSPEED meterType;

    [Header("個別項目"), Tooltip("胴体追加の種類")]
    public DASOKUSTATE_ADDBODY addBodyType;

    [Tooltip("移動速度倍率")]
    //移動速度倍率
    public float changeMoveSpeed = 1;

    [Tooltip("メーター速度倍率")]
    //メーター速度倍率
    public float changeMeterSpeed = 1;

    [Tooltip("追加する胴体の数")]
    //追加する胴体の数
    public int addBodyNum = 0;
}
