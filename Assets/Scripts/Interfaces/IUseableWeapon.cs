// Weapons that start locked and are unlocked by a power-up. Kept off IWeapon for the same
// reason as IReloadWeapon: a weapon Mario always carries would have to implement an Equip()
// that means nothing.
public interface IUseableWeapon : IWeapon
{
    void Equip();
    void UnEquip();
}
