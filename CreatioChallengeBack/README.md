# Creatio Challenge API

API .NET que expone un backend que consume Creatio OData usando OAuth2 (client credentials).

INSTRUCCIONES RÁPIDAS PARA USUARIO

1) Editá solo este archivo de configuración: `CreatioChallengeBack/appsettings.Development.json`.
   - Rellená la sección "Creatio" con los valores reales: ClientId, ClientSecret, TokenEndpoint y BaseUrl.
   - Ejemplo mínimo:

```json
{
  "Creatio": {
	"ClientId": "<tu-client-id>",
	"ClientSecret": "<tu-client-secret>",
	"TokenEndpoint": "https://tu-tenant.creatio.com/connect/token",
	"BaseUrl": "https://tu-tenant.creatio.com/0/odata/"
  }
}
```

2) Guardá el archivo, reiniciá la aplicación y probá los endpoints.

Nota de seguridad: no subas `ClientSecret` al repositorio.

CONTENIDO DE LA API

- GET /health
  - Devuelve 200 si la API está levantada.

- GET /api/Accounts?page={page}&pageSize={pageSize}&search={texto}
  - Lista cuentas con paginación y búsqueda por nombre.
  - Parámetros:
	- page: número de página (empieza en 1).
	- pageSize: elementos por página (por defecto 20).
	- search: texto opcional para filtrar por Name.
  - Respuesta (ejemplo):
	{
	  "page": 1,
	  "pageSize": 20,
	  "total": 123,
	  "items": [ { "id": "...", "name": "...", "code": "...", "typeName": "..." } ]
	}

- POST /api/Accounts
  - Crea una cuenta en Creatio.
  - Body JSON requerido: { "name": "...", "code": "...", "typeId": "<guid>", "phone": "...", "web": "..." }

TECNOLOGÍA DE AUTENTICACIÓN Y TOKEN

- La API usa OAuth2 Client Credentials para obtener un access token desde `TokenEndpoint`.
- El `ClientSecret` sólo debe existir en el backend; el frontend nunca lo recibe.
- El token se solicita en runtime y se cachea en memoria por el tiempo de expiración (o el valor configurado en `TokenCacheSeconds`).

Si necesitás más detalles técnicos (user-secrets, variables de entorno, o ver el token manualmente), pedímelo y te doy los comandos, pero para un usuario estándar sólo editá `appsettings.Development.json` y arrancá la app.

