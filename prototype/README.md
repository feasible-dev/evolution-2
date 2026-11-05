# Evolution Wars — v41 face continuity and card cleanup

Direction uses one clear arrow. Gerard's smaller excited faces are brought
closer to the normal portrait scale; speech keeps the chin and neck stable,
with restrained lip-only animation. Sources, previous art and a before/after
animation gallery are retained in `../art-direction/v41/`.

## v40 continuous stadium marquee

One full-width LED band now rotates current statistics and broadcast messages.
Orange/blue dominate; rare result alerts use bold larger red lettering and a
brief full-screen red treatment. Central score dividers and several decorative
blue borders are removed. Details: `../art-direction/v40/README.md`.

## v39 upright posture and LED messages

Gerard rests with an upright neck and follows major milestones with a spoken,
animated seated aftermath and delayed exchange with René. The scrolling message
strips are stationary multicolor LED dot grids. Details and review gallery:
`../art-direction/v39/README.md`, `../art-direction/v39/index.html`.

## v38 unified broadcast style

The default commentators now use one refined portrait library for seated,
speaking, gaze, blink, standing and desk-reaction poses. Shared master poses
and palette prevent the earlier static/cartoon style switch. The ticker has
an original colorful pixel alphabet, French accents and occasional letter
swells. Review gallery: `../art-direction/v38/index.html`; implementation notes:
`../art-direction/v38/README.md`. Earlier artwork remains preserved.

## v37 direction and rink attention

The first-level action cards are now Turn, Thrust and Direction. Direction is
an inherited absolute angle, with circular mutation and no target knowledge.
Commentators follow real concentrations and occasional individual agents, with
independent irregular timing. Large reactions accompany exceptional outcomes
and their commentary: 95%, perfect survival or a generation wipeout. Ordinary
selection does not trigger standing. Details: `../art-direction/v37/README.md`.

## v36 broadcast foundation

The first level now uses the simplified v35 commentators, with independently
timed conversational glances and new speaking poses for Le Chevalier. The first
selection triggers a table reaction; strong survival results trigger standing,
with a delayed partner reaction and a longer standing hold. Animations run in
real time, including during pause, with a cooldown between big gestures.

Launch normally with `Lancer-Evolution2.ps1` or `Lancer-Evolution2.cmd` from the
repository root. The older, more textured v34 art study remains preserved in
`../art-direction/v34/duo-scenes/`. This is a playable style trial; facial texture
may return in a later compromise. Notes: `../art-direction/v36/broadcast/README.md`.

The earlier revision notes below describe previous implementations; v36
supersedes their fixed-body portrait rendering and synchronized gaze behavior.

## Earlier layout and gameplay revision notes

Le mode d’affichage `canvas_items / expand` utilise toute la fenêtre, sans bandes noires latérales sur les écrans larges. Au-delà du cadrage de base, la largeur supplémentaire est répartie entre les deux panneaux joueurs ; l’arène reste centrée. Les deux bandes de messages défilent exclusivement de droite à gauche. Captures larges : `../art-direction/v21/captures`.

L’arène s’étend maintenant jusqu’au bas de la scène. La transformation d’affichage remplit son rectangle ; les dimensions physiques du monde, la sélection et les mouvements simulés restent inchangés. Les panneaux joueurs ont un fond sur toute leur hauteur, avec la survie en haut, la stratégie et la composition au milieu, puis les commandes sur notre côté.

Entre les commentateurs et l’arène, `ArenaBillboard` affiche la dernière génération terminée et sa survie réelle dans un module central. Le côté adverse reste un tiret. Deux `TickerStrip` font défiler un appel au public et le commentaire courant de droite à gauche, à vitesse constante en temps réel, y compris en pause. Ils sont découpés dans leurs propres zones pour ne pas recouvrir le score. Les textes existent en anglais et en français ; aucun vote du public ni vainqueur fictif n’est présenté.

