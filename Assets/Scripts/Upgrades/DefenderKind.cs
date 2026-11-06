// DefenderKind — identifies which upgrade track a Defender or the Tower belongs to.
// Sniper = the ranged unit (ArcherDefender.cs internally), Raccoon = the high-health
// frontline unit (BulwarkDefender.cs internally) — names kept in sync with the
// in-game display text, even though the underlying class/file names are unchanged.
public enum DefenderKind
{
    Golem,
    Sniper,
    Raccoon,
    Tower
}
