# Known bugs

Living list of bugs found while running or developing the emulator. One entry per problem. When fixed, move the entry
to **Fixed** with the date and a one-line how. Prefer a GitHub issue for anything that needs discussion or a patch from
someone else ([wiki/Bug-Submission.md](../wiki/Bug-Submission.md)); keep short working notes here so they are not lost
between sessions.

## Open

### 2026-10-19 — World NPC greet bubble had no Yes / No (reopened)

- **Where:** Fungus Scout interact (`Game/Npc/WorldNpcs.cs` + DisplayPropText).
- **What:** Speech bubble / chat text appeared; Yes/No (HudIndicators NpcDialogResponse) did not.
- **Cause (revised):** (1) Playtest after the first fix hit **Spire Herbalist** while chain-locked (`q=0`, no buttons by design) — both were Kobolds near the Spire. (2) Server `FullNpcs` is set when `SendFull` is *queued*, so question packets spoke as the creature before the client had a full actor; bubble can show, question HUD cannot (L30).
- **Status:** fix in progress — questions speak as the interactor; Herbalist moved / distinct model / locked line is chat-only; `npc talk <id>` to force dialogue. Needs rebuild + retest on **Fungus Scout** (`npc reset`, `npc goto fungus_scout`, interact or `npc talk fungus_scout`; log must show `NPC fungus_scout … q=` ≠ 0).

### 2026-10-19 — Build site management info section (claims themselves work)

- **Where:** Build site management UI — the info section (claims / site claim flow otherwise works).
- **What:** Something looks wrong or incomplete in that info section; earlier it looked like claims were broken, but claiming works.
- **Status:** open — do not fix until asked; need a clearer repro of what the info section shows vs expected.

## Fixed

### 2026-10-19 — Spires cut into hillsides / floating on steep ground

- **Where:** `Game/Spire.cs` placement on sloped landscapes (home at (0,0) especially).
- **What:** First carve+fill left hillside cliffs; then merge seating on steep spots left the downhill side hanging
  in mid-air.
- **Cause:** Air-box stamp + fill; later, home locked at (0, 0) / `MaxSpread` 8 m with deck seated to high ground.
- **Fix:** Merge seating (no air-box/fill); spot search for all landscapes including home — deck relief ≤ 17 m
  (~plinth height; box ≤ 22 m); steep `merge` markers re-pick on start-up (`.steep-reverted`).

### 2026-10-19 — Auth container won't start: host TCP 5000 forbidden

- **Where:** `docker compose up` → `landmarkemu-auth-1` port publish `${ACCOUNT_PORT}:5000`.
- **What:** `ports are not available: ... 0.0.0.0:5000 ... bind: An attempt was made to access a socket in a way forbidden by its access permissions.`
- **Cause:** Nothing was listening on 5000; Windows Hyper-V/NAT had excluded TCP 4923–5022, which includes 5000.
- **Fix:** Set `ACCOUNT_PORT=5050` in `.env` (host only; container still uses 5000). Noted the Windows excluded-port check in `.env.example`.

### 2026-10-19 — `docker compose up -d --build` fails (NETSDK1004 CaptureImport)

- **Where:** `docker/Dockerfile` build stage → `dotnet publish` WorldServer.
- **What:** Build exited 1 with `NETSDK1004: Assets file '.../LandmarkEmulator.CaptureImport/obj/project.assets.json' not found`.
- **Cause:** WorldServer ProjectReferences `LandmarkEmulator.CaptureImport`, but the Dockerfile’s layered restore did not COPY that `.csproj` before `dotnet restore`.
- **Fix:** COPY `LandmarkEmulator.CaptureImport.csproj` into the build stage before restore. Verified with `docker compose build auth world`.

### 2026-10-18 — Admin panel: Find (item by name) locks the page up

- **Where:** `/admin` → *Give item* row → *Find (server log)* (`Admin.cs` button → `finditem` in `DevCommands.cs`).
- **What:** Clicking Find made the admin page (and often the game) stop responding for a long time.
- **Cause:** `finditem` logged every match on the world tick. An empty needle matched every name (`Contains("")`);
  short needles matched hundreds. Thousands of log lines stalled the world thread; the in-game browser froze with it.
- **Fix:** refuse an empty needle; log at most 40 matches; reply with the match count (chat + log). The admin button
  also refuses a blank field before queueing.
