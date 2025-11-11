using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//プレイヤーの見た目用スクリプト
[RequireComponent(typeof(LineRenderer))]
public class PlayerVisual : MonoBehaviour
{
    [Header("基本設定")]
    public Transform head;              //蛇の頭
    public float pointSpacing = 0.1f;   //どのくらいの感覚で点を記録するか
    public float maxLength = 10f;       //頭から尻尾までの物理的な長さ
    public float moveSmooth = 0.5f;     //位置更新の補間係数（滑らかさ）

    private LineRenderer lr;
    //軌跡の点列 最新が頭、古いのがしっぽ
    private List<Vector3> points = new List<Vector3>();
    private float distSinceLast = 0f;   //

    private Vector3 plOffset = new Vector3(0, 0, 0);
    [SerializeField] private float downDistance = 0.3f;

    public PlayerController controller; //プレイヤーコントローラースクリプト

    void Awake()
    {
        //LineRendererを取得
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;

        //プレイヤーの胴体の長さを取得
        maxLength = controller.ReturnBodyNum();
    }

    void Update()
    {
        plOffset = controller.ReturnPlayerDownVec() * downDistance;

        // --- 点追加 ---
        if (points.Count == 0)
        {
            points.Add(head.position);
            lr.positionCount = 1;
            lr.SetPosition(0, head.position);
            return;
        }

        #region 前の(10/21)
        ////プレイヤーが回転中の時
        //if (controller.ReturnIsRot())
        //{
        //    //↓頭からしっぽまでの長さ計算
        //    //頭と1つ目の胴体まで
        //    float totalDis = Vector3.Distance(head.position, controller.segments[0].position);
        //    for (int i = 0; i < controller.segments.Count - 1; i++)
        //    {
        //        //各胴体からしっぽまで
        //        totalDis += Vector3.Distance(controller.segments[i].position, controller.segments[i + 1].position);
        //    }
        //    //↑長さ計算ここまで

        //    //長さを調整
        //    maxLength = totalDis;
        //}
        //else
        //{
        //    //長さを初期のものに戻す
        //    maxLength = startMaxLength;
        //}
        #endregion

        //↓頭からしっぽまでの長さ計算
        //頭と1つ目の胴体まで
        float totalDis = Vector3.Distance(head.position, controller.segments[0].position);
        for (int i = 0; i < controller.segments.Count - 1; i++)
        {
            //各胴体からしっぽまで
            totalDis += Vector3.Distance(controller.segments[i].position, controller.segments[i + 1].position);
        }
        //↑長さ計算ここまで

        //長さを調整
        maxLength = totalDis;


        //前回の動いた点から現在の位置までの距離を計算
        distSinceLast += Vector3.Distance(points[points.Count - 1], head.position);
        //pointSpacing以上動いたら
        if (distSinceLast >= pointSpacing)
        {
            //新しい点を追加
            points.Add(head.position + plOffset);
            distSinceLast = 0f;
        }

        // --- 長さ制限 ---
        TrimToMaxLength();

        // --- LineRenderer更新 ---
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }

    /// <summary>
    /// 長さの制限
    /// </summary>
    void TrimToMaxLength()
    {
        // 総距離を計算
        float totalDist = 0f;
        for (int i = points.Count - 1; i > 0; i--)
        {
            //最後の点(頭側)から順に距離を計算
            totalDist += Vector3.Distance(points[i], points[i - 1]);
            //全体の長さが最大値を超えたら
            if (totalDist > maxLength)
            {
                //超過距離
                // ここで尻尾をカットする
                float overDist = totalDist - maxLength;

                // 残すべき位置を線分上で補間して尻尾の位置を調整
                Vector3 tail = Vector3.Lerp(points[i - 1], points[i],
                                            overDist / Vector3.Distance(points[i], points[i - 1]));

                // 古い点を削除
                points.RemoveRange(0, i - 1);
                points[0] = tail;
                return;
            }
        }
    }

    // オプション：他スクリプトから点列を参照できるように
    public List<Vector3> GetPoints() => points;
}
