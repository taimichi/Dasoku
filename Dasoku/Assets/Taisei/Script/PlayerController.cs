using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //プレイヤーの基礎速度
    [SerializeField, Header("基礎速度")] private float basicSpeed = 1.0f;
    //プレイヤーの移動速度
    private float plSpeed = 1.0f;
    //プレイヤーの移動方向
    private Vector2 plVec;
    //プレイヤーのリジッドボディ
    private Rigidbody2D playerRB;

    //90度回転にかかる時間
    private float rotationTime = 1f;

    //重力変更用スクリプト
    private LocalGravityChanger gravityChanger = new LocalGravityChanger();

    //一度に回転できる角度
    private int limitRot = 90;

    //変更前のz軸の角度
    private float beforeRotZ = 0;

    /// <summary>
    /// プレイヤーの方向
    /// </summary>
    private enum PlayerDire_Mode
    {
        right,
        left,
    }

    [SerializeField, Header("プレイヤー　開始時の向いてる方向")] private PlayerDire_Mode startDire = PlayerDire_Mode.right;

    //回転時の中心座標
    private Vector2 saveRotPos;
    //回転を開始したか
    private bool isRot = false;
    //回転時にプレイヤーの親オブジェクトとなるオブジェクト
    //回転の位置を変更するため
    [SerializeField] private Transform RotPoint;

    void Start()
    {
        //プレイヤーのリジッドボディを取得
        playerRB = this.GetComponent<Rigidbody2D>();
        //初期速度設定
        PlayerSpeedChange(1.0f);

        //プレイヤーのリジッドボディを重力変更スクリプトに渡す
        gravityChanger.SetGravityChange(playerRB);

        //開始時のプレイヤーの向いてる方向を設定
        PlayerDirection(startDire);
    }

    private void Update()
    {

    }


    private void FixedUpdate()
    {
        //プレイヤーの入力
        PlayerInput();

        //回転中じゃなく、空中じゃないとき
        //回転中か空中では移動しない
        if (!CheckNowRotate() && !CheckNowAir())
        {
            //移動処理
            PlayerMove();
            //空中になる前の角度を取得
            beforeRotZ = RotPoint.localEulerAngles.z;
        }
        else
        {
            if (!isRot)
            {
                //回転位置を変更
                RotPoint.position = saveRotPos;
                //回転位置用オブジェクトをプレイヤーの親オブジェクトに
                this.transform.parent = RotPoint;
                isRot = true;
            }
            //回転処理
            PlayerRotate();
        }
        //重力の変更の有無
        gravityChanger.GravityChange(CheckUseGravity());

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
        else
        {
            plVec = Vector2.zero;
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
        
        this.transform.localScale = plSize;
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    private void PlayerMove()
    {
        //プレイヤー移動処理
        Vector2 plVelo = playerRB.velocity;
        plVelo = plVec * plSpeed;
        playerRB.velocity = plVelo;
    }

    /// <summary>
    /// 空中かどうか
    /// </summary>
    /// <returns>false=地面にいる / true=空中にいる</returns>
    private bool CheckNowAir()
    {
        //レイ　プレイヤーの中心から下方向に向けて
        Ray2D ray = new Ray2D(this.transform.position, -this.transform.up);

        //レイ表示
        Debug.DrawRay(ray.origin, ray.direction * 0.6f, Color.green);

        //レイがオブジェクトを取得した個数分ループ
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, 0.6f))
        {
            //オブジェクトと接触しているとき
            if (hit.collider)
            {
                //接触したのが地面だったら
                if (hit.collider.tag == "Ground_Normal")
                {
                    //回転中じゃないとき
                    if (!CheckNowRotate())
                    {
                        //親オブジェクトを解除
                        this.transform.parent = null;
                        //回転時の中心座標を更新
                        saveRotPos = hit.point;
                        //回転開始フラグをオフに
                        isRot = false;
                    }
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 疑似重力を使うかどうか
    /// </summary>
    /// <returns>false=疑似重力が必要 / true=疑似重力が必要</returns>
    private bool CheckUseGravity()
    {
        //現在のz軸の角度を取得
        float nowRotZ = RotPoint.localEulerAngles.z;

        //角度が下向き以外の時
        if(nowRotZ % 360 != 0)
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
        //重力の方向変更
        gravityChanger.ChangeGravityDirection(-this.transform.up);

        //回転スピード
        float rotSpeed = limitRot / rotationTime * Time.deltaTime;
        //現在の角度を取得
        Vector3 PLRot = RotPoint.localEulerAngles;

        //移動キーが押されてる時
        //Dキー(進む)が押されてる時
        if (Input.GetKey(KeyCode.D))
        {
            //角度が一度に回転できる量以内なら
            if (PLRot.z > beforeRotZ - limitRot)
            {
                PLRot.z -= rotSpeed;
            }
            //一度に回転できる量を超えた時
            else
            {
                
                PLRot.z = beforeRotZ - limitRot;
            }
        }
        //Aキー(戻る)が押されてる時
        else if (Input.GetKey(KeyCode.A))
        {
            //角度が一度に回転できる量以内なら
            if (PLRot.z < beforeRotZ)
            {
                PLRot.z += rotSpeed;
            }
            //一度に回転できる量を超えた時
            else
            {
                PLRot.z = beforeRotZ;
            }
        }
        //角度を変更
        RotPoint.localEulerAngles = PLRot;

    }

    /// <summary>
    /// 回転中かどうか
    /// </summary>
    /// <returns>false=回転してない / true=回転中</returns>
    private bool CheckNowRotate()
    {
        //現在のz軸の角度を取得
        int nowRotZ = Mathf.FloorToInt(Mathf.Abs(RotPoint.transform.localEulerAngles.z));
        //90度で割り切れないとき
        if(nowRotZ % limitRot != 0)
        {
            //回転中はプレイヤーが移動しないようにする
            playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

            Debug.Log("回転中");
            //回転中
            return true;   
        }
        //移動できるようにする
        playerRB.constraints = RigidbodyConstraints2D.None;
        Debug.Log("回転していない");
        //回転してない
        return false; ;
    }

    /// <summary>
    /// プレイヤーの速度を変更
    /// </summary>
    /// <param name="_changeSpeed">速度変化の倍率</param>
    public void PlayerSpeedChange(float _changeSpeed)
    {
        //基本速度に変化倍率をかける
        plSpeed = basicSpeed * _changeSpeed;
    }
}
