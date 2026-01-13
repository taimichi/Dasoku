using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool isActionInput = false;

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
                playerRB.AddForce(new Vector2(plMG.plVec.x, 0), ForceMode2D.Impulse);
                //ジャンプ処理
                if (!isActionInput)
                {
                    playerRB.AddForce(new Vector2(plMG.plVec.x, moveJumpPower), ForceMode2D.Impulse);
                }
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
        }
        //空中の時
        else
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

        //ジャンプしてる　かつ　進行方向の壁に触れてる時
        if (!CheckGround() && CheckWall())
        {
            //壁と当たった時ずり落ちるようにするため、velocity.xを0にする
            playerRB.velocity = new Vector2(0, playerRB.velocity.y);
        }

    }

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
    }

    public void NoInputLR()
    {
        //ベクトルを0に
        plMG.plVec = Vector2.zero;
        //向いてる方向をどちらも向いてない状態に
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.normal);
    }

    public void InputActionDown()
    {

    }

    public void InputAction()
    {
        isActionInput = true;
    }

    public void InputActionUp()
    {
        isActionInput = false;  
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
        float groundCheckDistance = 0.1f;

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

    public void FormChange(Sprite _sprite)
    {
        Appearance.sprite = _sprite;
    }
}
