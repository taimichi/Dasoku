using UnityEngine;

//蛇の移動処理
public partial class SnakeController : MonoBehaviour
{
    //地面判定を取る距離
    private float groundCheckDistance = 0.75f;

    //ハーフブロックが近いかどうか
    private bool isHalfGround = false;

    //落下中かどうか
    private bool isFall = false;
    //落下処理一回目かどうか
    private bool isStartFall = false;

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    private void PlayerMove()
    {
        //プレイヤー移動処理
        //現在のプレイヤーの速度を取得
        Vector2 newPLVelo = playerRB.velocity;

        //横移動の時
        if (plMG.plVec.x != 0)
        {
            //移動速度を計算
            newPLVelo.x = plMG.plVec.x * plMG.plData.moveSpeed;

            //速度が上限を超えた時
            if (newPLVelo.x > plMG.plData.maxVelocity.x)
            {
                //上限速度に設定
                newPLVelo.x = plMG.plData.maxVelocity.x;
            }
        }
        //縦移動の時
        if (plMG.plVec.y != 0)
        {
            //移動速度を計算
            newPLVelo.y = plMG.plVec.y * plMG.plData.moveSpeed;

            //速度が上限を超えた時
            if (newPLVelo.y > plMG.plData.maxVelocity.y)
            {
                //速度が上限を超えた時
                newPLVelo.y = plMG.plData.maxVelocity.y;
            }
        }
        //velocity変更
        playerRB.velocity = newPLVelo;

        //前方に0.5マスブロックがあるとき
        if (CheckHalfBlock() && isRotOK)
        {
            Vector2 startPos = this.transform.position;
            Vector2 offsetPos = this.transform.position - pivotPoint.position;
            Vector2 endPos = plMG.GM.GetNearestCorner(this.transform.position) + offsetPos;

            this.transform.position = Vector2.Lerp(startPos, endPos, 1);

            isRotOK = false;
        }

    }

    /// <summary>
    /// プレイヤーの落下処理
    /// </summary>
    private void PlayerFall()
    {
        //地面に触れてないとき
        if (!CheckGround() && !isRayOut)
        {
            Debug.Log("落下処理中");
            isFall = true;
            //疑似重力使用時
            if (CheckUseGravity(this.transform.localEulerAngles.z))
            {
                //重力を通常に戻す
                this.transform.localEulerAngles = Vector3.zero;
            }

            if (plMG.plSeeVec.y > 0 && !isStartFall)
            {
                Vector3 scale = this.transform.localScale;
                scale.x = -scale.x;
                this.transform.localScale = scale;

                isStartFall = true;
            }

            //プレイヤーの見た目を下向きにする
            Appearance.transform.localEulerAngles = new Vector3(0, 0, -90);
        }
        //地面に触れてるとき
        else
        {
            //プレイヤーの見た目を通常に戻す
            Appearance.transform.localEulerAngles = Vector3.zero;
            isFall = false;
            isStartFall = false;
        }
    }
}
