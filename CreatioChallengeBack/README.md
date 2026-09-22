# Creatio Challenge API

API .NET que conecta el frontend Angular con Creatio OData usando OAuth 2.0 Client Credentials.

## Requisitos

- .NET 10 SDK.
- Un cliente OAuth de Creatio con `Client ID` y `Client Secret`.
- El repositorio del frontend, si también se quiere probar la interfaz.

## Configuración segura

Las credenciales no se escriben en `appsettings.json`, en Angular ni en el README.

1. Ubicate en la raíz del repositorio, donde están `.gitignore` y `CreatioChallengeBack.slnx`.
2. Copiá el archivo de ejemplo:

```powershell
Copy-Item .\CreatioChallengeBack\.env.example .\.env
```

3. Abrí `.env` y completá los valores reales:

```env
CLIENT_ID=tu_client_id
CLIENT_SECRET=tu_client_secret
TOKEN_URL=https://tu-tenant.creatio.com/connect/token
BASE_URL=https://tu-tenant.creatio.com/0/odata/
```

El archivo `.env` está excluido por Git. No lo subas ni lo envíes dentro del repositorio.

## Ejecutar la API

Desde la raíz del repositorio:

```powershell
dotnet restore .\CreatioChallengeBack\CreatioChallengeBack.csproj
dotnet run --project .\CreatioChallengeBack\CreatioChallengeBack.csproj
```

La aplicación mostrará en la consola la URL local donde quedó disponible.

## Verificar que funciona

Primero comprobá el estado de la API usando la URL que mostró la consola:

```text
GET /health
```

Después probá el listado:

```text
GET /api/Accounts?page=1&pageSize=10&search=
```

La primera consulta a cuentas solicita el token a Creatio. Si las credenciales o las URLs son incorrectas, la API informa un error de autenticación o conexión.

## Endpoints principales

- `GET /health`: verifica que la API esté levantada.
- `GET /api/Accounts`: lista cuentas con búsqueda y paginación.
- `POST /api/Accounts`: crea una cuenta en Creatio.

El frontend nunca recibe el `Client Secret`; solo se comunica con esta API.

## Seguridad

- No guardar secretos en archivos versionados.
- No enviar credenciales en el JSON del frontend.
- Para CI/CD usar variables protegidas o un gestor de secretos.
- Si un secreto fue compartido o publicado, revocarlo y generar uno nuevo.
