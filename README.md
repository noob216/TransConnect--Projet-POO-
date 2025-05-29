```markdown
# TransConnect-CSharp

Une application **console** en C# pour gérer une petite flotte de transport pour une entreprise fictive TransConnect:  
- **Clients** (CRUD, historique de commandes)  
- **Salariés** (embauche, hiérarchie, licenciement)  
- **Commandes** (itinéraires, calcul de prix, suivi)  
- **Statistiques** (livraisons, CA, top clients)  
- **Graphe** des distances entre villes (BFS, DFS, plus court chemin, Floyd-Warshall)  
- **Persistance CSV** simple pour clients, salariés et commandes  

---

## 🚀 Prérequis

- [.NET 6 SDK (ou supérieur)](https://dotnet.microsoft.com/download)  
- Visual Studio 2022 (Community/Pro/Enterprise) ou VS Code  
- (Optionnel) [GraphViz](https://graphviz.org/) pour exporter un organigramme en DOT  

---

## 📁 Structure du dépôt

```markdown

/TransConnect-CSharp
├─ ClientConnect.sln
├─ TransConnect/              # projet principal
│  ├─ Program.cs
│  ├─ Domain/                 # entités métier (Client, Salarie, Commande, Graphe…)
│  ├─ DAL/                    # interfaces & repositories (Csv… + helpers)
│  ├─ BLL/                    # services métier & stratégies
│  ├─ UI/                     # modules console, AsciiOrgChart…
│  ├─ Data/                   # CSV d’exemple : clients.csv, salaries.csv, commandes.csv
│  └─ …
├─ .gitignore
└─ README.md

````

---

## ⚙️ Installation et exécution

1. **Cloner** ce dépôt  
   ```bash
   git clone https://github.com/noob216/TransConnect--Projet-POO-.git
   cd TransConnect--Projet-POO-
````

2. **Ouvrir** la solution

   * Avec Visual Studio : double-clic sur `ClientConnect.sln`
   * Avec VS Code : `code .`

3. **Restaurer** les packages NuGet et **compiler**

   ```bash
   dotnet restore
   dotnet build
   ```

4. **Initialiser** les CSV d’exemple (optionnel, une seule fois)

   ```csharp
   // Dans Program.cs, avant d’instancier les services :
   CsvSeeder.Seed();
   ```

5. **Lancer** l’application

   ```bash
   dotnet run --project TransConnect/TransConnect.csproj
   ```

---

## 🔧 Utilisation

Au lancement, un **menu principal** propose :

1. **Clients** : CRUD, recherche, top N clients, historique
2. **Salariés** : liste, embauche, licenciement, hiérarchie (ASCII ou GraphViz)
3. **Commandes** : création (calcul itinéraire), consultation, filtres
4. **Statistiques** : livraisons par chauffeur, CA, moyenne, périodes
5. **Distances/Graphe** : BFS, DFS, plus court chemin, matrice Floyd-Warshall
6. **Quitter**

Les sous-menus détaillent les opérations disponibles.

---

## 📂 Persistance CSV

* **Clients** : `clients.csv`
* **Salariés** : `salaries.csv`
* **Commandes** : `commandes.csv`

Le dossier `TransConnect/Data/` contient des jeux de données exemples. Les modifications (Add/Update/Delete) sont automatiquement répercutées dans les CSV.

---

## 🛠️ Bonnes pratiques

* Toutes les opérations métiers passent par des **services** (`ClientService`, `SalarieService`, `CommandeService`).
* Les données sont découpées en **repositories** implémentant `IRepository<T>`, facilement substituables.
* Les algorithmes de plus court chemin sont **stratégiques** (`IPlusCourtCheminStrategy`).
* L’**organigramme ASCII** est généré via `AsciiOrgChart`.
* Les **tests** console sont disponibles dans `tests/` pour valider chaque brique.

---

---

## 📄 Licence

Copyright © [2025] [ALI Mathis]

Tous droits réservés. Ce logiciel et son code source sont la propriété de [ALI Mathis]. 
Toute reproduction, distribution, modification ou utilisation de ce code, en tout ou en partie, est strictement interdite sans une autorisation écrite préalable.

Pour toute demande d'utilisation, contactez : [mojodream91@gmail.com].

```
