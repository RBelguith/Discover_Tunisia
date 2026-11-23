using System;
using UnityEngine;

namespace DiscoverTunisia.Core
{
    public enum MonumentStyle
    {
        Generic,
        MinaretDome,     // mosquée à minaret + coupole (Kairouan)
        SculptedFacade,  // façade à arcades sculptées (Trois Portes)
        MausoléeDome,    // enceinte à coupoles (Barbier)
        RuinsColumns,    // colonnes et arcs antiques (Néapolis, Utique)
        MedinaSouk,      // maisons de médina + porte en arc (souks)
        FortPort,        // tour de fort + port (Bizerte)
    }

    [Serializable]
    public class MonumentInfo
    {
        public string name;
        public string description;
        public string imageKey;
        public MonumentStyle style;

        public MonumentInfo(string name, string description, string imageKey, MonumentStyle style)
        {
            this.name = name;
            this.description = description;
            this.imageKey = imageKey;
            this.style = style;
        }
    }

    [Serializable]
    public class FoodInfo
    {
        public string name;
        public string description;
        public string imageKey;

        public FoodInfo(string name, string description, string imageKey)
        {
            this.name = name;
            this.description = description;
            this.imageKey = imageKey;
        }
    }

    [Serializable]
    public class ClothingInfo
    {
        public string name;
        public string description;
        public string imageKey;

        public ClothingInfo(string name, string description, string imageKey)
        {
            this.name = name;
            this.description = description;
            this.imageKey = imageKey;
        }
    }

    [Serializable]
    public class GovernorateInfo
    {
        public string id;
        public string name;
        public string subtitle;
        public string description;
        public string shortBlurb;       // panneau "À propos" du globe
        public float latitude;
        public float longitude;
        public Color accentColor;       // couleur de mise en évidence
        public MonumentInfo[] monuments;
        public FoodInfo[] foods;
        public ClothingInfo[] clothes;
    }

