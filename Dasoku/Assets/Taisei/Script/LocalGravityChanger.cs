using UnityEngine;

/// <summary>
/// オブジェクトごとに重力を変更するためのスクリプト
/// </summary>
public class LocalGravityChanger
{
    private Rigidbody2D rb;
    //疑似重力の方向
    private Vector2 gravityVec;

    //疑似重力を使用するかどうか
    private bool isGravity = false;

    //重力の強さ
    private float gravityStrength = 9.81f;

    /// <summary>
    /// 重力をかける
    /// </summary>
    private void UseGravity()
    {
        //nullチェック
        if (rb != null)
        {
            //疑似重力
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
