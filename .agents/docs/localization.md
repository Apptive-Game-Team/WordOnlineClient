# Localization Tables

Everything under `Assets/Localization` is Unity Localization data serialized as
YAML. The Unity Editor cannot run in this environment, so these files are edited
by hand. Read this before touching them.

## One magic, three different spellings of its name

`Assets/Scripts/Data/Magic/LocalCombinedMagicData.cs` takes the magic name that
the game server sends (snake_case, for example `evil_ent`) and derives three
separate names from it with `Data.Util.StringUtils`:

| Field | Transform | Example | Used for |
| --- | --- | --- | --- |
| `localizationKey` | `ToCamelCase` | `evilEnt` | key in the `Magic` string table (display name) |
| `textLocalizationKey` | `ToSnakeCase` | `evil_ent` | key in the `MagicBook` string table (description) |
| `resourceName` | `ToPascalCase` | `EvilEnt` | prefab and sprite name under `Assets/Resources` |

The two localization keys are the trap: the same unit is `evilEnt` in one table
and `evil_ent` in the other. Adding only one casing looks correct in the file you
edited and still leaves the other screen broken.

Both keys are read in two places, so check both:

- `Assets/Scripts/MagicBookScene/MagicInfo.cs` — magic book name and description.
- `Assets/Scripts/DeckScene/DeckInfoMagicPopup.cs` — deck popup name and description.

`MagicInfo.GetMagicBookKeyCandidates` falls back through several spellings, so a
missing `MagicBook` entry degrades quietly rather than throwing. A missing
`Magic` entry shows the raw key on screen. Neither fails the build; the only way
to notice is to look at the screen or to check the tables.

Not every magic needs a `MagicBook` row — the `Magic` table is larger than the
`MagicBook` table on purpose.

## Adding an entry

A string table is three files: `<Table> Shared Data.asset` maps key to `m_Id`,
and `<Table>_ko-KR.asset` and `<Table>_en.asset` map that same `m_Id` to the
localized string. Add the entry to all three, with the same `m_Id`, or the row
exists with no text.

`m_Id` is normally allocated by a `DistributedUIDGenerator` in the Editor. When
hand-editing, pick a value that is unused in that table and shaped like its
neighbours — continue the table's existing run rather than inventing a new
numbering scheme. Entries are stored in ascending `m_Id` order; appending the
new largest id keeps the file sorted and the diff to one hunk.

After editing, verify that the shared-data key count equals each locale file's
entry count, and that the new id occurs exactly once in each of the three files
and nowhere else in that table.

## YAML string encoding

- Korean is stored as `\uXXXX` escapes inside a double-quoted scalar, uppercase
  hex. English is stored as an unquoted plain scalar.
- Long values wrap. The rule that reproduces the existing files: write the value
  after `    m_Localized: `, and at each space, if the current column is already
  at or past 80, break the line instead and indent the continuation by 6 spaces.
  This counts the escaped output, so a Korean line breaks after far fewer words
  than an English one.
- Files are LF only (`.gitattributes` is `* text=auto eol=lf`).

Do not copy the neighbouring rows blindly. The tables already contain damage
that an Editor pass would fix but review will not:

- `Magic Shared Data.asset` has a dead key `'emberSpiritSwarm` with a leading
  apostrophe alongside the real `emberSpiritSwarm`. It can never be looked up.
- The Korean value for `treeGolem` ends with a stray `\n`.
- `chicken_commando` stores raw Hangul instead of `\uXXXX` escapes, so the
  encoding is not uniform.

## What still needs the Editor

Hand-edited tables are not loaded until someone opens the project. An Editor
pass should confirm the Localization Tables window shows the new row in both
locales, that the magic book and deck popup render it, and that saving the
tables produces no diff — a diff there means the hand-written YAML did not match
what Unity emits.
