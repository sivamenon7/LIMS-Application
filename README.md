# LIMS Enterprise - Laboratory Information Management System

A distributed, modular, enterprise-grade Laboratory Information Management System built with cutting-edge technologies.

## Architecture

This system is built using modern cloud-native patterns and technologies:

- **Frontend**: Blazor WebAssembly (WASM) with MudBlazor UI components
- **Backend**: ASP.NET Core Web APIs (.NET 9 / .NET 10 ready)
- **Database**: SQL Server 2025 with Temporal Tables for complete audit history
- **Data Access**: Dapper-based generated DAL with repository pattern
- **Architecture**: Event-driven with MassTransit and RabbitMQ
- **Caching**: Distributed caching with Redis
- **Orchestration**: .NET Aspire for cloud-native development
- **Containerization**: Docker and Docker Compose

## Features

### Sample Management
- Complete sample lifecycle tracking
- Batch management
- Chain of custody
- Storage location tracking
- Priority-based processing

### Test Management
- Configurable test definitions
- Test parameters with acceptance criteria
- Multiple test categories (Chemical, Physical, Microbiological, etc.)
- Method references and SOP integration

### Results & Quality Control
- Test result entry with validation
- Out-of-specification (OOS) detection
- Multi-level review and approval workflow
- Complete audit trail with temporal tables

### Inventory Management
- Reagent and consumable tracking
- Automatic reorder alerts
- Lot tracking and expiration management
- Transaction history

### Instrument Management
- Equipment tracking
- Calibration scheduling and records
- Maintenance history
- Status monitoring

### Security & Compliance
- Role-based access control (RBAC)
- Permission-based authorization
- Complete audit trail using SQL Server temporal tables
- 21 CFR Part 11 ready architecture

## Project Structure

```
LIMS-Application/
├── src/
│   ├── LIMS.Core/                 # Domain entities and interfaces
│   ├── LIMS.Infrastructure/       # Data access with Dapper
│   ├── LIMS.Application/          # Business logic and CQRS
│   ├── LIMS.API/                  # ASP.NET Core Web API
│   ├── LIMS.UI.Blazor/            # Blazor WASM frontend
│   ├── LIMS.EventBus/             # Event-driven messaging
│   └── LIMS.Caching/              # Redis caching layer
├── infrastructure/
│   ├── LIMS.AppHost/              # .NET Aspire orchestration
│   └── LIMS.ServiceDefaults/      # Shared service configurations
├── database/
│   ├── 01-CreateDatabase.sql     # Database creation
│   ├── 02-CreateTables.sql       # Table definitions with temporal tables
│   └── 03-SeedData.sql           # Initial seed data
├── docker-compose.yml             # Container orchestration
└── LIMS.Enterprise.sln            # Solution file
```

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- Docker Desktop
- SQL Server 2025 (or use Docker container)
- Visual Studio 2022 / VS Code / JetBrains Rider

### Running with Docker Compose

1. Clone the repository:
```bash
git clone <repository-url>
cd LIMS-Application
```

2. Start all services:
```bash
docker-compose up -d
```

3. Initialize the database:
```bash
docker exec -it lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -i /docker-entrypoint-initdb.d/01-CreateDatabase.sql

docker exec -it lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -i /docker-entrypoint-initdb.d/02-CreateTables.sql

docker exec -it lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -i /docker-entrypoint-initdb.d/03-SeedData.sql
```

4. Access the application:
- **Frontend**: http://localhost:5000
- **API**: http://localhost:7001
- **API Swagger**: http://localhost:7001/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Running with .NET Aspire

1. Install .NET Aspire workload:
```bash
dotnet workload update
dotnet workload install aspire
```

2. Run the AppHost:
```bash
cd infrastructure/LIMS.AppHost
dotnet run
```

3. Open the Aspire dashboard (URL shown in console)

### Running Locally (Development)

