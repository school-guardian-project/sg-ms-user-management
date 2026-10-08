#!/usr/bin/env bash
# Sincroniza los .proto que ms-user-management consume desde ms-school-management.
#
# El .proto es el contrato entre servicios, pero cada servicio corre su propio
# generador: Java (ms-iam) y .NET (aqui) no comparten el artefacto generado.
#
# A diferencia del sync de ms-iam, esta copia reescribe `option csharp_namespace`
# para que las clases cliente no vivan en el namespace del servidor
# (ms_school_management.Api.*) sino en el de quien las consume. Es la unica linea
# que cambia: servicio, mensajes, campos y numeros de campo son identicos, asi que
# ambos lados siguen hablando exactamente el mismo protocolo.
#
# Uso: ./sync-protos.sh   (desde backend/ms-user-management)
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOURCE="$HERE/../ms-school-management/src/ms-school-management.Api/Protos"
TARGET="$HERE/src/ms-user-management.Api/Protos"

mkdir -p "$TARGET"
for proto in "$SOURCE"/*.proto; do
  name="$(basename "$proto")"
  out="$TARGET/$name"
  sed 's|^option csharp_namespace = .*|option csharp_namespace = "ms_user_management.Api.Infrastructure.Grpc.External";|' \
    "$proto" > "$out"
  echo "sincronizado: $name -> $out"
done