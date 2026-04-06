# DNN Shopped Together Module

## Projekt leírás

A **Shopped Together** modul egy egyedi fejlesztésű DNN (DotNetNuke) kiterjesztés, amely a webáruház kosarában lévő termékek alapján automatikusan ajánl további, gyakran együtt vásárolt termékeket.

A modul célja a kosárérték növelése és a felhasználói élmény javítása ajánlások segítségével.

---

## Fő funkciók

* Gyakran együtt vásárolt termékek felismerése korábbi rendelések alapján
* Top 3 vagy Top 5 ajánlás kiválasztása súlyozott gyakoriság alapján
* Ajánlott termékek megjelenítése a kosár oldalon
* Napi szintű adataggregáció a teljesítmény optimalizálásához

---

## Technológia

* DNN (DotNetNuke) MVC modul
* ASP.NET Framework 4.7.2
* MSSQL adatbázis
* DAL2 ORM réteg
* JavaScript / jQuery frontend

---

## Működési elv

A rendszer a korábbi rendelésekből meghatározza, hogy mely termékeket vásárolták gyakran együtt.

Egy adott kosár esetén:

1. Lekéri a kosárban lévő termékeket
2. Összegzi az ezekhez tartozó együttvásárlási értékeket
3. Kiválasztja a leggyakoribb kapcsolódó termékeket
4. Megjeleníti az ajánlásokat

---

## Telepítés

1. Build (Release módban)
2. Az `install` mappában létrejött ZIP csomag használata
3. DNN admin felületen:
   * Extensions → Install Extension
4. Modul elhelyezése kosár oldalon

---

## Fejlesztési tudnivalók

* A projekt a DNN `DesktopModules` könyvtárán belül található
* A modul MVC alapú
* REST API végpontok külön controllerben vannak megvalósítva

### Fontos

* `.dnn` fájl módosításakor újratelepítés szükséges
* adatbázis struktúra módosításakor újratelepítés szükséges
* C# / view / CSS / JS módosítás esetén elég az újrafordítás

---

## Verziókezelés

A projekt Git verziókezelést használ.

---

## Fejlesztő

* Név: *Vitálos Tünde Eszter - Háztartászok*
* Projekt: Budapesti Corvinus Egyetem - Rendszerfejlesztés és IT architektúra tárgy feladata

---

## Licenc

Ez a projekt oktatási célból készült.
