# LIMS Enterprise - Architecture Documentation

## Overview

LIMS Enterprise is built using Clean Architecture principles with a focus on modularity, testability, and maintainability.

## Architecture Layers

### 1. Presentation Layer
- **LIMS.UI.Blazor**: Blazor WebAssembly frontend
  - Material Design UI with MudBlazor
  - Progressive Web App (PWA) support
  - Offline-capable with service workers
  - Client-side routing
  - Local storage for user preferences

### 2. API Layer
- **LIMS.API**: ASP.NET Core Web API
  - RESTful endpoints
  - Swagger/OpenAPI documentation
  - CORS configuration
  - Health checks
  - Structured logging with Serilog
  - API versioning support

### 3. Application Layer
- **LIMS.Application**: Business logic and orchestration
  - CQRS pattern with MediatR
  - Command and Query handlers
  - Business validation with FluentValidation
  - DTOs and AutoMapper
  - Application services

### 4. Domain Layer
- **LIMS.Core**: Domain entities and business rules
  - Entity models
  - Value objects
  - Domain interfaces
  - Business enumerations
  - Domain events

### 5. Infrastructure Layer
- **LIMS.Infrastructure**: Data access and external services
  - Dapper repositories
  - Unit of Work pattern
  - Database connection factory
  - SQL Server integration

- **LIMS.EventBus**: Event-driven messaging
  - MassTransit integration
  - RabbitMQ message broker
  - Event consumers
  - Pub/Sub patterns

- **LIMS.Caching**: Distributed caching
  - Redis integration
  - Cache-aside pattern
  - Sliding expiration
  - Cache invalidation strategies

### 6. Orchestration Layer
- **LIMS.AppHost**: .NET Aspire orchestration
  - Service discovery
  - Configuration management
  - Health monitoring
  - Telemetry collection

- **LIMS.ServiceDefaults**: Shared configurations
  - OpenTelemetry setup
  - Resilience patterns
  - Default health checks

## Design Patterns

