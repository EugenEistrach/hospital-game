# Hospital Game - Design Document

> PlateUp!-style co-op hospital management. Cure patients, manage chaos, don't let anyone die.

## Core Concept

You run a hospital ER. Patients arrive with conditions. You (and optionally a friend) must diagnose, treat, and discharge them before they lose patience and walk out. Each day brings more patients. Survive as long as you can.

**Inspiration:** PlateUp! but with patients instead of food.

---

## MVP Scope: "Flu Day"

The simplest playable loop - equivalent to PlateUp!'s tomato soup.

### One Condition: Flu
```
Patient arrives → Exam (2s) → Pharmacy (3s) → Discharge → +100 points
```

### Map Layout
```
┌─────────────────────────────────────────────┐
│  WAITING AREA                               │
│  [  ] [  ] [  ] [  ]    ← 4 waiting spots   │
│                                             │
│  ┌──────┐    ┌──────┐                       │
│  │ EXAM │    │ EXAM │   ← 2 exam tables     │
│  └──────┘    └──────┘                       │
│                                             │
│  ┌──────┐    ┌──────┐                       │
│  │PHARMA│    │PHARMA│   ← 2 pharmacies      │
│  └──────┘    └──────┘                       │
│                                             │
│  ══════════════════════                     │
│       DISCHARGE DESK                        │
└─────────────────────────────────────────────┘
```

### Controls
- **Arrow keys**: Move
- **Space**: Pickup / Drop

### Day Loop
1. **Preparation** (5s): "Day X starting..."
2. **Active** (50s): Patients spawn, you work
3. **Complete** (5s): Finish current patients, no new spawns
4. **Summary** (3s): Show stats, auto-continue

### Difficulty Scaling
| Day | Patients | Spawn Interval |
|-----|----------|----------------|
| 1   | 3        | 15s            |
| 2   | 4        | 12s            |
| 3   | 5        | 10s            |
| 4   | 6        | 8s             |
| 5+  | 7+       | 6s             |

### Fail States
- Patient waits 30s → walks out → -50 points, rating drops
- Rating hits 0 → Game Over

### Multiplayer
- Single player: Start immediately
- Coop: Second player can drop in anytime

---

## Post-MVP Roadmap

### Phase 1: More Conditions
Add treatment complexity like PlateUp! menu items.

| Condition | Path | Complexity |
|-----------|------|------------|
| Flu | Exam → Pharmacy → Discharge | ⭐ |
| Broken Arm | Exam → X-Ray → Cast Station → Discharge | ⭐⭐ |
| Food Poisoning | Exam → Lab → IV Drip (timed) → Pharmacy → Discharge | ⭐⭐⭐ |
| Surgery Case | Exam → X-Ray → Prep → Surgery → Recovery → Discharge | ⭐⭐⭐⭐ |

### Phase 2: Supplies System
Stations require items (like PlateUp! ingredients).

```
Pharmacy needs: Meds (from supply shelf)
Cast Station needs: Bandages
IV Drip needs: Saline bag
Surgery needs: Surgical kit
```

Player must fetch supplies AND move patients.

### Phase 3: Chaos Events
Random events that create pressure (like PlateUp! fires/mess).

| Event | Effect |
|-------|--------|
| Biohazard spill | Area blocked until cleaned |
| Patient vomit | Slip hazard, must mop |
| Ambulance arrival | Emergency patient (priority!) |
| Equipment malfunction | Station offline for 10s |
| Power flicker | All stations pause 3s |

### Phase 4: Night Phase & Upgrades
Between days, choose upgrades (roguelite progression).

**Card Selection (pick 1 of 3):**
- Auto-sanitizer: Stations clean themselves
- Speed gurney: Move 20% faster while carrying
- Patient TV: +10s patience for waiting patients
- Extra shelf: More supply storage
- Nurse station: AI helper does simple tasks

**Equipment Shop:**
- Second X-Ray machine
- Surgery room
- Recovery ward
- Conveyor belts (auto-move supplies)
- Grabbers (auto-pickup from conveyors)

### Phase 5: Meta Progression
Unlocks that persist across runs.

**Unlock Ideas:**
- New starting layouts
- Cosmetic hospital themes
- Challenge modifiers (no upgrades, speed run, etc.)
- Leaderboards (daily/weekly seeds)

### Phase 6: Content Expansion
- New biomes (Pediatric ward, Psychiatric, Maternity)
- Boss patients (VIP with special requirements)
- Seasonal events

---

## Technical Decisions

### Architecture
- **Autoloads**: GameEvents, NetworkManager, DayManager, SpawnManager, ScoreManager
- **Entities**: Player, Patient, Station (base class)
- **Server Authoritative**: All game state on host, clients observe

### Multiplayer Model
- Host runs game logic
- Clients send input requests via RPC
- State synced via MultiplayerSynchronizer
- Drop-in coop (join mid-game)

### Patterns
- **Ensure**: Fail-fast validation, no silent defensive code
- **GameEvents**: Global signal bus for decoupled systems
- **IPickupable/IDropTarget**: Interfaces for interaction system

---

## Open Questions (Future Discussion)

1. **Roguelite vs Endless**: Fixed run length with win condition, or pure endless high-score?
2. **Unlock progression**: What persists between runs? How to avoid grind?
3. **Difficulty modes**: Easy/Normal/Hard presets, or dynamic difficulty?
4. **Content pipeline**: How to easily add new conditions/stations/events?
5. **Mod support**: Should we design for modding from the start?
6. **Mobile port**: Touch controls feasible for this genre?

---

## Art Direction (Prototype)

Using programmatic drawing (`_Draw()`) + SVG icons from `assets/game-icons/`.

**Entities:**
- Player: Colored circle with direction indicator
- Patient: Circle with condition icon (😷, 🤕, 🤢)
- Station: Rectangle with type icon

**Visual Feedback:**
- Processing: Progress bar on station
- Impatient: Patient turns red gradually
- Held item: Follows player with slight offset

---

## References

- [PlateUp!](https://store.steampowered.com/app/1599600/PlateUp/) - Primary inspiration
- [Overcooked](https://store.steampowered.com/app/448510/Overcooked/) - Coop chaos cooking
- [Theme Hospital](https://en.wikipedia.org/wiki/Theme_Hospital) - Classic hospital management
