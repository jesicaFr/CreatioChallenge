# Configuración local y despliegue a Git

Pasos para ejecutar el proyecto localmente sin subir credenciales al repositorio.

1) Copiar archivo de ejemplo:

   cp .env.example .env

2) Editar `.env` y completar CLIENT_ID y CLIENT_SECRET. NO subir `.env` al repo.

3) Asegurarse de que `.env` está ignorado por Git (se añadió .gitignore con la entrada `.env`).

4) Comandos Git (ejemplo):

   git add -A
   git commit -m "Configuración: añadir servicio de token y cliente Creatio; añadir .env.example"
   git push origin main

   Nota: Antes de ejecutar git add, confirmá que `.env` no está en el listado de archivos a commitear con `git status`.

5) Ejecutar la app:

   dotnet restore
   dotnet build
   dotnet run

6) Alternativa: usar `dotnet user-secrets` para guardar las credenciales en tu máquina de desarrollo (recomendado si no querés usar .env):

   dotnet user-secrets init
   dotnet user-secrets set "CLIENT_ID" "tu-client-id"
   dotnet user-secrets set "CLIENT_SECRET" "tu-client-secret"

7) Seguridad: nunca subas claves al repositorio público. Si accidentalmente subiste secretos, rotalos inmediatamente en la plataforma que los emitió.
