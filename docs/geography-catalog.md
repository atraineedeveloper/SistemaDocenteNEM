# Offline Mexico geography catalog

Sistema Docente Local uses an offline entity/municipality catalog so group configuration remains usable without an Internet connection.

## Source of truth

The authoritative reference is INEGI's **Catálogo Único de Claves de Áreas Geoestadísticas Estatales, Municipales y Localidades**. The packaged application snapshot contains the 32 federal entities and their municipality/alcaldía display names.

The current packaged JSON is stored at:

```text
src/SistemaDocente.Presentation/Data/estados-municipios.json
```

`CatalogoGeograficoMexico` loads it as an embedded resource. Locality is intentionally not packaged; it remains a normalized free-text field in this version.

## Product behavior

- Entity is selected from the 32-item catalog.
- Municipality/alcaldía choices are filtered by the selected entity.
- The application works completely offline after installation.
- Existing legacy values that do not match the packaged catalog are not silently guessed; the user must select a valid catalog value when editing the configuration.

## Maintenance

Before a release that refreshes geographic data:

1. compare the packaged snapshot against the current INEGI catalog;
2. review municipality creations, renames and boundary/catalog changes;
3. preserve UTF-8 names and accents;
4. verify all 32 entity keys are present;
5. run geography-catalog tests, including Tabasco and at least one high-cardinality entity;
6. document the snapshot date in the pull request.

Display names are not intended to replace official INEGI geographic keys in future interoperability work. Import/export may introduce explicit official keys in a later schema extension where duplicate municipality names require unambiguous external identifiers.