Captures de cette disposition et du défilement : `../art-direction/v20/captures`.

La zone sous l’arène est libre. Chaque panneau latéral accueille une courbe de survie sur 40 générations et quatre barres de composition actuelle. Le côté adverse reste sans données tant qu’aucun adversaire n’est simulé : aucun partage 74/26 n’est calculé ou inventé. À droite : plan choisi en lecture seule, statistiques, slider de vitesse ×1–×10, pause, redémarrage et son. Les encadrés arrondis des commentateurs n’ont plus de pointe ; leurs bords extérieurs s’alignent sur l’arène.

Une ronde couvre toute la simulation lancée avec un plan. Les cartes sont verrouillées du démarrage jusqu’au redémarrage, y compris en pause et après une génération. Reprendre après les premiers 100 % conserve le même plan ; Repartir permet d’en choisir un nouveau.

## Language and localization

English is now the default, including cards, tooltips, budget messages and commentary. Launch normally for English, or use `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Lancer-Evolution2.ps1 -Language fr` from the project root for French. Godot also accepts `-- --lang=en` or `-- --lang=fr`. Language is selected at startup; changing it requires relaunching.

`scripts/presentation/L10n.cs` loads the UTF-8 catalogs in `localization/en.json` and `localization/fr.json`. Add messages with stable semantic IDs and access them with `L10n.Text("message.id", arguments...)`. Preserve numbered placeholders and their numeric formats across languages. Formatting uses the selected language's culture. A missing French entry falls back to English; a missing English ID is an error. Tests check key and placeholder parity and exercise the formatted messages in both languages. Supporting another language requires adding its catalog and registering its culture and launcher option.

The survival chart is now a line with point markers, visible axes, 0/50/100% ticks and a subtle dashed 100% guide. It retains the last 40 generations and hover details. Population composition remains a separate stacked chart. “UI SOUND: ON/OFF” toggles interface click sounds only; it does not control narration or music.

Current English captures: `../art-direction/v29/captures`.

## Changements de cette passe

- Portraits : une posture assise fixe par personnage. La parole anime seulement le bas du visage ; les changements de regard se fondent sur 0,22 seconde. Les anciens changements instantanés de gestes ne sont plus utilisés. Les bras et le bureau restent identiques dans les seize états vérifiés.
- Cartes : les trois sliders commencent à zéro. Chaque slider ne change que sa propre allocation, dans la limite des points restants. Quand les dix points sont dépensés, il faut descendre un slider pour pouvoir en monter un autre. Le compteur affiche les points encore à attribuer. Les commandes restent sous les cartes et VIDER remet le brouillon à zéro. Captures : `../art-direction/v29/captures`.
- Survie parfaite : la première sélection à 240/240 déclenche deux répliques spéciales et une pause automatique sur le résultat. Reprendre continue la simulation ; les prochains 100 % ne provoquent pas une nouvelle pause. Repartir réinitialise cette célébration.
- Temps : ×1 effectue désormais deux secondes simulées par seconde réelle. Les autres vitesses sont des multiples de cette nouvelle base. Une génération dure 36 secondes simulées, soit 18 secondes réelles à ×1, hors sélection.
- Premier défi : mutations moins fortes (0,03) et moins fréquentes (10 % de mutation du programme). Le test passe du plan 3/3/4 au plan Cible 10 à G3 ; il atteint 239/240 survivants à G17 (99,6 %) et termine G40 à 236/240 (98,3 %). Ces valeurs sont mesurées avec les graines du scénario ; chaque répartition de budget ne garantit pas ce résultat.
- Vérification : clics sur les deux faces et le slider externe, contrôle exhaustif des budgets partiels et complets sans déplacement des autres allocations, pause/reprise à 100 % avec une population de test placée dans la zone, et simulation de 40 générations. Les portraits conservent leurs 25 contrôles visuels et leur palette de 16 couleurs.

Lancer depuis PowerShell, à la racine du projet :

