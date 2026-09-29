# UnityDungeonBSP

Prototipo didattico di generazione procedurale tramite Binary Space Partitioning.

## Versione

- Unity 2022.3.62f2 LTS
- Apple Silicon
- Template 2D

## Stato della Fase 2

La pipeline implementa:

- configurazione tramite ScriptableObject.
- validazione dei parametri.
- PRNG locale deterministico xorshift32.
- partizionamento BSP ricorsivo.
- generazione di una stanza per ogni foglia.
- collegamento ricorsivo dei sottoalberi con corridoi rettilinei o a L.
- rasterizzazione di stanze e corridoi (conversione delle forme geometriche in celle della griglia).
- costruzione dei muri attorno alle celle di pavimento.
- rendering tramite due Tilemap.
- visualizzazione di debug tramite Gizmos.
- Inspector personalizzato con validazione e comandi.
- preset confrontabili.
- test Edit Mode di geometria e determinismo.

Non sono ancora implementati:

- controllo della connettività tramite flood fill.
- diagnostica completa del risultato.
- metriche descrittive.
- personaggio e gameplay.
- grafica definitiva.

Questi elementi appartengono alle fasi successive.

## Generare un dungeon

1. Aprire `Assets/Scenes/DungeonLab.unity`.
2. Selezionare `DungeonSystem` nella Hierarchy.
3. Assegnare un `DungeonGenerationProfile`.
4. Verificare che l'inspector mostri `Configuration is valid`.
5. Premere `Generate Dungeon`.
6. Osservare il risultato nella Game view o nella Scene view.

Non è necessario entrare in Play Mode.

Il pulsante `New Seed And Generate` assegna un nuovo seed al profilo e rigenera immediatamente la mappa.

Il pulsante `Clear Dungeon` cancella il risultato corrente e pulisce le Tilemap.

## Configurazione di riferimento

| Parametro | Valore |
| --- |---:|
| Map Width | 80 |
| Map Height | 50 |
| Seed | 12345 |
| Max Depth | 4 |
| Min Leaf Width | 12 |
| Min Leaf Height | 10 |
| Aspect Ratio Bias | 1.25 |
| Min Room Width | 6 |
| Min Room Height | 5 | 
| Room Margin | 1 |
| Corridor Width | 1 |

Con questa versione del codice, la configurazione di riferimento produce:

```text
23 nodi
12 foglie
12 stanze
11 corridoi
profondità massima 4
```

Lo stesso seed, la stessa configurazione e la stessa versione dell'algoritmo devono produrre la stessa griglia finale.

## Preset

Sono disponibili tre profili in: `Assets/Data/GenerationProfiles`

- `DefaultBspProfile`: configurazione di riferimento.
- `WideRoomsProfile`: meno partizioni, stanze più grandi e corridoi più larghi.
- `DenseRoomsProfile`: più partizioni e stanze più piccole.

I preset di confronto usano lo stesso seed e le stesse dimensioni della mappa, così le differenze osservate dipendono principalmente dai parametri strutturali.

## Debug tramite Gizmos

Il componente `BspGizmoDrawer` permette di mostrare separatamente:

- radice della mappa.
- foglie BSP.
- stanze.
- corridoi.

I Gizmos sono visibili nella Scene view quando `DungeonSystem` è selezionato e il pulsante `Gizmos` è attivo.

Colori utilizzati:

- giallo: regione radice.
- azzurro: foglie BSP.
- verde: stanze.
- rosa: corridoi.

I toggle modificano soltanto la visualizzazione di debug e non rigenerano il dungeon.

## Screenshot di confronto

### Configurazione di riferimento

![Default BSP, seed 12345](Documentation/Screenshots/defaultBsp_seed_12345.png)

### Partizionamento fitto

![Dense rooms, seed 12345](Documentation/Screenshots/denseRooms_seed_12345.png)

### Stanze ampie

![Wide rooms, seed 12345](Documentation/Screenshots/wideRooms_seed_12345.png)

## Eseguire i test

1. Aprire `Window > General > Test Runner`.
2. Selezionare `EditMode`.
3. Premere `Run All`.

La suite della Fase 2 contiene 20 casi:

- 8 sul partizionatore BSP.
- 6 sul PRNG deterministico.
- 2 sul posizionamento delle stanze.
- 1 sulla costruzione dei corridoi.
- 3 sulla rasterizzazione, sui muri e sulla griglia completa.

I test verificano, tra le altre cose:

- riproducibilità con lo stesso seed.
- validità geometrica dell'albero BSP.
- rispetto delle dimensioni minime.
- contenimento delle stanze nelle foglie.
- numero e continuità dei corridoi.
- rasterizzazione completa del pavimento.
- costruzione dei muri senza sovrascrivere il pavimento.
- uguaglianza cella per cella di due generazioni identiche.

Tutti i test devono risultare verdi prima di modificare la baseline.

## Documentazione tecnica

La scelta e i limiti del PRNG sono descritti in `Documentation/Xorshift32.md`.