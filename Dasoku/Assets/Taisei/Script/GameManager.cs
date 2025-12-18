using UnityEngine;

public class GameManager : MonoBehaviour
{
    //タイルマップのコライダー
    public CompositeCollider2D composite;

    private void Update()
    {
        #region デバッグ用
        //形態変化
        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayerData.Instance.nowMode = PlayerData.PLAYER_MODE.snakeHand;
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            PlayerData.Instance.nowMode = PlayerData.PLAYER_MODE.normal;
        }

        //胴体の数変更
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            PlayerData.Instance.BodyNumChange(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PlayerData.Instance.BodyNumChange(-1);
        }
        #endregion
    }

    private void OnDrawGizmos()
    {
        if (composite == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        int pathCount = composite.pathCount;

        for (int i = 0; i < pathCount; i++)
        {
            int pointCount = composite.GetPathPointCount(i);
            Vector2[] points = new Vector2[pointCount];
            composite.GetPath(i, points);

            for (int p = 0; p < points.Length; p++)
            {
                Gizmos.DrawSphere(points[p], 0.05f);
            }
        }
    }

    /// <summary>
    /// 指定したワールド座標から最も近いコライダーの角（頂点）を返す
    /// </summary>
    public Vector2 GetNearestCorner(Vector2 targetPos)
    {
        // スクリプトに CompositeCollider2D が設定されていない場合、
        // 自身のオブジェクトから自動で取得する
        if (composite == null)
        {
            composite = GetComponent<CompositeCollider2D>();
        }

        // 最小距離を初期化（非常に大きい値にセット）
        float minDist = float.MaxValue;

        // 最も近い角を保持する変数
        Vector2 nearest = Vector2.zero;

        // CompositeCollider2D が保持するパス（ポリゴン）の数
        int pathCount = composite.pathCount;

        // すべてのパスをループ
        for (int i = 0; i < pathCount; i++)
        {
            // このパスが持つ頂点の数を取得
            int pointCount = composite.GetPathPointCount(i);

            // 頂点座標を格納する配列
            Vector2[] points = new Vector2[pointCount];

            // ★ この時点で points の内容は「ワールド座標」で返される ★
            composite.GetPath(i, points);

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

        // 最も近かった角を返す
        return nearest;
    }
}
