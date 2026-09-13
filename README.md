# Orders

Solución a la prueba técnica: una tienda que recibe ordenes/pedidos, reserva
inventario de forma asíncrona y expone un panel web para operaciones.

## Arquitectura
Se crean dos servicios en el backend, una API Orders.Api y un worker Inventory.Worker 
que se comunican entre si mediante un servicio de colas con RabbitMQ y almacena su informacion
en dos bases de datos independientes para cada servicio usando PostgreSQL (orders, inventory) y 
se crea un servicio web utilizando react con vite para su creacion y en su construccion como 
contenedor se implementa nginx que funciona como proxy

```
                 POST /orders                 GET /orders (polling)
   React SPA  ───────────────────►  Orders.Api  ◄─────────────────── React SPA
  (nginx :3000)                     (ASP.NET, :8080)
                                       │      ▲
                               publica │      │ consume
                              OrderCreated  stock-result
                                       │      │
                                       ▼      │
                                    RabbitMQ (colas durables)
                                       │      ▲
                               consume │      │ publica
                              OrderCreated  StockReserved/Rejected
                                       ▼      │
                                 Inventory.Worker (.NET Worker Service)
                                       │
                                       ▼
                              Postgres: orders / inventory (bases separadas)
```

- **Orders.Api**: valida y persiste el pedido como `Pending`, publica
  `OrderCreated` y corre un `BackgroundService` que escucha `stock-result`
  para actualizar el pedido a `Confirmed`/`Rejected`.

- **Inventory.Worker**: consume `OrderCreated`, intenta descontar stock de
  forma transaccional e idempotente, y publica `StockReserved` o
  `StockRejected`.

- Cada servicio tiene **su propia base de datos** (`orders`, `inventory`)
  para no acoplar los despliegues — se comunican solo por eventos.

## Decisiones de arquitectura y trade-offs

- **RabbitMQ con colas simples (no exchange/topic)**: el flujo es lineal
  (dos colas: `order-created` y `stock-result`), así que un exchange con
  bindings hubiera sido complejidad sin beneficio para este alcance.

- **Catálogo de SKUs duplicado** en Orders.Api (para validar que el SKU
  exista) e Inventory.Worker (para el stock real). En un sistema real el
  catálogo lo expondría Inventory vía API/evento; aquí se duplicó como
  constante para no acoplar los servicios en tiempo de arranque.

- **`EnsureCreated()` en vez de migraciones EF completas**: el esquema es
  fijo para este ejercicio, así que se evitó el overhead de mantener
  migraciones versionadas. Ver "Qué haría distinto" para el trade-off.

- **Idempotencia por tabla de eventos procesados** (`ProcessedOrder`
  en Inventory, `ProcessedStock` en Orders) en vez de duplicación
  a nivel de broker: es explícita, fácil de testear y no depende de
  configuración específica de RabbitMQ.

- **RabbitMQ.Client 7.x (API asíncrona)**: Todo
  el código de mensajería usa `IChannel` y los métodos `*Async`
  (`CreateConnectionAsync`, `BasicPublishAsync`, `BasicConsumeAsync`).

- **Postgres real** (no in-memory) para que el estado persista mientras
  el sistema corre, como pide el enunciado; es el motor que ya uso en
  producción así que no agrega curva de aprendizaje.

## Manejo de fallos

- **Si Inventory.Worker no responde o está caído**: el mensaje
  `OrderCreated` queda esperando en la cola durable de RabbitMQ. El
  pedido se ve como `Pending` en el panel hasta que el worker vuelve y
  lo procesa. No hay pérdida de datos ni de mensajes.

- **Si el broker está caído cuando Orders.Api intenta publicar
  `OrderCreated`**: la publicación falla, se captura la excepción, se
  loguea como advertencia y el pedido queda persistido como `Pending`
  (la petición HTTP del cliente no falla: el pedido sí se creó). Esto
  puede dejar pedidos "huérfanos" si el broker cae justo después de
  guardar en base de datos — la solución correcta en producción es un
  **patrón Outbox** (guardar el evento en la misma transacción que el
  pedido, con un publicador en background que reintenta desde una tabla
  de outbox). Se documentó en vez de implementarse por alcance/tiempo.

- **Reentrega de mensajes**: los consumidores usan ack manual y solo
  confirman después de aplicar el cambio con éxito; si algo falla a
  mitad de camino, el mensaje se reencola (`nack requeue: true`) y se
  reprocesa sin efectos duplicados gracias a las tablas de idempotencia.

## Cómo correr todo

```bash
docker compose up --build
```

Esto levanta: Postgres (con las bases `orders` e `inventory` creadas
automáticamente vía script de init), RabbitMQ, Orders.Api, Inventory.Worker
y el frontend.

- Panel web: http://localhost:3000
- Orders.Api (Scalar/OpenApi): http://localhost:8080/scalar
- RabbitMQ management UI: http://localhost:15672 (guest/guest)

El stock inicial se siembra automáticamente al arrancar Inventory.Worker
(3 productos: `ABC-01` con 100 unidades, `ABC-02` con 50, `ABC-03` con 0
—a propósito en 0 para poder probar el flujo de rechazo desde el panel).

## Cómo correr los tests

```bash
cd backend
dotnet test src/Orders.Api.Tests/Orders.Api.Tests.csproj
```

Cubren:
- **Validación**: nombre vacío, SKU fuera de catálogo, cantidad fuera de
  rango 1-100, pedido válido.
- **Transición de estado**: `Pending → Confirmed` cuando hay stock,
  `Pending → Rejected` cuando no lo hay.
- **Idempotencia**: procesar el mismo `eventId` dos veces no vuelve a
  tocar el estado del pedido la segunda vez.

## Desarrollo local (sin Docker)

```bash
# Backend
cd backend
dotnet restore
# Postgres y RabbitMQ deben estar corriendo en localhost (ver appsettings.json)
dotnet run --project src/Orders.Api
dotnet run --project src/Inventory.Worker

# Frontend
cd frontend
npm install
npm run dev   # http://localhost:5173, usa VITE_API_URL=http://localhost:8080
```

## Qué haría distinto con más tiempo

- Implementar el **patrón Outbox** en Orders.Api para eliminar por
  completo la ventana de fallo entre "pedido guardado" y "evento
  publicado".

- Migraciones EF Core versionadas en vez de `EnsureCreated()`, para que
  el esquema pueda evolucionar de forma controlada.

- Exponer el catálogo de productos desde Inventory (endpoint o evento)
  en vez de duplicarlo como constante en Orders.Api.

- Actualización en tiempo real del panel vía SignalR en vez de polling.

- Cola/estrategia de dead-letter para mensajes que fallan repetidamente,
  en vez de reencolar indefinidamente.

- Tests de integración con Testcontainers (Postgres + RabbitMQ reales)
  además de los tests unitarios actuales.
