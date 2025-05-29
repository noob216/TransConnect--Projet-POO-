using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace projetc_
{
    //Représente un graphe orienté (ou non) de villes avec distances
    public class Graphe
    {
        public List<Noeud> Noeuds { get; set; }
        public List<Lien> Liens { get; set; }

        // Constructeur par défaut : graphe vide
        public Graphe()
        {
            Noeuds = new List<Noeud>();
            Liens = new List<Lien>();
        }

        // Constructeur complet : initialisation de tous les nœuds et liens
        public Graphe(IEnumerable<Noeud> noeuds, IEnumerable<Lien> liens)
        {
            Noeuds = new List<Noeud>(noeuds ?? throw new ArgumentNullException(nameof(noeuds)));
            Liens = new List<Lien>(liens ?? throw new ArgumentNullException(nameof(liens)));
        }

        public void AjouterNoeud(Noeud n)
        {
            if (n == null)
            {
                throw new ArgumentNullException(nameof(n));
            }
            if (!Noeuds.Contains(n)) Noeuds.Add(n);
        }
        public void AjouterLien(Lien l)
        {
            if (l == null)
            {
                throw new ArgumentNullException(nameof(l));
            }
            if (!Noeuds.Contains(l.Depart) || !Noeuds.Contains(l.Arrivee))
            {
                throw new ArgumentException("Les nœuds de l'arête doivent être présents dans le graphe.");
            }
            Liens.Add(l);
        }

        public override string ToString()
        {
            return $"Graphe : {Noeuds.Count} nœuds, {Liens.Count} liens";
        }

        public IEnumerable<Noeud> ParcoursBFS(Noeud depart)
        {
            if (depart == null) throw new ArgumentNullException(nameof(depart));
            var visites = new HashSet<Noeud>();
            var file = new Queue<Noeud>();
            visites.Add(depart);
            file.Enqueue(depart);

            while (file.Count > 0)
            {
                var actuel = file.Dequeue();
                yield return actuel;

                foreach (var voisin in Liens.Where(l => l.Depart == actuel).Select(l => l.Arrivee))
                {
                    if (visites.Add(voisin)) file.Enqueue(voisin);
                }
            }
        }
        public IEnumerable<Noeud> ParcoursDFS(Noeud depart)
        {
            if (depart == null) throw new ArgumentNullException(nameof(depart));
            var visites = new HashSet<Noeud>();
            return DFSRec(depart, visites);
        }

        private IEnumerable<Noeud> DFSRec(Noeud current, HashSet<Noeud> visites)
        {
            visites.Add(current);
            yield return current;

            foreach (var voisin in Liens
                 .Where(l => l.Depart == current)
                 .Select(l => l.Arrivee))
            {
                if (visites.Add(voisin))
                {
                    foreach (var n in DFSRec(voisin, visites))
                        yield return n;
                }
            }
        }

        public List<Lien> Dijkstra(Noeud source, Noeud cible)
        {
            if (source == null || cible == null)
                throw new ArgumentNullException("Les noeuds de départ et d'arrivé doivent être non nulles.");

            var dist = Noeuds.ToDictionary(n => n, n => double.PositiveInfinity);
            var pred = new Dictionary<Noeud, Lien>();
            var nonVisite = new HashSet<Noeud>(Noeuds);

            dist[source] = 0;

            while (nonVisite.Count > 0)
            {
                var u = nonVisite.OrderBy(n => dist[n]).First();
                nonVisite.Remove(u);

                if (dist[u] == double.PositiveInfinity || u == cible)
                    break;

                foreach (var lien in Liens.Where(l => l.Depart == u))
                {
                    var v = lien.Arrivee;
                    var alt = dist[u] + lien.DistanceKm;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        pred[v] = lien;
                    }
                }
            }

            // Reconstruction du chemin
            var chemin = new List<Lien>();
            var cour = cible;
            while (pred.ContainsKey(cour))
            {
                var l = pred[cour];
                chemin.Insert(0, l);
                cour = l.Depart;
            }
            return chemin;
        }

        //Algorithme de Bellman–Ford pour calculer les distances depuis une source.
        //Retourne un dictionnaire {nœud → distance minimale} ou exception si cycle négatif.
        public Dictionary<Noeud, double> BellmanFord(Noeud source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var dist = Noeuds.ToDictionary(n => n, n => double.PositiveInfinity);
            dist[source] = 0;

            for (int i = 0; i < Noeuds.Count - 1; i++)
            {
                foreach (var lien in Liens)
                {
                    if (dist[lien.Depart] + lien.DistanceKm < dist[lien.Arrivee])
                        dist[lien.Arrivee] = dist[lien.Depart] + lien.DistanceKm;
                }
            }

            // Détection de cycle négatif
            foreach (var lien in Liens)
            {
                if (dist[lien.Depart] + lien.DistanceKm < dist[lien.Arrivee])
                    throw new InvalidOperationException("Cycle de poids négatif détecté.");
            }

            return dist;
        }
        //Algorithme de Floyd–Warshall pour toutes les paires.
        //Retourne une matrice [i,j] des distances minimales entre Noeuds[i] et Noeuds[j].
        public double[,] FloydWarshall()
        {
            int n = Noeuds.Count;
            var index = Noeuds.Select((node, idx) => (node, idx))
                              .ToDictionary(p => p.node, p => p.idx);

            const double INF = double.PositiveInfinity;
            var D = new double[n, n];

            // Initialisation
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    D[i, j] = (i == j) ? 0 : INF;

            foreach (var lien in Liens)
            {
                int u = index[lien.Depart];
                int v = index[lien.Arrivee];
                D[u, v] = lien.DistanceKm;
            }

            // Itération principale
            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        if (D[i, k] + D[k, j] < D[i, j])
                            D[i, j] = D[i, k] + D[k, j];

            return D;
        }
    }

    // Service pour les fonctionnalités du graphe : parcours, connexité, cycles et plus court chemin
    public class GrapheService
    {
        private readonly Graphe _graphe;
        //Constructeur : on injecte le graphe déjà chargé depuis le CSV.
        public GrapheService(Graphe graphe)
        {
            _graphe = graphe ?? throw new ArgumentNullException(nameof(graphe));
        }

        // Liste des villes (noms des nœuds).
        public IEnumerable<string> GetVilles()
        {
           return _graphe.Noeuds.Select(n => n.Ville);
        }


        // Liste des liens (départ, arrivée, distance).
        public IEnumerable<(string Depart, string Arrivee, double DistanceKm)> GetLiens()
        {
            return _graphe.Liens
                .Select(l =>
                {
                    // On trie les deux noms de ville pour qu’ils soient toujours dans le même ordre
                    var d = l.Depart.Ville;
                    var a = l.Arrivee.Ville;
                    if (string.Compare(d, a, StringComparison.OrdinalIgnoreCase) > 0)
                        (d, a) = (a, d);

                    return (Depart: d, Arrivee: a, l.DistanceKm);
                })
                // Distinct sur les value tuples enlève automatiquement les doublons
                .Distinct();
        }

        //pour skiasharp
        // Expose la liste interne des objets Noeud (avec toutes leurs propriétés).
        public IEnumerable<Noeud> GetNoeudsObjects()
        {
            return _graphe.Noeuds;
        }

        // Expose la liste interne des objets Lien.
        public IEnumerable<Lien> GetLiensObjects()
        {
            return _graphe.Liens;
        }

        // Parcours BFS à partir d’une ville (renvoie la liste des noms de villes).
        public IEnumerable<string> ParcoursBFS(string villeDepart)
        {
            if (string.IsNullOrWhiteSpace(villeDepart))
                throw new ArgumentException("Ville de départ requise", nameof(villeDepart));

            var depart = _graphe.Noeuds
                .FirstOrDefault(n => n.Ville.Equals(villeDepart, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"Ville '{villeDepart}' introuvable dans le graphe.");

            return _graphe.ParcoursBFS(depart)
                          .Select(n => n.Ville);
        }

        // Parcours DFS à partir d’une ville (renvoie la liste des noms de villes).
        public IEnumerable<string> ParcoursDFS(string villeDepart)
        {
            if (string.IsNullOrWhiteSpace(villeDepart))
                throw new ArgumentException("Ville de départ requise", nameof(villeDepart));

            var depart = _graphe.Noeuds
                .FirstOrDefault(n => n.Ville.Equals(villeDepart, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"Ville '{villeDepart}' introuvable dans le graphe.");

            return _graphe.ParcoursDFS(depart)
                          .Select(n => n.Ville);
        }

        // Vérifie si le graphe est connexe (un seul composant).
        public bool IsConnected()
        {
            if (!_graphe.Noeuds.Any())
                return true;

            var premier = _graphe.Noeuds.First();
            var visite = new HashSet<Noeud>(_graphe.ParcoursBFS(premier));
            return visite.Count == _graphe.Noeuds.Count;
        }

        // Détecte un cycle dans le graphe (DFS avec pile de récursion).
        public bool HasCycle()
        {
            var visited = new HashSet<Noeud>();
            var recStack = new HashSet<Noeud>();

            foreach (var node in _graphe.Noeuds)
            {
                if (DetectCycle(node, visited, recStack))
                    return true;
            }
            return false;
        }

        private bool DetectCycle(Noeud node, HashSet<Noeud> visited, HashSet<Noeud> recStack)
        {
            if (!visited.Contains(node))
            {
                visited.Add(node);
                recStack.Add(node);

                foreach (var voisin in _graphe.Liens
                                              .Where(l => l.Depart == node)
                                              .Select(l => l.Arrivee))
                {
                    if (!visited.Contains(voisin) && DetectCycle(voisin, visited, recStack))
                        return true;
                    if (recStack.Contains(voisin))
                        return true;
                }
            }

            recStack.Remove(node);
            return false;
        }

        // Trouve le plus court chemin (liste de noms de villes) entre deux villes via la stratégie donnée.
        public IEnumerable<string> FindShortestPath(string villeDepart, string villeArrivee, IPlusCourtCheminStrategy strategy)
        {
            if (string.IsNullOrWhiteSpace(villeDepart) || string.IsNullOrWhiteSpace(villeArrivee))
                throw new ArgumentException("Villes de départ et d'arrivée requises.");

            var n1 = _graphe.Noeuds
                           .FirstOrDefault(n => n.Ville.Equals(villeDepart, StringComparison.OrdinalIgnoreCase))
                 ?? throw new KeyNotFoundException($"Ville de départ '{villeDepart}' introuvable.");

            var n2 = _graphe.Noeuds
                           .FirstOrDefault(n => n.Ville.Equals(villeArrivee, StringComparison.OrdinalIgnoreCase))
                 ?? throw new KeyNotFoundException($"Ville d'arrivée '{villeArrivee}' introuvable.");

            var liens = strategy.Calculer(n1, n2, _graphe) ?? new List<Lien>();

            // Reconstruction du chemin sous forme de noms de villes
            var path = new List<string>();
            if (liens.Any())
            {
                path.Add(liens.First().Depart.Ville);
                path.AddRange(liens.Select(l => l.Arrivee.Ville));
            }
            return path;
        }

        // Matrice des distances Floyd–Warshall pour toutes les paires de villes.
        public double[,] FloydWarshallMatrix()
            => _graphe.FloydWarshall();
    }

    //Interface pour les stratégies de calcul du plus court chemin.
    public interface IPlusCourtCheminStrategy
    {
        //Calcule le chemin minimal (liste de liens) entre deux nœuds dans un graphe.
        IEnumerable<Lien> Calculer(Noeud source, Noeud cible, Graphe graphe);
    }

    //Implémentations des stratégies de calcul du plus court chemin.
    public class DijkstraStrategy : IPlusCourtCheminStrategy
    {
        public IEnumerable<Lien> Calculer(Noeud source, Noeud cible, Graphe graphe)
        {
            if (source == null || cible == null || graphe == null)
                throw new ArgumentNullException("Arguments invalides pour DijkstraStrategy.");
            return graphe.Dijkstra(source, cible);
        }
    }
    public class BellmanFordStrategy : IPlusCourtCheminStrategy
    {
        public IEnumerable<Lien> Calculer(Noeud source, Noeud cible, Graphe graphe)
        {
            if (source == null || cible == null || graphe == null)
                throw new ArgumentNullException("Arguments invalides pour BellmanFordStrategy.");

            var dist = graphe.Noeuds.ToDictionary(n => n, n => double.PositiveInfinity);
            var pred = new Dictionary<Noeud, Lien>();
            dist[source] = 0;

            int n = graphe.Noeuds.Count;
            // Relaxation des arêtes
            for (int i = 0; i < n - 1; i++)
            {
                foreach (var lien in graphe.Liens)
                {
                    var u = lien.Depart;
                    var v = lien.Arrivee;
                    double alt = dist[u] + lien.DistanceKm;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        pred[v] = lien;
                    }
                }
            }
            // Détection de cycle négatif
            foreach (var lien in graphe.Liens)
            {
                if (dist[lien.Depart] + lien.DistanceKm < dist[lien.Arrivee])
                    throw new InvalidOperationException("Cycle de poids négatif détecté.");
            }

            // Reconstruction du chemin
            var chemin = new List<Lien>();
            var cour = cible;
            while (pred.ContainsKey(cour))
            {
                var l = pred[cour];
                chemin.Insert(0, l);
                cour = l.Depart;
            }
            return chemin;
        }
    }
    public class FloydWarshallStrategy : IPlusCourtCheminStrategy
    {
        public IEnumerable<Lien> Calculer(Noeud source, Noeud cible, Graphe graphe)
        {
            if (source == null || cible == null || graphe == null)
                throw new ArgumentNullException("Arguments invalides pour FloydWarshallStrategy.");

            // Initialisation
            var nodes = graphe.Noeuds;
            int n = nodes.Count;
            var index = nodes.Select((node, idx) => (node, idx))
                             .ToDictionary(p => p.node, p => p.idx);
            const double INF = double.PositiveInfinity;
            var dist = new double[n, n];
            var next = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = (i == j) ? 0 : INF;
                    next[i, j] = -1;
                }
            }
            // Arêtes
            foreach (var lien in graphe.Liens)
            {
                int u = index[lien.Depart];
                int v = index[lien.Arrivee];
                dist[u, v] = lien.DistanceKm;
                next[u, v] = v;
            }
            // Algorithme principal
            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            next[i, j] = next[i, k];
                        }
                    }
            // Reconstruction du chemin
            int s = index[source];
            int t = index[cible];
            if (next[s, t] == -1)
                return Enumerable.Empty<Lien>();

            var path = new List<Noeud> { source };
            int uidx = s;
            while (uidx != t)
            {
                uidx = next[uidx, t];
                path.Add(nodes[uidx]);
            }
            // Convertir en liste de liens
            var liensChemin = new List<Lien>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var u = path[i];
                var v = path[i + 1];
                var lien = graphe.Liens.First(l => l.Depart == u && l.Arrivee == v);
                liensChemin.Add(lien);
            }
            return liensChemin;
        }
    }

}
