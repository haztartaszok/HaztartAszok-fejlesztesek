# ÁszAdmin 1.0

## Projekt leírás

Az **ÁszAdmin 1.0** egy Windows alapú adminisztrációs alkalmazás, amely **Hotcakes Commerce** webáruházak kezelését támogatja **DNN** környezetben. Az alkalmazás fő célja, hogy a termékkezelési feladatokat ne kizárólag a webes adminfelületen kelljen elvégezni, hanem egy külön, gyorsabb, Excel-alapú asztali eszközzel is lehessen dolgozni.

Az alkalmazás két nagy területre fókuszál:

* **Excelből történő tömeges importálásra**
* **közvetlen Hotcakes API-n keresztüli tömeges módosításokra**

Az app nem egyszerű fájlfeltöltő: a tényleges import előtt beolvassa és ellenőrzi az adatokat, összeveti őket a Hotcakes-ben létező termékekkel, kategóriákkal, terméktípusokkal és tulajdonságokkal, majd csak jóváhagyás után futtatja le a módosításokat.

---

## Fő funkciók

* Import sablon (`.xlsx`) generálása az alkalmazásból
* Excel (`.xlsx`, `.xlsm`) munkafüzetek beolvasása és előnézete
* Termékek létrehozása vagy frissítése SKU alapján
* Kategóriakapcsolatok importálása külön munkalapról
* Termékképek feltöltése Excel alapján
* Terméktulajdonságok importálása és szükség esetén új Hotcakes property létrehozása
* Import előtti részletes validáció és hibajelzés
* Importelőzmények helyi naplózása
* Tömeges árfrissítés kiválasztott kategóriára vagy az összes termékre
* Tömeges aktiválás / inaktiválás kategória, összes termék vagy készlethiány alapján

---

## Technológia

* .NET 8
* Windows Forms
* C#
* Hotcakes Commerce REST API
* `HttpClient`
* `System.Text.Json`
* `ZipArchive` + `XDocument` alapú Excel feldolgozás
* Inno Setup 6 telepítő

---

## Az alkalmazás szerepe

Az ÁszAdmin 1.0 elsősorban operatív termékadminisztrációs eszköz:

* új termékek gyors felvitelére
* meglévő termékek tömeges karbantartására
* képek és kategóriák kapcsolására
* Hotcakes terméktulajdonságok kezelésére
* egyszerűbb tömeges ár- és státuszmódosításokra

Az alkalmazás nem igényel Excel telepítést a fájlok előnézetéhez vagy a sablon létrehozásához, mert az `.xlsx` fájlokat közvetlenül OpenXML/ZIP szinten dolgozza fel.

---

## Működési elv

### Induláskor

Az alkalmazás indulás után:

1. betölti a `settings.json` fájlt
2. felépíti a Hotcakes API klienskapcsolatot
3. lekéri a kategóriákat
4. megpróbálja lekérni a terméktípusokat
5. megpróbálja lekérni a terméktulajdonságokat

Ha a Hotcakes kapcsolat nem érhető el, az Excel előnézet továbbra is használható, de az import- és tömeges műveletek nem futtathatók biztonságosan.

### Importfolyamat

Az import általános menete:

1. Excel fájl kiválasztása
2. munkalap előnézetének megjelenítése
3. importtípus kiválasztása
4. validáció futtatása
5. felhasználói jóváhagyás
6. import végrehajtása
7. eredmény és hibák összegzése
8. importelőzmény mentése helyi JSON fájlba

### Tömeges műveletek

Az import oldaltól külön oldalon futnak:

* tömeges árfrissítés
* tömeges aktiválás / inaktiválás

Mindkét művelet a Hotcakes API-n keresztül végzi a módosítást, és a futás előtt megerősítést kér.

---

## Importtípusok

Az alkalmazás jelenleg az alábbi importmódokat támogatja:

* `Termek import`
* `Kep import`
* `Kategoria import`
* `Tulajdonsag import`
* `Osszes importalasa`

Fontos működési részlet:

* a `Termek import` a `Termekek` munkalap mellett opcionálisan a `Kategoriak` munkalapot is feldolgozza, ha az jelen van a munkafüzetben
* a `Kep import` csak a `Kepek` munkalapra épül
* a `Kategoria import` csak a `Kategoriak` munkalapra épül
* a `Tulajdonsag import` csak az `OpciokTulajdonsagok` munkalapra épül
* az `Osszes importalasa` a teljes sablonstruktúrát együtt kezeli

---

## Excel munkafüzet felépítése

Az alkalmazás sablongenerátora az alábbi munkalapokat hozza létre:

| Munkalap | Szerep | Alap oszlopok |
| --- | --- | --- |
| `Termekek` | termék létrehozás / frissítés | `SKU`, `Nev`, `Ar`, `Keszlet`, `TermekTipus`, `Leiras` |
| `Kategoriak` | termék-kategória összerendelés | `SKU`, `KategoriaSlug` |
| `Kepek` | termékképek feltöltése | `SKU`, `KepUtvonal`, `KepNev` |
| `OpciokTulajdonsagok` | terméktulajdonságok importja | `SKU`, `TulajdonsagNev`, `TulajdonsagErtek` |

