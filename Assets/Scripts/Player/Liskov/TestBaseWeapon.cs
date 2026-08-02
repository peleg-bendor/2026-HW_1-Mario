using UnityEngine;

public class TestBaseWeapon : MonoBehaviour
{
    void Start()
    {
        BaseWeapon bs = new BaseWeapon();
        AttackEnemy(bs);
        LightningWeapon bslw = new LightningWeapon();
        AttackEnemy(bslw);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void AttackEnemy(BaseWeapon attackingWeapon)
    {
        attackingWeapon.Attack();
    }
}
