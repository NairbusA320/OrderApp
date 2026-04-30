Application console de gestion de commandes

Les données sont actuellement inscrites en dur dans le code, mais il est parfaitement envisageable de changer la manière de les récupérer (via un fichier CSV ou BDD)
Les données se découpent en 3 parties : 

1) Une liste de commandes, dont chaque commande est composée de :
  - Un ID
  - Un prénom
  - Un nom
  - Un montant
  - Une entreprise
  - Un flag servant à savoir si elle a été payée
  - Un flag servant à savoir si elle a été expediée

2) Une liste liant les entreprises et le l'urgence d'expédition, composée de :
  - Une entreprise
  - Un type d'expédition (Urgent, Prioritaire, Normal, Lent)

3) Une liste de coût d'expédition par type, composé de :
 - Un type d'expédition
 - Un montant

Ensuite, l'application permet 9 actions :
  1. Lister toutes les commandes          => Liste toutes les commandes du système, par ordre d'ID (donc potentiellement par ordre de création ?)
  2. Rechercher une commande              => Rechercher une commande (par numéro, nom ou entreprise)
  3. Filtrer par statut (payé / envoyé)   => Affiche un sous-ensemble des commandes (payées, impayées, envoyées, non envoyées)
  4. Commandes à risque                   => Affiche les commandes à risque, à savoir les commandes envoyées mais non payées (par ordre de montant descendant)
  5. Tableau de bord (4 cadrans)          => Affiche un tableau de bord 4 cadrans : Nombre de commandes selon le statut (payé,envoyé), CA encaissé, le montant à recouvrer et le montant à risque
  6. File d'expédition (à envoyer)        => Affiche les commandes à envoyer (uniquement les payées, puisque les non payées doivent être payées avant d'être expediées), par priorité d'expédition 
  7. Statistiques financières             => Affiche les statistiques financières (CA, recouvrement, à risque, coût des expéditions, ...
  8. Top clients & entreprises            => Affiche le top client (par montant) et le top entreprise (par nombre de commandes)
  9. Plan de relances priorisé            => Génère le plan de relance priorisé selon le statut (payé, envoyé), le montant de la commande, ...
