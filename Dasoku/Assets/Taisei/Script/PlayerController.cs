using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //�v���C���[�̊�b���x
    [SerializeField, Header("��{���x")] private float basicSpeed = 1.0f;
    //�v���C���[�̈ړ����x
    private float plSpeed = 1.0f;
    //�v���C���[�̈ړ�����
    private Vector2 plVec;
    //�v���C���[�̃��W�b�h�{�f�B
    private Rigidbody2D playerRB;

    //90�x��]�ɂ����鎞��
    private float rotationTime = 1f;

    //�d�͕ύX�p�X�N���v�g
    private LocalGravityChanger gravityChanger = new LocalGravityChanger();

    //��x�ɉ�]�ł���p�x
    private int limitRot = 90;

    //�ύX�O��z���̊p�x
    private float beforeRotZ = 0;

    void Start()
    {
        //�v���C���[�̃��W�b�h�{�f�B���擾
        playerRB = this.GetComponent<Rigidbody2D>();
        //�������x�ݒ�
        PlayerSpeedChange(1.0f);

        //�v���C���[�̃��W�b�h�{�f�B���d�͕ύX�X�N���v�g�ɓn��
        gravityChanger.SetGravityChange(playerRB);
    }


    private void FixedUpdate()
    {
        //�v���C���[�̓���
        PlayerInput();

        //��]������Ȃ��A�󒆂���Ȃ��Ƃ�
        //��]�����󒆂ł͈ړ����Ȃ�
        if (!CheckNowRotate() && !CheckNowAir())
        {
            //�ړ�����
            PlayerMove();
            //�󒆂ɂȂ������̊p�x���擾
            beforeRotZ = this.transform.localEulerAngles.z;
        }
        else
        {
            //��]����
            PlayerRotate();
        }
        //�d�͂̕ύX�̗L��
        gravityChanger.GravityChange(CheckUseGravity());

        //�d�͗p�̍X�V
        gravityChanger.GravityUpdate();

    }

    /// <summary>
    /// �v���C���[�̓���
    /// </summary>
    private void PlayerInput()
    {
        //plVec.x = Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.D))
        {
            plVec = this.transform.right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            plVec = -this.transform.right;
        }
        else
        {
            plVec = Vector2.zero;
        }
    }

    /// <summary>
    /// �v���C���[�̈ړ�����
    /// </summary>
    private void PlayerMove()
    {
        //�v���C���[�ړ�����
        Vector2 plVelo = playerRB.velocity;
        plVelo = plVec * plSpeed;
        playerRB.velocity = plVelo;
    }

    /// <summary>
    /// �󒆂��ǂ���
    /// </summary>
    /// <returns>false=�n�ʂɂ��� / true=�󒆂ɂ���</returns>
    private bool CheckNowAir()
    {
        //���C�@�v���C���[�̒��S���牺�����Ɍ�����
        Ray2D ray = new Ray2D(this.transform.position, -this.transform.up);

        //���C�\��
        Debug.DrawRay(ray.origin, ray.direction * 0.6f, Color.green, 0.015f);

        //���C���I�u�W�F�N�g���擾�����������[�v
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, 0.6f))
        {
            //�I�u�W�F�N�g�ƐڐG���Ă���Ƃ�
            if (hit.collider)
            {
                //�ڐG�����̂��n�ʂ�������
                if (hit.collider.tag == "Ground_Normal")
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// �^���d�͂��g�����ǂ���
    /// </summary>
    /// <returns>false=�^���d�͂��K�v / true=�^���d�͂��K�v</returns>
    private bool CheckUseGravity()
    {
        //���݂�z���̊p�x���擾
        float nowRotZ = this.transform.localEulerAngles.z;

        //�p�x���������ȊO�̎�
        if(nowRotZ % 360 != 0)
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
        //��]�\�ɂȂ�����
        if (CheckNowAir() || CheckNowRotate())
        {
            //�d�͂̕����ύX
            gravityChanger.ChangeGravityDirection(-this.transform.up);

            //��]�X�s�[�h
            float rotSpeed = (limitRot * Time.deltaTime) / rotationTime;
            Vector3 PLRot = this.transform.localEulerAngles;
            
            //�ړ��L�[��������Ă鎞
            if(Input.GetKey(KeyCode.D))
            {
                if (PLRot.z > beforeRotZ - limitRot)
                {
                    //D�L�[(�i��)��������Ă鎞
                    PLRot.z -= rotSpeed;
                }
                else
                {
                    PLRot.z = beforeRotZ - limitRot;
                }
            }
            else if(Input.GetKey(KeyCode.A))
            {
                if (PLRot.z- 360f < beforeRotZ)
                {
                    //A�L�[(�߂�)��������Ă鎞
                    PLRot.z += rotSpeed;
                }
                else
                {
                    PLRot.z = beforeRotZ;
                }
            }
            //�v���C���[�̊p�x��ύX
            this.transform.localEulerAngles = PLRot;
        }
    }

    /// <summary>
    /// ��]�����ǂ���
    /// </summary>
    /// <returns>false=��]���ĂȂ� / true=��]��</returns>
    private bool CheckNowRotate()
    {
        //���݂�z���̊p�x���擾
        int nowRotZ = Mathf.FloorToInt(Mathf.Abs(this.transform.localEulerAngles.z));
        //90�x�Ŋ���؂�Ȃ��Ƃ�
        if(nowRotZ % limitRot != 0)
        {
            Debug.Log("��]��");
            //��]��
            return true;   
        }
        Debug.Log("��]���Ă��Ȃ�");
        //��]���ĂȂ�
        return false; ;
    }

    /// <summary>
    /// �v���C���[�̑��x��ύX
    /// </summary>
    /// <param name="_changeSpeed">���x�ω��̔{��</param>
    public void PlayerSpeedChange(float _changeSpeed)
    {
        //��{���x�ɕω��{����������
        plSpeed = basicSpeed * _changeSpeed;
    }
}
