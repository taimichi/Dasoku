using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オブジェクトごとに重力を変更するためのクラス
/// </summary>
public class LocalGravityChanger
{
    private Rigidbody2D rb;
    //新重力の方向
    private Vector2 gravityVec;

    //重力を変更するかどうか
    private bool isGravity = false;

    //重力の強さ
    private float gravityStrength = 9.81f;

    /// <summary>
    /// 重力をかける
    /// </summary>
    private void UseGravity()
    {
        if (rb != null)
        {
            //疑似重力
            //反転させた通常重力分の力を加えることで、通常の重力を打ち消す
            rb.AddForce(gravityVec * gravityStrength * rb.mass);
        }
    }
    
    /// <summary>
    /// 重力更新関数
    /// </summary>
    public void GravityUpdate()
    {
        //重力変更してるとき
        if (isGravity)
        {
            rb.gravityScale = 0;
            UseGravity();
        }
        //重力変更していないとき
        else
        {
            rb.gravityScale = 1;
        }
    }

    /// <summary>
    /// 重力の方向を変更する
    /// </summary>
    /// <param name="direction">変更する重力の方向</param>
    public void ChangeGravityDirection(Vector2 direction)
    {
        //重力の方向を取得
        gravityVec = direction;
    }

    /// <summary>
    /// 重力を変更するかどうか
    /// </summary>
    /// <param name="_isGravity">false=通常重力 / true=変更重力</param>
    public void GravityChange(bool _isGravity)
    {
        isGravity = _isGravity;
    }

    /// <summary>
    /// 重力を変更するオブジェクトのリジッドボディを設定
    /// </summary>
    /// <param name="_rb">重力を変更したいオブジェクトのリジッドボディ</param>
    public void SetGravityChange(Rigidbody2D _rb)
    {
        rb = _rb;
    }

}
