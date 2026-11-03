# Evolution Wars — passation à l’agent du projet Godot

Date : 31 octobre 2026.

## Mission et statut de validation

Nous passons d’un brainstorming visuel dans ChatGPT à une preuve de concept dans le projet Godot existant. L’utilisateur veut donner un véritable rythme de jeu à sa simulation d’évolution : personnages, dialogues, préparation chronométrée, transitions horizontales et réactions pendant une manche.

Une démonstration interactive a confirmé la faisabilité de ces interactions. Dernier retour exact de l’utilisateur : « ok cool ! je n’aime pas plein de choses, mais techniquement j’imagine que l’on peut itérer !!!! »

**C’est une validation de la possibilité d’itérer, pas une validation du graphisme, de la disposition, des dialogues, des réglages ou des règles de cette démo.** Ne pas reproduire aveuglément sa mise en page. La véritable introduction du jeu sera travaillée beaucoup plus tard. Le périmètre actuel commence à l’annonce du combat du niveau 1.

## 1. Commencer par le vrai projet

Le dépôt n’était pas accessible depuis la conversation de conception. Aucune revue récente de son code n’a été effectuée. Les éléments ci-dessous viennent d’anciens échanges et sont à vérifier :

- Dossier connu : `C:/dev/projects/evolution-game/prototype`.
- Godot 4.6, .NET/C#, cible PC ; vérifier les versions installées avant toute décision.
- Simulation C# séparée du rendu Godot. Préférence historique pour des collections et mises à jour manuelles, sans physique ni nœud Godot par agent.
- Noms évoqués : `AppRoot`, `SandboxController`, `SandboxRenderer.cs`, `ChallengePlayScene`, `ChallengePlayController`, `GeneProgram`, `GeneRule`.
- Des cartes influençaient l’espace des mutations futures plutôt que de modifier directement l’ADN existant. À confirmer dans le code.

Lire les instructions du dépôt, son état Git, les scènes et les systèmes pertinents. Identifier le chemin réellement utilisé pour jouer un défi. Ne pas écraser les modifications de l’utilisateur ni migrer de moteur ou de version sans raison.

Avant d’inventer des contrôles, comprendre précisément :

1. Ce que le joueur peut réellement choisir et quand ces choix prennent effet.
2. La signification exacte des couleurs des points.
3. Leur déplacement, leur vitesse et les paramètres de simulation.
4. La durée des périodes et les conditions de survie/mort à leur issue.
5. Le résultat, le score et la progression actuellement implémentés.
6. Ce qui existe déjà pour un adversaire, une seconde population ou un duel.

Si le duel n’existe pas, isoler une présentation expérimentale de l’adversaire sans prétendre avoir implémenté une vraie compétition. Expliquer le manque et conserver les règles existantes ; ne pas inventer silencieusement un système de combat.

## 2. Ce qu’est le jeu — et ce qu’il faut préserver

Le plateau contient des **points colorés abstraits** qui bougent à certaines vitesses. Leurs couleurs ont une signification de gameplay. Les périodes sont courtes ; les points meurent ou survivent selon les règles, notamment à la fin de la période.

Les points ne doivent pas être transformés en insectes, animaux, monstres ou robots illustrés. L’idée de « Robot Wars » concerne le spectacle, l’animateur et les concurrents ; elle n’autorise pas à remplacer la simulation par des combats physiques.

Le mode 1v1 local est une piste future. Un écran divisé a été évoqué, mais sa forme et ses mécaniques ne sont pas décidées. Ne pas en faire une dépendance du premier POC.

## 3. Direction artistique et ton

### Contraintes claires

- Fond majoritairement noir, plateau dominant et très lisible.
- Habillage sobre : cadres fins, menus utiles, peu de surfaces décoratives.
- Pixel art expressif avec des pixels perceptibles et des aplats francs. Références : esprit Commodore 64 / néo-rétro et personnages de *Dave the Diver* ; pas d’obligation d’émuler strictement le matériel 8 bits.
- Un peu de kitsch fin 90/début 2000 ; couleur signature à définir. Aucune palette définitive n’est approuvée.
- Personnages simples à animer : bouche ouverte/fermée, clignement et quelques expressions.
- Humour adulte, ironique, un peu cynique, personnages compétents mais imparfaits. Éviter le ton trop gentil ou enfantin.

