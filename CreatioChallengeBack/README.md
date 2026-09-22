# CreatioChallengeBack

API .NET que actúa como proxy seguro entre un frontend (Angular) y Creatio OData usando OAuth2 Client Credentials.

Resumen
- Arquitectura: Angular → .NET Web API → OAuth2 Client Credentials → Creatio OData
- Endpoints principales:
  - GET /api/accounts?search={q}&page={n}&pageSize={m}  (paginado server-side, select/expand)
  - POST /api/accounts  (crear Account — validar y mapear antes de enviar a Creatio)
  - GET /health
  - Swagger (Development): /swagger

Requisitos previos
- .NET 10 SDK
- Credenciales de OAuth2 para Creatio (ClientId y ClientSecret)

Configuración recomendada (local, segura)
1) Usar dotnet user-secrets (recomendado para desarrollo):
   cd CreatioChallengeBack
   dotnet user-secrets init --project "CreatioChallengeBack.csproj"
   dotnet user-secrets set "Creatio:ClientSecret" "<TU_CLIENT_SECRET>" --project "CreatioChallengeBack.csproj"
   dotnet user-secrets set "Creatio:ClientId" "<TU_CLIENT_ID>" --project "CreatioChallengeBack.csproj"
   dotnet user-secrets set "Creatio:TokenEndpoint" "https://.../connect/token" --project "CreatioChallengeBack.csproj"

2) Alternativa: variables de entorno (PowerShell, sesión actual):
   $env:CREATIO_CLIENT_SECRET = '<TU_CLIENT_SECRET>'
   $env:Creatio__ClientId = '<TU_CLIENT_ID>'
   $env:Creatio__TokenEndpoint = 'https://.../connect/token'
   dotnet run --project .\CreatioChallengeBack\CreatioChallengeBack.csproj

3) Para depuración en Visual Studio: editar Properties/launchSettings.json y añadir en el perfil:
   "environmentVariables": {
	 "CREATIO_CLIENT_SECRET": "<TU_CLIENT_SECRET>",
	 "Creatio__ClientId": "<TU_CLIENT_ID>",
	 "Creatio__TokenEndpoint": "https://.../connect/token"
   }

4) .env (opcional): copiar .env.example → .env en tu máquina local. Por defecto .NET no carga .env; si quieres usarlo, carga las variables en tu sesión o añadimos soporte con DotNetEnv.

Ejecución
- Desde la terminal (PowerShell) con las variables definidas o user-secrets configurados:
  dotnet run --project .\CreatioChallengeBack\CreatioChallengeBack.csproj
- Abrir: http://localhost:<puerto>/swagger (Development) o probar directamente:
  GET http://localhost:<puerto>/api/accounts?search=Our&page=1&pageSize=5

Qué debe hacer quien descargue el proyecto
1. Clonar el repo.
2. Proveer secretos localmente (user-secrets o variables de entorno) antes de ejecutar.
3. Ejecutar dotnet run desde la carpeta raíz o usar Visual Studio.

Eliminar secretos antes de commitear
- Nunca incluir secretos en archivos versionados. Pasos para limpiar:
  - Si usaste user-secrets: dotnet user-secrets remove "Creatio:ClientSecret" --project "CreatioChallengeBack\CreatioChallengeBack.csproj"
  - Si seteaste variables de entorno temporales, ciérralas (cierrar sesión) o eliminarlas con setx con valor vacío.

Errores comunes y significado
- InvalidOperationException: "Client secret no configurado" → no detectó secret (usar user-secrets o variable de entorno).
- 401/403 desde Creatio → ClientId/ClientSecret incorrectos o permisos.
- 429 → rate limit de Creatio (recomiendo añadir retries con backoff si ocurre frecuentemente).
- 5xx → error en Creatio; revisar body devuelto y logs del backend.

Buenas prácticas
- Mantener .env.example en el repo como plantilla.
- Usar user-secrets para desarrollo y variables de entorno en CI/CD.
- No exponer tokens ni secretos al frontend.

Contacto y ayuda
- Si necesitás que agregue scripts para inicializar secrets o que implemente creación POST a Creatio, decímelo y lo agrego.

