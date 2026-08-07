// The minimum every weapon WeaponsHandler manages has to offer. IsAvailable() sits here
// rather than on a narrower interface because cycling can't skip an unusable weapon unless
// every weapon in the list can answer the question.
public interface IWeapon
{
    void Attack();
    bool IsAvailable();
}
