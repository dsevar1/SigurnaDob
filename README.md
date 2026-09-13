# Sigurna dob

Web aplikacija za upravljanje domom za starije osobe — evidencija korisnika doma, soba, djelatnika, obiteljskih kontakata, zadataka skrbi, zahtjeva za posjet i aktivnosti.

## Arhitektura

Projekt se sastoji od tri dijela:

- **SigurnaDob.App** — Blazor Web App (Interactive Server), MudBlazor korisničko sučelje
- **SigurnaDob.Api** — ASP.NET Core Web API, Entity Framework Core, JWT autentikacija
- **SigurnaDob.Shared** — DTO modeli i EF entiteti koje App i Api dijele

Baza podataka: SQLite. App i Api se pokreću kao dva odvojena procesa i komuniciraju isključivo preko HTTP poziva (namjerno bez HTTPS-a između njih — vidi napomenu u nastavku).

## Preduvjeti

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (ili bilo koji uređivač + terminal)

## Postavljanje (prvi put nakon kloniranja)

### 1. Instaliraj lokalne alate

U korijenu repozitorija (gdje je `SigurnaDob.slnx`):
```
dotnet tool restore
```

Ovo instalira `dotnet-ef` alat potreban za migracije (lokalno, prema `.config/dotnet-tools.json`, ne globalno na sustavu).

### 2. Postavi JWT tajni ključ

Aplikacija koristi JWT tokene za autentikaciju. Tajni ključ **namjerno nije uključen u repozitorij** (sigurnosno pravilo — tajne se ne smiju spremati u Git). Prije prvog pokretanja, potrebno ga je postaviti lokalno:
```
cd SigurnaDob.Api
dotnet user-secrets set "Jwt:Key" "OVDJE-UPISI-NASUMICAN-DUGACAK-STRING-BAREM-32-ZNAKA"
```

Ključ može biti bilo koji dovoljno dugačak nasumičan tekst (preporučeno barem 32 znaka). Primjer generiranja u PowerShellu:

```powershell
$key = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
dotnet user-secrets set "Jwt:Key" $key
```

**Bez ovog koraka aplikacija neće raditi** — Api će prijaviti grešku pri pokretanju.

### 3. Primijeni migracije baze

I dalje unutar `SigurnaDob.Api` foldera:
```
dotnet ef database update
```

Ovo stvara `sigurnadob.db` SQLite datoteku sa svim tablicama i početnim (seed) podacima — statusi, tipovi, demo sobe, demo korisnički računi.

## Pokretanje

Potrebno je pokrenuti **oba** projekta istovremeno. URL na kojem se App otvara ovisi o načinu pokretanja — oba načina rade jednako dobro.

### Kroz Visual Studio (F5 / Multiple Startup Projects)

Desni klik na Solution → Properties → Startup Project → "Multiple startup projects" → postavi `SigurnaDob.Api` i `SigurnaDob.App` oboje na "Start".

Preglednik se automatski otvara na:
- Aplikacija (App): `https://localhost:7284`
- API Swagger: `https://localhost:7282/swagger`

### Kroz terminal (`dotnet run`)

U dva odvojena terminala:
```
dotnet run --project SigurnaDob.Api
dotnet run --project SigurnaDob.App
```

Nakon pokretanja:
- Aplikacija (App): `http://localhost:5232`
- API Swagger: `http://localhost:5285/swagger`

> **Napomena o HTTP-u/HTTPS-u:** Vizualno različit URL ovisno o metodi pokretanja (VS bira "https" launch profil, plain `dotnet run` bez `--launch-profile` bira prvi profil u `launchSettings.json`, "http") ne mijenja stvarnu komunikaciju — App i Api **interno** uvijek razgovaraju preko HTTP-a, neovisno o tome preko kojeg URL-a se pristupa App-u u pregledniku. `SigurnaDob.App/appsettings.json` ima `ApiBaseUrl` postavljen na `http://localhost:5285/`, a Api (bez obzira pokrenut li se "http" ili "https" profilom) uvijek sluša i na `http://localhost:5285` usporedno. Api namjerno ne koristi `UseHttpsRedirection()` (cross-scheme redirect bi izbrisao Authorization header i pokvario JWT autentikaciju) — zato App-Api komunikacija ostaje na HTTP-u čak i kad je Api pokrenut "https" profilom i preglednik gleda App preko HTTPS-a.

## Uloge u aplikaciji

- **Admin** — puni pristup, upravljanje korisničkim računima i ulogama
- **Coordinator** — upravljanje korisnicima doma, sobama, zadacima skrbi, zahtjevima za posjet i aktivnostima
- **Caregiver** — vidi i izvršava samo svoje dodijeljene zadatke skrbi
- **FamilyMember** — šalje zahtjeve za posjet i prati status vlastitih zahtjeva

## Demo računi

| Korisničko ime | Lozinka | Uloga |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `coordinator` | `Coord123!` | Coordinator |
| `caregiver` | `Caregiver123!` | Caregiver |
| `familymember` | `Family123!` | FamilyMember |

## Napomene

- Demo korisnik `familymember` nema unaprijed povezan obiteljski kontakt (nema seed podatka za to) — na svježoj bazi tablica obiteljskih kontakata je prazna. Da bi se testirao tok slanja zahtjeva za posjet: (1) prijavljen kao `admin` ili `coordinator`, na **Korisnici doma** stranici kreiraj korisnika doma (ili koristi postojećeg), (2) na njegovom profilu dodaj obiteljski kontakt, (3) na **Users** stranici (prijavljen kao `admin`) poveži `familymember` račun s tim novokreiranim kontaktom.
- Dokumenti korisnika doma spremaju se lokalno u `SigurnaDob.Api/Storage/` folder (generira se automatski kod prvog uploada). Ovaj repozitorij nema `.gitignore` datoteku (isključivanje se rješava kroz osobni globalni gitignore) — pripazi da testno uploadani dokumenti ne završe u commitu.
