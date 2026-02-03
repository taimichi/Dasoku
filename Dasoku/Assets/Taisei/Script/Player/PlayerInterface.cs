using UnityEngine;

public interface PlayerInterface
{
    /// <summary>
    /// 更新関数
    /// </summary>
    void PlUpdate();

    /// <summary>
    /// 見た目変更
    /// </summary>
    /// <param name="_sprite">変更後のスプライト</param>
    void FormChange(Sprite _sprite);

    /// <summary>
    /// 右方向のキーが押されたとき
    /// </summary>
    void InputRight();

    /// <summary>
    /// 左方向のキーが押されたとき
    /// </summary>
    void InputLeft();

    /// <summary>
    /// 左右キーどちらか離したとき
    /// </summary>
    void InputLRUp();

    /// <summary>
    /// 移動キーが押されてないとき
    /// </summary>
    void NoInput();

    /// <summary>
    /// 上方向のキーが押されたとき
    /// </summary>
    void InputUp();

    /// <summary>
    /// 下方向のキーが押されたとき
    /// </summary>
    void InputDown();

    /// <summary>
    /// 上下キーどちらかを離したとき
    /// </summary>
    void InputUDUp();

    /// <summary>
    /// アクションキーが押されたとき
    /// </summary>
    void InputActionDown();

    /// <summary>
    /// アクションキーを押してる間
    /// </summary>
    void InputAction();

    /// <summary>
    /// アクションキーを話したとき
    /// </summary>
    void InputActionUp();

    /// <summary>
    /// 地面に立っているかどうか
    /// </summary>
    /// <returns>false=立ってない / true=立ってる</returns>
    bool CheckStandGround();
}
