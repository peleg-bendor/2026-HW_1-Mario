using UnityEngine;

public class BaseWeapon
{
    private int range = 5;
    private int damage = 10;

    public virtual void Attack()
    {
        Debug.Log("BaseWeapon Attack, " + range + "," + damage);
    }
}

public class LightningWeapon : BaseWeapon
{
    bool isLightOn = false;
    public override void Attack()
    {
        Debug.Log("LongRangeWeapon " + isLightOn);
       base.Attack();
    }
}


