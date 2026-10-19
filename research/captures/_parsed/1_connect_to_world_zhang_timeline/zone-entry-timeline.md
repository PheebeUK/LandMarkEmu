# Official capture zone-entry timeline

Reference for comparing emulator S→C grouping/order and approximate timings.

## Source

| Field | Value |
|-------|-------|
| Capture | `1_connect_to_world_zhang.pcapng` |
| Parsed | `2026-10-12 19:52Z` |
| Window | first `SendSelfToClient` through +90s |
| t=0 | first `SendSelfToClient` (S→C) |
| Also listed | `rel_ready` = ms vs first `ClientIsReady` |
| Zone msgs in window | 201 |
| Total zone msgs in capture | 201 |

**Timing note:** timestamps are pcap/pcapng packet times of the UDP frame that *completed* SOE reassembly for that zone message (last fragment). Payload length is zone body after opcode bytes.

**Grouping note:** high-volume opcodes (featherweights, B00, cell request/reply spam, ClientLog) are collapsed into burst rows. Bootstrap catalogs and gate packets are listed individually.

**Unknown vs UnknownNoise:** `Unknown` = family-shaped prefix not in `ZoneMessageOpcode` (likely catalog gap). `UnknownNoise` = fails Landmark family-shape heuristic (`& 0xFF00FF == 0` / `0x26|0x27|0x29|0xEC`) — treat as ARC4/demux framing garbage until the parser stream is proven green.

## Anchors

| Event | Dir | Opcode | Payload | t vs SendSelf | t vs ClientIsReady |
|-------|-----|--------|---------|---------------|--------------------|

## Phases (approximate)

| Phase | First t(SendSelf) | Last t(SendSelf) | Msg count | Dominant opcodes |
|-------|-------------------|------------------|-----------|------------------|
| A. Pre-SendSelf catalogs | — | — | 0 | — |
| B. SendSelf → ClientIsReady | — | — | 0 | — |
| C. ClientIsReady → ZoneDone / early catalogs | — | — | 0 | — |
| D. Terrain cold start (A00 / first 100 / first 500) | — | — | 0 | — |
| E. World stream (featherweight / lightweight) | — | — | 0 | — |
| F. Remainder of window | — | — | 0 | — |

## Timeline (hybrid)

