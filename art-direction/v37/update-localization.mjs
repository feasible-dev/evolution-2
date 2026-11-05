import fs from 'node:fs/promises';
const en={
 'card.direction.title':'Direction','card.direction.effect':'Pushes along a fixed angle.','card.direction.heading':'An absolute world direction.',
 'card.direction.line1':'The angle is inherited','card.direction.line2':'and can mutate. No target.','card.direction.summary':'Same push. Evolving angle.',
 'behavior.direction':'DIRECTION','behavior.direction_help':'Pushes along an inherited world angle: 44 u/s² for both teams. The angle mutates; force does not. No target or region sensing.',
 'card.thrust.effect':'Pushes straight ahead.','card.thrust.line1':'Fixed force along the','card.thrust.line2':'agent’s current heading.','card.thrust.summary':'Forward push: 220 u/s².',
 'card.turn.line1':'The turning sense is inherited;','card.turn.line2':'the strength stays fixed.','card.turn.summary':'Rotation: 1 rad/s.',
 'board.direction':' / angle {0:0}° (0° right, 90° down)',
 'physics.summary':'Mean speed {0:0.0} u/s\nEnergy remaining {1:0}%\nDirection push {5:0.0} u/s²\nMass {3:0.00} | Drag {4:0.00}',
 'physics.help':'Both teams share direction push 44 u/s², forward thrust 220 u/s², turn rate 1 rad/s and decisions every 0.2 s. Direction is an inherited absolute angle, not a target. Energy, drag and walls affect actual motion. Speed limit: {0:0} u/s. Exhausted: {1}.',
 'speech.ninetyfive':'Ninety-five percent! Now that is a generation worth standing up for.',
 'speech.ninetyfive_rene':'{0} reached {1:0.0}% survival. A strong result; the remaining agents still matter.',
 'speech.wipeout':'No survivors for {0} this generation. The population will be rebuilt. We need a different approach.',
 'speech.focus':'{0:0}% of the agents are gathered in one area. That is where the action is now.',
 'speech.follow':'I am following that little wanderer. A promising direction… or a very confident mistake.',
 'speech.direction_coach':'They do not know where safety is. They inherit a direction; selection keeps the ones that work.',
 'speech.calm_gerard':'Somewhere in there is the next prodigy. I have time. Let us watch.',
 'plan.help':'Action cards only. Points set draw weights; action strength is fixed.'
};
const fr={
 'card.direction.title':'Direction','card.direction.effect':'Pousse selon un angle fixe.','card.direction.heading':'Une direction absolue.',
 'card.direction.line1':'L’angle est hérité','card.direction.line2':'et peut muter. Sans cible.','card.direction.summary':'Même force. Angle évolutif.',
 'behavior.direction':'DIRECTION','behavior.direction_help':'Pousse selon un angle absolu hérité : 44 u/s² pour les deux équipes. L’angle mute, pas la force. Aucune perception de cible ni de région.',
 'card.thrust.effect':'Pousse droit devant.','card.thrust.line1':'Une force fixe selon','card.thrust.line2':'le cap actuel de l’agent.','card.thrust.summary':'Poussée avant : 220 u/s².',
 'card.turn.line1':'Le sens de rotation est hérité ;','card.turn.line2':'la force reste fixe.','card.turn.summary':'Rotation : 1 rad/s.',
 'board.direction':' / angle {0:0}° (0° droite, 90° bas)',
 'physics.summary':'Vitesse moy. {0:0.0} u/s\nÉnergie restante {1:0}%\nPoussée direction {5:0.0} u/s²\nMasse {3:0.00} | Traînée {4:0.00}',
 'physics.help':'Les deux équipes partagent une poussée directionnelle de 44 u/s², une poussée avant de 220 u/s², une rotation de 1 rad/s et une décision toutes les 0,2 s. La direction est un angle absolu hérité, sans cible. Énergie, traînée et murs modifient le mouvement réel. Vitesse limite : {0:0} u/s. Agents épuisés : {1}.',
 'speech.ninetyfive':'Quatre-vingt-quinze pour cent ! Voilà une génération qui mérite qu’on se lève.',
 'speech.ninetyfive_rene':'{0} atteint {1:0.0}% de survie. Un beau résultat ; les agents restants comptent aussi.',
 'speech.wipeout':'Aucun survivant pour {0} dans cette génération. La population sera reconstruite. Il faudra revoir l’approche.',
 'speech.focus':'{0:0}% des agents sont rassemblés au même endroit. C’est là que ça se passe.',
 'speech.follow':'Je suis ce petit vagabond. Une direction prometteuse… ou une erreur très assumée.',
 'speech.direction_coach':'Ils ne savent pas où est la sécurité. Ils héritent d’une direction ; la sélection garde celles qui fonctionnent.',
 'speech.calm_gerard':'Le prochain prodige est peut-être là-dedans. J’ai le temps. Regardons.',
 'plan.help':'Cartes d’action seulement. Les points règlent le tirage, pas la force.'
};
for(const [lang,values] of Object.entries({en,fr})){
 const file=`prototype/localization/${lang}.json`;
 const old=JSON.parse(await fs.readFile(file,'utf8'));
 const data=Object.fromEntries(Object.entries(old).map(([k,v])=>[k.replace('card.target.','card.direction.').replace('behavior.target','behavior.direction').replace('board.target','board.direction'),v]));
 Object.assign(data,values);await fs.writeFile(file,JSON.stringify(data,null,2)+'\n');
}
