// Anything a projectile can destroy, and nothing more. Hurting Mario on touch is a separate
// concern, handled by attaching SC_Death, so an enemy that shouldn't hurt him simply doesn't
// get one - without this interface needing to know that such an enemy exists.
public interface IEnemy
{
    void Kill();
}
