using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;


namespace projetc_
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            // 1) Repositories (en mémoire)
            var clientRepo = new CsvClientRepository("clients.csv");    // : IRepository<Client>
            var salarieRepo = new CsvSalarieRepository("salaries.csv");   // : IRepository<Salarie>
            var commandeRepo = new CsvCommandeRepository(filePath: "commandes.csv", clientRepo: clientRepo, salarieRepo: salarieRepo);  // : IRepository<Commande>

            // 2) Chargement du graphe depuis le CSV (méthode statique)
            //    Attention : vérifiez que "distances_villes_france.csv" est dans le répertoire de l'exécutable
            Graphe graphe = GrapheRepository.LoadFromCsv("distances_villes_france.csv");
            var grapheService = new GrapheService(graphe);

            // 3) Services métier
            var clientService = new ClientService(clientRepo, commandeRepo);
            var salarieService = new SalarieService(salarieRepo);
            var commandeService = new CommandeService(
                commandeRepo,       // IRepository<Commande>
                clientRepo,      //  IRepository<Client>
                salarieRepo,     // IRepository<Salarie>
                graphe,      // GrapheService
                new DijkstraStrategy(), // IPlusCourtCheminStrategy
                tarifParKm: 0.5m
            );

            // 4) Boucle principale
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                        MENU PRINCIPAL                         ║");
                Console.WriteLine("║               Veuillez choisir une option :                   ║");
                Console.WriteLine("║                                                               ║");
                Console.WriteLine("║   1. Module Clients                                           ║");
                Console.WriteLine("║   2. Module Salariés                                          ║");
                Console.WriteLine("║   3. Module Commandes                                         ║");
                Console.WriteLine("║   4. Module Statistiques                                      ║");
                Console.WriteLine("║   5. Module Graphe                                            ║");
                Console.WriteLine("║   6. Quitter                                                  ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.Write("Choix> ");

                var choix = Console.ReadLine();
                Console.Clear();

                switch (choix)
                {
                    case "1":
                        ClientsModule.Show(clientService);
                        break;

                    case "2":
                        SalariesModule.Show(salarieService);
                        break;

                    case "3":
                        // On passe bien les 4 services requis
                        CommandesModule.Show(
                            commandeService,
                            salarieService,
                            clientService,
                            grapheService
                        );
                        break;

                    case "4":
                        StatistiquesModule.Show(
                            commandeService,
                            clientService,
                            salarieService
                        );
                        break;

                    case "5":
                        GrapheModule.Show(grapheService);
                        break;

                    case "6":
                        return;  // quitte l'application

                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour réessayer...");
                        Console.ReadKey();
                        break;
                }
            }


            #region Tests
            //test des méthodes
            //1.
            //Test1();

            //2. 
            //Test2();

            //3. 
            //Test3();

            //4.
            //Test4_1();
            //Test4_2();
            //Test4_3();

            //5.
            //Test5();

            //6.
            //Test6();

            //7.
            //Test7();

            //8.
            //Test8();

            //9.
            //Test9();

            //Test10();
            #endregion

            /*// Remplit une première fois les CSV avec des données d’exemple
            CsvSeeder.Seed();

            // Puis instancie les repositories et services habituels
            var clientRepo = new CsvClientRepository("clients.csv");
            var salarieRepo = new CsvSalarieRepository("salaries.csv");
            var commandeRepo = new CsvCommandeRepository("commandes.csv", clientRepo, salarieRepo);*/


        }     
    }

    #region Interfaces
    // Module Clients : Interface utilisateur pour gérer les clients
    public static class ClientsModule
    {
        public static void Show(ClientService clientService)
        {
            bool exit = false;

            // Boucle principale du module Clients (boucle pour eviter le stack overflow des recursions)
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                        MODULE CLIENTS                         ║");
                Console.WriteLine("║               Veuillez choisir une option :                   ║");
                Console.WriteLine("║                                                               ║");
                Console.WriteLine("║   1. Lister tous les clients                                  ║");
                Console.WriteLine("║   2. Lister clients triés par nom                             ║");
                Console.WriteLine("║   3. Rechercher clients par ville                             ║");
                Console.WriteLine("║   4. Afficher top n clients par dépenses                      ║");
                Console.WriteLine("║   5. Afficher historique d'un client                          ║");
                Console.WriteLine("║   6. Ajouter un client                                        ║");
                Console.WriteLine("║   7. Mettre à jour un client                                  ║");
                Console.WriteLine("║   8. Supprimer un client                                      ║");
                Console.WriteLine("║   9. Retour au menu principal                                 ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.Write("Choix> ");

                var choix = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choix)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("===== Liste des clients =====");
                            var tous = clientService.GetAllClients().ToList();
                            AfficherClients(tous);
                            break;
                        case "2":
                            Console.Clear();
                            Console.WriteLine("===== Liste des clients triés par nom =====");
                            var clientsTries = clientService.GetClientsSortedByName();
                            AfficherClients(clientsTries);
                            break;
                        case "3":
                            Console.Clear();
                            Console.WriteLine("===== Recherche de clients par ville =====");
                            Console.Write("Ville à rechercher> ");
                            var ville = Console.ReadLine();
                            var clientsVille = clientService.GetClientsByCity(ville);
                            if (clientsVille.Any())
                            {
                                AfficherClients(clientsVille);
                            }
                            else
                            {
                                Console.WriteLine("Aucun client trouvé dans cette ville.");
                            }
                            break;
                        case "4":
                            Console.Clear();
                            Console.WriteLine("===== Top N clients par dépenses =====");
                            Console.Write("Nombre de top clients> ");
                            if (int.TryParse(Console.ReadLine(), out int topN))
                            {
                                var topClients = clientService.GetTopClientsByTotalSpent(topN);
                                AfficherTopClients(topClients);
                            }
                            break;
                        case "5":
                            Console.Clear();
                            Console.WriteLine("===== Historique d'un client =====");

                            // 1) Proposer d'afficher la liste des clients
                            Console.WriteLine("Tapez 'Oui' pour lister les clients, ou appuyez sur Entrée pour continuer.");
                            string choix2;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix2 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix2) && !string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix2) && !string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousClients = clientService.GetAllClients().ToList();
                                AfficherClients(tousClients);    
                                Console.WriteLine();                   
                            }

                            // 2) Puis demander le NSS
                            Console.Write("Numéro SS du client> ");
                            var ss = Console.ReadLine();

                            // 3) Afficher l’historique
                            var hist = clientService.GetHistoryForClient(ss);
                            AfficherHistoriqueClient(hist);
                            break;
                        case "6":
                            Console.Clear();
                            Console.WriteLine("===== Ajouter un client =====");
                            var newClient = PromptForClient();
                            clientService.AddClient(newClient);
                            Console.WriteLine("Client ajouté.");
                            break;
                        case "7":
                            Console.Clear();
                            Console.WriteLine("===== Mettre à jour un client =====");
                            // Proposer d'afficher la liste des clients
                            Console.WriteLine("Tapez 'Oui' pour lister les clients, ou appuyez sur Entrée pour continuer.");
                            string choix3;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix3 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix3) && !string.Equals(choix3, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix3) && !string.Equals(choix3, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix3, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousClients = clientService.GetAllClients().ToList();
                                AfficherClients(tousClients);
                                Console.WriteLine(" ");
                            }
                            Console.Write("Numéro SS du client à modifier> ");
                            var ssMod = Console.ReadLine();
                            var client = clientService.GetAllClients().FirstOrDefault(c => c.NumeroSS == ssMod);
                            if (client != null)
                            {
                                UpdateClientFields(client);
                                clientService.UpdateClient(client);
                                Console.WriteLine("Client mis à jour.");
                            }
                            else
                            {
                                Console.WriteLine("Client non trouvé.");
                            }
                            break;
                        case "8":
                            Console.Clear();
                            Console.WriteLine("===== Supprimer un client =====");
                            // Proposer d'afficher la liste des clients
                            Console.WriteLine("Tapez 'Oui' pour lister les clients, ou appuyez sur Entrée pour continuer.");
                            string choix4;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix4 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix4) && !string.Equals(choix4, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix4) && !string.Equals(choix4, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix4, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousClients = clientService.GetAllClients().ToList();
                                AfficherClients(tousClients);
                                Console.WriteLine(" ");
                            }
                            Console.Write("Numéro SS du client à supprimer> ");
                            var ssDel = Console.ReadLine();
                            clientService.DeleteClient(ssDel);
                            Console.WriteLine("Client supprimé si existant.");
                            break;
                        case "9":
                            Console.Clear();
                            Console.WriteLine("Retour au menu principal.");
                            exit = true;
                            continue;
                        default:
                            Console.WriteLine("Choix invalide.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                }

                Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                Console.ReadKey();

            }
        }

        private static Client PromptForClient()
        {
            Console.Write("N° Sécurité sociale> ");
            var numeroSS = Console.ReadLine();
            Console.Write("Nom> ");
            var nom = Console.ReadLine();
            Console.Write("Prénom> ");
            var prenom = Console.ReadLine();
            Console.Write("Date de naissance (yyyy-MM-dd)> ");
            DateTime dateNaiss = DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d) ? d : default;
            Console.Write("Adresse postale> ");
            var adresse = Console.ReadLine();
            Console.Write("Email> ");
            var email = Console.ReadLine();
            Console.Write("Téléphone> ");
            var tel = Console.ReadLine();
            Console.Write("Statut (Standard/Premium)> ");
            var statut = Console.ReadLine();

            return new Client(
                numeroSS,
                nom,
                prenom,
                dateNaiss,
                adresse,
                email,
                tel,
                statut,
                historiqueInitial: null
            );
        }

        private static void UpdateClientFields(Client client)
        {
            Console.Write($"Nom ({client.Nom})> ");
            var s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) client.Nom = s;

            Console.Write($"Adresse ({client.AdressePostale})> ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) client.AdressePostale = s;

            Console.Write($"Email ({client.Email})> ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) client.Email = s;

            Console.Write($"Téléphone ({client.Telephone})> ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) client.Telephone = s;

            Console.Write($"Statut ({client.Statut})> ");
            s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) client.Statut = s;
        }

        public static void AfficherClients(IEnumerable<Client> clients)
        {
            // 1) Nom des colonnes
            var headers = new[] { "Nom", "Prénom", "NSS", "Né(e) le", "Statut" };

            // 2) Calcul des largeurs
            int wNom = Math.Max(headers[0].Length, clients.Max(c => c.Nom.Length));
            int wPrenom = Math.Max(headers[1].Length, clients.Max(c => c.Prenom.Length));
            int wNSS = Math.Max(headers[2].Length, clients.Max(c => c.NumeroSS.Length));
            int wDate = Math.Max(headers[3].Length, "dd/MM/yyyy".Length);
            int wStatut = Math.Max(headers[4].Length, clients.Max(c => c.Statut.Length));

            // 3) Affichage de l’en-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wNom)} " +
                $"| {headers[1].PadRight(wPrenom)} " +
                $"| {headers[2].PadRight(wNSS)} " +
                $"| {headers[3].PadRight(wDate)} " +
                $"| {headers[4].PadRight(wStatut)} ");

            // Ligne de séparation
            var totalWidth = wNom + wPrenom + wNSS + wDate + wStatut + (3 * 5) + 1;
            // (5 fois " | " et un "|" final)
            Console.WriteLine(new string('-', totalWidth));

            // 4) Affichage des clients
            foreach (var c in clients)
            {
                Console.WriteLine(
                    $"| {c.Nom.PadRight(wNom)} " +
                    $"| {c.Prenom.PadRight(wPrenom)} " +
                    $"| {c.NumeroSS.PadRight(wNSS)} " +
                    $"| {c.DateNaissance:dd/MM/yyyy}".PadRight(wDate + 2) +
                    $"| {c.Statut.PadRight(wStatut)}|");
            }
        }

        public static void AfficherTopClients(IEnumerable<(Client Client, decimal TotalSpent)> topClients)
        {
            var liste = topClients.ToList();
            if (!liste.Any())
            {
                Console.WriteLine("Aucun client à afficher.");
                return;
            }

            // 1) Définir les en-têtes
            var headers = new[] { "Nom", "Prénom", "NSS", "Né(e) le", "Statut", "Dépenses (€)" };

            // 2) Calculer les largeurs de colonnes
            int wNom = Math.Max(headers[0].Length, liste.Max(x => x.Client.Nom.Length));
            int wPrenom = Math.Max(headers[1].Length, liste.Max(x => x.Client.Prenom.Length));
            int wNSS = Math.Max(headers[2].Length, liste.Max(x => x.Client.NumeroSS.Length));
            int wDate = Math.Max(headers[3].Length, "dd/MM/yyyy".Length);
            int wStatut = Math.Max(headers[4].Length, liste.Max(x => x.Client.Statut.Length));
            int wDepense = Math.Max(headers[5].Length, liste.Max(x => $"{x.TotalSpent:N2} €".Length));

            // 3) Afficher l’en-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wNom)} " +
                $"| {headers[1].PadRight(wPrenom)} " +
                $"| {headers[2].PadRight(wNSS)} " +
                $"| {headers[3].PadRight(wDate)} " +
                $"| {headers[4].PadRight(wStatut)} " +
                $"| {headers[5].PadRight(wDepense)} |");

            // 4) Ligne de séparation
            int totalWidth = wNom + wPrenom + wNSS + wDate + wStatut + wDepense
                           + (3 * 6) + 1; // 6 fois " | " + un "|" final
            Console.WriteLine(new string('-', totalWidth));

            // 5) Afficher chaque client
            foreach (var (client, total) in liste)
            {
                Console.WriteLine(
                    $"| {client.Nom.PadRight(wNom)} " +
                    $"| {client.Prenom.PadRight(wPrenom)} " +
                    $"| {client.NumeroSS.PadRight(wNSS)} " +
                    $"| {client.DateNaissance:dd/MM/yyyy}".PadRight(wDate + 2) +
                    $"| {client.Statut.PadRight(wStatut)} " +
                    $"| {($"{total:N2} €").PadLeft(wDepense)} |");
            }
        }

        public static void AfficherHistoriqueClient(IEnumerable<Commande> commandes)
        {
            var liste = commandes.ToList();
            if (!liste.Any())
            {
                Console.WriteLine("Aucune commande trouvée.");
                return;
            }

            // 1) En-têtes de colonnes
            var headers = new[] { "ID", "Date", "Prix (€)", "Livrée", "Chauffeur", "Véhicule" };

            // 2) Calcul des largeurs selon le contenu et l'en-tête
            int wId = Math.Max(headers[0].Length, liste.Max(c => c.IdCommande.Length));
            int wDate = Math.Max(headers[1].Length, "dd/MM/yyyy".Length);
            int wPrix = Math.Max(headers[2].Length, liste.Max(c => c.Prix.ToString("N2").Length + 1)); // +1 pour le € si vous l'ajoutez
            int wLivree = Math.Max(headers[3].Length, liste.Max(c => (c.EstLivree ? "Oui" : "Non").Length));
            int wChauffeur = Math.Max(headers[4].Length, liste.Max(c => $"{c.Chauffeur.Nom} {c.Chauffeur.Prenom}".Length));
            int wVehicule = Math.Max(headers[5].Length, liste.Max(c => c.Vehicule.ToString().Length));

            // 3) Affichage de l’en-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wId)} " +
                $"| {headers[1].PadRight(wDate)} " +
                $"| {headers[2].PadRight(wPrix)} " +
                $"| {headers[3].PadRight(wLivree)} " +
                $"| {headers[4].PadRight(wChauffeur)} " +
                $"| {headers[5].PadRight(wVehicule)} |");

            // 4) Séparateur
            int totalWidth = wId + wDate + wPrix + wLivree + wChauffeur + wVehicule
                           + (3 * 6) + 1; // 6 fois " | " plus un "|" final
            Console.WriteLine(new string('-', totalWidth));

            // 5) Affichage des lignes
            foreach (var c in liste)
            {
                var dateText = c.DateLivraison.ToString("dd/MM/yyyy");
                var prixText = $"{c.Prix:N2}";
                var livreText = c.EstLivree ? "Oui" : "Non";
                var chauffText = $"{c.Chauffeur.Nom} {c.Chauffeur.Prenom}";
                var vehText = c.Vehicule.ToString();

                Console.WriteLine(
                    $"| {c.IdCommande.PadRight(wId)} " +
                    $"| {dateText.PadRight(wDate)} " +
                    $"| {prixText.PadLeft(wPrix)} " +
                    $"| {livreText.PadRight(wLivree)} " +
                    $"| {chauffText.PadRight(wChauffeur)} " +
                    $"| {vehText.PadRight(wVehicule)} |");
            }
        }

    }

    // Module Salariés : Interface utilisateur pour gérer les salariés
    public static class SalariesModule
    {
        public static void Show(SalarieService salarieService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                        MODULE SALARIÉS                        ║");
                Console.WriteLine("║               Veuillez choisir une option :                   ║");
                Console.WriteLine("║                                                               ║");
                Console.WriteLine("║   1. Lister tous les salariés                                 ║");
                Console.WriteLine("║   2. Afficher un salarié par numéro de sécurité sociale       ║");
                Console.WriteLine("║   3. Embaucher un salarié                                     ║");
                Console.WriteLine("║   4. Licencier un salarié                                     ║");
                Console.WriteLine("║   5. Mettre à jour un salarié                                 ║");
                Console.WriteLine("║   6. Voir subordonnés d’un salarié                            ║");
                Console.WriteLine("║   7. Voir supérieurs d’un salarié                             ║");
                Console.WriteLine("║   8. Afficher l’organigramme complet                          ║");
                Console.WriteLine("║   9. Retour au menu principal                                 ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.Write("Choix> ");

                var choix = Console.ReadLine();
                Console.Clear();

                try
                {
                    switch (choix)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("===== Liste des salariés =====");
                            var tousSalaires = salarieService.GetAllSalaries().ToList();
                            AfficherSalaries(tousSalaires);
                            Pause();
                            break;

                        case "2":
                            Console.Clear();
                            Console.WriteLine("===== Détails d'un salarié =====");
                            //Proposer d'afficher la liste des salariés
                            Console.WriteLine("Tapez 'Oui' pour lister les salariés, ou appuyez sur Entrée pour continuer.");
                            string choix2;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix2 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix2) && !string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix2) && !string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousSalaires2 = salarieService.GetAllSalaries().ToList();
                                AfficherSalaries(tousSalaires2);
                                Console.WriteLine();
                            }

                            Console.Write("Veuillez entrer le numéro de sécutité sociale du salarié >");
                            var id = Console.ReadLine();
                            var sal = salarieService.GetSalarieByNumeroSS(id);
                            if (sal != null) Console.WriteLine(sal);
                            else Console.WriteLine("Salarié non trouvé.");
                            Pause();
                            break;

                        case "3":
                            Console.Clear();
                            Console.WriteLine("===== Embauche d'un salarié =====");
                            Console.WriteLine(" ");
                            var nouveau = PromptForSalarie();
                            Console.Write("Veuillez entrer le numéro de sécurité sociale du supérieur (laisser vide pour aucun supérieur) >");
                            var supId = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(supId))
                                salarieService.Hire(nouveau);
                            else
                                salarieService.Hire(nouveau, supId);
                            Console.WriteLine("Salarié embauché.");
                            Pause();
                            break;

                        case "4":
                            Console.Clear();
                            Console.WriteLine("===== Licenciement d'un salarié =====");
                            Console.WriteLine(" ");
                            // Proposer d'afficher la liste des salariés
                            Console.WriteLine("Tapez 'Oui' pour lister les salariés, ou appuyez sur Entrée pour continuer.");
                            string choix3;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix3 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix3) && !string.Equals(choix3, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix3) && !string.Equals(choix3, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix3, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousSalaires3 = salarieService.GetAllSalaries().ToList();
                                AfficherSalaries(tousSalaires3);
                                Console.WriteLine();
                            }

                            Console.Write("Veuillez entrer le numéro de sécurité sociale du salarié à licencier >");
                            var licId = Console.ReadLine();
                            salarieService.Fire(licId);
                            Console.WriteLine("Licenciement effectué si le salarié existait.");
                            Pause();
                            break;

                        case "5":
                            Console.Clear();
                            Console.WriteLine("===== Mise à jour d'un salarié =====");
                            Console.WriteLine(" ");
                            // Proposer d'afficher la liste des salariés
                            Console.WriteLine("Tapez 'Oui' pour lister les salariés, ou appuyez sur Entrée pour continuer.");
                            string choix4;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix4 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix4) && !string.Equals(choix4, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix4) && !string.Equals(choix4, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix4, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousSalaires3 = salarieService.GetAllSalaries().ToList();
                                AfficherSalaries(tousSalaires3);
                                Console.WriteLine();
                            }

                            Console.Write("Veuillez entrer le numéro de sécurité sociale du salarié à mettre à jour >");
                            var updId = Console.ReadLine();
                            var toUpdate = salarieService.GetSalarieByNumeroSS(updId);
                            if (toUpdate != null)
                            {
                                UpdateSalarieFields(toUpdate);
                                salarieService.Update(toUpdate);
                                Console.WriteLine("Salarié mis à jour.");
                            }
                            else
                            {
                                Console.WriteLine("Salarié non trouvé.");
                            }
                            Pause();
                            break;

                        case "6":
                            Console.Clear();
                            Console.WriteLine("===== Subordonnés d'un salarié =====");
                            Console.WriteLine(" ");
                            // Proposer d'afficher la liste des salariés
                            Console.WriteLine("Tapez 'Oui' pour lister les salariés, ou appuyez sur Entrée pour continuer.");
                            string choix5;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix5 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix5) && !string.Equals(choix5, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }

                            } while (!string.IsNullOrEmpty(choix5) && !string.Equals(choix5, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix5, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousSalaires3 = salarieService.GetAllSalaries().ToList();
                                AfficherSalaries(tousSalaires3);
                                Console.WriteLine();
                            }

                            Console.Write("Veuillez entrer le numéro de sécurité sociale du salarié ");
                            var subId = Console.ReadLine();
                            var subs = salarieService.GetSubordinates(subId);
                            Console.WriteLine("Subordonnés :");
                            foreach (var s2 in subs)
                                Console.WriteLine("  - " + s2);
                            if (!subs.Any()) Console.WriteLine("  (aucun)");
                            Pause();
                            break;

                        case "7":
                            Console.Clear();
                            Console.WriteLine("===== Supérieurs d'un salarié =====");
                            Console.WriteLine(" ");
                            // Proposer d'afficher la liste des salariés
                            Console.WriteLine("Tapez 'Oui' pour lister les salariés, ou appuyez sur Entrée pour continuer.");
                            string choix6;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix6 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix6) && !string.Equals(choix6, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }

                            } while (!string.IsNullOrEmpty(choix6) && !string.Equals(choix6, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix6, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                var tousSalaires3 = salarieService.GetAllSalaries().ToList();
                                AfficherSalaries(tousSalaires3);
                                Console.WriteLine();
                            }

                            Console.Write("Veuillez entrer le numéro de sécurité sociale du salarié ");
                            var supListId = Console.ReadLine();
                            var sups = salarieService.GetSuperiors(supListId);
                            Console.WriteLine("Supérieurs :");
                            foreach (var s3 in sups)
                                Console.WriteLine("  - " + s3);
                            if (!sups.Any()) Console.WriteLine("  (aucun)");
                            Pause();
                            break;

                        case "8":
                            Console.Clear();
                            Console.WriteLine("===== Organigramme =====\n");
                            // Récupère tous les salariés
                            var all = salarieService.GetAllSalaries().ToList();
                            // Détermine les racines (ceux sans supérieur)
                            var roots = all.Where(s => s.Superieur == null).ToList();
                            if (!roots.Any())
                            {
                                Console.WriteLine("Aucun salarié racine trouvé.");
                            }
                            else
                            {
                                // Affiche chaque arbre
                                for (int i = 0; i < roots.Count; i++)
                                {
                                    bool lastRoot = (i == roots.Count - 1);
                                    AsciiOrgChart.Print(roots[i], indent: "", isLast: lastRoot);
                                    Console.WriteLine();
                                }
                            }
                            Pause();
                            break;

                        case "9":
                            return;

                        default:
                            Console.WriteLine("Choix invalide.");
                            Pause();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                    Pause();
                }
            }
        }

        private static void Pause()
        {
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
        public static void AfficherSalaries(IEnumerable<Salarie> salaries)
        {
            var liste = salaries.ToList();
            if (!liste.Any())
            {
                Console.WriteLine("Aucun salarié à afficher.");
                return;
            }

            // 1) En-têtes de colonnes
            var headers = new[] { "Nom", "Prénom", "NSS", "Date entrée", "Poste", "Salaire (€)" };

            // 2) Calcul des largeurs
            int wNom = Math.Max(headers[0].Length, liste.Max(s => s.Nom.Length));
            int wPrenom = Math.Max(headers[1].Length, liste.Max(s => s.Prenom.Length));
            int wNSS = Math.Max(headers[2].Length, liste.Max(s => s.NumeroSS.Length));
            int wDateEnt = Math.Max(headers[3].Length, "dd/MM/yyyy".Length);
            int wPoste = Math.Max(headers[4].Length, liste.Max(s => s.Poste.Length));
            int wSalaire = Math.Max(headers[5].Length, liste.Max(s => $"{s.Salaire:N2}".Length) + 1); // +1 pour l'éventuel €

            // 3) Affichage de l’en-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wNom)} " +
                $"| {headers[1].PadRight(wPrenom)} " +
                $"| {headers[2].PadRight(wNSS)} " +
                $"| {headers[3].PadRight(wDateEnt)} " +
                $"| {headers[4].PadRight(wPoste)} " +
                $"| {headers[5].PadRight(wSalaire)} |");

            // 4) Ligne de séparation
            int totalWidth = wNom + wPrenom + wNSS + wDateEnt + wPoste + wSalaire
                           + (3 * 6) + 1; // 6 fois " | " plus un '|' final
            Console.WriteLine(new string('-', totalWidth));

            // 5) Affichage des salariés
            foreach (var s in liste)
            {
                var dateEnt = s.DateEntree.ToString("dd/MM/yyyy");
                var salaire = $"{s.Salaire:N2}";
                Console.WriteLine(
                    $"| {s.Nom.PadRight(wNom)} " +
                    $"| {s.Prenom.PadRight(wPrenom)} " +
                    $"| {s.NumeroSS.PadRight(wNSS)} " +
                    $"| {dateEnt.PadRight(wDateEnt)} " +
                    $"| {s.Poste.PadRight(wPoste)} " +
                    $"| {salaire.PadLeft(wSalaire)} |");
            }
        }
        // Méthode pour demander les informations d'un salarié
        private static Salarie PromptForSalarie()
        {
            Console.Write("N° Sécurité sociale> ");
            var numeroSS = Console.ReadLine();
            Console.Write("Nom> ");
            var nom = Console.ReadLine();
            Console.Write("Prénom> ");
            var prenom = Console.ReadLine();
            Console.Write("Date de naissance (yyyy-MM-dd)> ");
            DateTime dateNaiss = DateTime.TryParseExact(
                Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d) ? d : default;
            Console.Write("Date d'entrée (yyyy-MM-dd)> ");
            DateTime dateEntree = DateTime.TryParseExact(
                Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var de) ? de : DateTime.Now;
            Console.Write("Poste> ");
            var poste = Console.ReadLine();

            // On utilise le constructeur minimal
            return new Salarie(numeroSS, nom, prenom, dateNaiss, dateEntree, poste);
        }

        // Méthode pour mettre à jour les informations d'un salarié
        private static void UpdateSalarieFields(Salarie s)
        {
            Console.Write($"Nom ({s.Nom})> ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) s.Nom = input;

            Console.Write($"Adresse ({s.AdressePostale})> ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) s.AdressePostale = input;

            Console.Write($"Email ({s.Email})> ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) s.Email = input;

            Console.Write($"Téléphone ({s.Telephone})> ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) s.Telephone = input;

            Console.Write($"Poste ({s.Poste})> ");
            input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) s.Poste = input;

            Console.Write($"Salaire ({s.Salaire})> ");
            input = Console.ReadLine();
            if (decimal.TryParse(input, out var sal) && sal >= 0) s.Salaire = sal;
        }
        private static void PrintHierarchy(Salarie root, IEnumerable<Salarie> descendants, int level)
        {
            Console.WriteLine($"{new string(' ', level * 2)}- {root.Nom} {root.Prenom} (Poste : {root.Poste})");
            var children = descendants.Where(s => s.Superieur?.NumeroSS == root.NumeroSS);
            foreach (var child in children)
                PrintHierarchy(child, descendants, level + 1);
        }
    }
    // organigramme 
    public static class AsciiOrgChart
    {
        public static void Print(Salarie node, string indent = "", bool isLast = true)
        {
            // Préfixe de la ligne : └─ ou ├─
            Console.Write(indent);
            Console.Write(isLast ? "└─" : "├─");

            // Le contenu du nœud
            Console.WriteLine($"[{node.Poste}: {node.Nom} {node.Prenom}]");

            // Prépare l'indent pour les enfants
            indent += isLast ? "  " : "│ ";

            // Parcours récursif des subordonnés
            for (int i = 0; i < node.Subordonnes.Count; i++)
            {
                bool lastChild = (i == node.Subordonnes.Count - 1);
                Print(node.Subordonnes[i], indent, lastChild);
            }
        }
    }
    // Module Commandes : Interface utilisateur pour gérer les commandes
    public static class CommandesModule 
    {
        public static void Show(
            CommandeService commandeService,
            SalarieService salarieService,
            ClientService clientService,
            GrapheService grapheService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                        MODULE COMMANDES                       ║");
                Console.WriteLine("║               Veuillez choisir une option :                   ║");
                Console.WriteLine("║                                                               ║");
                Console.WriteLine("║   1. Lister toutes les commandes                              ║");
                Console.WriteLine("║   2. Lister commandes par chauffeur                           ║");
                Console.WriteLine("║   3. Lister commandes par période                             ║");
                Console.WriteLine("║   4. Rechercher une commande par ID                           ║");
                Console.WriteLine("║   5. Créer une nouvelle commande                              ║");
                Console.WriteLine("║   6. Retour au menu principal                                 ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.Write("Choix> ");

                var choix = Console.ReadLine();
                Console.Clear();
                try
                {
                    switch (choix)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("===== Toutes les commandes =====");
                            var toutes = commandeService.GetAllCommandes().ToList();
                            AfficherCommandes(toutes);
                            Pause();
                            break;
                        case "2":
                            Console.Clear();
                            Console.WriteLine("===== Commandes par chauffeur =====");
                            Console.WriteLine();

                            // 1) Proposer d'afficher la liste des chauffeurs
                            Console.WriteLine("Tapez 'Oui' pour lister les chauffeurs, ou appuyez sur Entrée pour continuer.");
                            string choix2;
                            do
                            {
                                Console.Write("Votre choix (Oui/Entrée)> ");
                                choix2 = Console.ReadLine();
                                if (!string.IsNullOrEmpty(choix2) && !string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Veuillez taper 'Oui' ou appuyer sur Entrée.");
                                }
                            } while (!string.IsNullOrEmpty(choix2) && !string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase));

                            if (string.Equals(choix2, "Oui", StringComparison.OrdinalIgnoreCase))
                            {
                                // Filtrer uniquement les chauffeurs
                                var chauffeurs = salarieService.GetAllSalaries().Where(s => s.Poste.Equals("Chauffeur", StringComparison.OrdinalIgnoreCase)).ToList();

                                if (!chauffeurs.Any())
                                {
                                    Console.WriteLine("Aucun chauffeur trouvé.");
                                }
                                else
                                {
                                    Console.WriteLine("\nListe des chauffeurs :");
                                    AfficherSalaries(chauffeurs);
                                    Console.WriteLine();
                                }
                            }

                            // 2) Saisie sécurisée de l'ID du chauffeur
                            string idChauffeur;
                            do
                            {
                                Console.Write("Veuillez entrer l'ID (NSS) du chauffeur> ");
                                idChauffeur = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(idChauffeur))
                                {
                                    Console.WriteLine("Le numéro de sécurité sociale est requis.");
                                    continue;
                                }
                                // On peut aussi vérifier qu'il existe
                                if (salarieService.GetAllSalaries().All(s => !s.NumeroSS.Equals(idChauffeur, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Console.WriteLine($"Aucun chauffeur avec l'ID '{idChauffeur}' trouvé.");
                                    idChauffeur = null;
                                }
                            } while (string.IsNullOrWhiteSpace(idChauffeur));

                            // 3) Affichage des commandes filtrées
                            var commandesDuChauffeur = commandeService.GetCommandesByChauffeur(idChauffeur).ToList();
                            Console.WriteLine($"\n===== Commandes pour {idChauffeur} =====");
                            if (!commandesDuChauffeur.Any())
                            {
                                Console.WriteLine("Aucune commande trouvée pour ce chauffeur.");
                            }
                            else
                            {
                                AfficherCommandes(commandesDuChauffeur);
                            }

                            Pause();


                            break;
                        case "3":
                            Console.Clear();
                            Console.WriteLine("===== Commandes par période =====");
                            Console.WriteLine();

                            Console.Write("Veuillez entrer la date de début (yyyy-MM-dd)> ");
                            var d1 = DateTime.TryParseExact(Console.ReadLine() ?? "","yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var dt1)? dt1: DateTime.MinValue;

                            Console.Write("Veuillez entrer la date de fin (yyyy-MM-dd)> ");
                            var d2 = DateTime.TryParseExact(Console.ReadLine() ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2) ? dt2 : DateTime.MaxValue;

                            var periode = commandeService.GetCommandesByPeriod(d1, d2).ToList();
                            Console.WriteLine($"===== Commandes du {d1:yyyy-MM-dd} au {d2:yyyy-MM-dd} =====");

                            if (!periode.Any())
                            {
                                Console.WriteLine("Aucune commande trouvée pour cette période.");
                            }
                            else
                            {
                                AfficherCommandes(periode);
                            }

                            Pause();
                            break;
                        case "4":
                            Console.Clear();
                            Console.WriteLine("===== Rechercher une commande par ID =====");
                            Console.WriteLine(" ");
                            Console.Write("Veuillez entrer l'id de la commande ");
                            var idCmd = Console.ReadLine();
                            var cmd = commandeService.GetCommandeById(idCmd);
                            if (cmd != null) Console.WriteLine(cmd);
                            else Console.WriteLine("Commande non trouvée.");
                            Pause();
                            break;
                        case "5":
                            var newCmd = PromptForCommande(commandeService, clientService, salarieService, grapheService);
                            Console.WriteLine("Commande créée :");
                            Console.WriteLine(newCmd);
                            Pause();
                            break;
                        case "6":
                            return;
                        default:
                            Console.WriteLine("Choix invalide.");
                            Pause();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                    Pause();
                }
            }
        }

        private static void Pause()
        {
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
        public static void AfficherCommandes(IEnumerable<Commande> commandes)
        {
            var liste = commandes.ToList();
            if (!liste.Any())
            {
                Console.WriteLine("Aucune commande à afficher.");
                return;
            }

            // 1) En-têtes
            var headers = new[] { "ID", "Date", "Prix (€)", "Livrée", "Client NSS", "Chauffeur", "Véhicule" };

            // 2) Calcul des largeurs
            int wId = Math.Max(headers[0].Length, liste.Max(c => c.IdCommande.Length));
            int wDate = Math.Max(headers[1].Length, "dd/MM/yyyy".Length);
            int wPrix = Math.Max(headers[2].Length, liste.Max(c => $"{c.Prix:N2} €".Length));
            int wLivree = Math.Max(headers[3].Length, liste.Max(c => (c.EstLivree ? "Oui" : "Non").Length));
            int wClient = Math.Max(headers[4].Length, liste.Max(c => c.Client.NumeroSS.Length));
            int wChauffeur = Math.Max(headers[5].Length, liste.Max(c => $"{c.Chauffeur.Nom} {c.Chauffeur.Prenom}".Length));
            int wVehicule = Math.Max(headers[6].Length, liste.Max(c => c.Vehicule.ToString().Length));

            // 3) En-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wId)} " +
                $"| {headers[1].PadRight(wDate)} " +
                $"| {headers[2].PadRight(wPrix)} " +
                $"| {headers[3].PadRight(wLivree)} " +
                $"| {headers[4].PadRight(wClient)} " +
                $"| {headers[5].PadRight(wChauffeur)} " +
                $"| {headers[6].PadRight(wVehicule)} |");

            // 4) Ligne de séparation
            int totalWidth = wId + wDate + wPrix + wLivree + wClient + wChauffeur + wVehicule
                           + (3 * headers.Length) + 1; // " | " x7 + final "|"
            Console.WriteLine(new string('-', totalWidth));

            // 5) Lignes de données
            foreach (var c in liste)
            {
                var dateText = c.DateLivraison.ToString("dd/MM/yyyy").PadRight(wDate);
                var prixText = $"{c.Prix:N2} €".PadLeft(wPrix);
                var livreText = (c.EstLivree ? "Oui" : "Non").PadRight(wLivree);
                var clientText = c.Client.NumeroSS.PadRight(wClient);
                var chauffText = $"{c.Chauffeur.Nom} {c.Chauffeur.Prenom}".PadRight(wChauffeur);
                var vehText = c.Vehicule.ToString().PadRight(wVehicule);

                Console.WriteLine(
                    $"| {c.IdCommande.PadRight(wId)} " +
                    $"| {dateText} " +
                    $"| {prixText} " +
                    $"| {livreText} " +
                    $"| {clientText} " +
                    $"| {chauffText} " +
                    $"| {vehText} |");
            }
        }

        public static void AfficherSalaries(IEnumerable<Salarie> salaries)
        {
            var liste = salaries.ToList();
            if (!liste.Any())
            {
                Console.WriteLine("Aucun salarié à afficher.");
                return;
            }

            // 1) En-têtes
            var headers = new[] { "Nom", "Prénom", "NSS", "Date entrée", "Poste", "Salaire (€)" };

            // 2) Calcul des largeurs
            int wNom = Math.Max(headers[0].Length, liste.Max(s => s.Nom.Length));
            int wPrenom = Math.Max(headers[1].Length, liste.Max(s => s.Prenom.Length));
            int wNSS = Math.Max(headers[2].Length, liste.Max(s => s.NumeroSS.Length));
            int wDateEnt = Math.Max(headers[3].Length, "dd/MM/yyyy".Length);
            int wPoste = Math.Max(headers[4].Length, liste.Max(s => s.Poste.Length));
            int wSalaire = Math.Max(headers[5].Length, liste.Max(s => $"{s.Salaire:N2}".Length + 1)); // +1 pour €

            // 3) Affichage de l’en-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wNom)} " +
                $"| {headers[1].PadRight(wPrenom)} " +
                $"| {headers[2].PadRight(wNSS)} " +
                $"| {headers[3].PadRight(wDateEnt)} " +
                $"| {headers[4].PadRight(wPoste)} " +
                $"| {headers[5].PadRight(wSalaire)} |");

            // 4) Ligne de séparation
            int totalWidth = wNom + wPrenom + wNSS + wDateEnt + wPoste + wSalaire
                           + (3 * headers.Length) + 1; // " | " x6 + final '|'
            Console.WriteLine(new string('-', totalWidth));

            // 5) Affichage des salariés
            foreach (var s in liste)
            {
                var dateEnt = s.DateEntree.ToString("dd/MM/yyyy").PadRight(wDateEnt);
                var salaire = $"{s.Salaire:N2}".PadLeft(wSalaire - 1) + " €";

                Console.WriteLine(
                    $"| {s.Nom.PadRight(wNom)} " +
                    $"| {s.Prenom.PadRight(wPrenom)} " +
                    $"| {s.NumeroSS.PadRight(wNSS)} " +
                    $"| {dateEnt} " +
                    $"| {s.Poste.PadRight(wPoste)} " +
                    $"| {salaire.PadLeft(wSalaire)} |");
            }
        }


        private static Commande PromptForCommande(CommandeService commandeService,ClientService clientService,SalarieService salarieService,GrapheService grapheService)
        {
            Console.Write("ID Commande> ");
            var id = Console.ReadLine();

            Console.Write("N° SS Client> ");
            var ssClient = Console.ReadLine();
            var client = clientService.GetAllClients().FirstOrDefault(c => c.NumeroSS == ssClient)
                         ?? throw new InvalidOperationException("Client non trouvé.");

            Console.Write("N° SS Chauffeur> ");
            var ssChauf = Console.ReadLine();
            var chauffeur = salarieService.GetSalarieByNumeroSS(ssChauf)
                            ?? throw new InvalidOperationException("Chauffeur non trouvé.");

            Console.Write("Type de véhicule> ");
            var typeVeh = Console.ReadLine();
            Console.Write("Capacité du véhicule> ");
            var capOk = int.TryParse(Console.ReadLine(), out var cap) ? cap : 1;
            var vehicule = VehiculeFactory.CreateVehicule(typeVeh, Guid.NewGuid().ToString(), cap, null);

            Console.Write("Ville de départ> ");
            var vDep = Console.ReadLine();
            Console.Write("Ville d'arrivée> ");
            var vArr = Console.ReadLine();

            Console.Write("Date livraison (yyyy-MM-dd)> ");
            var dateOk = DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateLiv) ? dateLiv : DateTime.Now;

            return commandeService.CreateCommande(
                id,
                client.NumeroSS,
                chauffeur.NumeroSS,
                vehicule,
                vDep,
                vArr,
                dateLiv
            );
        }
    }

    // Module Statistiques : Interface utilisateur pour afficher les statistiques
    public static class StatistiquesModule
    {
        public static void Show(
            CommandeService commandeService,
            ClientService clientService,
            SalarieService salarieService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("===== MODULE STATISTIQUES =====");
                Console.WriteLine("1. Nombre de livraisons par chauffeur");
                Console.WriteLine("2. Commandes par période");
                Console.WriteLine("3. Prix moyen des commandes");
                Console.WriteLine("4. Chiffre d'affaires moyen par client");
                Console.WriteLine("5. Liste des commandes pour un client");
                Console.WriteLine("6. Retour au menu principal");
                Console.Write("Choix> ");

                var choix = Console.ReadLine();
                Console.Clear();

                try
                {
                    switch (choix)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("===== Nombre de livraisons par chauffeur =====");
                            Console.WriteLine(" ");
                            var allCmds1 = commandeService.GetAllCommandes();
                            var grpParChauffeur = allCmds1
                                .GroupBy(c => c.Chauffeur)
                                .OrderBy(g => g.Key.Nom);
                            Console.WriteLine("Nombre de livraisons par chauffeur :");
                            foreach (var groupe in grpParChauffeur)
                            {
                                var chauffeur = groupe.Key;
                                Console.WriteLine($"{chauffeur.Nom} {chauffeur.Prenom} ({chauffeur.NumeroSS}) : {groupe.Count()}");
                            }
                            Pause();
                            break;

                        case "2":
                            Console.Clear();
                            Console.WriteLine("===== Commandes par période =====");
                            Console.WriteLine();

                            Console.Write("Veuillez entrer la date de début (yyyy-MM-dd)> ");
                            var d1 = DateTime.TryParseExact(Console.ReadLine() ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt1) ? dt1 : DateTime.MinValue;

                            Console.Write("Veuillez entrer la date de fin (yyyy-MM-dd)> ");
                            var d2 = DateTime.TryParseExact(Console.ReadLine() ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2) ? dt2 : DateTime.MaxValue;

                            var periode = commandeService.GetCommandesByPeriod(d1, d2).ToList();
                            Console.WriteLine($"===== Commandes du {d1:yyyy-MM-dd} au {d2:yyyy-MM-dd} =====");

                            if (!periode.Any())
                            {
                                Console.WriteLine("Aucune commande trouvée pour cette période.");
                            }
                            else
                            {
                                AfficherCommandes(periode);
                            }

                            Pause();
                            break;

                        case "3":

                            Console.Clear();
                            Console.WriteLine("===== Prix moyen des commandes =====");
                            Console.WriteLine(" ");
                            var allCmds2 = commandeService.GetAllCommandes();
                            if (allCmds2.Any())
                            {
                                var prixMoyen = allCmds2.Average(c => c.Prix);
                                Console.WriteLine($"Prix moyen des commandes : {prixMoyen:C}");
                            }
                            else
                            {
                                Console.WriteLine("Aucune commande enregistrée.");
                            }
                            Pause();
                            break;

                        case "4":
                            Console.Clear();
                            Console.WriteLine("===== Chiffre d'affaires moyen par client =====");
                            Console.WriteLine(" ");
                            var allClients = clientService.GetAllClients();
                            if (allClients.Any())
                            {
                                var caMoyen = allClients
                                    .Select(c => c.HistoriqueLivraisons.Sum(cmd => cmd.Prix))
                                    .Average();
                                Console.WriteLine($"Chiffre d'affaires moyen par client : {caMoyen:C}");
                            }
                            else
                            {
                                Console.WriteLine("Aucun client enregistré.");
                            }
                            Pause();
                            break;

                        case "5":
                            Console.Clear();
                            Console.WriteLine("===== Liste des commandes pour un client =====");
                            Console.WriteLine(" ");
                            Console.Write("Veuillez entrer le numéro SS du client> ");
                            var ssClient = Console.ReadLine();
                            var historique = clientService.GetHistoryForClient(ssClient);
                            Console.WriteLine($"Historique des commandes pour le client {ssClient} :");
                            if (historique.Any())
                                foreach (var cmd in historique)
                                    Console.WriteLine(cmd);
                            else
                                Console.WriteLine("Aucune commande pour ce client.");
                            Pause();
                            break;

                        case "6":
                            return;

                        default:
                            Console.WriteLine("Choix invalide.");
                            Pause();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                    Pause();
                }
            }
        }

        public static void AfficherCommandes(IEnumerable<Commande> commandes)
        {
            var liste = commandes.ToList();
            if (!liste.Any())
            {
                Console.WriteLine("Aucune commande à afficher.");
                return;
            }

            // 1) En-têtes
            var headers = new[] { "ID", "Date", "Prix (€)", "Livrée", "Client NSS", "Chauffeur", "Véhicule" };

            // 2) Calcul des largeurs
            int wId = Math.Max(headers[0].Length, liste.Max(c => c.IdCommande.Length));
            int wDate = Math.Max(headers[1].Length, "dd/MM/yyyy".Length);
            int wPrix = Math.Max(headers[2].Length, liste.Max(c => $"{c.Prix:N2} €".Length));
            int wLivree = Math.Max(headers[3].Length, liste.Max(c => (c.EstLivree ? "Oui" : "Non").Length));
            int wClient = Math.Max(headers[4].Length, liste.Max(c => c.Client.NumeroSS.Length));
            int wChauffeur = Math.Max(headers[5].Length, liste.Max(c => $"{c.Chauffeur.Nom} {c.Chauffeur.Prenom}".Length));
            int wVehicule = Math.Max(headers[6].Length, liste.Max(c => c.Vehicule.ToString().Length));

            // 3) En-tête
            Console.WriteLine(
                $"| {headers[0].PadRight(wId)} " +
                $"| {headers[1].PadRight(wDate)} " +
                $"| {headers[2].PadRight(wPrix)} " +
                $"| {headers[3].PadRight(wLivree)} " +
                $"| {headers[4].PadRight(wClient)} " +
                $"| {headers[5].PadRight(wChauffeur)} " +
                $"| {headers[6].PadRight(wVehicule)} |");

            // 4) Ligne de séparation
            int totalWidth = wId + wDate + wPrix + wLivree + wClient + wChauffeur + wVehicule
                           + (3 * headers.Length) + 1; // " | " x7 + final "|"
            Console.WriteLine(new string('-', totalWidth));

            // 5) Lignes de données
            foreach (var c in liste)
            {
                var dateText = c.DateLivraison.ToString("dd/MM/yyyy").PadRight(wDate);
                var prixText = $"{c.Prix:N2} €".PadLeft(wPrix);
                var livreText = (c.EstLivree ? "Oui" : "Non").PadRight(wLivree);
                var clientText = c.Client.NumeroSS.PadRight(wClient);
                var chauffText = $"{c.Chauffeur.Nom} {c.Chauffeur.Prenom}".PadRight(wChauffeur);
                var vehText = c.Vehicule.ToString().PadRight(wVehicule);

                Console.WriteLine(
                    $"| {c.IdCommande.PadRight(wId)} " +
                    $"| {dateText} " +
                    $"| {prixText} " +
                    $"| {livreText} " +
                    $"| {clientText} " +
                    $"| {chauffText} " +
                    $"| {vehText} |");
            }
        }
        private static void Pause()
        {
            Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
            Console.ReadKey();
        }
    }

    // Module Graphes : Interface utilisateur pour afficher les graphes
    public static class GrapheModule
    {
        public static void Show(GrapheService grapheService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                        MODULE GRAPHE                          ║");
                Console.WriteLine("║               Veuillez choisir une option :                   ║");
                Console.WriteLine("║                                                               ║");
                Console.WriteLine("║   1. Lister toutes les villes                                 ║");
                Console.WriteLine("║   2. Lister tous les liens                                    ║");
                Console.WriteLine("║   3. Parcours BFS depuis une ville                            ║");
                Console.WriteLine("║   4. Parcours DFS depuis une ville                            ║");
                Console.WriteLine("║   5. Vérifier si le graphe est connexe                        ║");
                Console.WriteLine("║   6. Détecter un cycle                                        ║");
                Console.WriteLine("║   7. Trouver le plus court chemin                             ║");
                Console.WriteLine("║   8. Afficher la matrice Floyd–Warshall                       ║");
                Console.WriteLine("║   9. Afficher Le graphe des villes                            ║");
                Console.WriteLine("║  10. Retour au menu principal                                 ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.Write("Choix> ");

                var choix = Console.ReadLine();
                Console.Clear();

                try
                {
                    switch (choix)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("Toutes les villes :");
                            Console.WriteLine(" ");
                            foreach (var n in grapheService.GetVilles())
                                Console.WriteLine($"- {n}");
                            Pause();
                            break;

                        case "2":
                            Console.Clear();
                            Console.WriteLine("Tous les liens (arêtes) :");
                            Console.WriteLine(" ");
                            foreach (var l in grapheService.GetLiens())
                                Console.WriteLine($"- {l.Depart} => {l.Arrivee} : {l.DistanceKm:N1} km");
                            Pause();
                            break;

                        case "3":
                            Console.Clear();
                            Console.WriteLine("===== Parcours BFS depuis une ville =====");
                            Console.WriteLine(" ");
                            Console.Write("Veuillez entrer le nom de la ville de départ :");
                            var startBfs = Console.ReadLine();
                            var nodeBfs = grapheService
                                .GetVilles()
                                .FirstOrDefault(n => n.Equals(startBfs, StringComparison.OrdinalIgnoreCase));
                            if (nodeBfs != null)
                            {
                                Console.WriteLine($"BFS depuis {nodeBfs} :");
                                foreach (var v in grapheService.ParcoursBFS(nodeBfs))
                                    Console.WriteLine($"- {v}");
                            }
                            else
                            {
                                Console.WriteLine("Ville introuvable.");
                            }
                            Pause();
                            break;

                        case "4":
                            Console.Clear();
                            Console.WriteLine("===== Parcours DFS depuis une ville =====");
                            Console.WriteLine(" ");
                            Console.Write("Veuillez entrer le nom de la ville de départ :");
                            var startDfs = Console.ReadLine();
                            var nodeDfs = grapheService
                                .GetVilles()
                                .FirstOrDefault(n => n.Equals(startDfs, StringComparison.OrdinalIgnoreCase));
                            if (nodeDfs != null)
                            {
                                Console.WriteLine($"DFS depuis {nodeDfs} :");
                                foreach (var v in grapheService.ParcoursDFS(nodeDfs))
                                    Console.WriteLine($"- {v}");
                            }
                            else
                            {
                                Console.WriteLine("Ville introuvable.");
                            }
                            Pause();
                            break;

                        case "5":
                            Console.Clear();
                            Console.WriteLine("===== Vérification de la connexité du graphe =====");
                            Console.WriteLine(" ");
                            Console.WriteLine(grapheService.IsConnected()
                                ? "Le graphe est connexe."
                                : "Le graphe n'est pas connexe."
                            );
                            Pause();
                            break;

                        case "6":
                            Console.Clear();
                            Console.WriteLine("===== Détection de cycle =====");
                            Console.WriteLine(" ");
                            Console.WriteLine(
                                grapheService.HasCycle()
                                ? "Un cycle a été détecté."
                                : "Aucun cycle détecté."
                            );
                            Pause();
                            break;

                        case "7":
                            Console.Clear();
                            Console.WriteLine("===== Trouver le plus court chemin =====");
                            Console.Write("Veuillez entrer le nom de la ville de départ :");
                            var dep = Console.ReadLine();
                            Console.Write("Veuillez entrer le nom de la ville d'arrivée :");
                            var arr = Console.ReadLine();

                            Console.WriteLine("Choix de l'algorithme :");
                            Console.WriteLine("  1. Dijkstra");
                            Console.WriteLine("  2. Bellman–Ford");
                            Console.WriteLine("  3. Floyd–Warshall");
                            var alg = Console.ReadLine();

                            IPlusCourtCheminStrategy strategy = alg switch
                            {
                                "2" => new BellmanFordStrategy(),
                                "3" => new FloydWarshallStrategy(),
                                _ => new DijkstraStrategy()
                            };

                            var chemin = grapheService.FindShortestPath(dep, arr, strategy);
                            if (chemin.Any())
                            {
                                Console.WriteLine("Chemin optimal :");
                                foreach (var ville in chemin)
                                    Console.WriteLine($"- {ville}");
                            }
                            else
                            {
                                Console.WriteLine("Aucun chemin trouvé entre ces villes.");
                            }
                            Pause();
                            break;

                        case "8":
                            Console.Clear();
                            Console.WriteLine("===== Matrice Floyd–Warshall =====");
                            Console.WriteLine(" ");
                            Console.WriteLine("Cette matrice représente les distances entre toutes les villes.");
                            Console.WriteLine("Les valeurs infinies (∞) indiquent qu'il n'y a pas de chemin direct entre les villes.");
                            Console.WriteLine(" ");
                            var villes = grapheService.GetVilles().ToList();
                            var matrix = grapheService.FloydWarshallMatrix();
                            int k = villes.Count;
                            if (matrix.GetLength(0) != k || matrix.GetLength(1) != k)
                                throw new ArgumentException("La taille de la matrice doit correspondre au nombre de villes.");

                            // 1) Calculer une largeur de colonne suffisante
                            int colWidth = villes.Max(v => v.Length);
                            // On s’assure aussi d’avoir la place pour afficher "9999.9" ou "∞"
                            colWidth = Math.Max(colWidth, 7) + 2;

                            // 2) En-tête : case vide + noms de colonnes
                            Console.Write(new string(' ', colWidth));
                            foreach (var ville in villes)
                            {
                                Console.Write(ville.PadLeft(colWidth));
                            }
                            Console.WriteLine();

                            // 3) Chaque ligne : nom de la ville + ses distances
                            for (int i = 0; i < k; i++)
                            {
                                // nom de la ligne
                                Console.Write(villes[i].PadRight(colWidth));

                                // valeurs de la ligne
                                for (int j = 0; j < k; j++)
                                {
                                    string texte;
                                    if (double.IsInfinity(matrix[i, j]))
                                        texte = "∞";
                                    else
                                        texte = matrix[i, j].ToString("0.0");

                                    Console.Write(texte.PadLeft(colWidth));
                                }
                                Console.WriteLine();
                            }
                            Pause();
                            break;

                        case "9":
                            Console.Clear();
                            Console.WriteLine("===== Graphe des villes (cercle + gradient) =====\n");

                            // 1) Définir l'ordre voulu
                            var ordered = new List<string>
                             {
                                "Strasbourg","Nice","Rennes","Paris","Lyon",
                                "Marseille","Toulouse","Bordeaux","Lille","Nantes"
                            };
                            var liens = grapheService.GetLiens()
                                .Select(x => (x.Depart, x.Arrivee, x.DistanceKm))
                                .ToList();

                            GrapheVisualizer.DrawCircularGradientGraph(
                                ordered, liens, 800, 800, "graphe.png"
                            );
                            Process.Start(new ProcessStartInfo("graphe.png") { UseShellExecute = true });

                            Pause();
                            break;

                        case "10":
                            return;

                        default:
                            Console.WriteLine("Choix invalide.");
                            Pause();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                    Pause();
                }
            }


        }

        public static class GrapheVisualizer
        {
            private static SKColor InterpolateColor(SKColor start, SKColor end, float t)
            {
                t = Math.Clamp(t, 0f, 1f); // Ensure t is between 0 and 1
                byte r = (byte)(start.Red + (end.Red - start.Red) * t);
                byte g = (byte)(start.Green + (end.Green - start.Green) * t);
                byte b = (byte)(start.Blue + (end.Blue - start.Blue) * t);
                byte a = (byte)(start.Alpha + (end.Alpha - start.Alpha) * t);
                return new SKColor(r, g, b, a);
            }
            public static void DrawCircularGradientGraph(
                List<string> orderedCities,
                List<(string CityA, string CityB, double Distance)> edges,
                int width,
                int height,
                string outputPath)
            {
                // Positions en cercle
                int n = orderedCities.Count;
                var center = new SKPoint(width / 2f, height / 2f);
                float radius = Math.Min(width, height) * 0.40f;
                var pos = new Dictionary<string, SKPoint>();
                for (int i = 0; i < n; i++)
                {
                    double angle = 2 * Math.PI * i / n - Math.PI / 2;
                    pos[orderedCities[i]] = new SKPoint(
                        center.X + radius * (float)Math.Cos(angle),
                        center.Y + radius * (float)Math.Sin(angle)
                    );
                }

                // Min/max
                var allD = edges.Select(e => e.Distance).ToList();
                double dMin = allD.Min(), dMax = allD.Max();

                using var bmp = new SKBitmap(width, height);
                using var canvas = new SKCanvas(bmp);
                canvas.Clear(SKColors.White);

                // 1) Arêtes avec gradient 4 stops
                var paintEdge = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
                foreach (var (a, b, dist) in edges)
                {
                    var p1 = pos[a]; var p2 = pos[b];
                    float t = (float)((dist - dMin) / (dMax - dMin));

                    // Dégradé multi‑couleurs 
                    // beige → jaune → orange → rouge
                    SKColor c;
                    if (t < 0.33f)
                        c = InterpolateColor(new SKColor(245, 245, 220 /* beige */), SKColors.Yellow, t / 0.33f);
                    else if (t < 0.66f)
                        c = InterpolateColor(SKColors.Yellow, SKColors.Orange, (t - 0.33f) / 0.33f);
                    else
                        c = InterpolateColor(SKColors.Orange, SKColors.Red, (t - 0.66f) / 0.34f);

                    paintEdge.Color = c;
                    canvas.DrawLine(p1, p2, paintEdge);
                }

                // 2) Nœuds & étiquettes
                var paintNode = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.LightBlue, IsAntialias = true };
                var paintText = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };
                foreach (var city in orderedCities)
                {
                    var p = pos[city];
                    canvas.DrawCircle(p, 12, paintNode);
                    var w = paintText.MeasureText(city);
                    canvas.DrawText(city, p.X - w / 2, p.Y - 14, paintText);
                }

                // 3) Légende multi-repères
                float legendW = 20, legendH = radius * 1.2f;
                float lx = 20, ly = center.Y - legendH / 2;
                // shader linéaire vertical
                var shader = SKShader.CreateLinearGradient(
                    new SKPoint(lx, ly + legendH),   // bas de la barre
                    new SKPoint(lx, ly),             // haut de la barre
                    new SKColor[] {
                        new SKColor(245, 245, 220),  // beige
                        SKColors.Yellow,
                        SKColors.Orange,
                         SKColors.Red
                },
                new float[] { 0f, 0.33f, 0.66f, 1f },
                SKShaderTileMode.Clamp);

                var paintLegend = new SKPaint { Shader = shader };
                var rect = new SKRect(lx, ly, lx + legendW, ly + legendH);
                canvas.DrawRect(rect, paintLegend);
                // bordure
                canvas.DrawRect(rect, new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = 1 });

                // 4) Repères et labels
                paintText.TextSize = 12;
                int steps = 4;
                for (int i = 0; i <= steps; i++)
                {
                    float frac = i / (float)steps;         // 0, .25, .5, .75, 1
                    double val = dMin + (dMax - dMin) * frac;
                    float yPos = ly + legendH * (1 - frac);
                    // petit trait
                    canvas.DrawLine(lx + legendW, yPos, lx + legendW + 6, yPos, paintText);
                    // texte indenté
                    var label = $"{val:0} km";
                    canvas.DrawText(label, lx + legendW + 10, yPos + 4, paintText);
                }

                // 5) Sauvegarde
                using var img = SKImage.FromBitmap(bmp);
                using var data = img.Encode(SKEncodedImageFormat.Png, 90);
                using var fs = File.OpenWrite(outputPath);
                data.SaveTo(fs);
            }
        }


        private static void Pause()
        {
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }
    #endregion
}