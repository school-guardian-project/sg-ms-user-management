# sg-ms-user-management

`Person.Phone` se almacena como `BIGINT`. La entidad, el modelo y los DTO de
persona/familia usan `long` para conservar telefonos de diez digitos.
Si el contenedor ejecuta una imagen anterior con `int`, la consulta del perfil
puede responder 500 con `InvalidCastException`; reconstruir el servicio con
`docker compose up -d --build --no-deps ms-user-management.api`.