    /// <summary>
    /// Base de contenus des trois gouvernorats de la v1 (Nabeul, Bizerte, Kairouan).
    /// Textes rédigés d'après Wikipédia (licence CC BY-SA) — à faire valider par l'encadrant.
    /// </summary>
    public static class ContentDatabase
    {
        public static GovernorateInfo[] All = new GovernorateInfo[]
        {
            new GovernorateInfo
            {
                id = "nabeul",
                name = "Nabeul",
                subtitle = "Cap Bon — Côte est",
                description =
                    "Chef-lieu du gouvernorat de Nabeul, la ville se trouve au sud de la péninsule du cap Bon, " +
                    "à une soixantaine de kilomètres au sud-est de Tunis. Entourée de vergers et de jardins, " +
                    "elle est réputée pour sa poterie et ses faïences, ses souks séculaires et ses plages du golfe d'Hammamet.",
                shortBlurb = "Nabeul, capitale du cap Bon : poterie, souks et agrumes face au golfe d'Hammamet.",
                latitude = 36.45f,
                longitude = 10.73f,
                accentColor = new Color(1.00f, 0.55f, 0.20f),
                monuments = new MonumentInfo[]
                {
                    new MonumentInfo(
                        "Site romain de Néapolis",
                        "Fondée il y a plus de 2 400 ans, Néapolis (« nouvelle cité ») est l'une des villes les plus " +
                        "anciennes d'Afrique du Nord mentionnées par les textes après Carthage. Le site archéologique, " +
                        "à deux kilomètres du centre, dévoile notamment la Maison des Nymphes et de riches mosaïques romaines.",
                        "nabeul_neapolis", MonumentStyle.RuinsColumns),
                    new MonumentInfo(
                        "Médina et souks de Nabeul",
                        "Cœur historique de la ville, la médina s'ouvre par Bab Bled et abrite des souks séculaires : " +
                        "Souk El Balgha (l'un des plus anciens), Souk El Haddada des forgerons et Souk Ezzit de l'huile, " +
                        "autour de la Grande Mosquée érigée au IXe siècle.",
                        "nabeul_medina", MonumentStyle.MedinaSouk),
                },
                foods = new FoodInfo[]
                {
                    new FoodInfo(
                        "Poupées de sucre du cap Bon",
                        "Nabeul est le seul lieu de Tunisie où l'on fabrique des figurines en sucre moulé et peint aux " +
                        "colorants naturels : mariées, gazelles ou poissons pour les filles, coqs, chevaux ou lions pour " +
                        "les garçons. Offertes au Nouvel an islamique, elles sont cassées dix jours après pour célébrer " +
                        "Achoura. La ville est aussi réputée pour sa harissa et ses agrumes.",
                        "nabeul_food"),
                },
                clothes = new ClothingInfo[]
                {
                    new ClothingInfo(
                        "Caftan brodé et parures du cap Bon",
                        "Les ateliers de la médina perpétuent le caftan de soie orné de fil d'or et la chéchia, coiffure " +
                        "de feutre rouge emblématique de la Tunisie. La mariée du cap Bon porte le khit er rouh (« fil de " +
                        "l'âme »), longue chaîne d'argent offerte par la famille du marié, avec des boucles d'oreilles et " +
                        "des bracelets ciselés.",
                        "nabeul_clothes"),
                },
            },

            new GovernorateInfo
            {
                id = "bizerte",
                name = "Bizerte",
                subtitle = "Nord — Entre mer et lac",
                description =
                    "Ville du nord de la Tunisie située entre la mer Méditerranée et le lac de Bizerte, surnommée " +
                    "« Venise africaine ». Comptoir phénicien fondé vers 1100 av. J.-C. (Hippo Diarrhytus), elle doit " +
                    "au canal qui relie le lac à la mer son port millénaire et son vieux port classique.",
                shortBlurb = "Bizerte, la « Venise africaine » : un vieux port millénaire entre lac et Méditerranée.",
                latitude = 37.28f,
                longitude = 9.87f,
                accentColor = new Color(0.25f, 0.65f, 1.00f),
                monuments = new MonumentInfo[]
                {
                    new MonumentInfo(
                        "Vieux port et médina de Bizerte",
                        "Le vieux port, bordé de maisons aux couleurs vives, est l'un des plus anciens du bassin " +
                        "méditerranéen. La médina et le fort édifié au XVIe siècle gardent la mémoire d'une cité corsaire, " +
                        "et l'église orthodoxe Saint-Alexandre-Nevski rappelle la communauté russe installée ici en 1920.",
                        "bizerte_port", MonumentStyle.FortPort),
                    new MonumentInfo(
                        "Ruines d'Utique",
                        "À une trentaine de kilomètres de Bizerte, Utique fut un comptoir phénicien devenu cité romaine " +
                        "prospère. Son théâtre, ses thermes et ses basiliques chrétiennes témoignent de siècles " +
                        "d'histoire punique, romaine et byzantine.",
                        "bizerte_utique", MonumentStyle.RuinsColumns),
                },
                foods = new FoodInfo[]
                {
                    new FoodInfo(
                        "Makrouna abri de Bizerte",
                        "La makrouna abri (ou makrouna diari), pâtes mijotées généreuses de la ville du canal, est " +
                        "accompagnée du tlaytou, de la rechata hlow, de la boughaja et du knef, une pâtisserie locale " +
                        "très appréciée. La cuisine bizertine tire aussi parti de la pêche du lac et de la mer.",
                        "bizerte_food"),
                },
                clothes = new ClothingInfo[]
                {
                    new ClothingInfo(
                        "Costume du nord et bijoux d'argent",
                        "Au nord comme ailleurs en Tunisie, la tenue traditionnelle feminine repose sur le caftan de soie " +
                        "et la fouta, pagne de coton rayé noué à la taille, portés avec la chéchia. Les parures d'argent " +
                        "massif et les filets de corail complètent la tenue des fêtes, notamment le jour du mariage.",
                        "bizerte_clothes"),
                },
            },

            new GovernorateInfo
            {
                id = "kairouan",
                name = "Kairouan",
                subtitle = "Centre — Ville sainte",
                description =
                    "Capitale de l'Ifriqiya pendant cinq siècles et souvent appelée quatrième ville sainte de l'islam, " +
                    "Kairouan est inscrite au patrimoine mondial de l'UNESCO depuis 1988. Médina, souks organisés par " +
                    "corporations et ateliers de tapis en font la première ville sainte du Maghreb.",
                shortBlurb = "Kairouan, ville sainte et patrimoine mondial de l'UNESCO, berceau des tapis tunisiens.",
                latitude = 35.68f,
                longitude = 10.10f,
                accentColor = new Color(0.95f, 0.30f, 0.35f),
                monuments = new MonumentInfo[]
                {
                    new MonumentInfo(
                        "Grande mosquée de Kairouan",
                        "Fondée vers 670 par Oqba Ibn Nafi, elle est le plus ancien et le plus prestigieux monument " +
                        "islamique du Maghreb. Son minaret, édifié entre le VIIIe et le IXe siècle, est le plus ancien " +
                        "au monde encore debout, et son minbar de 862, la plus ancienne chaire du monde musulman.",
                        "kairouan_mosquee", MonumentStyle.MinaretDome),
                    new MonumentInfo(
                        "Mosquée des Trois Portes",
                        "Bâtie en 866, elle est considérée comme la plus ancienne mosquée à façade sculptée de l'islam. " +
                        "Sa façade haute de sept mètres porte trois frises d'inscriptions koufiques d'une rare finesse.",
                        "kairouan_troisportes", MonumentStyle.SculptedFacade),
                    new MonumentInfo(
                        "Mausolée du Barbier",
                        "Vaste zaouïa érigée au XVIIe siècle autour du tombeau d'Abou Zamaa el-Balaoui, compagnon du " +
                        "Prophète. Ses coupoles côtelées, ses céramiques à la mode turque et ses plafonds sculptés en " +
                        "font l'un des lieux les plus visités de Kairouan.",
                        "kairouan_barbier", MonumentStyle.MausoléeDome),
                },
                foods = new FoodInfo[]
                {
                    new FoodInfo(
                        "Makroud de Kairouan",
                        "Pâtisserie emblématique introduite dans la région par les Aghlabides : une semoule fourrée d'une " +
                        "pâte de dattes, coupée en losanges et trempée dans le miel. Servi lors des fêtes et des " +
                        "mariages, le makroud accompagne le thé à la menthe des grandes occasions.",
                        "kairouan_makroud"),
                },
                clothes = new ClothingInfo[]
                {
                    new ClothingInfo(
                        "Caftan de la ville sainte",
                        "À Kairouan, ville des métiers d'art, la tenue de fête associe le caftan de soie brodé, la chéchia " +
                        "rouge et de larges ceintures ornées. La mariée y reçoit le khit er rouh, longue chaîne d'argent " +
                        "symbole de la bénédiction familiale, dans une parure complétée par boucles et bracelets ciselés.",
                        "kairouan_clothes"),
                },
            },
        };

        public static GovernorateInfo GetById(string id)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].id == id) return All[i];
            return null;
        }
    }
}