### 1. Repository Pattern
Abstracts data access logic:
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
}
```

### 2. Unit of Work Pattern
Manages transactions across repositories:
```csharp
public interface IUnitOfWork
{
    IRepository<Sample> Samples { get; }
    IRepository<Test> Tests { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
}
```

### 3. CQRS (Command Query Responsibility Segregation)
Separates read and write operations:
- **Commands**: Modify state (CreateSampleCommand)
- **Queries**: Read data (GetSampleByIdQuery)

### 4. Mediator Pattern
Decouples request handling:
```csharp
var result = await _mediator.Send(new CreateSampleCommand { ... });
```

### 5. Dependency Injection
All dependencies injected via DI container:
```csharp
services.AddApplication();
services.AddInfrastructure();
services.AddEventBus(configuration);
services.AddCaching(configuration);
```

## Data Access Strategy

### Dapper-Based DAL
- **Micro ORM**: Lightweight and performant
- **Generated Queries**: Type-safe SQL generation
- **Stored Procedures**: Support for complex operations
- **Bulk Operations**: Efficient batch processing

### Why Dapper over EF Core?
1. **Performance**: 50-100% faster than EF Core
2. **Control**: Full control over SQL queries
3. **Flexibility**: Easy integration with temporal tables
4. **Simplicity**: Less abstraction, more predictability
5. **Learning Curve**: Easier for SQL-proficient teams

## Database Design

### Temporal Tables
All entities use SQL Server temporal tables:
```sql
CREATE TABLE Samples (
    -- columns --
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.SamplesHistory));
```

Benefits:
- Automatic audit trail
- Point-in-time queries
- Compliance with regulations (FDA 21 CFR Part 11)
- No application code for history tracking

### Soft Delete
Entities use soft delete pattern:
```csharp
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }
public string? DeletedBy { get; set; }
```

## Event-Driven Architecture

### Event Flow
1. Action occurs (e.g., sample created)
2. Event published to RabbitMQ
3. Consumers process event asynchronously
4. Side effects executed (notifications, logging, etc.)

### Event Types
- **Domain Events**: Business-level events (SampleCreated)
- **Integration Events**: Cross-service events
- **Notification Events**: User notifications

### Benefits
- **Decoupling**: Services don't directly depend on each other
- **Scalability**: Async processing of long-running tasks
- **Reliability**: Message persistence and retry logic
- **Audit**: Complete event history

## Caching Strategy

### Cache-Aside Pattern
```csharp
var sample = await _cache.GetAsync<Sample>(key);
if (sample == null)
{
    sample = await _repository.GetByIdAsync(id);
    await _cache.SetAsync(key, sample, TimeSpan.FromMinutes(30));
}
return sample;
```

### Cache Invalidation
- Time-based expiration
- Event-based invalidation
- Manual invalidation on updates

### What to Cache
- Reference data (test definitions, customers)
- Frequently accessed samples
- User permissions and roles
- Configuration settings

## Security Architecture

### Authentication
- JWT token-based authentication (ready for implementation)
- Integration with Identity providers (Azure AD, Auth0)
- Token refresh mechanism

### Authorization
- Role-based access control (RBAC)
- Permission-based authorization
- Custom authorization policies

### Data Protection
- Encryption at rest (SQL Server TDE)
- Encryption in transit (TLS/HTTPS)
- Sensitive data masking
- Audit logging

## Scalability Considerations

### Horizontal Scaling
- Stateless API design
- Load balancer ready
- Session state in Redis
- File storage in cloud (Azure Blob, S3)

### Database Scaling
- Read replicas for queries
- Write to primary database
- Connection pooling
- Query optimization

### Caching Layer
- Distributed cache (Redis)
- Multi-level caching (memory + distributed)
- Cache warming strategies

## Monitoring & Observability

### OpenTelemetry
- Distributed tracing
- Metrics collection
- Log correlation

### Health Checks
- Liveness probes
- Readiness probes
- Dependency health (database, cache, message broker)

### Logging
- Structured logging with Serilog
- Log aggregation (ELK, Application Insights)
- Correlation IDs for request tracking

## Deployment Architecture

### Containerization
```
┌─────────────┐
│   Nginx     │ ← Blazor WASM
└─────────────┘
       │
┌─────────────┐
│  LIMS API   │ ← ASP.NET Core
└─────────────┘
       │
┌──────┬──────┬────────┐
│ SQL  │Redis │RabbitMQ│
└──────┴──────┴────────┘
```

### Cloud Deployment (Azure Example)
- **App Services**: Host API and Blazor
- **Azure SQL**: Managed database
- **Redis Cache**: Managed Redis
- **Service Bus**: Alternative to RabbitMQ
- **Application Insights**: Monitoring
- **Azure DevOps**: CI/CD pipelines

## Performance Optimizations

### API Layer
- Response compression
- Output caching
- Async/await throughout
- Connection pooling

### Database Layer
- Proper indexing strategy
- Query optimization
- Stored procedures for complex operations
- Batch operations with Dapper

### Frontend
- Lazy loading
- Virtual scrolling for large lists
- Image optimization
- Service worker caching

## Testing Strategy

### Unit Tests
- Business logic in Application layer
- Domain entity validation
- Repository mocks

### Integration Tests
- API endpoint testing
- Database integration
- Event bus integration

### E2E Tests
- Playwright/Selenium for UI
- Full user workflows

## Future Architecture Enhancements

1. **Microservices Migration**
   - Sample service
   - Test service
   - Inventory service
   - Reporting service

2. **GraphQL API**
   - Flexible querying
   - Reduced over-fetching
   - Real-time subscriptions

3. **CQRS with Event Sourcing**
   - Complete event history
   - Replay events
   - Time-travel debugging

4. **Advanced Search**
   - Elasticsearch integration
   - Full-text search
   - Complex filtering

5. **Real-time Features**
   - SignalR for notifications
   - Live dashboard updates
   - Collaborative editing
