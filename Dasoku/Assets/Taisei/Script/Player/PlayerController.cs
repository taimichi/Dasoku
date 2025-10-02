using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    //プレイヤーのコライダー
    private BoxCollider2D box;

    //プレイヤーのリジッドボディ
    private Rigidbody2D playerRB;

    #region 移動関連
    //プレイヤーの基礎速度
    [SerializeField, Header("基礎速度")] private float basicSpeed = 1.0f;
    //プレイヤーの移動速度
    private float plSpeed = 1.0f;
    //プレイヤーの移動方向
    private Vector2 plVec;
    #endregion

    #region 回転関連
    //地面用のレイヤー
    [SerializeField] private LayerMask groundLayer;
    //一度に回転できる角度
    private float limitRot = 90f;
    //１秒に何度回転する量
    [SerializeField, Header("1秒に回転する量")] private float rotSpeed = 90f;
    //回転する際の支点
    private Vector3 pivotPos;
    //どこまで回転したか
    private float rotateAmount = 0f;
    //回転するかどうか
    private bool isRot = false;
    //回転位置の取得用のオブジェクト
    [SerializeField] private Transform pivotPoint;
    //レイがはみ出た座標保持用
    private Vector3 rayOut_savePos;
    //レイがはみ出したかどうか
    private bool isRayOut = false;
    /// <summary>
    /// 回転に関するプレイヤーの状態
    /// </summary>
    private enum PlayerRot_Mode
    {
        none,       //通常
        rot,        //回転中
        finish,     //回転終了
    }
    //現在の回転関連のプレイヤーの状態
    private PlayerRot_Mode nowRot = PlayerRot_Mode.none;
    //初めて回転モードに入った時のプレイヤーの向いてる方向
    private PlayerDire_Mode startRotDire = PlayerDire_Mode.normal;
    //初めて回転モードに入ったか
    private bool isStartRotDire = false;

    /// <summary>
    /// 回転する際、どこを基準にするか
    /// </summary>
    private enum BasicRot_State
    {
        none,       //通常
        ground,     //地面
        wall,       //壁
    }
    private BasicRot_State rotState = BasicRot_State.none;

    #endregion

    //重力操作用スクリプト
    private LocalGravityChanger gravityChanger = new LocalGravityChanger();

    /// <summary>
    /// プレイヤーの方向
    /// </summary>
    private enum PlayerDire_Mode
    {
        normal,
        right,
        left,
    }
    [SerializeField, Header("プレイヤーの向いてる方向")] private PlayerDire_Mode nowDire = PlayerDire_Mode.right;


    void Start()
    {
        //プレイヤーのコライダーを取得
        box = this.GetComponent<BoxCollider2D>();
        //プレイヤーのリジッドボディを取得
        playerRB = this.GetComponent<Rigidbody2D>();

        //初期速度設定
        PlayerSpeedChange(1.0f);

        //プレイヤーのリジッドボディを重力操作スクリプトに渡す
        gravityChanger.SetGravityChange(playerRB);

        //開始時のプレイヤーの向いてる方向を設定
        PlayerDirection(nowDire);
    }

    private void Update()
    {

    }


    private void FixedUpdate()
    {
        //プレイヤーの入力
        PlayerInput();

        //回転中じゃないとき
        if (!CheckNowRotate())
        {
            if(nowRot != PlayerRot_Mode.rot)
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
            //回転処理
            PlayerRotate();

            //回転後の処理
            RotateAfter();
        }

        Gravity();
    }

    /// <summary>
    /// 重力関連の処理
    /// </summary>
    private void Gravity()
    {
        //重力の変更の有無
        gravityChanger.GravityChange(CheckUseGravity());

        //重力の方向設定
        Vector2 gravityDire = -this.transform.up;

        //重力の方向変更
        gravityChanger.ChangeGravityDirection(gravityDire);

        //重力用の更新
        gravityChanger.GravityUpdate();
    }

    /// <summary>
    /// プレイヤーの入力
    /// </summary>
    private void PlayerInput()
    {
        //右方向
        if (Input.GetKey(KeyCode.D))
        {
            plVec = this.transform.right;
            PlayerDirection(PlayerDire_Mode.right);
        }
        //左方向
        else if (Input.GetKey(KeyCode.A))
        {
            plVec = -this.transform.right;
            PlayerDirection(PlayerDire_Mode.left);
        }
        //何も押されてないとき
        else
        {
            plVec = Vector2.zero;
            PlayerDirection(PlayerDire_Mode.normal);
        }

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
            //マイナスにして、左向きにする
            plSize.x = -(Mathf.Abs(plSize.x));
        }
        //右
        else if(plDire == PlayerDire_Mode.right)
        {
            //マイナスをなくして、右向きにする
            plSize.x = Mathf.Abs(plSize.x);
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
        Vector2 newPLVelo = playerRB.velocity;
        //横移動の時
        if (plVec.x != 0)
        {
            newPLVelo.x = plVec.x * plSpeed; 
        }
        //縦移動の時
        if (plVec.y != 0)
        {
            newPLVelo.y = plVec.y * plSpeed;
        }
        //velocity変更
        playerRB.velocity = newPLVelo;
    }

    /// <summary>
    /// 回転するかの判定
    /// </summary>
    private void CheckRotate()
    {
        Vector2 size    = box.size;                 //コライダーのサイズ
        Vector2 offset  = box.offset;               //コライダーのオフセット
        Vector2 down    = -this.transform.up;       //プレイヤーの下方向ベクトル

        //左下の座標
        //左下のローカル座標を取得
        Vector2 local_leftBottom = offset + new Vector2(-size.x / 2f, -size.y / 2f);
        //ワールド座標に変換
        Vector2 leftBottom = transform.TransformPoint(local_leftBottom);

        //右下の座標
        //右下のローカル座標を取得
        Vector2 local_rightBottom = offset + new Vector2(size.x / 2f, -size.y / 2f);
        //ワールド座標に変換
        Vector2 rightBottom = transform.TransformPoint(local_rightBottom);

        //前方の座標
        //前方のローカル座標を取得
        Vector2 local_forward = offset + new Vector2(size.x / 2f, 0f);
        //ワールド座標に変換
        Vector2 forward = transform.TransformPoint(local_forward);

        //左下、右下が地面と触れているかレイで判定
        //左下
        bool isLeftGrounded = Physics2D.Raycast(leftBottom, down, 0.05f, groundLayer);
        //右下
        bool isRightGrounded = Physics2D.Raycast(rightBottom, down, 0.05f, groundLayer);

        //前方の壁とレイが触れてるか
        bool isForwardGrounded = Physics2D.Raycast(forward, plVec, 0.5f, groundLayer);

        #region レイ表示(デバッグ用)
        Debug.DrawRay(leftBottom, down * 0.05f, Color.green);
        Debug.DrawRay(rightBottom, down * 0.05f, Color.green);
        Debug.DrawRay(forward, plVec * 0.5f, Color.green);
        #endregion

        //回転終了時の時
        if (nowRot == PlayerRot_Mode.finish)
        {
            //回転直後、前の場所に戻ろうとしたとき
            if (nowDire != PlayerDire_Mode.normal && nowDire != startRotDire)
            {
                //モードを通常に
                nowRot = PlayerRot_Mode.none;
                //回転状態を初期化
                isRot = false;
                isStartRotDire = false;
                rotState = BasicRot_State.none;

                //回転の支点座標は変わらないため、先ほどの座標を利用
                rayOut_savePos = pivotPos;
            }
            else
            {
                //左下と右下両方が地面と触れてる時
                if (isLeftGrounded && isRightGrounded)
                {
                    //回転モードを通常に戻す
                    nowRot = PlayerRot_Mode.none;
                    //回転フラグを初期状態に
                    isRot = false;

                    //回転時の処理に使ってた物を初期状態に戻す
                    startRotDire = PlayerDire_Mode.normal;
                    isStartRotDire = false;
                    rotState = BasicRot_State.none;
                }
            }
            return;
        }

        //左下か右下のどちらかのみが地面と触れてる時
        if (isLeftGrounded ^ isRightGrounded && rotState != BasicRot_State.wall)
        {
            //回転基準を地面に
            rotState = BasicRot_State.ground;
            //レイがはみ出して初めての処理の時
            if (!isRayOut)
            {
                rayOut_savePos = !isLeftGrounded ? leftBottom : rightBottom;
                isRayOut = true;
            }

            //それぞれの座標の小数点第2位以下を切り捨て
            //レイがはみ出したときの座標
            Vector3 save = rayOut_savePos;
            save.x = Mathf.Floor(save.x * 10f) / 10f;
            save.y = Mathf.Floor(save.y * 10f) / 10f;
            save.z = Mathf.Floor(save.z * 10f) / 10f;
            //現在の回転支点の座標
            Vector3 pivot = pivotPoint.position;
            pivot.x = Mathf.Floor(pivot.x * 10f) / 10f;
            pivot.y = Mathf.Floor(pivot.y * 10f) / 10f;
            pivot.z = Mathf.Floor(pivot.z * 10f) / 10f;

            //レイがはみ出した座標と回転支点が一緒の時
            if (save == pivot)
            {
                //回転量を初期化
                rotateAmount = 0f;

                //回転の支点の位置を変更
                pivotPos = pivotPoint.position;

                //物理挙動での移動ができないように
                playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

                Debug.Log("回転する(地面)");
                isRot = true;
            }
            return;
        }
        //前方の壁に触れた時
        else if (isForwardGrounded && rotState != BasicRot_State.ground)
        {
            Vector2 pivot = this.transform.position;
            pivot.x = Mathf.Floor(pivot.x * 100f) / 100f;
            pivot.y = Mathf.Floor(pivot.y * 100f) / 100f;
            Vector2 pivotOffset = this.transform.up * 0.5f;
            pivot += pivotOffset;

            //回転の支点の位置を変更
            pivotPos = pivot;

            //回転量を初期化
            rotateAmount = 0f;

            //回転基準を壁に
            rotState = BasicRot_State.wall;

            //物理挙動での移動ができないように
            playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

            Debug.Log("回転する(壁)");
            isRot = true;

            return;
        }

        //プレイヤーを物理挙動での移動ができるようにする
        //(回転はさせたくないため、rotationはそのまま固定)
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log("回転しない");
        //回転しないとして、falseを返す
        isRot = false;

        isRayOut = false;
        nowRot = PlayerRot_Mode.none;

        rotState = BasicRot_State.none;
    }

    /// <summary>
    /// 疑似重力を使うかどうか
    /// </summary>
    /// <returns>false=疑似重力が必要 / true=疑似重力が必要</returns>
    private bool CheckUseGravity()
    {
        //現在のz軸の角度を取得
        float nowRotZ = (int)this.transform.localEulerAngles.z;

        //角度が下向き以外の時
        if(nowRotZ % 360 != 0)
        {
            Debug.Log("疑似重力");
            return true;
        }
        Debug.Log("通常重力");
        return false;
    }

    /// <summary>
    /// 現在回転中かどうか
    /// </summary>
    /// <returns>true=回転中 / false=回転中じゃない</returns>
    private bool CheckNowRotate()
    {

        //現在のz軸の角度を取得
        float nowRotZ = (float)Math.Round(this.transform.localEulerAngles.z, MidpointRounding.AwayFromZero);        

        //回転がまだ途中の時
        if(nowRotZ % limitRot != 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// プレイヤーの回転処理
    /// </summary>
    private void PlayerRotate()
    {
        //回転終了状態じゃないとき
        if(nowRot != PlayerRot_Mode.finish)
        {
            //回転モードに変更
            nowRot = PlayerRot_Mode.rot;

            //地面基準
            if(rotState == BasicRot_State.ground)
            {
                //回転処理
                Rot_Ground();
            }
            //壁基準
            else if(rotState == BasicRot_State.wall)
            {
                //回転処理
                Rot_Wall();
            }

        }
    }

    /// <summary>
    /// 回転処理(地面)
    /// </summary>
    private void Rot_Ground()
    {
        //向いてる方向が右か左、どちらかを向いてる時のみ
        if (nowDire != PlayerDire_Mode.normal)
        {
            //初めて回転モードに入った時
            if (!isStartRotDire)
            {
                //初めて回転モードに入った時のプレイヤーの向いてる方向を取得
                startRotDire = nowDire;
                //この処理は1度だけ
                isStartRotDire = true;
            }

            //回転する量
            float step = rotSpeed * Time.deltaTime;

            //初めて回転モードに入った時のプレイヤーの向いてる方向と現在の方向が同じとき
            if (startRotDire == nowDire)
            {
                //一度に回転する量を超えた時
                if (rotateAmount + step > limitRot)
                {
                    //目標角度で止める
                    step = limitRot - rotateAmount;
                }
            }
            //違うとき
            else
            {
                if (rotateAmount - step < 0)
                {
                    step = 0 + rotateAmount;
                }
            }

            //回転
            //前進時
            if (nowDire == PlayerDire_Mode.right)
            {
                //pivotPosを支点にz軸で回転
                this.transform.RotateAround(pivotPos, Vector3.forward, -step);

                if (startRotDire == nowDire)
                {
                    //回転した量を追加
                    rotateAmount += step;
                }
                else
                {
                    //回転した量を追加
                    rotateAmount -= step;
                }
            }
            //後退時
            else if (nowDire == PlayerDire_Mode.left)
            {
                //pivotPosを支点にz軸で回転
                this.transform.RotateAround(pivotPos, Vector3.forward, step);

                if (startRotDire == nowDire)
                {
                    //回転した量を追加
                    rotateAmount += step;
                }
                else
                {
                    //回転した量を追加
                    rotateAmount -= step;
                }
            }

            //回転した量が1度に回転できる量以上になった時
            if (rotateAmount >= limitRot || rotateAmount <= 0)
            {
                //回転終了モードに変更
                nowRot = PlayerRot_Mode.finish;
            }
        }
    }

    /// <summary>
    /// 回転処理(壁)
    /// </summary>
    private void Rot_Wall()
    {
        if (nowDire != PlayerDire_Mode.normal)
        {
            //初めて回転モードに入った時
            if (!isStartRotDire)
            {
                //初めて回転モードに入った時のプレイヤーの向いてる方向を取得
                startRotDire = nowDire;
                //この処理は1度だけ
                isStartRotDire = true;
            }

            //回転する量
            float step = rotSpeed * Time.deltaTime;

            //初めて回転モードに入った時のプレイヤーの向いてる方向と現在の方向が同じとき
            if (startRotDire == nowDire)
            {
                //一度に回転する量を超えた時
                if (rotateAmount + step > limitRot)
                {
                    //目標角度で止める
                    step = limitRot - rotateAmount;
                }
            }
            //違うとき
            else
            {
                if (rotateAmount - step < 0)
                {
                    step = 0 + rotateAmount;
                }
            }

            //回転
            //前方
            if (nowDire == PlayerDire_Mode.right)
            {
                //pivotPosを支点にz軸で回転
                this.transform.RotateAround(pivotPos, Vector3.forward, step);

                if (startRotDire == nowDire)
                {
                    //回転した量を追加
                    rotateAmount += step;
                }
                else
                {
                    //回転した量を追加
                    rotateAmount -= step;
                }

            }
            //後方
            else if (nowDire == PlayerDire_Mode.left)
            {
                //pivotPosを支点にz軸で回転
                this.transform.RotateAround(pivotPos, Vector3.forward, -step);

                if (startRotDire == nowDire)
                {
                    //回転した量を追加
                    rotateAmount += step;
                }
                else
                {
                    //回転した量を追加
                    rotateAmount -= step;
                }

            }

            //回転した量が1度に回転できる量以上になった時
            if (rotateAmount >= limitRot || rotateAmount <= 0)
            {
                //回転終了モードに変更
                nowRot = PlayerRot_Mode.finish;
            }
        }
    }

    /// <summary>
    /// 回転処理終了後処理
    /// </summary>
    private void RotateAfter()
    {
        //回転処理終了後の時
        if(nowRot == PlayerRot_Mode.finish)
        {
            //物理挙動を可能にする
            playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

            //現在の向いてる方角が通常以外(右か左)の時
            if (nowDire != PlayerDire_Mode.normal)
            {
                PlayerMove();
            }
        }
    }

    /// <summary>
    /// プレイヤーの速度を変更
    /// </summary>
    /// <param name="_changeSpeed">速度変化の倍率</param>
    public void PlayerSpeedChange(float _changeSpeed)
    {
        //基本速度に変化倍率をかける
        plSpeed = basicSpeed * _changeSpeed;

        //仮　回転速度にも速度倍率をかける
        rotSpeed = rotSpeed * _changeSpeed;
    }
}
