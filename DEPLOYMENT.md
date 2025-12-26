# LIMS Enterprise - Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Local Development](#local-development)
3. [Docker Deployment](#docker-deployment)
4. [Production Deployment](#production-deployment)
5. [Database Migration](#database-migration)
6. [Monitoring](#monitoring)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software
- .NET 9 SDK or later
- Docker Desktop 4.25 or later
- SQL Server 2019+ or SQL Server 2025
- Git

### Optional Tools
- Visual Studio 2022 or later
- VS Code with C# extension
- Azure CLI (for cloud deployment)
- Kubernetes CLI (for K8s deployment)

## Local Development

### Option 1: Using .NET Aspire (Recommended)

1. Install .NET Aspire:
```bash
dotnet workload update
dotnet workload install aspire
```

2. Run the AppHost:
```bash
cd infrastructure/LIMS.AppHost
dotnet run
```

3. Access the Aspire Dashboard (URL shown in console output)

### Option 2: Manual Setup

1. Start infrastructure services:
```bash
docker-compose up sqlserver redis rabbitmq -d
```

2. Initialize the database:
```bash
# Connect to SQL Server
sqlcmd -S localhost,1433 -U sa -P YourStrong@Passw0rd

# Run scripts
:r database/01-CreateDatabase.sql
:r database/02-CreateTables.sql
:r database/03-SeedData.sql
GO
```

3. Run the API:
```bash
cd src/LIMS.API
dotnet run --launch-profile https
```

4. Run the Blazor UI:
```bash
cd src/LIMS.UI.Blazor
dotnet run
```

## Docker Deployment

### Full Stack Deployment

1. Build and start all services:
```bash
docker-compose up --build -d
```

2. Monitor logs:
```bash
docker-compose logs -f
```

3. Check service health:
```bash
docker-compose ps
```

4. Initialize database:
```bash
# Wait for SQL Server to be healthy
docker exec -it lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -Q "SELECT @@VERSION"

# Run initialization scripts
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

### Service URLs
- **Frontend**: http://localhost:5000
- **API**: http://localhost:7001
- **Swagger**: http://localhost:7001/swagger
- **RabbitMQ Management**: http://localhost:15672
- **SQL Server**: localhost,1433

### Stopping Services
```bash
docker-compose down
```

### Removing Volumes (Clean Restart)
```bash
docker-compose down -v
```

## Production Deployment

### Azure Deployment

#### Prerequisites
- Azure subscription
- Azure CLI installed
- Resource group created

#### 1. Azure Container Registry

```bash
# Login to Azure
az login

# Create container registry
az acr create --resource-group lims-rg \
  --name limsacr --sku Basic

# Login to ACR
az acr login --name limsacr

# Build and push images
docker-compose build
docker tag lims-api:latest limsacr.azurecr.io/lims-api:latest
docker tag lims-ui:latest limsacr.azurecr.io/lims-ui:latest
docker push limsacr.azurecr.io/lims-api:latest
docker push limsacr.azurecr.io/lims-ui:latest
```

#### 2. Azure SQL Database

```bash
# Create SQL Server
az sql server create \
  --name lims-sql-server \
  --resource-group lims-rg \
  --location eastus \
  --admin-user sqladmin \
  --admin-password YourPassword123!

# Create database
az sql db create \
  --resource-group lims-rg \
  --server lims-sql-server \
  --name LIMSEnterprise \
  --service-objective S0

# Configure firewall
az sql server firewall-rule create \
  --resource-group lims-rg \
  --server lims-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

#### 3. Azure Cache for Redis

```bash
az redis create \
  --resource-group lims-rg \
  --name lims-redis \
  --location eastus \
  --sku Basic \
  --vm-size c0
```

#### 4. Azure Service Bus (RabbitMQ Alternative)

```bash
az servicebus namespace create \
  --resource-group lims-rg \
  --name lims-servicebus \
  --location eastus \
  --sku Standard
```

#### 5. Azure Container Apps

```bash
# Create environment
az containerapp env create \
  --name lims-env \
  --resource-group lims-rg \
  --location eastus

# Deploy API
az containerapp create \
  --name lims-api \
  --resource-group lims-rg \
  --environment lims-env \
  --image limsacr.azurecr.io/lims-api:latest \
  --target-port 80 \
  --ingress external \
  --registry-server limsacr.azurecr.io \
  --env-vars \
    "ConnectionStrings__LIMSDatabase=Server=tcp:lims-sql-server.database.windows.net,1433;Database=LIMSEnterprise;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;" \
    "Redis__ConnectionString=lims-redis.redis.cache.windows.net:6380,ssl=True,password=<redis-key>"

# Deploy UI
az containerapp create \
  --name lims-ui \
  --resource-group lims-rg \
  --environment lims-env \
  --image limsacr.azurecr.io/lims-ui:latest \
  --target-port 80 \
  --ingress external \
  --env-vars "ApiBaseUrl=https://lims-api.azurecontainerapps.io"
```

### Kubernetes Deployment

#### 1. Create Kubernetes Manifests

Create `k8s/deployment.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: lims-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: lims-api
  template:
    metadata:
      labels:
        app: lims-api
    spec:
      containers:
      - name: lims-api
        image: limsacr.azurecr.io/lims-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__LIMSDatabase
          valueFrom:
            secretKeyRef:
              name: lims-secrets
              key: db-connection
---
apiVersion: v1
kind: Service
metadata:
  name: lims-api
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 80
  selector:
    app: lims-api
```

#### 2. Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace lims

# Create secrets
kubectl create secret generic lims-secrets \
  --from-literal=db-connection="Server=..." \
  --namespace lims

# Apply deployments
kubectl apply -f k8s/deployment.yaml -n lims

# Check status
kubectl get pods -n lims
kubectl get services -n lims
```

## Database Migration

### Creating Migration Scripts

For schema changes, create new SQL scripts:
```sql
-- database/migrations/04-AddNewColumn.sql
USE LIMSEnterprise;
GO

ALTER TABLE Samples
ADD ExternalReference NVARCHAR(100) NULL;
GO
```

### Running Migrations

```bash
# Local
sqlcmd -S localhost,1433 -U sa -P password -i database/migrations/04-AddNewColumn.sql

# Docker
docker exec -it lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -i /migrations/04-AddNewColumn.sql

# Azure
sqlcmd -S lims-sql-server.database.windows.net -U sqladmin -P password \
  -d LIMSEnterprise -i database/migrations/04-AddNewColumn.sql
```

## Monitoring

### Application Insights (Azure)

1. Create Application Insights:
```bash
az monitor app-insights component create \
  --app lims-insights \
  --location eastus \
  --resource-group lims-rg
```

2. Add instrumentation key to appsettings:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### Health Checks

Check service health:
```bash
# API health
curl http://localhost:7001/health

# Liveness probe
curl http://localhost:7001/alive
```

### Logs

View logs:
```bash
# Docker
docker-compose logs -f lims-api

# Kubernetes
kubectl logs -f deployment/lims-api -n lims

# Local files
tail -f src/LIMS.API/logs/lims-*.log
```

## Troubleshooting

### Database Connection Issues

```bash
# Test SQL Server connection
docker exec -it lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"

# Check connection string in API logs
docker logs lims-api | grep "Connection"
```

### Redis Connection Issues

```bash
# Test Redis connection
docker exec -it lims-redis redis-cli ping
# Should return: PONG

# Check Redis logs
docker logs lims-redis
```

### RabbitMQ Connection Issues

```bash
# Check RabbitMQ status
docker exec -it lims-rabbitmq rabbitmq-diagnostics status

# Access management UI
# Open: http://localhost:15672
# Credentials: guest/guest
```

### Container Issues

```bash
# Restart specific service
docker-compose restart lims-api

# Rebuild and restart
docker-compose up --build -d lims-api

# Check container logs
docker logs lims-api --tail 100

# Execute commands in container
docker exec -it lims-api /bin/bash
```

### Performance Issues

1. Check database indexes:
```sql
-- Missing index query
SELECT * FROM sys.dm_db_missing_index_details
WHERE database_id = DB_ID('LIMSEnterprise');
```

2. Monitor Redis cache hit ratio
3. Check API response times in Application Insights
4. Review RabbitMQ queue depths

### Common Errors

**Error: Database not found**
- Run database creation scripts
- Check connection string

**Error: Redis timeout**
- Increase timeout in configuration
- Check Redis memory usage

**Error: Port already in use**
- Change port in docker-compose.yml
- Stop conflicting service

## Backup and Recovery

### Database Backup

```bash
# Manual backup
docker exec lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -Q "BACKUP DATABASE LIMSEnterprise TO DISK='/var/opt/mssql/backup/lims.bak'"

# Automated backup script
# See scripts/backup.sh
```

### Database Restore

```bash
docker exec lims-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Passw0rd \
  -Q "RESTORE DATABASE LIMSEnterprise FROM DISK='/var/opt/mssql/backup/lims.bak' WITH REPLACE"
```

## Security Checklist

- [ ] Change default passwords
- [ ] Enable HTTPS/TLS
- [ ] Configure firewall rules
- [ ] Implement authentication
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Backup encryption
- [ ] Network isolation
- [ ] Secret management (Azure Key Vault)
- [ ] API rate limiting

## Performance Tuning

- [ ] Enable response compression
- [ ] Configure output caching
- [ ] Optimize database indexes
- [ ] Tune Redis eviction policy
- [ ] Configure connection pooling
- [ ] Enable CDN for static files
- [ ] Implement query result caching
- [ ] Use async/await properly