1. Start dependencies:
```bash
docker-compose up sqlserver redis rabbitmq -d
```

2. Initialize the database (run SQL scripts in order)

3. Run the API:
```bash
cd src/LIMS.API
dotnet run
```

4. Run the Blazor UI:
```bash
cd src/LIMS.UI.Blazor
dotnet run
```

## Configuration

### Connection Strings

Update `appsettings.json` in LIMS.API:

```json
{
  "ConnectionStrings": {
    "LIMSDatabase": "Server=localhost,1433;Database=LIMSEnterprise;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## Default Credentials

- **Username**: admin
- **Password**: Admin@123 (Please change in production)

## Key Technologies

### Backend
- **ASP.NET Core 9.0**: Modern, high-performance web framework
- **Dapper**: Lightweight ORM for high-performance data access
- **MediatR**: CQRS and mediator pattern implementation
- **FluentValidation**: Business rule validation
- **Serilog**: Structured logging

### Frontend
- **Blazor WebAssembly**: C# in the browser
- **MudBlazor**: Material Design component library
- **Blazored.LocalStorage**: Client-side storage

### Infrastructure
- **SQL Server 2025**: Temporal tables for audit history
- **Redis**: Distributed caching
- **RabbitMQ**: Message broker for event-driven architecture
- **MassTransit**: Service bus abstraction
- **.NET Aspire**: Cloud-native orchestration
- **Docker**: Containerization

## Event-Driven Architecture

The system uses events for decoupled communication:

- `SampleCreatedEvent`: Fired when a new sample is registered
- `SampleStatusChangedEvent`: Fired when sample status changes
- `TestResultEnteredEvent`: Fired when test results are entered
- `TestResultApprovedEvent`: Fired when results are approved

## Temporal Tables

All major entities use SQL Server temporal tables for:
- Complete audit history
- Point-in-time queries
- Compliance and regulatory requirements
- Change tracking without application code

Query historical data:
```sql
SELECT * FROM Samples
FOR SYSTEM_TIME AS OF '2024-01-01T00:00:00'
WHERE SampleNumber = 'SMP-2024-001';
```

## API Documentation

Once running, access Swagger UI at: http://localhost:7001/swagger

### Sample Endpoints

- `GET /api/samples` - Get all samples
- `GET /api/samples/{id}` - Get sample by ID
- `POST /api/samples` - Create new sample
- `PUT /api/samples/{id}` - Update sample
- `DELETE /api/samples/{id}` - Delete sample (soft delete)

## Performance

- **Dapper**: Direct SQL mapping for maximum performance
- **Redis Caching**: Reduces database load for frequently accessed data
- **Async/Await**: Non-blocking operations throughout
- **Connection Pooling**: Efficient database connection management

## Security

- Role-based access control (RBAC)
- JWT token authentication (ready for implementation)
- HTTPS/TLS encryption
- SQL injection prevention with parameterized queries
- CORS configuration
- Input validation with FluentValidation

## Monitoring

- OpenTelemetry integration for distributed tracing
- Health checks on all services
- Serilog structured logging
- .NET Aspire dashboard for real-time monitoring

## Scalability

- Stateless API design for horizontal scaling
- Distributed caching with Redis
- Event-driven architecture for loose coupling
- Docker containers for easy deployment
- Load balancer ready

## Future Enhancements

- [ ] ElasticSearch integration for advanced search
- [ ] SignalR for real-time notifications
- [ ] Advanced reporting with Crystal Reports / Telerik
- [ ] Mobile app with .NET MAUI
- [ ] Barcode/QR code scanning
- [ ] Electronic signatures (21 CFR Part 11)
- [ ] Integration with laboratory instruments (LIMS middleware)
- [ ] Advanced analytics and ML predictions

## Contributing

This is an enterprise-grade reference architecture. Contributions welcome!

## License

Proprietary - Enterprise License

## Support

For support, please contact your system administrator or open an issue in the repository.
