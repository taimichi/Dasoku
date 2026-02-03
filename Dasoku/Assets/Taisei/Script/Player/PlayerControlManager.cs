using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public partial class PlayerControlManager : MonoBehaviour
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
    //移動時間
    private float gauge_distanceAmount = 0;
    //ゲージマックスにかかる移動移動時間
    [SerializeField] private float gauge_MaxTime = 3;

    private List<Vector2> startOffset = new List<Vector2>();

    //抽選用
    private LotteryData lottery;

    //煙幕のプレハブ
    [SerializeField] private GameObject SmokePre;
    //形態変化アニメーション中かどうか
    private bool isFormChangeAnim = false;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text dasokuNameText;
    [SerializeField] private Text dasokuEffectText;

    //ウロボロス、ケツァルコアトル、蛇神に変化したかどうか
    private bool isOuroboros = false;
    private bool isQuetzalcoatl = false;
    private bool isSnakeGod = false;

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

        lottery = LotteryData.LotteryEntity;
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

        canvasGroup.alpha = 0;

        DasokuReset();
    }

    void Update()
    {
        //ゲームクリアしてる時はこれ以上処理を行わない
        if (GM.isClear)
        {
            Rigidbody2D rb = plData.nowBody.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            return;
        }

        //メニュー開いてる時は操作なしに
        if (GM.isMenu)
        {
            return;
        }

        //形態変更アニメーション中は以下の処理をやらない
        if (isFormChangeAnim)
        {
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

        //蛇足ゲージの処理
        GaugeUpdate();

        //身体の数が8以上の時
        if(plData.bodyNum >= 8 && !isOuroboros)
        {
            //ウロボロスに強制的に変化
            plData.nowMode = PlayerData.PLAYER_MODE.ouroboros;
            isOuroboros = true;
        }

        //形態が変化した時
        if (plData.nowMode != saveMode)
        {
            isFormChangeAnim = true;
            StartCoroutine(ChangeAnime(pl));

        }

    }

    private IEnumerator ChangeAnime(PlayerInterface _nowPl)
    {
        //地面に触れてないときは形態変化をしない
        while (!_nowPl.CheckStandGround())
        {
            yield return null;
        }

        //プレイヤーの速度を0に
        Rigidbody2D _rb = plData.nowBody.GetComponent<Rigidbody2D>();
        _rb.velocity = Vector2.zero;

        //煙を出現させる位置を設定
        Vector2 _pos;
        if(plData.nowBodyType == PlayerData.PLAYER_BODYTYPE.snake)
        {
            GameObject _obj = GameObject.Find("Bodys");
            int _childCount = _obj.transform.childCount;
            _pos = _obj.transform.GetChild(_childCount / 2).position;
        }
        else
        {
            _pos = plData.nowBody.transform.position;
        }

        //煙の大きさを設定
        Vector2 _size;
        if(plData.bodyNum < 5)
        {
            _size = new Vector2(3, 3);
        }
        else
        {
            _size = new Vector2(4, 4);
        }

        //煙を生成
        GameObject _smoke = Instantiate(SmokePre,_pos,Quaternion.identity);
        _smoke.transform.localScale = _size;

        //煙のアニメーター取得
        Animator _anim = _smoke.GetComponent<Animator>();
        //１フレーム待機
        yield return null;

        //煙のアニメーションの半分が経過するまで待機
        while(_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5)
        {
            yield return null;
        }

        //形態を変更
        ModeChange();

        PlayerInterface newPl = GetPlayerScript();
        newPl.FormChange(nowState.headSprite);

        //テキストを変更
        canvasGroup.alpha = 1;
        dasokuNameText.text = nowState.name;
        dasokuEffectText.text = nowState.explanation;

        isFormChangeAnim = false;
    }

    /// <summary>
    /// 蛇足ゲージの処理
    /// </summary>
    private void GaugeUpdate()
    {
        //移動距離計算
        float dis = (float)Math.Truncate(Vector2.Distance(beforePos, plData.nowBody.transform.position) * 100) / 100;
        //移動距離が0より多いとき
        if (dis > 0)
        {
            //移動時間を加算
            gauge_distanceAmount += Time.deltaTime * plData.meterSpeed;
            //移動時間が最大値を超えてたら、最大値を入れる
            gauge_distanceAmount = Mathf.Min(gauge_distanceAmount, gauge_MaxTime);
            //移動時間をもとにゲージを増やす
            DasokuGauge.fillAmount = gauge_distanceAmount / gauge_MaxTime;

            if(plData.nowBody.transform.position.x >= 22 && CheckTutorial())
            {
                DasokuGauge.fillAmount = 1;
            }

            //ゲージが最大になった時
            if (DasokuGauge.fillAmount >= 1)
            {
                if (CheckTutorial())
                {
                    SpecialLottery_Form(PlayerData.PLAYER_MODE.snakeReg);
                }
                else
                {
                    //抽選処理
                    LotteryFunction();
                }

                //ゲージ量、移動量を0に戻す
                DasokuGauge.fillAmount = 0;
                gauge_distanceAmount = 0;
            }
        }
        //直前の位置を更新
        beforePos = plData.nowBody.transform.position;
    }

    private bool CheckTutorial()
    {
        bool _isTutorial = SceneManager.GetActiveScene().name == "Tutorial";
        return _isTutorial;
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
