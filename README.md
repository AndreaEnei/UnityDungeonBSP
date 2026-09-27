# UnityDungeonBSP

Prototipo didattico di generazione procedurale tramite Binary Space Partitioning.

## Versione

- Unity 2022.3.62f2 LTS
- Apple Silicon
- Template 2D

## Stato della Fase 1

La baseline implementa:

- configurazione tramite ScriptableObject.
- validazione dei parametri.
- PRNG locale deterministico xorshift32.
- partizionamento BSP ricorsivo.
- visualizzazione di radice e foglie tramite Gizmos.
- test Edit Mode di determinismo e validità geometrica.

Non sono ancora implementati:

- stanze.
- corridoi.
- Tilemap.
- generazione del livello giocabile.
- Inspector personalizzato.

Questi elementi appartengono alle fasi successive.

## Generare un albero BSP

1. Aprire `Assets/Scenes/DungeonLab.unity`.
2. Selezionare `DungeonSystem` nella Hierarchy.
3. Verificare che `DefaultBspProfile` sia assegnato al controller.
4. Aprire il Context Menu di `Dungeon Generator Controller`.
5. Selezionare `Generate BSP`.
6. Mantenere `DungeonSystem` selezionato.
7. Osservare le partizioni nella Scene view con `Gizmos` attivo.

Non è necessario entrare in Play Mode.

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

Con questa versione del codice, la configurazione di riferimento produce:

```text
23 nodi
12 foglie
profondità massima 4
```

Lo stesso seed, la stessa configurazione e la stessa versione dell'algoritmo devono produrre lo stesso albero.

## Eseguire i test

1. Aprire `Window > General > Test Runner`.
2. Selezionare `EditMode`.
3. Premere `Run All`.

La suite della Fase 1 contiene 14 casi di test:

- 8 sul partizionatore BSP.
- 6 sul PRNG deterministico.

Tutti i test devono risultare verdi prima di modificare la baseline.

## Documentazione tecnica

La scelta e i limiti del PRNG sono descritti in `Documentation/Xorshift32.md`.