~~~powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Lancer-Evolution2.ps1
~~~

Le fichier Lancer-Evolution2.cmd reste utilisable par double-clic dans l'Explorateur ou par .\Lancer-Evolution2.cmd dans le terminal. Ne pas copier ses lignes batch internes dans PowerShell.

## Interface et palette

L'arene occupe les 70 % centraux ; les panneaux lateraux utilisent chacun 15 %. Les trois grandes cartes sont visibles uniquement pendant la preparation. Une fois la ronde lancee, le plan reste affiche en lecture seule sur notre cote.

- **Gérard McCautry**, nom de travail : chemise hawaïenne bleu pastel, citrons jaunes, fleurs rouges et feuilles vertes.
- **René le Chevalier** : plus âgé, cheveux blancs, petite moustache aristocratique, vêtements bleu pastel.
- Portraits v13 : menton relevé, yeux ouverts, regard légèrement incliné vers le terrain, redessinés en aplats, avec **16 encres communes**, rendus à **192 × 128 pixels** au lieu de 48 × 32. Quatre poses, bouches ouvertes/fermées, regard vers le bas. La palette est imposée après échantillonnage par `assets/portrait-palette-v05.gdshader`. Aperçu : `../art-direction/v14/dossier.html`.
- Points de **5 pixels de diamètre logique**, contre environ 21 en v03. 240 points dans le scénario.
- Couleur du point = action : orange virage, jaune poussée, cyan cible, anneau gris sans règle. La condition « toujours » reste inspectable ; elle ne prend plus la moitié d'un point minuscule.

Les commentaires restent textuels avec animation de bouche. Aucun doublage n'a encore été ajouté.

## Budget

Le plan comprend **10 crédits, tous à répartir** entre Virage, Poussée et Cible. Un crédit représente 10 % du tirage conditionnel de famille d'action. Les sliders externes sont indépendants : augmenter une carte consomme seulement les points disponibles, et la diminuer restitue des points sans modifier les autres. Les trois allocations démarrent à zéro ; le démarrage et la validation nécessitent dix points attribués. Retourner une carte conserve son budget. VIDER remet le budget a zero uniquement avant le demarrage.

Un plan incomplet ou au-delà du budget ne peut pas être validé. Une famille à zéro est retirée du réservoir de nouvelles règles. Ce retrait n'efface pas immédiatement les règles déjà héritées.

Les poids s'appliquent au démarrage et aux changements de famille lors des mutations. L'immigration aléatoire du moteur garde son mécanisme d'origine ; les proportions réelles ne sont donc pas garanties identiques au plan. Le panneau POPULATION les mesure séparément.

Pendant la ronde, le moteur rejette toute modification, remise à zéro du budget ou nouvelle validation. Il faut redémarrer pour préparer un autre plan.

## Temps et sélection

- Slider de vitesse de **×1 à ×10**, par pas de 1 ; départ à ×5. Aucun bouton de vitesse ni raccourci numérique.
- Espace : pause/reprise. Entrée : démarrer depuis la préparation. R : repartir.
- Chaque période dure 36 secondes simulées ; ×1 correspond à deux secondes simulées par seconde réelle.
- Sélection puis reproduction automatique, avec une pause de célébration au premier résultat à 100 %.
- Première sélection : révélation lisible, 0,8 seconde réelle de sélection. Ensuite : 0,1 seconde réelle par sélection.
- Zone exacte : rectangle normalisé de largeur **0,05**, centré à **0,975** en X. Donc uniquement les **5 % de droite** du plateau.
- La zone reste cachée jusqu'à la première sélection. Le moteur conserve le critère supplémentaire : déplacement d'au moins six pixels depuis la naissance ; le bord du rectangle a un score nul.
- En cas d'extinction, le repli du moteur reconstruit une population neuve ; l'extinction est comptée.