### Fejlécfelismerés

A fejlécazonosítás:

* kis- és nagybetű érzéketlen
* ékezetérzéketlen
* szóközöket és nem alfanumerikus jeleket figyelmen kívül hagy

Ezért az alkalmazás több angol alias formát is elfogad.

### Támogatott oszlopaliasok

| Logikai mező | Elfogadott fejlécek |
| --- | --- |
| SKU | `SKU` |
| Név | `Nev`, `Name` |
| Ár | `Ar`, `Price` |
| Készlet | `Keszlet`, `Inventory`, `Stock` |
| Terméktípus | `TermekTipus`, `ProductType`, `ProductTypeName` |
| Leírás | `Leiras`, `Description`, `LongDescription` |
| Kategória slug | `KategoriaSlug`, `CategorySlug`, `RewriteUrl` |
| Kép útvonal | `KepUtvonal`, `ImagePath`, `ImageFolder` |
| Kép név | `KepNev`, `ImageName`, `FileName` |
| Tulajdonság név | `TulajdonsagNev`, `PropertyName`, `Key` |
| Tulajdonság érték | `TulajdonsagErtek`, `PropertyValue`, `Value` |

---

## Importszabályok

### Termék import

* a `SKU` kötelező
* új terméknél a `Nev` kötelező
* a `Ar` és `Keszlet` mezők opcionálisak
* a `TermekTipus` opcionális, de ha meg van adva, feloldhatónak kell lennie Hotcakes oldalon
* a meglévő elemek kezelése kétféle lehet:
  * frissítés SKU alapján
  * kihagyás

### Terméktípus feloldása

A `TermekTipus` mező:

* megadható **Hotcakes BVIN** alapján
* megadható **terméktípus név** alapján

Ha a név több Hotcakes terméktípusra is illeszkedik, az import hibával megáll validációs szinten.

### Kategória import

* a `KategoriaSlug` a Hotcakes kategória `RewriteUrl` értékeihez illeszkedik
* az alkalmazás ellenőrzi, hogy a megadott SKU létező vagy ugyanabban az importban létrejövő termékre mutat-e
* a már meglévő vagy ugyanazon fájlon belül duplikált kapcsolatok nem kerülnek újra létrehozásra

### Kép import

* soronként legalább a `SKU` és a kép azonosításához szükséges adat kell
* a kép feloldása több módon is történhet:
  * abszolút elérési útról
  * a munkafüzet mappájához képest relatív útról
  * mappa + fájlnév kombinációból
* SKU-nként az első sikeresen feloldott kép **főkép**
* főkép esetén az alkalmazás a Hotcakes termék kép-metaadatait is frissíti

### Tulajdonság import

* a `TulajdonsagNev` kötelező
* a tulajdonságimport csak olyan terméknél futtatható, amelyhez tartozik vagy feloldható **Hotcakes ProductType**
* ha a megadott tulajdonság még nem létezik Hotcakes oldalon, az alkalmazás új property-t hoz létre
* az új property alapértelmezés szerint:
  * text típusú
  * megjeleníthető a felületen
  * `en-US` culture kóddal jön létre
* ha a property még nincs hozzárendelve a termék ProductType-jához, az alkalmazás ezt is automatikusan elvégzi

### Készletkezelés

Ha a `Keszlet` mező ki van töltve:

* az alkalmazás inventory rekordot hoz létre vagy frissít
* új terméknél a készletmód `WhenOutOfStockShow` jellegű működésre vált
* a készletmódosítás a Hotcakes inventory végponton keresztül történik

---

## Validáció

Az import előtt az alkalmazás részletes ellenőrzést végez, többek között:

* üres SKU sorok keresése
* duplikált SKU-k felismerése
* új termékeknél hiányzó név figyelése
* hibás ár- és készletformátumok felismerése
* nem feloldható terméktípusok ellenőrzése
* ismeretlen kategória slugok ellenőrzése
* hiányzó vagy nem feloldható képfájlok ellenőrzése
* nem létező SKU-kra hivatkozó kategória-, kép- és tulajdonságsorok ellenőrzése

Ha a validáció hibát talál, az import nem indítható el, amíg a fájl javítása és az újraellenőrzés meg nem történik.

---

## Tömeges műveletek

### Tömeges árfrissítés

A tömeges árfrissítés:

* az összes termékre vagy egy kiválasztott kategóriára futtatható
* az alábbi módokat támogatja:
  * százalékos módosítás
  * fix összeg hozzáadása
  * új ár beállítása
* a Hotcakes termék `ListPrice` és `SitePrice` mezőit egyszerre módosítja

### Tömeges aktiválás / inaktiválás

A státuszmódosítás:

* adott kategóriára
* az összes termékre
* vagy a készlethiányos termékekre

is lefuttatható.

Az alkalmazás ilyenkor a Hotcakes terméknél:

* a `Status` mezőt módosítja
* az `IsAvailableForSale` mezőt is hozzáigazítja

