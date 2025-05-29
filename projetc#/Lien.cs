using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente une arête dans le graphe, soit un trajet entre deux villes.
    public class Lien
    {
        public Noeud Depart { get; set; }
        public Noeud Arrivee { get; set; }
        public double DistanceKm { get; set; }

        // Constructeur par défaut
        public Lien()
        {
            Depart = new Noeud("VilleA_Defaut");
            Arrivee = new Noeud("VilleB_Defaut");
            DistanceKm = 0.0;
        }

        // Constructeur complet : tous les attributs
        public Lien(Noeud depart, Noeud arrivee, double distanceKm)
        {
            Depart = depart ?? throw new ArgumentNullException(nameof(depart));
            Arrivee = arrivee ?? throw new ArgumentNullException(nameof(arrivee));
            if (distanceKm < 0)
                throw new ArgumentOutOfRangeException(nameof(distanceKm), "La distance doit être positive ou nulle.");
            DistanceKm = distanceKm;
        }

        public override string ToString()
        {
            return $"{Depart} → {Arrivee} : {DistanceKm:N1} km";
        }
    }
}
