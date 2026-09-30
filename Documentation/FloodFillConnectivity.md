# Verifica della connettività tramite flood fill

## Scopo

La classe [`DungeonConnectivityValidator`](../Assets/Dungeon/Runtime/Validation/DungeonConnectivityValidator.cs)
verifica che tutte le celle calpestabili di una `DungeonGrid` appartengano alla
stessa regione connessa. Il controllo avviene dopo la rasterizzazione di stanze
e corridoi e dopo la costruzione dei muri, senza modificare la griglia.

L'ispezione visiva non è sufficiente a garantire questa proprietà: una separazione
di una sola cella può essere difficile da individuare, mentre una verifica
automatica permette di rilevare lo stesso difetto per qualsiasi seed.

## Modello della griglia

La griglia viene interpretata come un grafo implicito:

- ogni cella `Floor` rappresenta un nodo attraversabile;
- due nodi sono collegati quando le rispettive celle condividono un lato;
- le celle `Wall` ed `Empty` non sono attraversabili;
- non è necessario costruire esplicitamente oggetti per nodi e archi.

Il controllo usa i quattro spostamenti cardinali:

```csharp
private static readonly Vector2Int[] Directions =
{
    Vector2Int.right,
    Vector2Int.left,
    Vector2Int.up,
    Vector2Int.down
};
```

Due celle che si toccano soltanto in diagonale non sono quindi considerate
connesse. Questa scelta corrisponde a un movimento su griglia che attraversa i
lati delle celle e impedisce che due pavimenti separati da un angolo vengano
classificati come un passaggio valido.

La costruzione dei muri usa invece un vicinato a otto direzioni per riempire
anche gli angoli visivi. Le due scelte non sono in conflitto: una riguarda la
raggiungibilità del pavimento, l'altra la sua rappresentazione grafica.

## Algoritmo

La verifica è divisa in due passaggi.

### 1. Conteggio e punto di partenza

L'intera griglia viene scansionata per contare le celle `Floor`. La prima cella
incontrata viene conservata come punto di partenza:

```csharp
int totalFloorCells = 0;
Vector2Int? startCell = null;
```

`startCell` è nullable perché la scansione potrebbe non trovare alcun pavimento.
L'ordine corrente visita prima le coordinate `x` e poi le coordinate `y`; la
scelta del punto iniziale è pertanto deterministica.

### 2. Visita flood fill

A partire da `startCell`, l'algoritmo visita tutte le celle di pavimento
raggiungibili mediante una ricerca in ampiezza, o **breadth-first search** (BFS).
La frontiera della ricerca è rappresentata da una coda FIFO:

```csharp
var visited = new bool[grid.Width, grid.Height];
var frontier = new Queue<Vector2Int>();
```

Il funzionamento può essere riassunto così:

```text
inserisci la cella iniziale nella coda
marcala come visitata

finché la coda non è vuota:
    estrai la prima cella
    incrementa il numero di celle raggiunte

    per ognuno dei quattro vicini:
        ignora coordinate esterne alla griglia
        ignora celle già visitate
        ignora celle che non sono Floor
        marca il vicino come visitato
        inseriscilo nella coda
```

Una cella viene marcata come visitata quando entra nella coda, non quando ne
esce. In questo modo due celle differenti non possono inserire lo stesso vicino
più volte prima che venga elaborato. Ogni cella attraversabile entra quindi
nella coda al massimo una volta.

## Perché una BFS

Per il solo controllo di connettività, una visita in profondità (DFS) produrrebbe
lo stesso risultato. La BFS è stata scelta perché:

- rende esplicita la frontiera tramite `Queue<T>`;
- evita una ricorsione la cui profondità dipenderebbe dalla forma del pavimento;
- può essere estesa in futuro per calcolare distanze minime in numero di passi.

L'implementazione corrente non usa l'ordine di visita per generare contenuto e
non calcola distanze: conta soltanto le celle raggiunte.

## Criterio di connettività

Il risultato è rappresentato da
[`DungeonConnectivityResult`](../Assets/Dungeon/Runtime/Validation/DungeonConnectivityResult.cs):

```csharp
public bool IsConnected =>
    TotalFloorCells > 0 &&
    ReachableFloorCells == TotalFloorCells;
```

Il dungeon è connesso quando:

1. esiste almeno una cella di pavimento;
2. il flood fill raggiunge tutte le celle di pavimento presenti.

Il numero di celle non raggiunte è una proprietà derivata:

```csharp
public int UnreachableFloorCells =>
    TotalFloorCells - ReachableFloorCells;
```

Se la griglia contiene più componenti separate, `ReachableFloorCells` descrive
la componente che contiene la prima cella trovata. Non rappresenta
necessariamente la componente più grande. Questa distinzione non modifica
`IsConnected`, che risulta comunque `false`, ma è rilevante nell'interpretazione
del conteggio diagnostico.

## Griglia senza pavimento

Se non viene trovata alcuna cella `Floor`, il validatore restituisce:

```text
TotalFloorCells       = 0
ReachableFloorCells   = 0
UnreachableFloorCells = 0
IsConnected           = false
```

Dal punto di vista puramente matematico, un grafo vuoto può ricevere definizioni
diverse di connettività. Il prototipo adotta invece una regola applicativa: una
mappa priva di pavimento non costituisce un dungeon valido e viene quindi
considerata non connessa.

## Complessità

Indicando con `W` la larghezza, con `H` l'altezza e con `F` il numero di celle
di pavimento:

- la scansione iniziale costa `O(W × H)`;
- la BFS elabora al massimo `F` celle e quattro vicini per cella, quindi costa
  `O(F)`;
- il tempo complessivo è `O(W × H)`;
- la matrice `visited` occupa `O(W × H)` memoria;
- la coda può contenere fino a `O(F)` celle nel caso peggiore.

La costante di quattro vicini non modifica la complessità asintotica.

## Determinismo

Il validatore non usa numeri casuali e non modifica la griglia. A parità di
contenuto e dimensioni della `DungeonGrid`, restituisce sempre lo stesso
risultato. Il controllo può quindi essere ripetuto senza alterare la pipeline di
generazione o il suo stato pseudocasuale.

## Limiti

La verifica corrente risponde alla domanda: "tutto il pavimento appartiene a
una sola componente?" Non calcola:

- il numero totale delle componenti separate;
- la posizione di ogni isola irraggiungibile;
- la lunghezza dei percorsi tra stanze;
- diametro, vicoli ciechi o ridondanza dei collegamenti;
- connettività diagonale o regole di movimento differenti;
- tipi attraversabili diversi da `CellType.Floor`.

Queste informazioni richiederebbero risultati e algoritmi aggiuntivi, ma non
sono necessarie per validare la connettività della baseline corrente.

## Verifica automatica

I test si trovano in
[`DungeonConnectivityValidatorTests`](../Assets/Dungeon/Tests/EditMode/Tesi.Dungeon.EditMode.Tests/DungeonConnectivityValidatorTests.cs)
e coprono:

1. una regione di pavimento interamente connessa;
2. due regioni separate;
3. una griglia senza pavimento;
4. cinque seed espliciti della pipeline completa;
5. una mappa minima non partizionata;
6. un partizionamento profondo;
7. una mappa stretta;
8. corridoi alla larghezza massima della configurazione provata;
9. una batteria deterministica sui primi 1.000 seed.

I test su griglie costruite manualmente verificano direttamente il validatore;
quelli sulla pipeline completa controllano anche che partizionamento, stanze,
corridoi e rasterizzazione producano pavimento raggiungibile senza eccezioni.

