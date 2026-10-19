# Official capture zone-entry timeline

Reference for comparing emulator S→C grouping/order and approximate timings.

## Source

| Field | Value |
|-------|-------|
| Capture | `No_ID_Full_Session.pcapng` |
| Parsed | `2026-10-12 19:55Z` |
| Window | first `SendSelfToClient` through +90s |
| t=0 | first `SendSelfToClient` (S→C) |
| Also listed | `rel_ready` = ms vs first `ClientIsReady` |
| Zone msgs in window | 3724 |
| Total zone msgs in capture | 19606 |

**Timing note:** timestamps are pcap/pcapng packet times of the UDP frame that *completed* SOE reassembly for that zone message (last fragment). Payload length is zone body after opcode bytes.

**Grouping note:** high-volume opcodes (featherweights, B00, cell request/reply spam, ClientLog) are collapsed into burst rows. Bootstrap catalogs and gate packets are listed individually.

**Unknown vs UnknownNoise:** `Unknown` = family-shaped prefix not in `ZoneMessageOpcode` (likely catalog gap). `UnknownNoise` = fails Landmark family-shape heuristic (`& 0xFF00FF == 0` / `0x26|0x27|0x29|0xEC`) — treat as ARC4/demux framing garbage until the parser stream is proven green.

## Anchors

| Event | Dir | Opcode | Payload | t vs SendSelf | t vs ClientIsReady |
|-------|-----|--------|---------|---------------|--------------------|
| SendZoneDetails (first) | S | `0x00002E00` | 396 B | +6.103 s | +1.253 s |
| CommandBase (first) | S | `0x00001E00` | 23 B | +5.808 s | +958 ms |
| SendSelfToClient (first) | S | `0x00000F00` | 747709 B | +0.0 ms | -4.850 s |
| ClientIsReady (first) | C | `0x00001000` | 0 B | +4.850 s | +0.0 ms |
| AchievementBase (first) | S | `0x00009200` | 404712 B | +5.808 s | +958 ms |
| PropBase (first) | S | `0x0000ED00` | 353732 B | +6.102 s | +1.252 s |
| ZoneDoneSendingInitialData (first) | S | `0x00001100` | 0 B | +6.102 s | +1.252 s |
| TerrainCellModifiedCellManifest (first) | S | `0xEC000A00` | 1502216 B | +7.482 s | +2.632 s |
| TerrainCellClientCellRequest (first) | C | `0xEC000100` | 206 B | +7.705 s | +2.855 s |
| TerrainCellCellsUpdate (first) | S | `0xEC000500` | 39620 B | +8.268 s | +3.418 s |
| ClaimsBase (first) | S | `0x0000F900` | 513 B | +6.398 s | +1.548 s |
| TemplateBase (first) | S | `0x0000F000` | 22199 B | +8.229 s | +3.379 s |
| PlayerUpdateAddFeatherweightObject (first) | S | `0x26007400` | 90 B | +8.394 s | +3.544 s |

## Phases (approximate)

| Phase | First t(SendSelf) | Last t(SendSelf) | Msg count | Dominant opcodes |
|-------|-------------------|------------------|-----------|------------------|
| A. Pre-SendSelf catalogs | — | — | 0 | — |
| B. SendSelf → ClientIsReady | +0.0 ms | +3.601 s | 12 | ClientLog×5, GameTimeSync×2, SendSelfToClient×1, ClientPcDataBase×1 |
| C. ClientIsReady → ZoneDone / early catalogs | +4.850 s | +6.811 s | 106 | CommandBase×43, InGamePurchaseBase×7, ClaimsBase×6, ActivityBase×4 |
| D. Terrain cold start (A00 / first 100 / first 500) | +7.482 s | +8.274 s | 5 | TerrainCellClientCellRequest×2, TerrainCellCellsUpdate×2, TerrainCellModifiedCellManifest×1 |
| E. World stream (featherweight / lightweight) | +8.378 s | +24.590 s | 2164 | PlayerUpdateAddFeatherweightObject×1854, PlayerUpdateAddLightweightPropWire×218, PlayerUpdateLightweightToFullPropWire×84, PlayerUpdateAddLightweightNpcWire×4 |
| F. Remainder of window | +15.012 s | +89.946 s | 841 | UnknownNoise×617, KeepAlive×74, PlayerUpdateFullCharacterDataRequestWire×42, Synchronization×34 |

## Timeline (hybrid)

