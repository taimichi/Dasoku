using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //�v���C���[�̊�b���x
    [SerializeField, Header("��b���x")] private float basicSpeed = 1.0f;
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
    //��]���̊p�x�̕ω���
    private float rotAmountChange = 0f;

    /// <summary>
    /// �v���C���[�̕���
    /// </summary>
    private enum PlayerDire_Mode
    {
        right,
        left,
    }

    [SerializeField, Header("�v���C���[�̌����Ă����")] private PlayerDire_Mode nowDire = PlayerDire_Mode.right;

    //��]���̒��S���W
    private Vector2 saveRotPos;
    //��]���J�n������
    private bool isRot = false;
    //��]���Ƀv���C���[�̐e�I�u�W�F�N�g�ƂȂ�I�u�W�F�N�g
    //��]�̈ʒu��ύX���邽��
    [SerializeField] private Transform RotPoint;

    void Start()
    {
        //�v���C���[�̃��W�b�h�{�f�B���擾
        playerRB = this.GetComponent<Rigidbody2D>();
        //�������x�ݒ�
        PlayerSpeedChange(1.0f);

        //�v���C���[�̃��W�b�h�{�f�B���d�͕ύX�X�N���v�g�ɓn��
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

        //��]������Ȃ��A�󒆂���Ȃ��Ƃ�
        //��]�����󒆂ł͈ړ����Ȃ�
        if (!CheckNowRotate() && !CheckNowAir())
        {
            Debug.Log("�ړ���");
            //�ړ�����
            PlayerMove();
        }
        else
        {
            if (!isRot)
            {
                //��]�ʒu��ύX
                RotPoint.position = saveRotPos;
                //��]�ʒu�p�I�u�W�F�N�g���v���C���[�̐e�I�u�W�F�N�g��
                this.transform.parent = RotPoint;
                isRot = true;

                //�v���C���[�����������Ă��Ԃ�������
                if (nowDire == PlayerDire_Mode.left)
                {
                    rotAmountChange = limitRot;
                }
            }
            //��]����
            PlayerRotate();
        }

        //�d�͂̕ύX�̗L��
        gravityChanger.GravityChange(CheckUseGravity());
        //�d�͗p�̍X�V
        gravityChanger.GravityUpdate();

        //�d�͂̕����ݒ�
        Vector2 gravityDire = -this.transform.up;
        //�����_�ȉ���؂�̂�
        gravityDire.x = (int) gravityDire.x;
        gravityDire.y = (int)gravityDire.y;

        //�d�͂̕����ύX
        gravityChanger.ChangeGravityDirection(gravityDire);


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
        else
        {
            plVec = Vector2.zero;
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
        //���C�̒���
        float duration = 0.7f;
        //���C�@�v���C���[�̒��S���牺�����Ɍ�����
        Ray2D ray = new Ray2D(this.transform.position, -this.transform.up);

        //���C�\��
        Debug.DrawRay(ray.origin, ray.direction * duration, Color.green);

        //���C���I�u�W�F�N�g���擾�����������[�v
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, duration))
        {
            //�I�u�W�F�N�g�ƐڐG���Ă���Ƃ�
            if (hit.collider)
            {
                //�ڐG�����̂��n�ʂ�������
                if (hit.collider.tag == "Ground_Normal")
                {
                    //��]������Ȃ��Ƃ�
                    if (!CheckNowRotate())
                    {
                        //�e�I�u�W�F�N�g������
                        this.transform.parent = null;
                        //��]���̒��S���W���X�V
                        saveRotPos = hit.point;
                        isRot = false;
                    }
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
    /// �v���C���[�̉�]����
    /// </summary>
    private void PlayerRotate()
    {
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        //�d�͂̕����ݒ�
        Vector2 gravityDire = -this.transform.up;
        //�����_�ȉ���؂�̂�
        gravityDire.x = Mathf.FloorToInt(gravityDire.x);
        gravityDire.y = Mathf.FloorToInt(gravityDire.y);

        //�d�͂̕����ύX
        gravityChanger.ChangeGravityDirection(gravityDire);

        //��]�X�s�[�h
        float rotSpeed = Mathf.Floor((limitRot / rotationTime * Time.deltaTime) * 100) / 100;

        //��]����
        //�E�����Ă鎞
        if (nowDire == PlayerDire_Mode.right)
        {
            //while (rotAmountChange <= limitRot)
            //{
            //    //���݂̊p�x���擾
            //    Vector3 PLRot = RotPoint.localEulerAngles;

            //    PLRot.z += -rotSpeed;
            //    rotAmountChange += rotSpeed;

            //    //�p�x��ύX
            //    RotPoint.localEulerAngles = PLRot;

            //    yield return null;
            //}

            Vector3 PLRot = RotPoint.localEulerAngles;
            PLRot.z = Mathf.FloorToInt(PLRot.z) - 90;
            RotPoint.localEulerAngles = PLRot;
        }
        //�������̎�
        else if (nowDire == PlayerDire_Mode.left)
        {
            //while (rotAmountChange <= limitRot)
            //{
            //    //���݂̊p�x���擾
            //    Vector3 PLRot = RotPoint.localEulerAngles;

            //    PLRot.z += rotSpeed;
            //    rotAmountChange += rotSpeed;

            //    //�p�x��ύX
            //    RotPoint.localEulerAngles = PLRot;

            //    yield return null;
            //}

            Vector3 PLRot = RotPoint.localEulerAngles;
            PLRot.z = Mathf.FloorToInt(PLRot.z) + 90;
            RotPoint.localEulerAngles = PLRot;
        }

        yield return null;
    }

    /// <summary>
    /// ��]�����ǂ���
    /// </summary>
    /// <returns>false=��]���ĂȂ� / true=��]��</returns>
    private bool CheckNowRotate()
    {
        //���݂�z���̊p�x���擾
        int nowRotZ = Mathf.FloorToInt(Mathf.Abs(RotPoint.transform.localEulerAngles.z));
        //90�x�Ŋ���؂�Ȃ��Ƃ�
        if(nowRotZ % limitRot != 0)
        {
            //��]���̓v���C���[���ړ����Ȃ��悤�ɂ���
            playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

            Debug.Log("��]��");
            //��]��
            return true;   
        }
        //�ړ��ł���悤�ɂ���
        playerRB.constraints = RigidbodyConstraints2D.None;
        rotAmountChange = 0f;
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
