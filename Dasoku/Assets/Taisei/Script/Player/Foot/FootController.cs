using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FootController : MonoBehaviour, PlayerInterface
{
    private PlayerControlManager plMG;

    private Rigidbody2D playerRB;

    private BoxCollider2D box;
    private Vector2 colliderSize;
    private Vector2 colliderOffset;

    //見た目
    [SerializeField] private SpriteRenderer Appearance;

    //ジャンプ力
    [SerializeField] private float jumpPower = 10f;
    //アクションキーを押しているかどうか
    private bool isActionInput = false;

    //ヒール関連
    /// <summary>
    /// ヒールアクションの状態
    /// </summary>
    private enum HEELACTION_STATE
    {
        idle,   //待機状態
        jump,   //ジャンプ状態
        action, //アクション状態
    }
    private HEELACTION_STATE nowHeelState = HEELACTION_STATE.idle;
    //
    private bool isHeelDrop = false;

    void Start()
    {
        plMG = PlayerControlManager.Instance;

        playerRB = this.GetComponent<Rigidbody2D>();
        box = this.GetComponent<BoxCollider2D>();
        colliderSize = box.size;
        colliderOffset = box.offset;

    }

    public void PlUpdate()
    {
        PlayerMove();
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void PlayerMove()
    {
        //最高速度　仮
        float max = 2;
        //地面と接してる時
        if (CheckGround())
        {
            //どちらかの方向に入力されてる時
            if (plMG.nowDire != PlayerControlManager.PlayerDire_Mode.normal)
            {
                //移動時のジャンプ力
                float moveJumpPower = 1f;
                //移動
                playerRB.AddForce(new Vector2(plMG.plVec.x * plMG.plData.moveSpeed, 0), ForceMode2D.Impulse);

                //ジャンプ処理
                //通常
                if (!isActionInput)
                {
                    playerRB.AddForce(new Vector2(0, moveJumpPower), ForceMode2D.Impulse);
                }
                //アクション(大ジャンプ)
                else
                {
                    playerRB.velocity = new Vector2(playerRB.velocity.x, jumpPower);
                }
                //速度制限
                if (Mathf.Abs(playerRB.velocity.x) >= max)
                {
                    playerRB.velocity = new Vector2(plMG.plVec.x * max, playerRB.velocity.y);
                }
            }
            //方向が入力されてないとき
            else
            {
                //ジャンプ(アクション)処理
                if (isActionInput)
                {
                    playerRB.velocity = new Vector2(playerRB.velocity.x, jumpPower);
                }
            }
        }
        //空中の時
        else
        {
            //ヒールアクション状態がアクション状態じゃないとき
            if(nowHeelState != HEELACTION_STATE.action)
            {
                //移動
                playerRB.AddForce(new Vector2(plMG.plVec.x, 0), ForceMode2D.Impulse);
                //速度制限
                if (Mathf.Abs(playerRB.velocity.x) >= max)
                {
                    playerRB.velocity = new Vector2(plMG.plVec.x * max, playerRB.velocity.y);
                }
                //移動方向を変化
                playerRB.velocity = new Vector2(Mathf.Abs(playerRB.velocity.x) * plMG.plSeeVec.x, playerRB.velocity.y);
            }
            //アクション状態の時
            else
            {
                //落下アクションが一度も行われてないとき
                if (!isHeelDrop)
                {
                    StartCoroutine(HeelDrop());
                    isHeelDrop = true;
                }
            }
        }

        //ジャンプしてる　かつ　進行方向の壁に触れてる時
        if (!CheckGround() && CheckWall())
        {
            //壁と当たった時ずり落ちるようにするため、velocity.xを0にする
            playerRB.velocity = new Vector2(0, playerRB.velocity.y);
        }

    }

    #region 入力処理
    public void InputRight()
    {
        //プレイヤーの右向きベクトルを取得
        plMG.plVec = this.transform.right;

        //x
        plMG.plVec.x = (float)Math.Truncate(plMG.plVec.x * 100f + 1e-6f) / 100f;
        //y
        plMG.plVec.y = (float)Math.Truncate(plMG.plVec.y * 100f + 1e-6f) / 100f;

        //向いてる方向を右に変更
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.right);
    }

    public void InputLeft()
    {
        //プレイヤーの左向きベクトルを取得
        plMG.plVec = -this.transform.right;

        //x
        plMG.plVec.x = (float)Math.Truncate(plMG.plVec.x * 100f + 1e-6f) / 100f;
        //y
        plMG.plVec.y = (float)Math.Truncate(plMG.plVec.y * 100f + 1e-6f) / 100f;

        //向いてる方向を左に変更
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.left);
    }

    public void InputLRUp()
    {
        playerRB.velocity = new Vector2(0, playerRB.velocity.y);

        //向いてる方向をどちらも向いてない状態に
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.normal);
    }

    public void NoInput()
    {
        //ベクトルを0に
        plMG.plVec = Vector2.zero;
    }

    public void InputUp()
    {

    }

    public void InputDown()
    {

    }

    public void InputUDUp()
    {

    }


    public void InputActionDown()
    {
        //ヒール形態の時のみ
        if (plMG.plData.nowMode == PlayerData.PLAYER_MODE.heel)
        {
            //現在のヒールアクション状態が待機状態の時
            if (nowHeelState == HEELACTION_STATE.idle)
            {
                //ジャンプ状態に変更
                nowHeelState = HEELACTION_STATE.jump;
            }
            //現在のヒールアクション状態がジャンプ状態の時
            else if (nowHeelState == HEELACTION_STATE.jump && CheckHeight())
            {
                //アクション状態に変更
                nowHeelState = HEELACTION_STATE.action;
            }
        }
    }

    public void InputAction()
    {
        isActionInput = true;
    }

    public void InputActionUp()
    {
        isActionInput = false;  
    }
    #endregion

    public void FormChange(Sprite _sprite)
    {
        Appearance.sprite = _sprite;
    }

    /// <summary>
    /// 地面判定用
    /// </summary>
    /// <returns>false=触れてない / true=触れてる</returns>
    private bool CheckGround()
    {
        //地面判定
        bool isGround = true;

        Vector2 down = -this.transform.up;          //プレイヤーの下方向ベクトル
        float groundCheckDistance = 0.05f;

        //真ん中
        Vector2 localBottom_center = colliderOffset + new Vector2(0, -colliderSize.y / 2f);
        Vector2 bottom_center = transform.TransformPoint(localBottom_center);
        //左
        Vector2 localBottom_left = colliderOffset + new Vector2(-colliderSize.x / 2f, -colliderSize.y / 2f);
        Vector2 bottom_left = transform.TransformPoint(localBottom_left);
        //右
        Vector2 localBottom_right = colliderOffset + new Vector2(colliderSize.x / 2f, -colliderSize.y / 2f);
        Vector2 bottom_right = transform.TransformPoint(localBottom_right);

        //レイ判定
        bool isCenter = Physics2D.Raycast(bottom_center, down, groundCheckDistance, plMG.groundLayer);
        bool isLeft = Physics2D.Raycast(bottom_left, down, groundCheckDistance, plMG.groundLayer);
        bool isRight = Physics2D.Raycast(bottom_right, down, groundCheckDistance, plMG.groundLayer);

        #region デバッグ用_レイ表示
        Debug.DrawRay(bottom_center, down * groundCheckDistance, Color.blue);
        Debug.DrawRay(bottom_left, down * groundCheckDistance, Color.blue);
        Debug.DrawRay(bottom_right, down * groundCheckDistance, Color.blue);
        #endregion

        //全ての判定がfalseの時
        if (!isCenter && !isLeft && !isRight)
        {
            //地面が離れた
            isGround = false;
        }
        //1つでも地面と触れてる時
        else
        {
            //地面と触れている
            isGround = true;
        }

        return isGround;
    }

    /// <summary>
    /// 壁と触れてるかどうか
    /// </summary>
    /// <returns>false=触れてない / true=触れてる</returns>
    private bool CheckWall()
    {
        bool isWall = false;
        float rayDistance = 0.05f;

        //右上のレイ
        Vector2 local_upRight = colliderOffset + new Vector2(colliderSize.x / 2, -colliderSize.y / 2);
        Vector2 upRight = transform.TransformPoint(local_upRight);
        bool isUp = Physics2D.Raycast(upRight, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        //右下のレイ
        Vector2 local_downRight = colliderOffset + new Vector2(colliderSize.x / 2, colliderSize.y / 2);
        Vector2 downRight = transform.TransformPoint(local_downRight);
        bool isDown = Physics2D.Raycast(downRight, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        //レイのどちらかが壁と触れてる時
        if (isUp || isDown)
        {
            isWall = true;
        }
        else
        {
            isWall = false;
        }

        return isWall;
    }

    /// <summary>
    /// ヒール形態のアクション
    /// </summary>
    private void HeelAction(Collision2D collision)
    {
        //接触したのが破壊可能オブジェクトの時
        if (collision.collider.CompareTag("Ground"))
        {
            //タイルマップコンポーネントを取得できる時
            if (collision.collider.gameObject.TryGetComponent<Tilemap>(out Tilemap targetTilemap))
            {
                // プレイヤー自身に付ける場合は判定不要
                // 別オブジェクト用ならタグなどで判定
                ContactPoint2D contact = collision.contacts[0];

                // 接触したワールド座標
                Vector3 hitPosition = contact.point;

                // ワールド座標 → セル座標へ変換
                Vector3Int cellPos = targetTilemap.WorldToCell(hitPosition);

                // タイルが存在するなら削除
                if (targetTilemap.HasTile(cellPos))
                {
                    targetTilemap.SetTile(cellPos, null);
                    nowHeelState = HEELACTION_STATE.idle;
                    isHeelDrop = false;
                }
            }
        }
    }

    /// <summary>
    /// 地面とプレイヤーの距離が一定以上か調べる
    /// </summary>
    /// <returns>false=一定距離離れてない / true=離れてる</returns>
    private bool CheckHeight()
    {
        float rayDistance = 1.5f;
        Vector2 local_bottom = colliderOffset + new Vector2(0, -colliderSize.y / 2);
        Vector2 bottom = transform.TransformPoint(local_bottom);
        bool isHit = !Physics2D.Raycast(bottom, -this.transform.up, rayDistance, plMG.groundLayer);

        return isHit;
    }

    /// <summary>
    /// 落下アクション処理(非同期)
    /// </summary>
    private IEnumerator HeelDrop()
    {
        //速度を0にする
        playerRB.velocity = Vector2.zero;
        //一時的に物理挙動をなしにする
        playerRB.bodyType = RigidbodyType2D.Kinematic;
        //空中で待機
        yield return new WaitForSeconds(0.5f);

        //物理挙動を元に戻す
        playerRB.bodyType = RigidbodyType2D.Dynamic;
        //下に落下させる
        playerRB.velocity = new Vector2(0, -10f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //ジャンプ後の着地時
        if (nowHeelState == HEELACTION_STATE.action)
        {
            HeelAction(collision);
        }
        nowHeelState = HEELACTION_STATE.idle;
        isHeelDrop = false;
    }
}
