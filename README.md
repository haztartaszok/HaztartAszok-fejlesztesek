# ÁszAdmin2.0

## Áttekintés

Az **ÁszAdmin2.0** egy `.NET 8` alapú Windows Forms adminisztrációs alkalmazás
Hotcakes Commerce / DNN webshopok kezeléséhez.

A projekt célja, hogy a napi termékadminisztrációs feladatok egy részét ne csak a
webes adminfelületen, hanem egy gyorsabb, célzott asztali alkalmazásból is el
lehessen végezni.

A jelenlegi alkalmazás fő területei:

- Excel alapú importálás
- tömeges ár- és státuszmódosítás
- kategóriák tömeges kezelése
- termékképek kezelése


## Fő funkciók

### Importálás

Az alkalmazás Excel munkafüzetből képes adatokat beolvasni és feldolgozni.

Támogatott importterületek:

- termékek létrehozása és frissítése SKU alapján
- kategóriakapcsolatok importálása
- termékképek importálása
- terméktulajdonságok importálása

Főbb jellemzők:

- `.xlsx` és `.xlsm` fájlok beolvasása
- előnézet és validáció import előtt
- SKU alapú termékazonosítás
- terméktípusok és property-k feloldása Hotcakes oldalról
- importelőzmény helyi naplózása


### Tömeges műveletek

Az alkalmazás külön felületen támogatja a nagyobb katalógusmódosításokat.

Jelenleg elérhető:

- tömeges árfrissítés
- tömeges aktiválás / inaktiválás

Az árfrissítés futtatható:

- összes termékre
- adott kategóriára

Támogatott árfrissítési módok:

- százalékos módosítás
- fix összegű módosítás
- új ár beállítása

Az aktiválás / inaktiválás futtatható:

- összes termékre
- adott kategóriára
- nincs készleten szűrés alapján


### Kategória kezelő

Az alkalmazás külön **Kategória kezelő** fület tartalmaz a termékek kategóriáinak
tömeges szerkesztésére.

Fő funkciók:

- szűrés SKU alapján
- szűrés `Product type` alapján
- találatok megjelenítése táblázatban
- aktuális kategóriák megjelenítése termékenként
- kategória tömeges hozzáadása kijelölt termékekhez
- kategória tömeges eltávolítása kijelölt termékektől

Ez a modul elkülönített megvalósításban került a projektbe, így könnyebben
karbantartható és szükség esetén egyszerűbben visszavonható.


### Kép szerkesztő

Az alkalmazás külön **Kép szerkesztő** fület tartalmaz a termékképek kezelésére.

Fő funkciók:

- SKU szerinti termékszűrés
- találati lista `DataGridView`-ban
- kiválasztott termék képeinek betöltése
- főkép és további képek megjelenítése
- kiválasztott kép főképpé tétele
- kiválasztott kép törlése
- új képek feltöltése egyesével vagy több fájllal
- képek frissítése

Fontosabb működési részletek:

- a rendszer kezeli a főképek és a további képek eltérő Hotcakes tárolási
  struktúráját
- az azonos nevű, már meglévő képek feltöltéskor kihagyásra kerülnek
- a megerősítő ablakok magyar `Igen / Nem` gombokat használnak


## Képimport és képkezelés fontosabb javításai

A projekt jelenlegi állapotában a képfeltöltés és képkezelés már az alábbi
javításokat tartalmazza:

- a további képekhez szükséges Hotcakes `ProductImage` rekord létrehozása
- a feltöltött további képek helyes megjelenítése a webshopban
- duplikált képek kiszűrése azonos fájlnév alapján
- már meglévő főkép mellett az új képek további képként kezelése
- külön képszerkesztő felület a manuális karbantartáshoz


## Technológia

- .NET 8
- Windows Forms
- C#
- Hotcakes Commerce REST API
- `HttpClient`
- `System.Text.Json`
- `ZipArchive`
- `XDocument`


## Projektstruktúra

```text
repo/
|- README.md
|- ImportSablon.xlsx
|- logo.ico
\- ÁszAdmin1.0/
   |- ÁszAdmin1.0.slnx
   \- WinFormsApp1/
      |- AppSettings.cs
      |- Form1.cs
      |- Form1.CategoryManager.cs
      |- Form1.ImageEditor.cs
      |- HotcakesApiClient.cs
      |- HotcakesModels.cs
      |- ImportHistoryDialog.cs
      |- ImportHistoryStore.cs
      |- Program.cs
      |- Hasznalati-utmutato.txt
      |- settings.example.json
      |- settings.json
      \- WinFormsApp1.csproj
```


## Fontosabb fájlok

- [README.md](C:/Users/inforendszerek/Desktop/Máté/repo/README.md)
- [ÁszAdmin1.0/WinFormsApp1/Form1.cs](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/Form1.cs)
- [ÁszAdmin1.0/WinFormsApp1/Form1.CategoryManager.cs](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/Form1.CategoryManager.cs)
- [ÁszAdmin1.0/WinFormsApp1/Form1.ImageEditor.cs](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/Form1.ImageEditor.cs)
- [ÁszAdmin1.0/WinFormsApp1/HotcakesApiClient.cs](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/HotcakesApiClient.cs)
- [ÁszAdmin1.0/WinFormsApp1/settings.example.json](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/settings.example.json)
- [ÁszAdmin1.0/WinFormsApp1/Hasznalati-utmutato.txt](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/Hasznalati-utmutato.txt)


## Konfiguráció

Az alkalmazás a Hotcakes kapcsolat adatait `settings.json` fájlból olvassa be.

Minta:

```json
{
  "Hotcakes": {
    "BaseUrl": "http://szerver-cim-vagy-domain",
    "ApiKey": "ide-jon-a-hotcakes-api-kulcs"
  }
}
```

Megjegyzés:

- a `settings.example.json` csak minta
- a tényleges `settings.json` fájl nincs verziókezelésre szánva
- ha a kapcsolat nem érhető el, az alkalmazás egyes funkciói nem használhatók


## Excel sablon felépítése

Az alkalmazás a következő logikai munkalapokkal dolgozik:

| Munkalap | Cél | Alap oszlopok |
| --- | --- | --- |
| `Termekek` | termék létrehozás / frissítés | `SKU`, `Nev`, `Ar`, `Keszlet`, `TermekTipus`, `Leiras` |
| `Kategoriak` | kategóriakapcsolatok | `SKU`, `KategoriaSlug` |
| `Kepek` | képfeltöltés | `SKU`, `KepUtvonal`, `KepNev` |
| `OpciokTulajdonsagok` | tulajdonságimport | `SKU`, `TulajdonsagNev`, `TulajdonsagErtek` |


## Build

Egyszerű build:

```powershell
dotnet build .\ÁszAdmin1.0\WinFormsApp1\WinFormsApp1.csproj
```


## Használati útmutató

Az alkalmazáshoz külön, végfelhasználóknak szánt rövid útmutató is tartozik:

- [ÁszAdmin1.0/WinFormsApp1/Hasznalati-utmutato.txt](C:/Users/inforendszerek/Desktop/Máté/repo/ÁszAdmin1.0/WinFormsApp1/Hasznalati-utmutato.txt)


## Megjegyzés

Az újabb nagyobb bővítések, például a **Kategória kezelő** és a **Kép szerkesztő**
elkülönített partial fájlokban készültek el. Ez tudatos döntés volt, hogy az új
funkciók könnyebben fejleszthetők, karbantarthatók és szükség esetén egyszerűbben
visszavonhatók legyenek.
