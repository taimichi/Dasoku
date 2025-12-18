using System.Collections.Generic;
using UnityEngine;

//プレイヤーの見た目用スクリプト
[RequireComponent(typeof(LineRenderer))]
public class SnakeVisual : MonoBehaviour
{
    [Header("基本設定")]
    public Transform head;              //蛇の頭
    public float pointSpacing = 0.1f;   //どのくらいの感覚で点を記録するか
    public float maxLength = 10f;       //頭から尻尾までの物理的な長さ
    public float moveSmooth = 0.5f;     //位置更新の補間係数（滑らかさ）

    private LineRenderer lineRenderer;
    //軌跡の点列 最新が頭、古いのがしっぽ
    private List<Vector3> points = new List<Vector3>();
    private float distSinceLast = 0f;   //

    private Vector3 plOffset = new Vector3(0, 0, 0);
    [SerializeField] private float downDistance = 0.3f;

    private float nowLength = 0f; 

    public SnakeController controller; //プレイヤーコントローラースクリプト
    public SnakeBody plBody;

    void Start()
    {
        //LineRendererを取得
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        nowLength = maxLength;

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
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, head.position);
            return;
        }

        #region 必要なくなったが念のため残しておく
        ////↓頭からしっぽまでの長さ計算
        ////頭と1つ目の胴体まで
        //float totalDis = Vector3.Distance(head.position, plBody.segments[0].position);
        //for (int i = 0; i < plBody.segments.Count - 1; i++)
        //{
        //    //各胴体からしっぽまで
        //    totalDis += Vector3.Distance(plBody.segments[i].position, plBody.segments[i + 1].position);
        //}
        ////↑長さ計算ここまで

        ////長さを調整
        //if(totalDis >= maxLength)
        //{
        //    totalDis = maxLength;
        //}
        //nowLength = totalDis;
        #endregion

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
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
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
