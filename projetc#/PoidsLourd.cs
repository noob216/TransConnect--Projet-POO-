using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente un poids-lourd de la flotte
    public class PoidsLourd : Vehicule
    {
        // Charge maximale en kilogrammes.
        public double ChargeMaxKg { get; set; }

        //Volume de chargement en mètres cubes.
        public double VolumeM3 { get; set; }

        // Constructeur par défaut
        public PoidsLourd()
            : base(id: "PL000", capacite: 10000)
        {
            ChargeMaxKg = 10000.0;
            VolumeM3 = 20.0;
        }

        // 2) Constructeur complet : tous les attributs
        public PoidsLourd(
            string id,
            double chargeMaxKg,
            double volumeM3
        ) : base(id, capacite: (int)chargeMaxKg)
        {
            if (chargeMaxKg <= 0)
                throw new ArgumentOutOfRangeException(nameof(chargeMaxKg), "La charge maximale doit être positive.");
            if (volumeM3 <= 0)
                throw new ArgumentOutOfRangeException(nameof(volumeM3), "Le volume doit être positif.");

            ChargeMaxKg = chargeMaxKg;
            VolumeM3 = volumeM3;
        }

        // 3) Constructeur minimal sans volume
        public PoidsLourd(
            string id,
            double chargeMaxKg
        ) : base(id, capacite: (int)chargeMaxKg)
        {
            if (chargeMaxKg <= 0)
                throw new ArgumentOutOfRangeException(nameof(chargeMaxKg), "La charge maximale doit être positive.");

            ChargeMaxKg = chargeMaxKg;
            VolumeM3 = chargeMaxKg / 500.0; // estimation basique du volume
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Charge max : {ChargeMaxKg:N0} kg, Volume : {VolumeM3:N1} m³";
        }
    }
}
