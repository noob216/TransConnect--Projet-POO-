using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente un camion frigorifique de la flotte.
    public class CamionFrigorifique : Vehicule
    {
        //Volume de la chambre frigorifique en mètres cubes.
        public double VolumeM3 { get; set; }

        //Température minimale en degrés Celsius.
        public double TemperatureMinC { get; set; }

        //Température maximale en degrés Celsius.
        public double TemperatureMaxC { get; set; }

        //Constructeur par défaut
        public CamionFrigorifique()
            : base(id: "CF000", capacite: 3000)
        {
            VolumeM3 = 10.0;
            TemperatureMinC = -20.0;
            TemperatureMaxC = 5.0;
        }

        //Constructeur complet
        public CamionFrigorifique(
            string id,
            int capacite,
            double volumeM3,
            double temperatureMinC,
            double temperatureMaxC
        ) : base(id, capacite)
        {
            if (volumeM3 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeM3), "Le volume doit être positif.");
            }
            if (temperatureMinC > temperatureMaxC)
            {
                throw new ArgumentException("La température minimale doit être inférieure ou égale à la température maximale.");
            }
            VolumeM3 = volumeM3;
            TemperatureMinC = temperatureMinC;
            TemperatureMaxC = temperatureMaxC;
        }

        // 3) Constructeur minimal sans température
        public CamionFrigorifique(string id,double volumeM3) : base(id, capacite: (int)(volumeM3 * 250))
        {
            if (volumeM3 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeM3), "Le volume doit être positif.");
            }

            VolumeM3 = volumeM3;
            TemperatureMinC = -20.0;
            TemperatureMaxC = 5.0;
        }
        public override string ToString()
        {
            return $"{base.ToString()}, Volume : {VolumeM3:N1} m³, Température de {TemperatureMinC}°C à {TemperatureMaxC}°C";
        }
    }
}
