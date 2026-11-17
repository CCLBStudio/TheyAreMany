# Wave System

Systeme autonome dans le namespace `CCLBStudio.WaveSystem`, sans dependance envers les armes,
les degats, l'IA, le pooling ou une scene particuliere. Seule dependance : `CCLBStudio.EventBus`.

## Les trois responsabilites

| Couche | Types | Role |
| --- | --- | --- |
| Definition | `WaveEnemyDefinition`, `WaveEligibilityRule`, `WaveGenerationSettings`, `WaveSpawnGroup` | Caracteristiques de selection, conditions et configuration |
| Generation | `ProceduralWaveGenerator`, `WaveGenerationContext`, `WavePlan` | Choix ponderes, quantites et calendrier relatif, sans instanciation |
| Execution | `WaveRunner`, `IWaveEnemy`, `WaveEnemyProvider`, `WaveSpawnLocation`, `WaveSpawnPoints`, `WaveSpawnLocationRouter`, evenements | Recuperation via les providers, placement, suivi, enchainement et evenements (EventBus) |

## Configuration dans Unity

1. Creer un asset **CCLB Studio > Wave System > Enemy** pour chaque type d'ennemi et lui assigner
   un `WaveEnemyProvider`. Pour les ennemis du jeu (prefab avec `EnemyFacade`), creer un asset
   **CCLB Studio > Wave System > Providers > From Pool** (`ProvideWaveEnemyFromPool`) et y assigner
   le `ScriptablePool` de l'ennemi. Le provider initialise le pool avant la premiere vague.
   `WaveRunner` appelle `IWaveEnemy.OnWaveSpawn` juste apres `Provide()` : l'ennemi s'y place a la
   pose fournie et active son GameObject. Reglage de pool recommande : `Enable Object On Request`
   decoche, pour que l'ennemi ne soit actif qu'une fois place.
2. Regler `Cost`, `First Wave`, `Last Wave`, `Max Per Wave`, `Max Per Run` et `Preference`.
   Les vagues sont numerotees a partir de **1**. `Last Wave = 0` et les plafonds a `0`
   signifient respectivement aucune limite superieure et aucune limite de quantite.
   Pour autoriser un ennemi seulement apres la vague 5, choisir `First Wave = 6`.
3. Creer un asset **Generation Settings**, ajouter les ennemis, puis regler les courbes
   `Budget` et `Enemy Limit`. Leur axe Y est une valeur arrondie vers le bas. Le nombre est
   un **maximum**, pas un objectif obligatoire.
   `Wave Count` fixe le nombre de vagues d'une sequence (`0` = illimite).
   `Curve Axis` choisit l'axe X de **toutes** les courbes (budget, limite, preferences) :
   - `WaveNumber` (defaut) : X = numero de vague (1, 2, 3...). Recommande pour les sequences
     illimitees ou pour un equilibrage precis vague par vague.
   - `NormalizedProgress` : X = 0 a la vague 1 et 1 a la derniere (`Wave Count`). Les courbes
     restent valides si on change le nombre de vagues. En mode illimite, `Reference Wave Count`
     (50 par defaut) est la vague a X = 1 ; au-dela, X depasse 1 et le `Post Wrap Mode`
     de la courbe s'applique (Clamp = plateau, ou cles placees apres 1 pour continuer a monter).
   Les seuils en nombre de vagues (`First Wave`, `Last Wave`, `Mandatory Enemies`) restent
   toujours des numeros de vague absolus.
4. Ajouter les contraintes `Mandatory Enemies`. Exemple : un boss a partir de la vague 5,
   `Repeat Every = 5`, `Count = 1` impose un boss aux vagues 5, 10, 15, etc.
   `Repeat Every = 0` impose uniquement `First Wave`. `Last Wave` borne les repetitions.
   Mettre egalement `Max Per Wave = 1` sur le boss si un seul boss doit etre possible.
