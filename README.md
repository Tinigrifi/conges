# 🏖 Congés

Trouve les meilleures périodes de l'année pour poser des congés en maximisant le ratio :

> **ratio = jours off consécutifs obtenus / jours de congé posés**

Exemple : poser **2 jours** autour du pont du 1er mai peut donner **8 jours off** → ratio **4.0**

---

## Projets

| Projet | Description |
|---|---|
| `CalculateBestDaysOff.Domain` | Logique métier partagée (jours fériés, optimiseur) |
| `CalculateBestDaysOff` | Application console colorée |
| `Conges` | Application WinForms |
| `CalculateBestDaysOff.Tests` | Tests unitaires (xUnit) |

---

## Console

```bash
dotnet run --project CalculateBestDaysOff -- [année] [options]
```

### Arguments

| Argument | Description | Défaut |
|---|---|---|
| `année` | Année à analyser | année courante |
| `--max-days <n>` | Max jours de congé posés par opportunité | `10` |
| `--weeks <n>` | Filtre : blocs d'au moins N semaines consécutives | `0` (tous) |
| `--top <n>` | Nombre de résultats à afficher | `10` |
| `--help`, `-h` | Affiche l'aide | |

### Exemples

```bash
# Top 10 opportunités de 2025
dotnet run --project CalculateBestDaysOff -- 2025

# Meilleurs blocs de 2 semaines
dotnet run --project CalculateBestDaysOff -- 2025 --weeks 2

# Top 5, max 3 jours posés, pour 2026
dotnet run --project CalculateBestDaysOff -- 2026 --max-days 3 --top 5
```

### Exemple de sortie

```
╭──────┬─────────────────────────────┬───────┬───────┬────────┬────────────────────┬──────────────╮
│    # │ Période                     │ Durée │ Posés │  Ratio │ Fériés inclus      │ Jours à      │
│      │                             │       │       │        │                    │ poser        │
├──────┼─────────────────────────────┼───────┼───────┼────────┼────────────────────┼──────────────┤
│ 🥇 1 │ ven. 18 avr. → lun. 21 avr. │   4 j │   1 j │ 👍 4.0 │ Lundi de Pâques    │ 18/04        │
│ 🥈 2 │ jeu. 1 mai  → dim. 4 mai    │   4 j │   1 j │ 👍 4.0 │ Fête du Travail    │ 02/05        │
│ 🥉 3 │ sam. 19 avr. → mar. 22 avr. │   4 j │   1 j │ 👍 4.0 │ Lundi de Pâques    │ 22/04        │
╰──────┴─────────────────────────────┴───────┴───────┴────────┴────────────────────┴──────────────╯
```

---

## WinForms

Lance `Conges.exe` ou :

```bash
dotnet run --project Conges
```

---

## Jours fériés pris en compte

Les 11 jours fériés nationaux français :

- Jour de l'An (1er janvier)
- Lundi de Pâques *(calculé)*
- Fête du Travail (1er mai)
- Victoire 1945 (8 mai)
- Ascension *(calculé)*
- Lundi de Pentecôte *(calculé)*
- Fête Nationale (14 juillet)
- Assomption (15 août)
- Toussaint (1er novembre)
- Armistice (11 novembre)
- Noël (25 décembre)

> Pâques est calculé avec l'algorithme de [Meeus/Jones/Butcher](https://en.wikipedia.org/wiki/Date_of_Easter#Anonymous_Gregorian_algorithm).

---

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Build & tests

```bash
dotnet build
dotnet test
```
