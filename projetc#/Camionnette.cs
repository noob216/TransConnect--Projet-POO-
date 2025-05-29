using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente une camionnette de la flotte.

    public class Camionnette : Vehicule
    {
        //Volume de chargement en mètres cubes.
        public double VolumeM3 { get; set; }

        //Charge utile maximale en kilogrammes.
        public double ChargeMaxKg { get; set; }


        // 1) Constructeur par défaut
        public Camionnette()
            : base(id: "CMT000", capacite: 1000)
        {
            VolumeM3 = 3.0;
            ChargeMaxKg = 1200;
        }

        // 2) Constructeur complet
        public Camionnette(string id,int capacite,double volumeM3,double chargeMaxKg) : base(id, capacite)
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
        }

        // 3) Constructeur minimal sans charge max
        public Camionnette(string id,double volumeM3) : base(id, capacite: (int)(volumeM3 * 300))
        {
            if (volumeM3 <= 0)
                throw new ArgumentOutOfRangeException(nameof(volumeM3), "Le volume doit être positif.");

            VolumeM3 = volumeM3;
            ChargeMaxKg = volumeM3 * 400; // estimation basique
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Volume : {VolumeM3:N1} m³, Charge max : {ChargeMaxKg:N0} kg";
        }
    }
}