5. Ajouter `WaveSpawnPoints` dans une scene et renseigner ses transforms : selection
   cyclique ou aleatoire. Ajouter `WaveRunner`, assigner les settings et ce fournisseur.
   Activer `Start Automatically`, ou appeler `StartSequence()`.
   **Points par type d'ennemi** : creer des assets **CCLB Studio > Wave System > Spawn Group**
   (ex. `Flying`, `Boss`) et les assigner au champ `Spawn Group` des ennemis concernes.
   Dans la scene, ajouter un `WaveSpawnLocationRouter` : `Default Location` pour les ennemis
   sans groupe (ou sans route), et une route `groupe -> WaveSpawnLocation` par groupe.
   Assigner le router comme `Spawn Location` du `WaveRunner`. La selection cyclique est
   calculee par emplacement : les ennemis d'un groupe parcourent ses points sans en sauter.
6. A la mort d'un ennemi, il doit etre despawn (`IWaveEnemy.Despawn()` ou, pour `EnemyFacade`,
   `ReleaseSelf()`), **pas** `Destroy` ni uniquement `SetActive(false)` : l'evenement
   `IWaveEnemy.Despawned` doit etre emis. La fin de vague attend que toutes les apparitions
   soient executees et que tous les ennemis aient ete despawn.

Les courbes de preference sont des **poids relatifs**, pas des probabilites absolues :
deux ennemis de poids 3 et 1 ont respectivement 75 % et 25 % de chance a chaque tirage
s'ils sont tous deux eligibles et abordables. Un poids nul ou negatif exclut un ennemi
des tirages aleatoires, mais pas des obligations. Par exemple, faire decroitre le poids
de l'ennemi simple de 10 a 1 entre les vagues 1 et 20 et augmenter celui d'un ennemi
complexe de 0 a 8 sur la meme plage. Configurer aussi son `First Wave` pour interdire
completement son apparition initiale.

## Regles de generation

- Les ennemis obligatoires sont reserves dans l'ordre de configuration avant les tirages.
  Ils consomment le budget et les places et respectent toutes les conditions et plafonds.
  Une obligation incompatible leve une exception : aucun boss obligatoire n'est omis.
- La generation s'arrete lorsqu'il ne reste plus de place ou plus de candidat eligible,
  abordable et de poids positif. Un reliquat de budget est normal. Une vague vide est
  une erreur explicite, pas une vague automatiquement ignoree.
- `WavePlan.Spawns` est en lecture seule. Les ennemis sont melanges, puis repartis entre 0
  et `Spawn Duration`. Une seule apparition a lieu a 0. Avec `Spawn Timing Jitter = 0`
  l'espacement est regulier ; a `1`, chaque intervalle varie entre 0 et 2 fois l'intervalle
  moyen (rafales et accalmies). La premiere apparition reste a 0, la derniere a
  `Spawn Duration`, et le tirage utilise la graine (deterministe, sans effet sur la selection).
- Une graine identique, les memes assets et des regles deterministes donnent les memes
  plans. La generation n'affecte pas `UnityEngine.Random`. La selection aleatoire des
  **positions**, elle, utilise `UnityEngine.Random` et ne fait pas partie de cette garantie.
- Les compteurs `Max Per Run` comptent les selections des plans **generes avec succes**,
  meme si leur execution est ensuite annulee. Un echec de generation ne modifie ni
  les compteurs ni le numero de la prochaine vague. Une nouvelle instance de generateur
  ou `StartSequence()` apres un arret demarre une nouvelle partie a la vague 1.
- Les assets ne portent pas d'etat de partie. Chaque generateur a ses propres compteurs.

Pour une orchestration externe :

```csharp
var generator = new ProceduralWaveGenerator(settings, seed: 42);
WavePlan plan = generator.GenerateNextWave();
runner.StartWave(plan);
```

