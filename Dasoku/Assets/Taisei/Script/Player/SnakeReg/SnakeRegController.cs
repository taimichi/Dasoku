using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SnakeRegController : MonoBehaviour
{
    private GameManager GM;

    private PlayerData PlData;

    private Rigidbody2D playerRB;

    private Vector2 plVec;
    private Vector2 plSeeVec;
    /// <summary>
    /// プレイヤーの方向
    /// </summary>
    private enum PlayerDire_Mode
    {
        normal,
        right,
        left,
    }
    //現在のプレイヤーの向いてる方向
    private PlayerDire_Mode nowDire = PlayerDire_Mode.right;

    private BoxCollider2D plCollider;
    private Vector2 colliderSize;
    private Vector2 colliderOffset;

    private void Awake()
    {
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        playerRB = this.GetComponent<Rigidbody2D>();
        PlData = PlayerData.Instance;

        plCollider = this.GetComponent<BoxCollider2D>();
        colliderSize = plCollider.size;
        colliderOffset = plCollider.offset;

        PlData.SpeedChange(1.0f);

        StartBodySet();
    }

    private void Update()
    {
        BodyUpdate();
    }

    private void FixedUpdate()
    {
        PlayerInput();
        PlayerMove();
    }

    private void PlayerInput()
    {
        #region 移動キー
        //右方向
        if (Input.GetKey(KeyCode.D))
        {
            //プレイヤーの右向きベクトルを取得
            plVec = this.transform.right;

            //x
            plVec.x = (float)Math.Truncate(plVec.x * 100f + 1e-6f) / 100f;
            //y
            plVec.y = (float)Math.Truncate(plVec.y * 100f + 1e-6f) / 100f;

            //向いてる方向を右に変更
            PlayerDirection(PlayerDire_Mode.right);
        }
        //左方向
        else if (Input.GetKey(KeyCode.A))
        {
            //プレイヤーの左向きベクトルを取得
            plVec = -this.transform.right;

            //x
            plVec.x = (float)Math.Truncate(plVec.x * 100f + 1e-6f) / 100f;
            //y
            plVec.y = (float)Math.Truncate(plVec.y * 100f + 1e-6f) / 100f;

            //向いてる方向を左に変更
            PlayerDirection(PlayerDire_Mode.left);
        }
        //移動キーを離したとき
        else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
        {
            //速度を0に変更
            playerRB.velocity = Vector2.zero;
        }
        //何も押されてないとき
        else
        {
            //ベクトルを0に
            plVec = Vector2.zero;
            //向いてる方向をどちらも向いてない状態に
            PlayerDirection(PlayerDire_Mode.normal);
        }
        #endregion

        #region アクションキー
        //スペースキーを押したとき
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }
        #endregion
    }

    /// <summary>
    /// プレイヤーの向いている方向を変更
    /// </summary>
    /// <param name="_isRight">false=左 / true=右</param>
    private void PlayerDirection(PlayerDire_Mode plDire)
    {
        Vector3 plSize = this.transform.localScale;
        //左
        if (plDire == PlayerDire_Mode.left)
        {
            //マイナスにして、見た目を左向きにする
            plSize.x = -(Mathf.Abs(plSize.x));

            plSeeVec = -this.transform.right;
        }
        //右
        else if (plDire == PlayerDire_Mode.right)
        {
            //マイナスをなくして、見た目を右向きにする
            plSize.x = Mathf.Abs(plSize.x);
            plSeeVec = this.transform.right;
        }
        nowDire = plDire;
        this.transform.localScale = plSize;
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    private void PlayerMove()
    {
        //プレイヤー移動処理
        //現在のプレイヤーの速度を取得
        Vector2 newPLVelo = playerRB.velocity;

        //横移動の時
        if (plVec.x != 0)
        {
            //移動速度を計算
            newPLVelo.x = plVec.x * PlData.moveSpeed;

            //速度が上限を超えた時
            if (newPLVelo.x > PlData.maxVelocity.x)
            {
                //上限速度に設定
                newPLVelo.x = PlData.maxVelocity.x;
            }
        }
        //縦移動の時
        if (plVec.y != 0)
        {
            //移動速度を計算
            newPLVelo.y = plVec.y * PlData.moveSpeed;

            //速度が上限を超えた時
            if (newPLVelo.y > PlData.maxVelocity.y)
            {
                //速度が上限を超えた時
                newPLVelo.y = PlData.maxVelocity.y;
            }
        }
        //velocity変更
        playerRB.velocity = newPLVelo;
    }

}
