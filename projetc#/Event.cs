using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    // Arguments pour les événements relatifs à un salarié
    public class SalarieEventArgs : EventArgs
    {
        public Salarie EventSalarie { get; }
        public DateTime Timestamp { get; }

        public SalarieEventArgs(Salarie salarie)
        {
            EventSalarie = salarie ?? throw new ArgumentNullException(nameof(salarie));
            Timestamp = DateTime.UtcNow;
        }
    }

    //Arguments pour les événements relatifs à une commande
    public class CommandeEventArgs : EventArgs
    {
        public Commande EventCommande { get; }
        public DateTime Timestamp { get; }

        public CommandeEventArgs(Commande cmd)
        {
            EventCommande = cmd ?? throw new ArgumentNullException(nameof(cmd));
            Timestamp = DateTime.UtcNow;
        }
    }

    //Notifier pour exposer des événements métiers globaux
    public static class Notifier
    {
        //Événement déclenché lors d'une embauche
        public static event EventHandler<SalarieEventArgs> SalariéEmbauché;
        //Événement déclenché lors d'un licenciement
        public static event EventHandler<SalarieEventArgs> SalariéLicencié;
        //Événement déclenché lors de la création d'une commande
        public static event EventHandler<CommandeEventArgs> CommandeCréée;

        internal static void OnSalariéEmbauché(Salarie salarie)
        {
            SalariéEmbauché?.Invoke(null, new SalarieEventArgs(salarie));
        }

        internal static void OnSalariéLicencié(Salarie salarie)
        {
            SalariéLicencié?.Invoke(null, new SalarieEventArgs(salarie));
        }

        internal static void OnCommandeCréée(Commande cmd)
        {
            CommandeCréée?.Invoke(null, new CommandeEventArgs(cmd));
        }
    }
}
