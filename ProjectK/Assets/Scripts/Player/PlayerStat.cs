using System;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    private float maxHp;
    [SerializeField] private float hp;
    private float maxStemina;
    [SerializeField] private float stamina;

    private void Awake()
    {
        maxHp = 100;
        hp = maxHp;

        maxStemina = 100;
        stamina = maxStemina;
    }

    public float GetHP()
    {
        return hp;
    }

    public float GetStamina()
    {
        return stamina;
    }

    public void ApplyHp(float inApplyHpValue)
    {
        hp += inApplyHpValue;

        if(CheckDie() == true)
        {
            return;
        }

        Logger.Info("���� HP: " + hp);
    }

    public bool CheckDie()
    {
        if (hp <= 0)
        {
            Logger.Info("�÷��̾ ����");
            return true;
        }

        return false;
    }
}
