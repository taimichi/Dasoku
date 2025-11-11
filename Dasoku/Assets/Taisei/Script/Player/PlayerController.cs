using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//プレイヤーに関するスクリプト(操作など)
public class PlayerController : MonoBehaviour
{
    //プレイヤーデータ
    [SerializeField] private PlayerData PlData;

    //プレイヤーの見た目オブジェクト
    [SerializeField] private GameObject PlAppearance;
    //プレイヤーの見た目
    private SpriteRenderer PlSpriteRenderer;

    //プレイヤーのコライダー
    private BoxCollider2D box;

    //プレイヤーのリジッドボディ
    private Rigidbody2D playerRB;

    //地面用のレイヤー
    [SerializeField, Tooltip("地面のレイヤー")] private LayerMask groundLayer;

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

    #region 移動関連
    [Header("移動関連")]
    //速度上限
    [SerializeField, Tooltip("速度上限")] private Vector2 maxVelocity = new Vector2(20, 20);

    //最終的なプレイヤーの移動速度
    private float plSpeed = 1.0f;

    //プレイヤーの移動方向
    private Vector2 plVec;

    //地面判定を取る距離
    private float groundCheckDistance = 0.75f;
    #endregion

    #region 回転関連
    [Header("回転関連")]
    //最終的なプレイヤーの回転速度
    private float plRotSpeed = 180f;

    //一度に回転できる角度
    private float limitRot = 90f;

    //回転する際の支点
    private Vector3 pivotPos;

    //どこまで回転したか
    private float rotateAmount = 0f;

    //回転するかどうか
    private bool isRot = false;

    //回転位置の取得用のオブジェクト
    [SerializeField, Header("回転位置")] private Transform pivotPoint;

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

    #region 胴体関連
    [Header("胴体関連")]
    [Tooltip("胴体隙間")] public float segmentSpacing = 0.5f;

    [Tooltip("胴体の更新距離")] public float pointSpacing = 0.04f;

    [Tooltip("胴体のプレハブ")] public Transform bodyPrefab;

    //生成した胴体のリスト
    public List<Transform> segments = new List<Transform>();

    //座標の履歴
    private List<Vector3> posHistory = new List<Vector3>();

    //回転値の履歴
    private List<Vector3> rotHistory = new List<Vector3>();

    //スケール値の履歴
    private List<Vector3> sizeHistory = new List<Vector3>();

    /// <summary>
    /// 保持する履歴の最大数
    /// </summary>
    private int MaxHistoryCount
    {
        get
        {
            //胴体が追従する距離分 + 少し余裕
            return (segments.Count + 10) * Mathf.RoundToInt(segmentSpacing / pointSpacing);
        }
    }

    private bool isAllBodyGroundCheck = false;
    #endregion

    //重力操作用スクリプト
    private LocalGravityChanger gravityChanger = new LocalGravityChanger();

    private void Awake()
    {
        //開始時の座標と回転値を履歴に追加
        posHistory.Add(this.transform.position);
        rotHistory.Add(this.transform.localEulerAngles);
        sizeHistory.Add(this.transform.localScale);

        //初期設定の胴体の数生成
        for (int i = 0; i < PlData.bodyNum; i++)
        {
            //胴体の生成
            AddSegment();

            //最後の胴体追加の時
            if (i == PlData.bodyNum - 1)
            {
                //最後の胴体はしっぽのため、名前変更
                segments[i].name = "Tail";
            }
        }
    }

