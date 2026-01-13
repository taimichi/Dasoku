using UnityEngine;
using System;

//プレイヤーに関するスクリプト(操作など)
public partial class SnakeController : MonoBehaviour, PlayerInterface
{
    private PlayerControlManager plMG;

    //プレイヤーの見た目オブジェクト
    [SerializeField, Tooltip("見た目オブジェクト")] private GameObject Appearance;
    //プレイヤーの見た目
    private SpriteRenderer HeadSpriteRenderer;

    //プレイヤーのコライダー
    private BoxCollider2D box;
    private Vector2 colliderSize;       //コライダーのサイズ
    private Vector2 colliderOffset;     //コライダーのオフセット

    //プレイヤーのリジッドボディ
    private Rigidbody2D playerRB;

    //重力操作用スクリプト
    private LocalGravityChanger gravityChanger = new LocalGravityChanger();

    //蛇手関連
    private GameObject CatchObj;
    [SerializeField] private Transform CatchPoint;
    //地面用のレイヤー
    [SerializeField, Tooltip("つかめる物レイヤー")] private LayerMask CatchLayer;

    void Start()
    {
        plMG = PlayerControlManager.Instance;

        //プレイヤーの見た目オブジェクトのスプライトレンダラーを取得
        HeadSpriteRenderer = Appearance.GetComponent<SpriteRenderer>();

        //プレイヤーのコライダーを取得
        box = this.GetComponent<BoxCollider2D>();
        colliderSize = box.size;
        colliderOffset = box.offset;
        //プレイヤーのリジッドボディを取得
        playerRB = this.GetComponent<Rigidbody2D>();

        //初期速度設定
        plMG.plData.SpeedChange(1.0f);

        //プレイヤーのリジッドボディを重力操作スクリプトに渡す
        gravityChanger.SetGravityChange(playerRB);

        //開始時のプレイヤーの向いてる方向を設定
        plMG.PlayerDirection(this.transform ,plMG.nowDire);
    }

    public void PlUpdate()
    {
        //重力処理
        Gravity();

        //落下判定+処理
        PlayerFall();
        //落下中は以降の処理をしない
        if (isFall)
        {
            return;
        }

        //回転中じゃなく、地面と触れてる時
        if (!CheckNowRotate(limitRot, this.transform.localEulerAngles.z))
        {
            if (nowRot != PlayerRot_Mode.rot)
            {
                //回転するかの判定
                CheckRotate();
            }
        }

        //移動処理をするか回転処理をするか
        if (!isRot)
        {
            //移動処理
            PlayerMove();
        }
        else
        {
            //地面と触れてる時のみ
            if (CheckGround())
            {
                //回転処理
                PlayerRotate();

                //回転後の処理
                RotateAfter();
            }
        }
    }

    /// <summary>
    /// 重力関連の処理
    /// </summary>
    private void Gravity()
    {
        //重力の変更の有無
        gravityChanger.GravityChange(CheckUseGravity(this.transform.localEulerAngles.z));

        //重力の方向設定
        Vector2 gravityDire = -this.transform.up;

        //重力の方向変更
        gravityChanger.ChangeGravityDirection(gravityDire);

        //重力用の更新
        gravityChanger.GravityUpdate();
    }

