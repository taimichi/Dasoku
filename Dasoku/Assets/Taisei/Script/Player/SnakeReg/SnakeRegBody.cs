using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SnakeRegController : MonoBehaviour
{
    //メモ
    //bodyのサイズxが1伸びたら、x座標は-0.5
    //足の位置は1伸びたら前足+0.5、後ろ足-0.5
    //コライダーはサイズはbodyのサイズx+0.8(頭)
    //オフセットはサイズ1伸びるごとにx-0.5

    [Header("体関連")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private Transform frontReg;
    [SerializeField] private Transform backReg;
    private int nowBodyNum = 4;
    private Vector2 headSize = new Vector2(0.8f, 0.7f);

    private void StartBodySet()
    {
        //初期設定用
        if (nowBodyNum == 0)
        {
            nowBodyNum = plMG.plData.bodyNum;
        }

        bodySpriteRenderer = body.GetComponent<SpriteRenderer>();

        if (plMG.plData.nowMode == PlayerData.PLAYER_MODE.snakeReg)
        {
            AnimChange("isPro", false);
        }
        else if (plMG.plData.nowMode == PlayerData.PLAYER_MODE.snakeReg_Pro)
        {
            AnimChange("isPro", true);
        }

    }

    private void BodySet(int _index)
    {
        //胴体の数を更新
        nowBodyNum = plMG.plData.bodyNum;

        //頭の座標
        Vector2 headPos = head.localPosition;
        headPos.x += 0.5f * _index;
        head.localPosition = headPos;

        //足の座標
        //前
        Vector2 frontRegPos = frontReg.localPosition;
        frontRegPos.x += 0.5f * _index;
        frontReg.localPosition = frontRegPos;
        //後ろ
        Vector2 backRegPos = backReg.localPosition;
        backRegPos.x += -0.5f * _index;
        backReg.localPosition = backRegPos;

        //コライダーのサイズ
        plCollider.size = new Vector2(nowBodyNum + headSize.x, colliderSize.y);
        colliderSize = plCollider.size;

        colliderOffset = plCollider.offset;
        
        //見た目のサイズ変更
        bodySpriteRenderer.size = new Vector2(nowBodyNum, 1);

    }

    private void BodyUpdate()
    {
        int index = plMG.plData.bodyNum - nowBodyNum;
        //胴体の数に変化があった時
        if (index != 0)
        {
            BodySet(index);
        }
    }
}
