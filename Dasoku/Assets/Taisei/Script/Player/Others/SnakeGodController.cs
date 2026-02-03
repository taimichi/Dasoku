using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeGodController : MonoBehaviour, PlayerInterface
{
    private PlayerControlManager plMG;
    //リジッドボディ
    private Rigidbody2D playerRB;
    //boxコライダー
    private BoxCollider2D box;
    //見た目
    [SerializeField] private SpriteRenderer Appearance;

    //蛇神のアクションを使ったかどうか
    private bool isAction = false;
    //テレポート先のオフセット
    private Vector2 teleportOffset = new Vector2(0, 3);

    private float moveAmount = 0;
    private float moveSpeed = 0.5f;

    void Start()
    {
        isAction = false;
    }

    public void PlUpdate()
    {
        if (isAction)
        {
            if(moveAmount <= teleportOffset.y)
            {
                Vector2 pos = this.transform.position;
                pos += -teleportOffset * moveSpeed * Time.deltaTime;
                this.transform.position = pos;

                moveAmount += teleportOffset.y * moveSpeed * Time.deltaTime;
            }

        }
    }

    #region 入力処理
    public void InputRight()
    {

    }

    public void InputLeft()
    {

    }

    public void InputLRUp()
    {

    }

    public void NoInput()
    {

    }

    public void InputUp()
    {

    }

    public void InputDown()
    {

    }

    public void InputUDUp()
    {

    }


    public void InputActionDown()
    {
        //一度だけ
        if (!isAction)
        {
            //ゴールの位置を取得
            Vector2 goalPos = Goal.Instance.ReturnGoal().transform.position;

            //ゴールの上へテレポート
            this.transform.position = goalPos + teleportOffset;

            isAction = true;
        }
    }

    public void InputAction()
    {

    }

    public void InputActionUp()
    {

    }
    #endregion

    public bool CheckStandGround()
    {
        return true;
    }

    public void FormChange(Sprite _sprite)
    {

    }

}
