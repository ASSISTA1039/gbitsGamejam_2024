using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleItem : IState
{
    private readonly MonsterStateController _controller;

    public IdleItem(MonsterStateController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("���˽���Idle״̬���ȴ�ѡ��");
    }

    public void Tick()
    {
        // ����п��ÿ��ƣ�ת�Ƶ�����״̬
        //if (_controller.monster.GetItems().Count > 0)
        //{
        //    _controller.IntoItemPhase();
        //}
    }

    public void OnExit()
    {
        Debug.Log("�����뿪Idle״̬");
    }
}

