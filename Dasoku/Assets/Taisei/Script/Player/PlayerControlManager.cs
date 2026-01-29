using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControlManager : MonoBehaviour
{
    public static PlayerControlManager Instance;

    //ゲームマネージャー
    [NonSerialized] public GameManager GM;
    //プレイヤーデータ
    [NonSerialized] public PlayerData plData;

    //プレイヤーごとの共通変数
    /// <summary>
    /// プレイヤーの方向
    /// </summary>
    public enum PlayerDire_Mode
    {
        normal,
        right,
        left,
    }
    //現在のプレイヤーの向いてる方向
    public PlayerDire_Mode nowDire = PlayerDire_Mode.normal;

    //地面のレイヤー
    public LayerMask groundLayer;

    //プレイヤー用カメラ
    [SerializeField] private GameObject plCam;

    //蛇足ゲージ
    [SerializeField] private Image DasokuGauge;

    //プレイヤーの移動方向
    [NonSerialized] public Vector2 plVec;
    //プレイヤーの見ている方向
    [NonSerialized] public Vector2 plSeeVec;

    //保存されてるプレイヤーの形態
    [NonSerialized] public PlayerData.PLAYER_MODE saveMode;
    //現在のプレイヤーの形態情報
    [NonSerialized] private PlayerData.PLAYER_STATE nowState;

    //移動キーが何も入力されていないか
    //false=何かが入力されている / true=入力されていない
    private bool isNoMoveInput = false;

    //蛇足ゲージ関連
    //直前のプレイヤーの位置
    private Vector2 beforePos;
    //移動距離
    private float gauge_distanceAmount = 0;
    //ゲージマックスにかかる移動距離
    [SerializeField] private float gauge_MaxDistance = 50;

    private List<Vector2> startOffset = new List<Vector2>();

    private void Awake()
    {
        //シングルトン
        if(Instance == null)
        {
            Instance = this;
        }

        GM = GameManager.Instance;
        //nullチェック
        if(GM == null)
        {
            GM = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        plData = PlayerData.Instance;
        //nullチェック
        if(plData == null)
        {
            plData = this.GetComponent<PlayerData>();
        }
    }

    void Start()
    {
        saveMode = plData.nowMode;
        
        for (int i = 0; i < plData.PlayerObjct.Length; i++)
        {
            startOffset.Add(plData.PlayerObjct[i].transform.localPosition);
        }

        ModeChange();

        //初期速度設定
        plData.SpeedChange(plData.speedMultiplier);

        //開始時のプレイヤーの向いてる方向を設定
        PlayerDirection(plData.nowBody.transform, nowDire);

        //現在の体の位置を取得
        beforePos = plData.nowBody.transform.position;

        //蛇足ゲージを0に
        DasokuGauge.fillAmount = 0;
    }

    void Update()
    {
        if (plData.nowMode != saveMode)
        {
            //形態を変更
            ModeChange();

            PlayerInterface pl = GetPlayerScript();
            pl.FormChange(nowState.headSprite);
        }

    }

    private void FixedUpdate()
    {
        //ゲームクリアしてる時はこれ以上処理を行わない
        if (GM.isClear)
        {
            Rigidbody2D rb = plData.nowBody.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            return;
        }

        //スクリプト取得
        PlayerInterface pl = GetPlayerScript();
        //nullチェック
        if (pl == null)
        {
            return;
        }

        //入力処理
        PlayerInput(pl);

        //更新
        pl.PlUpdate();

        //カメラをプレイヤーの位置に移動
        Vector3 plPos = plData.nowBody.transform.position;
        plCam.transform.position = new Vector3(plPos.x, plPos.y, plCam.transform.position.z);

        //蛇足ゲージ処理
        //移動距離計算
        float dis = (float)Math.Truncate(Vector2.Distance(beforePos, plData.nowBody.transform.position) * 100) / 100;
        //移動距離が0より多いとき
        if (dis > 0)
        {
            //移動量を加算
            gauge_distanceAmount += dis;
            //移動量が最大値を超えてたら、最大値を入れる
            gauge_distanceAmount = Mathf.Min(gauge_distanceAmount, gauge_MaxDistance);
            //移動量をもとにゲージを増やす
            DasokuGauge.fillAmount = gauge_distanceAmount / gauge_MaxDistance;

            //ゲージが最大になった時
            if (DasokuGauge.fillAmount >= 1)
            {
                //ゲージ量、移動量を0に戻す
                DasokuGauge.fillAmount = 0;
                gauge_distanceAmount = 0;
            }
        }
        //直前の位置を更新
        beforePos = plData.nowBody.transform.position;

    }

    /// <summary>
    /// 現在の状態に合わせてプレイヤースクリプトを取得
    /// </summary>
    private PlayerInterface GetPlayerScript()
    {
        int index = (int)plData.nowControl;

        //バグチェック
        if (index < 0 || plData.plScripts.Length <= 0)
        {
            return null;
        }

        return plData.plScripts[index] as PlayerInterface;
    }

    /// <summary>
    /// 入力処理
    /// </summary>
    private void PlayerInput(PlayerInterface _pl)
    {
        #region 移動入力
        //形態がケツァルコアトルの時のみ
        if (plData.nowMode == PlayerData.PLAYER_MODE.quetzalcoatl)
        {
            //上下移動キーを押してる時
            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                //上
                if (Input.GetKey(KeyCode.W))
                {
                    _pl.InputUp();
                }
                //下
                else if (Input.GetKey(KeyCode.S))
                {
                    _pl.InputDown();
                }
                isNoMoveInput = false;
            }
            //上下キーのどちらかを離したとき
            else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            {
                _pl.InputUDUp();
                isNoMoveInput = true;
            }
        }

        //左右移動キーを押してる時
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            //左
            if (Input.GetKey(KeyCode.A))
            {
                _pl.InputLeft();
            }
            //右
            else if (Input.GetKey(KeyCode.D))
            {
                _pl.InputRight();
            }
            isNoMoveInput = false;
        }
        else if(Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            _pl.InputLRUp();
            isNoMoveInput = true;
        }

        if (isNoMoveInput)
        {
            _pl.NoInput();
        }

        #endregion

        #region アクション入力
        //アクション
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _pl.InputActionDown();
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            _pl.InputAction();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            _pl.InputActionUp();
        }

        #endregion
    }

    /// <summary>
    /// 形態を変更
    /// </summary>
    private void ModeChange()
    {
        //保存されてる状態を現在の更新
        saveMode = plData.nowMode;
        //現在の形態のデータを取得
        nowState = plData.SearchState(plData.nowMode);
        //スクリプトを変更
        plData.nowControl = nowState.control;
        //身体を変更
        plData.nowBodyType = nowState.bodyType;

        BodyChange();
    }

    /// <summary>
    /// 体オブジェクトを変更+表示非表示変更
    /// </summary>
    private void BodyChange()
    {
        int _index = (int)plData.nowBodyType;
        //バグチェック
        if (_index < 0 || plData.PlayerObjct.Length <= 0)
        {
            return;
        }

        //プレイヤーオブジェクトの座標が取得で来てるかどうか
        bool _isGetPlObj = false;
        Vector2 _pos = Vector2.zero;
        Vector2 _offset = Vector2.zero;
        if(plData.nowBody != null)
        {
            //形態変更前のプレイヤーの位置を取得
            _pos = plData.nowBody.transform.position;
            _isGetPlObj = true;
        }

        //現在の体オブジェクトを取得
        plData.nowBody = plData.PlayerObjct[_index];
        //現在の体オブジェクトを表示、それ以外を非表示にする
        for(int i= 0; i < plData.PlayerObjct.Length; i++)
        {
            if(i == _index)
            {
                plData.PlayerObjct[i].SetActive(true);
                _offset = startOffset[i];
            }
            else
            {
                plData.PlayerObjct[i].SetActive(false);
            }
        }

        //プレイヤーオブジェクトの座標が取得できてる時
        if (_isGetPlObj)
        {
            //プレイヤーの位置を調整
            plData.nowBody.transform.position = _pos;
            plData.nowBody.transform.localPosition += (Vector3)_offset;
        }
    }

    /// <summary>
    /// プレイヤーの向いている方向を変更
    /// </summary>
    /// <param name="_isRight">false=左 / true=右</param>
    public void PlayerDirection(Transform pl, PlayerDire_Mode plDire)
    {
        Vector3 plSize = pl.transform.localScale;
        //左
        if (plDire == PlayerDire_Mode.left)
        {
            //マイナスにして、見た目を左向きにする
            plSize.x = -(Mathf.Abs(plSize.x));

            plSeeVec = -pl.transform.right;
        }
        //右
        else if (plDire == PlayerDire_Mode.right)
        {
            //マイナスをなくして、見た目を右向きにする
            plSize.x = Mathf.Abs(plSize.x);
            plSeeVec = pl.transform.right;
        }
        nowDire = plDire;
        pl.transform.localScale = plSize;
    }

}
