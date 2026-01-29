using UnityEngine;

//蛇の回転処理
public partial class SnakeController : MonoBehaviour
{
    [Header("回転関連")]
    //回転位置の取得用のオブジェクト
    [SerializeField] private Transform pivotPoint;

    //一度に回転できる角度
    private float limitRot = 90f;

    //回転する際の支点
    private Vector3 pivotPos;

    //どこまで回転したか
    private float rotateAmount = 0f;

    //回転するかどうか
    private bool isRot = false;

    //レイがはみ出た座標保持用
    private Vector3 rayOut_savePos;

    //レイがはみ出したかどうか
    private bool isRayOut = false;

    /// <summary>
    /// 回転に関するプレイヤーの状態
    /// </summary>
    private enum PlayerRot_Mode
    {
        none,       //通常
        rot,        //回転中
        finish,     //回転終了
    }

    //現在の回転関連のプレイヤーの状態
    private PlayerRot_Mode nowRot = PlayerRot_Mode.none;

    //初めて回転モードに入った時のプレイヤーの向いてる方向
    private PlayerControlManager.PlayerDire_Mode startRotDire = PlayerControlManager.PlayerDire_Mode.normal;

    //初めて回転モードに入ったか
    private bool isStartRotDire = false;

    /// <summary>
    /// 回転する際、どこを基準にするか
    /// </summary>
    private enum BasicRot_State
    {
        none,       //通常
        ground,     //地面
        wall,       //壁
    }
    private BasicRot_State rotState = BasicRot_State.none;
    //回転していいかどうか
    //false=ダメ / true=オーケー
    private bool isRotOK = true;


