using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente un nœud (ville) dans le graphe géographique.
    public class Noeud
    {
        public string Ville { get; set; }

        //Constructeur par défaut
        public Noeud()
        {
            Ville = "VilleDefaut";
        }

        //Constructeur complet
        public Noeud(string ville)
        {
            if (string.IsNullOrWhiteSpace(ville))
                throw new ArgumentException("Le nom de la ville est requis.", nameof(ville));
            Ville = ville;
        }
        public override string ToString()
        {
            return Ville;
        }
    }
}
