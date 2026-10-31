# Wave System

Systeme autonome dans le namespace `CCLBStudio.WaveSystem`, sans dependance envers les armes,
les degats, l'IA ou une scene particuliere.

## Les trois responsabilites

| Couche | Types | Role |
| --- | --- | --- |
| Definition | `WaveEnemyDefinition`, `WaveEligibilityRule`, `WaveGenerationSettings` | Caracteristiques de selection, conditions et configuration |
| Generation | `ProceduralWaveGenerator`, `WaveGenerationContext`, `WavePlan` | Choix ponderes, quantites et calendrier relatif, sans instanciation |
| Execution | `WaveRunner`, `WavePooledEnemy`, `WaveSpawnLocation`, `WaveSpawnPoints` | Recuperation depuis les pools, placement, suivi et enchainement |

## Configuration dans Unity

1. Creer un asset **CCLB Studio > Wave System > Enemy** pour chaque type d'ennemi.
   Assigner son `ScriptablePool` (**CCLB Studio > Scriptable Pooling > Pool**). Le prefab gere
   par ce pool doit porter `WavePooledEnemy`. Reglages recommandes du pool : `Disable Object On
   Creation` et `Disable Object On Release` coches. `WaveRunner` active lui-meme l'ennemi apres
   l'avoir place, quel que soit `Enable Object On Request`.
   Ne pas remplacer ce contrat par `Instantiate` dans le code de vagues.
2. Regler `Cost`, `First Wave`, `Last Wave`, `Max Per Wave`, `Max Per Run` et `Preference`.
   Les vagues sont numerotees a partir de **1**. `Last Wave = 0` et les plafonds a `0`
   signifient respectivement aucune limite superieure et aucune limite de quantite.
   Pour autoriser un ennemi seulement apres la vague 5, choisir `First Wave = 6`.
3. Creer un asset **Generation Settings**, ajouter les ennemis, puis regler les courbes
   `Budget` et `Enemy Limit`. Leur axe X est le numero de vague, leur axe Y une valeur
   arrondie vers le bas. Le nombre est un **maximum**, pas un objectif obligatoire.
4. Ajouter les contraintes `Mandatory Enemies`. Exemple : un boss a partir de la vague 5,
   `Repeat Every = 5`, `Count = 1` impose un boss aux vagues 5, 10, 15, etc.
   `Repeat Every = 0` impose uniquement `First Wave`. `Last Wave` borne les repetitions.
   Mettre egalement `Max Per Wave = 1` sur le boss si un seul boss doit etre possible.
5. Ajouter `WaveSpawnPoints` dans une scene et renseigner ses transforms : selection
   cyclique ou aleatoire. Ajouter `WaveRunner`, assigner les settings et ce fournisseur.
   Activer `Start Automatically`, ou appeler `StartSequence()`.
6. A la mort d'un ennemi, appeler `WavePooledEnemy.Despawn()`, **pas** `Destroy` ni
   uniquement `SetActive(false)`. La fin de vague attend que toutes les apparitions
   soient executees et que tous les ennemis soient revenus au pool.

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
- `WavePlan.Spawns` est en lecture seule. Les ennemis sont melanges, puis repartis
  regulierement entre 0 et `Spawn Duration`. Une seule apparition a lieu a 0.
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
suivante apres la completion et `Delay Between Waves`. `Last Wave = 0` permet une sequence
illimitee. Les delais utilisent `Time.deltaTime` : `Time.timeScale = 0` les suspend.
`StopWaves()` et la desactivation du runner annulent l'enchainement et rendent les
ennemis encore suivis a leur pool, sans emettre `WaveCompleted`. Une erreur d'execution
arrete la sequence et est signalee dans la console Unity.

Les evenements `WaveStarted`, `EnemySpawned` et `WaveCompleted` permettent de brancher
l'interface, les recompenses ou d'autres mecanismes. Sur le prefab, `WavePooledEnemy.Spawned`
est emis **apres le placement** et donne acces a `Definition` et `WaveNumber` pour
reinitialiser l'IA ou les caracteristiques de combat. Les callbacks de pool arrivent avant
ce placement : ne pas utiliser `OnEnable` comme notification d'apparition finalisee.

## Extensions

Deriver de `WaveEligibilityRule` pour ajouter une condition sous forme de ScriptableObject.
`CanSpawn` recoit le numero de vague, le budget et les places restants, les quantites
deja selectionnees dans la vague et les selections des vagues precedentes.
Les regles doivent etre sans effet de bord et ne pas consommer de generateur aleatoire
global si le determinisme est souhaite. Les caracteristiques de combat restent dans
les composants/assets de votre jeu et peuvent etre raccordees via l'evenement d'apparition.

Deriver de `WaveSpawnLocation` et implementer `GetSpawnPose` pour une autre strategie :
zone 2D, anneau autour d'une cible, NavMesh, etc. Le fournisseur ne doit pas recuperer
d'ennemi lui-meme : `WaveRunner` conserve cette responsabilite via `ScriptablePool`.

## Validation et dependance de pooling

Le systeme depend du sous-module `Assets/CCLBStudio/ScriptablePooling`
(https://github.com/CCLBStudio/ScriptablePooling), declare dans `.gitmodules`. Apres un clone :

```powershell
git submodule update --init
```

`ScriptablePool.ReleaseObject` n'appelle pas `OnObjectReleased` : c'est pourquoi le retour au
pool doit passer par `WavePooledEnemy.Despawn()`, qui notifie le `WaveRunner`. Un appel direct a
`Pool.ReleaseObject` rendrait l'objet au pool sans que la vague le sache (elle ne se terminerait pas).

`Tests~` (ignore par Unity) contient une validation executee dans un projet Unity temporaire,
avec les vraies courbes Unity et les **vraies sources** de `ScriptablePooling` :

```powershell
& '.\Assets\CCLB Studio\WaveSystem\Tests~\Validate.ps1' `
    -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.3.16f1\Editor\Unity.exe'
```

Le script n'ouvre pas et ne met pas a niveau le projet de jeu. Il supprime son projet
temporaire a la fin et renvoie un code d'echec si une assertion ne passe pas.