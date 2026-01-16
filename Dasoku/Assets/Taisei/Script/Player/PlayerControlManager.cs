using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    public static PlayerControlManager Instance;

    //ゲームマネージャー
    [System.NonSerialized] public GameManager GM;
    //プレイヤーデータ
    [System.NonSerialized] public PlayerData plData;

    //プレイヤー用カメラ
    [SerializeField] private GameObject plCam;  

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

    //地面のレイヤー
    [SerializeField] public LayerMask groundLayer;

    //プレイヤーの移動方向
    [System.NonSerialized] public Vector2 plVec;
    //プレイヤーの見ている方向
    public Vector2 plSeeVec;

    //保存されてるプレイヤーの形態
    [System.NonSerialized] public PlayerData.PLAYER_MODE saveMode;
    //現在のプレイヤーの形態情報
    [System.NonSerialized] private PlayerData.PLAYER_STATE nowState;

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

        ModeChange();
        PlayerInterface pl = GetPlayerScript();
        pl.FormChange(nowState.headSprite);
    }

    void Update()
    {
        //スクリプト取得
        PlayerInterface pl = GetPlayerScript();
        //nullチェック
        if(pl == null)
        {
            return;
        }

        //入力処理
        PlayerInput(pl);

        //更新
        pl.PlUpdate();

        if (plData.nowMode != saveMode)
        {
            //形態を変更
            ModeChange();
            //見た目を変更
            pl.FormChange(nowState.headSprite);
        }

        Vector3 plPos = plData.nowBody.transform.position;
        plCam.transform.position = new Vector3(plPos.x, plPos.y, plCam.transform.position.z);
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
        //身体を変更
        plData.nowBodyType = nowState.bodyType;

        BodyChange();
    }

    /// <summary>
    /// 体オブジェクトを変更+表示非表示変更
    /// </summary>
    private void BodyChange()
    {
        int index = (int)plData.nowBodyType;
        //バグチェック
        if (index < 0 || plData.PlayerObjct.Length <= 0)
        {
            return;
        }

        //現在の体オブジェクトを取得
        plData.nowBody = plData.PlayerObjct[index];
        //現在の体オブジェクトを表示、それ以外を非表示にする
        for(int i= 0; i < plData.PlayerObjct.Length; i++)
        {
            if(i == index)
            {
                plData.PlayerObjct[i].SetActive(true);
            }
            else
            {
                plData.PlayerObjct[i].SetActive(false);
            }
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