A készlethiányos szűrés a készletrekordokat is figyelembe veszi, és a ténylegesen elérhető mennyiség alapján számol.

---

## Importelőzmények

Az alkalmazás minden lefuttatott import után helyi előzményt ment:

* futtatás dátuma
* import típusa
* forrásfájl
* létrehozott és frissített rekordok száma
* képek, kategóriák, tulajdonságok száma
* hibák száma
* részletes eredményüzenet

Az előzmények külön párbeszédablakban visszanézhetők.

---

## Konfiguráció

Az alkalmazás a Hotcakes kapcsolat adatait `settings.json` fájlból olvassa be.

Példa:

```json
{
  "Hotcakes": {
    "BaseUrl": "http://szerver-cim-vagy-domain",
    "ApiKey": "ide-jon-a-hotcakes-api-kulcs"
  }
}
```

### Beállítások betöltési sorrendje

1. `%LOCALAPPDATA%\AszAdmin1.0\settings.json`
2. az alkalmazás mappájában található `settings.json`

Ha egyik sem létezik, az alkalmazás megpróbálja a `settings.example.json` fájlt bemásolni a felhasználói profil alá, majd hibajelzéssel leáll, hogy a konfiguráció kitölthető legyen.

### Fontos

* a `BaseUrl` mezőbe a webáruház alap URL-je kerül
* az alkalmazás ehhez automatikusan hozzáfűzi a Hotcakes REST API útvonalát:
  `DesktopModules/Hotcakes/API/rest/v1/`
* az `ApiKey` a Hotcakes REST API kulcsa

---

## Telepítés

### Végfelhasználói telepítés

1. futtasd az `installer\AszAdmin-Setup.exe` telepítőt
2. töltsd ki a létrejövő `settings.json` fájlt
3. indítsd újra az alkalmazást

Az Inno Setup konfiguráció alapján:

* a telepítés alapértelmezés szerint a `Program Files\AszAdmin 1.0` mappába történik
* adminisztrátori jogosultság szükséges
* x64 kompatibilis Windows környezetre készül
* opcionálisan asztali parancsikon is létrehozható

### Telepítőcsomag továbbadása

A build script ZIP csomagot is készít, amelyben a setup EXE és a hozzá tartozó BIN fájlok együtt szerepelnek. Ezeket együtt kell továbbítani.

---

## Fejlesztői build

### Egyszerű build

```powershell
dotnet build .\WinFormsApp1\WinFormsApp1.csproj
```

### Publish

```powershell
dotnet publish .\WinFormsApp1\WinFormsApp1.csproj /p:PublishProfile=AszAdmin-win-x64
```

A publish profil jellemzői:

* `win-x64`
* `Release`
* self-contained
* single-file

A publish kimenet:

* `WinFormsApp1\bin\Release\net8.0-windows\publish\win-x64\AszAdmin.exe`

### Telepítő készítése

```powershell
powershell -ExecutionPolicy Bypass -File .\build-installer.ps1
```

Ehhez szükséges:

* .NET SDK
* Inno Setup 6

### Code signing

A telepítőscript támogatja a kódszignálást is. A részletek a `SIGNING.md` fájlban találhatók.

---

## Projektstruktúra

```text
ÁszAdmin1.0/
|- WinFormsApp1/
|  |- Program.cs
|  |- Form1.cs
|  |- HotcakesApiClient.cs
|  |- HotcakesModels.cs
|  |- AppSettings.cs
|  |- ImportHistoryStore.cs
|  |- ImportHistoryDialog.cs
|  |- settings.example.json
|  \- Properties/PublishProfiles/
|- installer/
|  \- AszAdmin1.iss
|- build-installer.ps1
|- SIGNING.md
|- logo.ico
\- ÁszAdmin1.0.slnx
```

---

## Fejlesztési tudnivalók

* a megoldás jelenleg egyetlen Windows Forms projektet tartalmaz
* a fő felhasználói logika nagyrészt a `Form1.cs` fájlban található
* a Hotcakes REST kommunikáció külön kliensosztályban van leválasztva
* az Excel sablongenerálás és a workbook beolvasás külső Excel függőség nélkül működik
* az importelőzmények nem adatbázisba, hanem helyi JSON fájlba kerülnek

### Jelenlegi funkcionális határ

A kódban láthatók előkészített UI elemek kategória-áthelyezéshez és tömeges törléshez is, de a jelenlegi állapot alapján az aktívan használt és bekötött tömeges műveletek:

* árfrissítés
* aktiválás / inaktiválás

---

## Fontos megjegyzések

* Az import indítása előtt mindig kötelező a validáció lefuttatása.
* A Hotcakes terméklista a validáció és a tömeges műveletek során memóriába töltődik.
* A képfeltöltés és a property-import részben az aktuális Hotcakes állapothoz igazodik, ezért API-hozzáférés nélkül ezek nem működnek.
* A konfiguráció és az importelőzmények felhasználónként, helyi gépen tárolódnak.

---

## Verzió

Az aktuális projektverzió:

* `1.6.0`

---

