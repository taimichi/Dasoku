using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class OuroborosController : MonoBehaviour, PlayerInterface
{
    private PlayerControlManager plMG;

    private Rigidbody2D playerRB;

    private CircleCollider2D circle;
    private float circleSize;
    private Vector2 circleOffset;

    //見た目
    [SerializeField] private GameObject Appearance;
    private SpriteRenderer AppearanceSpriteRenderer;

    //加速ゲージ
    [SerializeField] private Image AcceleGauge;

    //ジャンプ力
    [SerializeField] private float jumpPower = 10f;

    //直前のプレイヤーの向いてる方向
    private PlayerControlManager.PlayerDire_Mode beforeDire;

    //加速力
    private float accele = 1f;
    //最大加速力
    private const float Accele_MAX = 6;
    //加速力の変化量
    private float acceleAmount;
    //加速力が最大値になったかどうか
    private bool isAcceleMax = false;

    //見た目の回転スピード(1秒に回転する角度)
    private float rotSpeed = -180;

    private void Awake()
    {
        AppearanceSpriteRenderer = Appearance.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        plMG = PlayerControlManager.Instance;

        playerRB = this.GetComponent<Rigidbody2D>();
        circle = this.GetComponent<CircleCollider2D>();
        circleSize = circle.radius;
        circleOffset = circle.offset;

        beforeDire = plMG.nowDire;

        //加速力の変化量
        acceleAmount = 0.05f;
    }

    private void PlayerMove()
    {
        //現在のプレイヤーの速度を取得
        Vector2 newPLVelo = playerRB.velocity;

        if(plMG.nowDire != PlayerControlManager.PlayerDire_Mode.normal)
        {
            //現在の方向と直前の方向が同じとき
            if (beforeDire == plMG.nowDire)
            {
                //徐々に加速度を上げる
                accele += acceleAmount;
                //加速量が最大値を超えた時
                if (accele >= Accele_MAX)
                {
                    accele = Accele_MAX;
                    isAcceleMax = true;
                }
            }
            //別の方向だったとき
            else
            {
                //加速量を初期値に
                accele = 1;
                //直前の方向を今と同じに
                beforeDire = plMG.nowDire;
                isAcceleMax = false;

                rotSpeed = -rotSpeed;
            }

            //見た目のオブジェクトを回転
            Appearance.transform.RotateAround(Appearance.transform.position, Vector3.forward, rotSpeed * Time.deltaTime * accele);
        }

        //移動速度を計算
        newPLVelo.x = plMG.plVec.x * plMG.plData.moveSpeed * accele;

        //速度が上限を超えた時
        if (Mathf.Abs(newPLVelo.x) > plMG.plData.maxVelocity.x)
        {
            //上限速度に設定
            newPLVelo.x = plMG.plVec.x * plMG.plData.maxVelocity.x;
        }

        //壁とぶつかってる時
        if (CheckWall())
        {
            //加速力を初期値に
            accele = 1;
        }

        //velocity変更
        playerRB.velocity = newPLVelo;
    }

    /// <summary>
    /// 加速ゲージの処理
    /// </summary>
    private void AcceleGaugeUpdate()
    {
        float nowGauge = accele - 1;
        float maxGauge = Accele_MAX - 1;

        AcceleGauge.fillAmount = nowGauge / maxGauge;
    }

    /// <summary>
    /// 地面判定
    /// </summary>
    /// <returns>false=触れてない / true=触れてる</returns>
    private bool CheckGround()
    {
        Vector2 down = -this.transform.up;
        float rayDistance = 0.05f;

        Vector2 local_bottom = circleOffset + new Vector2(0, -circleSize);
        Vector2 bottom = transform.TransformPoint(local_bottom);

        bool _isGround = Physics2D.Raycast(bottom, down, rayDistance, plMG.groundLayer);

        return _isGround;
    }

    /// <summary>
    /// 壁と触れてるかどうか
    /// </summary>
    /// <returns>false=触れてない / true=触れてる</returns>
    private bool CheckWall()
    {
        float rayDistance = 0.05f;

        //右のレイ
        Vector2 local_right = circleOffset + new Vector2(circleSize, 0);
        Vector2 right = transform.TransformPoint(local_right);
        bool isWall = Physics2D.Raycast(right, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        return isWall;
    }


    public void PlUpdate()
    {
        PlayerMove();

        AcceleGaugeUpdate();

        //加速力が最大値のとき
        if (isAcceleMax)
        {
            OuroborosAction();
        }
    }

    /// <summary>
    /// ウロボロス形態のアクション
    /// </summary>
    private void OuroborosAction()
    {
        float rayDistance = 0.5f;

        Vector2 local_UpFront = circleOffset + new Vector2(circleSize, circleSize / 2);
        Vector2 upFront = transform.TransformPoint(local_UpFront);
        Vector2 local_DownFront = circleOffset + new Vector2(circleSize, -circleSize / 2);
        Vector2 downFront = transform.TransformPoint(local_DownFront);

        RaycastHit2D hitUp = Physics2D.Raycast(upFront, plMG.plSeeVec, rayDistance, plMG.groundLayer);
        RaycastHit2D hitDown = Physics2D.Raycast(downFront, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        //上部
        if (hitUp.collider != null)
        {
            Tilemap tile = hitUp.collider.gameObject.GetComponent<Tilemap>();
            if (tile != null)
            {
                Vector3 hitPoint = hitUp.point + plMG.plSeeVec * 0.01f;

                //ワールド座標→セル座標へ変換
                Vector3Int cellPos = tile.WorldToCell(hitPoint);
                //タイル破壊
                tile.SetTile(cellPos, null);
            }
        }
        //下部
        if(hitDown.collider != null)
        {
            Tilemap tile = hitDown.collider.gameObject.GetComponent<Tilemap>();
            if (tile != null)
            {
                Vector3 hitPoint = hitDown.point + plMG.plSeeVec * 0.01f;
                //ワールド座標→セル座標
                Vector3Int cellPos = tile.WorldToCell(hitPoint);
                //タイル破壊
                tile.SetTile(cellPos, null);
            }
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
        //速度を0に
        playerRB.velocity = Vector2.zero;

        //加速力を初期値に
        accele = 1;

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

    public void FormChange(Sprite _sprite)
    {
        //見た目変更
        AppearanceSpriteRenderer.sprite = _sprite;
    }

}