    /// <summary>
    /// 回転するかの判定
    /// </summary>
    private void CheckRotate()
    {
        Vector2 down = -this.transform.up;      //プレイヤーの下方向ベクトル
        float rayDistance_under = 0.1f;         //レイの長さ　地面
        float rayDistance_front = 0.6f;        //レイの長さ　壁

        //左下の座標
        //左下のローカル座標を取得→ワールド座標に変換
        Vector2 local_leftBottom = colliderOffset + new Vector2(-colliderSize.x / 2f, -colliderSize.y / 2f);
        Vector2 leftBottom = transform.TransformPoint(local_leftBottom);

        //右下の座標
        //右下のローカル座標を取得→ワールド座標に変換
        Vector2 local_rightBottom = colliderOffset + new Vector2(colliderSize.x / 2f, -colliderSize.y / 2f);
        Vector2 rightBottom = transform.TransformPoint(local_rightBottom);

        //前方の座標
        //前方のローカル座標を取得→ワールド座標に変換
        Vector2 local_forward = colliderOffset + new Vector2(colliderSize.x / 2f, 0f);
        Vector2 forward = transform.TransformPoint(local_forward);

        //レイが地面と触れているか
        //左下
        bool isLeftGrounded = Physics2D.Raycast(leftBottom, down, rayDistance_under, plMG.groundLayer);
        //右下
        bool isRightGrounded = Physics2D.Raycast(rightBottom, down, rayDistance_under, plMG.groundLayer);
        //前方の壁とレイが触れてるか
        bool isForwardGrounded = Physics2D.Raycast(forward, plMG.plSeeVec, rayDistance_front, plMG.groundLayer);

        //前方に壁があるとき
        if (isForwardGrounded)
        {
            RaycastHit2D frontHit = Physics2D.Raycast(forward, plMG.plSeeVec, rayDistance_front, plMG.groundLayer);
            Vector2 nearCorner = plMG.GM.GetNearestCorner(frontHit.point);

            //プレイヤーとブロックの高さ
            float h = Mathf.Abs(pivotPoint.position.y - nearCorner.y);
            h = Mathf.Round(h * 10) / 10f;

            //一番近い角が0.5マス以上1マス未満のとき
            if (h >= 0.5f && h < 1f)
            {
                //壁回転ができないようにする
                isForwardGrounded = false;
            }
        }

        #region レイ表示(デバッグ用)
        Debug.DrawRay(leftBottom, down * rayDistance_under, Color.green);
        Debug.DrawRay(rightBottom, down * rayDistance_under, Color.red);
        Debug.DrawRay(forward, plMG.plSeeVec * rayDistance_front, Color.green);
        #endregion

        //地面が両方触れてる時かつ回転不可能状態の時
        if ((isLeftGrounded && isRightGrounded) && !isRotOK)
        {
            isRotOK = true;
        }

        //回転してダメなとき
        //これ以上処理をしない
        if (!isRotOK)
        {
            return;
        }

        //回転終了時の時
        if (nowRot == PlayerRot_Mode.finish)
        {
            //回転終了時用の壁判定用
            //レイの長さ
            float rayDis = 0.6f;
            //壁に当たったかどうか
            bool isForwardWall = Physics2D.Raycast(forward, plMG.plSeeVec, rayDis, plMG.groundLayer);
            RaycastHit2D hit = Physics2D.Raycast(forward, plMG.plSeeVec, rayDis, plMG.groundLayer);

            //両端が地面と触れてる時
            if (isLeftGrounded && isRightGrounded)
            {
                //回転モードを通常に戻す
                nowRot = PlayerRot_Mode.none;
                //回転フラグを初期状態に
                isRot = false;

                //回転時の処理に使ってた物を初期状態に戻す
                startRotDire = PlayerControlManager.PlayerDire_Mode.normal;
                isStartRotDire = false;
                rotState = BasicRot_State.none;
            }
            //片方のみが地面と触れてる時
            else if (isLeftGrounded ^ isRightGrounded)
            {
                //回転直後、前の場所に戻ろうとしたとき
                if (plMG.nowDire != PlayerControlManager.PlayerDire_Mode.normal && plMG.nowDire != startRotDire)
                {
                    //モードを通常に
                    nowRot = PlayerRot_Mode.none;
                    //回転状態を初期化
                    isRot = false;
                    isStartRotDire = false;
                }

                if (!isRightGrounded && !isForwardWall && rotState != BasicRot_State.wall)
                {
                    Process_RotGround(this.transform.position);
                }
                else if (isForwardWall && rotState != BasicRot_State.ground)
                {
                    //仮　Process_RotWall()を使うと挙動がなぜかおかしくなるので旧処理のまま
                    rotState = BasicRot_State.wall;
                    //レイが当たった座標に近い角の座標を取得
                    Vector2 rot = plMG.GM.GetNearestCorner(hit.point);
                    //以下の計算は一緒
                    Vector2 X = plMG.plSeeVec * colliderSize.x * 7 / 8;
                    Vector2 Y = this.transform.up * colliderSize.y * 7 / 8;
                    rot -= X;
                    rot += Y;
                    rot.x = Mathf.Round(rot.x * 10f) / 10f;
                    rot.y = Mathf.Round(rot.y * 10f) / 10f;

                    rotateAmount = 0;
                    pivotPos = rot;
                    //物理挙動での移動ができないように
                    playerRB.constraints = RigidbodyConstraints2D.FreezeAll;
                    isRot = true;
                    nowRot = PlayerRot_Mode.rot;
                }
            }
            return;
        }

        //どちらかの方向キーを押してる時
        if (plMG.nowDire != PlayerControlManager.PlayerDire_Mode.normal)
        {
            //左下か右下のどちらかのみが地面と触れてる時
            if (isLeftGrounded ^ isRightGrounded && rotState != BasicRot_State.wall)
            {
                if (isHalfGround)
                {
                    Process_RotGround(pivotPoint.position);
                    return;
                }
            }
            //前方の壁に触れた時
            else if (isForwardGrounded && rotState != BasicRot_State.ground)
            {
                Process_RotWall(colliderSize, rightBottom);
                return;
            }
        }

        //プレイヤーを物理挙動での移動ができるようにする
        //(回転はさせたくないため、rotationはそのまま固定)
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log("回転しない");
        //回転しないとして、falseを返す
        isRot = false;

        isRayOut = false;
        nowRot = PlayerRot_Mode.none;

        rotState = BasicRot_State.none;
    }

    /// <summary>
    /// 壁基準の回転処理
    /// </summary>
    private void Process_RotWall(Vector2 size, Vector2 targetPos)
    {
        //一番近い角を取得
        Vector2 rotPos = plMG.GM.GetNearestCorner(targetPos);

        //回転の支点を計算
        //×4/5は微調整用
        Vector2 X = plMG.plSeeVec * size.x * 7 / 8;
        Vector2 Y = this.transform.up * size.y * 7 / 8;
        rotPos -= X;
        rotPos += Y;
        rotPos.x = Mathf.Round(rotPos.x * 10f) / 10f;
        rotPos.y = Mathf.Round(rotPos.y * 10f) / 10f;

        //回転の支点の位置を変更
        pivotPos = rotPos;

        //回転量を初期化
        rotateAmount = 0f;

        //回転基準を壁に
        rotState = BasicRot_State.wall;

        //物理挙動での移動ができないように
        playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

        Debug.Log("回転する(壁)");
        isRot = true;
    }

