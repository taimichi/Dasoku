using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LotteryData", menuName = "ScriptableObjects/LotteryData")]
public class LotteryData : ScriptableObject
{
    public const string PATH = "LotteryData";
    private static LotteryData _lotteryEntity;
    public static LotteryData LotteryEntity
    {
        get
        {
            if (_lotteryEntity == null)
            {
                _lotteryEntity = Resources.Load<LotteryData>(PATH);
                if (_lotteryEntity == null)
                {
                    Debug.LogError(PATH + " not found");
                }
            }
            return _lotteryEntity;
        }
    }

    //配列を作ったかどうか
    public bool isLotteryCreate = false;

    //形態か状態かの種類
    public enum FORM_OR_DASOKU
    {
        form,
        dasoku
    }

    //形態か状態かの配列の要素数
    private const int form_dasoku_Num = 10;

    [Header("形態か状態か")]
    //形態か状態かを決める機構用の配列
    public FORM_OR_DASOKU[] form_dasoku = new FORM_OR_DASOKU[form_dasoku_Num];

    //形態の出る割合
    public const int formNum = 3;
    //状態の出る割合
    public const int dasokuNum = 7;

    [Header("形態変化の抽選配列")]
    //形態変化の抽選機構用の配列
    public List<PlayerData.PLAYER_MODE> form_lotterys = new List<PlayerData.PLAYER_MODE>();

    //形態変化の要素数
    public int numElements_Form = 0;

    [Header("状態変化の抽選配列")]
    //状態変化の抽選機構用の配列
    public List<DasokuStateObject.DASOKUSTATE_KINDS> dasoku_lotterys = new List<DasokuStateObject.DASOKUSTATE_KINDS>();

    //状態変化の要素数
    public int numElements_dasoku = 0;

    //足形態　配列の要素数　５分の１の確率でヒール形態 trueがヒール形態
    public const int footLottery_num = 5;
    [Header("特殊抽選がある形態変化")]
    public bool[] footLottery = new bool[footLottery_num];

    //蛇手形態　配列の要素数　５分の１の確率で剛腕形態 trueが剛腕形態
    public const int handLottery_num = 5;
    public bool[] handLottery = new bool[handLottery_num];

    [Header("種類がある状態変化")]
    //蛇足解除
    public List<DasokuStateObject.DASOKUSTATE_DELETE> Dels = new List<DasokuStateObject.DASOKUSTATE_DELETE>();
    [System.NonSerialized]
    public DasokuStateObject.DASOKUSTATE_DELETE selectDel;
    //移動速度
    public List<DasokuStateObject.DASOKUSTATE_MOVESPEED> MoveSpeeds = new List<DasokuStateObject.DASOKUSTATE_MOVESPEED>();
    [System.NonSerialized]
    public DasokuStateObject.DASOKUSTATE_MOVESPEED selectMove;
    //メーター速度
    public List<DasokuStateObject.DASOKUSTATE_METERSPEED> MeterSpeeds = new List<DasokuStateObject.DASOKUSTATE_METERSPEED>();
    [System.NonSerialized]
    public DasokuStateObject.DASOKUSTATE_METERSPEED selectMeter;
    //胴体追加
    public List<DasokuStateObject.DASOKUSTATE_ADDBODY> Bodys = new List<DasokuStateObject.DASOKUSTATE_ADDBODY>();
    [System.NonSerialized]
    public DasokuStateObject.DASOKUSTATE_ADDBODY selectBody;


}