| t(SendSelf) | t(Ready) | Dir | Opcode | Name | Payload | Notes |
|-------------|----------|-----|--------|------|---------|-------|
| +0.0 ms | -4.850 s | S | `0x00000F00` | SendSelfToClient | 747709 B | |
| +943 ms | -3.907 s | C | `0x0000DD00` | ClientPcDataBase | 17 B | |
| +1.767 s | -3.083 s | C | `0x00006F00` | ClientLog | 35–491 B ×3 (sum 672 B) | burst +1.767 s → +1.767 s |
| +1.778 s | -3.072 s | C | `0x00005B00` | SetLocale | 9 B | |
| +1.778 s | -3.072 s | C | `0x0000A200` | ClientInitializationDetails | 4 B | |
| +3.266 s | -1.584 s | C | `0x00009800` | FotomatBase | 22 B | |
| +3.335 s | -1.515 s | C | `0x00006F00` | ClientLog | 83–98 B ×2 (sum 181 B) | burst +3.335 s → +3.335 s |
| +3.442 s | -1.408 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +3.601 s | -1.249 s | S | `0x00003700` | GameTimeSync | 13 B | |
| +4.850 s | +0.0 ms | C | `0x00001000` | ClientIsReady | 0 B | |
| +4.850 s | +0.0 ms | C | `0x26007100` | PlayerUpdateBindingCustomization | 6 B | |
| +5.038 s | +188 ms | S | `0x00004600` | QuickChatBase | 6 B | |
| +5.038 s | +188 ms | S | `0x29001B00` | ClientUpdateDoneSendingPreloadCharacters | 1 B | |
| +5.808 s | +958 ms | S | `0x00009200` | AchievementBase | 404712 B | |
| +5.808 s | +958 ms | S | `0x00009200` | AchievementBase | 25 B | |
| +5.808 s | +958 ms | S | `0x00008F00` | ActivityManagerBase | 5 B | |
| +5.808 s | +958 ms | S | `0x0026001F` | PlayerUpdateEffectTagCompositeEffectsEnable | 4 B | |
| +5.808 s | +958 ms | S | `0x29002E00` | Unknown | 120 B ×1 |  |
| +5.808 s | +958 ms | S | `0x0000C400` | ResourcesBase | 105 B | |
| +5.808 s | +958 ms | S | `0x29002C00` | ClientUpdateUpdateCalculatedStats | 388 B | |
| +5.808 s | +958 ms | S | `0x29000600` | ClientUpdateUpdateStat | 485 B | |
| +5.808 s | +958 ms | S | `0x26005100` | PlayerUpdateUpdateCharacterStateDelta | 38 B | |
| +5.808 s | +958 ms | S | `0x00003700` | GameTimeSync | 13 B | |
| +5.808 s | +958 ms | S | `0x0000C400` | ResourcesBase | 346 B | |
| +5.808 s | +958 ms | S | `0x0000BD00` | LoadoutsBase | 220 B | |
| +5.808 s | +958 ms | S | `0x00001E00` | CommandBase | 23 B | |
| +5.899 s | +1.049 s | C | `0x00004200` | RecipeBase | 4 B | |
| +6.102 s | +1.252 s | S | `0x0000ED00` | PropBase | 353732 B | |
| +6.102 s | +1.252 s | S | `0x26000F00` | PlayerUpdateUpdateCharacterState | 22 B | |
| +6.102 s | +1.252 s | S | `0x00004100` | UpdateEncounterDataCommon | 43 B | |
| +6.102 s | +1.252 s | S | `0x00001100` | ZoneDoneSendingInitialData | 0 B | |
| +6.102 s | +1.252 s | S | `0x0000D700` | MapRegionBase | 17 B | |
| +6.102 s | +1.252 s | S | `0x00002D00` | InventoryBase | 3 B | |
| +6.103 s | +1.253 s | S | `0x00004200` | RecipeBase | 5420 B | |
| +6.103 s | +1.253 s | S | `0x00002E00` | SendZoneDetails | 396 B | |
| +6.103 s | +1.253 s | S | `0x29001F00` | ClientUpdateZonePopulation | 16 B | |
| +6.103 s | +1.253 s | S | `0x0000A000` | ActivityBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x0000A000` | ActivityBase | 110 B | |
| +6.103 s | +1.253 s | S | `0x00009E00` | CoinStoreBase | 24 B | |
| +6.103 s | +1.253 s | S | `0x00006000` | LoadWelcomeScreen | 17 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 18 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 12 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 8 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 13 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 11 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 12 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 13 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 10 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 12 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 13 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 9 B | |
| +6.103 s | +1.253 s | S | `0x00001E00` | CommandBase | 14 B | |
| +6.103 s | +1.253 s | S | `0x0000D700` | MapRegionBase | 21 B | |
| +6.103 s | +1.253 s | S | `0x0000CF00` | ThrustPadBase | 5 B | |
| +6.103 s | +1.253 s | S | `0x0000BE00` | ExperienceBase | 81 B | |
| +6.396 s | +1.545 s | S | `0x0000ED00` | PropBase | 353732 B | |
| +6.396 s | +1.546 s | S | `0x0000ED00` | PropBase | 3066 B | |
| +6.396 s | +1.546 s | S | `0x00001200` | ZoneCoordinateInfo | 80 B | |
| +6.396 s | +1.546 s | S | `0x0000CB00` | EquipmentBase | 241 B | |
| +6.396 s | +1.546 s | S | `0x18012000` | UnknownNoise | 9 B ×1 |  |
| +6.398 s | +1.548 s | S | `0x0000F900` | ClaimsBase | 513 B | |
| +6.398 s | +1.548 s | S | `0x29002600` | ClientUpdateMovementVersion | 1 B | |
| +6.398 s | +1.548 s | S | `0x00002D00` | InventoryBase | 10 B | |
| +6.398 s | +1.548 s | S | `0x00002D00` | InventoryBase | 10 B | |
| +6.398 s | +1.548 s | S | `0x0000F900` | ClaimsBase | 53 B | |
| +6.398 s | +1.548 s | S | `0x00005900` | WorldDisplayInfo | 4 B | |
| +6.398 s | +1.548 s | S | `0x00001E00` | CommandBase | 17 B | |
| +6.398 s | +1.548 s | S | `0x0000D100` | MissionsBase | 13 B | |
| +6.398 s | +1.548 s | S | `0x0000B900` | VoiceBase | 76 B | |
| +6.398 s | +1.548 s | S | `0x00004500` | InGamePurchaseBase | 29 B | |
| +6.398 s | +1.548 s | S | `0x29002B00` | ClientUpdateLoyaltyPoints | 4 B | |
| +6.398 s | +1.548 s | S | `0x0000B900` | VoiceBase | 62 B | |
| +6.398 s | +1.548 s | S | `0x00008100` | HousingBase | 6 B | |
| +6.398 s | +1.548 s | S | `0x0000E900` | ClientSettings | 110 B | |
| +6.398 s | +1.548 s | S | `0x0000A000` | ActivityBase | 10 B | |
| +6.398 s | +1.548 s | S | `0x0000A000` | ActivityBase | 110 B | |
| +6.398 s | +1.548 s | S | `0x00004500` | InGamePurchaseBase | 3 B | |
| +6.398 s | +1.548 s | S | `0x00004500` | InGamePurchaseBase | 4 B | |
| +6.398 s | +1.548 s | S | `0x00004500` | InGamePurchaseBase | 14 B | |
| +6.398 s | +1.548 s | S | `0x00004500` | InGamePurchaseBase | 6 B | |
| +6.399 s | +1.548 s | S | `0x00004500` | InGamePurchaseBase | 1652 B | |
| +6.399 s | +1.549 s | S | `0x00004500` | InGamePurchaseBase | 6738 B | |
| +6.399 s | +1.549 s | S | `0x0000F900` | ClaimsBase | 53 B | |
| +6.811 s | +1.961 s | C | `0x0000F900` | ClaimsBase | 9 B | |
| +6.811 s | +1.961 s | C | `0x0000F900` | ClaimsBase | 9 B | |
| +6.811 s | +1.961 s | C | `0x0000F900` | ClaimsBase | 9 B | |
| +7.482 s | +2.632 s | S | `0xEC000A00` | TerrainCellModifiedCellManifest | 1502216 B | |
| +7.482 s | +2.632 s | S | `0xEA000100` | AreaDefinitionSetDefinitions | 15 B | |
| +7.629 s | +2.779 s | C | `0x0000A700` | RatingBase | 9 B | |
| +7.629 s | +2.779 s | C | `0x00004500` | InGamePurchaseBase | 2 B | |
| +7.637 s | +2.787 s | C | `0x00000D00` | ClientFinishedLoading | 0 B | |
| +7.649 s | +2.799 s | C | `0x00001301` | WarpNetworkWire | 32 B | |
| +7.649 s | +2.799 s | C | `0x00533333` | UnknownNoise | 35 B ×1 |  |
| +7.649 s | +2.799 s | C | `0x27001300` | AbilityDefaultInfoRequest | 0 B | |
| +7.649 s | +2.799 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0xEC000100` | TerrainCellClientCellRequest | 206 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.705 s | +2.855 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.716 s | +2.866 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +7.744 s | +2.894 s | C | `0x00004500` | InGamePurchaseBase | 11 B | |
| +7.744 s | +2.894 s | C | `0x00004500` | InGamePurchaseBase | 11 B | |
| +7.744 s | +2.894 s | C | `0x00004500` | InGamePurchaseBase | 2 B | |
| +7.744 s | +2.894 s | C | `0x00004500` | InGamePurchaseBase | 2 B | |
| +7.744 s | +2.894 s | C | `0x26008000` | PlayerUpdateSetPlayerBio | 6 B | |
| +7.744 s | +2.894 s | C | `0x00006300` | KeepAlive | 4 B | |
| +7.744 s | +2.894 s | C | `0x0000C300` | Synchronization | 49 B | |
| +7.793 s | +2.943 s | C | `0x00E23333` | UnknownNoise | 35 B ×1 |  |
| +7.822 s | +2.972 s | C | `0xEC000100` | TerrainCellClientCellRequest | 62 B ×1 |  |
| +7.822 s | +2.972 s | C | `0x01083433` | UnknownNoise | 4 B ×1 |  |
| +7.965 s | +3.115 s | C | `0x00006F00` | ClientLog | 56–61 B ×2 (sum 117 B) | burst +7.965 s → +7.965 s |
| +8.100 s | +3.250 s | S | `0x00004500` | InGamePurchaseBase | 833179 B | |
| +8.107 s | +3.257 s | S | `0x00004500` | InGamePurchaseBase | 30 B | |
| +8.107 s | +3.257 s | S | `0x00000300` | PreferencePacket | 150 B | |
| +8.107 s | +3.257 s | S | `0x0000D100` | MissionsBase | 13 B | |
| +8.108 s | +3.257 s | S | `0x00004D00` | FriendBase | 558 B | |
| +8.108 s | +3.257 s | S | `0x00007200` | IgnoreBase | 5 B | |
| +8.108 s | +3.257 s | S | `0x0000B900` | VoiceBase | 69 B | |
| +8.108 s | +3.257 s | S | `0x0000B900` | VoiceBase | 69 B | |
| +8.119 s | +3.269 s | C | `0x00006F00` | ClientLog | 53–68 B ×5 (sum 290 B) | burst +8.119 s → +8.119 s |
| +8.217 s | +3.367 s | S | `0x00008300` | GuildBase | 110669 B | |
| +8.218 s | +3.368 s | S | `0x00008300` | GuildBase | 10 B | |
| +8.218 s | +3.368 s | S | `0x00008300` | GuildBase | 150 B | |
| +8.218 s | +3.368 s | S | `0x00001300` | ChatBase | 59 B | |
| +8.229 s | +3.379 s | S | `0x0000F000` | TemplateBase | 22199 B | |
| +8.233 s | +3.383 s | S | `0x00004200` | RecipeBase | 25088 B | |
| +8.233 s | +3.383 s | S | `0x0000F900` | ClaimsBase | 2641 B | |
| +8.233 s | +3.383 s | S | `0x0000F900` | ClaimsBase | 209 B | |
| +8.233 s | +3.383 s | S | `0x0000F900` | ClaimsBase | 191 B | |
| +8.233 s | +3.383 s | S | `0x0000F900` | ClaimsBase | 191 B | |
| +8.234 s | +3.384 s | S | `0x0000F900` | ClaimsBase | 1164 B | |
| +8.234 s | +3.384 s | S | `0x0000F900` | ClaimsBase | 191 B | |
| +8.234 s | +3.384 s | S | `0x0000F900` | ClaimsBase | 191 B | |
| +8.234 s | +3.384 s | S | `0x0000F900` | ClaimsBase | 191 B | |
| +8.234 s | +3.384 s | S | `0x0000F900` | ClaimsBase | 191 B | |
| +8.234 s | +3.384 s | S | `0x27000400` | AbilityAbilityStart | 51 B | |
| +8.234 s | +3.384 s | S | `0x0000C400` | ResourcesBase | 346 B | |
| +8.234 s | +3.384 s | S | `0x0000C400` | ResourcesBase | 346 B | |
| +8.234 s | +3.384 s | S | `0x0000C400` | ResourcesBase | 346 B | |
| +8.234 s | +3.384 s | S | `0x27000700` | AbilityCastSuccess | 44 B | |
| +8.234 s | +3.384 s | S | `0x29002C00` | ClientUpdateUpdateCalculatedStats | 20 B | |
| +8.234 s | +3.384 s | S | `0x26005100` | PlayerUpdateUpdateCharacterStateDelta | 38 B | |
| +8.250 s | +3.400 s | S | `0x00001301` | WarpNetworkWire | 80 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27001400` | AbilityDefaultInfoReply | 4 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.250 s | +3.400 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 218 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 224 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.251 s | +3.401 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.252 s | +3.402 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x00004500` | InGamePurchaseBase | 6017 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.253 s | +3.403 s | S | `0x27000300` | AbilityCasterInfoReply | 394 B | |
| +8.253 s | +3.403 s | S | `0x0000A700` | RatingBase | 13 B | |
| +8.268 s | +3.418 s | S | `0xEC000500` | TerrainCellCellsUpdate | 39620 B | |
| +8.268 s | +3.418 s | S | `0x26008000` | PlayerUpdateSetPlayerBio | 6 B | |
| +8.268 s | +3.418 s | S | `0x0000C300` | Synchronization | 49 B | |
| +8.268 s | +3.418 s | S | `0x00004500` | InGamePurchaseBase | 1066 B | |
| +8.274 s | +3.424 s | S | `0x00004500` | InGamePurchaseBase | 7155 B | |
| +8.274 s | +3.424 s | S | `0xEC000500` | TerrainCellCellsUpdate | 200 B ×1 |  |
| +8.274 s | +3.424 s | S | `0x00002D00` | InventoryBase | 10 B | |
| +8.274 s | +3.424 s | S | `0x00002D00` | InventoryBase | 10 B | |
| +8.274 s | +3.424 s | S | `0x00004500` | InGamePurchaseBase | 6 B | |
| +8.274 s | +3.424 s | S | `0x00004500` | InGamePurchaseBase | 42 B | |
| +8.274 s | +3.424 s | S | `0x00004500` | InGamePurchaseBase | 29 B | |
| +8.274 s | +3.424 s | S | `0x00002D00` | InventoryBase | 10 B | |
| +8.274 s | +3.424 s | S | `0x00002D00` | InventoryBase | 10 B | |
| +8.274 s | +3.424 s | S | `0x0000FE00` | PlayerStudioRegistered | 1 B | |
| +8.378 s | +3.528 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.378 s | +3.528 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 421 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 419 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 419 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 419 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.379 s | +3.529 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 407 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 407 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 455 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 449 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 407 B | |
| +8.380 s | +3.530 s | S | `0x00260059` | PlayerUpdateAddLightweightNpcWire | 271 B | |
| +8.380 s | +3.530 s | S | `0x26005C00` | PlayerUpdateLightweightToFullNpc | 367 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 449 B | |
| +8.380 s | +3.530 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 467 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.381 s | +3.531 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 435 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 467 B | |
| +8.382 s | +3.532 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.383 s | +3.533 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.384 s | +3.534 s | S | `0x00260059` | PlayerUpdateAddLightweightNpcWire | 266 B | |
| +8.384 s | +3.534 s | S | `0x26005C00` | PlayerUpdateLightweightToFullNpc | 1212 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 418 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 435 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 407 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.384 s | +3.534 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 419 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 419 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 441 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.385 s | +3.535 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.386 s | +3.536 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.387 s | +3.537 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.388 s | +3.538 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.389 s | +3.539 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 448 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.390 s | +3.540 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 468 B | |
| +8.390 s | +3.540 s | S | `0x00260059` | PlayerUpdateAddLightweightNpcWire | 283 B | |
| +8.390 s | +3.540 s | S | `0x26005C00` | PlayerUpdateLightweightToFullNpc | 367 B | |
| +8.390 s | +3.540 s | S | `0x00260059` | PlayerUpdateAddLightweightNpcWire | 283 B | |
| +8.391 s | +3.541 s | S | `0x26005C00` | PlayerUpdateLightweightToFullNpc | 367 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 470 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 592 B | |
| +8.391 s | +3.541 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +8.391 s | +3.541 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 640 B | |
| +8.392 s | +3.542 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 592 B | |
| +8.392 s | +3.542 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 408 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 407 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 408 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 407 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 408 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 401 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.393 s | +3.543 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 455 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400 B | |
| +8.394 s | +3.544 s | S | `0x26007400` | PlayerUpdateAddFeatherweightObject | 90 B | |
| +8.394 s | +3.544 s | S | `0x26007400` | PlayerUpdateAddFeatherweightObject | 90 B ×844 | burst +8.394 s → +8.423 s |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.457 s | +3.607 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.528 s | +3.678 s | S | `0x26007400` | PlayerUpdateAddFeatherweightObject | 90 B ×120 | burst +8.528 s → +8.529 s |
| +8.530 s | +3.679 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 593 B | |
| +8.530 s | +3.679 s | S | `0x00260062` | PlayerUpdateAddLightweightPropWire | 593 B | |
| +8.530 s | +3.679 s | S | `0x26007400` | PlayerUpdateAddFeatherweightObject | 90 B ×872 | burst +8.530 s → +8.560 s |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 218 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 224 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.612 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.762 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.613 s | +3.763 s | S | `0x27000300` | AbilityCasterInfoReply | 394 B | |
| +8.660 s | +3.809 s | C | `0x00006F00` | ClientLog | 235 B ×1 |  |
| +8.763 s | +3.913 s | C | `0x27000200` | AbilityCasterInfoRequest | 4 B | |
| +8.763 s | +3.913 s | C | `0x00006F00` | ClientLog | 56 B ×1 |  |
| +8.768 s | +3.918 s | S | `0x26006500` | Unknown | 16 B ×3 | burst +8.768 s → +8.768 s |
| +8.775 s | +3.925 s | C | `0x00006300` | KeepAlive | 4 B | |
| +8.866 s | +4.016 s | S | `0x26006500` | Unknown | 16 B ×1 |  |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.909 s | +4.059 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.919 s | +4.069 s | S | `0x27000300` | AbilityCasterInfoReply | 178 B | |
| +8.919 s | +4.069 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.919 s | +4.069 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.919 s | +4.069 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +8.983 s | +4.133 s | C | `0x0000C300` | Synchronization | 49 B | |
| +8.983 s | +4.133 s | C | `0x00004500` | InGamePurchaseBase | 9 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.044 s | +4.194 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.054 s | +4.204 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.054 s | +4.204 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.054 s | +4.204 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.160 s | +4.309 s | C | `0x1401C02C` | UnknownNoise | 58 B ×1 |  |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.201 s | +4.350 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.208 s | +4.358 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.220 s | +4.369 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.220 s | +4.369 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.220 s | +4.369 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.283 s | +4.433 s | S | `0x0000F000` | TemplateBase | 5136 B | |
| +9.335 s | +4.484 s | C | `0x14016E2D` | UnknownNoise | 58 B ×1 |  |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.348 s | +4.498 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.354 s | +4.504 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.354 s | +4.504 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.354 s | +4.504 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.403 s | +4.552 s | C | `0x1401B32D` | UnknownNoise | 58 B ×1 |  |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.454 s | +4.603 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.464 s | +4.614 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.464 s | +4.614 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.464 s | +4.614 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.553 s | +4.702 s | C | `0x1401482E` | UnknownNoise | 58 B ×1 |  |
| +9.571 s | +4.720 s | C | `0x0000F000` | TemplateBase | 18 B | |
| +9.593 s | +4.743 s | S | `0x0000F000` | TemplateBase | 31461 B | |
| +9.594 s | +4.744 s | S | `0x0000F000` | TemplateBase | 3816 B | |
| +9.594 s | +4.744 s | S | `0x0000F000` | TemplateBase | 4830 B | |
| +9.595 s | +4.745 s | S | `0x0000F000` | TemplateBase | 14774 B | |
| +9.708 s | +4.858 s | C | `0x1401E32E` | UnknownNoise | 58 B ×1 |  |
| +9.748 s | +4.897 s | S | `0x0000F000` | TemplateBase | 5676 B | |
| +9.749 s | +4.899 s | S | `0x0000F000` | TemplateBase | 5658 B | |
| +9.749 s | +4.899 s | S | `0x0000F000` | TemplateBase | 751 B | |
| +9.749 s | +4.899 s | S | `0x0000F000` | TemplateBase | 1643 B | |
| +9.750 s | +4.900 s | S | `0x0000F000` | TemplateBase | 4547 B | |
| +9.751 s | +4.901 s | S | `0x0000F000` | TemplateBase | 7593 B | |
| +9.753 s | +4.903 s | S | `0x0000F000` | TemplateBase | 20601 B | |
| +9.753 s | +4.903 s | S | `0x0000F000` | TemplateBase | 5712 B | |
| +9.753 s | +4.903 s | S | `0x0000F000` | TemplateBase | 365 B | |
| +9.753 s | +4.903 s | S | `0x0000F000` | TemplateBase | 430 B | |
| +9.753 s | +4.903 s | S | `0x0000F000` | TemplateBase | 2799 B | |
| +9.754 s | +4.904 s | S | `0x0000F000` | TemplateBase | 5805 B | |
| +9.756 s | +4.906 s | S | `0x0000F000` | TemplateBase | 10835 B | |
| +9.786 s | +4.935 s | C | `0x00006300` | KeepAlive | 4 B | |
| +9.833 s | +4.982 s | C | `0x1401602F` | UnknownNoise | 58 B ×1 |  |
| +9.868 s | +5.018 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.868 s | +5.018 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +9.900 s | +5.049 s | S | `0x0000F000` | TemplateBase | 14122 B | |
| +9.901 s | +5.050 s | S | `0x0000F000` | TemplateBase | 10660 B | |
| +9.901 s | +5.050 s | S | `0x0000F000` | TemplateBase | 1544 B | |
| +9.901 s | +5.051 s | S | `0x0000F000` | TemplateBase | 4686 B | |
| +9.904 s | +5.054 s | S | `0x0000F000` | TemplateBase | 26267 B | |
| +9.904 s | +5.054 s | S | `0x0000F000` | TemplateBase | 1234 B | |
| +9.906 s | +5.056 s | S | `0x0000F000` | TemplateBase | 11988 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 295 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 295 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 295 B | |
| +9.906 s | +5.056 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +9.907 s | +5.056 s | S | `0x0000F000` | TemplateBase | 4557 B | |
| +9.911 s | +5.060 s | S | `0x0000F000` | TemplateBase | 37677 B | |
| +9.911 s | +5.061 s | S | `0x0000F000` | TemplateBase | 1602 B | |
| +9.912 s | +5.061 s | S | `0x0000F000` | TemplateBase | 4556 B | |
| +9.912 s | +5.061 s | S | `0x0000F000` | TemplateBase | 915 B | |
| +9.913 s | +5.063 s | S | `0x0000F000` | TemplateBase | 11162 B | |
| +9.914 s | +5.064 s | S | `0x0000F000` | TemplateBase | 9295 B | |
| +9.994 s | +5.143 s | C | `0x14010430` | UnknownNoise | 58 B ×1 |  |
| +10.047 s | +5.196 s | S | `0x0000F000` | TemplateBase | 5751 B | |
| +10.047 s | +5.196 s | S | `0x0000F000` | TemplateBase | 724 B | |
| +10.048 s | +5.198 s | S | `0x0000F000` | TemplateBase | 12742 B | |
| +10.048 s | +5.198 s | S | `0x0000F000` | TemplateBase | 1540 B | |
| +10.049 s | +5.199 s | S | `0x0000F000` | TemplateBase | 4853 B | |
| +10.049 s | +5.199 s | S | `0x0000F000` | TemplateBase | 2335 B | |
| +10.050 s | +5.199 s | S | `0x0000F000` | TemplateBase | 1169 B | |
| +10.051 s | +5.200 s | S | `0x0000F000` | TemplateBase | 6063 B | |
| +10.052 s | +5.202 s | S | `0x0000F000` | TemplateBase | 15827 B | |
| +10.053 s | +5.202 s | S | `0x0000F000` | TemplateBase | 8350 B | |
| +10.054 s | +5.203 s | S | `0x0000F000` | TemplateBase | 8323 B | |
| +10.068 s | +5.218 s | S | `0x0000F000` | TemplateBase | 126486 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +10.132 s | +5.281 s | C | `0x14019730` | UnknownNoise | 58 B ×1 |  |
| +10.213 s | +5.363 s | S | `0x0000F000` | TemplateBase | 162522 B | |
| +10.285 s | +5.435 s | C | `0x14012531` | UnknownNoise | 58 B ×1 |  |
| +10.356 s | +5.505 s | S | `0x0000F000` | TemplateBase | 125092 B | |
| +10.451 s | +5.601 s | C | `0x1401CC31` | UnknownNoise | 58 B ×1 |  |
| +10.497 s | +5.646 s | S | `0x0000F000` | TemplateBase | 147235 B | |
| +10.518 s | +5.668 s | S | `0x0000F000` | TemplateBase | 111320 B | |
| +10.525 s | +5.675 s | S | `0x0000F000` | TemplateBase | 42876 B | |
| +10.597 s | +5.747 s | C | `0x14015E32` | UnknownNoise | 58 B ×1 |  |
| +10.668 s | +5.818 s | S | `0x0000F000` | TemplateBase | 163427 B | |
| +10.669 s | +5.819 s | S | `0x0000F000` | TemplateBase | 10949 B | |
| +10.674 s | +5.823 s | S | `0x0000F000` | TemplateBase | 27969 B | |
| +10.746 s | +5.896 s | C | `0x1401F232` | UnknownNoise | 58 B ×1 |  |
| +10.802 s | +5.952 s | S | `0x0000F000` | TemplateBase | 26451 B | |
| +10.824 s | +5.973 s | S | `0x0000F000` | TemplateBase | 178019 B | |
| +11.110 s | +6.260 s | S | `0x0000F000` | TemplateBase | 157250 B | |
| +11.114 s | +6.264 s | S | `0x0000F000` | TemplateBase | 34201 B | |
| +11.121 s | +6.271 s | C | `0x00006300` | KeepAlive | 4 B | |
| +11.123 s | +6.273 s | S | `0x0000F000` | TemplateBase | 75160 B | |
| +11.127 s | +6.277 s | S | `0x0000F000` | TemplateBase | 23272 B | |
| +11.260 s | +6.410 s | S | `0x0000F000` | TemplateBase | 55624 B | |
| +11.266 s | +6.416 s | S | `0x0000F000` | TemplateBase | 41198 B | |
| +11.267 s | +6.417 s | S | `0x0000F000` | TemplateBase | 6799 B | |
| +11.271 s | +6.421 s | S | `0x0000F000` | TemplateBase | 29033 B | |
| +11.273 s | +6.423 s | S | `0x0000F000` | TemplateBase | 13490 B | |
| +11.280 s | +6.430 s | S | `0x0000F000` | TemplateBase | 52367 B | |
| +11.280 s | +6.430 s | S | `0x0000F000` | TemplateBase | 283 B | |
| +11.280 s | +6.430 s | S | `0x0000F000` | TemplateBase | 316 B | |
| +11.404 s | +6.554 s | S | `0x0000F000` | TemplateBase | 36668 B | |
| +11.405 s | +6.555 s | C | `0x14018335` | UnknownNoise | 58 B ×1 |  |
| +11.405 s | +6.555 s | S | `0x0000F000` | TemplateBase | 15592 B | |
| +11.407 s | +6.557 s | S | `0x0000F000` | TemplateBase | 12070 B | |
| +11.412 s | +6.562 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.412 s | +6.562 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.412 s | +6.562 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.415 s | +6.565 s | S | `0x0000F000` | TemplateBase | 24938 B | |
| +11.415 s | +6.565 s | S | `0x0000F000` | TemplateBase | 3527 B | |
| +11.435 s | +6.585 s | S | `0x0000F000` | TemplateBase | 129759 B | |
| +11.436 s | +6.586 s | S | `0x0000F000` | TemplateBase | 9916 B | |
| +11.436 s | +6.586 s | S | `0x0000F000` | TemplateBase | 305 B | |
| +11.445 s | +6.595 s | C | `0x1401AC35` | UnknownNoise | 58 B ×1 |  |
| +11.513 s | +6.663 s | C | `0x1401F135` | UnknownNoise | 58 B ×1 |  |
| +11.563 s | +6.713 s | S | `0x0000F000` | TemplateBase | 44295 B | |
| +11.567 s | +6.717 s | S | `0x0000F000` | TemplateBase | 29371 B | |
| +11.568 s | +6.718 s | S | `0x0000F000` | TemplateBase | 8151 B | |
| +11.571 s | +6.721 s | S | `0x0000F000` | TemplateBase | 15594 B | |
| +11.572 s | +6.722 s | S | `0x0000F000` | TemplateBase | 1870 B | |
| +11.572 s | +6.722 s | S | `0x0000F000` | TemplateBase | 1277 B | |
| +11.575 s | +6.725 s | S | `0x0000F000` | TemplateBase | 20118 B | |
| +11.581 s | +6.731 s | S | `0x0000F000` | TemplateBase | 35097 B | |
| +11.590 s | +6.740 s | S | `0x0000F000` | TemplateBase | 29538 B | |
| +11.590 s | +6.740 s | S | `0x0000F000` | TemplateBase | 9061 B | |
| +11.591 s | +6.741 s | S | `0x0000F000` | TemplateBase | 3168 B | |
| +11.591 s | +6.741 s | S | `0x0000F000` | TemplateBase | 4639 B | |
| +11.672 s | +6.822 s | C | `0x14019036` | UnknownNoise | 58 B ×1 |  |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.688 s | +6.838 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +11.700 s | +6.850 s | S | `0x0000F000` | TemplateBase | 4050 B | |
| +11.700 s | +6.850 s | S | `0x0000F000` | TemplateBase | 426 B | |
| +11.701 s | +6.851 s | S | `0x0000F000` | TemplateBase | 377 B | |
| +11.701 s | +6.851 s | S | `0x0000F000` | TemplateBase | 3194 B | |
| +11.707 s | +6.857 s | S | `0x0000F000` | TemplateBase | 22152 B | |
| +11.707 s | +6.857 s | S | `0x0000F000` | TemplateBase | 4342 B | |
| +11.715 s | +6.865 s | S | `0x0000F000` | TemplateBase | 11299 B | |
| +11.716 s | +6.866 s | S | `0x0000F000` | TemplateBase | 2908 B | |
| +11.716 s | +6.866 s | S | `0x0000F000` | TemplateBase | 454 B | |
| +11.716 s | +6.866 s | S | `0x0000F000` | TemplateBase | 3672 B | |
| +11.720 s | +6.870 s | S | `0x0000F000` | TemplateBase | 28447 B | |
| +11.720 s | +6.870 s | S | `0x0000F000` | TemplateBase | 414 B | |
| +11.720 s | +6.870 s | S | `0x0000F000` | TemplateBase | 412 B | |
| +11.720 s | +6.870 s | S | `0x0000F000` | TemplateBase | 425 B | |
| +11.722 s | +6.872 s | S | `0x0000F000` | TemplateBase | 2311 B | |
| +11.723 s | +6.873 s | S | `0x0000F000` | TemplateBase | 6243 B | |
| +11.723 s | +6.873 s | S | `0x0000F000` | TemplateBase | 607 B | |
| +11.724 s | +6.874 s | S | `0x0000F000` | TemplateBase | 11516 B | |
| +11.724 s | +6.874 s | S | `0x0000F000` | TemplateBase | 408 B | |
| +11.725 s | +6.875 s | S | `0x0000F000` | TemplateBase | 7170 B | |
| +11.726 s | +6.876 s | S | `0x0000F000` | TemplateBase | 6751 B | |
| +11.735 s | +6.884 s | S | `0x0000F000` | TemplateBase | 35521 B | |
| +11.735 s | +6.885 s | S | `0x0000F000` | TemplateBase | 5973 B | |
| +11.735 s | +6.885 s | S | `0x0000F000` | TemplateBase | 1083 B | |
| +11.735 s | +6.885 s | S | `0x0000F000` | TemplateBase | 1048 B | |
| +11.735 s | +6.885 s | S | `0x0000F000` | TemplateBase | 503 B | |
| +11.735 s | +6.885 s | S | `0x0000F000` | TemplateBase | 1191 B | |
| +11.736 s | +6.885 s | S | `0x0000F000` | TemplateBase | 1932 B | |
| +11.736 s | +6.886 s | S | `0x0000F000` | TemplateBase | 2285 B | |
| +11.746 s | +6.896 s | S | `0x0000F000` | TemplateBase | 10636 B | |
| +11.828 s | +6.978 s | C | `0x14012B37` | UnknownNoise | 58 B ×1 |  |
| +11.866 s | +7.016 s | S | `0x0000F000` | TemplateBase | 77758 B | |
| +11.866 s | +7.016 s | S | `0x0000F000` | TemplateBase | 1814 B | |
| +11.876 s | +7.026 s | S | `0x0000F000` | TemplateBase | 54245 B | |
| +11.876 s | +7.026 s | S | `0x0000F000` | TemplateBase | 1110 B | |
| +12.035 s | +7.184 s | S | `0x0000F000` | TemplateBase | 220245 B | |
| +12.035 s | +7.185 s | S | `0x0000F000` | TemplateBase | 2127 B | |
| +12.036 s | +7.186 s | S | `0x0000F000` | TemplateBase | 8536 B | |
| +12.154 s | +7.304 s | C | `0x00006300` | KeepAlive | 4 B | |
| +12.154 s | +7.304 s | C | `0x14017638` | UnknownNoise | 58 B ×1 |  |
| +12.174 s | +7.324 s | S | `0x0000F000` | TemplateBase | 173288 B | |
| +12.174 s | +7.324 s | S | `0x0000F000` | TemplateBase | 1734 B | |
| +12.174 s | +7.324 s | S | `0x0000F000` | TemplateBase | 1952 B | |
| +12.174 s | +7.324 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.174 s | +7.324 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.174 s | +7.324 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.174 s | +7.324 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.175 s | +7.325 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.175 s | +7.325 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.175 s | +7.325 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.178 s | +7.328 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.179 s | +7.328 s | S | `0x26000700` | PlayerUpdateNpcRelevance | 968 B | |
| +12.179 s | +7.329 s | S | `0x26000500` | PlayerUpdateAddNotifications | 1362 B | |
| +12.179 s | +7.329 s | S | `0x0000C400` | ResourcesBase | 346 B | |
| +12.179 s | +7.329 s | S | `0x0000C400` | ResourcesBase | 346 B | |
| +12.179 s | +7.329 s | S | `0x0000C400` | ResourcesBase | 346 B | |
| +12.179 s | +7.329 s | S | `0x0000C400` | ResourcesBase | 105 B | |
| +12.179 s | +7.329 s | S | `0x0000C400` | ResourcesBase | 105 B | |
| +12.179 s | +7.329 s | S | `0x0000C400` | ResourcesBase | 105 B | |
| +12.179 s | +7.329 s | S | `0x29002C00` | ClientUpdateUpdateCalculatedStats | 20 B | |
| +12.179 s | +7.329 s | S | `0x26005100` | PlayerUpdateUpdateCharacterStateDelta | 38 B | |
| +12.179 s | +7.329 s | S | `0x00004500` | InGamePurchaseBase | 480 B | |
| +12.179 s | +7.329 s | S | `0x29001F00` | ClientUpdateZonePopulation | 16 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 283 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 283 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 325 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.179 s | +7.329 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 283 B | |
| +12.560 s | +7.710 s | C | `0x14010A3A` | UnknownNoise | 58 B ×1 |  |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.577 s | +7.727 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.621 s | +7.771 s | C | `0x1401443A` | UnknownNoise | 58 B ×1 |  |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 295 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.739 s | +7.889 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +12.831 s | +7.981 s | C | `0x0000C300` | Synchronization | 49 B | |
| +12.831 s | +7.981 s | C | `0x1401193B` | UnknownNoise | 58 B ×1 |  |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.851 s | +8.001 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +12.896 s | +8.046 s | C | `0x1401573B` | UnknownNoise | 58 B ×1 |  |
| +13.007 s | +8.157 s | C | `0x1401C73B` | UnknownNoise | 58 B ×1 |  |
| +13.009 s | +8.159 s | S | `0x0000C300` | Synchronization | 49 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 292 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.011 s | +8.161 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.012 s | +8.162 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.012 s | +8.162 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.012 s | +8.162 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.097 s | +8.247 s | C | `0x0000C300` | Synchronization | 49 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.116 s | +8.265 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.159 s | +8.308 s | C | `0x00006300` | KeepAlive | 4 B | |
| +13.159 s | +8.308 s | C | `0x14013C3F` | UnknownNoise | 58 B ×1 |  |
| +13.268 s | +8.417 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.268 s | +8.417 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.268 s | +8.417 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.268 s | +8.417 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.268 s | +8.417 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.268 s | +8.417 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.269 s | +8.418 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.269 s | +8.418 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.269 s | +8.418 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.269 s | +8.418 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.318 s | +8.468 s | C | `0x1401DC3F` | UnknownNoise | 58 B ×1 |  |
| +13.370 s | +8.519 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.370 s | +8.519 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.370 s | +8.519 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.370 s | +8.519 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +13.475 s | +8.624 s | C | `0x14017740` | UnknownNoise | 58 B ×1 |  |
| +13.526 s | +8.676 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.526 s | +8.676 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.526 s | +8.676 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.526 s | +8.676 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +13.617 s | +8.767 s | C | `0x14010541` | UnknownNoise | 58 B ×1 |  |
| +13.760 s | +8.909 s | C | `0x14019541` | UnknownNoise | 58 B ×1 |  |
| +13.909 s | +9.058 s | C | `0x14012A42` | UnknownNoise | 58 B ×1 |  |
| +14.045 s | +9.195 s | C | `0x1401B342` | UnknownNoise | 58 B ×1 |  |
| +14.162 s | +9.311 s | C | `0x00006300` | KeepAlive | 4 B | |
| +14.225 s | +9.374 s | C | `0x14016643` | UnknownNoise | 58 B ×1 |  |
| +14.382 s | +9.532 s | C | `0x14010344` | UnknownNoise | 58 B ×1 |  |
| +14.505 s | +9.655 s | C | `0x14017E44` | UnknownNoise | 58 B ×1 |  |
| +14.679 s | +9.829 s | C | `0x14012B45` | UnknownNoise | 58 B ×1 |  |
| +14.795 s | +9.945 s | C | `0x00006F00` | ClientLog | 80–82 B ×2 (sum 162 B) | burst +14.795 s → +15.012 s |
| +15.069 s | +10.219 s | C | `0x26006C00` | PlayerUpdateLoading | 3 B | |
| +15.262 s | +10.412 s | C | `0x0000F900` | ClaimsBase | 1 B | |
| +15.300 s | +10.450 s | C | `0x00006300` | KeepAlive | 4 B | |
| +15.300 s | +10.450 s | C | `0x029E47FF` | UnknownNoise | 14 B ×1 |  |
| +15.300 s | +10.450 s | C | `0x14019E47` | UnknownNoise | 58 B ×1 |  |
| +15.300 s | +10.450 s | C | `0x0000A700` | RatingBase | 66 B | |
| +15.300 s | +10.450 s | C | `0x0000F900` | ClaimsBase | 9 B | |
| +15.300 s | +10.450 s | C | `0x05010311` | UnknownNoise | 12 B ×1 |  |
| +15.312 s | +10.462 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +15.342 s | +10.492 s | C | `0x1401C447` | UnknownNoise | 58 B ×1 |  |
| +15.413 s | +10.563 s | S | `0x0000F900` | ClaimsBase | 53 B | |
| +15.413 s | +10.563 s | S | `0x0000F900` | ClaimsBase | 192 B | |
| +15.480 s | +10.630 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +15.568 s | +10.718 s | S | `0x0000A700` | RatingBase | 41 B | |
| +16.035 s | +11.185 s | C | `0x1401774A` | UnknownNoise | 58 B ×1 |  |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.035 s | +11.185 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +16.158 s | +11.308 s | C | `0x1401F34A` | UnknownNoise | 58 B ×1 |  |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 325 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 294 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 317 B | |
| +16.198 s | +11.348 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 295 B | |
| +16.356 s | +11.506 s | C | `0x00006300` | KeepAlive | 4 B | |
| +16.356 s | +11.506 s | C | `0x1401B94B` | UnknownNoise | 58 B ×1 |  |
| +16.404 s | +11.554 s | C | `0x1401E74B` | UnknownNoise | 58 B ×1 |  |
| +16.451 s | +11.601 s | C | `0x1401174C` | UnknownNoise | 58 B ×1 |  |
| +16.597 s | +11.747 s | C | `0x1401AA4C` | UnknownNoise | 58 B ×1 |  |
| +16.744 s | +11.893 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +16.781 s | +11.930 s | C | `0x1401614D` | UnknownNoise | 58 B ×1 |  |
| +16.903 s | +12.052 s | C | `0x1401DA4D` | UnknownNoise | 58 B ×1 |  |
| +16.903 s | +12.053 s | S | `0x00003700` | GameTimeSync | 13 B | |
| +17.078 s | +12.227 s | C | `0x14018B4E` | UnknownNoise | 58 B ×1 |  |
| +17.236 s | +12.385 s | C | `0x1401294F` | UnknownNoise | 58 B ×1 |  |
| +17.352 s | +12.502 s | C | `0x14019E4F` | UnknownNoise | 58 B ×1 |  |
| +17.382 s | +12.532 s | C | `0x00006300` | KeepAlive | 4 B | |
| +17.501 s | +12.650 s | C | `0x14013150` | UnknownNoise | 58 B ×1 |  |
| +17.646 s | +12.796 s | C | `0x1401C450` | UnknownNoise | 58 B ×1 |  |
| +17.797 s | +12.947 s | C | `0x14015A51` | UnknownNoise | 58 B ×1 |  |
| +17.861 s | +13.011 s | C | `0x0000C300` | Synchronization | 49 B | |
| +17.976 s | +13.125 s | C | `0x14010D52` | UnknownNoise | 58 B ×1 |  |
| +18.040 s | +13.190 s | S | `0x0000C300` | Synchronization | 49 B | |
| +18.067 s | +13.217 s | C | `0x0000C300` | Synchronization | 49 B | |
| +18.094 s | +13.244 s | C | `0x14018352` | UnknownNoise | 58 B ×1 |  |
| +18.243 s | +13.393 s | C | `0x14011953` | UnknownNoise | 58 B ×1 |  |
| +18.397 s | +13.547 s | C | `0x00006300` | KeepAlive | 4 B | |
| +18.423 s | +13.573 s | C | `0x1401CC53` | UnknownNoise | 58 B ×1 |  |
| +18.524 s | +13.674 s | S | `0x26007400` | PlayerUpdateAddFeatherweightObject | 90 B ×17 | burst +18.524 s → +18.540 s |
| +18.578 s | +13.728 s | C | `0x14016654` | UnknownNoise | 58 B ×1 |  |
| +18.691 s | +13.841 s | C | `0x1401D854` | UnknownNoise | 58 B ×1 |  |
| +18.869 s | +14.019 s | C | `0x14018B55` | UnknownNoise | 58 B ×1 |  |
| +19.005 s | +14.155 s | C | `0x00006F00` | ClientLog | 55 B ×1 |  |
| +19.029 s | +14.179 s | C | `0x14012A56` | UnknownNoise | 58 B ×1 |  |
| +19.142 s | +14.292 s | C | `0x14019C56` | UnknownNoise | 58 B ×1 |  |
| +19.325 s | +14.475 s | C | `0x14015057` | UnknownNoise | 58 B ×1 |  |
| +19.412 s | +14.562 s | C | `0x00006300` | KeepAlive | 4 B | |
| +19.477 s | +14.627 s | C | `0x1401EC57` | UnknownNoise | 58 B ×1 |  |
| +19.594 s | +14.744 s | C | `0x14015F58` | UnknownNoise | 58 B ×1 |  |
| +19.773 s | +14.923 s | C | `0x14011259` | UnknownNoise | 58 B ×1 |  |
| +19.818 s | +14.967 s | S | `0x27000D00` | AbilityEffectEnd | 63 B | |
| +19.818 s | +14.967 s | S | `0x27000500` | AbilityAbilityEnd | 31 B | |
| +19.818 s | +14.967 s | S | `0x26005100` | PlayerUpdateUpdateCharacterStateDelta | 38 B | |
| +19.929 s | +15.079 s | C | `0x1401AD59` | UnknownNoise | 58 B ×1 |  |
| +20.042 s | +15.192 s | C | `0x1401205A` | UnknownNoise | 58 B ×1 |  |
| +20.223 s | +15.373 s | C | `0x1401D35A` | UnknownNoise | 58 B ×1 |  |
| +20.378 s | +15.527 s | C | `0x1401705B` | UnknownNoise | 58 B ×1 |  |
| +20.433 s | +15.582 s | C | `0x00006300` | KeepAlive | 4 B | |
| +20.493 s | +15.642 s | C | `0x1401E25B` | UnknownNoise | 58 B ×1 |  |
| +20.673 s | +15.822 s | C | `0x1401955C` | UnknownNoise | 58 B ×1 |  |
| +20.827 s | +15.976 s | C | `0x1401315D` | UnknownNoise | 58 B ×1 |  |
| +20.943 s | +16.092 s | C | `0x1401A35D` | UnknownNoise | 58 B ×1 |  |
| +21.124 s | +16.274 s | C | `0x1401595E` | UnknownNoise | 58 B ×1 |  |
| +21.278 s | +16.427 s | C | `0x1401F35E` | UnknownNoise | 58 B ×1 |  |
| +21.392 s | +16.542 s | C | `0x1401655F` | UnknownNoise | 58 B ×1 |  |
| +21.457 s | +16.607 s | C | `0x00006300` | KeepAlive | 4 B | |
| +21.570 s | +16.720 s | C | `0x14011860` | UnknownNoise | 58 B ×1 |  |
| +21.724 s | +16.874 s | C | `0x1401B160` | UnknownNoise | 58 B ×1 |  |
| +21.871 s | +17.021 s | C | `0x14014461` | UnknownNoise | 58 B ×1 |  |
| +22.020 s | +17.170 s | C | `0x1401D961` | UnknownNoise | 58 B ×1 |  |
| +22.174 s | +17.324 s | C | `0x14017562` | UnknownNoise | 58 B ×1 |  |
| +22.290 s | +17.440 s | C | `0x1401E762` | UnknownNoise | 58 B ×1 |  |
| +22.471 s | +17.621 s | C | `0x00006300` | KeepAlive | 4 B | |
| +22.471 s | +17.621 s | C | `0x14019C63` | UnknownNoise | 58 B ×1 |  |
| +22.626 s | +17.776 s | C | `0x14013764` | UnknownNoise | 58 B ×1 |  |
| +22.739 s | +17.889 s | C | `0x1401AA64` | UnknownNoise | 58 B ×1 |  |
| +22.862 s | +18.012 s | C | `0x0000C300` | Synchronization | 49 B | |
| +22.925 s | +18.075 s | C | `0x14016265` | UnknownNoise | 58 B ×1 |  |
| +23.021 s | +18.171 s | S | `0x0000C300` | Synchronization | 49 B | |
| +23.047 s | +18.197 s | C | `0x0000C300` | Synchronization | 49 B | |
| +23.081 s | +18.231 s | C | `0x1401FF65` | UnknownNoise | 58 B ×1 |  |
| +23.195 s | +18.345 s | C | `0x14017166` | UnknownNoise | 58 B ×1 |  |
| +23.374 s | +18.524 s | C | `0x14012367` | UnknownNoise | 58 B ×1 |  |
| +23.494 s | +18.643 s | C | `0x00006300` | KeepAlive | 4 B | |
| +23.529 s | +18.679 s | C | `0x04BD67FF` | UnknownNoise | 9 B ×1 |  |
| +23.529 s | +18.679 s | C | `0x1401BD67` | UnknownNoise | 58 B ×1 |  |
| +23.622 s | +18.772 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.667 s | +18.817 s | C | `0x044868FF` | UnknownNoise | 10 B ×1 |  |
| +23.667 s | +18.817 s | C | `0x14014868` | UnknownNoise | 58 B ×1 |  |
| +23.690 s | +18.840 s | C | `0x036268FF` | UnknownNoise | 14 B ×1 |  |
| +23.724 s | +18.874 s | C | `0x048068FF` | UnknownNoise | 10 B ×1 |  |
| +23.773 s | +18.923 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 295 B | |
| +23.816 s | +18.966 s | C | `0x00DD68FF` | UnknownNoise | 37 B ×1 |  |
| +23.816 s | +18.966 s | C | `0x1401DE68` | UnknownNoise | 58 B ×1 |  |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.885 s | +19.035 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +23.975 s | +19.124 s | C | `0x14017B69` | UnknownNoise | 58 B ×1 |  |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.046 s | +19.196 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.097 s | +19.246 s | C | `0x04F669FF` | UnknownNoise | 10 B ×1 |  |
| +24.097 s | +19.246 s | C | `0x1401F769` | UnknownNoise | 58 B ×1 |  |
| +24.257 s | +19.407 s | C | `0x049B6AFF` | UnknownNoise | 10 B ×1 |  |
| +24.257 s | +19.407 s | C | `0x14019B6A` | UnknownNoise | 58 B ×1 |  |
| +24.299 s | +19.448 s | C | `0x03C16AFF` | UnknownNoise | 14 B ×1 |  |
| +24.335 s | +19.484 s | C | `0x04E46AFF` | UnknownNoise | 11 B ×1 |  |
| +24.401 s | +19.550 s | C | `0x00266BFF` | UnknownNoise | 37 B ×1 |  |
| +24.401 s | +19.550 s | C | `0x1401276B` | UnknownNoise | 58 B ×1 |  |
| +24.431 s | +19.581 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +24.431 s | +19.581 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +24.431 s | +19.581 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +24.494 s | +19.644 s | C | `0x00006300` | KeepAlive | 4 B | |
| +24.495 s | +19.644 s | S | `0x26007200` | PlayerUpdateMobMovement | 102 B | |
| +24.553 s | +19.702 s | C | `0x04BD6BFF` | UnknownNoise | 10 B ×1 |  |
| +24.553 s | +19.702 s | C | `0x1401BD6B` | UnknownNoise | 58 B ×1 |  |
| +24.590 s | +19.740 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.590 s | +19.740 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276 B | |
| +24.590 s | +19.740 s | S | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 283 B | |
| +24.598 s | +19.747 s | C | `0x03EC6BFF` | UnknownNoise | 14 B ×1 |  |
| +24.697 s | +19.846 s | C | `0x14014D6C` | UnknownNoise | 58 B ×1 |  |
| +24.846 s | +19.995 s | C | `0x1401E26C` | UnknownNoise | 58 B ×1 |  |
| +24.991 s | +20.141 s | C | `0x04746DFF` | UnknownNoise | 10 B ×1 |  |
| +24.991 s | +20.141 s | C | `0x1401746D` | UnknownNoise | 58 B ×1 |  |
| +25.164 s | +20.314 s | C | `0x1401216E` | UnknownNoise | 58 B ×1 |  |
| +25.313 s | +20.462 s | C | `0x1401B66E` | UnknownNoise | 58 B ×1 |  |
| +25.458 s | +20.608 s | C | `0x1401486F` | UnknownNoise | 58 B ×1 |  |
| +25.507 s | +20.657 s | C | `0x00006300` | KeepAlive | 4 B | |
| +25.607 s | +20.757 s | C | `0x1401DB6F` | UnknownNoise | 58 B ×1 |  |
| +25.756 s | +20.906 s | C | `0x14017170` | UnknownNoise | 58 B ×1 |  |
| +25.935 s | +21.085 s | C | `0x14012371` | UnknownNoise | 58 B ×1 |  |
| +26.063 s | +21.213 s | C | `0x1401A471` | UnknownNoise | 58 B ×1 |  |
| +26.207 s | +21.357 s | C | `0x14013572` | UnknownNoise | 58 B ×1 |  |
| +26.354 s | +21.504 s | C | `0x1401C672` | UnknownNoise | 58 B ×1 |  |
| +26.501 s | +21.651 s | C | `0x14015A73` | UnknownNoise | 58 B ×1 |  |
| +26.525 s | +21.675 s | C | `0x00006300` | KeepAlive | 4 B | |
| +26.647 s | +21.797 s | C | `0x1401ED73` | UnknownNoise | 58 B ×1 |  |
| +26.793 s | +21.943 s | C | `0x14017F74` | UnknownNoise | 58 B ×1 |  |
| +26.943 s | +22.093 s | C | `0x14011375` | UnknownNoise | 58 B ×1 |  |
| +27.114 s | +22.264 s | C | `0x1401BF75` | UnknownNoise | 58 B ×1 |  |
| +27.260 s | +22.410 s | C | `0x14015276` | UnknownNoise | 58 B ×1 |  |
| +27.407 s | +22.557 s | C | `0x1401E576` | UnknownNoise | 58 B ×1 |  |
| +27.530 s | +22.680 s | C | `0x00006300` | KeepAlive | 4 B | |
| +27.556 s | +22.706 s | C | `0x14017977` | UnknownNoise | 58 B ×1 |  |
| +27.705 s | +22.854 s | C | `0x14010C78` | UnknownNoise | 58 B ×1 |  |
| +27.755 s | +22.904 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +27.852 s | +23.002 s | C | `0x1401A278` | UnknownNoise | 58 B ×1 |  |
| +27.878 s | +23.027 s | C | `0x0000C300` | Synchronization | 49 B | |
| +27.907 s | +23.057 s | S | `0x00003700` | GameTimeSync | 13 B | |
| +28.000 s | +23.149 s | C | `0x14013579` | UnknownNoise | 58 B ×1 |  |
| +28.036 s | +23.185 s | S | `0x0000C300` | Synchronization | 49 B | |
| +28.047 s | +23.196 s | C | `0x0000C300` | Synchronization | 49 B | |
| +28.147 s | +23.297 s | C | `0x14014979` | UnknownNoise | 58 B ×1 |  |
| +28.295 s | +23.445 s | C | `0x1401DC79` | UnknownNoise | 58 B ×1 |  |
| +28.441 s | +23.590 s | C | `0x14016F7A` | UnknownNoise | 58 B ×1 |  |
| +28.539 s | +23.689 s | C | `0x00006300` | KeepAlive | 4 B | |
| +28.614 s | +23.763 s | C | `0x14011C7B` | UnknownNoise | 58 B ×1 |  |
| +28.758 s | +23.908 s | C | `0x1401AE7B` | UnknownNoise | 58 B ×1 |  |
| +28.908 s | +24.058 s | C | `0x1401417C` | UnknownNoise | 58 B ×1 |  |
| +29.055 s | +24.205 s | C | `0x1401D47C` | UnknownNoise | 58 B ×1 |  |
| +29.202 s | +24.352 s | C | `0x1401697D` | UnknownNoise | 58 B ×1 |  |
| +29.227 s | +24.377 s | C | `0x00001E00` | CommandBase | 13 B | |
| +29.347 s | +24.497 s | C | `0x1401FA7D` | UnknownNoise | 58 B ×1 |  |
| +29.439 s | +24.589 s | S | `0x00001E00` | CommandBase | 75 B | |
| +29.502 s | +24.652 s | C | `0x1401947E` | UnknownNoise | 58 B ×1 |  |
| +29.542 s | +24.692 s | C | `0x00006300` | KeepAlive | 4 B | |
| +29.643 s | +24.793 s | C | `0x1401207F` | UnknownNoise | 58 B ×1 |  |
| +29.813 s | +24.963 s | C | `0x1401CC7F` | UnknownNoise | 58 B ×1 |  |
| +29.959 s | +25.109 s | C | `0x14015F80` | UnknownNoise | 58 B ×1 |  |
| +30.109 s | +25.259 s | C | `0x1401F380` | UnknownNoise | 58 B ×1 |  |
| +30.256 s | +25.406 s | C | `0x14018681` | UnknownNoise | 58 B ×1 |  |
| +30.403 s | +25.553 s | C | `0x14011882` | UnknownNoise | 58 B ×1 |  |
| +30.549 s | +25.699 s | C | `0x00006300` | KeepAlive | 4 B | |
| +30.549 s | +25.699 s | C | `0x1401AB82` | UnknownNoise | 58 B ×1 |  |
| +30.698 s | +25.848 s | C | `0x14013F83` | UnknownNoise | 58 B ×1 |  |
| +30.843 s | +25.993 s | C | `0x1401D383` | UnknownNoise | 58 B ×1 |  |
| +30.992 s | +26.142 s | C | `0x14016584` | UnknownNoise | 58 B ×1 |  |
| +31.164 s | +26.314 s | C | `0x14011285` | UnknownNoise | 58 B ×1 |  |
| +31.312 s | +26.462 s | C | `0x1401A685` | UnknownNoise | 58 B ×1 |  |
| +31.460 s | +26.610 s | C | `0x14013A86` | UnknownNoise | 58 B ×1 |  |
| +31.559 s | +26.709 s | C | `0x00006300` | KeepAlive | 4 B | |
| +31.607 s | +26.757 s | C | `0x1401CE86` | UnknownNoise | 58 B ×1 |  |
| +31.755 s | +26.905 s | C | `0x14016087` | UnknownNoise | 58 B ×1 |  |
| +31.905 s | +27.055 s | C | `0x1401F587` | UnknownNoise | 58 B ×1 |  |
| +32.048 s | +27.197 s | C | `0x14018588` | UnknownNoise | 58 B ×1 |  |
| +32.196 s | +27.345 s | C | `0x14011A89` | UnknownNoise | 58 B ×1 |  |
| +32.344 s | +27.493 s | C | `0x1401AD89` | UnknownNoise | 58 B ×1 |  |
| +32.515 s | +27.665 s | C | `0x1401588A` | UnknownNoise | 58 B ×1 |  |
| +32.564 s | +27.713 s | C | `0x00006300` | KeepAlive | 4 B | |
| +32.663 s | +27.813 s | C | `0x1401EC8A` | UnknownNoise | 58 B ×1 |  |
| +32.687 s | +27.836 s | C | `0x00006B00` | ClientMetrics | 172 B | |
| +32.810 s | +27.959 s | C | `0x1401808B` | UnknownNoise | 58 B ×1 |  |
| +32.882 s | +28.032 s | C | `0x0000C300` | Synchronization | 49 B | |
| +32.959 s | +28.108 s | C | `0x1401158C` | UnknownNoise | 58 B ×1 |  |
| +33.061 s | +28.211 s | S | `0x0000C300` | Synchronization | 49 B | |
| +33.084 s | +28.234 s | C | `0x0000C300` | Synchronization | 49 B | |
| +33.108 s | +28.258 s | C | `0x1401958C` | UnknownNoise | 58 B ×1 |  |
| +33.259 s | +28.409 s | C | `0x14012B8D` | UnknownNoise | 58 B ×1 |  |
| +33.403 s | +28.553 s | C | `0x1401BD8D` | UnknownNoise | 58 B ×1 |  |
| +33.552 s | +28.702 s | C | `0x1401528E` | UnknownNoise | 58 B ×1 |  |
| +33.576 s | +28.726 s | C | `0x00006300` | KeepAlive | 4 B | |
| +33.702 s | +28.852 s | C | `0x1401E98E` | UnknownNoise | 58 B ×1 |  |
| +33.851 s | +29.001 s | C | `0x14017D8F` | UnknownNoise | 58 B ×1 |  |
| +34.000 s | +29.150 s | C | `0x14011190` | UnknownNoise | 58 B ×1 |  |
| +34.147 s | +29.297 s | C | `0x1401A590` | UnknownNoise | 58 B ×1 |  |
| +34.297 s | +29.447 s | C | `0x14013B91` | UnknownNoise | 58 B ×1 |  |
| +34.443 s | +29.593 s | C | `0x1401CE91` | UnknownNoise | 58 B ×1 |  |
| +34.591 s | +29.741 s | C | `0x00006300` | KeepAlive | 4 B | |
| +34.591 s | +29.741 s | C | `0x14016392` | UnknownNoise | 58 B ×1 |  |
| +34.765 s | +29.915 s | C | `0x14010E93` | UnknownNoise | 58 B ×1 |  |
| +34.911 s | +30.061 s | C | `0x1401A293` | UnknownNoise | 58 B ×1 |  |
| +35.061 s | +30.210 s | C | `0x14013794` | UnknownNoise | 58 B ×1 |  |
| +35.211 s | +30.361 s | C | `0x1401CC94` | UnknownNoise | 58 B ×1 |  |
| +35.357 s | +30.507 s | C | `0x14015F95` | UnknownNoise | 58 B ×1 |  |
| +35.505 s | +30.655 s | C | `0x1401F395` | UnknownNoise | 58 B ×1 |  |
| +35.603 s | +30.752 s | C | `0x00006300` | KeepAlive | 4 B | |
| +35.652 s | +30.802 s | C | `0x14018696` | UnknownNoise | 58 B ×1 |  |
| +35.781 s | +30.930 s | S | `0x00008F00` | ActivityManagerBase | 9 B | |
| +35.789 s | +30.938 s | C | `0x14011997` | UnknownNoise | 58 B ×1 |  |
| +35.947 s | +31.097 s | C | `0x1401AD97` | UnknownNoise | 58 B ×1 |  |
| +36.095 s | +31.244 s | C | `0x14014198` | UnknownNoise | 58 B ×1 |  |
| +36.243 s | +31.392 s | C | `0x1401D598` | UnknownNoise | 58 B ×1 |  |
| +36.391 s | +31.541 s | C | `0x14016A99` | UnknownNoise | 58 B ×1 |  |
| +36.566 s | +31.716 s | C | `0x1401189A` | UnknownNoise | 58 B ×1 |  |
| +36.613 s | +31.763 s | C | `0x00006300` | KeepAlive | 4 B | |
| +36.710 s | +31.860 s | C | `0x1401AA9A` | UnknownNoise | 58 B ×1 |  |
| +36.859 s | +32.009 s | C | `0x14013E9B` | UnknownNoise | 58 B ×1 |  |
| +37.007 s | +32.157 s | C | `0x1401D19B` | UnknownNoise | 58 B ×1 |  |
| +37.154 s | +32.304 s | C | `0x1401659C` | UnknownNoise | 58 B ×1 |  |
| +37.304 s | +32.454 s | C | `0x1401F99C` | UnknownNoise | 58 B ×1 |  |
| +37.450 s | +32.600 s | C | `0x14018C9D` | UnknownNoise | 58 B ×1 |  |
| +37.597 s | +32.747 s | C | `0x14011F9E` | UnknownNoise | 58 B ×1 |  |
| +37.622 s | +32.772 s | C | `0x00006300` | KeepAlive | 4 B | |
| +37.746 s | +32.896 s | C | `0x1401B49E` | UnknownNoise | 58 B ×1 |  |
| +37.894 s | +33.044 s | C | `0x0000C300` | Synchronization | 49 B | |
| +37.894 s | +33.044 s | C | `0x1401489F` | UnknownNoise | 58 B ×1 |  |
| +38.043 s | +33.193 s | C | `0x1401DC9F` | UnknownNoise | 58 B ×1 |  |
| +38.081 s | +33.231 s | C | `0x0000C300` | Synchronization | 49 B | |
| +38.215 s | +33.365 s | C | `0x140164A0` | UnknownNoise | 58 B ×1 |  |
| +38.362 s | +33.512 s | C | `0x1401F5A0` | UnknownNoise | 58 B ×1 |  |
| +38.509 s | +33.659 s | C | `0x140188A1` | UnknownNoise | 58 B ×1 |  |
| +38.630 s | +33.780 s | C | `0x00006300` | KeepAlive | 4 B | |
| +38.655 s | +33.805 s | C | `0x14011CA2` | UnknownNoise | 58 B ×1 |  |
| +38.754 s | +33.904 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +38.804 s | +33.954 s | C | `0x1401B0A2` | UnknownNoise | 58 B ×1 |  |
| +38.950 s | +34.100 s | C | `0x140144A3` | UnknownNoise | 58 B ×1 |  |
| +39.098 s | +34.248 s | C | `0x1401D7A3` | UnknownNoise | 58 B ×1 |  |
| +39.244 s | +34.394 s | C | `0x14016AA4` | UnknownNoise | 58 B ×1 |  |
| +39.393 s | +34.543 s | C | `0x1401FDA4` | UnknownNoise | 58 B ×1 |  |
| +39.542 s | +34.692 s | C | `0x140191A5` | UnknownNoise | 58 B ×1 |  |
| +39.640 s | +34.789 s | C | `0x00006300` | KeepAlive | 4 B | |
| +39.714 s | +34.863 s | C | `0x14013DA6` | UnknownNoise | 58 B ×1 |  |
| +39.863 s | +35.012 s | C | `0x1401D2A6` | UnknownNoise | 58 B ×1 |  |
| +40.008 s | +35.158 s | C | `0x140165A7` | UnknownNoise | 58 B ×1 |  |
| +40.161 s | +35.310 s | C | `0x1401FCA7` | UnknownNoise | 58 B ×1 |  |
| +40.308 s | +35.457 s | C | `0x14018FA8` | UnknownNoise | 58 B ×1 |  |
| +40.454 s | +35.603 s | C | `0x140122A9` | UnknownNoise | 58 B ×1 |  |
| +40.602 s | +35.752 s | C | `0x1401B5A9` | UnknownNoise | 58 B ×1 |  |
| +40.649 s | +35.799 s | C | `0x00006300` | KeepAlive | 4 B | |
| +40.748 s | +35.898 s | C | `0x140148AA` | UnknownNoise | 58 B ×1 |  |
| +40.895 s | +36.045 s | C | `0x1401DBAA` | UnknownNoise | 58 B ×1 |  |
| +41.044 s | +36.193 s | C | `0x14016FAB` | UnknownNoise | 58 B ×1 |  |
| +41.214 s | +36.363 s | C | `0x14011BAC` | UnknownNoise | 58 B ×1 |  |
| +41.362 s | +36.511 s | C | `0x1401AEAC` | UnknownNoise | 58 B ×1 |  |
| +41.509 s | +36.658 s | C | `0x140141AD` | UnknownNoise | 58 B ×1 |  |
| +41.656 s | +36.805 s | C | `0x00006300` | KeepAlive | 4 B | |
| +41.656 s | +36.805 s | C | `0x1401D4AD` | UnknownNoise | 58 B ×1 |  |
| +41.807 s | +36.956 s | C | `0x14016BAE` | UnknownNoise | 58 B ×1 |  |
| +41.955 s | +37.104 s | C | `0x1401FEAE` | UnknownNoise | 58 B ×1 |  |
| +42.101 s | +37.251 s | C | `0x140190AF` | UnknownNoise | 58 B ×1 |  |
| +42.248 s | +37.397 s | C | `0x140123B0` | UnknownNoise | 58 B ×1 |  |
| +42.394 s | +37.543 s | C | `0x1401B6B0` | UnknownNoise | 58 B ×1 |  |
| +42.542 s | +37.691 s | C | `0x14014AB1` | UnknownNoise | 58 B ×1 |  |
| +42.666 s | +37.816 s | C | `0x00006300` | KeepAlive | 4 B | |
| +42.714 s | +37.864 s | C | `0x1401F6B1` | UnknownNoise | 58 B ×1 |  |
| +42.861 s | +38.011 s | C | `0x14018AB2` | UnknownNoise | 58 B ×1 |  |
| +42.909 s | +38.059 s | C | `0x0000C300` | Synchronization | 49 B | |
| +43.008 s | +38.158 s | C | `0x14011DB3` | UnknownNoise | 58 B ×1 |  |
| +43.075 s | +38.225 s | C | `0x0000C300` | Synchronization | 49 B | |
| +43.163 s | +38.313 s | C | `0x1401C7B3` | UnknownNoise | 58 B ×1 |  |
| +43.311 s | +38.461 s | C | `0x140159B4` | UnknownNoise | 58 B ×1 |  |
| +43.457 s | +38.607 s | C | `0x1401EDB4` | UnknownNoise | 58 B ×1 |  |
| +43.607 s | +38.757 s | C | `0x140181B5` | UnknownNoise | 58 B ×1 |  |
| +43.680 s | +38.830 s | C | `0x00006300` | KeepAlive | 4 B | |
| +43.753 s | +38.903 s | C | `0x140115B6` | UnknownNoise | 58 B ×1 |  |
| +43.900 s | +39.050 s | C | `0x1401A8B6` | UnknownNoise | 58 B ×1 |  |
| +44.051 s | +39.201 s | C | `0x14013DB7` | UnknownNoise | 58 B ×1 |  |
| +44.197 s | +39.347 s | C | `0x1401D0B7` | UnknownNoise | 58 B ×1 |  |
| +44.343 s | +39.493 s | C | `0x140163B8` | UnknownNoise | 58 B ×1 |  |
| +44.515 s | +39.665 s | C | `0x14010EB9` | UnknownNoise | 58 B ×1 |  |
| +44.662 s | +39.812 s | C | `0x1401A1B9` | UnknownNoise | 58 B ×1 |  |
| +44.686 s | +39.836 s | C | `0x00006300` | KeepAlive | 4 B | |
| +44.810 s | +39.960 s | C | `0x140135BA` | UnknownNoise | 58 B ×1 |  |
| +44.958 s | +40.108 s | C | `0x1401CABA` | UnknownNoise | 58 B ×1 |  |
| +45.109 s | +40.258 s | C | `0x14015EBB` | UnknownNoise | 58 B ×1 |  |
| +45.256 s | +40.405 s | C | `0x1401F2BB` | UnknownNoise | 58 B ×1 |  |
| +45.402 s | +40.551 s | C | `0x140186BC` | UnknownNoise | 58 B ×1 |  |
| +45.552 s | +40.701 s | C | `0x14011BBD` | UnknownNoise | 58 B ×1 |  |
| +45.698 s | +40.847 s | C | `0x00006300` | KeepAlive | 4 B | |
| +45.698 s | +40.847 s | C | `0x1401ADBD` | UnknownNoise | 58 B ×1 |  |
| +45.843 s | +40.993 s | C | `0x140140BE` | UnknownNoise | 58 B ×1 |  |
| +45.991 s | +41.141 s | C | `0x1401D3BE` | UnknownNoise | 58 B ×1 |  |
| +46.164 s | +41.313 s | C | `0x14017FBF` | UnknownNoise | 58 B ×1 |  |
| +46.314 s | +41.464 s | C | `0x140114C0` | UnknownNoise | 58 B ×1 |  |
| +46.459 s | +41.609 s | C | `0x1401A7C0` | UnknownNoise | 58 B ×1 |  |
| +46.607 s | +41.757 s | C | `0x14013AC1` | UnknownNoise | 58 B ×1 |  |
| +46.704 s | +41.854 s | C | `0x00006300` | KeepAlive | 4 B | |
| +46.755 s | +41.905 s | C | `0x1401CDC1` | UnknownNoise | 58 B ×1 |  |
| +46.900 s | +42.050 s | C | `0x140160C2` | UnknownNoise | 58 B ×1 |  |
| +47.050 s | +42.200 s | C | `0x1401F4C2` | UnknownNoise | 58 B ×1 |  |
| +47.196 s | +42.346 s | C | `0x140188C3` | UnknownNoise | 58 B ×1 |  |
| +47.343 s | +42.493 s | C | `0x14011BC4` | UnknownNoise | 58 B ×1 |  |
| +47.492 s | +42.642 s | C | `0x1401AEC4` | UnknownNoise | 58 B ×1 |  |
| +47.663 s | +42.813 s | C | `0x14015BC5` | UnknownNoise | 58 B ×1 |  |
| +47.688 s | +42.838 s | C | `0x00006B00` | ClientMetrics | 172 B | |
| +47.714 s | +42.864 s | C | `0x00006300` | KeepAlive | 4 B | |
| +47.811 s | +42.961 s | C | `0x1401EFC5` | UnknownNoise | 58 B ×1 |  |
| +47.935 s | +43.085 s | C | `0x0000C300` | Synchronization | 49 B | |
| +47.958 s | +43.108 s | C | `0x140182C6` | UnknownNoise | 58 B ×1 |  |
| +48.107 s | +43.257 s | C | `0x140115C7` | UnknownNoise | 58 B ×1 |  |
| +48.130 s | +43.280 s | C | `0x0000C300` | Synchronization | 49 B | |
| +48.253 s | +43.403 s | C | `0x140197C7` | UnknownNoise | 58 B ×1 |  |
| +48.398 s | +43.548 s | C | `0x14012AC8` | UnknownNoise | 58 B ×1 |  |
| +48.547 s | +43.697 s | C | `0x1401BFC8` | UnknownNoise | 58 B ×1 |  |
| +48.693 s | +43.843 s | C | `0x140151C9` | UnknownNoise | 58 B ×1 |  |
| +48.718 s | +43.868 s | C | `0x00006300` | KeepAlive | 4 B | |
| +48.841 s | +43.991 s | C | `0x1401E5C9` | UnknownNoise | 58 B ×1 |  |
| +48.992 s | +44.142 s | C | `0x14017CCA` | UnknownNoise | 58 B ×1 |  |
| +49.141 s | +44.290 s | C | `0x140110CB` | UnknownNoise | 58 B ×1 |  |
| +49.315 s | +44.464 s | C | `0x1401BFCB` | UnknownNoise | 58 B ×1 |  |
| +49.464 s | +44.613 s | C | `0x140153CC` | UnknownNoise | 58 B ×1 |  |
| +49.612 s | +44.762 s | C | `0x1401E7CC` | UnknownNoise | 58 B ×1 |  |
| +49.737 s | +44.886 s | C | `0x00006300` | KeepAlive | 4 B | |
| +49.737 s | +44.886 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +49.759 s | +44.909 s | C | `0x14017BCD` | UnknownNoise | 58 B ×1 |  |
| +49.907 s | +45.056 s | C | `0x14010FCE` | UnknownNoise | 58 B ×1 |  |
| +50.057 s | +45.206 s | C | `0x1401A4CE` | UnknownNoise | 58 B ×1 |  |
| +50.206 s | +45.356 s | C | `0x140139CF` | UnknownNoise | 58 B ×1 |  |
| +50.354 s | +45.504 s | C | `0x1401CDCF` | UnknownNoise | 58 B ×1 |  |
| +50.501 s | +45.650 s | C | `0x140161D0` | UnknownNoise | 58 B ×1 |  |
| +50.648 s | +45.798 s | C | `0x1401F4D0` | UnknownNoise | 58 B ×1 |  |
| +50.747 s | +45.897 s | C | `0x00006300` | KeepAlive | 4 B | |
| +50.798 s | +45.948 s | C | `0x14018AD1` | UnknownNoise | 58 B ×1 |  |
| +50.948 s | +46.098 s | C | `0x14011FD2` | UnknownNoise | 58 B ×1 |  |
| +51.095 s | +46.245 s | C | `0x1401B2D2` | UnknownNoise | 58 B ×1 |  |
| +51.242 s | +46.392 s | C | `0x140146D3` | UnknownNoise | 58 B ×1 |  |
| +51.414 s | +46.564 s | C | `0x1401F1D3` | UnknownNoise | 58 B ×1 |  |
| +51.564 s | +46.714 s | C | `0x140187D4` | UnknownNoise | 58 B ×1 |  |
| +51.710 s | +46.860 s | C | `0x14011BD5` | UnknownNoise | 58 B ×1 |  |
| +51.759 s | +46.909 s | C | `0x00006300` | KeepAlive | 4 B | |
| +51.857 s | +47.007 s | C | `0x1401AED5` | UnknownNoise | 58 B ×1 |  |
| +52.007 s | +47.157 s | C | `0x140142D6` | UnknownNoise | 58 B ×1 |  |
| +52.154 s | +47.304 s | C | `0x1401D5D6` | UnknownNoise | 58 B ×1 |  |
| +52.303 s | +47.453 s | C | `0x14016BD7` | UnknownNoise | 58 B ×1 |  |
| +52.453 s | +47.603 s | C | `0x140101D8` | UnknownNoise | 58 B ×1 |  |
| +52.602 s | +47.751 s | C | `0x140194D8` | UnknownNoise | 58 B ×1 |  |
| +52.677 s | +47.827 s | C | `0x00001300` | ChatBase | 112 B | |
| +52.751 s | +47.901 s | C | `0x14012AD9` | UnknownNoise | 58 B ×1 |  |
| +52.774 s | +47.924 s | C | `0x00006300` | KeepAlive | 4 B | |
| +52.898 s | +48.048 s | C | `0x1401BDD9` | UnknownNoise | 58 B ×1 |  |
| +52.944 s | +48.094 s | C | `0x0000C300` | Synchronization | 49 B | |
| +53.049 s | +48.198 s | C | `0x140153DA` | UnknownNoise | 58 B ×1 |  |
| +53.133 s | +48.283 s | C | `0x0000C300` | Synchronization | 49 B | |
| +53.192 s | +48.342 s | C | `0x1401E2DA` | UnknownNoise | 58 B ×1 |  |
| +53.341 s | +48.491 s | C | `0x140176DB` | UnknownNoise | 58 B ×1 |  |
| +53.512 s | +48.662 s | C | `0x140121DC` | UnknownNoise | 58 B ×1 |  |
| +53.660 s | +48.810 s | C | `0x1401B4DC` | UnknownNoise | 58 B ×1 |  |
| +53.785 s | +48.935 s | C | `0x00006300` | KeepAlive | 4 B | |
| +53.808 s | +48.958 s | C | `0x140149DD` | UnknownNoise | 58 B ×1 |  |
| +53.956 s | +49.105 s | C | `0x1401DCDD` | UnknownNoise | 58 B ×1 |  |
| +54.103 s | +49.252 s | C | `0x140170DE` | UnknownNoise | 58 B ×1 |  |
| +54.249 s | +49.399 s | C | `0x140103DF` | UnknownNoise | 58 B ×1 |  |
| +54.396 s | +49.546 s | C | `0x140196DF` | UnknownNoise | 58 B ×1 |  |
| +54.550 s | +49.700 s | C | `0x14012FE0` | UnknownNoise | 58 B ×1 |  |
| +54.699 s | +49.849 s | C | `0x1401C3E0` | UnknownNoise | 58 B ×1 |  |
| +54.795 s | +49.945 s | C | `0x00006300` | KeepAlive | 4 B | |
| +54.846 s | +49.996 s | C | `0x140159E1` | UnknownNoise | 58 B ×1 |  |
| +54.994 s | +50.144 s | C | `0x1401EAE1` | UnknownNoise | 58 B ×1 |  |
| +55.142 s | +50.292 s | C | `0x14017FE2` | UnknownNoise | 58 B ×1 |  |
| +55.291 s | +50.441 s | C | `0x140114E3` | UnknownNoise | 58 B ×1 |  |
| +55.464 s | +50.614 s | C | `0x1401C0E3` | UnknownNoise | 58 B ×1 |  |
| +55.611 s | +50.761 s | C | `0x140154E4` | UnknownNoise | 58 B ×1 |  |
| +55.760 s | +50.910 s | C | `0x1401E8E4` | UnknownNoise | 58 B ×1 |  |
| +55.809 s | +50.959 s | C | `0x00006300` | KeepAlive | 4 B | |
| +55.906 s | +51.056 s | C | `0x14017DE5` | UnknownNoise | 58 B ×1 |  |
| +56.056 s | +51.206 s | C | `0x140113E6` | UnknownNoise | 58 B ×1 |  |
| +56.205 s | +51.355 s | C | `0x1401A6E6` | UnknownNoise | 58 B ×1 |  |
| +56.354 s | +51.504 s | C | `0x14013BE7` | UnknownNoise | 58 B ×1 |  |
| +56.499 s | +51.649 s | C | `0x1401CDE7` | UnknownNoise | 58 B ×1 |  |
| +56.647 s | +51.797 s | C | `0x14015FE8` | UnknownNoise | 58 B ×1 |  |
| +56.794 s | +51.944 s | C | `0x1401F4E8` | UnknownNoise | 58 B ×1 |  |
| +56.819 s | +51.969 s | C | `0x00006300` | KeepAlive | 4 B | |
| +56.943 s | +52.093 s | C | `0x140188E9` | UnknownNoise | 58 B ×1 |  |
| +57.116 s | +52.265 s | C | `0x140134EA` | UnknownNoise | 58 B ×1 |  |
| +57.263 s | +52.412 s | C | `0x1401C7EA` | UnknownNoise | 58 B ×1 |  |
| +57.411 s | +52.560 s | C | `0x14015CEB` | UnknownNoise | 58 B ×1 |  |
| +57.561 s | +52.710 s | C | `0x1401F0EB` | UnknownNoise | 58 B ×1 |  |
| +57.707 s | +52.856 s | C | `0x140183EC` | UnknownNoise | 58 B ×1 |  |
| +57.830 s | +52.980 s | C | `0x00006300` | KeepAlive | 4 B | |
| +57.855 s | +53.004 s | C | `0x140118ED` | UnknownNoise | 58 B ×1 |  |
| +57.954 s | +53.103 s | C | `0x0000C300` | Synchronization | 49 B | |
| +58.002 s | +53.152 s | C | `0x1401ACED` | UnknownNoise | 58 B ×1 |  |
| +58.149 s | +53.299 s | C | `0x14013FEE` | UnknownNoise | 58 B ×1 |  |
| +58.175 s | +53.325 s | C | `0x0000C300` | Synchronization | 49 B | |
| +58.297 s | +53.447 s | C | `0x1401C3EE` | UnknownNoise | 58 B ×1 |  |
| +58.447 s | +53.596 s | C | `0x140157EF` | UnknownNoise | 58 B ×1 |  |
| +58.594 s | +53.744 s | C | `0x1401ECEF` | UnknownNoise | 58 B ×1 |  |
| +58.744 s | +53.893 s | C | `0x140180F0` | UnknownNoise | 58 B ×1 |  |
| +58.842 s | +53.992 s | C | `0x00006300` | KeepAlive | 4 B | |
| +58.893 s | +54.043 s | C | `0x140114F1` | UnknownNoise | 58 B ×1 |  |
| +59.041 s | +54.191 s | C | `0x1401A9F1` | UnknownNoise | 58 B ×1 |  |
| +59.213 s | +54.363 s | C | `0x140156F2` | UnknownNoise | 58 B ×1 |  |
| +59.364 s | +54.514 s | C | `0x1401EBF2` | UnknownNoise | 58 B ×1 |  |
| +59.511 s | +54.661 s | C | `0x140180F3` | UnknownNoise | 58 B ×1 |  |
| +59.662 s | +54.812 s | C | `0x140116F4` | UnknownNoise | 58 B ×1 |  |
| +59.809 s | +54.959 s | C | `0x1401AAF4` | UnknownNoise | 58 B ×1 |  |
| +59.860 s | +55.010 s | C | `0x00006300` | KeepAlive | 4 B | |
| +59.958 s | +55.108 s | C | `0x14013FF5` | UnknownNoise | 58 B ×1 |  |
| +60.104 s | +55.254 s | C | `0x1401D2F5` | UnknownNoise | 58 B ×1 |  |
| +60.255 s | +55.405 s | C | `0x140167F6` | UnknownNoise | 58 B ×1 |  |
| +60.402 s | +55.552 s | C | `0x1401FAF6` | UnknownNoise | 58 B ×1 |  |
| +60.551 s | +55.701 s | C | `0x14018FF7` | UnknownNoise | 58 B ×1 |  |
| +60.699 s | +55.848 s | C | `0x140123F8` | UnknownNoise | 58 B ×1 |  |
| +60.747 s | +55.897 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +60.849 s | +55.999 s | C | `0x1401B8F8` | UnknownNoise | 58 B ×1 |  |
| +60.873 s | +56.023 s | C | `0x00006300` | KeepAlive | 4 B | |
| +60.995 s | +56.145 s | C | `0x14014DF9` | UnknownNoise | 58 B ×1 |  |
| +61.146 s | +56.296 s | C | `0x1401E2F9` | UnknownNoise | 58 B ×1 |  |
| +61.298 s | +56.447 s | C | `0x14017AFA` | UnknownNoise | 58 B ×1 |  |
| +61.452 s | +56.601 s | C | `0x140114FB` | UnknownNoise | 58 B ×1 |  |
| +61.601 s | +56.750 s | C | `0x1401A7FB` | UnknownNoise | 58 B ×1 |  |
| +61.751 s | +56.900 s | C | `0x14013DFC` | UnknownNoise | 58 B ×1 |  |
| +61.897 s | +57.046 s | C | `0x00006300` | KeepAlive | 4 B | |
| +61.897 s | +57.046 s | C | `0x1401D1FC` | UnknownNoise | 58 B ×1 |  |
| +62.046 s | +57.196 s | C | `0x140168FD` | UnknownNoise | 58 B ×1 |  |
| +62.197 s | +57.347 s | C | `0x1401FEFD` | UnknownNoise | 58 B ×1 |  |
| +62.347 s | +57.496 s | C | `0x140193FE` | UnknownNoise | 58 B ×1 |  |
| +62.497 s | +57.647 s | C | `0x140129FF` | UnknownNoise | 58 B ×1 |  |
| +62.645 s | +57.795 s | C | `0x1401BDFF` | UnknownNoise | 58 B ×1 |  |
| +62.694 s | +57.844 s | C | `0x00006B00` | ClientMetrics | 172 B | |
| +62.795 s | +57.945 s | C | `0x14015300` | UnknownNoise | 58 B ×1 |  |
| +62.916 s | +58.066 s | C | `0x00006300` | KeepAlive | 4 B | |
| +62.943 s | +58.093 s | C | `0x1401E800` | UnknownNoise | 58 B ×1 |  |
| +62.966 s | +58.116 s | C | `0x0000C300` | Synchronization | 49 B | |
| +63.090 s | +58.240 s | C | `0x14017A01` | UnknownNoise | 58 B ×1 |  |
| +63.159 s | +58.309 s | C | `0x0000C300` | Synchronization | 49 B | |
| +63.263 s | +58.413 s | C | `0x14012802` | UnknownNoise | 58 B ×1 |  |
| +63.415 s | +58.565 s | C | `0x1401BE02` | UnknownNoise | 58 B ×1 |  |
| +63.563 s | +58.713 s | C | `0x14015303` | UnknownNoise | 58 B ×1 |  |
| +63.710 s | +58.860 s | C | `0x1401E703` | UnknownNoise | 58 B ×1 |  |
| +63.861 s | +59.011 s | C | `0x14017C04` | UnknownNoise | 58 B ×1 |  |
| +63.934 s | +59.084 s | C | `0x00006300` | KeepAlive | 4 B | |
| +64.008 s | +59.158 s | C | `0x14011005` | UnknownNoise | 58 B ×1 |  |
| +64.157 s | +59.307 s | C | `0x1401A405` | UnknownNoise | 58 B ×1 |  |
| +64.304 s | +59.454 s | C | `0x14013906` | UnknownNoise | 58 B ×1 |  |
| +64.452 s | +59.602 s | C | `0x1401CD06` | UnknownNoise | 58 B ×1 |  |
| +64.601 s | +59.750 s | C | `0x14016107` | UnknownNoise | 58 B ×1 |  |
| +64.750 s | +59.899 s | C | `0x1401F607` | UnknownNoise | 58 B ×1 |  |
| +64.897 s | +60.046 s | C | `0x14018A08` | UnknownNoise | 58 B ×1 |  |
| +64.946 s | +60.096 s | C | `0x00006300` | KeepAlive | 4 B | |
| +65.046 s | +60.195 s | C | `0x14011D09` | UnknownNoise | 58 B ×1 |  |
| +65.195 s | +60.344 s | C | `0x1401B109` | UnknownNoise | 58 B ×1 |  |
| +65.342 s | +60.491 s | C | `0x1401460A` | UnknownNoise | 58 B ×1 |  |
| +65.517 s | +60.666 s | C | `0x1401F50A` | UnknownNoise | 58 B ×1 |  |
| +65.642 s | +60.792 s | C | `0x1401720B` | UnknownNoise | 58 B ×1 |  |
| +65.816 s | +60.966 s | C | `0x14011F0C` | UnknownNoise | 58 B ×1 |  |
| +65.961 s | +61.111 s | C | `0x00006300` | KeepAlive | 4 B | |
| +65.961 s | +61.111 s | C | `0x1401B20C` | UnknownNoise | 58 B ×1 |  |
| +66.110 s | +61.260 s | C | `0x1401470D` | UnknownNoise | 58 B ×1 |  |
| +66.258 s | +61.408 s | C | `0x1401DA0D` | UnknownNoise | 58 B ×1 |  |
| +66.408 s | +61.558 s | C | `0x1401700E` | UnknownNoise | 58 B ×1 |  |
| +66.556 s | +61.706 s | C | `0x1401040F` | UnknownNoise | 58 B ×1 |  |
| +66.702 s | +61.852 s | C | `0x1401960F` | UnknownNoise | 58 B ×1 |  |
| +66.782 s | +61.932 s | S | `0x22254CC0` | UnknownNoise | 7 B ×1 |  |
| +66.850 s | +62.000 s | C | `0x14012A10` | UnknownNoise | 58 B ×1 |  |
| +66.974 s | +62.124 s | C | `0x00006300` | KeepAlive | 4 B | |
| +67.000 s | +62.150 s | C | `0x1401C110` | UnknownNoise | 58 B ×1 |  |
| +67.149 s | +62.299 s | C | `0x14015611` | UnknownNoise | 58 B ×1 |  |
| +67.297 s | +62.447 s | C | `0x1401EA11` | UnknownNoise | 58 B ×1 |  |
| +67.443 s | +62.593 s | C | `0x14017D12` | UnknownNoise | 58 B ×1 |  |
| +67.592 s | +62.742 s | C | `0x14011113` | UnknownNoise | 58 B ×1 |  |
| +67.715 s | +62.865 s | C | `0x00004500` | InGamePurchaseBase | 2 B | |
| +67.767 s | +62.917 s | C | `0x1401BF13` | UnknownNoise | 58 B ×1 |  |
| +67.892 s | +63.042 s | C | `0x14013B14` | UnknownNoise | 58 B ×1 |  |
| +67.989 s | +63.139 s | C | `0x00006300` | KeepAlive | 4 B | |
| +67.989 s | +63.139 s | C | `0x0000C300` | Synchronization | 49 B | |
| +68.053 s | +63.203 s | C | `0x1401E814` | UnknownNoise | 58 B ×1 |  |
| +68.151 s | +63.301 s | C | `0x0000C300` | Synchronization | 49 B | |
| +68.210 s | +63.360 s | C | `0x14017A15` | UnknownNoise | 58 B ×1 |  |
| +68.359 s | +63.509 s | C | `0x14010E16` | UnknownNoise | 58 B ×1 |  |
| +68.507 s | +63.657 s | C | `0x1401A216` | UnknownNoise | 58 B ×1 |  |
| +68.656 s | +63.806 s | C | `0x14013817` | UnknownNoise | 58 B ×1 |  |
| +68.805 s | +63.955 s | C | `0x1401CD17` | UnknownNoise | 58 B ×1 |  |
| +68.954 s | +64.104 s | C | `0x14016118` | UnknownNoise | 58 B ×1 |  |
| +69.001 s | +64.150 s | C | `0x00006300` | KeepAlive | 4 B | |
| +69.101 s | +64.251 s | C | `0x1401F518` | UnknownNoise | 58 B ×1 |  |
| +69.249 s | +64.398 s | C | `0x14018A19` | UnknownNoise | 58 B ×1 |  |
| +69.398 s | +64.547 s | C | `0x14011D1A` | UnknownNoise | 58 B ×1 |  |
| +69.547 s | +64.696 s | C | `0x1401B21A` | UnknownNoise | 58 B ×1 |  |
| +69.694 s | +64.843 s | C | `0x1401451B` | UnknownNoise | 58 B ×1 |  |
| +69.864 s | +65.014 s | C | `0x1401F11B` | UnknownNoise | 58 B ×1 |  |
| +70.014 s | +65.164 s | C | `0x00006300` | KeepAlive | 4 B | |
| +70.014 s | +65.164 s | C | `0x1401861C` | UnknownNoise | 58 B ×1 |  |
| +70.165 s | +65.314 s | C | `0x14011C1D` | UnknownNoise | 58 B ×1 |  |
| +70.312 s | +65.462 s | C | `0x1401B01D` | UnknownNoise | 58 B ×1 |  |
| +70.462 s | +65.612 s | C | `0x1401471E` | UnknownNoise | 58 B ×1 |  |
| +70.611 s | +65.761 s | C | `0x1401DB1E` | UnknownNoise | 58 B ×1 |  |
| +70.759 s | +65.909 s | C | `0x14016F1F` | UnknownNoise | 58 B ×1 |  |
| +70.909 s | +66.059 s | C | `0x14010520` | UnknownNoise | 58 B ×1 |  |
| +71.032 s | +66.182 s | C | `0x00006300` | KeepAlive | 4 B | |
| +71.055 s | +66.205 s | C | `0x14019920` | UnknownNoise | 58 B ×1 |  |
| +71.207 s | +66.357 s | C | `0x14012F21` | UnknownNoise | 58 B ×1 |  |
| +71.354 s | +66.504 s | C | `0x1401C221` | UnknownNoise | 58 B ×1 |  |
| +71.504 s | +66.654 s | C | `0x14015822` | UnknownNoise | 58 B ×1 |  |
| +71.652 s | +66.802 s | C | `0x1401ED22` | UnknownNoise | 58 B ×1 |  |
| +71.751 s | +66.901 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +71.800 s | +66.950 s | C | `0x14018223` | UnknownNoise | 58 B ×1 |  |
| +71.950 s | +67.100 s | C | `0x14011624` | UnknownNoise | 58 B ×1 |  |
| +72.049 s | +67.199 s | C | `0x00006300` | KeepAlive | 4 B | |
| +72.098 s | +67.248 s | C | `0x1401AA24` | UnknownNoise | 58 B ×1 |  |
| +72.248 s | +67.398 s | C | `0x14014025` | UnknownNoise | 58 B ×1 |  |
| +72.395 s | +67.545 s | C | `0x1401D525` | UnknownNoise | 58 B ×1 |  |
| +72.545 s | +67.695 s | C | `0x14016926` | UnknownNoise | 58 B ×1 |  |
| +72.692 s | +67.842 s | C | `0x1401FD26` | UnknownNoise | 58 B ×1 |  |
| +72.841 s | +67.991 s | C | `0x14019227` | UnknownNoise | 58 B ×1 |  |
| +72.990 s | +68.139 s | C | `0x0000C300` | Synchronization | 49 B | |
| +73.016 s | +68.165 s | C | `0x14014028` | UnknownNoise | 58 B ×1 |  |
| +73.067 s | +68.216 s | C | `0x00006300` | KeepAlive | 4 B | |
| +73.165 s | +68.314 s | C | `0x1401D428` | UnknownNoise | 58 B ×1 |  |
| +73.178 s | +68.328 s | C | `0x0000C300` | Synchronization | 49 B | |
| +73.313 s | +68.462 s | C | `0x14016929` | UnknownNoise | 58 B ×1 |  |
| +73.462 s | +68.612 s | C | `0x1401FD29` | UnknownNoise | 58 B ×1 |  |
| +73.610 s | +68.759 s | C | `0x1401922A` | UnknownNoise | 58 B ×1 |  |
| +73.758 s | +68.908 s | C | `0x1401282B` | UnknownNoise | 58 B ×1 |  |
| +73.909 s | +69.059 s | C | `0x1401BD2B` | UnknownNoise | 58 B ×1 |  |
| +74.058 s | +69.208 s | C | `0x1401512C` | UnknownNoise | 58 B ×1 |  |
| +74.080 s | +69.230 s | C | `0x00006300` | KeepAlive | 4 B | |
| +74.205 s | +69.355 s | C | `0x1401E62C` | UnknownNoise | 58 B ×1 |  |
| +74.353 s | +69.503 s | C | `0x14017B2D` | UnknownNoise | 58 B ×1 |  |
| +74.505 s | +69.655 s | C | `0x1401112E` | UnknownNoise | 58 B ×1 |  |
| +74.652 s | +69.802 s | C | `0x1401A52E` | UnknownNoise | 58 B ×1 |  |
| +74.801 s | +69.951 s | C | `0x1401392F` | UnknownNoise | 58 B ×1 |  |
| +74.950 s | +70.100 s | C | `0x1401CD2F` | UnknownNoise | 58 B ×1 |  |
| +75.096 s | +70.246 s | C | `0x00006300` | KeepAlive | 4 B | |
| +75.096 s | +70.246 s | C | `0x14016230` | UnknownNoise | 58 B ×1 |  |
| +75.248 s | +70.398 s | C | `0x1401F930` | UnknownNoise | 58 B ×1 |  |
| +75.399 s | +70.549 s | C | `0x14018E31` | UnknownNoise | 58 B ×1 |  |
| +75.546 s | +70.696 s | C | `0x14012332` | UnknownNoise | 58 B ×1 |  |
| +75.697 s | +70.847 s | C | `0x1401BA32` | UnknownNoise | 58 B ×1 |  |
| +75.849 s | +70.999 s | C | `0x14015033` | UnknownNoise | 58 B ×1 |  |
| +75.997 s | +71.147 s | C | `0x1401E433` | UnknownNoise | 58 B ×1 |  |
| +76.122 s | +71.272 s | C | `0x00006300` | KeepAlive | 4 B | |
| +76.146 s | +71.296 s | C | `0x14017A34` | UnknownNoise | 58 B ×1 |  |
| +76.295 s | +71.444 s | C | `0x14010F35` | UnknownNoise | 58 B ×1 |  |
| +76.443 s | +71.592 s | C | `0x1401A335` | UnknownNoise | 58 B ×1 |  |
| +76.591 s | +71.741 s | C | `0x14013736` | UnknownNoise | 58 B ×1 |  |
| +76.765 s | +71.914 s | C | `0x1401E436` | UnknownNoise | 58 B ×1 |  |
| +76.912 s | +72.061 s | C | `0x14017837` | UnknownNoise | 58 B ×1 |  |
| +77.061 s | +72.211 s | C | `0x14010C38` | UnknownNoise | 58 B ×1 |  |
| +77.134 s | +72.284 s | C | `0x00006300` | KeepAlive | 4 B | |
| +77.209 s | +72.358 s | C | `0x1401A138` | UnknownNoise | 58 B ×1 |  |
| +77.357 s | +72.507 s | C | `0x14013539` | UnknownNoise | 58 B ×1 |  |
| +77.505 s | +72.655 s | C | `0x1401CB39` | UnknownNoise | 58 B ×1 |  |
| +77.656 s | +72.806 s | C | `0x1401603A` | UnknownNoise | 58 B ×1 |  |
| +77.704 s | +72.854 s | C | `0x00006B00` | ClientMetrics | 172 B | |
| +77.803 s | +72.953 s | C | `0x1401F33A` | UnknownNoise | 58 B ×1 |  |
| +77.950 s | +73.100 s | C | `0x1401873B` | UnknownNoise | 58 B ×1 |  |
| +77.999 s | +73.149 s | C | `0x0000C300` | Synchronization | 49 B | |
| +78.099 s | +73.249 s | C | `0x14011C3C` | UnknownNoise | 58 B ×1 |  |
| +78.148 s | +73.298 s | C | `0x00006300` | KeepAlive | 4 B | |
| +78.188 s | +73.338 s | C | `0x0000C300` | Synchronization | 49 B | |
| +78.248 s | +73.398 s | C | `0x1401AC3C` | UnknownNoise | 58 B ×1 |  |
| +78.399 s | +73.549 s | C | `0x1401433D` | UnknownNoise | 58 B ×1 |  |
| +78.548 s | +73.698 s | C | `0x1401D73D` | UnknownNoise | 58 B ×1 |  |
| +78.694 s | +73.844 s | C | `0x14016B3E` | UnknownNoise | 58 B ×1 |  |
| +78.845 s | +73.995 s | C | `0x1401003F` | UnknownNoise | 58 B ×1 |  |
| +78.994 s | +74.144 s | C | `0x1401973F` | UnknownNoise | 58 B ×1 |  |
| +79.145 s | +74.295 s | C | `0x14012C40` | UnknownNoise | 58 B ×1 |  |
| +79.169 s | +74.319 s | C | `0x00006300` | KeepAlive | 4 B | |
| +79.293 s | +74.443 s | C | `0x1401C140` | UnknownNoise | 58 B ×1 |  |
| +79.443 s | +74.593 s | C | `0x14015641` | UnknownNoise | 58 B ×1 |  |
| +79.594 s | +74.744 s | C | `0x1401ED41` | UnknownNoise | 58 B ×1 |  |
| +79.744 s | +74.894 s | C | `0x14018442` | UnknownNoise | 58 B ×1 |  |
| +79.895 s | +75.045 s | C | `0x14011B43` | UnknownNoise | 58 B ×1 |  |
| +80.045 s | +75.195 s | C | `0x1401B143` | UnknownNoise | 58 B ×1 |  |
| +80.172 s | +75.322 s | C | `0x00006300` | KeepAlive | 4 B | |
| +80.196 s | +75.345 s | C | `0x14014844` | UnknownNoise | 58 B ×1 |  |
| +80.347 s | +75.496 s | C | `0x1401DD44` | UnknownNoise | 58 B ×1 |  |
| +80.496 s | +75.645 s | C | `0x14017245` | UnknownNoise | 58 B ×1 |  |
| +80.648 s | +75.797 s | C | `0x14010A46` | UnknownNoise | 58 B ×1 |  |
| +80.796 s | +75.945 s | C | `0x14019F46` | UnknownNoise | 58 B ×1 |  |
| +80.945 s | +76.095 s | C | `0x14013547` | UnknownNoise | 58 B ×1 |  |
| +81.098 s | +76.248 s | C | `0x1401CD47` | UnknownNoise | 58 B ×1 |  |
| +81.174 s | +76.324 s | C | `0x00006300` | KeepAlive | 4 B | |
| +81.252 s | +76.401 s | C | `0x14016748` | UnknownNoise | 58 B ×1 |  |
| +81.403 s | +76.553 s | C | `0x1401FF48` | UnknownNoise | 58 B ×1 |  |
| +81.557 s | +76.707 s | C | `0x14019749` | UnknownNoise | 58 B ×1 |  |
| +81.708 s | +76.858 s | C | `0x14012F4A` | UnknownNoise | 58 B ×1 |  |
| +81.860 s | +77.010 s | C | `0x1401C54A` | UnknownNoise | 58 B ×1 |  |
| +82.011 s | +77.161 s | C | `0x14015E4B` | UnknownNoise | 58 B ×1 |  |
| +82.163 s | +77.313 s | C | `0x1401F54B` | UnknownNoise | 58 B ×1 |  |
| +82.186 s | +77.336 s | C | `0x00006300` | KeepAlive | 4 B | |
| +82.313 s | +77.463 s | C | `0x14018C4C` | UnknownNoise | 58 B ×1 |  |
| +82.462 s | +77.612 s | C | `0x1401214D` | UnknownNoise | 58 B ×1 |  |
| +82.614 s | +77.764 s | C | `0x1401B94D` | UnknownNoise | 58 B ×1 |  |
| +82.737 s | +77.887 s | C | `0x00003700` | GameTimeSync | 13 B | |
| +82.764 s | +77.914 s | C | `0x1401504E` | UnknownNoise | 58 B ×1 |  |
| +82.916 s | +78.066 s | C | `0x1401E74E` | UnknownNoise | 58 B ×1 |  |
| +83.013 s | +78.163 s | C | `0x0000C300` | Synchronization | 49 B | |
| +83.066 s | +78.216 s | C | `0x14017D4F` | UnknownNoise | 58 B ×1 |  |
| +83.192 s | +78.342 s | C | `0x00006300` | KeepAlive | 4 B | |
| +83.218 s | +78.368 s | C | `0x0000C300` | Synchronization | 49 B | |
| +83.218 s | +78.368 s | C | `0x14010F50` | UnknownNoise | 58 B ×1 |  |
| +83.241 s | +78.391 s | C | `0x04265000` | UnknownNoise | 10 B ×1 |  |
| +83.317 s | +78.467 s | C | `0x03725000` | UnknownNoise | 14 B ×1 |  |
| +83.343 s | +78.493 s | C | `0x048C5000` | UnknownNoise | 10 B ×1 |  |
| +83.343 s | +78.493 s | C | `0x14018C50` | UnknownNoise | 58 B ×1 |  |
| +83.424 s | +78.574 s | C | `0x04DE5000` | UnknownNoise | 10 B ×1 |  |
| +83.452 s | +78.602 s | C | `0x03F95000` | UnknownNoise | 14 B ×1 |  |
| +83.511 s | +78.661 s | C | `0x04345100` | UnknownNoise | 11 B ×1 |  |
| +83.511 s | +78.661 s | C | `0x14013551` | UnknownNoise | 58 B ×1 |  |
| +83.541 s | +78.691 s | C | `0x00525100` | UnknownNoise | 37 B ×1 |  |
| +83.631 s | +78.781 s | C | `0x01AD5100` | UnknownNoise | 5 B ×1 |  |
| +83.663 s | +78.813 s | C | `0x1401CD51` | UnknownNoise | 58 B ×1 |  |
| +83.694 s | +78.844 s | C | `0x02ED5100` | UnknownNoise | 18 B ×1 |  |
| +83.787 s | +78.937 s | C | `0x02485200` | UnknownNoise | 18 B ×1 |  |
| +83.819 s | +78.969 s | C | `0x14016652` | UnknownNoise | 58 B ×1 |  |
| +83.879 s | +79.029 s | C | `0x02A55200` | UnknownNoise | 18 B ×1 |  |
| +83.969 s | +79.119 s | C | `0x04FE5200` | UnknownNoise | 10 B ×1 |  |
| +83.969 s | +79.119 s | C | `0x1401FE52` | UnknownNoise | 58 B ×1 |  |
| +84.030 s | +79.180 s | C | `0x043C5300` | UnknownNoise | 11 B ×1 |  |
| +84.086 s | +79.235 s | C | `0x02735300` | UnknownNoise | 18 B ×1 |  |
| +84.119 s | +79.269 s | C | `0x14019453` | UnknownNoise | 58 B ×1 |  |
| +84.144 s | +79.294 s | C | `0x03AF5300` | UnknownNoise | 13 B ×1 |  |
| +84.174 s | +79.324 s | C | `0x04CB5300` | UnknownNoise | 9 B ×1 |  |
| +84.193 s | +79.342 s | C | `0x00006300` | KeepAlive | 4 B | |
| +84.193 s | +79.342 s | C | `0x02E25300` | UnknownNoise | 18 B ×1 |  |
| +84.260 s | +79.409 s | C | `0x14012054` | UnknownNoise | 58 B ×1 |  |
| +84.362 s | +79.511 s | C | `0x04865400` | UnknownNoise | 10 B ×1 |  |
| +84.385 s | +79.534 s | C | `0x02A25400` | UnknownNoise | 18 B ×1 |  |
| +84.385 s | +79.534 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +84.385 s | +79.534 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +84.385 s | +79.534 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +84.416 s | +79.566 s | C | `0x00BC5400` | UnknownNoise | 42 B ×1 |  |
| +84.416 s | +79.566 s | C | `0x1401BD54` | UnknownNoise | 58 B ×1 |  |
| +84.445 s | +79.595 s | C | `0x01DB5400` | UnknownNoise | 5 B ×1 |  |
| +84.445 s | +79.595 s | S | `0x924C2993` | UnknownNoise | 90 B ×1 |  |
| +84.445 s | +79.595 s | S | `0x347E1C53` | UnknownNoise | 10 B ×1 |  |
| +84.473 s | +79.622 s | C | `0x00F55400` | UnknownNoise | 42 B ×1 |  |
| +84.497 s | +79.647 s | C | `0x000E5500` | UnknownNoise | 44 B ×1 |  |
| +84.552 s | +79.702 s | C | `0x02505500` | UnknownNoise | 18 B ×1 |  |
| +84.564 s | +79.714 s | C | `0x14015055` | UnknownNoise | 58 B ×1 |  |
| +84.596 s | +79.745 s | C | `0x04715500` | UnknownNoise | 10 B ×1 |  |
| +84.662 s | +79.811 s | C | `0x01B55500` | UnknownNoise | 5 B ×1 |  |
| +84.662 s | +79.811 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +84.731 s | +79.880 s | C | `0x01F85500` | UnknownNoise | 5 B ×1 |  |
| +84.731 s | +79.880 s | C | `0x1401F955` | UnknownNoise | 58 B ×1 |  |
| +84.756 s | +79.906 s | C | `0x00125600` | UnknownNoise | 44 B ×1 |  |
| +84.815 s | +79.964 s | C | `0x014C5600` | UnknownNoise | 5 B ×1 |  |
| +84.827 s | +79.977 s | S | `0xC7631100` | UnknownNoise | 275 B ×1 |  |
| +84.858 s | +80.008 s | C | `0x14017656` | UnknownNoise | 58 B ×1 |  |
| +84.877 s | +80.026 s | C | `0x028B5600` | UnknownNoise | 20 B ×1 |  |
| +84.960 s | +80.110 s | C | `0x02DE5600` | UnknownNoise | 20 B ×1 |  |
| +85.004 s | +80.153 s | C | `0x14010857` | UnknownNoise | 58 B ×1 |  |
| +85.060 s | +80.209 s | C | `0x02405700` | UnknownNoise | 20 B ×1 |  |
| +85.086 s | +80.235 s | C | `0x045A5700` | UnknownNoise | 10 B ×1 |  |
| +85.138 s | +80.288 s | C | `0x028E5700` | UnknownNoise | 20 B ×1 |  |
| +85.225 s | +80.375 s | C | `0x00006300` | KeepAlive | 4 B | |
| +85.225 s | +80.375 s | C | `0x1401EC57` | UnknownNoise | 58 B ×1 |  |
| +85.225 s | +80.375 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.247 s | +80.397 s | C | `0x00000001` | DebugDrawBase | 43 B | |
| +85.272 s | +80.422 s | C | `0x01175800` | UnknownNoise | 4 B ×1 |  |
| +85.292 s | +80.442 s | C | `0x14012A58` | UnknownNoise | 58 B ×1 |  |
| +85.334 s | +80.483 s | C | `0x02545800` | UnknownNoise | 20 B ×1 |  |
| +85.373 s | +80.523 s | C | `0x047C5800` | UnknownNoise | 10 B ×1 |  |
| +85.415 s | +80.564 s | C | `0x02A55800` | UnknownNoise | 20 B ×1 |  |
| +85.439 s | +80.589 s | C | `0x00BB5800` | UnknownNoise | 43 B ×1 |  |
| +85.439 s | +80.589 s | C | `0x1401BC58` | UnknownNoise | 58 B ×1 |  |
| +85.493 s | +80.642 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.493 s | +80.642 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.493 s | +80.642 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.493 s | +80.642 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.493 s | +80.642 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.493 s | +80.642 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +85.519 s | +80.669 s | C | `0x000C5900` | UnknownNoise | 43 B ×1 |  |
| +85.617 s | +80.767 s | C | `0x006D5900` | UnknownNoise | 37 B ×1 |  |
| +85.617 s | +80.767 s | C | `0x14016E59` | UnknownNoise | 58 B ×1 |  |
| +85.644 s | +80.794 s | S | `0x89DFE4B5` | UnknownNoise | 275 B ×1 |  |
| +85.778 s | +80.927 s | C | `0x14010F5A` | UnknownNoise | 58 B ×1 |  |
| +85.912 s | +81.061 s | C | `0x1401955A` | UnknownNoise | 58 B ×1 |  |
| +85.932 s | +81.082 s | C | `0x00A95A00` | UnknownNoise | 38 B ×1 |  |
| +86.061 s | +81.211 s | C | `0x002A5B00` | UnknownNoise | 44 B ×1 |  |
| +86.061 s | +81.211 s | C | `0x14012B5B` | UnknownNoise | 58 B ×1 |  |
| +86.128 s | +81.278 s | C | `0x026D5B00` | UnknownNoise | 20 B ×1 |  |
| +86.194 s | +81.344 s | C | `0x02AF5B00` | UnknownNoise | 20 B ×1 |  |
| +86.219 s | +81.369 s | C | `0x1401C85B` | UnknownNoise | 58 B ×1 |  |
| +86.252 s | +81.402 s | C | `0x00006300` | KeepAlive | 4 B | |
| +86.286 s | +81.436 s | C | `0x020C5C00` | UnknownNoise | 20 B ×1 |  |
| +86.348 s | +81.498 s | C | `0x04495C00` | UnknownNoise | 10 B ×1 |  |
| +86.348 s | +81.498 s | C | `0x14014A5C` | UnknownNoise | 58 B ×1 |  |
| +86.380 s | +81.530 s | C | `0x006C5C00` | UnknownNoise | 43 B ×1 |  |
| +86.411 s | +81.561 s | C | `0x048F5C00` | UnknownNoise | 11 B ×1 |  |
| +86.443 s | +81.593 s | C | `0x01A95C00` | UnknownNoise | 3 B ×1 |  |
| +86.515 s | +81.665 s | C | `0x00F05C00` | UnknownNoise | 42 B ×1 |  |
| +86.515 s | +81.665 s | C | `0x1401F05C` | UnknownNoise | 58 B ×1 |  |
| +86.587 s | +81.737 s | C | `0x00260069` | PlayerUpdateInteractionInfoRequestWire | 15 B | |
| +86.615 s | +81.765 s | C | `0x02535D00` | UnknownNoise | 20 B ×1 |  |
| +86.688 s | +81.838 s | C | `0x14019E5D` | UnknownNoise | 58 B ×1 |  |
| +86.725 s | +81.875 s | C | `0x02C25D00` | UnknownNoise | 20 B ×1 |  |
| +86.823 s | +81.973 s | C | `0x02295E00` | UnknownNoise | 20 B ×1 |  |
| +86.823 s | +81.973 s | C | `0x14012A5E` | UnknownNoise | 58 B ×1 |  |
| +86.823 s | +81.973 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +86.862 s | +82.012 s | C | `0x044B5E00` | UnknownNoise | 10 B ×1 |  |
| +86.933 s | +82.083 s | C | `0x02925E00` | UnknownNoise | 21 B ×1 |  |
| +86.969 s | +82.119 s | C | `0x1401B65E` | UnknownNoise | 58 B ×1 |  |
| +87.042 s | +82.192 s | C | `0x02FF5E00` | UnknownNoise | 21 B ×1 |  |
| +87.078 s | +82.228 s | C | `0x04235F00` | UnknownNoise | 10 B ×1 |  |
| +87.112 s | +82.262 s | C | `0x14014A5F` | UnknownNoise | 58 B ×1 |  |
| +87.112 s | +82.262 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +87.160 s | +82.310 s | C | `0x02755F00` | UnknownNoise | 21 B ×1 |  |
| +87.217 s | +82.367 s | C | `0x04AE5F00` | UnknownNoise | 10 B ×1 |  |
| +87.261 s | +82.411 s | C | `0x00006300` | KeepAlive | 4 B | |
| +87.261 s | +82.411 s | C | `0x02D95F00` | UnknownNoise | 21 B ×1 |  |
| +87.261 s | +82.411 s | C | `0x1401DA5F` | UnknownNoise | 58 B ×1 |  |
| +87.287 s | +82.437 s | C | `0x01FF5F00` | UnknownNoise | 4 B ×1 |  |
| +87.318 s | +82.468 s | C | `0x02136000` | UnknownNoise | 18 B ×1 |  |
| +87.375 s | +82.525 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +87.375 s | +82.525 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +87.375 s | +82.525 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +87.375 s | +82.525 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +87.402 s | +82.552 s | C | `0x02686000` | UnknownNoise | 17 B ×1 |  |
| +87.402 s | +82.552 s | C | `0x14016860` | UnknownNoise | 58 B ×1 |  |
| +87.421 s | +82.571 s | C | `0x007E6000` | UnknownNoise | 36 B ×1 |  |
| +87.540 s | +82.690 s | C | `0x1401FC60` | UnknownNoise | 58 B ×1 |  |
| +87.631 s | +82.781 s | C | `0x004C6100` | UnknownNoise | 37 B ×1 |  |
| +87.728 s | +82.877 s | C | `0x01AD6100` | UnknownNoise | 5 B ×1 |  |
| +87.728 s | +82.877 s | C | `0x1401AD61` | UnknownNoise | 58 B ×1 |  |
| +87.791 s | +82.941 s | C | `0x02F06100` | UnknownNoise | 18 B ×1 |  |
| +87.863 s | +83.013 s | C | `0x02346200` | UnknownNoise | 18 B ×1 |  |
| +87.863 s | +83.013 s | C | `0x14013462` | UnknownNoise | 58 B ×1 |  |
| +87.962 s | +83.112 s | C | `0x02976200` | UnknownNoise | 19 B ×1 |  |
| +87.997 s | +83.147 s | C | `0x04B96200` | UnknownNoise | 9 B ×1 |  |
| +88.030 s | +83.180 s | C | `0x0000C300` | Synchronization | 49 B | |
| +88.030 s | +83.180 s | C | `0x1401DD62` | UnknownNoise | 58 B ×1 |  |
| +88.165 s | +83.315 s | C | `0x02646300` | UnknownNoise | 18 B ×1 |  |
| +88.165 s | +83.315 s | C | `0x14016463` | UnknownNoise | 58 B ×1 |  |
| +88.238 s | +83.388 s | C | `0x0000C300` | Synchronization | 49 B | |
| +88.238 s | +83.388 s | C | `0x04A46300` | UnknownNoise | 10 B ×1 |  |
| +88.269 s | +83.419 s | C | `0x00006300` | KeepAlive | 4 B | |
| +88.269 s | +83.419 s | C | `0x00C56300` | UnknownNoise | 42 B ×1 |  |
| +88.273 s | +83.423 s | S | `0xF201E539` | UnknownNoise | 90 B ×1 |  |
| +88.273 s | +83.423 s | S | `0xA545B6A7` | UnknownNoise | 90 B ×1 |  |
| +88.273 s | +83.423 s | S | `0xBBD45AC2` | UnknownNoise | 90 B ×1 |  |
| +88.273 s | +83.423 s | S | `0x9ED770C2` | UnknownNoise | 90 B ×1 |  |
| +88.273 s | +83.423 s | S | `0xB898DD25` | UnknownNoise | 90 B ×1 |  |
| +88.274 s | +83.424 s | S | `0x7BECA120` | UnknownNoise | 90 B ×1 |  |
| +88.274 s | +83.424 s | S | `0xEE4D6798` | UnknownNoise | 10 B ×1 |  |
| +88.274 s | +83.424 s | S | `0xE4DC0273` | UnknownNoise | 10 B ×1 |  |
| +88.274 s | +83.424 s | S | `0xB62FD569` | UnknownNoise | 10 B ×1 |  |
| +88.310 s | +83.460 s | C | `0x1401EE63` | UnknownNoise | 58 B ×1 |  |
| +88.383 s | +83.533 s | C | `0x00366400` | UnknownNoise | 42 B ×1 |  |
| +88.383 s | +83.533 s | C | `0x00260069` | PlayerUpdateInteractionInfoRequestWire | 15 B | |
| +88.450 s | +83.600 s | C | `0x027D6400` | UnknownNoise | 18 B ×1 |  |
| +88.450 s | +83.600 s | C | `0x00260069` | PlayerUpdateInteractionInfoRequestWire | 15 B | |
| +88.483 s | +83.633 s | C | `0x04996400` | UnknownNoise | 10 B ×1 |  |
| +88.483 s | +83.633 s | C | `0x14019A64` | UnknownNoise | 58 B ×1 |  |
| +88.504 s | +83.654 s | C | `0x03AF6400` | UnknownNoise | 14 B ×1 |  |
| +88.587 s | +83.737 s | C | `0x02036500` | UnknownNoise | 18 B ×1 |  |
| +88.618 s | +83.768 s | C | `0x04226500` | UnknownNoise | 9 B ×1 |  |
| +88.618 s | +83.768 s | C | `0x14012265` | UnknownNoise | 58 B ×1 |  |
| +88.691 s | +83.841 s | C | `0x026B6500` | UnknownNoise | 18 B ×1 |  |
| +88.731 s | +83.881 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +88.731 s | +83.881 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +88.758 s | +83.908 s | C | `0x1401AC65` | UnknownNoise | 58 B ×1 |  |
| +88.831 s | +83.981 s | C | `0x00F66500` | UnknownNoise | 40 B ×1 |  |
| +88.895 s | +84.045 s | S | `0xBCCDB3D9` | UnknownNoise | 275 B ×1 |  |
| +88.935 s | +84.085 s | C | `0x005F6600` | UnknownNoise | 41 B ×1 |  |
| +88.935 s | +84.085 s | C | `0x14015F66` | UnknownNoise | 58 B ×1 |  |
| +88.997 s | +84.147 s | C | `0x02A16600` | UnknownNoise | 18 B ×1 |  |
| +88.997 s | +84.147 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +89.068 s | +84.218 s | C | `0x00E36600` | UnknownNoise | 37 B ×1 |  |
| +89.068 s | +84.218 s | C | `0x1401E466` | UnknownNoise | 58 B ×1 |  |
| +89.203 s | +84.353 s | C | `0x14016A67` | UnknownNoise | 58 B ×1 |  |
| +89.266 s | +84.416 s | C | `0x00006300` | KeepAlive | 4 B | |
| +89.266 s | +84.416 s | C | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11 B | |
| +89.373 s | +84.523 s | C | `0x14011368` | UnknownNoise | 58 B ×1 |  |
| +89.592 s | +84.742 s | C | `0x00F06800` | UnknownNoise | 38 B ×1 |  |
| +89.592 s | +84.742 s | C | `0x1401F068` | UnknownNoise | 58 B ×1 |  |
| +89.660 s | +84.810 s | C | `0x01346900` | UnknownNoise | 5 B ×1 |  |
| +89.660 s | +84.810 s | C | `0x14013569` | UnknownNoise | 58 B ×1 |  |
| +89.709 s | +84.859 s | C | `0x02656900` | UnknownNoise | 18 B ×1 |  |
| +89.783 s | +84.933 s | C | `0x00260069` | PlayerUpdateInteractionInfoRequestWire | 15 B | |
| +89.813 s | +84.963 s | C | `0x1401CD69` | UnknownNoise | 58 B ×1 |  |
| +89.887 s | +85.037 s | C | `0x02166A00` | UnknownNoise | 18 B ×1 |  |
| +89.946 s | +85.096 s | C | `0x045C6A00` | UnknownNoise | 9 B ×1 |  |
| +89.946 s | +85.096 s | C | `0x14015C6A` | UnknownNoise | 58 B ×1 |  |

