using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    // Représente une personne de la société (salarié ou client).

    public abstract class Personne
    {
        public string NumeroSS { get; }
        public string Nom { get; set; }
        public string Prenom { get; }
        public DateTime DateNaissance { get; }
        public string AdressePostale { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        protected Personne(string numeroSS, string nom, string prenom, DateTime dateNaissance, string adressePostale, string email, string telephone)
        {
            // Vérification des arguments
            if (string.IsNullOrWhiteSpace(numeroSS))
                throw new ArgumentException("Le numéro de sécurité sociale est requis.", nameof(numeroSS));
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom est requis.", nameof(nom));
            if (string.IsNullOrWhiteSpace(prenom))
                throw new ArgumentException("Le prénom est requis.", nameof(prenom));
            if (string.IsNullOrWhiteSpace(adressePostale))
                throw new ArgumentException("L'adresse postale est requise.", nameof(adressePostale));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("L'email est requis.", nameof(email));
            if (string.IsNullOrWhiteSpace(telephone))
                throw new ArgumentException("Le téléphone est requis.", nameof(telephone));

            this.NumeroSS = numeroSS;
            this.Nom = nom;
            this.Prenom = prenom;
            this.DateNaissance = dateNaissance;
            this.AdressePostale = adressePostale;
            this.Email = email;
            this.Telephone = telephone;
        }

        public override string ToString()
        {
            return $"{Nom} {Prenom} (NSS : {NumeroSS}) – Né(e) le {DateNaissance:dd/MM/yyyy}";
        }

        // Méthode test pour afficher les informations de la personne
        public virtual void AfficherInformations()
        {
            Console.WriteLine($"Nom : {Nom}");
            Console.WriteLine($"Prénom : {Prenom}");
            Console.WriteLine($"Numéro de sécurité sociale : {NumeroSS}");
            Console.WriteLine($"Date de naissance : {DateNaissance:dd/MM/yyyy}");
            Console.WriteLine($"Adresse postale : {AdressePostale}");
            Console.WriteLine($"Email : {Email}");
            Console.WriteLine($"Téléphone : {Telephone}");
        }
    }
}
