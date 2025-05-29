using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    // Représente une commande de livraison.
    public class Commande
    {
        public string IdCommande { get; }

        //Client ayant passé la commande.
        public Client Client { get; set; }

        //Chauffeur assigné à la livraison.
        public Salarie Chauffeur { get; set; }

        //Véhicule utilisé pour la livraison
        public Vehicule Vehicule { get; set; }
        public DateTime DateLivraison { get; set; }
        public decimal Prix { get; set; }

        //Indique si la commande a été livrée.</summary>
        public bool EstLivree { get; set; }

        // Constructeur par défaut
        public Commande()
        {
            IdCommande = "CMD000";
            Client = new Client();
            Chauffeur = new Salarie();
            Vehicule = null;
            DateLivraison = DateTime.Now;
            Prix = 0m;
            EstLivree = false;
        }

        // 2) Constructeur complet
        public Commande(
            string idCommande,
            Client client,
            Salarie chauffeur,
            Vehicule vehicule,
            DateTime dateLivraison,
            decimal prix,
            bool estLivree
        )
        {
            if (string.IsNullOrWhiteSpace(idCommande))
                throw new ArgumentException("L'identifiant de la commande est requis.", nameof(idCommande));
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (chauffeur == null)
                throw new ArgumentNullException(nameof(chauffeur));
            if (vehicule == null)
                throw new ArgumentNullException(nameof(vehicule));
            if (prix < 0m)
                throw new ArgumentOutOfRangeException(nameof(prix), "Le prix doit être positif ou nul.");

            IdCommande = idCommande;
            Client = client;
            Chauffeur = chauffeur;
            Vehicule = vehicule;
            DateLivraison = dateLivraison;
            Prix = prix;
            EstLivree = estLivree;
        }

        // 3) Constructeur minimal sans date, prix et statut
        public Commande(
            string idCommande,
            Client client,
            Salarie chauffeur,
            Vehicule vehicule
        )
        {
            if (string.IsNullOrWhiteSpace(idCommande))
                throw new ArgumentException("L'identifiant de la commande est requis.", nameof(idCommande));
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (chauffeur == null)
                throw new ArgumentNullException(nameof(chauffeur));
            if (vehicule == null)
                throw new ArgumentNullException(nameof(vehicule));

            IdCommande = idCommande;
            Client = client;
            Chauffeur = chauffeur;
            Vehicule = vehicule;
            DateLivraison = DateTime.Now;
            Prix = 0m;
            EstLivree = false;
        }

        public override string ToString()
        {
            string vehiculeInfo = Vehicule != null ? Vehicule.ToString() : "—";
            return $"Commande {IdCommande} | Client : {Client.Nom} {Client.Prenom} | " + $"Chauffeur : {Chauffeur.Nom} {Chauffeur.Prenom} | Véhicule : {vehiculeInfo} | " + $"Date : {DateLivraison:dd/MM/yyyy} | Prix : {Prix:C} | Livrée : {EstLivree}";
        }
    }

    //Service pour la gestion des commandes : création, règles de disponibilité, calcul tarifaire et requêtes.
    public class CommandeService
    {
        //readonly car ne doit pas pouvoir être modiffié en dehors du constructeur
        private readonly IRepository<Commande> _commandeRepo;
        private readonly IRepository<Client> _clientRepo;
        private readonly IRepository<Salarie> _salarieRepo;
        private readonly Graphe _graphe;
        private readonly IPlusCourtCheminStrategy _strategy;
        private readonly decimal _tarifParKm;

        //Constructeur
        public CommandeService(
            IRepository<Commande> commandeRepo,
            IRepository<Client> clientRepo,
            IRepository<Salarie> salarieRepo,
            Graphe graphe,
            IPlusCourtCheminStrategy strategy,
            decimal tarifParKm)
        {
            _commandeRepo = commandeRepo ?? throw new ArgumentNullException(nameof(commandeRepo));
            _clientRepo = clientRepo ?? throw new ArgumentNullException(nameof(clientRepo));
            _salarieRepo = salarieRepo ?? throw new ArgumentNullException(nameof(salarieRepo));
            _graphe = graphe ?? throw new ArgumentNullException(nameof(graphe));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _tarifParKm = tarifParKm;
        }

        //Crée une nouvelle commande en respectant les règles (disponibilité, tarification, historique)
        public Commande CreateCommande(
            string idCommande,
            string numeroClient,
            string numeroChauffeur,
            Vehicule vehicule,
            string villeDepart,
            string villeArrivee,
            DateTime dateLivraison)
        {
            if (string.IsNullOrWhiteSpace(idCommande))
                throw new ArgumentException("Identifiant de commande requis.", nameof(idCommande));
            if (_commandeRepo.GetById(idCommande) != null)
                throw new InvalidOperationException("Une commande avec cet ID existe déjà.");

            // Récupération ou création du client
            var client = _clientRepo.GetById(numeroClient) ?? new Client(numeroClient, "NomDefaut", "PrenomDefaut", default, "Standard");
            if (_clientRepo.GetById(numeroClient) == null)
                _clientRepo.Add(client);

            // Récupération du chauffeur
            var chauffeur = _salarieRepo.GetById(numeroChauffeur) ?? throw new KeyNotFoundException("Chauffeur introuvable.");

            // Vérification disponibilité du chauffeur
            bool dejaOccupe = _commandeRepo.GetAll().Any(c => c.Chauffeur.NumeroSS == numeroChauffeur && c.DateLivraison.Date == dateLivraison.Date);
            if (dejaOccupe)
                throw new InvalidOperationException("Le chauffeur n'est pas disponible à cette date.");

            // Calcul du chemin et de la distance
            var noeudDepart = new Noeud(villeDepart);
            var noeudArrivee = new Noeud(villeArrivee);
            var chemin = _strategy.Calculer(noeudDepart, noeudArrivee, _graphe) ?? new List<Lien>();
            double distance = chemin.Sum(l => l.DistanceKm);

            // Calcul du prix
            decimal prix = (decimal)distance * _tarifParKm;

            // Création de la commande
            var cmd = new Commande(
                idCommande,
                client,
                chauffeur,
                vehicule,
                dateLivraison,
                prix,
                estLivree: false);

            // Persistance
            _commandeRepo.Add(cmd);

            // Mise à jour de l'historique client
            client.AjouterCommande(cmd);
            _clientRepo.Update(client);

            return cmd;
        }

        //Retourne toutes les commandes.
        public IEnumerable<Commande> GetAllCommandes()
        {
            return _commandeRepo.GetAll();
        }

        //Recherche les commandes d'un chauffeur.
        public IEnumerable<Commande> GetCommandesByChauffeur(string numeroChauffeur)
        {
            return _commandeRepo.GetAll().Where(c => c.Chauffeur.NumeroSS == numeroChauffeur);
        }

        // Recherche les commandes entre deux dates
        public IEnumerable<Commande> GetCommandesByPeriod(DateTime from, DateTime to)
        {
            return _commandeRepo.GetAll().Where(c => c.DateLivraison.Date >= from.Date && c.DateLivraison.Date <= to.Date);
        }

        //Recherche une commande par ID.
        public Commande GetCommandeById(string id)
        {
            return _commandeRepo.GetById(id);
        }
    }

    // Builder pour créer une instance de Commande pas à pas.
    public class CommandeBuilder
    {
        private string _id;
        private Client _client;
        private Salarie _chauffeur;
        private Vehicule _vehicule;
        private DateTime _dateLivraison;
        private List<Lien> _itineraire = new List<Lien>();
        private decimal _prix;
        private bool _estLivree;

        private readonly Graphe _graphe;
        private readonly IPlusCourtCheminStrategy _strategy;
        private readonly decimal _tarifParKm;

        // Initialise le builder avec le graphe, la stratégie de chemin et le tarif par km
        public CommandeBuilder(Graphe graphe, IPlusCourtCheminStrategy strategy, decimal tarifParKm)
        {
            _graphe = graphe ?? throw new ArgumentNullException(nameof(graphe));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            if (tarifParKm <= 0) throw new ArgumentOutOfRangeException(nameof(tarifParKm), "Le tarif par km doit être positif.");
            _tarifParKm = tarifParKm;
        }

        public CommandeBuilder SetId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("L'identifiant de la commande est requis.", nameof(id));
            _id = id;
            return this;
        }

        public CommandeBuilder SetClient(Client client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            return this;
        }

        public CommandeBuilder SetChauffeur(Salarie chauffeur)
        {
            _chauffeur = chauffeur ?? throw new ArgumentNullException(nameof(chauffeur));
            return this;
        }

        public CommandeBuilder SetVehicule(Vehicule vehicule)
        {
            _vehicule = vehicule ?? throw new ArgumentNullException(nameof(vehicule));
            return this;
        }

        public CommandeBuilder SetDateLivraison(DateTime date)
        {
            _dateLivraison = date;
            return this;
        }

        //Calcule l'itinéraire (liste de liens) entre deux villes.
        public CommandeBuilder CalculateItineraire(string villeDepart, string villeArrivee)
        {
            if (string.IsNullOrWhiteSpace(villeDepart) || string.IsNullOrWhiteSpace(villeArrivee))
                throw new ArgumentException("Villes de départ et d'arrivée requises.");

            var dep = new Noeud(villeDepart);
            var arr = new Noeud(villeArrivee);
            _itineraire = _strategy.Calculer(dep, arr, _graphe)?.ToList() ?? new List<Lien>();
            return this;
        }

        // Calcule le prix en fonction de l'itinéraire et du tarif par km.
        public CommandeBuilder CalculatePrix()
        {
            double distanceTotal = _itineraire.Sum(l => l.DistanceKm);
            _prix = (decimal)distanceTotal * _tarifParKm;
            return this;
        }

        public CommandeBuilder MarkDelivered(bool estLivree = true)
        {
            _estLivree = estLivree;
            return this;
        }

        // Construit et retourne la Commande.
        public Commande Build()
        {
            if (string.IsNullOrWhiteSpace(_id))
                throw new InvalidOperationException("IdCommande non défini.");
            if (_client == null)
                throw new InvalidOperationException("Client non défini.");
            if (_chauffeur == null)
                throw new InvalidOperationException("Chauffeur non défini.");
            if (_vehicule == null)
                throw new InvalidOperationException("Véhicule non défini.");
            if (_dateLivraison == default)
                _dateLivraison = DateTime.Now;

            return new Commande(
                _id,
                _client,
                _chauffeur,
                _vehicule,
                _dateLivraison,
                _prix,
                _estLivree
            );
        }
    }
}
