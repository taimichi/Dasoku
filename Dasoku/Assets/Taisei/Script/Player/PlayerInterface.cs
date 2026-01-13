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

    void NoInputLR();

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
}