### Pistes explicitement rejetées

- Arènes métalliques lourdes, cages détaillées, gros cadres industriels et textures omniprésentes.
- Créatures illustrées à la place des points.
- Portraits semi-réalistes très détaillés.
- Visages trop bon enfant et ambiance générique de jeu pour enfants.

La maquette épurée à points sur noir a été jugée « bien meilleure », mais trop bon enfant. Les portraits suivants ont exploré un ton plus mordant. Ils restent des propositions à retravailler.

## 4. Personnages de travail

Les noms et biographies sont provisoires. Pour le POC, limiter la production à un animateur et un adversaire ; Kitch et Patch constituent un choix de travail proposé, pas une obligation déjà confirmée.

| Personnage | Intention |
| --- | --- |
| Kitch, animateur | Homme 40–50 ans, ingénieur/techie expérimenté, barbiche, allure rocker/tough, regard rusé, un peu usé. Référence d’allure demandée : John McAfee. Un personnage original, pas un gentil papa ni un méchant caricatural. |
| Patch | Américain de 22 ans, un peu gros, boutonneux, bidouille chez lui. Ingénieur sûr de lui, pas assez de sommeil, prototype « qui marchait chez lui ». |
| Lin | Jeune geek chinoise de Shenzhen, lunettes, électronique. L’utilisateur a retiré la demande de grandes dents : ne pas en faire un trait imposé. |
| Irina | Russe, très sévère, esprit ingénieure soviétique. Humour sec. |
| Darwin | Reconnaissable d’après le portrait historique : crâne dégarni, grande barbe blanche. Patient, observateur, remarques incisives et courtoises. |
| Tunde | Bidouilleur nigérian compétent, inventif, aime détourner du matériel. |
| Bruce | Australien, chapeau, personnalité décontractée et accent marqué envisagé. |
| Otto | Ingénieur allemand ou autrichien ; précision, qualité, mesures et tolérances poussées à l’absurde. |

Les nationalités sont des éléments de personnage ; construire les gags autour des manies et du comportement individuel. Les accents parlés et le doublage ne sont pas nécessaires au POC.

## 5. Séquence jouable demandée

### A. Annonce du combat du niveau 1

- L’animateur apparaît avec un mouvement horizontal perceptible.
- Texte clairement lisible dans une bulle/panneau sobre, portrait pixel art animé pendant la parole.
- Progression manuelle avec un bouton **Next**. Si le texte est progressif, un clic peut l’afficher entièrement, puis le clic suivant avance ; rendre le comportement cohérent.
- Présentation de l’adversaire par une arrivée depuis la gauche, puis passage à l’étape suivante.
- Échanges courts. Ce n’est pas la cinématique d’introduction générale du jeu.

### B. Entrée en préparation

- Transition horizontale vers un écran/état de réglages distinct.
- Le joueur déclenche explicitement le début de cette phase ; pas de chrono consommé pendant un dialogue qu’il lit.
- Durée de travail : **30 secondes**, configurable ; 60 secondes reste une possibilité évoquée.
- Choix et boutons tirés des vrais systèmes du projet.
- Retour immédiat au clic : son bref, état visuel, valeur/choix mis à jour.
- Pression croissante et lisible vers la fin : chiffres, jauge, bips espacés puis plus pressants, accent visuel discret. Pas de surcharge ou de stroboscope.
- Bouton **Prêt / Terminer** disponible avant l’expiration : un joueur expérimenté peut aller vite.
- À zéro, verrouiller les choix et continuer automatiquement avec un état valide.

### C. Départ de la manche

- Nouvelle transition horizontale, courte annonce de départ.
- Un compte à rebours bref est une proposition à tester, pas une obligation.
- Entrée sur le plateau réel et lancement de la période.
- Éviter les transitions lentes et les temps morts imposés entre chaque action.

### D. Simulation et réactions

