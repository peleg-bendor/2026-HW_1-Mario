// Weapons that hold a finite supply and can be given more. Kept off IWeapon so a weapon
// with nothing to reload never has to implement a Reload() that does nothing or throws.
public interface IReloadWeapon : IWeapon
{
    void Reload();
}
