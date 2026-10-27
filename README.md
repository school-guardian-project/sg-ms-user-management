# sg-ms-user-management

`Person.Phone` se almacena como `BIGINT`. La entidad, el modelo y los DTO de
persona/familia usan `long` para conservar telefonos de diez digitos.
Si el contenedor ejecuta una imagen anterior con `int`, la consulta del perfil
puede responder 500 con `InvalidCastException`; reconstruir el servicio con
`docker compose up -d --build --no-deps ms-user-management.api`.

## Escuela y sede de los usuarios

Los POST de estudiantes, conductores y acudientes requieren `campusId`, que debe
identificar una sede activa de una escuela activa. Si falta o no existe, devuelven
400 antes de guardar la persona. Sus eventos `student.created`, `driver.created` y
`parent.created` incluyen `CampusId`; IAM persiste la relacion en
`Iam.Profile.CampuseId`. La escuela se obtiene desde `SchoolCampus.SchoolId`.

Los administradores siguen enviando `schoolId`: IAM mantiene la relacion existente
`School.SchoolAdmin`, sin reemplazarla por una sede. Los PUT de usuarios normales
conservan su sede; este contrato no implementa traslados entre sedes.

El DTO compartido de actualizacion incluye `cityId` y `schoolId` para el PUT
de administradores; el controlador exige ambos y publica la reasignacion de escuela.

## Retry-safe record deletion

Person deletion (administrators, parents, students and drivers) logically
deactivates the person and their IAM profiles and removes their family
memberships in one transaction. Family deletion removes the family and its
memberships atomically. Both transactions run inside EF Core's execution
strategy, as required by SQL Server's `EnableRetryOnFailure`. Set-based writes
allow a rolled-back attempt to be replayed without stale tracked entity states.
Existing authorization and school-scoping rules are unchanged.

Regression tests use an in-memory SQLite database with SQL Server's retry
strategy to check successful deletion, rollback, transient retries, retry
exhaustion and isolation of unrelated records. They do not contact production
SQL Server or send events:

```powershell
dotnet test .\tests\ms-user-management.Tests\ms-user-management.Tests.csproj --filter FullyQualifiedName~RecordDeletionPersistenceTests
```

Rebuild and deploy `ms-user-management` after merging the hotfix. A successful
authorized DELETE responds with `204 No Content`; a previously deployed image
will continue to exhibit the old transaction failure.