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