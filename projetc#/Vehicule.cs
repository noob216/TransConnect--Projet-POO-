using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    // Véhicule
    public abstract class Vehicule
    {
        public string Id { get; }
        public int Capacite { get; } //Capacité de chargement (volume en m³strictement positif)

        // Constructeur protégé pour forcer l'utilisation par les sous-classes.
        protected Vehicule(string id, int capacite)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("L'identifiant du véhicule est requis.", nameof(id));
            if (capacite <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacite), "La capacité doit être strictement positive.");

            Id = id;
            Capacite = capacite;
        }

        // Type du véhicule (le nom de la classe concrète).
        public string Type => GetType().Name;

        public override string ToString()
        {
            return $"{Type} (ID: {Id}, Capacité: {Capacite})";
        }
    }

    //Factory pour créer des instances de Véhicule selon un type et des paramètres optionnels.
    public static class VehiculeFactory
    {
        /*Crée un véhicule du type spécifié.
        <param name="type">"Voiture", "Camionnette", "PoidsLourd", "CamionCiterne", "CamionBenne", "CamionFrigorifique".</param>
        <param name="id">Identifiant unique du véhicule.</param>
        <param name="capacite">Capacité de base (places ou charge en kg).</param>
        <param name="options">Paramètres spécifiques au type (noms des clés :</param>
        <returns>Instance de Vehicule.</returns>*/
        public static Vehicule CreateVehicule(
            string type,
            string id,
            int capacite,
            Dictionary<string, object> options = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Le type de véhicule est requis.", nameof(type));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("L'ID du véhicule est requis.", nameof(id));
            if (capacite <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacite), "La capacité doit être positive.");

            options ??= new Dictionary<string, object>();
            switch (type.Trim().ToLowerInvariant())
            {
                case "voiture":
                    {
                        int nombrePlaces = options.ContainsKey("NombrePlaces")
                            ? Convert.ToInt32(options["NombrePlaces"]) : capacite;
                        string carburant = options.ContainsKey("TypeCarburant")
                            ? options["TypeCarburant"].ToString() : "Essence";
                        return new Voiture(id, capacite, nombrePlaces, carburant);
                    }
                case "camionnette":
                    {
                        double volumeM3 = options.ContainsKey("VolumeM3")
                            ? Convert.ToDouble(options["VolumeM3"]) : 3.0;
                        double chargeMax = options.ContainsKey("ChargeMaxKg")
                            ? Convert.ToDouble(options["ChargeMaxKg"]) : capacite;
                        return new Camionnette(id, capacite, volumeM3, chargeMax);
                    }
                case "poidslourd":
                    {
                        double chargeMax = options.ContainsKey("ChargeMaxKg")
                            ? Convert.ToDouble(options["ChargeMaxKg"]) : capacite;
                        double volumeM3 = options.ContainsKey("VolumeM3")
                            ? Convert.ToDouble(options["VolumeM3"]) : chargeMax / 500.0;
                        return new PoidsLourd(id, chargeMax, volumeM3);
                    }
                case "camionciterne":
                    {
                        double volumeM3 = options.ContainsKey("VolumeM3")
                            ? Convert.ToDouble(options["VolumeM3"]) : 5.0;
                        string liquide = options.ContainsKey("TypeLiquide")
                            ? options["TypeLiquide"].ToString() : "Eau";
                        double pression = options.ContainsKey("PressionMaxBar")
                            ? Convert.ToDouble(options["PressionMaxBar"]) : 10.0;
                        return new CamionCiterne(id, capacite, volumeM3, liquide, pression);
                    }
                case "camionbenne":
                    {
                        double volumeM3 = options.ContainsKey("VolumeM3")
                            ? Convert.ToDouble(options["VolumeM3"]) : 8.0;
                        double chargeMax = options.ContainsKey("ChargeMaxKg")
                            ? Convert.ToDouble(options["ChargeMaxKg"]) : capacite;
                        bool grue = options.ContainsKey("GrueAuxiliaire")
                            ? Convert.ToBoolean(options["GrueAuxiliaire"]) : false;
                        return new CamionBenne(id, capacite, volumeM3, chargeMax, grue);
                    }
                case "camionfrigorifique":
                    {
                        double volumeM3 = options.ContainsKey("VolumeM3")
                            ? Convert.ToDouble(options["VolumeM3"]) : 10.0;
                        double tMin = options.ContainsKey("TemperatureMinC")
                            ? Convert.ToDouble(options["TemperatureMinC"]) : -10.0;
                        double tMax = options.ContainsKey("TemperatureMaxC")
                            ? Convert.ToDouble(options["TemperatureMaxC"]) : 5.0;
                        return new CamionFrigorifique(id, capacite, volumeM3, tMin, tMax);
                    }
                default:
                    throw new ArgumentException($"Type de véhicule inconnu : {type}", nameof(type));
            }
        }
    }
}
