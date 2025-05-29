using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetc_
{
    //Représente un salarié de l'entreprise
    // Hérite de Personne ; propose trois constructeurs (par défaut, complet et minimal)
    public class Salarie : Personne
    {
        public DateTime DateEntree { get; }
        public string Poste { get; set; }
        public decimal Salaire { get; set; }

        // Le supérieur hiérarchique (null si c’est un top‑manager)
        public Salarie Superieur { get; private set; }

        // Liste des subordonnés directs
        public IReadOnlyList<Salarie> Subordonnes => _subordonnes;
        private readonly List<Salarie> _subordonnes = new List<Salarie>();


        //Constructeur par défaut
        public Salarie()
            : base(numeroSS: "0000000000000", nom: "NomDefaut", prenom: "PrenomDefaut", dateNaissance: default, adressePostale: "AdresseDefaut", email: "email@exemple.com", telephone: "0000000000")
        {
            DateEntree = DateTime.Now;
            Poste = "PosteDefaut";
            Salaire = 0m;
        }

        //Constructeur complet
        public Salarie(
            string numeroSS,
            string nom,
            string prenom,
            DateTime dateNaissance,
            string adressePostale,
            string email,
            string telephone,
            DateTime dateEntree,
            string poste,
            decimal salaire
        ) : base(numeroSS, nom, prenom, dateNaissance, adressePostale, email, telephone)
        {
            DateEntree = dateEntree;
            Poste = poste ?? throw new ArgumentNullException(nameof(poste));
            Salaire = salaire;
        }

        //Constructeur sans adresse, email et téléphone
        public Salarie(
            string numeroSS,
            string nom,
            string prenom,
            DateTime dateNaissance,
            DateTime dateEntree,
            string poste
        ) : base(
            numeroSS,
            nom,
            prenom,
            dateNaissance,
            adressePostale: "AdresseDefaut",
            email: "email@exemple.com",
            telephone: "0000000000"
        )
        {
            DateEntree = dateEntree;
            Poste = poste ?? throw new ArgumentNullException(nameof(poste));
            Salaire = 0m;
        }

        //Méthodes
        public void DefinirSuperieur(Salarie nouveauSuperieur)
        {
            if (nouveauSuperieur == null) throw new ArgumentNullException(nameof(nouveauSuperieur));
            if (nouveauSuperieur == this) throw new InvalidOperationException("Un salarié ne peut pas être son propre supérieur.");

            // Retirer de l'ancien
            Superieur?._subordonnes.Remove(this);

            // Affecter le nouveau
            Superieur = nouveauSuperieur;
            if (nouveauSuperieur != null && !nouveauSuperieur._subordonnes.Contains(this))
                nouveauSuperieur._subordonnes.Add(this);
        }

        internal void AjouterSubordonne(Salarie sub)
        {
            if (sub == null) throw new ArgumentNullException(nameof(sub));
            if (sub == this) throw new InvalidOperationException("Un salarié ne peut pas être son propre subordonne.");
            if (!_subordonnes.Contains(sub))
            {
                _subordonnes.Add(sub);
                sub.Superieur = this;
            }
        }

        public void RetirerSubordonne(Salarie sub)
        {
            if (sub == null) throw new ArgumentNullException(nameof(sub));
            if (_subordonnes.Remove(sub))
                sub.Superieur = null;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Entré le {DateEntree:dd/MM/yyyy}, Poste : {Poste}, Salaire : {Salaire:C}";
        }
    }

    //Service pour la gestion des salariés : organigramme, embauche, licenciement.
    public class SalarieService
    {
        private readonly IRepository<Salarie> _salarieRepo;
        // Constructeur du service avec injection du repository Salarié.
        public SalarieService(IRepository<Salarie> salarieRepo)
        {
            _salarieRepo = salarieRepo ?? throw new ArgumentNullException(nameof(salarieRepo));
        }

        // Retourne tous les salariés.
        public IEnumerable<Salarie> GetAllSalaries()
            => _salarieRepo.GetAll();

        // Retourne le salarié correspondant au numéro SS.
        public Salarie GetSalarieByNumeroSS(string numeroSS)
        {
            if (string.IsNullOrWhiteSpace(numeroSS))
                throw new ArgumentException("Le numéro de sécurité sociale est requis.", nameof(numeroSS));

            return _salarieRepo.GetById(numeroSS);
        }

        // Embauche un nouveau salarié, optionnellement sous un supérieur existant
        public void Hire(Salarie nouveau, string numeroSuperieurSS = null)
        {
            if (nouveau == null) throw new ArgumentNullException(nameof(nouveau));
            _salarieRepo.Add(nouveau);

            if (!string.IsNullOrWhiteSpace(numeroSuperieurSS))
            {
                var sup = GetSalarieByNumeroSS(numeroSuperieurSS)
                          ?? throw new KeyNotFoundException("Supérieur introuvable.");
                nouveau.DefinirSuperieur(sup);
                _salarieRepo.Update(nouveau);
                _salarieRepo.Update(sup);
            }
        }

        // Licencie un salarié : détache subordonnés et supprime.
        public void Fire(string numeroSS)
        {
            var s = GetSalarieByNumeroSS(numeroSS)
                    ?? throw new KeyNotFoundException("Salarié introuvable.");

            // Détacher subordonnés
            foreach (var sub in s.Subordonnes.ToList())
            {
                s.RetirerSubordonne(sub);
                _salarieRepo.Update(sub);
            }

            // Détacher du supérieur
            if (s.Superieur != null)
            {
                var sup = s.Superieur;
                sup.RetirerSubordonne(s);
                _salarieRepo.Update(sup);
            }

            _salarieRepo.Delete(numeroSS);
        }

        // Modifie un salarié existant.
        public void Update(Salarie salarie)
        {
            if (salarie == null) throw new ArgumentNullException(nameof(salarie));
            _salarieRepo.Update(salarie);
        }

        // Retourne les subordonnés directs d'un salarié.
        public IEnumerable<Salarie> GetSubordinates(string numeroSS)
        {
            var s = GetSalarieByNumeroSS(numeroSS)
                    ?? throw new KeyNotFoundException("Salarié introuvable.");
            return s.Subordonnes;
        }

        //Retourne la chaîne de supérieurs jusqu'à la racine.
        public IEnumerable<Salarie> GetSuperiors(string numeroSS)
        {
            var list = new List<Salarie>();
            var current = GetSalarieByNumeroSS(numeroSS)?.Superieur;
            while (current != null)
            {
                list.Add(current);
                current = current.Superieur;
            }
            return list;
        }

        // Construit la hiérarchie complète (arbre) sous forme d'un dictionnaire <racine, descendants>.
        public IDictionary<Salarie, IEnumerable<Salarie>> GetHierarchy()
        {
            var all = GetAllSalaries().ToList();
            var roots = all.Where(s => s.Superieur == null);
            var dict = new Dictionary<Salarie, IEnumerable<Salarie>>();
            foreach (var root in roots)
                dict[root] = GetDescendants(root);
            return dict;
        }

        private IEnumerable<Salarie> GetDescendants(Salarie root)
        {
            foreach (var sub in root.Subordonnes)
            {
                yield return sub;
                foreach (var desc in GetDescendants(sub))
                    yield return desc;
            }
        }
    }
}