`StartWave` execute uniquement ce plan. `StartSequence` genere automatiquement la vague
suivante apres la completion et `Delay Between Waves`, jusqu'a `Wave Count` (`0` = sequence
illimitee). Au-dela, `GenerateNextWave` leve une exception (`HasNextWave` permet de le verifier).
Les delais utilisent `Time.deltaTime` : `Time.timeScale = 0` les suspend.
`StopWaves()` et la desactivation du runner annulent l'enchainement et despawn les
ennemis encore suivis, sans emettre `WaveCompletedEvent`. Une erreur d'execution
arrete la sequence, est signalee dans la console Unity et emet `WaveSequenceStoppedEvent`
avec `Failed = true`.

## Evenements (EventBus)

Tous les evenements majeurs passent par `CCLBStudio.EventBus` et portent le `Runner` emetteur :

| Evenement | Emis quand |
| --- | --- |
| `WaveStartedEvent` | une vague demarre (`Wave`, `TotalWaves`) |
| `WaveEnemySpawnedEvent` | un ennemi a ete fourni et place (`Enemy`, `Context`) |
| `WaveCompletedEvent` | toutes les apparitions sont faites et tous les ennemis despawn (`IsLastWave`) |
| `AllWavesCompletedEvent` | la derniere vague d'une sequence limitee est terminee |
| `WaveSequenceStoppedEvent` | une vague/sequence en cours est annulee (`InterruptedWave`, `Failed`) |

```csharp
EvtBus.Subscribe<WaveCompletedEvent>(OnWaveCompleted); // ou IEventListener<WaveCompletedEvent>
void OnWaveCompleted(in WaveCompletedEvent evt) { ... }
```

Le bus intercepte et journalise les exceptions des listeners : elles n'arretent pas la vague.
`IWaveEnemy.OnWaveSpawn` recoit `Definition`, `WaveNumber`, `SpawnIndex` et `Pose` pour
reinitialiser l'IA ou les caracteristiques de combat. Les callbacks de pool arrivent avant
ce placement : ne pas utiliser `OnEnable` comme notification d'apparition finalisee.

## Extensions

Deriver de `WaveEligibilityRule` pour ajouter une condition sous forme de ScriptableObject.
`CanSpawn` recoit le numero de vague, le budget et les places restants, les quantites
deja selectionnees dans la vague et les selections des vagues precedentes.
Les regles doivent etre sans effet de bord et ne pas consommer de generateur aleatoire
global si le determinisme est souhaite. Les caracteristiques de combat restent dans
les composants/assets de votre jeu et peuvent etre raccordees via l'evenement d'apparition.

Deriver de `WaveSpawnLocation` et implementer `GetSpawnPose(in WaveSpawnRequest)` pour une
autre strategie : zone 2D, anneau autour d'une cible, NavMesh, etc. La requete donne la vague,
`SpawnIndex`, `Enemy` et `LocalIndex` (index parmi les apparitions recues par cet emplacement,
a utiliser pour une selection cyclique). Le fournisseur ne doit pas recuperer
d'ennemi lui-meme : `WaveRunner` conserve cette responsabilite via `WaveEnemyProvider`.

Deriver de `WaveEnemyProvider` pour changer la facon d'obtenir les ennemis (autre pool,
factory...). `Provide()` doit renvoyer un `IWaveEnemy` dont `IsSpawned` vaut `true`, et cet
ennemi doit emettre `Despawned` lorsqu'il quitte le jeu. `Prepare()` est appele une fois par
runner avant la premiere vague qui utilise le provider.

## Validation

`Tests~` (ignore par Unity) contient une validation executee dans un projet Unity temporaire,
avec les vraies courbes Unity et les vraies sources de `ScriptablePooling` et `EventBus`
(`Tests~/Runtime` fournit un ennemi et un provider de test) :

```powershell
& '.\Assets\CCLBStudio\WaveSystem\Tests~\Validate.ps1' `
    -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe'
```

Le script n'ouvre pas et ne met pas a niveau le projet de jeu. Il supprime son projet
temporaire a la fin et renvoie un code d'echec si une assertion ne passe pas.