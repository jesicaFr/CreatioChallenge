# Creatio Challenge API

API REST desarrollada con **.NET** que expone un backend para consumir **Creatio OData API** utilizando autenticación **OAuth 2.0 – Client Credentials**.

## 🚀 Inicio rápido

### 1. Configuración

La API utiliza las credenciales de Creatio para autenticarse mediante OAuth 2.0.

Editá únicamente:

```text
CreatioChallengeBack/appsettings.Development.json
```

Completá la sección `Creatio` con los valores correspondientes a tu instancia:

```json
{
  "Creatio": {
    "ClientId": "",
    "ClientSecret": "",
    "TokenEndpoint": "https://tu-tenant.creatio.com/connect/token",
    "BaseUrl": "https://tu-tenant.creatio.com/0/odata/"
  }
}
```

> **Importante:** no subir credenciales reales al repositorio. El `ClientSecret` debe mantenerse únicamente en el entorno del backend.

### 2. Ejecutar la API

Desde la carpeta del proyecto:

```bash
dotnet restore
dotnet run
```

La URL de ejecución dependerá de la configuración de `launchSettings.json`.

---

## 📡 Endpoints

### Health Check

```http
GET /health
```

Permite verificar que la API se encuentra funcionando correctamente.

**Respuesta:**

```http
200 OK
```

---

### Obtener cuentas

```http
GET /api/Accounts
```

Obtiene las cuentas desde Creatio, permitiendo paginación y búsqueda por nombre.

#### Parámetros

| Parámetro  | Tipo   | Descripción                            | Default |
| ---------- | ------ | -------------------------------------- | ------- |
| `page`     | int    | Número de página. Comienza en 1        | `1`     |
| `pageSize` | int    | Cantidad de elementos por página       | `20`    |
| `search`   | string | Texto opcional para filtrar por nombre | —       |

#### Ejemplo

```http
GET /api/Accounts?page=1&pageSize=20&search=Acme
```

#### Respuesta

```json
{
  "page": 1,
  "pageSize": 20,
  "total": 123,
  "items": [
    {
      "id": "...",
      "name": "Acme",
      "code": "...",
      "typeName": "..."
    }
  ]
}
```

---

### Crear una cuenta

```http
POST /api/Accounts
```

Crea una nueva cuenta en Creatio.

#### Request Body

```json
{
  "name": "Acme",
  "code": "ACM001",
  "typeId": "",
  "phone": "+54 11 1234-5678",
  "web": "https://example.com"
}
```

---

## 🔐 Autenticación

La integración con Creatio utiliza **OAuth 2.0 Client Credentials**.

El flujo es:

1. El backend solicita un `access_token` al `TokenEndpoint`.
2. Creatio valida las credenciales configuradas.
3. La API obtiene el token.
4. El token se utiliza para realizar las llamadas a OData.
5. El token se almacena temporalmente en memoria para evitar solicitarlo en cada request.
6. Una vez expirado, se solicita automáticamente un nuevo token.

El tiempo de cache puede utilizar el valor configurado mediante `TokenCacheSeconds`.

### Seguridad

El `ClientSecret` se utiliza exclusivamente en el backend y **nunca se expone al frontend**.

Para entornos reales se recomienda utilizar variables de entorno, secretos administrados o un servicio de gestión de secretos en lugar de almacenar credenciales directamente en archivos de configuración.

---

## 🏗️ Tecnologías

* **.NET**
* **ASP.NET Core Web API**
* **C#**
* **Creatio OData**
* **OAuth 2.0 Client Credentials**
* **HTTP / REST**
* **JSON**

---

## 📁 Estructura general

```text
CreatioChallengeBack/
├── Controllers/
├── Services/
├── Models/
├── Configuration/
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

---

## 🧪 Funcionalidades implementadas

* Health check de la API.
* Autenticación OAuth 2.0 mediante Client Credentials.
* Obtención y cache de `access_token`.
* Integración con Creatio OData.
* Consulta de cuentas.
* Paginación.
* Búsqueda de cuentas por nombre.
* Creación de cuentas.
* Separación de responsabilidades entre controllers, servicios y configuración.

---

## ⚠️ Credenciales

Las credenciales utilizadas para la instancia de Creatio son específicas del entorno de prueba.

**No incluir `ClientSecret` ni otros secretos reales en el repositorio.**

