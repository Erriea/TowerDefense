// IUpgradable — anything that can receive an UpgradeTier (defenders and the tower),
// and have that upgrade's effects undone again once its timer runs out.
public interface IUpgradable
{
    void ApplyUpgrade(UpgradeTier tier);
    void RevertUpgrade();
}