    public void InputRight()
    {
        //プレイヤーの右向きベクトルを取得
        plMG.plVec = this.transform.right;

        //x
        NumTolerance(plMG.plVec.x);
        plMG.plVec.x = (float)Math.Truncate(plMG.plVec.x * 100f + 1e-6f) / 100f;
        //y
        NumTolerance(plMG.plVec.y);
        plMG.plVec.y = (float)Math.Truncate(plMG.plVec.y * 100f + 1e-6f) / 100f;

        //向いてる方向を右に変更
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.right);
    }

    public void InputLeft()
    {
        //プレイヤーの左向きベクトルを取得
        plMG.plVec = -this.transform.right;

        //plVecのx、yの数値がそれぞれ0.00001未満なら0として扱う
        //x
        NumTolerance(plMG.plVec.x);
        plMG.plVec.x = (float)Math.Truncate(plMG.plVec.x * 100f + 1e-6f) / 100f;
        //y
        NumTolerance(plMG.plVec.y);
        plMG.plVec.y = (float)Math.Truncate(plMG.plVec.y * 100f + 1e-6f) / 100f;

        //向いてる方向を左に変更
        plMG.PlayerDirection(this.transform, PlayerControlManager.PlayerDire_Mode.left);
    }

    public void InputLRUp()
    {
        playerRB.velocity = Vector2.zero;   
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
        //各モードのアクション
        switch (plMG.plData.nowMode)
        {
            //蛇
            case PlayerData.PLAYER_MODE.snake:
                //回転中じゃないとき
                if (!isRot)
                {
                    //はりつき解除
                    this.transform.localEulerAngles = Vector3.zero;
                }
                break;

            //蛇手
            case PlayerData.PLAYER_MODE.snakeHand:
                if (!isRot)
                {
                    //物をつかんでないとき
                    if (!plMG.plData.isCatchObj)
                    {
                        CatchObject();
                    }
                    //物をつかんでる時
                    else
                    {
                        ReleaseObject();
                    }
                }
                break;
        }
    }

    public void InputAction()
    {

    }

    public void InputActionUp()
    {

    }

    /// <summary>
    /// 離す処理
    /// </summary>
    private void ReleaseObject()
    {
        //つかんだものを子オブジェクトじゃなくする
        CatchObj.transform.parent = null;
        //大きさを元に戻す
        CatchObj.transform.localScale = Vector2.one;
        //角度を普通にする
        CatchObj.transform.localEulerAngles = Vector3.zero;

        //再設置場所を計算
        Vector2 pos = CatchObj.transform.position;
        float dis = 1.0f;   //プレイヤーからの距離
        pos += plMG.plSeeVec * dis;
        //設置
        CatchObj.transform.position = pos;

        //当たり判定を有効化
        BoxCollider2D boxCol = CatchObj.GetComponent<BoxCollider2D>();
        boxCol.enabled = true;
        //物理演算を有効化
        Rigidbody2D rb = CatchObj.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        CatchObj = null;
        plMG.plData.isCatchObj = false;
    }

    /// <summary>
    /// つかむ処理
    /// </summary>
    private void CatchObject()
    {
        //前方につかめる物があるとき
        if (CheckCatch())
        {
            //つかんだものを子オブジェクトにする
            CatchObj.transform.parent = this.transform;
            //つかんだものを持ってる状態の時の場所に移動
            CatchObj.transform.position = CatchPoint.position;
            //大きさを小さくする
            CatchObj.transform.localScale = new Vector2(0.7f, 0.7f);

            //当たり判定を無効化
            BoxCollider2D boxCol = CatchObj.GetComponent<BoxCollider2D>();
            boxCol.enabled = false;
            //物理演算を無効化
            Rigidbody2D rb = CatchObj.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            plMG.plData.isCatchObj = true;
        }
    }

    /// <summary>
    /// 前方につかめる物があるかどうか
    /// </summary>
    /// <returns>false=ない / true=ある</returns>
    private bool CheckCatch()
    {
        Vector2 local_front = colliderOffset + new Vector2(colliderSize.x / 2, 0);
        Vector2 front = transform.TransformPoint(local_front);

        RaycastHit2D hit = Physics2D.Raycast(front, plMG.plSeeVec, 0.5f, CatchLayer);

        Debug.DrawRay(front, plMG.plSeeVec * 0.5f, Color.red);

        //レイがオブジェクトを取得した時
        if(hit.transform.gameObject != null)
        {
            Debug.Log(hit.transform.gameObject.name);

            //取得したオブジェクトがつかめる物だったとき
            if (hit.transform.tag == "CatchObject")
            {
                CatchObj = hit.transform.gameObject;
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// float型の数値で、0.00001未満のような0に近い数値を0にする
    /// </summary>
    /// <param name="_value">判定したい数値(float型)</param>
    /// <returns>判定し終わった数値</returns>
    private float NumTolerance(float _value)
    {
        //_valueが0.00001未満の時、0として扱う
        if(Mathf.Abs(_value) < 1e-5f)
        {
            _value = 0;
        }
        return _value;
    }

    /// <summary>
    /// 0.5マスの高さのブロックが前方にあるかチェック
    /// </summary>
    /// <returns>false=0.5マスブロックなし / true=0.5マスブロックあり</returns>
    private bool CheckHalfBlock()
    {
        float rayDistance = 0.2f;

        //上判定
        Vector2 up_local = colliderOffset + new Vector2(colliderSize.x / 2f, colliderSize.y / 2.5f);
        Vector2 up = transform.TransformPoint(up_local);
        //下判定
        Vector2 down_local = colliderOffset + new Vector2(colliderSize.x / 2f, -colliderSize.y / 2.5f);
        Vector2 down = transform.TransformPoint(down_local);

        //上判定のレイ
        bool isUp = Physics2D.Raycast(up, plMG.plSeeVec, rayDistance, plMG.groundLayer);
        //下判定のレイ
        bool isDown = Physics2D.Raycast(down, plMG.plSeeVec, rayDistance, plMG.groundLayer);

        #region デバッグ用_レイ表示
        Debug.DrawRay(up, plMG.plSeeVec * rayDistance, Color.yellow);
        Debug.DrawRay(down, plMG.plSeeVec * rayDistance, Color.yellow);
        #endregion

        //どちらかのみが地面に触れてる時
        if (isUp ^ isDown)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 地面判定用
    /// </summary>
    /// <returns>false=地面と触れてない / true=地面と触れてる</returns>
    private bool CheckGround()
    {
        //地面判定
        bool isGround = true;

        Vector2 down = -this.transform.up;          //プレイヤーの下方向ベクトル

        //座標計算 y座標はコライダーの途中から出したいため/2ではなく/4に
        //真ん中
        Vector2 localBottom_center = colliderOffset + new Vector2(0, -colliderSize.y / 4f);
        Vector2 bottom_center = transform.TransformPoint(localBottom_center);
        //左
        Vector2 localBottom_left = colliderOffset + new Vector2(-colliderSize.x / 2f, -colliderSize.y / 4f);
        Vector2 bottom_left = transform.TransformPoint(localBottom_left);
        //右
        Vector2 localBottom_right = colliderOffset + new Vector2(colliderSize.x / 2f, -colliderSize.y / 4f);
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

        if (isLeft ^ isRight)
        {
            isHalfGround = true;
        }
        else
        {
            isHalfGround = false;
        }

        return isGround;
    }

    public void FormChange(Sprite _sprite)
    {            
        //見た目変更
        HeadSpriteRenderer.sprite = _sprite;

        //形態変化した時に物を持ってたら
        if (plMG.plData.isCatchObj)
        {
            //物を離す
            ReleaseObject();
        }

    }

    /// <summary>
    /// プレイヤーの下ベクトルを返す
    /// </summary>
    /// <returns>プレイヤーの下ベクトル</returns>
    public Vector3 ReturnPlayerDownVec() => -this.transform.up;

    /// <summary>
    /// 胴体の数を返す
    /// </summary>
    /// <returns>頭を除く胴体の数(しっぽは含む)</returns>
    public int ReturnBodyNum() => plMG.plData.bodyNum;
}
