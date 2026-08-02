using UnityEngine;

// Runs the LSP demo on scene load. Its only visible effect is the Console output below -
// there's no gameplay tied to this, it exists purely to prove the substitutability point.
public class TestBaseWeapon : MonoBehaviour
{
    void Start()
    {
        // Same method, AttackEnemy(BaseWeapon), called with a plain BaseWeapon and then with
        // a LightningWeapon. AttackEnemy has no idea which one it got and doesn't need to -
        // both work correctly. That's the Liskov Substitution Principle in action: a subtype
        // (LightningWeapon) can stand in for its base type (BaseWeapon) without breaking
        // anything the caller expects.
        BaseWeapon bs = new BaseWeapon();
        AttackEnemy(bs);
        LightningWeapon bslw = new LightningWeapon();
        AttackEnemy(bslw);
    }

    public void AttackEnemy(BaseWeapon attackingWeapon)
    {
        attackingWeapon.Attack();
    }
}
