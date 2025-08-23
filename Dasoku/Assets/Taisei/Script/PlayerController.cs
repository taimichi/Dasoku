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
    //回転時の角度の変化量
    private float rotAmountChange = 0f;

    /// <summary>
    /// プレイヤーの方向
    /// </summary>
    private enum PlayerDire_Mode
    {
        right,
        left,
    }

    [SerializeField, Header("プレイヤーの向いてる方向")] private PlayerDire_Mode nowDire = PlayerDire_Mode.right;

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
        PlayerDirection(nowDire);
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
            Debug.Log("移動中");
            //移動処理
            PlayerMove();
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

                //プレイヤーが左を向いてる状態だったら
                if (nowDire == PlayerDire_Mode.left)
                {
                    rotAmountChange = limitRot;
                }
            }
            //回転処理
            PlayerRotate();
        }

        //重力の変更の有無
        gravityChanger.GravityChange(CheckUseGravity());
        //重力用の更新
        gravityChanger.GravityUpdate();

        //重力の方向設定
        Vector2 gravityDire = -this.transform.up;
        //小数点以下を切り捨て
        gravityDire.x = (int) gravityDire.x;
        gravityDire.y = (int)gravityDire.y;

        //重力の方向変更
        gravityChanger.ChangeGravityDirection(gravityDire);


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
        nowDire = plDire;
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
        //レイの長さ
        float duration = 0.7f;
        //レイ　プレイヤーの中心から下方向に向けて
        Ray2D ray = new Ray2D(this.transform.position, -this.transform.up);

        //レイ表示
        Debug.DrawRay(ray.origin, ray.direction * duration, Color.green);

        //レイがオブジェクトを取得した個数分ループ
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, duration))
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
    /// プレイヤーの回転処理
    /// </summary>
    private void PlayerRotate()
    {
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        //重力の方向設定
        Vector2 gravityDire = -this.transform.up;
        //小数点以下を切り捨て
        gravityDire.x = Mathf.FloorToInt(gravityDire.x);
        gravityDire.y = Mathf.FloorToInt(gravityDire.y);

        //重力の方向変更
        gravityChanger.ChangeGravityDirection(gravityDire);

        //回転スピード
        float rotSpeed = Mathf.Floor((limitRot / rotationTime * Time.deltaTime) * 100) / 100;

        //回転処理
        //右向いてる時
        if (nowDire == PlayerDire_Mode.right)
        {
            //while (rotAmountChange <= limitRot)
            //{
            //    //現在の角度を取得
            //    Vector3 PLRot = RotPoint.localEulerAngles;

            //    PLRot.z += -rotSpeed;
            //    rotAmountChange += rotSpeed;

            //    //角度を変更
            //    RotPoint.localEulerAngles = PLRot;

            //    yield return null;
            //}

            Vector3 PLRot = RotPoint.localEulerAngles;
            PLRot.z = Mathf.FloorToInt(PLRot.z) - 90;
            RotPoint.localEulerAngles = PLRot;
        }
        //左向きの時
        else if (nowDire == PlayerDire_Mode.left)
        {
            //while (rotAmountChange <= limitRot)
            //{
            //    //現在の角度を取得
            //    Vector3 PLRot = RotPoint.localEulerAngles;

            //    PLRot.z += rotSpeed;
            //    rotAmountChange += rotSpeed;

            //    //角度を変更
            //    RotPoint.localEulerAngles = PLRot;

            //    yield return null;
            //}

            Vector3 PLRot = RotPoint.localEulerAngles;
            PLRot.z = Mathf.FloorToInt(PLRot.z) + 90;
            RotPoint.localEulerAngles = PLRot;
        }

        yield return null;
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
        rotAmountChange = 0f;
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
