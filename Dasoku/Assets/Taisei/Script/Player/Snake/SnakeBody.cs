using System.Collections.Generic;
using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    private PlayerData PlData;

    [Header("胴体関連")]
    [Tooltip("胴体隙間")] public float segmentSpacing = 0.5f;

    [Tooltip("胴体の更新距離")] public float pointSpacing = 0.04f;

    [Tooltip("胴体のプレハブ")] public Transform bodyPrefab;

    //胴体のプレハブの親オブジェクト
    public Transform BodysObjct;

    //生成した胴体のリスト
    public List<Transform> segments = new List<Transform>();

    //座標の履歴
    private List<Vector3> posHistory = new List<Vector3>();

    //回転値の履歴
    private List<Vector3> rotHistory = new List<Vector3>();

    //スケール値の履歴
    private List<Vector3> sizeHistory = new List<Vector3>();

    /// <summary>
    /// 保持する履歴の最大数
    /// </summary>
    private int MaxHistoryCount
    {
        get
        {
            //胴体が追従する距離分 + 少し余裕
            return (segments.Count + 10) * Mathf.RoundToInt(segmentSpacing / pointSpacing);
        }
    }

    private bool isAllBodyGroundCheck = false;

    private void Start()
    {
        PlData = PlayerData.Instance;

        //開始時の座標と回転値を履歴に追加
        posHistory.Add(this.transform.position);
        rotHistory.Add(this.transform.localEulerAngles);
        sizeHistory.Add(this.transform.localScale);

        //初期設定の胴体の数生成
        for (int i = 0; i < PlData.bodyNum; i++)
        {
            //胴体の生成
            AddSegment();

            //最後の胴体追加の時
            if (i == PlData.bodyNum - 1)
            {
                //最後の胴体はしっぽのため、名前変更
                segments[i].name = "Tail";
            }
        }
    }

    void Update()
    {
        //胴体の見た目更新
        RecordPosition();
        UpdateSegments();
    }

    /// <summary>
    /// 座標、回転値の保存
    /// </summary>
    private void RecordPosition()
    {
        //胴体が空中にあるかチェック
        for (int i = 0; i < segments.Count; i++)
        {
            Body body = segments[i].GetComponent<Body>();
            if (!body.CheckBodyGround())
            {
                //1つでも空中にあった場合、ループから抜け出す
                isAllBodyGroundCheck = false;
                break;
            }
            isAllBodyGroundCheck = true;
        }

        //胴体が全て地上と接しているとき
        if (isAllBodyGroundCheck)
        {
            //保存
            //最後の履歴と現在の頭の距離が更新距離を超えた時
            if (Vector3.Distance(posHistory[posHistory.Count - 1], this.transform.position) > pointSpacing)
            {
                //座標の履歴追加
                posHistory.Add(this.transform.position);
                //回転値の履歴追加
                rotHistory.Add(this.transform.localEulerAngles);
                //スケール値の履歴追加
                sizeHistory.Add(this.transform.localScale);

                //メモリ対策用:古い履歴の削除
                //履歴の最大数を取得
                int maxHistory = MaxHistoryCount;
                //履歴の数が最大数を超えた時
                if (posHistory.Count > maxHistory)
                {
                    //それぞれの古い履歴削除
                    posHistory.RemoveAt(0);
                    rotHistory.RemoveAt(0);
                    sizeHistory.RemoveAt(0);
                }
            }
        }
        //胴体が1つでも空中にある時
        else
        {
            //座標の履歴追加
            posHistory.Add(this.transform.position);
            //回転値の履歴追加
            rotHistory.Add(this.transform.localEulerAngles);
            //スケール値の履歴追加
            sizeHistory.Add(this.transform.localScale);

            //メモリ対策用:古い履歴の削除
            //履歴の最大数を取得
            int maxHistory = MaxHistoryCount;
            //履歴の数が最大数を超えた時
            if (posHistory.Count > maxHistory)
            {
                //それぞれの古い履歴削除
                posHistory.RemoveAt(0);
                rotHistory.RemoveAt(0);
                sizeHistory.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 胴体の更新
    /// </summary>
    private void UpdateSegments()
    {
        //胴体同士がどれくらい離れているか計算
        int stepsPerSeg = Mathf.RoundToInt(segmentSpacing / pointSpacing);

        for (int i = 0; i < segments.Count; i++)
        {
            //頭が通過した履歴のどの地点の座標を参照するか
            int index = Mathf.Max(0, posHistory.Count - 1 - stepsPerSeg * (i + 1));
            //座標更新
            segments[i].position = posHistory[index];

            //頭が通過した履歴のどの地点の回転値を参照するか
            int rotIndex = Mathf.Max(0, rotHistory.Count - 1 - stepsPerSeg * (i + 1));
            //回転値更新
            segments[i].localEulerAngles = rotHistory[rotIndex];

            //頭が通過した履歴のどの地点のスケール値を参照するか
            int sizeIndex = Mathf.Max(0, sizeHistory.Count - 1 - stepsPerSeg * (i + 1));
            //スケール値更新
            segments[i].localScale = sizeHistory[sizeIndex];
        }
    }


    /// <summary>
    /// 胴体の追加
    /// </summary>
    public void AddSegment()
    {
        //胴体を生成
        Transform seg = Instantiate(bodyPrefab, transform.position, Quaternion.identity, BodysObjct);
        //胴体リストに追加
        segments.Add(seg);
    }
}
