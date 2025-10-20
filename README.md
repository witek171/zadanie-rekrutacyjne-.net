# ToDo API - Instrukcja uruchomienia

Ten projekt uruchamia prostą aplikację ToDo w ASP.NET Core z bazą PostgreSQL przy użyciu Docker Compose.

---

## Wymagania

- <a href="https://www.docker.com/get-started" target="_blank" rel="noopener noreferrer">Docker</a>
- <a href="https://docs.docker.com/compose/install/" target="_blank" rel="noopener noreferrer">Docker Compose</a>

Upewnij się, że porty **5290** i **5432** są wolne, aby kontenery mogły się uruchomić.

---

## Krok 1: Sklonuj repozytorium

```bash
git clone https://github.com/witek171/zadanie-rekrutacyjne-.net.git
cd <KATALOG_PROJEKTU>
```

---

## Krok 2: Uruchom Docker Compose

W katalogu projektu, w którym znajduje się plik `docker-compose.yml`, wykonaj:

```bash
docker-compose up --build
```

---

## Krok 3: Sprawdź działanie aplikacji

API będzie dostępne razem z dokumentacją Swagger pod adresem: <a href="http://localhost:5290/swagger" target="_blank" rel="noopener noreferrer">http://localhost:5290/swagger</a>

---

## Krok 4: Wyłączenie i czyszczenie kontenerów

Aby zatrzymać kontenery i usunąć wolumen z danymi (reset bazy), w katalogu, w którym znajduje się plik `docker-compose.yml` wykonaj:

```bash
docker-compose down -v
```

