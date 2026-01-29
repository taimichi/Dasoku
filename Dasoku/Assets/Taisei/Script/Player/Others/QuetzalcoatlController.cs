using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuetzalcoatlController : MonoBehaviour, PlayerInterface
{
    private PlayerControlManager plMG;

    private Rigidbody2D playerRB;

    private BoxCollider2D box;
    private Vector2 colliderSize;
    private Vector2 colliderOffset;

    //見た目
    [SerializeField] private GameObject Appearance;
    private SpriteRenderer AppearanceSpriteRenderer;

    private void Awake()
    {
        AppearanceSpriteRenderer = Appearance.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        plMG = PlayerControlManager.Instance;

        playerRB = this.GetComponent<Rigidbody2D>();
        box = this.GetComponent<BoxCollider2D>();
        colliderSize = box.size;
        colliderOffset = box.offset;
    }

    private void PlayerMove()
    {
        //現在のプレイヤーの速度を取得
        Vector2 newPLVelo = playerRB.velocity;

        plMG.plVec = plMG.plVec.normalized;

        //横移動
        //移動速度を計算
        newPLVelo.x = plMG.plVec.x * plMG.plData.moveSpeed;
        //速度が上限を超えた時
        if (Mathf.Abs(newPLVelo.x) > plMG.plData.maxVelocity.x)
        {
            //上限速度に設定
            newPLVelo.x = plMG.plData.maxVelocity.x * plMG.plVec.x;
        }

        //縦移動
        //速度計算
        newPLVelo.y = plMG.plVec.y * plMG.plData.moveSpeed;
        //速度が上限を超えた時
        if(Mathf.Abs(newPLVelo.y) > plMG.plData.maxVelocity.y)
        {
            //上限速度に設定
            newPLVelo.y = plMG.plData.maxVelocity.y * plMG.plVec.y;
        }

        //velocity変更
        playerRB.velocity = newPLVelo;
    }

    public void PlUpdate()
    {
        PlayerMove();
    }

    #region 入力処理
    public void InputRight()
    {
        plMG.plVec.x = 1;

        //向いてる方向を右に変更
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.right);
    }

    public void InputLeft()
    {
        plMG.plVec.x = -1;

        //向いてる方向を左に変更
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.left);
    }

    public void InputLRUp()
    {
        plMG.plVec.x = 0;
        //横の速度を0に
        playerRB.velocity = new Vector2(0, playerRB.velocity.y);
    }

    public void NoInput()
    {
        //ベクトルを0に
        plMG.plVec = Vector2.zero;
        //向いてる方向をどちらも向いてない状態に
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.normal);
    }

    public void InputUp()
    {
        plMG.plVec.y = 1;
    }

    public void InputDown()
    {
        plMG.plVec.y = -1;
    }

    public void InputUDUp()
    {
        plMG.plVec.y = 0;
        //縦の速度を0に
        playerRB.velocity = new Vector2(playerRB.velocity.x, 0);
    }


    public void InputActionDown()
    {

    }

    public void InputAction()
    {

    }

    public void InputActionUp()
    {

    }
    #endregion

    public void FormChange(Sprite _sprite)
    {
        AppearanceSpriteRenderer.sprite = _sprite;
    }

}
