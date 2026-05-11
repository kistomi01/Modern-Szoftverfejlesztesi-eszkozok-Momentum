# Momentum 2D Platformer

## Projekt Leírása
Ez egy Unity motorral készült 2D platformer játék. 
A projekt célja a stabil technikai alapok bemutatása: mozgás, fizika, sebződés-mechanika és a pályák közötti átmenet.

## Futtatás és Build
A projekt futtatható verziója (.exe) a GitHub **Releases** menüpontja alatt érhető el a `release` tag-nél, egy tömörített `.zip` fájlban.
- **Platform:** Windows
- **Futtatás:** A `.zip` kicsomagolása után a `.exe` fájllal indítható.

## Tesztelés
A projekt legalább 3 darab automata tesztesetet tartalmaz, amelyek a Unity **Test Runner** (Play Mode) segítségével futtathatóak:
1. **PlayerHealthTest**: Ellenőrzi, hogy a `HealthManager.HurtPlayer()` hívásakor az életerő csökken-e, és nulla pontnál triggereli-e a halált.
2. **CoinPickupTest**: Igazolja, hogy az érmék felvételekor a `GameManager` számlálója megfelelően növekszik.
3. **RespawnLogicTest**: Teszteli, hogy a játékos a legutolsó érvényes talajpozíción éled-e újra sebződés után, megállított fizika mellett.

**Futtatás menete:** `Window > General > Test Runner` -> `Play Mode` fül -> `Run All`.

## Pályák és Adatkezelés
A játék manuálisan felépített szinteket használ, amelyek Tilemap alapúak.
- **Pályák helye:** `Assets/Scenes/`
- **Build sorrend:** A `File > Build Settings` ablakban konfigurálva.
- **Adattárolás:** A játékos aktuális életerejét és a gyűjtött érméket a `HealthManager` és a `GameManager` kezelik futásidőben.

## Felhasznált Külső Assetek:
- **Kamera:** Cinemachine (Virtual Camera & Confiner 2D)
- **Basic Unity 2d Assets** Unity beépített dolgai

## Verziókezelés
A leadott állapot a `v1.0` tag-gel lett megjelölve.