La vitesse choisie multiplie les pas de physique par le temps réel ; il ne multiplie pas seulement les déplacements dessinés. La sélection conserve une durée réelle lisible. Le compteur « effectif » mesure le temps de physique effectivement écoulé par seconde réelle ; il peut donc être inférieur à la consigne pendant les sélections ou si la machine atteint sa limite. En pause, les répliques continuent de s'afficher.

## Proportions et historique

Le terrain reste le spectacle principal. Chaque panneau latéral possède deux graphiques : une courbe de survie sur les **40 dernières générations**, avec axes et guide discret à 100 %, puis la composition actuelle en barres horizontales avec pourcentages. Les deux panneaux affichent les mesures réelles de leur population respective.

Le survol du graphique indique génération, survivants et plan. L'historique complet reste en mémoire jusqu'au reset. Le compteur distingue la génération en cours des générations terminées. L'ancien objectif fixe « 60 sur 100 » est retiré pour laisser observer 20, 30, 40 générations et plus.

## Moteur et performances

Le moteur vient de evolution-game. Le projet voisin n'a pas été modifié. Sur les 48 fichiers copiés, **46 restent identiques** au manifeste original. Deux copies locales ont été optimisées :

- BoardGeometry.cs : contours arrondis mis en cache ; chemins rapides sur les portions droites ; calculs polygonaux d'origine conservés pour les coins et cas limites. Les tableaux publics sont copiés pour protéger le cache.
- Simulation.cs : observation spatiale coûteuse uniquement si un gène hérité la nécessite ; arrêt des passes de collision quand une passe n'a corrigé aucune position.

Les empreintes originales restent dans scripts/vendor/source-manifest.json. Les dérivations et empreintes locales sont dans scripts/vendor/local-changes-v23.json.

La physique reste à pas fixe de 1/60 de seconde, avec accumulateur d'horloge en double précision. Les paramètres du scénario et les graines sont centralisés dans LevelOneSession.cs. Les couleurs et familles sont dans BehaviorCatalog.cs.

## Vérifier et capturer

~~~powershell
.\Lancer-Evolution2.ps1 -Test
.\Lancer-Evolution2.ps1 -Captures
~~~

La verification couvre le budget independant, le verrouillage de ronde, le slider 1-10, les panneaux lateraux, la pause, la survie parfaite, la physique fixe et 40 generations.

Les captures proviennent du vrai rendu Godot, sans scores inventés ni positions imposées. Les captures actuelles sont sous `../art-direction/v29/captures` ; les anciens dossiers visuels sont conservés dans les révisions précédentes.

Godot .NET 4.6.1 et .NET 8 sont utilisés. Le lanceur emploie les paquets hors ligne fournis avec Godot et le cache NuGet local ../.cache/nuget. Les chemins de Godot sont à adapter dans le lanceur et NuGet.Config sur une autre machine. Dans le bac à sable, les avertissements de certificats/cache utilisateur n'empêchent pas le rendu ou les vérifications.

## Captures de la révision 22

`powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Lancer-Evolution2.ps1 -Captures` enregistre les écrans réels dans `../art-direction/v29/captures`. Ajouter `-Language fr` pour une capture française.

L’adversaire local choisit un budget valide de dix points avant chaque ronde. Sa stratégie varie entre les rondes et reste verrouillée pendant le jeu, comme celle du joueur.

## Duo et répliques

Gérard McCautry et René Le Chevalier sont réunis au centre. Leur bulle conserve une place fixe : Gérard à gauche, René à droite. Les intitulés de rôle sont retirés. Les regards latéraux, vers le partenaire, puis le retour au terrain sont tirés des atlas 4 × 2. Le rythme utilise le temps réel, séparé de la simulation. Le rendu compose le visage animé sur un corps stable, avec une transition progressive des regards et de la bouche.

## Duel local et espace réservé

