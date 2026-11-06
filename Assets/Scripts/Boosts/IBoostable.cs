// IBoostable — anything that can have a temporary BoostType activated on it
// (and have that effect undone again when its timer runs out).
public interface IBoostable
{
    void ApplyBoost(BoostType type, BoostDefinition definition);
    void RevertBoost(BoostType type);
}
