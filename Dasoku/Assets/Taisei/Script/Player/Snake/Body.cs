using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    [SerializeField] private BoxCollider2D box;
    //地面用のレイヤー
    [SerializeField, Tooltip("地面のレイヤー")] private LayerMask groundLayer;
    private float rayDistance = 0.5f;

    /// <summary>
    /// 胴体が地面に触れてるかどうか
    /// </summary>
    /// <returns>false=触れてない / true=触れてる</returns>
    public bool CheckBodyGround()
    {
        Vector2 size = box.size;                 //コライダーのサイズ
        Vector2 offset = box.offset;             //コライダーのオフセット
        Vector2 down = -this.transform.up;       //プレイヤーの下方向ベクトル

        //中心下の座標
        //中心下のローカル座標を取得
        Vector2 local_bottom = offset + new Vector2(0, -size.y / 2f);
        //ワールド座標に変換
        Vector2 bottom = transform.TransformPoint(local_bottom);

        //中心下が地面と触れているかレイで判定
        //中心下
        bool isBottomGrounded = Physics2D.Raycast(bottom, down, rayDistance, groundLayer);

        Debug.DrawRay(bottom, down * rayDistance, Color.green);

        return isBottomGrounded;
    }

}
