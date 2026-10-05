SubastaYa



Plataforma web de subastas en tiempo real y comercio electrónico desarrollada para la cátedra Proyecto de Software – UNAJ.



Tecnologías

C#

ASP.NET Core Web API

.NET 10

Entity Framework Core

SQL Server Express

HTML5

CSS3

JavaScript

Fetch API

SignalR

Swagger / OpenAPI

Git / GitHub

Estructura del proyecto

SubastaYa.Api/

├── frontend/

│   ├── index.html

│   ├── crear-subasta.html

│   ├── subasta.html

│   ├── billetera.html

│   ├── usuario.html

│   ├── css/

│   └── js/

│

├── SubastaYa.Api/

│   ├── Controllers/

│   ├── Data/

│   ├── Models/

│   ├── Services/

│   ├── Hubs/

│   ├── Migrations/

│   └── Program.cs

│

├── SubastaYa.Api.Tests/

└── SubastaYa.Api.slnx

Requisitos



Para ejecutar el proyecto se necesita:



Windows

.NET SDK 10

SQL Server Express

Visual Studio 2022/2026 o terminal con .NET SDK

Git

Base de datos



El proyecto utiliza SQL Server Express mediante Entity Framework Core.



Configuración utilizada:



Servidor: .\\SQLEXPRESS

Base de datos: SubastaYaDb

Autenticación: Windows



La conexión se encuentra configurada en appsettings.json.



Crear o actualizar la base de datos



Desde la carpeta raíz del proyecto:



dotnet ef database update --project SubastaYa.Api\\SubastaYa.Api.csproj



Las tablas se generan mediante migraciones de Entity Framework Core utilizando el enfoque Code-First.



Ejecutar el backend



Desde la carpeta raíz:



dotnet run --project SubastaYa.Api\\SubastaYa.Api.csproj



La API queda disponible en:



http://localhost:5013



Swagger:



http://localhost:5013/swagger

Ejecutar el frontend



El frontend se encuentra en la carpeta:



frontend



Se puede abrir el archivo:



frontend\\index.html



Por ejemplo, desde CMD:



start .\\frontend\\index.html



El frontend utiliza JavaScript y Fetch API para comunicarse con el backend mediante los endpoints REST.



Funcionalidades principales

Catálogo de subastas



Permite:



Visualizar subastas.

Filtrar por estado.

Filtrar por categoría.

Filtrar por precio.

Ordenar resultados.

Visualizar título, imagen, categoría y precio de la puja.

Visualizar cantidad de pujas.

Visualizar cuenta regresiva de la subasta.

Creación de subastas



El vendedor puede crear publicaciones indicando:



Título.

Descripción.

Imagen.

Categoría.

Precio base.

Incremento mínimo.

Fecha y hora de inicio.

Fecha y hora de finalización.



El frontend realiza validaciones antes de enviar la información al backend.



Sala de pujas



La sala de pujas permite:



Visualizar el tiempo restante.

Realizar nuevas pujas.

Visualizar el historial de pujas.

Mostrar el estado del usuario como líder o superado.

Sugerir la próxima puja.

Realizar una puja personalizada.

Mostrar alertas cuando la subasta se encuentra próxima a finalizar.



La actualización en tiempo real se implementa mediante SignalR.



Hub utilizado:



/hubs/subastas



También existe actualización periódica como mecanismo complementario.



Billetera



La billetera permite visualizar:



Saldo total.

Saldo retenido.

Saldo disponible.

Movimientos.

Depósitos simulados.



El saldo disponible se calcula como:



saldo\_disponible = saldo\_total - saldo\_retenido

Reglas de negocio

Escrow



Cuando un usuario realiza una puja válida:



Se retiene el importe correspondiente al líder.

Si otro usuario supera la puja, se libera la retención anterior.

Se retiene el nuevo importe para el nuevo líder.

Las operaciones se realizan dentro de una transacción para mantener la consistencia de los fondos.

Anti-sniping



Si se recibe una puja válida durante los últimos 60 segundos de una subasta, la fecha de finalización se extiende automáticamente 2 minutos.



La extensión queda registrada en el sistema de auditoría.



Cierre automático



El sistema cuenta con un proceso en segundo plano que controla las subastas vencidas.



Si existe un ganador:



FINALIZADA



Los fondos retenidos se transfieren al vendedor y se registra la operación correspondiente.



Si no existen pujas:



DESIERTA

Concurrencia optimista



Las subastas utilizan un campo de versión para implementar control de concurrencia optimista.



Ante dos pujas simultáneas sobre la misma subasta:



Ambas solicitudes leen el mismo estado.

La primera solicitud que logra actualizar la versión es aceptada.

La segunda detecta que la versión cambió.

La segunda operación es rechazada con HTTP 409 Conflict.



Esto evita que dos pujas concurrentes sean aceptadas incorrectamente sobre el mismo estado.



La respuesta de conflicto utiliza el formato de errores definido por el proyecto:



\[CODE-ERROR] -

Auditoría



El sistema registra eventos importantes, entre ellos:



Cambios de estado de las subastas.

Extensiones por anti-sniping.

Operaciones rechazadas por concurrencia.

Validaciones de negocio rechazadas.

Depósitos manuales.

Operaciones relacionadas con las pujas.

API REST



Los recursos principales utilizan rutas REST plurales:



/api/v1/categorias

/api/v1/usuarios

/api/v1/billeteras

/api/v1/subastas

/api/v1/pujas

/api/v1/auditorias

/api/v1/transacciones-ledgers



Se utilizan códigos HTTP según el resultado de cada operación, incluyendo:



200 OK

201 Created

204 No Content

400 Bad Request

404 Not Found

409 Conflict



La API utiliza la versión:



X-Api-version: 1.0

Arquitectura



El backend utiliza una separación de responsabilidades entre:



Controllers

Services

Data

Models

Hubs



Los Controllers reciben las solicitudes HTTP y delegan la lógica de negocio a los Services.



Entity Framework Core se utiliza para el acceso a datos y las migraciones.



SignalR se utiliza para la comunicación en tiempo real de las pujas.



Datos iniciales



El sistema incluye datos iniciales para poder probar las funcionalidades principales.



Usuarios de prueba:



vendedor@test.com

comprador1@test.com

comprador2@test.com

sinfondos@test.com



También se incluyen categorías y subastas de prueba para facilitar la demostración del sistema.



Ejecución rápida

Iniciar SQL Server Express.

Abrir una terminal en la carpeta raíz del proyecto.

Actualizar la base de datos:

dotnet ef database update --project SubastaYa.Api\\SubastaYa.Api.csproj

Ejecutar el backend:

dotnet run --project SubastaYa.Api\\SubastaYa.Api.csproj

Abrir Swagger:

http://localhost:5013/swagger

Abrir el frontend:

start .\\frontend\\index.html