Chaque joueur dispose de 240 agents, de sa propre simulation, de graines aléatoires distinctes et de sa propre sélection/reproduction. Les deux populations suivent la même horloge et le même objectif des 5 % de droite. Pause, vitesse et redémarrage sont communs. Les collisions restent internes à chaque population : ce duel compare les performances de survie, sans interactions physiques entre équipes. Le premier 100 % de chaque joueur suspend la ronde et peut être repris.

Les points sont cyan pour nous et roses pour l’adversaire, quelle que soit leur règle. Les courbes de survie reprennent ces couleurs. Les barres de composition conservent leur code par action. Le bandeau central compare les derniers résultats réels ; ses messages défilent de droite à gauche.

Une bande vide de 56 pixels logiques reste sous la patinoire, soit environ 1,5 cm à l’échelle de référence (la taille physique dépend de l’écran). Les espaces extérieurs supérieurs restent disponibles pour le futur logo à gauche et les outils à droite.

## Revision 23: contour et contacts

Le dessin du terrain utilise exactement le polygone de BoardGeometry, avec des coins de rayon 14 % du petit axe. La zone coloree des 5 % est intersectee avec ce contour. La physique tient maintenant compte du rayon des agents sur les bords droits aussi. Ces changements peuvent modifier les trajectoires et les resultats de survie.

La grille de collisions utilise le diametre reel de contact au lieu du diametre visuel. Benchmark reproductible : `-- --cards-test --collision-test`, 240 pas de 240 agents regroupes. Avant : 23 180 437 paires candidates / 1331 ms ; apres : 3 001 333 / 646 ms sur cette machine. Ce chiffre concerne le test dense, pas le FPS global. Aucun contact n'est volontairement supprime.

En jeu, les pas fixes des deux equipes disposent ensemble d'un budget de 8 ms par image, verifie apres chaque paire de pas. En surcharge, le temps simule ralentit et le retard est plafonne ; l'interface reste reactive. Un pas individuel peut depasser ce budget. Les tests deterministes ne sont pas soumis a ce plafond.

## Revision 24: coins circulaires

La projection utilise une echelle uniforme dans les quatre coins. Les segments droits absorbent le changement de proportions de la fenetre. Contour, agents, inspection et zone de selection utilisent cette meme transformation. La simulation conserve ses dimensions et ses regles ; la zone de 5 % reste definie dans les coordonnees physiques. Verification du rayon constant sur les quatre coins pour trois rapports largeur/hauteur.

## Revision 25: projection uniforme et physique visible

Cette revision remplace la projection par sections de v24 : une seule echelle s’applique a tous les points, axes et deplacements. Le terrain conserve son rapport physique 1280/560 ; l’espace excedentaire reste sous le terrain. Les coins restent circulaires et la zone de selection suit exactement la physique.

Les deux panneaux montrent les moyennes de vitesse physique (unites/s), de pourcentage d’energie restante, de capacite d’acceleration avant (poussee/masse), de masse et de trainee. Le survol explique les definitions, la vitesse limite commune et le nombre d’agents epuises. La capacite n’est pas une mesure d’acceleration instantanee. Les moyennes incluent la population actuelle, pas seulement les survivants du dernier resultat.

Le texte des cartes precise que les points ponderent les tirages de comportements. Les traits physiques sont herites separement. Le parametre de scenario Drag=0.08, inutilise par le moteur, est retire ; la trainee individuelle et tout l’equilibrage restent inchanges.

Audit reproductible : `-- --cards-test --motion-audit`. A cartes et graines identiques, positions, vitesses et energie sont identiques pour les deux moteurs. Verification supplementaire de la projection avec `-- --cards-test --collision-test`.

## Revision 26: pleine hauteur et coins circulaires

Le terrain physique adopte les proportions de l’espace central disponible, sur une largeur de 1280 unites. Le dessin conserve une echelle uniforme et remplit toute la hauteur. La reserve inferieure est de 48 pixels logiques, environ un demi-pouce a 96 DPI.

