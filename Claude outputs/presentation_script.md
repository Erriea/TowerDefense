# Tower Defender — Part 1 Presentation Script

Short outline for the video. Each point names the script(s) to have open on screen and one line to say about it.

## 1. Intro (either of you, ~15 sec)
"We're building a procedurally generated tower defense game in Unity. I'll cover terrain, pathways and defenders; [friend] covers enemy spawning and the mana system."

## 2. AP — Terrain & Pathways (`MapGenerator.cs`)
- Show the mesh generating fresh at runtime — re-enter play mode to prove it's different every time.
- Say: "Terrain is Perlin noise, quantized into steps for the pixelated look, with a random seed each run."
- Point out at least 3 winding paths leading to the center — say how they're generated (noise-driven meander, not just straight lines) and spread evenly around the map.

## 3. AP — Tower Placement (`TowerManager.cs`)
- Show the WizardTower spawning at the map's center point.
- Say: "The tower always spawns at the terrain's center, at the correct height for that run's terrain."
- Mention health/auto-attack status honestly if not finished yet.

## 4. AP — Defender Placement (`DefenderPlacementGenerator.cs`, `Defender.cs`, `Golem.cs`)
- Demo: click the placement button, move near a path, show the golem snapping to the nearest valid spot beside (not on) the path, click to confirm.
- Say: "Valid spots are generated next to the paths so defenders never block the enemy route."
- Show the golem attacking an enemy in range and taking damage.

## 5. Friend — Enemy Spawning & Movement (`MonsterSpawner.cs`, `Enemy.cs`)
- Show enemies spawning on a path and walking it toward the tower.
- Say: "Enemies spawn on one of the generated paths and follow its waypoints to the tower, attacking any defender they pass."
- Show an enemy attacking the tower and the tower losing health.

## 6. Friend — Mana System (`Mana.cs`, `ManaUI.cs`)
- Show the mana UI and the defender button greying out when mana is too low.
- Say: "Mana regenerates over time and gates how often defenders can be placed."

## 7. Wrap-up (either, ~15 sec)
- One line on working together: separate branches on GitHub, merged periodically.
- One line on shared design: Tower/Defender/Enemy all share a common damage/health pattern so the code isn't duplicated three times.
- Thank you / questions.

---
**Reminder:** rehearse the handoffs (who talks when) once before recording — that's usually the only rough part.
