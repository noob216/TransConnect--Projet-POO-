using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente un client de l'entreprise
    // Hérite de Personne, propose trois constructeurs (par défaut, complet et minimal)
    public class Client : Personne
    {
        // Historique des commandes passées par le client.
        public List<Commande> HistoriqueLivraisons { get; }

        // Statut du client (ex. "Standard", "Premium", etc.).
        public string Statut { get; set; }

        //Constructeur par défaut
        public Client()
            : base(
                numeroSS: "0000000000000",
                nom: "NomDefaut",
                prenom: "PrenomDefaut",
                dateNaissance: default,
                adressePostale: "AdresseDefaut",
                email: "email@exemple.com",
                telephone: "0000000000"
            )
        {
            Statut = "Standard";
            HistoriqueLivraisons = new List<Commande>();
        }

        // Constructeur complet : tous les attributs
        public Client(
            string numeroSS,
            string nom,
            string prenom,
            DateTime dateNaissance,
            string adressePostale,
            string email,
            string telephone,
            string statut,
            IEnumerable<Commande> historiqueInitial
        ) : base(numeroSS, nom, prenom, dateNaissance, adressePostale, email, telephone)
        {
            if (string.IsNullOrWhiteSpace(statut))
            {
                throw new ArgumentException("Le statut est requis.", nameof(statut));
            }

            Statut = statut;
            HistoriqueLivraisons = new List<Commande>(historiqueInitial ?? Array.Empty<Commande>());
        }

        // 3) Constructeur minimal : paramètres essentiels
        public Client(
            string numeroSS,
            string nom,
            string prenom,
            DateTime dateNaissance,
            string statut
        ) : base(
                numeroSS,
                nom,
                prenom,
                dateNaissance,
                adressePostale: "AdresseDefaut",
                email: "email@exemple.com",
                telephone: "0000000000"
            )
        {
            if (string.IsNullOrWhiteSpace(statut))
            {
                throw new ArgumentException("Le statut est requis.", nameof(statut));
            }

            Statut = statut;
            HistoriqueLivraisons = new List<Commande>();
        }

        /// <summary>
        /// Ajoute une commande à l'historique.
        /// </summary>
        public void AjouterCommande(Commande cmd)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));
            HistoriqueLivraisons.Add(cmd);
        }

        /// <summary>
        /// Affiche les informations clés du client.
        /// </summary>
        public override string ToString()
        {
            return $"{base.ToString()}, Statut : {Statut}, Commandes passées : {HistoriqueLivraisons.Count}";
        }
    }

    //Service pour la gestion des clients : tris, historique et opérations
    public class ClientService
    {
        private readonly IRepository<Client> _clientRepo;  // champs privé pour le dépôt de clients
        private readonly IRepository<Commande> _commandeRepo;

        // Listes en mémoire
        private readonly List<Client> _clients;
        private readonly List<Commande> _commandes;

        public ClientService(IRepository<Client> clientRepo, IRepository<Commande> commandeRepo)
        {
            _clientRepo = clientRepo ?? throw new ArgumentNullException(nameof(clientRepo));
            _commandeRepo = commandeRepo ?? throw new ArgumentNullException(nameof(commandeRepo));

            // Chargement en mémoire
            _clients = _clientRepo.GetAll().ToList();
            _commandes = _commandeRepo.GetAll().ToList();

            // Peuplement des historiques
            foreach (var cmd in _commandes)
            {
                var client = _clients.FirstOrDefault(c => c.NumeroSS == cmd.Client.NumeroSS);
                if (client != null)
                {
                    client.AjouterCommande(cmd);
                }
            }
        }

        // --- Lecture ---

        public IEnumerable<Client> GetAllClients()
        {
            return _clients;
        }



        public IEnumerable<Client> GetClientsSortedByName()
        {
            return _clients.OrderBy(c => c.Nom).ThenBy(c => c.Prenom);
        }

        public IEnumerable<Client> GetClientsByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("La ville est requise.", nameof(city));
            }
            return _clients.Where(c => !string.IsNullOrWhiteSpace(c.AdressePostale) && c.AdressePostale.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public IEnumerable<(Client Client, decimal TotalSpent)> GetTopClientsByTotalSpent(int topN)
        {
            if (topN <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(topN), "Le nombre doit être supérieur à zéro.");
            }

            var query = _clients.Select(c => (Client: c,TotalSpent: _commandes.Where(cmd => cmd.Client.NumeroSS == c.NumeroSS).Sum(cmd => cmd.Prix))).OrderByDescending(x => x.TotalSpent).Take(topN);
            return query;
        }

        public IEnumerable<Commande> GetHistoryForClient(string numeroSS)
        {
            if (string.IsNullOrWhiteSpace(numeroSS))
            {
                throw new ArgumentException("Le numéro de sécurité sociale est requis.", nameof(numeroSS));
            }

            return _commandes.Where(cmd => cmd.Client.NumeroSS == numeroSS).OrderBy(cmd => cmd.DateLivraison);
        }

        // --- Écriture ---

        public void AddClient(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _clientRepo.Add(client);
            _clients.Add(client);
        }


        public void UpdateClient(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _clientRepo.Update(client);
            var idx = _clients.FindIndex(c => c.NumeroSS == client.NumeroSS);
            if (idx >= 0) _clients[idx] = client;
            else _clients.Add(client);
        }

        public void DeleteClient(string numeroSS)
        {
            if (string.IsNullOrWhiteSpace(numeroSS))
            {
                throw new ArgumentException("Le numéro de sécurité sociale est requis.", nameof(numeroSS));
            }

            _clientRepo.Delete(numeroSS);
            _clients.RemoveAll(c => c.NumeroSS == numeroSS);
            _commandes.RemoveAll(cmd => cmd.Client.NumeroSS == numeroSS);
        }
    }
}