Les deux equipes partagent les memes dimensions. Le redimensionnement adapte positions et origines sans changer vitesses, energie, traits, historique ou horloge. La zone de selection et les cibles normalisees suivent les nouvelles dimensions. La geometrie change donc les distances de trajet par rapport a v25 ; les resultats de survie ne sont pas garantis identiques. WorldState.Size est desormais modifiable dans l’assembly pour rester coherent avec les limites physiques.

## Revision 27: duel equitable et calcul plus rapide

Les deux equipes utilisent les memes graines de simulation, de programmes, de mutation et de reproduction. Le placement initial provient d’un pool commun avant application des cartes : changer les cartes ne change pas le tirage des corps et des positions. Les populations restent deux objets independants. L’adversaire choisit toujours son propre plan avant la ronde.

Les capacites physiques sont fixes via PhysicsProfile.Default : masse 1, poussee avant 220, trainee 0.35, energie initiale 100, vitesse limite 110. EvolutionSettings.FixedPhysicsProfile assure cette egalite pour les descendants, immigrants et reprises apres extinction ; les comportements et leurs parametres continuent d’evoluer. Les memes cartes produisent les memes trajectoires. Avec des cartes differentes, les trajectoires, la depense d’energie et la vitesse moyenne peuvent differer.

Verification `-- --cards-test --motion-audit` avec 7/3/0 pour chacun : vitesses et energie identiques aux secondes 3, 9 et 18 ; ADN, positions, vitesses et energie identiques apres trois reproductions, avec 37/37, 73/73 puis 83/83 survivants.

Le calcul de normale du mur est evite quand l’agent ne touche pas le bord, sans changer la physique. Benchmark dense : 542 ms contre 730 ms en v26 pour les memes 3 001 333 paires candidates. Le budget d’execution par image passe de 8 a 16 ms ; le curseur reste limite a x10 et l’horloge reste commune aux deux equipes. Le debit reel depend de la machine et de la concentration des agents.

## Revision 28: supprimer les intensites cachees

La reproduction des plans rouge 2/2/6 et bleu 0/3/7 a revele a G12 une intensite cible moyenne de 1.084 contre 0.378, soit une poussee de 47.689 contre 16.639 malgre la meme capacite physique affichee.

DuelMotionPolicy centralise maintenant les intensites de comportement et la cadence. Chaque programme initial ou reproduit est normalise : cible 1, poussee 1, virage +/-1, decision 0.2 s sans jitter. Les cibles et les choix de comportement continuent d’evoluer. Le moteur ramene la contribution directionnelle vers la cible a 44 unites/s2, meme si plusieurs regles sont presentes. Le bruit exploratoire utilise un poids commun constant de 0.121, sans facteur cache de lignee. Ces options restent propres au duel ; les valeurs nulles conservent le comportement historique du moteur generique.

Verification `-- --cards-test --duel-audit` : apres 12 generations, 44.000 de poussee cible des deux cotes, intensite 1.000, cadence 0.200. Les vitesses moyennes restent dependantes des trajectoires et des contacts avec les murs (22.22 et 24.35 dans ce test), pas d’un bonus de puissance. Le test 7/3/0 contre 7/3/0 reste strictement identique sur trois reproductions. Les panneaux affichent desormais l’acceleration cible partagee au lieu de la capacite physique maximale, qui masquait le probleme.

## Revision 29: identite des joueurs

Deux blocs de portrait apparaissent au-dessus des panneaux lateraux, pendant la preparation et la ronde. Les silhouettes generiques sont des emplacements, pas des portraits personnels. CompetitorProfile porte un identifiant stable, les cles localisees du nom et du personnage, et un PortraitPath optionnel. CompetitorIdentity accepte le remplacement du profil et utilise la silhouette si aucune texture n’est disponible.

Les futurs selecteurs d’adversaire et de personnage pourront affecter PlayerIdentity.Profile et OpponentIdentity.Profile. Ces profils restent independants des cartes et de la physique ; aucun selecteur fictif n’est expose pour le moment.
