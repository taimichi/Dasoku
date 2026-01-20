using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SnakeController : MonoBehaviour
{
    //蛇手関連
    [Header("蛇手関連")]
    //スプリングジョイント
    [SerializeField] private SpringJoint2D joint;
    //lineRenderer
    [SerializeField] private LineRenderer line;
    //蛇手のアクション用オブジェクトの親オブジェクト
    [SerializeField] private GameObject arrows;
    //方向を決める矢印
    [SerializeField] private Transform rotArrow;
    //手の当たり判定確認用
    [SerializeField] private SnakeHandHit handHit;
    /// <summary>
    /// 蛇手のアクション状態
    /// </summary>
    private enum HANDACTION_STATE
    {
        idle,   //待機
        action, //アクション(方向決定)
        move,   //移動(手の移動、壁に当たったらグラップル発動)
    }
    private HANDACTION_STATE nowHandAction = HANDACTION_STATE.idle;

    //開始時の矢印の角度
    private Vector3 startArrowRot;

    //手の進むスピード
    private float handSpeed = 10f;
    //手の進める距離
    private float handDistance = 10f;

    //９０度回転するのにかかる秒数
    private float arrowRotSpeed = 1.5f;
    //変化量
    private float arrowRotAmount = 0f;
    //+回転か-回転か
    private bool isArrowRot = false;
    //グラップルを使ってるかどうか
    private bool isGrapple = false;

    private Vector3[] points = new Vector3[2];    
    
    /// <summary>
    /// 蛇手のアクション
    /// </summary>
    private void SnakeHandAction()
    {
        switch (nowHandAction)
        {
            case HANDACTION_STATE.action:
                if (!arrows.activeSelf)
                {
                    arrows.SetActive(true);
                }
                //現在の矢印の回転値を取得
                Vector3 rot = rotArrow.localEulerAngles;
                //1fで回転する量
                float rotSpeed = (90f / arrowRotSpeed) * Time.deltaTime;
                //左回転
                if (isArrowRot)
                {
                    //
                    rot.z -= rotSpeed;
                    arrowRotAmount += rotSpeed;
                    //変化量が90度を超えた時
                    if (arrowRotAmount >= 90)
                    {
                        //-90度より先に行かないようにする
                        rot.z = -90;
                        //右回転に
                        isArrowRot = false;
                        //変化量を0に
                        arrowRotAmount = 0;
                    }
                }
                //右回転
                else
                {
                    //
                    rot.z += rotSpeed;
                    arrowRotAmount += rotSpeed;
                    //変化量が90度を超えた時
                    if (arrowRotAmount >= 90)
                    {
                        //0度より先に行かないようにする
                        rot.z = 0;
                        //左回転に
                        isArrowRot = true;
                        //変化量を0に
                        arrowRotAmount = 0;
                    }
                }
                //回転
                rotArrow.localEulerAngles = rot;
                break;

            case HANDACTION_STATE.move:

                //LineRenderer有効化
                line.enabled = true;

                Vector3 playreDown = -this.transform.up * 0.3f;
                Vector3 handDown = -Appearance.transform.up * 0.3f;

                points[0] = this.transform.position + playreDown;
                points[1] = Appearance.transform.position + handDown;
                line.positionCount = 2;
                line.SetPositions(points);

                //グラップル非発動
                if (!isGrapple)
                {
                    //手が地面と当たってないとき
                    if (!handHit.ReturnIsHit())
                    {
                        //矢印の向きを取得
                        Vector2 arrowDire = rotArrow.up;
                        //矢印の方向に手を移動させる
                        Vector2 pos = arrowDire * handSpeed * Time.deltaTime;
                        Appearance.transform.position += (Vector3)pos;

                        Vector3 r = rotArrow.localEulerAngles;
                        r.z += 90;
                        //手の向きを矢印の方向と同じに
                        Appearance.transform.localEulerAngles = r;
                    }
                    //当たった時
                    else
                    {
                        isGrapple = true;

                        //スプリングジョイント有効化
                        joint.enabled = true;

                        //接続アンカーの位置を手が当たった位置に設定
                        joint.connectedAnchor = Appearance.transform.position;
                        return;
                    }

                    //手とプレイヤーオブジェクトの距離
                    float Distance = Vector2.Distance(Appearance.transform.position, this.transform.position);

                    //距離が規定距離を超えた時 or 手が地面と当たった時
                    if (Distance >= handDistance)
                    {
                        HandActionReset();
                    }
                }
                //グラップル発動
                else
                {
                    //手をアンカーの位置で固定
                    Appearance.transform.position = joint.connectedAnchor;

                    //手とプレイヤーオブジェクトの距離
                    float Distance = Vector2.Distance(Appearance.transform.position, this.transform.position);

                    if(Distance <= joint.distance)
                    {
                        HandActionReset();
                    }
                }

                break;
        }
    }

    /// <summary>
    /// 蛇手用の変数リセット
    /// </summary>
    private void HandActionReset()
    {
        //初期化
        //矢印の角度を初期値に
        rotArrow.localEulerAngles = startArrowRot;
        //変化量を0に
        arrowRotAmount = 0;
        //右回転に
        isArrowRot = false;
        if (arrows.activeSelf)
        {
            arrows.SetActive(false);
        }
        //見た目オブジェクトを元の位置に
        Appearance.transform.localPosition = Vector3.zero;

        isGrapple = false;
        joint.enabled = false;
        line.enabled = false;

        //ハンドアクション終わり
        nowHandAction = HANDACTION_STATE.idle;
    }
}
