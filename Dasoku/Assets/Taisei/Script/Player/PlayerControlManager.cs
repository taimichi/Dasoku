using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    public static PlayerControlManager Instance;

    [System.NonSerialized] public GameManager GM;
    [System.NonSerialized] public PlayerData plData;

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
    public PlayerDire_Mode nowDire = PlayerDire_Mode.right;

    [SerializeField] public LayerMask groundLayer;

    //プレイヤーの移動方向
    [System.NonSerialized] public Vector2 plVec;
    //プレイヤーの見ている方向
    public Vector2 plSeeVec;

    [System.NonSerialized] public PlayerData.PLAYER_MODE saveMode;
    [System.NonSerialized] private PlayerData.PLAYER_STATE nowState;


    private void Awake()
    {
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

        ModeChange();
    }

    void Update()
    {
        int index = (int)plData.nowControl;

        //バグチェック
        if(index < 0 || plData.plScripts.Length <= 0)
        {
            return;
        }

        //スクリプト取得
        PlayerInterface pl = plData.plScripts[index] as PlayerInterface;
        //nullチェック
        if(pl == null)
        {
            return;
        }

        //入力処理
        PlayerInput(pl);

        //nullじゃないときのみ実行
        pl.PlUpdate();

        if (plData.nowMode != saveMode)
        {
            //形態を変更
            ModeChange();
            //見た目を変更
            pl.FormChange(nowState.headSprite);
        }
    }

    /// <summary>
    /// 入力処理
    /// </summary>
    private void PlayerInput(PlayerInterface _pl)
    {
        #region 移動入力
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
        else if(Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            _pl.InputLRUp();
        }
        else
        {
            _pl.NoInputLR();
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
