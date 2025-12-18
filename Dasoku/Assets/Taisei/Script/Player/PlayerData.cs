using UnityEngine;

//プレイヤーの各データや状態を保持するスクリプト
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    #region プレイヤーステータス
    //基礎速度
    [Tooltip("基礎速度")] public float moveSpeed;
    //速度上限
    [SerializeField, Tooltip("速度上限")] public Vector2 maxVelocity = new Vector2(20, 20);
    //回転速度
    [Tooltip("1秒に回転する角度")] public float RotSpeed;
    //胴体の数
    [Tooltip("胴体の数")] public int bodyNum = 4;
    //現在の形態
    [Tooltip("現在の形態")] public PLAYER_MODE nowMode = PLAYER_MODE.normal;
    //物をつかんでいるかどうか
    [Tooltip("物をつかんでるか")] public bool isCatchObj = false;
    #endregion

    //蛇の形態
    public enum PLAYER_MODE
    {
        normal,         //蛇
        snakeReg,       //蛇足
        snakeReg_Pro,   //蛇足プロ
        foot,           //足
        heel,           //ヒール
        snakeHand,      //蛇手
        strongArm,      //剛腕
        ouroboros,      //ウロボロス
        quetzalcoatl,   //ケツァルコアトル
        snakeGod,       //蛇神
    }

    //形態の構造体
    [System.Serializable]
    public struct PLAYER_STATE
    {
        public PLAYER_MODE mode;        //形態
        public Sprite headSprite;       //頭のスプライト
        public bool isLineRenderer;     //胴体の表現にラインレンダラーを使うかどうか
    }

    //各形態ごとの構造体を保存する配列
    public PLAYER_STATE[] States;

    private float basicMoveSpeed = 2;

    private float basicRotSpeed = 180;

    //胴体の最低数
    private const int MIN_BODYNUM = 4;
    //胴体の最大数
    private const int MAX_BODYNUM = 10;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 移動速度と回転速度を変更
    /// </summary>
    /// <param name="_amount">速度の倍率</param>
    public void SpeedChange(float _amount)
    {
        moveSpeed = basicMoveSpeed * _amount;
        RotSpeed = basicRotSpeed * _amount;
    }

    /// <summary>
    /// 形態一覧の中から探したい形態のデータを探す
    /// </summary>
    /// <param name="_mode">探したい形態のモード</param>
    /// <returns>見つかった形態のデータ 見つからなかった場合は通常(蛇)のデータ</returns>
    public PLAYER_STATE SearchState(PLAYER_MODE _mode)
    {
        PLAYER_STATE returnState = States[0];
        for(int i = 0; i < States.Length; i++)
        {
            //同じモードが見つかった時
            if(_mode == States[i].mode)
            {
                returnState = States[i];
                break;
            }
        }

        return returnState;
    }

    /// <summary>
    /// 胴体の数を変化
    /// </summary>
    /// <param name="_amount">増やしたい(+)or減らしたい(-)胴体の数</param>
    public void BodyNumChange(int _amount)
    {
        bodyNum += _amount;
        //最低数を下回った時
        if(bodyNum < MIN_BODYNUM)
        {
            bodyNum = MIN_BODYNUM;
        }
        //最大数を上回った時
        else if(bodyNum > MAX_BODYNUM)
        {
            bodyNum = MAX_BODYNUM;
        }
    }
}
