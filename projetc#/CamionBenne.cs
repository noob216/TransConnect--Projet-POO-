using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente un camion-benne de la flotte (travaux publics, benne amovible).
    public class CamionBenne : Vehicule
    {

        //Volume de la benne en mètres cubes.
        public double VolumeM3 { get; set; }
        public double ChargeMaxKg { get; set; }

        //Indique si le camion est équipé d'une grue auxiliaire.
        public bool GrueAuxiliaire { get; set; }

        //Constructeur par défaut
        public CamionBenne()
            : base(id: "CB000", capacite: 8000)
        {
            VolumeM3 = 8.0;
            ChargeMaxKg = 5000;
            GrueAuxiliaire = false;
        }

        // Constructeur complet
        public CamionBenne(
            string id,
            int capacite,
            double volumeM3,
            double chargeMaxKg,
            bool grueAuxiliaire
        ) : base(id, capacite)
        {
            if (volumeM3 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeM3), "Le volume doit être positif.");
            }
            if (chargeMaxKg <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chargeMaxKg), "La charge maximale doit être positive.");
            }

            VolumeM3 = volumeM3;
            ChargeMaxKg = chargeMaxKg;
            GrueAuxiliaire = grueAuxiliaire;
        }

        // 3) Constructeur minimal sans charge max
        public CamionBenne(
            string id,
            double volumeM3
        ) : base(id, capacite: (int)(volumeM3 * 700))
        {
            if (volumeM3 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeM3), "Le volume doit être positif.");
            }

            VolumeM3 = volumeM3;
            ChargeMaxKg = volumeM3 * 600;  // estimation
            GrueAuxiliaire = false;
        }
        public override string ToString()
        {
            return $"{base.ToString()}, Volume : {VolumeM3:N1} m³, Charge max : {ChargeMaxKg:N0} kg, " + $"Grue auxiliaire : {(GrueAuxiliaire ? "Oui" : "Non")}";
        }
    }
}
