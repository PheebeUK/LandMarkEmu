# Client 0.695.1.132647 — gap catalogue

**Goal:** Login + zone-in working on **0.695.1.132647** while keeping **0.589** support gated.

| Build | Role |
|-------|------|
| `0.589.1.128896` (Steam / validated) | Current production path (`Uses589Opcodes` → true) |
| `0.695.1.132647` | Dual-client path (`Use589Layout` → false) — **login + zone-in verified 2026-10-12** |

**Install (this machine):** `D:\Landmark\Game` — window title / gateway string `0.695.1.132647`.  
`ClientConfig.ini` → `Server=192.168.250.15:20042` (matches `.env` `ZONE_HOST`).  
`gameData/` rebuilt via `ops/dev.ps1 -Action prep -GamePath 'D:\Landmark\Game'`.

Zone **opcodes are the same** between builds. Divergence is mostly **payload layouts**, plus packets only newer clients understand.

---

## Existing gates (inventory)

| File | Behavior |
|------|----------|
| `LandmarkEmulator.Gateway/Network/GatewaySession.cs` | Stores `ClientBuild` from `LoginRequest` |
| `LandmarkEmulator.Gateway/Network/Message/Model/LoginRequest.cs` | Reads `ClientBuild` string |
| `LandmarkEmulator.WorldServer/Network/Message/ZoneOpcodeTranslator.cs` | `Uses589Opcodes(build)` ↔ `build.StartsWith("0.589.")` |
| `LandmarkEmulator.WorldServer/Network/WorldSession.cs` | `SendSelf.Use589Layout = Uses589Opcodes(ClientBuild)` |
| `LandmarkEmulator.WorldServer/Network/Message/Model/SendSelfToClient.cs` | Default `Write` = **0.695** tail; `WriteTail589` when `Use589Layout` |
| `LandmarkEmulator.WorldServer/Network/Handlers/MiscHandler.cs` | Sends `Resources/PacketDumps/145-PropBase` only when **not** 0.589 |
| `LandmarkEmulator.WorldServer/HostedService.cs` / `Game/Props.cs` | Always loads 145-PropBase for server-side prop model ids |

---

## Seeded known diffs

| Area | Status | Notes / refs |
|------|--------|----------------|
| SendSelf tail (after shared mid / ~Unknown62+) | **done (smoke)** | Default `Write` path accepted by 0.695: `ClientIsReady` ~4.8s after SendSelf; client log “received our character: Pheebe”; no `0xBADBEEF`. Gates untouched. |
| Inventory / mail item trailing byte | **partial** | `InventoryItem.Write` includes `_ExtraData` (0.695); `WriteRecord` omits it for 0.589-shaped callers. SendSelf uses `Write` — OK for this login. Mail/trade/chest paths still need dual-client vigilance. |
| PropBase sub `0x0A` definition list | **done (gated)** | Dump sent on this login (non-589). Many `AddLightweightProp` after ready. |
| TerrainCel2 / cell packing | **open / ok at zone-in** | Client reached `cClientRunStateRunning` with contour updates; deeper mining/building not retested on 0.695. |
| Opcodes | **done** | Same zone opcode set. Protocol string seen: `LandmarkClientProtocol_709`. |

---

## Checklist

| Step | Status | Evidence |
|------|--------|----------|
| Login (auth/gateway, `ClientBuild` = `0.695…`) | **done** | World log: `1, <ticket>, LandmarkClientProtocol_709, 0.695.1.132647` |
| SendSelf (`Use589Layout == false`, no BADBEEF) | **done** | `Sending packet SendSelfToClient`; `WIRE-TIMING ClientIsReady … sinceSendSelfMs=4769` |
| Zone-in (details → ready → world spawn) | **failing (UI)** | **Do not treat `Running` / `ClientFinishedLoading` / `ModalBar` as in-world.** Retests that looked “green” still had loading bar ~50% and **no** `HideLoadingscreen` after `LoadingIntoWorld`. Pass only when loading screen actually clears (or player can move). |
| Movement | todo | Not exercised this pass |
| Inventory | todo | Login inventory accepted; UI not audited |
| Props | partial | Prop spawns after ready; no placement test |
| Terrain | partial | Contours after ready; severe hitching during first minute |

---

## Session log

| When | Milestone | Result |
|------|-----------|--------|
| 2026-10-12 | M0 | Hooks grepped; this file created; install confirmed. |
| 2026-10-12 | M1 | `LANDMARK_DIR=D:\Landmark\Game`; prep OK; auth was down (exit 137) — restarted. |
| 2026-10-12 | M2 | First zone attempts failed: CharacterLoginReply pointed at **stale** `192.168.1.50:19000` (Auth caches `zone_server` at boot). After host `192.168.250.15` + auth restart → gateway connect OK. |
| 2026-10-12 | M3 | **No SendSelf code change required.** Existing non-589 `Write` path consumed cleanly. |
| 2026-10-12 | M4 | Protocol in-world; **user-visible zone-in incomplete** (stuck/hitchy loading). |
| 2026-10-12 | M5 | STATUS + this file; PropBase deferred (below). |

### Zone-in incomplete (user report, 2026-10-12)

**Symptom:** After Play, loading never feels finished (freeze / quit), even though gateway logs show SendSelf + `ClientIsReady` + `Running`.

