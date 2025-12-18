using UnityEngine;
using System;

public partial class SnakeController
{
    /// <summary>
    /// 疑似重力を使うかどうか
    /// </summary>
    /// <returns>false=疑似重力が不必要 / true=疑似重力が必要</returns>
    public bool CheckUseGravity(float _z)
    {
        //現在のz軸の角度を取得
        float nowRotZ = (int)_z;

        //角度が下向き以外の時
        if (nowRotZ % 360 != 0)
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
    public bool CheckNowRotate(float _limitRot, float _z)
    {
        //現在のz軸の角度を取得
        float nowRotZ = (float)Math.Round(_z, MidpointRounding.AwayFromZero);

        //回転がまだ途中の時
        if (nowRotZ % _limitRot != 0)
        {
            return true;
        }

        return false;
    }

}
