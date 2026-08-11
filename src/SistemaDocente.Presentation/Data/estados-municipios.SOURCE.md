# Geographic catalog provenance

This file records the source, transformation and update process for
`estados-municipios.json`.

## Source

The catalog is an offline AulaRaíz projection of the official **INEGI Catálogo
Único de Claves de Áreas Geoestadísticas Estatales, Municipales y Localidades**.

- Catalog: <https://www.inegi.org.mx/app/ageeml/>
- Service documentation: <https://www.inegi.org.mx/servicios/catalogounico.html>
- State endpoint: `https://gaia.inegi.org.mx/wscatgeo/v2/mgee/`
- Municipal-area endpoint by state:
  `https://gaia.inegi.org.mx/wscatgeo/v2/mgem/{cve_ent}`
- Terms: <https://www.inegi.org.mx/inegi/terminos.html>
- Reference classification: INEGI classification updated through 2025-06-17.
- Provenance review: 2026-08-11.

Required attribution:

> Fuente: INEGI, Catálogo Único de Claves de Áreas Geoestadísticas Estatales,
> Municipales y Localidades, clasificación actualizada al 17 de junio de 2025.

INEGI permits copying, publishing, adapting, reordering, extracting and
commercial use of its information subject to its terms, including attribution,
preserving metadata and identifying transformations. INEGI does not endorse
AulaRaíz.

## AulaRaíz transformation

The project-created JSON:

1. retains only official state names and municipality/Mexico City territorial
   demarcation names;
2. groups municipal names under the corresponding state name;
3. orders state keys by official state key and municipal values alphabetically
   for the offline selector;
4. omits INEGI geographic keys, capital names, population fields, geometry and
   all other attributes;
5. stores no personal information and performs no network request at runtime.

The transformation belongs to AulaRaíz and must not be presented as an analysis
or transformation performed by INEGI.

## Reviewed snapshot

The current file contains:

- 32 entities;
- 2,478 municipalities or Mexico City territorial demarcations;
- current 2025 classification names including `Playa del Carmen`,
  `Villa de Pozos`, `Eldorado` and `Juan José Ríos`.

The total agrees with INEGI's 2025 classification and current national
geographic-area count. This validation establishes the reference snapshot; it
does not make the list permanently current.

## Update procedure

A catalog update must:

1. retrieve all 32 state records and their municipal-area records from the
   documented INEGI service;
2. preserve the official UTF-8 names exactly;
3. apply only the transformation described above;
4. verify there are 32 unique state keys and no duplicate municipal names
   inside a state;
5. compare the resulting national and per-state counts with the INEGI source;
6. review renames, additions and removals explicitly;
7. update the reference date, reviewed snapshot and attribution when the source
   changes;
8. run the complete test and installer workflows before merge.

Do not silently combine this catalog with an unattributed third-party list.
