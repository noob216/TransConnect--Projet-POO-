using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    // Helper générique pour lire et écrire des fichiers CSV avec un en-tête.
    // Les valeurs de chaque ligne sont séparées par des points-virgules.
    public static class CsvHelper
    {
        /// <summary>
        /// Lit toutes les lignes d'un CSV (s'il existe), ignore la première (en-tête),
        /// et désérialise chaque ligne en T via la fonction fournie.
        /// </summary>
        /// <typeparam name="T">Type des objets à retourner.</typeparam>
        /// <param name="filePath">Chemin du fichier CSV.</param>
        /// <param name="deserializer">
        /// Fonction prenant un tableau de chaînes (colonnes) et retournant une instance de T.
        /// </param>
        /// <returns>Liste d'objets T lus depuis le CSV.</returns>
        public static IEnumerable<T> ReadAll<T>(string filePath, Func<string[], T> deserializer)
        {
            if (!File.Exists(filePath))
                return Enumerable.Empty<T>();

            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
                return Enumerable.Empty<T>();

            // Skip header line
            return lines
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(';'))
                .Select(fields => deserializer(fields));
        }

        /// <summary>
        /// Écrit (ou réécrit) tout le CSV à partir de la collection d'objets fournie.
        /// </summary>
        /// <typeparam name="T">Type des objets à sérialiser.</typeparam>
        /// <param name="filePath">Chemin du fichier CSV.</param>
        /// <param name="items">Collection d'objets à écrire.</param>
        /// <param name="serializer">
        /// Fonction prenant un objet T et retournant un tableau de chaînes (colonnes).
        /// </param>
        /// <param name="headerLine">La ligne d'en-tête (ex. "Col1;Col2;Col3").</param>
        public static void WriteAll<T>(
            string filePath,
            IEnumerable<T> items,
            Func<T, string[]> serializer,
            string headerLine)
        {
            // Assure que le dossier existe
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(filePath, false);
            writer.WriteLine(headerLine);

            foreach (var item in items)
            {
                var fields = serializer(item);
                writer.WriteLine(string.Join(";", fields));
            }
        }

        /// <summary>
        /// Ajoute une seule ligne à un CSV, créant le fichier avec en-tête si nécessaire.
        /// </summary>
        /// <typeparam name="T">Type de l'objet à sérialiser.</typeparam>
        /// <param name="filePath">Chemin du fichier CSV.</param>
        /// <param name="item">Objet à ajouter.</param>
        /// <param name="serializer">Fonction de sérialisation en colonnes.</param>
        /// <param name="headerLine">Ligne d'en-tête à écrire si le fichier n'existe pas.</param>
        public static void AppendLine<T>(
            string filePath,
            T item,
            Func<T, string[]> serializer,
            string headerLine)
        {
            var exists = File.Exists(filePath);
            // Assure dossier
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(filePath, true);
            if (!exists)
                writer.WriteLine(headerLine);

            var fields = serializer(item);
            writer.WriteLine(string.Join(";", fields));
        }
    }
    // Interface générique pour un repository.
    public interface IRepository<T>
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(string id);
        T GetById(string id);
        IEnumerable<T> GetAll();
    }

    // Repository en mémoire pour les clients.
    public class CsvClientRepository : IRepository<Client>
    {
        private readonly string _filePath;
        private const string Header = "NumeroSS;Nom;Prenom;DateNaissance;AdressePostale;Email;Telephone;Statut";

        public CsvClientRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Chemin de fichier CSV requis.", nameof(filePath));
            _filePath = filePath;
        }

        public void Add(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            // AppendLine va créer le fichier avec en-tête si nécessaire
            CsvHelper.AppendLine(
                _filePath,
                entity,
                Serialize,
                Header
            );
        }

        public void Update(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var all = GetAll().ToList();
            var existing = all.FirstOrDefault(c => c.NumeroSS == entity.NumeroSS)
                ?? throw new KeyNotFoundException("Client introuvable.");
            // Remplace l'ancien
            all.Remove(existing);
            all.Add(entity);
            // Réécrit tout le fichier
            CsvHelper.WriteAll(
                _filePath,
                all,
                Serialize,
                Header
            );
        }

        public void Delete(string id)
        {
            var all = GetAll().ToList();
            var client = all.FirstOrDefault(c => c.NumeroSS == id);
            if (client != null)
            {
                all.Remove(client);
                CsvHelper.WriteAll(
                    _filePath,
                    all,
                    Serialize,
                    Header
                );
            }
        }

        public Client GetById(string id)
        {
            return GetAll().FirstOrDefault(c => c.NumeroSS == id);
        }

        public IEnumerable<Client> GetAll()
        {
            return CsvHelper.ReadAll(
                _filePath,
                Deserialize
            );
        }

        // Transforme une ligne CSV en objet Client
        private static Client Deserialize(string[] fields)
        {
            // fields ordonnées : 0=NumeroSS,1=Nom,2=Prenom,3=DateNaissance,4=Adresse,5=Email,6=Telephone,7=Statut
            return new Client(
                numeroSS: fields[0],
                nom: fields[1],
                prenom: fields[2],
                dateNaissance: DateTime.Parse(fields[3]),
                adressePostale: fields[4],
                email: fields[5],
                telephone: fields[6],
                statut: fields[7],
                historiqueInitial: null
            );
        }

        // Transforme un objet Client en tableau de valeurs pour le CSV
        private static string[] Serialize(Client c)
        {
            return new[]
            {
                c.NumeroSS,
                c.Nom,
                c.Prenom,
                c.DateNaissance.ToString("yyyy-MM-dd"),
                c.AdressePostale,
                c.Email,
                c.Telephone,
                c.Statut
            };
        }
    }

    // Repository en mémoire pour les salariés.
    public class CsvSalarieRepository : IRepository<Salarie>
    {
        private readonly string _filePath;
        private const string Header = "NumeroSS;Nom;Prenom;DateNaissance;AdressePostale;Email;Telephone;DateEntree;Poste;Salaire;SuperieurSS";

        public CsvSalarieRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Chemin de fichier CSV requis.", nameof(filePath));
            _filePath = filePath;
        }

        public void Add(Salarie entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            CsvHelper.AppendLine(
                _filePath,
                entity,
                Serialize,
                Header
            );
        }

        public void Update(Salarie entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var all = GetAll().ToList();
            var existing = all.FirstOrDefault(s => s.NumeroSS == entity.NumeroSS)
                ?? throw new KeyNotFoundException("Salarié introuvable.");
            all.Remove(existing);
            all.Add(entity);
            CsvHelper.WriteAll(
                _filePath,
                all,
                Serialize,
                Header
            );
        }

        public void Delete(string id)
        {
            var all = GetAll().ToList();
            var sal = all.FirstOrDefault(s => s.NumeroSS == id);
            if (sal != null)
            {
                all.Remove(sal);
                CsvHelper.WriteAll(
                    _filePath,
                    all,
                    Serialize,
                    Header
                );
            }
        }

        public Salarie GetById(string id)
        {
            return GetAll().FirstOrDefault(s => s.NumeroSS == id);
        }

        public IEnumerable<Salarie> GetAll()
        {
            var temp = new List<(Salarie sal, string supId)>();

            if (!File.Exists(_filePath))
                return Enumerable.Empty<Salarie>();

            var lines = File.ReadAllLines(_filePath).Skip(1);
            int lineNum = 1;

            foreach (var line in lines)
            {
                lineNum++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var f = line.Split(';');
                if (f.Length < 10)
                {
                    Console.Error.WriteLine($"Ligne {lineNum} ignorée (colonnes insuffisantes).");
                    continue;
                }

                // Parse sécurisé des dates
                DateTime dateNaiss = DateTime.TryParse(f[3], out var dn)
                    ? dn
                    : DateTime.MinValue;

                DateTime dateEntree = DateTime.TryParse(f[7], out var de)
                    ? de
                    : DateTime.MinValue;

                // Parse du salaire
                decimal salaire = decimal.TryParse(f[9], out var sa)
                    ? sa
                    : 0m;

                // Construction du salarié
                var sal = new Salarie(
                    numeroSS: f[0],
                    nom: f[1],
                    prenom: f[2],
                    dateNaissance: dateNaiss,
                    adressePostale: f.Length > 4 ? f[4] : "",
                    email: f.Length > 5 ? f[5] : "",
                    telephone: f.Length > 6 ? f[6] : "",
                    dateEntree: dateEntree,
                    poste: f.Length > 8 ? f[8] : "",
                    salaire: salaire
                );

                // Supérieur (colonne 10) s’il existe
                var supId = f.Length > 10 ? f[10] : "";
                temp.Add((sal, supId));
            }

            // Liaison des supérieurs
            var lookup = temp.ToDictionary(t => t.sal.NumeroSS, t => t.sal);
            foreach (var (sal, supId) in temp)
            {
                if (!string.IsNullOrWhiteSpace(supId) && lookup.TryGetValue(supId, out var sup))
                    sal.DefinirSuperieur(sup);
            }

            return lookup.Values;
        }



        private static Salarie Deserialize(string[] f)
        {
            // Champs : 
            // 0=NumeroSS,1=Nom,2=Prenom,3=DateNaissance,
            // 4=AdressePostale,5=Email,6=Telephone,
            // 7=DateEntree,8=Poste,9=Salaire,10=SuperieurSS
            var sal = new Salarie(
                numeroSS: f[0],
                nom: f[1],
                prenom: f[2],
                dateNaissance: DateTime.Parse(f[3]),
                adressePostale: f[4],
                email: f[5],
                telephone: f[6],
                dateEntree: DateTime.Parse(f[7]),
                poste: f[8],
                salaire: decimal.TryParse(f[9], out var s) ? s : 0m
            );

            // Stocke l'ID du supérieur en placeholder pour la deuxième passe
            if (!string.IsNullOrWhiteSpace(f[10]))
            {
                // On crée juste un objet temporaire avec l'ID, 
                // il sera lié correctement dans la passe suivante
                sal.DefinirSuperieur(new Salarie(
                    numeroSS: f[10],
                    nom: "",
                    prenom: "",
                    dateNaissance: DateTime.MinValue,
                    adressePostale: "",
                    email: "",
                    telephone: "",
                    dateEntree: DateTime.MinValue,
                    poste: "",
                    salaire: 0m
                ));
            }

            return sal;
        }


        private static string[] Serialize(Salarie s)
        {
            var superId = s.Superieur?.NumeroSS ?? "";
            return new[]
            {
                s.NumeroSS,
                s.Nom,
                s.Prenom,
                s.DateNaissance.ToString("yyyy-MM-dd"),
                s.AdressePostale,
                s.Email,
                s.Telephone,
                s.DateEntree.ToString("yyyy-MM-dd"),
                s.Poste,
                s.Salaire.ToString(),
                superId
            };
        }
    }

    //Repository en mémoire pour les commandes.

    public class CsvCommandeRepository : IRepository<Commande>
    {
        private readonly string _filePath;
        private readonly IRepository<Client> _clientRepo;
        private readonly IRepository<Salarie> _salarieRepo;
        private const string Header = "IdCommande;NumeroClient;NumeroChauffeur;VehiculeType;VehiculeId;VehiculeCapacite;DateLivraison;Prix;EstLivree";


        public CsvCommandeRepository(
            string filePath,
            IRepository<Client> clientRepo,
            IRepository<Salarie> salarieRepo)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Chemin de fichier CSV requis.", nameof(filePath));
            _filePath = filePath;
            _clientRepo = clientRepo ?? throw new ArgumentNullException(nameof(clientRepo));
            _salarieRepo = salarieRepo ?? throw new ArgumentNullException(nameof(salarieRepo));
        }

        public void Add(Commande entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            CsvHelper.AppendLine(
                _filePath,
                entity,
                Serialize,
                Header
            );
        }

        public void Update(Commande entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var all = GetAll().ToList();
            var existing = all.FirstOrDefault(c => c.IdCommande == entity.IdCommande)
                ?? throw new KeyNotFoundException("Commande introuvable.");
            all.Remove(existing);
            all.Add(entity);
            CsvHelper.WriteAll(
                _filePath,
                all,
                Serialize,
                Header
            );
        }

        public void Delete(string id)
        {
            var all = GetAll().ToList();
            var cmd = all.FirstOrDefault(c => c.IdCommande == id);
            if (cmd != null)
            {
                all.Remove(cmd);
                CsvHelper.WriteAll(
                    _filePath,
                    all,
                    Serialize,
                    Header
                );
            }
        }

        public Commande GetById(string id)
        {
            return GetAll().FirstOrDefault(c => c.IdCommande == id);
        }

        public IEnumerable<Commande> GetAll()
        {
            return CsvHelper.ReadAll(
                _filePath,
                Deserialize
            );
        }

        private Commande Deserialize(string[] f)
        {
            // Maintenant : 
            // 0=IdCommande
            // 1=NumeroClient
            // 2=NumeroChauffeur
            // 3=VehiculeType
            // 4=VehiculeId
            // 5=VehiculeCapacite
            // 6=DateLivraison
            // 7=Prix
            // 8=EstLivree

            var id = f[0];
            var client = _clientRepo.GetById(f[1])
                               ?? throw new KeyNotFoundException($"Client {f[1]} introuvable.");
            var chauffeur = _salarieRepo.GetById(f[2])
                               ?? throw new KeyNotFoundException($"Chauffeur {f[2]} introuvable.");

            // Nouvelles lignes :
            if (!int.TryParse(f[5], out var capacite) || capacite <= 0)
                throw new FormatException($"Capacité invalide dans CSV pour la commande {id}.");

            var vehicule = VehiculeFactory.CreateVehicule(
                f[3],        // type
                f[4],        // id
                capacite,    // la capacité correcte
                new Dictionary<string, object>()
            );

            var dateLiv = DateTime.Parse(f[6]);
            // Le prix est en format français (virgule et non point)
            var textePrix = f[7].Replace('.', ',');
            var prix = decimal.Parse(textePrix, CultureInfo.GetCultureInfo("fr-FR"));
            var estLivree = bool.Parse(f[8]);

            return new Commande(
                id,
                client,
                chauffeur,
                vehicule,
                dateLiv,
                prix,
                estLivree
            );
        }

        private string[] Serialize(Commande c)
        {
            return new[]
            {
                c.IdCommande,
                c.Client.NumeroSS,
                c.Chauffeur.NumeroSS,
                c.Vehicule.GetType().Name.ToLowerInvariant(), // correspond au type attendu par la factory
                c.Vehicule.Id,
                c.Vehicule.Capacite.ToString(),
                c.DateLivraison.ToString("yyyy-MM-dd"),
                c.Prix.ToString(),
                c.EstLivree.ToString()
            };
        }
    }

    // Repository pour charger un graphe
    public static class GrapheRepository
    {
        public static Graphe LoadFromCsv(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Le chemin du fichier est requis.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Fichier CSV introuvable.", filePath);

            var graphe = new Graphe();
            var lines = File.ReadAllLines(filePath).Skip(1);
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length < 3) continue;

                var v1 = parts[0].Trim();
                var v2 = parts[1].Trim();
                if (!double.TryParse(parts[2].Trim(), out var dist))
                    continue;

                var n1 = graphe.Noeuds.FirstOrDefault(n => n.Ville == v1) ?? new Noeud(v1);
                var n2 = graphe.Noeuds.FirstOrDefault(n => n.Ville == v2) ?? new Noeud(v2);

                graphe.AjouterNoeud(n1);
                graphe.AjouterNoeud(n2);
                graphe.AjouterLien(new Lien(n1, n2, dist));
            }
            return graphe;
        }
    }
}
