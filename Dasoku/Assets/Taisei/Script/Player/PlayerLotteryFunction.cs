using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerControlManager : MonoBehaviour
{
    //最新の蛇足
    private DasokuStateObject newestDasoku;

    //カギ
    private GameObject key;

    /// <summary>
    /// 抽選処理
    /// </summary>
    private void LotteryFunction()
    {
        //最初の抽選　形態か状態か
        int rand = Random.Range(0, 10);

        LotteryData.FORM_OR_DASOKU _result = lottery.form_dasoku[rand];
        Debug.Log(_result);
        switch (_result)
        {
            //形態のとき
            case LotteryData.FORM_OR_DASOKU.form:
                //ウロボロス、ケツァルコアトル、蛇神が出現してないときのみ
                if (!isOuroboros && !isQuetzalcoatl && !isSnakeGod)
                {
                    Lottery_Form();
                }
                //上記のいずれかが出現した時
                else
                {
                    //全てはずれ
                    DasokuReflection(GM.allDasoku.dasokuKinds[GM.allDasoku.dasokuKinds.Length - 1]);
                }
                break;

            //状態の時
            case LotteryData.FORM_OR_DASOKU.dasoku:
                Lottery_Dasoku();
                break;
        }
    }

    /// <summary>
    /// 形態変化の抽選
    /// </summary>
    private void Lottery_Form()
    {
        //乱数生成
        int rand = Random.Range(0, lottery.numElements_Form);

        PlayerData.PLAYER_MODE _result = lottery.form_lotterys[rand];

        //形態一覧の中から抽選で選ばれた形態を選出
        for(int i = 0; i < plData.States.Length; i++)
        {
            if(_result == plData.States[i].mode)
            {
                //現在と同じ形態の時ははずれを
                if(plData.nowMode == plData.States[i].mode)
                {
                    DasokuReflection(GM.allDasoku.dasokuKinds[GM.allDasoku.dasokuKinds.Length - 1]);
                    break;
                }
                SpecialLottery_Form(_result);
                break;
            }
        }
    }

    /// <summary>
    /// 形態の特殊抽選
    /// </summary>
    private void SpecialLottery_Form(PlayerData.PLAYER_MODE _mode)
    {
        //一部の形態のみさらに抽選
        switch (_mode)
        {
            //蛇足形態
            case PlayerData.PLAYER_MODE.snakeReg:
                //蛇足プロのフラグがオンの時
                if (GameData.GameEntity.isSnakeReg_Pro)
                {
                    plData.nowMode = PlayerData.PLAYER_MODE.snakeReg_Pro;
                }
                else
                {
                    plData.nowMode = PlayerData.PLAYER_MODE.snakeReg;
                }
                break;

            //足形態
            case PlayerData.PLAYER_MODE.foot:
                int foot_rand = Random.Range(0, LotteryData.footLottery_num);
                //ヒール形態を当てた時
                if (lottery.footLottery[foot_rand])
                {
                    plData.nowMode = PlayerData.PLAYER_MODE.heel;
                }
                //足形態のままの時
                else
                {
                    plData.nowMode = PlayerData.PLAYER_MODE.foot;
                }
                break;

            //蛇手形態
            case PlayerData.PLAYER_MODE.snakeHand:
                int hand_rand = Random.Range(0, LotteryData.handLottery_num);
                //剛腕形態を当てた時
                if (lottery.handLottery[hand_rand])
                {
                    plData.nowMode = PlayerData.PLAYER_MODE.strongArm;
                }
                //蛇手形態のままのとき
                else
                {
                    plData.nowMode = PlayerData.PLAYER_MODE.snakeHand;
                }
                break;

            //一部以外の形態の時
            default:
                plData.nowMode = _mode;

                //ケツァルコアトル、蛇神だったとき
                if(_mode == PlayerData.PLAYER_MODE.quetzalcoatl)
                {
                    isQuetzalcoatl = true;
                }
                if(_mode == PlayerData.PLAYER_MODE.snakeGod)
                {
                    isSnakeGod = true;
                }

                break;
        }
    }

    /// <summary>
    /// 状態変化の抽選
    /// </summary>
    private void Lottery_Dasoku()
    {
        int rand = Random.Range(0, lottery.numElements_dasoku);

        DasokuStateObject.DASOKUSTATE_KINDS _result = lottery.dasoku_lotterys[rand];

        //種類がある状態変化の時
        int _spRand = -1;
        switch (_result)
        {
            case DasokuStateObject.DASOKUSTATE_KINDS.dasokuDelete:
                _spRand = Random.Range(0, lottery.Dels.Count);
                lottery.selectDel = lottery.Dels[_spRand];
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.speedChange:
                _spRand = Random.Range(0, lottery.MoveSpeeds.Count);
                lottery.selectMove = lottery.MoveSpeeds[_spRand];
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.meterSpeedChenge:
                _spRand = Random.Range(0, lottery.MeterSpeeds.Count);
                lottery.selectMeter = lottery.MeterSpeeds[_spRand];
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.addBody:
                _spRand = Random.Range(0, lottery.Bodys.Count);
                lottery.selectBody = lottery.Bodys[_spRand];
                break;
        }

        //状態変化一覧の中から抽選で選ばれた形態を選出
        for (int i = 0; i < GM.allDasoku.dasokuKinds.Length; i++)
        {
            if(_result == GM.allDasoku.dasokuKinds[i].commons.kinds)
            {
                var selectDasoku = GM.allDasoku.dasokuKinds[i];
                Debug.Log(selectDasoku.commons.kinds);

                //種類がある状態変化だった場合
                if (_spRand >= 0)
                {
                    //選ばれた種類の状態変化と同じだったか
                    bool _isSame = false;
                    switch (_result)
                    {
                        case DasokuStateObject.DASOKUSTATE_KINDS.dasokuDelete:
                            if(lottery.selectDel == selectDasoku.deleteType)
                            {
                                _isSame = true;
                            }
                            break;

                        case DasokuStateObject.DASOKUSTATE_KINDS.speedChange:
                            if(lottery.selectMove == selectDasoku.moveType)
                            {
                                _isSame = true;
                            }
                            break;

                        case DasokuStateObject.DASOKUSTATE_KINDS.meterSpeedChenge:
                            if(lottery.selectMeter == selectDasoku.meterType)
                            {
                                _isSame = true;
                            }
                            break;

                        case DasokuStateObject.DASOKUSTATE_KINDS.addBody:
                            if(lottery.selectBody == selectDasoku.addBodyType)
                            {
                                _isSame = true;
                            }
                            break;
                    }
                    if (_isSame)
                    {
                        //選ばれた状態変化が一度も選出されていないとき
                        if (!selectDasoku.commons.isAppearance)
                        {
                            DasokuReflection(selectDasoku);
                            //最新の蛇足を記録
                            newestDasoku = selectDasoku;
                        }
                        //既に一度選出されてた時
                        else
                        {
                            //はずれを反映させる
                            DasokuReflection(GM.allDasoku.dasokuKinds[GM.allDasoku.dasokuKinds.Length - 1]);
                        }
                        break;
                    }
                }
                //種類がない状態変化だったとき
                else
                {
                    //選ばれた状態変化が一度も選出されていないとき
                    if (!selectDasoku.commons.isAppearance)
                    {
                        DasokuReflection(selectDasoku);
                        //最新の蛇足を記録
                        newestDasoku = selectDasoku;
                    }
                    //既に一度選出されてた時
                    else
                    {
                        //はずれを反映させる
                        DasokuReflection(GM.allDasoku.dasokuKinds[GM.allDasoku.dasokuKinds.Length - 1]);
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 状態変化を反映させる
    /// </summary>
    /// <param name="_kind">状態変化の種類</param>
    private void DasokuReflection(DasokuStateObject _dasoku)
    {
        _dasoku.commons.isAppearance = true;

        switch (_dasoku.commons.kinds)
        {
            //カギ追加
            case DasokuStateObject.DASOKUSTATE_KINDS.addKey:
                //出現場所が設定されてる時
                if (GM.randomPos.Length > 0)
                {
                    int keyRand = Random.Range(0, GM.randomPos.Length);

                    //カギ生成
                    key = Instantiate(_dasoku.KeyPrefab, GM.randomPos[keyRand].position, Quaternion.identity);
                }
                break;

            //蛇足解除
            case DasokuStateObject.DASOKUSTATE_KINDS.dasokuDelete:
                //蛇足解除
                DasokuDelete(_dasoku.deleteType);
                break;

            //移動速度
            case DasokuStateObject.DASOKUSTATE_KINDS.speedChange:

                plData.SpeedChange(_dasoku.changeMoveSpeed);
                break;

            //蛇足ゲージ速度
            case DasokuStateObject.DASOKUSTATE_KINDS.meterSpeedChenge:
                plData.MeterSpeedChange(_dasoku.changeMeterSpeed);
                break;

            //胴体追加
            case DasokuStateObject.DASOKUSTATE_KINDS.addBody:
                plData.BodyNumChange(_dasoku.addBodyNum);
                break;

            //ランダムワープ
            case DasokuStateObject.DASOKUSTATE_KINDS.teleport:
                //出現場所が設定されてる時
                if (GM.randomPos.Length > 0)
                {
                    int teleprotRand = Random.Range(0, GM.randomPos.Length);

                    plData.nowBody.transform.position = GM.randomPos[teleprotRand].position;
                }
                break;

            //ファンファーレ
            case DasokuStateObject.DASOKUSTATE_KINDS.fanfare:
                break;

            //BGM変更
            case DasokuStateObject.DASOKUSTATE_KINDS.changeBGM:
                break;

            //エフェクト追加
            case DasokuStateObject.DASOKUSTATE_KINDS.addEffect:
                break;

            //デコレーション
            case DasokuStateObject.DASOKUSTATE_KINDS.decoration:
                break;

            //はずれ
            case DasokuStateObject.DASOKUSTATE_KINDS.miss:
                break;
        }

        canvasGroup.alpha = 1;
        dasokuNameText.text = _dasoku.commons.name;
        dasokuEffectText.text = _dasoku.commons.explanation;
    }

    /// <summary>
    /// 状態変化を初期状態にリセット
    /// </summary>
    private void DasokuReset()
    {
        for(int i = 0; i < GM.allDasoku.dasokuKinds.Length; i++)
        {
            GM.allDasoku.dasokuKinds[i].commons.isAppearance = false;
        }
    }

    /// <summary>
    /// 蛇足を解除
    /// </summary>
    private void DasokuDelete(DasokuStateObject.DASOKUSTATE_DELETE _deleteMode)
    {
        switch (_deleteMode)
        {
            case DasokuStateObject.DASOKUSTATE_DELETE.one:
                if(newestDasoku != null)
                {
                    Dasoku_OneDelete(newestDasoku);
                }
                break;

            case DasokuStateObject.DASOKUSTATE_DELETE.all:
                //移動速度、ゲージ増加速度の倍率を１に
                plData.SpeedChange(1);
                plData.MeterSpeedChange(1);
                //胴体の長さを最小値(初期値に)
                plData.BodyNumChange(-6);

                DasokuReset();
                break;
        }
    }

    /// <summary>
    /// 蛇足を１つ解除
    /// </summary>
    private void Dasoku_OneDelete(DasokuStateObject _dasoku)
    {
        switch (_dasoku.commons.kinds)
        {
            //カギ削除
            case DasokuStateObject.DASOKUSTATE_KINDS.addKey:
                Goal.Instance.DeleteKey(key);
                break;

            //移動速度
            case DasokuStateObject.DASOKUSTATE_KINDS.speedChange:
                plData.SpeedChange(1);
                break;

            //蛇足ゲージ速度
            case DasokuStateObject.DASOKUSTATE_KINDS.meterSpeedChenge:
                plData.MeterSpeedChange(1);
                break;

            //胴体削除
            case DasokuStateObject.DASOKUSTATE_KINDS.addBody:
                plData.BodyNumChange(-6);
                break;

            //ファンファーレ
            case DasokuStateObject.DASOKUSTATE_KINDS.fanfare:
                break;

            //BGM変更
            case DasokuStateObject.DASOKUSTATE_KINDS.changeBGM:
                break;

            //エフェクト追加
            case DasokuStateObject.DASOKUSTATE_KINDS.addEffect:
                break;

            //デコレーション
            case DasokuStateObject.DASOKUSTATE_KINDS.decoration:
                break;
        }

    }
}