    void Start()
    {
        //プレイヤーの見た目オブジェクトのスプライトレンダラーを取得
        PlSpriteRenderer = PlAppearance.GetComponent<SpriteRenderer>();

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
        //胴体の表示(見た目上)
        RecordPosition();
        UpdateSegments();

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

            //落下判定+処理
            PlayerFall();
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



        //重力処理
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
        #region A,Dキー
        //右方向
        if (Input.GetKey(KeyCode.D))
        {
            //プレイヤーの右向きベクトルを取得
            plVec = this.transform.right;

            //plVecのx、yの数値がそれぞれ0.00001未満なら0として扱う
            //x
            NumTolerance(plVec.x);
            //y
            NumTolerance(plVec.y);

            //向いてる方向を右に変更
            PlayerDirection(PlayerDire_Mode.right);
        }
        //左方向
        else if (Input.GetKey(KeyCode.A))
        {
            //プレイヤーの左向きベクトルを取得
            plVec = -this.transform.right;

            //plVecのx、yの数値がそれぞれ0.00001未満なら0として扱う
            //x
            NumTolerance(plVec.x);
            //y
            NumTolerance(plVec.y);

            //向いてる方向を左に変更
            PlayerDirection(PlayerDire_Mode.left);
        }
        //移動キーを話したとき
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

        #region アクションキー(スペースキー)
        //スペースキーを押したとき
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isRot)
            {
                //はりつき解除
                this.transform.localEulerAngles = Vector3.zero;
            }
        }
        #endregion

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
        }
        //右
        else if(plDire == PlayerDire_Mode.right)
        {
            //マイナスをなくして、見た目を右向きにする
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
        //現在のプレイヤーの速度を取得
        Vector2 newPLVelo = playerRB.velocity;

        //横移動の時
        if (plVec.x != 0)
        {
            //移動速度を計算
            newPLVelo.x = plVec.x * plSpeed;

            //速度が上限を超えた時
            if(newPLVelo.x > maxVelocity.x)
            {
                //上限速度に設定
                newPLVelo.x = maxVelocity.x;
            }
        }
        //縦移動の時
        if (plVec.y != 0)
        {
            //移動速度を計算
            newPLVelo.y = plVec.y * plSpeed;

            //速度が上限を超えた時
            if (newPLVelo.y > maxVelocity.y)
            {
                //速度が上限を超えた時
                newPLVelo.y = maxVelocity.y;
            }
        }

        //velocity変更
        playerRB.velocity = newPLVelo;
    }

    /// <summary>
    /// プレイヤーの落下処理
    /// </summary>
    private void PlayerFall()
    {
        //地面に触れてないとき
        if (!CheckGround() && !isRayOut)
        {
            //プレイヤーの見た目の向きを下向きにする
            if (nowDire == PlayerDire_Mode.left)
            {
                PlAppearance.transform.localEulerAngles = new Vector3(0, 0, -90);
            }
            else if(nowDire == PlayerDire_Mode.right)
            {
                PlAppearance.transform.localEulerAngles = new Vector3(0, 0, -90);
            }
        }
        //地面に触れてるとき
        else
        {
            //プレイヤーの見た目の向きを通常に戻す
            PlAppearance.transform.localEulerAngles = Vector3.zero;
        }
    }

    /// <summary>
    /// 地面判定用
    /// </summary>
    /// <returns>false=地面と触れてない / true=地面と触れてる</returns>
    private bool CheckGround()
    {
        //地面判定用
        bool isGround ;

        Vector2 size = box.size;                    //コライダーのサイズ
        Vector2 offset = box.offset;                //コライダーのオフセット
        Vector2 down = -this.transform.up;          //プレイヤーの下方向ベクトル

        //座標計算 y座標はコライダーの途中から出したいため/2ではなく/4に
        //真ん中
        Vector2 localBottom_center = offset + new Vector2(0, -size.y / 4f);
        Vector2 bottom_center = transform.TransformPoint(localBottom_center);
        //左
        Vector2 localBottom_left = offset + new Vector2(-size.x / 2f, -size.y / 4f);
        Vector2 bottom_left = transform.TransformPoint(localBottom_left);
        //右
        Vector2 localBottom_right = offset + new Vector2(size.x / 2f, -size.y / 4f);
        Vector2 bottom_right = transform.TransformPoint(localBottom_right);

        //レイ判定
        bool isCenter = Physics2D.Raycast(bottom_center, down, groundCheckDistance, groundLayer);
        bool isLeft = Physics2D.Raycast(bottom_left, down, groundCheckDistance, groundLayer);
        bool isRight = Physics2D.Raycast(bottom_right, down, groundCheckDistance, groundLayer);


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

        #region デバッグ用_レイ表示
        Debug.DrawRay(bottom_center, down * groundCheckDistance, Color.blue);
        Debug.DrawRay(bottom_left, down * groundCheckDistance, Color.blue);
        Debug.DrawRay(bottom_right, down * groundCheckDistance, Color.blue);
        #endregion

        return isGround;
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
    /// プレイヤーの回転のまとめ
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
            float step = plRotSpeed * Time.deltaTime;

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
            float step = plRotSpeed * Time.deltaTime;

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
    /// 座標、回転値の保存
    /// </summary>
    private void RecordPosition()
    {
        //胴体が空中にあるかチェック
        for(int i = 0; i < segments.Count; i++)
        {
            Body body = segments[i].GetComponent<Body>();
            if (!body.CheckBodyGround())
            {
                //1つでも空中にあった場合、ループから抜け出す
                isAllBodyGroundCheck = false;
                break;
            }
            isAllBodyGroundCheck = true;
        }

        //胴体が全て地上と接しているとき
        if (isAllBodyGroundCheck)
        {
            //保存
            //最後の履歴と現在の頭の距離が更新距離を超えた時
            if (Vector3.Distance(posHistory[posHistory.Count - 1], this.transform.position) > pointSpacing)
            {
                //座標の履歴追加
                posHistory.Add(this.transform.position);
                //回転値の履歴追加
                rotHistory.Add(this.transform.localEulerAngles);
                //スケール値の履歴追加
                sizeHistory.Add(this.transform.localScale);

                //メモリ対策用:古い履歴の削除
                //履歴の最大数を取得
                int maxHistory = MaxHistoryCount;
                //履歴の数が最大数を超えた時
                if (posHistory.Count > maxHistory)
                {
                    //それぞれの古い履歴削除
                    posHistory.RemoveAt(0);
                    rotHistory.RemoveAt(0);
                    sizeHistory.RemoveAt(0);
                }
            }
        }
        //胴体が1つでも空中にある時
        else
        {
            //座標の履歴追加
            posHistory.Add(this.transform.position);
            //回転値の履歴追加
            rotHistory.Add(this.transform.localEulerAngles);
            //スケール値の履歴追加
            sizeHistory.Add(this.transform.localScale);

            //メモリ対策用:古い履歴の削除
            //履歴の最大数を取得
            int maxHistory = MaxHistoryCount;
            //履歴の数が最大数を超えた時
            if (posHistory.Count > maxHistory)
            {
                //それぞれの古い履歴削除
                posHistory.RemoveAt(0);
                rotHistory.RemoveAt(0);
                sizeHistory.RemoveAt(0);
            }
        }
    }
    /// <summary>
    /// 胴体の更新
    /// </summary>
    private void UpdateSegments()
    {
        //胴体同士がどれくらい離れているか計算
        int stepsPerSeg = Mathf.RoundToInt(segmentSpacing / pointSpacing);

        for (int i = 0; i < segments.Count; i++)
        {
            //頭が通過した履歴のどの地点の座標を参照するか
            int index = Mathf.Max(0, posHistory.Count - 1 - stepsPerSeg * (i + 1));
            //座標更新
            segments[i].position = posHistory[index];

            //頭が通過した履歴のどの地点の回転値を参照するか
            int rotIndex = Mathf.Max(0, rotHistory.Count - 1 - stepsPerSeg * (i + 1));
            //回転値更新
            segments[i].localEulerAngles = rotHistory[rotIndex];

            //頭が通過した履歴のどの地点のスケール値を参照するか
            int sizeIndex = Mathf.Max(0, sizeHistory.Count - 1 - stepsPerSeg * (i + 1));
            //スケール値更新
            segments[i].localScale = sizeHistory[sizeIndex];
        }
    }

    /// <summary>
    /// プレイヤーの速度を変更
    /// </summary>
    /// <param name="_changeSpeed">速度変化の倍率</param>
    public void PlayerSpeedChange(float _changeSpeed)
    {
        //基本速度に変化倍率をかける
        plSpeed = PlData.basicSpeed * _changeSpeed;

        //仮　回転速度にも速度倍率をかける
        plRotSpeed = PlData.rotSpeed * _changeSpeed;
    }

    /// <summary>
    /// 胴体の追加
    /// </summary>
    public void AddSegment()
    {
        //胴体を生成
        Transform seg = Instantiate(bodyPrefab, transform.position, Quaternion.identity, transform.parent);
        //胴体リストに追加
        segments.Add(seg);
    }

    /// <summary>
    /// 回転中かどうかを返す
    /// </summary>
    /// <returns>false=回転中じゃない / true=回転中</returns>
    public bool ReturnIsRot() => isRot;

    /// <summary>
    /// 胴体の数を返す
    /// </summary>
    /// <returns>頭を除く胴体の数(しっぽは含む)</returns>
    public int ReturnBodyNum() => PlData.bodyNum;

    /// <summary>
    /// プレイヤーの下ベクトルを返す
    /// </summary>
    /// <returns>プレイヤーの下ベクトル</returns>
    public Vector3 ReturnPlayerDownVec() => -this.transform.up;
}
