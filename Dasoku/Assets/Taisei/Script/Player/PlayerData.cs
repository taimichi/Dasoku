using UnityEngine;

//プレイヤーの各データや状態を保持するスクリプト
public class PlayerData : MonoBehaviour
{
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

    //基礎速度
    [Tooltip("基礎速度")] public float basicSpeed = 2f;
    //回転速度
    [Tooltip("1秒に回転する角度")] public float rotSpeed = 180f;
    //胴体の数
    [Tooltip("胴体の数")] public int bodyNum = 4;

    //形態の構造体
    [System.Serializable]
    public struct PLAYER_STATE
    {
        public PLAYER_MODE mode;
        public Sprite sprite;
    }

    //各形態ごとの構造体を保存する配列
    public PLAYER_STATE[] States;

}
