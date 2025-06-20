using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �I�u�W�F�N�g���Ƃɏd�͂�ύX���邽�߂̃N���X
/// </summary>
public class LocalGravityChanger
{
    private Rigidbody2D rb;
    //�V�d�͂̕���
    private Vector2 gravityVec;

    //�d�͂�ύX���邩�ǂ���
    private bool isGravity = false;

    //�d�͂̋���
    private float gravityStrength = 9.81f;

    /// <summary>
    /// �d�͂�������
    /// </summary>
    private void UseGravity()
    {
        if (rb != null)
        {
            //�^���d��
            //���]�������ʏ�d�͕��̗͂������邱�ƂŁA�ʏ�̏d�͂�ł�����
            rb.AddForce(gravityVec * gravityStrength * rb.mass);
        }
    }
    
    /// <summary>
    /// �d�͍X�V�֐�
    /// </summary>
    public void GravityUpdate()
    {
        //�d�͕ύX���Ă�Ƃ�
        if (isGravity)
        {
            rb.gravityScale = 0;
            UseGravity();
        }
        //�d�͕ύX���Ă��Ȃ��Ƃ�
        else
        {
            rb.gravityScale = 1;
        }
    }

    /// <summary>
    /// �d�͂̕�����ύX����
    /// </summary>
    /// <param name="direction">�ύX����d�͂̕���</param>
    public void ChangeGravityDirection(Vector2 direction)
    {
        //�d�͂̕������擾
        gravityVec = direction;
    }

    /// <summary>
    /// �d�͂�ύX���邩�ǂ���
    /// </summary>
    /// <param name="_isGravity">false=�ʏ�d�� / true=�ύX�d��</param>
    public void GravityChange(bool _isGravity)
    {
        isGravity = _isGravity;
    }

    /// <summary>
    /// �d�͂�ύX����I�u�W�F�N�g�̃��W�b�h�{�f�B��ݒ�
    /// </summary>
    /// <param name="_rb">�d�͂�ύX�������I�u�W�F�N�g�̃��W�b�h�{�f�B</param>
    public void SetGravityChange(Rigidbody2D _rb)
    {
        rb = _rb;
    }

}
