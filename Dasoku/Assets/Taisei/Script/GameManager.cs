using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public AllDasokuState allDasoku;

    //タイルマップのコライダー
    public CompositeCollider2D[] composite;

    public bool isClear = false;

    public Transform[] randomPos;

    //抽選用データ
    private LotteryData lottery;

    [SerializeField] private MenuManager menuCanvas;
    public bool isMenu = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        //FPS値を６０で固定
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        isClear = false;

        //抽選機構を取得
        lottery = LotteryData.LotteryEntity;
        //抽選配列が創られていない場合
        if (!lottery.isLotteryCreate)
        {
            CreateLottery();
            lottery.isLotteryCreate = true;
        }

        menuCanvas.MenueClose();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //メニューが開いてる時
            if (isMenu)
            {
                menuCanvas.MenueClose();
            }
            //開いてないとき
            else
            {
                menuCanvas.MenuOpen();
            }
        }
    }

    /// <summary>
    /// 抽選用の配列作成
    /// </summary>
    private void CreateLottery()
    {
        int _fCount = 0;    //形態が出た回数
        int _dCount = 0;    //状態が出た回数
        //形態か状態かの配列作成
        for(int i = 0; i < lottery.form_dasoku.Length; i++)
        {
            //乱数生成　0=形態 / 1=状態
            int rand = Random.Range(0, 2);
            if(rand == 0)
            {
                //形態が出た数が規定の数以下の時
                if(_fCount < LotteryData.formNum)
                {
                    lottery.form_dasoku[i] = LotteryData.FORM_OR_DASOKU.form;
                    _fCount++;
                }
                else
                {
                    lottery.form_dasoku[i] = LotteryData.FORM_OR_DASOKU.dasoku;
                }
            }
            else if(rand == 1)
            {
                //状態が出た数が規定の数以下の時
                if (_dCount < LotteryData.dasokuNum)
                {
                    lottery.form_dasoku[i] = LotteryData.FORM_OR_DASOKU.dasoku;
                    _dCount++;
                }
                else
                {
                    lottery.form_dasoku[i] = LotteryData.FORM_OR_DASOKU.form;
                }
            }
        }

        //形態
        //既に要素がある時
        if(lottery.numElements_Form != 0)
        {
            lottery.numElements_Form = 0;
            lottery.form_lotterys = new List<PlayerData.PLAYER_MODE>();
        }
        //形態変化の抽選配列作成
        for(int i = 0; i < allDasoku.dasokuForms.Length; i++)
        {
            //要素数を計算
            lottery.numElements_Form += allDasoku.dasokuForms[i].probability;
            //抽選配列に要素を入れていく
            for (int j = 0; j < allDasoku.dasokuForms[i].probability; j++)
            {
                lottery.form_lotterys.Add(allDasoku.dasokuForms[i].kind);
            }
        }

        //状態
        //既に要素があるとき
        if(lottery.numElements_dasoku != 0)
        {
            //各要素をリセット
            lottery.numElements_dasoku = 0;
            lottery.dasoku_lotterys = new List<DasokuStateObject.DASOKUSTATE_KINDS>();

            lottery.Dels = new List<DasokuStateObject.DASOKUSTATE_DELETE>();
            lottery.MoveSpeeds = new List<DasokuStateObject.DASOKUSTATE_MOVESPEED>();
            lottery.MeterSpeeds = new List<DasokuStateObject.DASOKUSTATE_METERSPEED>();
            lottery.Bodys = new List<DasokuStateObject.DASOKUSTATE_ADDBODY>();
        }
        //状態変化の抽選配列作成
        for(int i = 0; i < allDasoku.dasokuKinds.Length; i++)
        {
            var data = allDasoku.dasokuKinds[i];   // 長い参照を短縮
            var common = data.commons;

            //要素数を計算
            lottery.numElements_dasoku += common.probability;
            //抽選配列に要素を入れていく
            for(int j = 0; j < common.probability; j++)
            {
                lottery.dasoku_lotterys.Add(common.kinds);
            }

            switch (common.kinds)
            {
                case DasokuStateObject.DASOKUSTATE_KINDS.dasokuDelete:
                    AddByProbability(lottery.Dels, data.deleteType, common.probability);
                    break;

                case DasokuStateObject.DASOKUSTATE_KINDS.speedChange:
                    AddByProbability(lottery.MoveSpeeds, data.moveType, common.probability);
                    break;

                case DasokuStateObject.DASOKUSTATE_KINDS.meterSpeedChenge:
                    AddByProbability(lottery.MeterSpeeds, data.meterType, common.probability);
                    break;

                case DasokuStateObject.DASOKUSTATE_KINDS.addBody:
                    AddByProbability(lottery.Bodys, data.addBodyType, common.probability);
                    break;
            }
        }

        //特殊抽選用
        //足形態
        int _heelCount = 0;
        for(int i = 0; i < LotteryData.footLottery_num; i++)
        {
            //乱数生成　0=足形態 / 1=ヒール形態
            int rand = Random.Range(0, 2);
            if(rand == 0)
            {
                lottery.footLottery[i] = false;
            }
            else
            {
                //ヒール形態出た回数
                if(_heelCount == 0)
                {
                    lottery.footLottery[i] = true;
                }
                else
                {
                    lottery.footLottery[i] = false;
                }
                _heelCount++;
            }
        }

        //蛇手形態
        int _armCount = 0;
        for (int i = 0; i < LotteryData.handLottery_num; i++)
        {
            //乱数生成　0=蛇手形態 / 1=剛腕形態
            int rand = Random.Range(0, 2);
            if (rand == 0)
            {
                lottery.handLottery[i] = false;
            }
            else
            {
                //剛腕形態出た回数
                if (_armCount == 0)
                {
                    lottery.handLottery[i] = true;
                }
                else
                {
                    lottery.handLottery[i] = false;
                }
                _armCount++;
            }
        }
    }
    /// <summary>
    /// 指定回数、同じ値をリストに追加する共通関数
    /// </summary>
    private void AddByProbability<T>(List<T> list, T value, int probability)
    {
        for (int i = 0; i < probability; i++)
        {
            list.Add(value);
        }
    }

    private void OnDrawGizmos()
    {
        if (composite == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        for(int n = 0; n < composite.Length; n++)
        {
            int pathCount = composite[n].pathCount;

            for (int i = 0; i < pathCount; i++)
            {
                int pointCount = composite[n].GetPathPointCount(i);
                Vector2[] points = new Vector2[pointCount];
                composite[n].GetPath(i, points);

                for (int p = 0; p < points.Length; p++)
                {
                    Gizmos.DrawSphere(points[p], 0.05f);
                }
            }
        }
    }

    /// <summary>
    /// 指定したワールド座標から最も近いコライダーの角（頂点）を返す
    /// </summary>
    public Vector2 GetNearestCorner(Vector2 targetPos)
    {
        //nullチェック
        if (composite == null)
        {
            return Vector2.zero;
        }

        // 最小距離を初期化（非常に大きい値にセット）
        float minDist = float.MaxValue;

        // 最も近い角を保持する変数
        Vector2 nearest = Vector2.zero;

        for(int n = 0; n < composite.Length; n++)
        {
            // CompositeCollider2D が保持するパス（ポリゴン）の数
            int pathCount = composite[n].pathCount;

            // すべてのパスをループ
            for (int i = 0; i < pathCount; i++)
            {
                // このパスが持つ頂点の数を取得
                int pointCount = composite[n].GetPathPointCount(i);

                // 頂点座標を格納する配列
                Vector2[] points = new Vector2[pointCount];

                //ワールド座標を取得
                composite[n].GetPath(i, points);

                // すべての頂点をループ
                for (int p = 0; p < points.Length; p++)
                {
                    // ターゲット位置との距離を計算
                    float dist = Vector2.Distance(targetPos, points[p]);

                    // 現在の距離が最も短ければ更新
                    if (dist < minDist)
                    {
                        minDist = dist;     // 最短距離を上書き
                        nearest = points[p];  // 最も近い角を記録
                    }
                }
            }
        }

        // 最も近かった角を返す
        return nearest;
    }
}