| t(SendSelf) | t(Ready) | Dir | Opcode | Name | Payload | Notes |
|-------------|----------|-----|--------|------|---------|-------|
| — | — | C | `0x872D0A9B` | UnknownNoise | 11 B ×1 |  |
| — | — | S | `0xF7EDF24C` | UnknownNoise | 7 B ×1 |  |
| — | — | S | `0x7C702413` | UnknownNoise | 7 B ×1 |  |
| — | — | S | `0x9EDA984D` | UnknownNoise | 7 B ×1 |  |
| — | — | S | `0xA2E089D5` | UnknownNoise | 9 B ×1 |  |
| — | — | S | `0x9B6D22F5` | UnknownNoise | 8 B ×1 |  |
| — | — | S | `0xE2AE8BC5` | UnknownNoise | 7 B ×1 |  |
| — | — | S | `0x65F35388` | UnknownNoise | 3 B ×1 |  |
| — | — | S | `0x9DA2B987` | UnknownNoise | 108 B ×1 |  |
| — | — | S | `0x994B1982` | UnknownNoise | 6736 B ×1 |  |
| — | — | C | `0x0C5B5482` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0x074B2B1D` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0x16B08D49` | UnknownNoise | 0 B ×1 |  |
| — | — | S | `0x687D7095` | UnknownNoise | 189 B ×1 |  |
| — | — | S | `0xFD911A9E` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0xABB52678` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0xA41CAC8C` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0xCE700E94` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0x36AEC3` | UnknownNoise | 0 B ×1 |  |
| — | — | S | `0xA760442A` | UnknownNoise | 447 B ×1 |  |
| — | — | S | `0x3E644209` | UnknownNoise | 399 B ×1 |  |
| — | — | S | `0x7A67BA6D` | UnknownNoise | 399 B ×1 |  |
| — | — | S | `0x905E608D` | UnknownNoise | 399 B ×1 |  |
| — | — | S | `0x05471CC9` | UnknownNoise | 399 B ×1 |  |
| — | — | S | `0xF3D6DCA8` | UnknownNoise | 399 B ×1 |  |
| — | — | S | `0xC03A6DDA` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x94A5408F` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x74124C37` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xCEF96628` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0BA495B3` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x516C6CDE` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xE54EBD0E` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xFE94EB62` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x6074DBBB` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x42C5CE67` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0C36179B` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x66BE20AE` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xC620209E` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xB7508190` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xD7E4F44D` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x864BDB30` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xBFC5A6A8` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x71F68CA1` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xB9C08422` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x410EC2D8` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xAA23E284` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xF9AF2EDB` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x15764CC5` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x8C8B0D6A` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x7B745DD6` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x5735419E` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xCB2EED96` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0E180B69` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x9E9F3E63` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x696B9B17` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x4023C1E7` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xCDAA083D` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xCEBF5592` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x14AFE94B` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xB5BB7478` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x5B50D694` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xABFB3C53` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x45E04181` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xC73E3E6C` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xCFD7248C` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xAF36859E` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xB371D5F9` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0B279F81` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0B285B04` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0346CF97` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x2FE19058` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x9B016790` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x10A04974` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x8C0E31E9` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xED334A21` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x289DF77B` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0C2BCBDA` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x894F1E23` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x2B8490F2` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x5DB88EDD` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x80D96FDA` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xAE5072D8` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xEA5ED2C8` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x834850D1` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xC7011776` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xB541E558` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xF0E9FFC4` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x269C3304` | Unknown | 90 B ×1 |  |
| — | — | S | `0x721B0223` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x0C381148` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xD9285028` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x42F7F21F` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x57C1CB29` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xE0F3D08D` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xEFC7C5A0` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x20FCF140` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xFA2A8CD7` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xA5547166` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x01E941FA` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xEE893FE6` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xD0546856` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xF66CC526` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x7BD3F77C` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x974ED5EC` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x6A54EEA9` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x602EB1F1` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x9529D029` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x5492FFD6` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x9B0AA77E` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x64A124C2` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xE17C2029` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x1608FB6C` | UnknownNoise | 90 B ×1 |  |
| — | — | C | `0x698EBE54` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0xFDC9B848` | UnknownNoise | 4 B ×1 |  |
| — | — | S | `0xE1806834` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xD1920C48` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x266EF5AD` | Unknown | 90 B ×1 |  |
| — | — | S | `0xB4209E37` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x864F1E78` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x57843892` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xEAF57769` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x21AD39AD` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xECF2E965` | Unknown | 90 B ×1 |  |
| — | — | S | `0xF4F3DF1B` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xA5665F5C` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x762332C5` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x89BA16A8` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x1A4DF75A` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x1E41564B` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x9EBACF58` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xE8C15E72` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x5DCE3C15` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x28C1ED64` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x2083A7F8` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x8226306E` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0x548604C1` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xF061FA7B` | UnknownNoise | 90 B ×1 |  |
| — | — | S | `0xF4FCD666` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0xE001DA5E` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0x626E77CF` | UnknownNoise | 178 B ×1 |  |
| — | — | C | `0xC88550FE` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0x89854051` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0x444E2FC6` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0x920A8B3B` | UnknownNoise | 4 B ×1 |  |
| — | — | C | `0x41F7B0E8` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x576E3844` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x92D1A679` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x6E568232` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x7ACC324C` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0xC9EDA588` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x06A607A6` | UnknownNoise | 16 B ×1 |  |
| — | — | S | `0x230AF5BF` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0x156238F8` | UnknownNoise | 178 B ×1 |  |
| — | — | S | `0xD4271C5D` | UnknownNoise | 178 B ×1 |  |
| — | — | C | `0xC0C1A1E3` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x5F57BCDE` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0xC58F38B9` | UnknownNoise | 16 B ×1 |  |
| — | — | C | `0x80221A49` | UnknownNoise | 16 B ×1 |  |
| — | — | S | `0x6E83F25C` | UnknownNoise | 1641 B ×1 |  |
| — | — | C | `0xC12CFAEA` | UnknownNoise | 22 B ×1 |  |
| — | — | S | `0x1EC0F546` | UnknownNoise | 11160 B ×1 |  |
| — | — | C | `0x6038C6E5` | UnknownNoise | 22 B ×1 |  |
| — | — | S | `0x2AE89A7F` | UnknownNoise | 24936 B ×1 |  |
| — | — | S | `0x336E500A` | UnknownNoise | 281 B ×1 |  |
| — | — | S | `0x0F1D3F08` | UnknownNoise | 3670 B ×1 |  |
| — | — | S | `0x963785C5` | UnknownNoise | 35519 B ×1 |  |
| — | — | S | `0x62275D1A` | UnknownNoise | 77756 B ×1 |  |
| — | — | S | `0x02E42B4A` | UnknownNoise | 275 B ×1 |  |
| — | — | S | `0xE2AD3649` | UnknownNoise | 275 B ×1 |  |
| — | — | S | `0x5FE8C361` | UnknownNoise | 275 B ×1 |  |
| — | — | S | `0xC8FB72A4` | UnknownNoise | 275 B ×1 |  |
| — | — | S | `0x98DB35D5` | UnknownNoise | 47 B ×1 |  |
| — | — | C | `0xC68290F2` | UnknownNoise | 2 B ×1 |  |
| — | — | S | `0x7F51DE91` | UnknownNoise | 11 B ×1 |  |
| — | — | C | `0x5CF80BD2` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0xE9CCEC80` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0xED1E3811` | UnknownNoise | 47 B ×1 |  |
| — | — | S | `0xE2B03509` | UnknownNoise | 275 B ×1 |  |
| — | — | S | `0xDA4D7AA3` | UnknownNoise | 275 B ×1 |  |
| — | — | C | `0x54298FD4` | UnknownNoise | 22 B ×1 |  |
| — | — | S | `0x5218E996` | UnknownNoise | 275 B ×1 |  |
| — | — | C | `0xAFD955DC` | UnknownNoise | 10 B ×1 |  |
| — | — | S | `0xB06D49C1` | UnknownNoise | 275 B ×1 |  |
| — | — | C | `0x3965A896` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x1D6481CE` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x9EFBB621` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x3F49C8E3` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x3F7ADC35` | UnknownNoise | 2 B ×1 |  |
| — | — | C | `0xCF6BCB9C` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0xCAAF9D60` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0xD54A2CDB` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0xD638E5A8` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x6D34FA57` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x61AA7512` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x2E973186` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x9EB60A08` | UnknownNoise | 22 B ×1 |  |
| — | — | C | `0x1BC1E9E3` | UnknownNoise | 47 B ×1 |  |
| — | — | C | `0x4115BDBA` | UnknownNoise | 22 B ×1 |  |
| — | — | S | `0xB85D2A0D` | UnknownNoise | 47 B ×1 |  |
| — | — | C | `0x80413005` | UnknownNoise | 22 B ×1 |  |
| — | — | S | `0x0F761970` | UnknownNoise | 47 B ×1 |  |

## Opcode counts in window

| Count | Dir mix (S/C) | Opcode | Name | Payload min–max |
|------:|---------------|--------|------|-----------------|
| 1 | 0/1 | `0x872D0A9B` | UnknownNoise | 11–11 B |
| 1 | 1/0 | `0xF7EDF24C` | UnknownNoise | 7–7 B |
| 1 | 1/0 | `0x7C702413` | UnknownNoise | 7–7 B |
| 1 | 1/0 | `0x9EDA984D` | UnknownNoise | 7–7 B |
| 1 | 1/0 | `0xA2E089D5` | UnknownNoise | 9–9 B |
| 1 | 1/0 | `0x9B6D22F5` | UnknownNoise | 8–8 B |
| 1 | 1/0 | `0xE2AE8BC5` | UnknownNoise | 7–7 B |
| 1 | 1/0 | `0x65F35388` | UnknownNoise | 3–3 B |
| 1 | 1/0 | `0x9DA2B987` | UnknownNoise | 108–108 B |
| 1 | 1/0 | `0x994B1982` | UnknownNoise | 6736–6736 B |
| 1 | 0/1 | `0x0C5B5482` | UnknownNoise | 4–4 B |
| 1 | 0/1 | `0x074B2B1D` | UnknownNoise | 4–4 B |
| 1 | 0/1 | `0x16B08D49` | UnknownNoise | 0–0 B |
| 1 | 1/0 | `0x687D7095` | UnknownNoise | 189–189 B |
| 1 | 1/0 | `0xFD911A9E` | UnknownNoise | 178–178 B |
| 1 | 1/0 | `0xABB52678` | UnknownNoise | 178–178 B |
| 1 | 1/0 | `0xA41CAC8C` | UnknownNoise | 178–178 B |
| 1 | 1/0 | `0xCE700E94` | UnknownNoise | 178–178 B |
| 1 | 1/0 | `0x36AEC3` | UnknownNoise | 0–0 B |
| 1 | 1/0 | `0xA760442A` | UnknownNoise | 447–447 B |
| 1 | 1/0 | `0x3E644209` | UnknownNoise | 399–399 B |
| 1 | 1/0 | `0x7A67BA6D` | UnknownNoise | 399–399 B |
| 1 | 1/0 | `0x905E608D` | UnknownNoise | 399–399 B |
| 1 | 1/0 | `0x05471CC9` | UnknownNoise | 399–399 B |
| 1 | 1/0 | `0xF3D6DCA8` | UnknownNoise | 399–399 B |
| 1 | 1/0 | `0xC03A6DDA` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x94A5408F` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x74124C37` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0xCEF96628` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x0BA495B3` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x516C6CDE` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0xE54EBD0E` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0xFE94EB62` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x6074DBBB` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x42C5CE67` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x0C36179B` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0x66BE20AE` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0xC620209E` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0xB7508190` | UnknownNoise | 90–90 B |
| 1 | 1/0 | `0xD7E4F44D` | UnknownNoise | 90–90 B |

