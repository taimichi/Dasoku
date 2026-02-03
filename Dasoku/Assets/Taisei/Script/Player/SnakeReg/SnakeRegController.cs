using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SnakeRegController : MonoBehaviour, PlayerInterface
{
    private PlayerControlManager plMG;

    private Rigidbody2D playerRB;

    private BoxCollider2D plCollider;
    private Vector2 colliderSize;
    private Vector2 colliderOffset;

    [Header("ジャンプ力")]
    //ジャンプ力
    [SerializeField] private float jumpPower = 10;

    [Header("見た目用")]
    //見た目用
    //頭
    [SerializeField] private SpriteRenderer HeadSpriteRenderer;
    //足
    [SerializeField] private SpriteRenderer[] RegSpriteRenderer;
    [SerializeField] private Sprite[] regSprite;

    //アニメーター
    [SerializeField] private Animator[] animator;

    private bool isTest = false;

    void Start()
    {
        plMG = PlayerControlManager.Instance;

        playerRB = this.GetComponent<Rigidbody2D>();

        plCollider = this.GetComponent<BoxCollider2D>();
        colliderSize = plCollider.size;
        colliderOffset = plCollider.offset;

        StartBodySet();
    }


    public void PlUpdate()
    {
        BodyUpdate();

        PlayerMove();

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

        //歩くモーションに
        AnimChange("isMove", true);
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

        AnimChange("isMove", true);
    }

    public void InputLRUp()
    {
        //速度を0に
        playerRB.velocity = Vector2.zero;

        AnimChange("isMove" ,false);

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

    }

    public void InputAction()
    {
        if (CheckGround())
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, jumpPower);
        }
    }

    public void InputActionUp()
    {

    }
    #endregion

    public bool CheckStandGround()
    {
        return CheckGround();
    }

    /// <summary>
    /// アニメーション変更
    /// </summary>
    /// <param name="_trigger">false=待機モーション / true=歩くモーション</param>
    private void AnimChange(string _para ,bool _trigger)
    {
        for(int i = 0; i < animator.Length; i++)
        {
            animator[i].SetBool(_para, _trigger);
        }
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    private void PlayerMove()
    {
        //現在のプレイヤーの速度を取得
        Vector2 newPLVelo = playerRB.velocity;

        //移動速度を計算
        newPLVelo.x = plMG.plVec.x * plMG.plData.moveSpeed;

        //速度が上限を超えた時
        if (Mathf.Abs(newPLVelo.x) > plMG.plData.maxVelocity.x)
        {
            //上限速度に設定
            newPLVelo.x = plMG.plData.maxVelocity.x * plMG.plVec.x;
        }

        //ジャンプしてる　かつ　進行方向の壁に触れてる時
        if (!CheckGround() && CheckWall())
        {
            //壁と当たった時ずり落ちるようにするため、velocity.xを0にする
            newPLVelo.x = 0;
        }

        //velocity変更
        playerRB.velocity = newPLVelo;

        ////ジャンプしてないとき
        //if (CheckGround())
        //{
        //    //前方に0.5マスブロックがあるとき
        //    if (CheckHalfBlock())
        //    {
        //        //コライダーの右下の座標
        //        Vector2 local_rightBottom = colliderOffset + new Vector2(colliderSize.x / 2f, -colliderSize.y / 2f);
        //        Vector2 rightBottom = transform.TransformPoint(local_rightBottom);
        //        //コライダーの右端の座標
        //        Vector2 local_right = colliderOffset + new Vector2(colliderSize.x / 2f, 0);
        //        Vector2 right = transform.TransformPoint(local_right);

        //        Vector2 offset = right - rightBottom;

        //        Vector2 startPos = right;
        //        Vector2 endPos = plMG.GM.GetNearestCorner(right) + offset;

        //        this.transform.position = Vector2.Lerp(startPos, endPos, 1);
        //    }
        //}

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
        if(isUp || isDown)
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
    /// 0.5マスの高さのブロックが前方にあるかチェック
    /// </summary>
    /// <returns>false=0.5マスブロックなし / true=0.5マスブロックあり</returns>
    private bool CheckHalfBlock()
    {
        float rayDistance = 0.2f;

        //下判定
        Vector2 down_local = colliderOffset + new Vector2(colliderSize.x / 2f, -colliderSize.y / 2.5f);
        Vector2 down = transform.TransformPoint(down_local);

        //下判定のレイ
        bool isDown = Physics2D.Raycast(down, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        #region デバッグ用_レイ表示
        Debug.DrawRay(down, plMG.plSeeVec * rayDistance, Color.yellow);
        #endregion

        //地面に触れてる時
        if (isDown)
        {
            return true;
        }

        return false;
    }

    public void FormChange(Sprite _sprite)
    {
        //見た目変更
        HeadSpriteRenderer.sprite = _sprite;

        if (isTest)
        {
            RegChange();
        }
        isTest = true;
    }
}