## Opcode counts in window

| Count | Dir mix (S/C) | Opcode | Name | Payload min–max |
|------:|---------------|--------|------|-----------------|
| 1854 | 1854/0 | `0x26007400` | PlayerUpdateAddFeatherweightObject | 90–90 B |
| 253 | 127/126 | `0x0000F000` | TemplateBase | 18–220245 B |
| 218 | 218/0 | `0x00260062` | PlayerUpdateAddLightweightPropWire | 400–640 B |
| 108 | 0/108 | `0x0026005E` | PlayerUpdateFullCharacterDataRequestWire | 11–11 B |
| 97 | 97/0 | `0x27000300` | AbilityCasterInfoReply | 178–394 B |
| 86 | 0/86 | `0x27000200` | AbilityCasterInfoRequest | 4–4 B |
| 84 | 84/0 | `0x00260063` | PlayerUpdateLightweightToFullPropWire | 276–325 B |
| 81 | 0/81 | `0x00006300` | KeepAlive | 4–4 B |
| 45 | 44/1 | `0x00001E00` | CommandBase | 8–75 B |
| 40 | 6/34 | `0x0000C300` | Synchronization | 49–49 B |
| 23 | 16/7 | `0x00004500` | InGamePurchaseBase | 2–833179 B |
| 19 | 14/5 | `0x0000F900` | ClaimsBase | 1–2641 B |
| 17 | 0/17 | `0x00006F00` | ClientLog | 35–491 B |
| 12 | 4/8 | `0x00003700` | GameTimeSync | 13–13 B |
| 11 | 11/0 | `0x0000C400` | ResourcesBase | 105–346 B |
| 7 | 7/0 | `0x00002D00` | InventoryBase | 3–10 B |
| 4 | 4/0 | `0x26005100` | PlayerUpdateUpdateCharacterStateDelta | 38–38 B |
| 4 | 4/0 | `0x0000A000` | ActivityBase | 10–110 B |
| 4 | 4/0 | `0x0000B900` | VoiceBase | 62–76 B |
| 4 | 2/2 | `0x0000A700` | RatingBase | 9–66 B |
| 4 | 4/0 | `0x00260059` | PlayerUpdateAddLightweightNpcWire | 266–283 B |
| 4 | 4/0 | `0x26005C00` | PlayerUpdateLightweightToFullNpc | 367–1212 B |
| 4 | 4/0 | `0x26006500` | Unknown | 16–16 B |
| 4 | 0/4 | `0x00006B00` | ClientMetrics | 172–172 B |
| 4 | 0/4 | `0x00260069` | PlayerUpdateInteractionInfoRequestWire | 15–15 B |
| 3 | 3/0 | `0x29002C00` | ClientUpdateUpdateCalculatedStats | 20–388 B |
| 3 | 2/1 | `0x00004200` | RecipeBase | 4–25088 B |
| 3 | 3/0 | `0x0000ED00` | PropBase | 3066–353732 B |
| 3 | 3/0 | `0x00008300` | GuildBase | 10–110669 B |
| 2 | 2/0 | `0x00009200` | AchievementBase | 25–404712 B |
| 2 | 2/0 | `0x00008F00` | ActivityManagerBase | 5–9 B |
| 2 | 2/0 | `0x0000D700` | MapRegionBase | 17–21 B |
| 2 | 2/0 | `0x29001F00` | ClientUpdateZonePopulation | 16–16 B |
| 2 | 2/0 | `0x0000D100` | MissionsBase | 13–13 B |
| 2 | 1/1 | `0x00001301` | WarpNetworkWire | 32–80 B |
| 2 | 0/2 | `0xEC000100` | TerrainCellClientCellRequest | 62–206 B |
| 2 | 1/1 | `0x26008000` | PlayerUpdateSetPlayerBio | 6–6 B |
| 2 | 1/1 | `0x00001300` | ChatBase | 59–112 B |
| 2 | 2/0 | `0xEC000500` | TerrainCellCellsUpdate | 200–39620 B |
| 2 | 0/2 | `0x1401EC57` | UnknownNoise | 58–58 B |

