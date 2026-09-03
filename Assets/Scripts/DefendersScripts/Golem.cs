using UnityEngine;

// Golem.cs — one subclass per defender type
public class Golem : Defender
{
    // only overrides what's actually different about a Golem,
    // e.g. a special attack animation trigger
    protected override void Update()
    {
        base.Update();
        // Golem-specific stuff, if any
    }
}