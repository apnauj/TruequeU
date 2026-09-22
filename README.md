# TruequeU

API REST para una plataforma de trueque entre estudiantes: publicar objetos que ya no se
usan, buscarlos por categoría, conversar con el dueño y cerrar el intercambio. Sin dinero
de por medio, y por lo tanto sin pasarela de pagos — lo que mueve la plataforma es la
conversación y la confianza, no la transacción.

Construida para el curso de Ingeniería Web en la **Universidad EIA**.
El cliente en React vive en **[TruequeU-Front](https://github.com/apnauj/TruequeU-Front)**.

**Stack** · .NET 10 · ASP.NET Core · Entity Framework Core · SQL Server · ASP.NET Identity
· JWT · Scalar

---

## Cómo correrlo

Necesitas el SDK de .NET 10 y una instancia de SQL Server.

```bash
git clone https://github.com/apnauj/TruequeU.git
cd TruequeU

cp appsettings.Example.json appsettings.Development.json   # y editar la cadena de conexión
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"

dotnet ef database update
dotnet run
```

La documentación interactiva queda en `/scalar/v1` cuando corres en desarrollo.
`DbInitializer` siembra los roles y unos datos de ejemplo en el primer arranque.

---

## Arquitectura

Cuatro capas, cada una con una sola responsabilidad:

```
Controllers/   Traducen HTTP a llamadas de dominio. No contienen reglas de negocio.
Services/      Las reglas: quién puede editar qué, cuándo un anuncio cambia de estado.
Persistence/   ApplicationDbContext y el sembrado inicial.
Models/        Entidades y DTOs. Los DTOs existen para no exponer las entidades tal cual.
```

Los servicios se registran contra sus interfaces (`Interfaces/`), así que los controladores
dependen de la abstracción y no de la implementación.

### Autenticación

ASP.NET Identity con claves `Guid`, y un JWT que **no viaja en el header `Authorization`
sino en una cookie `auth_token`**. El evento `OnMessageReceived` del handler es el que lo
saca de ahí:

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        context.Token = context.Request.Cookies["auth_token"];
        return Task.CompletedTask;
    }
};
```

La razón es que una cookie `HttpOnly` no es legible desde JavaScript, lo que cierra la vía
más común de robo de token por XSS. El costo es que hay que habilitar CORS con
`AllowCredentials` y pensar en CSRF, que es el siguiente pendiente de la lista.

Dos roles, sembrados al arrancar: `User` y `Admin`.
`ClockSkew = TimeSpan.Zero` para que un token expirado lo sea de verdad y no cinco minutos
después, que es el valor por defecto.

---

## Dominio

| Recurso | Qué hace |
|---|---|
| **Users** | Registro, login, logout, perfil, recuperación de contraseña |
| **Listings** | Publicaciones con imágenes, categoría, condición y estado |
| **Conversations / Messages** | Chat por publicación, con marcado de leídos |
| **Favorites** | Guardar publicaciones de interés |
| **Reports** | Denuncias de usuarios sobre publicaciones |
| **Moderation** | Acciones de admin: ocultar publicaciones, suspender usuarios |

El ciclo de vida de una publicación está en un enum, no en cadenas sueltas:
`Available → Reserved → Sold`, más `Hidden` por moderación. Cada transición es su propio
endpoint (`PATCH /listings/{id}/reserved`, `/sold`, `/available`), de modo que el estado
no se puede poner en cualquier valor desde un `PUT` genérico.

Las listas devuelven `PagedResult<T>`, no arreglos sueltos.

---

## Decisiones

- **Enums en vez de cadenas** para categoría, condición y estado. El compilador atrapa los
  valores inválidos antes de que lleguen a la base de datos.
- **Un endpoint por transición de estado** en vez de un campo editable. Hace explícito qué
  cambios son válidos.
- **Servicios detrás de interfaces.** Cuesta poco al escribir y es lo que permite sustituir
  la implementación en pruebas.

## Limitaciones conocidas

- **La cobertura de pruebas es mínima.** `TruequeU.Tests` solo cubre los enums; la lógica
  de los servicios se verificó a mano. Es lo primero que ampliaría.
- No hay protección CSRF, que es la contraparte necesaria de autenticar por cookie.
- Las imágenes se guardan como registros en base de datos, no en almacenamiento de
  objetos; no escala más allá de un proyecto de curso.
- No hay límite de peticiones ni registro estructurado.
