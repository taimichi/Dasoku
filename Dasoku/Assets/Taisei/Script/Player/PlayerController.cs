using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    //�v���C���[�̃R���C�_�[
    private BoxCollider2D box;

    //�v���C���[�̃��W�b�h�{�f�B
    private Rigidbody2D playerRB;

    #region �ړ��֘A
    //�v���C���[�̊�b���x
    [SerializeField, Header("��b���x")] private float basicSpeed = 1.0f;
    //�v���C���[�̈ړ����x
    private float plSpeed = 1.0f;
    //�v���C���[�̈ړ�����
    private Vector2 plVec;
    #endregion

    #region ��]�֘A
    //�n�ʗp�̃��C���[
    [SerializeField] private LayerMask groundLayer;
    //��x�ɉ�]�ł���p�x
    private float limitRot = 90f;
    //�P�b�ɉ��x��]�����
    [SerializeField, Header("1�b�ɉ�]�����")] private float rotSpeed = 90f;
    //��]����ۂ̎x�_
    private Vector3 pivotPos;
    //�ǂ��܂ŉ�]������
    private float rotateAmount = 0f;
    //��]���邩�ǂ���
    private bool isRot = false;
    //��]�ʒu�̎擾�p�̃I�u�W�F�N�g
    [SerializeField] private Transform pivotPoint;
    //���C���͂ݏo�����W�ێ��p
    private Vector3 rayOut_savePos;
    //���C���͂ݏo�������ǂ���
    private bool isRayOut = false;
    /// <summary>
    /// ��]�Ɋւ���v���C���[�̏��
    /// </summary>
    private enum PlayerRot_Mode
    {
        none,       //�ʏ�
        rot,        //��]��
        finish,     //��]�I��
    }
    //���݂̉�]�֘A�̃v���C���[�̏��
    private PlayerRot_Mode nowRot = PlayerRot_Mode.none;
    //���߂ĉ�]���[�h�ɓ��������̃v���C���[�̌����Ă����
    private PlayerDire_Mode startRotDire = PlayerDire_Mode.normal;
    //���߂ĉ�]���[�h�ɓ�������
    private bool isStartRotDire = false;

    /// <summary>
    /// ��]����ہA�ǂ�����ɂ��邩
    /// </summary>
    private enum BasicRot_State
    {
        none,       //�ʏ�
        ground,     //�n��
        wall,       //��
    }
    private BasicRot_State rotState = BasicRot_State.none;

    #endregion

    //�d�͑���p�X�N���v�g
    private LocalGravityChanger gravityChanger = new LocalGravityChanger();

    /// <summary>
    /// �v���C���[�̕���
    /// </summary>
    private enum PlayerDire_Mode
    {
        normal,
        right,
        left,
    }
    [SerializeField, Header("�v���C���[�̌����Ă����")] private PlayerDire_Mode nowDire = PlayerDire_Mode.right;


    void Start()
    {
        //�v���C���[�̃R���C�_�[���擾
        box = this.GetComponent<BoxCollider2D>();
        //�v���C���[�̃��W�b�h�{�f�B���擾
        playerRB = this.GetComponent<Rigidbody2D>();

        //�������x�ݒ�
        PlayerSpeedChange(1.0f);

        //�v���C���[�̃��W�b�h�{�f�B���d�͑���X�N���v�g�ɓn��
        gravityChanger.SetGravityChange(playerRB);

        //�J�n���̃v���C���[�̌����Ă������ݒ�
        PlayerDirection(nowDire);
    }

    private void Update()
    {

    }


    private void FixedUpdate()
    {
        //�v���C���[�̓���
        PlayerInput();

        //��]������Ȃ��Ƃ�
        if (!CheckNowRotate())
        {
            if(nowRot != PlayerRot_Mode.rot)
            {
                //��]���邩�̔���
                CheckRotate();
            }
        }

        //�ړ����������邩��]���������邩
        if (!isRot)
        {
            //�ړ�����
            PlayerMove();
        }
        else
        {
            //��]����
            PlayerRotate();

            //��]��̏���
            RotateAfter();
        }

        Gravity();
    }

    /// <summary>
    /// �d�͊֘A�̏���
    /// </summary>
    private void Gravity()
    {
        //�d�͂̕ύX�̗L��
        gravityChanger.GravityChange(CheckUseGravity());

        //�d�͂̕����ݒ�
        Vector2 gravityDire = -this.transform.up;

        //�d�͂̕����ύX
        gravityChanger.ChangeGravityDirection(gravityDire);

        //�d�͗p�̍X�V
        gravityChanger.GravityUpdate();
    }

    /// <summary>
    /// �v���C���[�̓���
    /// </summary>
    private void PlayerInput()
    {
        //�E����
        if (Input.GetKey(KeyCode.D))
        {
            plVec = this.transform.right;
            PlayerDirection(PlayerDire_Mode.right);
        }
        //������
        else if (Input.GetKey(KeyCode.A))
        {
            plVec = -this.transform.right;
            PlayerDirection(PlayerDire_Mode.left);
        }
        //����������ĂȂ��Ƃ�
        else
        {
            plVec = Vector2.zero;
            PlayerDirection(PlayerDire_Mode.normal);
        }

    }

    /// <summary>
    /// �v���C���[�̌����Ă��������ύX
    /// </summary>
    /// <param name="_isRight">false=�� / true=�E</param>
    private void PlayerDirection(PlayerDire_Mode plDire)
    {
        Vector3 plSize = this.transform.localScale;
        //��
        if (plDire == PlayerDire_Mode.left)
        {
            //�}�C�i�X�ɂ��āA�������ɂ���
            plSize.x = -(Mathf.Abs(plSize.x));
        }
        //�E
        else if(plDire == PlayerDire_Mode.right)
        {
            //�}�C�i�X���Ȃ����āA�E�����ɂ���
            plSize.x = Mathf.Abs(plSize.x);
        }
        nowDire = plDire;
        this.transform.localScale = plSize;
    }

    /// <summary>
    /// �v���C���[�̈ړ�����
    /// </summary>
    private void PlayerMove()
    {
        //�v���C���[�ړ�����
        Vector2 newPLVelo = playerRB.velocity;
        //���ړ��̎�
        if (plVec.x != 0)
        {
            newPLVelo.x = plVec.x * plSpeed; 
        }
        //�c�ړ��̎�
        if (plVec.y != 0)
        {
            newPLVelo.y = plVec.y * plSpeed;
        }
        //velocity�ύX
        playerRB.velocity = newPLVelo;
    }

    /// <summary>
    /// ��]���邩�̔���
    /// </summary>
    private void CheckRotate()
    {
        Vector2 size    = box.size;                 //�R���C�_�[�̃T�C�Y
        Vector2 offset  = box.offset;               //�R���C�_�[�̃I�t�Z�b�g
        Vector2 down    = -this.transform.up;       //�v���C���[�̉������x�N�g��

        //�����̍��W
        //�����̃��[�J�����W���擾
        Vector2 local_leftBottom = offset + new Vector2(-size.x / 2f, -size.y / 2f);
        //���[���h���W�ɕϊ�
        Vector2 leftBottom = transform.TransformPoint(local_leftBottom);

        //�E���̍��W
        //�E���̃��[�J�����W���擾
        Vector2 local_rightBottom = offset + new Vector2(size.x / 2f, -size.y / 2f);
        //���[���h���W�ɕϊ�
        Vector2 rightBottom = transform.TransformPoint(local_rightBottom);

        //�O���̍��W
        //�O���̃��[�J�����W���擾
        Vector2 local_forward = offset + new Vector2(size.x / 2f, 0f);
        //���[���h���W�ɕϊ�
        Vector2 forward = transform.TransformPoint(local_forward);

        //�����A�E�����n�ʂƐG��Ă��邩���C�Ŕ���
        //����
        bool isLeftGrounded = Physics2D.Raycast(leftBottom, down, 0.05f, groundLayer);
        //�E��
        bool isRightGrounded = Physics2D.Raycast(rightBottom, down, 0.05f, groundLayer);

        //�O���̕ǂƃ��C���G��Ă邩
        bool isForwardGrounded = Physics2D.Raycast(forward, plVec, 0.5f, groundLayer);

        #region ���C�\��(�f�o�b�O�p)
        Debug.DrawRay(leftBottom, down * 0.05f, Color.green);
        Debug.DrawRay(rightBottom, down * 0.05f, Color.green);
        Debug.DrawRay(forward, plVec * 0.5f, Color.green);
        #endregion

        //��]�I�����̎�
        if (nowRot == PlayerRot_Mode.finish)
        {
            //��]����A�O�̏ꏊ�ɖ߂낤�Ƃ����Ƃ�
            if (nowDire != PlayerDire_Mode.normal && nowDire != startRotDire)
            {
                //���[�h��ʏ��
                nowRot = PlayerRot_Mode.none;
                //��]��Ԃ�������
                isRot = false;
                isStartRotDire = false;
                rotState = BasicRot_State.none;

                //��]�̎x�_���W�͕ς��Ȃ����߁A��قǂ̍��W�𗘗p
                rayOut_savePos = pivotPos;
            }
            else
            {
                //�����ƉE���������n�ʂƐG��Ă鎞
                if (isLeftGrounded && isRightGrounded)
                {
                    //��]���[�h��ʏ�ɖ߂�
                    nowRot = PlayerRot_Mode.none;
                    //��]�t���O��������Ԃ�
                    isRot = false;

                    //��]���̏����Ɏg���Ă�����������Ԃɖ߂�
                    startRotDire = PlayerDire_Mode.normal;
                    isStartRotDire = false;
                    rotState = BasicRot_State.none;
                }
            }
            return;
        }

        //�������E���̂ǂ��炩�݂̂��n�ʂƐG��Ă鎞
        if (isLeftGrounded ^ isRightGrounded && rotState != BasicRot_State.wall)
        {
            //��]���n�ʂ�
            rotState = BasicRot_State.ground;
            //���C���͂ݏo���ď��߂Ă̏����̎�
            if (!isRayOut)
            {
                rayOut_savePos = !isLeftGrounded ? leftBottom : rightBottom;
                isRayOut = true;
            }

            //���ꂼ��̍��W�̏����_��2�ʈȉ���؂�̂�
            //���C���͂ݏo�����Ƃ��̍��W
            Vector3 save = rayOut_savePos;
            save.x = Mathf.Floor(save.x * 10f) / 10f;
            save.y = Mathf.Floor(save.y * 10f) / 10f;
            save.z = Mathf.Floor(save.z * 10f) / 10f;
            //���݂̉�]�x�_�̍��W
            Vector3 pivot = pivotPoint.position;
            pivot.x = Mathf.Floor(pivot.x * 10f) / 10f;
            pivot.y = Mathf.Floor(pivot.y * 10f) / 10f;
            pivot.z = Mathf.Floor(pivot.z * 10f) / 10f;

            //���C���͂ݏo�������W�Ɖ�]�x�_���ꏏ�̎�
            if (save == pivot)
            {
                //��]�ʂ�������
                rotateAmount = 0f;

                //��]�̎x�_�̈ʒu��ύX
                pivotPos = pivotPoint.position;

                //���������ł̈ړ����ł��Ȃ��悤��
                playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

                Debug.Log("��]����(�n��)");
                isRot = true;
            }
            return;
        }
        //�O���̕ǂɐG�ꂽ��
        else if (isForwardGrounded && rotState != BasicRot_State.ground)
        {
            Vector2 pivot = this.transform.position;
            pivot.x = Mathf.Floor(pivot.x * 100f) / 100f;
            pivot.y = Mathf.Floor(pivot.y * 100f) / 100f;
            Vector2 pivotOffset = this.transform.up * 0.5f;
            pivot += pivotOffset;

            //��]�̎x�_�̈ʒu��ύX
            pivotPos = pivot;

            //��]�ʂ�������
            rotateAmount = 0f;

            //��]���ǂ�
            rotState = BasicRot_State.wall;

            //���������ł̈ړ����ł��Ȃ��悤��
            playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

            Debug.Log("��]����(��)");
            isRot = true;

            return;
        }

        //�v���C���[�𕨗������ł̈ړ����ł���悤�ɂ���
        //(��]�͂��������Ȃ����߁Arotation�͂��̂܂܌Œ�)
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log("��]���Ȃ�");
        //��]���Ȃ��Ƃ��āAfalse��Ԃ�
        isRot = false;

        isRayOut = false;
        nowRot = PlayerRot_Mode.none;

        rotState = BasicRot_State.none;
    }

    /// <summary>
    /// �^���d�͂��g�����ǂ���
    /// </summary>
    /// <returns>false=�^���d�͂��K�v / true=�^���d�͂��K�v</returns>
    private bool CheckUseGravity()
    {
        //���݂�z���̊p�x���擾
        float nowRotZ = (int)this.transform.localEulerAngles.z;

        //�p�x���������ȊO�̎�
        if(nowRotZ % 360 != 0)
        {
            Debug.Log("�^���d��");
            return true;
        }
        Debug.Log("�ʏ�d��");
        return false;
    }

    /// <summary>
    /// ���݉�]�����ǂ���
    /// </summary>
    /// <returns>true=��]�� / false=��]������Ȃ�</returns>
    private bool CheckNowRotate()
    {

        //���݂�z���̊p�x���擾
        float nowRotZ = (float)Math.Round(this.transform.localEulerAngles.z, MidpointRounding.AwayFromZero);        

        //��]���܂��r���̎�
        if(nowRotZ % limitRot != 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// �v���C���[�̉�]����
    /// </summary>
    private void PlayerRotate()
    {
        //��]�I����Ԃ���Ȃ��Ƃ�
        if(nowRot != PlayerRot_Mode.finish)
        {
            //��]���[�h�ɕύX
            nowRot = PlayerRot_Mode.rot;

            //�n�ʊ
            if(rotState == BasicRot_State.ground)
            {
                //��]����
                Rot_Ground();
            }
            //�Ǌ
            else if(rotState == BasicRot_State.wall)
            {
                //��]����
                Rot_Wall();
            }

        }
    }

    /// <summary>
    /// ��]����(�n��)
    /// </summary>
    private void Rot_Ground()
    {
        //�����Ă�������E�����A�ǂ��炩�������Ă鎞�̂�
        if (nowDire != PlayerDire_Mode.normal)
        {
            //���߂ĉ�]���[�h�ɓ�������
            if (!isStartRotDire)
            {
                //���߂ĉ�]���[�h�ɓ��������̃v���C���[�̌����Ă�������擾
                startRotDire = nowDire;
                //���̏�����1�x����
                isStartRotDire = true;
            }

            //��]�����
            float step = rotSpeed * Time.deltaTime;

            //���߂ĉ�]���[�h�ɓ��������̃v���C���[�̌����Ă�����ƌ��݂̕����������Ƃ�
            if (startRotDire == nowDire)
            {
                //��x�ɉ�]����ʂ𒴂�����
                if (rotateAmount + step > limitRot)
                {
                    //�ڕW�p�x�Ŏ~�߂�
                    step = limitRot - rotateAmount;
                }
            }
            //�Ⴄ�Ƃ�
            else
            {
                if (rotateAmount - step < 0)
                {
                    step = 0 + rotateAmount;
                }
            }

            //��]
            //�O�i��
            if (nowDire == PlayerDire_Mode.right)
            {
                //pivotPos���x�_��z���ŉ�]
                this.transform.RotateAround(pivotPos, Vector3.forward, -step);

                if (startRotDire == nowDire)
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount += step;
                }
                else
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount -= step;
                }
            }
            //��ގ�
            else if (nowDire == PlayerDire_Mode.left)
            {
                //pivotPos���x�_��z���ŉ�]
                this.transform.RotateAround(pivotPos, Vector3.forward, step);

                if (startRotDire == nowDire)
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount += step;
                }
                else
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount -= step;
                }
            }

            //��]�����ʂ�1�x�ɉ�]�ł���ʈȏ�ɂȂ�����
            if (rotateAmount >= limitRot || rotateAmount <= 0)
            {
                //��]�I�����[�h�ɕύX
                nowRot = PlayerRot_Mode.finish;
            }
        }
    }

    /// <summary>
    /// ��]����(��)
    /// </summary>
    private void Rot_Wall()
    {
        if (nowDire != PlayerDire_Mode.normal)
        {
            //���߂ĉ�]���[�h�ɓ�������
            if (!isStartRotDire)
            {
                //���߂ĉ�]���[�h�ɓ��������̃v���C���[�̌����Ă�������擾
                startRotDire = nowDire;
                //���̏�����1�x����
                isStartRotDire = true;
            }

            //��]�����
            float step = rotSpeed * Time.deltaTime;

            //���߂ĉ�]���[�h�ɓ��������̃v���C���[�̌����Ă�����ƌ��݂̕����������Ƃ�
            if (startRotDire == nowDire)
            {
                //��x�ɉ�]����ʂ𒴂�����
                if (rotateAmount + step > limitRot)
                {
                    //�ڕW�p�x�Ŏ~�߂�
                    step = limitRot - rotateAmount;
                }
            }
            //�Ⴄ�Ƃ�
            else
            {
                if (rotateAmount - step < 0)
                {
                    step = 0 + rotateAmount;
                }
            }

            //��]
            //�O��
            if (nowDire == PlayerDire_Mode.right)
            {
                //pivotPos���x�_��z���ŉ�]
                this.transform.RotateAround(pivotPos, Vector3.forward, step);

                if (startRotDire == nowDire)
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount += step;
                }
                else
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount -= step;
                }

            }
            //���
            else if (nowDire == PlayerDire_Mode.left)
            {
                //pivotPos���x�_��z���ŉ�]
                this.transform.RotateAround(pivotPos, Vector3.forward, -step);

                if (startRotDire == nowDire)
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount += step;
                }
                else
                {
                    //��]�����ʂ�ǉ�
                    rotateAmount -= step;
                }

            }

            //��]�����ʂ�1�x�ɉ�]�ł���ʈȏ�ɂȂ�����
            if (rotateAmount >= limitRot || rotateAmount <= 0)
            {
                //��]�I�����[�h�ɕύX
                nowRot = PlayerRot_Mode.finish;
            }
        }
    }

    /// <summary>
    /// ��]�����I���㏈��
    /// </summary>
    private void RotateAfter()
    {
        //��]�����I����̎�
        if(nowRot == PlayerRot_Mode.finish)
        {
            //�����������\�ɂ���
            playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

            //���݂̌����Ă���p���ʏ�ȊO(�E����)�̎�
            if (nowDire != PlayerDire_Mode.normal)
            {
                PlayerMove();
            }
        }
    }

    /// <summary>
    /// �v���C���[�̑��x��ύX
    /// </summary>
    /// <param name="_changeSpeed">���x�ω��̔{��</param>
    public void PlayerSpeedChange(float _changeSpeed)
    {
        //��{���x�ɕω��{����������
        plSpeed = basicSpeed * _changeSpeed;

        //���@��]���x�ɂ����x�{����������
        rotSpeed = rotSpeed * _changeSpeed;
    }
}
