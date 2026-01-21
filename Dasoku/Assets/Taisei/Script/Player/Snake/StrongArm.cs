using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class SnakeController : MonoBehaviour
{
    [Header("剛腕関連")]
    //握った時のスプライト
    [SerializeField] private Sprite gripSprite;

    //剛腕のアクション状態
    private enum STRONGARM_STATE
    {
        idle,       //待機
        grip,       //握りこぶし
        action      //破壊
    }
    //現在の剛腕のアクション状態
    private STRONGARM_STATE nowStrongArm = STRONGARM_STATE.idle;

    /// <summary>
    /// 剛腕形態のアクション
    /// </summary>
    private void StrongArmAction()
    {
        //現在のアクション状態
        switch (nowStrongArm)
        {
            //待機状態
            case STRONGARM_STATE.idle:
                nowStrongArm = STRONGARM_STATE.grip;
                HeadSpriteRenderer.sprite = gripSprite;
                break;
            
            //握りこぶし
            case STRONGARM_STATE.grip:
                //前方に壁があるとき
                if (CheckWall())
                {
                    nowStrongArm = STRONGARM_STATE.action;
                    WallBreak();
                }
                //前方に壁がなかった時
                else
                {
                    nowStrongArm = STRONGARM_STATE.idle;
                    HeadSpriteRenderer.sprite = plMG.plData.SearchState(plMG.plData.nowMode).headSprite;
                }
                break;
        }
    }

    /// <summary>
    /// 壁破壊
    /// </summary>
    private void WallBreak()
    {
        float rayDistance = 0.5f;

        Vector2 local_front = colliderOffset + new Vector2(colliderSize.x / 2, 0);
        Vector2 front = transform.TransformPoint(local_front);

        RaycastHit2D hit = Physics2D.Raycast(front, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        if(hit.collider != null)
        {
            Tilemap tile = hit.collider.gameObject.GetComponent<Tilemap>();
            if (tile != null)
            {
                Vector3 hitPoint = hit.point + plMG.plSeeVec * 0.01f;

                // ワールド座標 → セル座標へ変換
                Vector3Int cellPos = tile.WorldToCell(hitPoint);

                tile.SetTile(cellPos, null);

            }
        }
        //握りこぶし状態に戻す
        nowStrongArm = STRONGARM_STATE.grip;
    }

    /// <summary>
    /// 前方に壁があるか
    /// </summary>
    /// <returns>false=壁がない / true=壁がある</returns>
    private bool CheckWall()
    {
        bool _isHit = false;
        float rayDistance = 0.5f;

        Vector2 local_front = colliderOffset + new Vector2(colliderSize.x / 2, 0);
        Vector2 front = transform.TransformPoint(local_front);

        _isHit = Physics2D.Raycast(front, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        Debug.DrawRay(front, plMG.plSeeVec * rayDistance, Color.green);

        return _isHit;
    }
}
