using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    // Représente un camion-citerne de la flotte (transport de liquides ou gaz)
    public class CamionCiterne : Vehicule
    {
        // Volume de la citerne en m³
        public double VolumeM3 { get; set; }

        // Type de liquide à transporter
        public string TypeLiquide { get; set; }

        // Pression maximale supportée (en bars)
        public double PressionMaxBar { get; set; }

        // Constructeur par défaut
        public CamionCiterne()
            : base("CT000", 5000)
        {
            VolumeM3 = 5.0;
            TypeLiquide = "Eau";
            PressionMaxBar = 10.0;
        }

        // Constructeur complet
        public CamionCiterne(string id, int capacite, double volumeM3, string typeLiquide, double pressionMaxBar)
            : base(id, capacite)
        {
            // Vérification des paramètres
            if (volumeM3 <= 0)
            {
                throw new ArgumentOutOfRangeException("volumeM3", "Le volume doit être supérieur à zéro.");
            }

            if (typeLiquide == null || typeLiquide.Trim().Length == 0)
            {
                throw new ArgumentException("Vous devez indiquer un type de liquide.", "typeLiquide");
            }

            if (pressionMaxBar <= 0)
            {
                throw new ArgumentOutOfRangeException("pressionMaxBar", "La pression doit être positive.");
            }

            VolumeM3 = volumeM3;
            TypeLiquide = typeLiquide;
            PressionMaxBar = pressionMaxBar;
        }

        // Constructeur minimal : volume + id
        public CamionCiterne(string id, double volumeM3)
            : base(id, (int)(volumeM3 * 1000))
        {
            if (volumeM3 <= 0)
            {
                throw new ArgumentOutOfRangeException("volumeM3", "Le volume doit être supérieur à zéro.");
            }

            VolumeM3 = volumeM3;
            TypeLiquide = "Liquide indéterminé";
            PressionMaxBar = 5.0;
        }

        // Texte affiché en console
        public override string ToString()
        {
            return base.ToString()+ ", Volume : " + VolumeM3.ToString("N1") + " m³"+ ", Liquide : " + TypeLiquide+ ", Pression max : " + PressionMaxBar.ToString("N1") + " bar";
        }
    }
}