- Les points restent le spectacle principal.
- Animateur en haut, zones joueur/adversaire autour du bas du plateau : intention de disposition, dimensions à itérer.
- Répliques conditionnées par ce qui se passe réellement : avantage, difficulté, retournement, situation serrée, résultat.
- Banque cible : environ **40 messages préécrits**. Pas de génération LLM en cours de jeu nécessaire.
- Une seule prise de parole à la fois ; délai entre interventions, prévention des répétitions et priorité aux événements importants.
- Ne pas annoncer qu’un adversaire « gagne » sans métrique légitime dans le vrai jeu. Si on ne dispose que d’une estimation, la distinguer du résultat final.
- Animation de bouche simple pendant la réplique ; réaction faciale ponctuelle. Sons de dialogue courts facultatifs, réglables séparément si possible.

### E. Fin de période

- Respecter la sélection et le résultat du projet.
- Réaction courte du personnage, bilan clair, possibilité de rejouer.
- Ne pas ajouter une campagne entière, des sauvegardes nouvelles ou tous les adversaires pour terminer ce premier essai.

## 6. Ce que faisait la démo web — ne pas confondre avec les règles

La démo de conversation contenait : dialogues Next, entrées horizontales, texte progressif, portraits, sons synthétiques activables, préparation de 30 secondes, validation anticipée, transition, décompte, simulation de 20 secondes, résultat et rejeu. Elle possédait une banque de 40 répliques.

**Éléments inventés uniquement pour cette démo :** budget de 6 points, contrôles « Mobilité / Persistance / Exploration », deux groupes de 60 points, survie dans une zone à droite, couleurs d’équipes, durée de 20 secondes et calcul des résultats. Aucun de ces éléments ne doit remplacer les règles réelles par défaut.

Les parcours validation anticipée, expiration automatique et rejeu ont été vérifiés dans un test DOM simulé. Le navigateur de test n’a pas pu être installé dans l’environnement distant : ne pas considérer cela comme une validation visuelle exhaustive, ni comme un test Godot.

## 7. Organisation recommandée, à adapter au code

- Ajouter un flux de présentation explicite : annonce → préparation → transition → simulation → résultat.
- Séparer ce flux de la simulation et du calcul de survie.
- Brancher les commentaires sur les événements/mesures existants ; ne pas coder une seconde simulation dans l’UI.
- Centraliser les dialogues avec identifiant, locuteur, déclencheur, priorité et délai de réutilisation.
- Centraliser durées, vitesses de transition et volumes pour faciliter les ajustements.
- Réutiliser un composant de portrait/dialogue pour l’animateur et l’adversaire.
- Garder le sandbox/éditeur existant utilisable ; isoler le POC dans une scène ou un parcours approprié après lecture de l’architecture.
- Ne pas introduire une dépendance payante, un service distant ou une génération d’images à chaque lancement.

## 8. Premier travail attendu dans Godot

1. Faire une revue ciblée du vrai flux de jeu et résumer les écarts avec les hypothèses de cette note.
2. Proposer une première tranche très petite avec les scènes et fichiers réellement concernés ; demander uniquement les décisions de gameplay bloquantes.
3. Implémenter une boucle complète réutilisant les vrais choix et la vraie simulation, avec portraits temporaires si nécessaire.
4. Exécuter le projet et vérifier : Next, validation anticipée, chrono à zéro, verrouillage des choix, lancement unique de la simulation, résultat, rejeu, absence de dialogues simultanés ou de timers résiduels.
5. Montrer une courte capture vidéo ou un parcours exécutable. Puis itérer avec l’utilisateur sur les points qu’il n’aime pas, sans présenter la première version comme une DA finale.

Le critère de succès est une séquence courte, vivante et facile à ajuster, qui donne un sentiment de jeu vidéo **tout en conservant la simulation d’évolution existante**.

## Références visuelles issues de cette conversation

Ces liens permettent à l’utilisateur de retrouver les images ; un agent local ne pourra pas forcément les ouvrir sans qu’elles soient téléchargées dans le dépôt.

- [Plateau épuré : Dots and Dialogue](https://chatgpt.com/library?file_id=libfile_6d4ad68db8548191a2a4cbc4aaa73cff&account_id=personal) — composition mieux reçue, personnages jugés trop bon enfant.
- [Huit portraits pixel](https://chatgpt.com/library?file_id=libfile_7754eb69b34c8191a8c42c11b4f10d14&account_id=personal) — exploration plus mordante, à itérer.

Ne pas utiliser comme cible les premières images d’arènes industrielles et de créatures : elles ont été rejetées.