**Evidence:**
- Pioneer start on **417** (`Pioneer.Settle`): NetInfo `OpenBetaGeometry_417`; Ui stayed on `LoadingScreen.gfx`; no `ModalBarAbilities`; client quit with `ClientLogout` after ~44s and 10–23s slow frames.
- Retest on **406**: HUD did appear (`ModalBarAbilities` ~2s after `Running`) but then 8–56s `SlowFrameMs` and SOE `outRtoResend` in the thousands — feels hung.
- Note: `LoadingScreen.txt` often never logs `Hide` even on known-good 0.589 sessions; use Ui `ModalBarAbilities.gfx` / playability, not Hide lines.

**Likely cause (hitching):** 0.695-only **~354 KB PropBase `145-PropBase`** in the `ClientIsReady` burst overlaps terrain stream → SOE congestion (`outRtoResend` 10k+, multi‑10s `SlowFrameMs`).

**Bad experiment:** deferring PropBase to `ClientFinishedLoading` made the loading bar sit ~**50%** for ~50s (ModalBar only after that). Protocol still reached `Running`, but UI felt stuck. **Reverted** — PropBase stays on `ClientIsReady` (gated `!Uses589Opcodes`).

### How “in world” was wrongly judged

Used: server `ClientFinishedLoading` / `entered the world`, client `cClientRunStateRunning`, optional `ModalBarAbilities`.

**Insufficient:** session `1791834269` (PropBase-before-manifest retest) hit `WaitForWorldReady: waiting complete` + `Running` at 20:44:57, then `SlowFrameMs=22695`, and **never** logged `HideLoadingscreen` after `LoadingIntoWorld` / `WaitForWorldReady`. Loading bar at ~50% is `GameData.LoadingProgress` (client-local); protocol can advance while the screen stays up.

**Pass signal:** `HideLoadingscreen` after world load, or clear visual clear of the bar / movement — not `Running` alone.

### Official capture order (`No_ID_Full_Session.pcapng`, parsed 2026-10-12)

After first `ClientIsReady` (+0):

| t vs Ready | S→C | Size |
|------------|-----|------|
| +958 ms | AchievementBase | ~405 KB |
| **+1.252 s** | **PropBase** | **353732 B** (same size as our `145-PropBase`) |
| +1.252 s | ZoneDoneSendingInitialData | 0 |
| +1.253 s | SendZoneDetails | 396 B |
| **+2.632 s** | **TerrainCellModifiedCellManifest** | **~1.5 MB** |
| ~+2.9 s | first TerrainCellClientCellRequest | |
| (after manifest) | ClientFinishedLoading | |

Also: capture sends `ArtDataTintDefinitionManagerData` **~736 KB before SendSelf**; we only send 8 empty u32 lists at ready.

**Alignment (2026-10-13):** match `No_ID_Full_Session` pacing/order where we have dumps:

| Capture | Our change |
|---------|------------|
| ArtDataTint ~736 KB before SendSelf | send `ArtDataTintDefinitionManagerData.bin` (extracted) pre-SendSelf on non-589 |
| PropBase 354 KB ×2 around ZoneDone + tint 3066 | send definition list twice, then `SendTintList` |
| Inventory after ZoneDone | moved |
| Activity early + late | stub sent both sides of the deferred manifest |
| Manifest ~1.1 s after 2nd PropBase | `Task.Delay(1100)` then `SendManifest` (still **our** edited-cell manifest, not the live 1.5 MB dump) |

**Pass signal unchanged:** `HideLoadingscreen` after world load / playable UI — not `Running` alone.

**Follow-ups:** Claims/Welcome/CoinStore filling; whether live 1.5 MB manifest dump is safe; automation on HideLoading.

### Ops gotcha (blocker that looked like a layout bug)

Auth loads `zone_server` **once** at startup (`ZoneServerManager.Initialise`). Updating the DB while Auth is running does not change `CharacterLoginReply.ServerAddress`. Symptom: auth login works, then `ExternalGatewayApi` → `DisconnectReasonConnectFail` to an old LAN IP. Fix: set `ZONE_HOST`, `UPDATE zone_server`, **restart `lmemu-auth`**.

### Minor leftovers (not blocking zone-in)

1. Auth `LoginRequest`: “13 bytes remaining” (`73-00-00-00` + zeros) — login still succeeds.
2. World: `Failed to read entire contents of Zone packet PlayerUpdateSetPlayerBio (6 bytes remaining)` — C→S reader short; investigate later.
3. Private `re/tools` / Frida harness / Ghidra instance not required for this smoke; Ghidra project exists at `D:\Landmark\ghidra\Landmark.gpr` but was not loaded.
4. No 0.589 binary on this machine for live dual-path regression; keep gated writers.

---

## File-level changes this pass

| Path | Change |
|------|--------|
| `research/notes/CLIENT-695.md` | Created / updated (this file) |
| `research/notes/STATUS.md` | 0.695 marked in progress / smoke-verified |
| `.env` `LANDMARK_DIR` | Already `d:\landmark\game` (prep confirmed) |
| `MiscHandler.cs` | PropBase on `ClientIsReady`, ordered **before** `ZoneDone` (matches `No_ID_Full_Session` capture) |
| `research/captures/_parsed/No_ID_Full_Session_timeline/` | Parsed official zone-entry timeline |
| Character `Pheebe` | Moved to zone **406** `(0,1200,0)` for retest (was Pioneer 417) |
