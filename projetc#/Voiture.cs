using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente une voiture de la flotte (transport de passagers).
    public class Voiture : Vehicule
    {
        //Nombre de places passagers.
        public int NombrePlaces { get; set; }

        // Type de carburant (ex. "Essence", "Diesel", "Électrique").
        public string TypeCarburant { get; set; }

        // 1) Constructeur par défaut
        public Voiture()
            : base(
                id: "V000",
                capacite: 4
            )
        {
            NombrePlaces = 4;
            TypeCarburant = "Essence";
        }

        // 2) Constructeur complet : tous les attributs
        public Voiture(
            string id,
            int capacite,
            int nombrePlaces,
            string typeCarburant
        ) : base(id, capacite)
        {
            if (nombrePlaces <= 0)
                throw new ArgumentOutOfRangeException(nameof(nombrePlaces), "Le nombre de places doit être positif.");
            if (string.IsNullOrWhiteSpace(typeCarburant))
                throw new ArgumentException("Le type de carburant est requis.", nameof(typeCarburant));

            NombrePlaces = nombrePlaces;
            TypeCarburant = typeCarburant;
        }

        // 3) Constructeur minimal : paramètres essentiels
        public Voiture(
            string id,
            int nombrePlaces
        ) : base(id, nombrePlaces)
        {
            if (nombrePlaces <= 0)
                throw new ArgumentOutOfRangeException(nameof(nombrePlaces), "Le nombre de places doit être positif.");

            NombrePlaces = nombrePlaces;
            TypeCarburant = "Essence";
        }

        // Affiche les détails de la voiture.
        public override string ToString()
        {
            return $"{base.ToString()}, Places : {NombrePlaces}, Carburant : {TypeCarburant}";
        }
    }
}
