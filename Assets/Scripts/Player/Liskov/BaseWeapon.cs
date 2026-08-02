using UnityEngine;

// Standalone Liskov Substitution Principle demo. Not part of the real weapon system used by
// AxeWeapon/FireballWeapon/LaserWeapon - those implement IWeapon independently and don't
// subclass each other, so this is the only place in the project that actually exercises LSP
// via a base/derived class relationship. See TestBaseWeapon for the substitutability check.
public class BaseWeapon
{
    private int range = 5;
    private int damage = 10;

    public virtual void Attack()
    {
        Debug.Log("BaseWeapon Attack, " + range + "," + damage);
    }
}

// A proper LSP-respecting subtype: it only adds behavior (its own log line) on top of the
// base Attack() via base.Attack() - it never changes what Attack() means, throws where the
// base wouldn't, or requires extra setup the base doesn't. That's what makes it safe to pass
// wherever a BaseWeapon is expected (see TestBaseWeapon.AttackEnemy).
public class LightningWeapon : BaseWeapon
{
    bool isLightOn = false;
    public override void Attack()
    {
        Debug.Log("LongRangeWeapon " + isLightOn);
       base.Attack();
    }
}