    /// <summary>
    /// 地面基準の回転処理
    /// </summary>
    private void Process_RotGround(Vector2 targetPos)
    {
        //回転基準を地面に
        rotState = BasicRot_State.ground;
        //レイがはみ出して初めての処理の時
        if (!isRayOut)
        {
            rayOut_savePos = plMG.GM.GetNearestCorner(targetPos);
            isRayOut = true;
        }

        //それぞれの座標の小数点第2位以下を切り捨て
        //レイがはみ出したときの座標
        Vector2 save = rayOut_savePos;
        save.x = Mathf.Round(save.x * 10f) / 10f;
        save.y = Mathf.Round(save.y * 10f) / 10f;

        //現在の回転支点の座標
        //真ん中
        Vector2 pivot = pivotPoint.position;
        pivot.x = Mathf.Round(pivot.x * 10f) / 10f;
        pivot.y = Mathf.Round(pivot.y * 10f) / 10f;

        //レイがはみ出した座標と回転支点が一緒の時
        if (save.x == pivot.x && save.y == pivot.y)
        {
            //回転量を初期化
            rotateAmount = 0f;

            //回転の支点の位置を変更
            pivotPos = rayOut_savePos;

            //物理挙動での移動ができないように
            playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

            Debug.Log("回転する(地面)");
            isRot = true;
        }
    }

    /// <summary>
    /// プレイヤーの回転処理まとめ
    /// </summary>
    private void PlayerRotate()
    {
        //回転終了状態じゃないとき
        if (nowRot != PlayerRot_Mode.finish)
        {
            //回転モードに変更
            nowRot = PlayerRot_Mode.rot;

            //地面基準
            if (rotState == BasicRot_State.ground)
            {
                //回転処理
                RotFunction(1);
            }
            //壁基準
            else if (rotState == BasicRot_State.wall)
            {
                //回転処理
                RotFunction(-1);
            }
        }
    }

    /// <summary>
    /// 回転処理 
    /// 引数が1の時は地面、-1の時は壁
    /// </summary>
    private void RotFunction(int _rot)
    {
        //向いてる方向が右か左、どちらかを向いてる時のみ
        if (plMG.nowDire != PlayerControlManager.PlayerDire_Mode.normal)
        {
            //初めて回転モードに入った時
            if (!isStartRotDire)
            {
                //初めて回転モードに入った時のプレイヤーの向いてる方向を取得
                startRotDire = plMG.nowDire;
                //この処理は1度だけ
                isStartRotDire = true;
            }

            //回転する量
            float step = plMG.plData.RotSpeed * Time.deltaTime;

            //初めて回転モードに入った時のプレイヤーの向いてる方向と現在の方向が同じとき
            if (startRotDire == plMG.nowDire)
            {
                //一度に回転する量を超えた時
                if (rotateAmount + step > limitRot)
                {
                    //目標角度で止める
                    step = limitRot - rotateAmount;
                }
            }
            else//違うとき
            {
                if (rotateAmount - step < 0)
                {
                    step = 0 + rotateAmount;
                }
            }

            //回転
            //前進時
            if (plMG.nowDire == PlayerControlManager.PlayerDire_Mode.right)
            {
                //pivotPosを支点にz軸で回転
                this.transform.RotateAround(pivotPos, Vector3.forward, -step * _rot);

                //回転量の計算
                CalculationRotateAmount(step);
            }
            //後退時
            else if (plMG.nowDire == PlayerControlManager.PlayerDire_Mode.left)
            {
                //pivotPosを支点にz軸で回転
                this.transform.RotateAround(pivotPos, Vector3.forward, step * _rot);

                //回転量の計算
                CalculationRotateAmount(step);
            }

            //回転した量が1度に回転できる量以上になった時
            if (rotateAmount >= limitRot || rotateAmount <= 0)
            {
                //回転終了モードに変更
                nowRot = PlayerRot_Mode.finish;
            }
        }
    }

    /// <summary>
    /// 回転量の加算・減算
    /// </summary>
    /// <param name="step">回転する量</param>
    private void CalculationRotateAmount(float step)
    {
        if (startRotDire == plMG.nowDire)
        {
            //回転した量を追加
            rotateAmount += step;
        }
        else
        {
            //回転した量を追加
            rotateAmount -= step;
        }
    }

    /// <summary>
    /// 回転処理終了後処理
    /// </summary>
    private void RotateAfter()
    {
        //回転処理終了後の時
        if (nowRot == PlayerRot_Mode.finish)
        {
            rotState = BasicRot_State.none;
            isRayOut = false;
            //物理挙動を可能にする
            playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

            //現在の向いてる方角が通常以外(右か左)の時
            if (plMG.nowDire != PlayerControlManager.PlayerDire_Mode.normal)
            {
                PlayerMove();
            }
        }
    }

}
