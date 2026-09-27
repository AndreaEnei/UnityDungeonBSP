# `DeterministicRandom`: implementazione xorshift32

## Scopo

Il prototipo usa un generatore di numeri pseudocasuali (PRNG) locale per compiere scelte variabili mantenendo la riproducibilità. La classe [`DeterministicRandom`](../Assets/Dungeon/Runtime/Generation/DeterministicRandom.cs) implementa una variante di **xorshift32**, appartenente alla famiglia descritta da George Marsaglia.[^1]

La scelta è circoscritta alle esigenze del prototipo:

- implementazione breve e interamente ispezionabile;
- stato locale, indipendente da `UnityEngine.Random` e `System.Random`;
- operazioni intere deterministiche;
- sequenza riproducibile a parità di seed e ordine delle chiamate;
- costo computazionale ridotto per semplici scelte discrete.

Identificatore della versione documentata:

```text
xorshift32-v1: uint32, shift (13, 17, 5), zero fallback 0xA341316C
```

## Stato e transizione

Il generatore mantiene un solo stato senza segno a 32 bit:

```csharp
private uint state;
```

Ogni chiamata a `NextUInt()` applica tre operazioni di shift e XOR, quindi salva e restituisce il nuovo stato:

```csharp
uint value = state;

value ^= value << 13;
value ^= value >> 17;
value ^= value << 5;

state = value;
return value;
```

Indicando con `x` lo stato corrente, la transizione è:

```text
x = x XOR (x << 13)
x = x XOR (x >> 17)
x = x XOR (x << 5)
```

`<<` e `>>` spostano i bit; XOR produce `1` quando i bit confrontati sono diversi. Nessuna entropia esterna viene aggiunta: ogni stato dipende soltanto dal precedente.

Per la parametrizzazione `(13, 17, 5)`, uno stato iniziale non nullo produce un periodo massimo di `2^32 - 1`: la sequenza attraversa tutti gli stati non nulli prima di ripetersi. La lunghezza del periodo non implica qualità crittografica né assenza di correlazioni statistiche.

## Inizializzazione e seed zero

Il costruttore usa il seed come stato iniziale. Due istanze con lo stesso seed iniziano dalla stessa posizione della sequenza:

```csharp
var first = new DeterministicRandom(12345);
var second = new DeterministicRandom(12345);
```

Lo stato zero non è valido per xorshift32: shift e XOR applicati a zero continuano a produrre zero.

```text
0 -> 0 -> 0 -> 0 -> ...
```

L'implementazione sostituisce quindi il seed zero con una costante fissa e non nulla:

```csharp
private const uint NonZeroFallbackSeed = 0xA341316C;

state = seed == 0
    ? NonZeroFallbackSeed
    : seed;
```

La costante non ha un significato applicativo particolare; rende soltanto esplicita e stabile la politica adottata. Ne consegue una collisione intenzionale: i seed `0` e `0xA341316C` producono la stessa sequenza. La collisione è inevitabile quando `2^32` possibili input vengono mappati sui soli `2^32 - 1` stati validi.

## API

### `NextUInt()`

Aggiorna lo stato e restituisce il successivo valore `uint` della sequenza. Ogni chiamata consuma esattamente una transizione.

### `NextInt(minInclusive, maxExclusive)`

Restituisce un intero nell'intervallo semiaperto:

```text
[minInclusive, maxExclusive)
```

Per esempio, `NextInt(3, 7)` può produrre `3`, `4`, `5` o `6`, ma non `7`. Se `minInclusive >= maxExclusive`, il metodo genera un'`ArgumentOutOfRangeException` riferita a `maxExclusive`.

La conversione corrente è:

```csharp
uint range = (uint)(maxExclusive - minInclusive);
uint offset = NextUInt() % range;
return minInclusive + (int)offset;
```

Il progetto usa intervalli abbastanza piccoli da non causare overflow nella sottrazione `maxExclusive - minInclusive`. Valori estremi dell'intero non appartengono al contratto corrente.

### `NextBool()`

Consuma un valore tramite `NextUInt()` e controlla il bit più significativo:

```csharp
return (NextUInt() & 0x80000000u) != 0u;
```

La maschera `0x80000000u` conserva il bit numero 31. Un risultato nullo corrisponde a `false`; un risultato non nullo corrisponde a `true`. La scelta del bit più significativo evita di basare tutte le decisioni booleane sul bit più basso, ma non elimina la struttura lineare del generatore.

## Contratto di determinismo e compatibilità

La sequenza restituita da `DeterministicRandom` dipende da:

- seed iniziale e politica per il seed zero;
- tipo e ampiezza dello stato;
- costanti e ordine degli shift;
- metodo usato per convertire `NextUInt()` in interi o booleani;
- ordine e numero delle chiamate.

Lo stesso seed riproduce la stessa sequenza soltanto se questi elementi restano invariati. Inserire una chiamata aggiuntiva, anche scartandone il risultato, sposta tutti i valori successivi.

Modificare shift, fallback, conversione degli intervalli o consumo della sequenza costituisce quindi un cambiamento di versione. Non deve essere trattato come un refactoring invisibile se esistono seed salvati o risultati sperimentali da riprodurre.

## Limiti

### Modulo bias

L'operazione `NextUInt() % range` può introdurre un piccolo **modulo bias**: se i valori disponibili non si distribuiscono esattamente fra tutti i possibili resti, alcuni risultati compaiono una volta più di altri lungo l'intero periodo.

Per i brevi intervalli usati dal prototipo questo limite è accettato e documentato. Se fosse necessaria una distribuzione discreta più rigorosa, `NextInt()` dovrebbe adottare rejection sampling. Tale modifica potrebbe consumare un numero variabile di valori e richiederebbe una nuova versione dell'algoritmo.

### Qualità statistica e sicurezza

Xorshift32 è piccolo e veloce, ma conserva relazioni lineari e non offre garanzie crittografiche. Non è adatto a:

- crittografia o generazione di segreti;
- gioco d'azzardo;
- simulazioni che richiedano proprietà statistiche più forti;
- situazioni in cui un osservatore non debba poter ricostruire la sequenza.

L'adeguatezza della scelta riguarda esclusivamente il suo impiego come sorgente trasparente e riproducibile per decisioni discrete del prototipo.

## Verifica dell'implementazione

I test diretti si trovano in [`DeterministicRandomTests`](../Assets/Dungeon/Tests/EditMode/Tesi.Dungeon.EditMode.Tests/DeterministicRandomTests.cs) e verificano:

1. uguaglianza delle sequenze generate dallo stesso seed;
2. sostituzione efficace del seed zero;
3. stabilità di un test vector noto;
4. rispetto dell'intervallo semiaperto nel campione controllato;
5. eccezione per intervalli vuoti o invertiti.

Il controllo dell'intervallo usa un campione deterministico e costituisce un test di regressione, non una prova esaustiva di tutti gli stati.

### Test vector di `xorshift32-v1`

Con seed `12345`, le prime cinque chiamate a `NextUInt()` devono produrre:

| Chiamata | Valore atteso |
|---:|---:|
| 1 | `3336926330` |
| 2 | `1697253807` |
| 3 | `2816511904` |
| 4 | `1955480042` |
| 5 | `718842323` |

Il test vector rileva modifiche alla transizione anche quando una nuova implementazione rimane internamente deterministica.

## Riferimento

[^1]: George Marsaglia, “Xorshift RNGs”, *Journal of Statistical Software*, vol. 8, n. 14, pp. 1–6, 2003. DOI: [10.18637/jss.v008.i14](https://doi.org/10.18637/jss.v008.i14).
