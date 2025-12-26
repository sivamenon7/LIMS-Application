# MasterDataModule Database

This database project contains all SQL scripts for the Master Data module.

## Structure

```
Scripts/
├── 01-CreateSchema.sql          - Creates schemas
├── 02-Organism-Table.sql        - Organism table with temporal support
└── ...                          - Additional tables
```

## Running Scripts

### Option 1: Using SQLCMD
```bash
sqlcmd -S localhost -U sa -P YourPassword -d LIMSEnterprise -i Scripts/01-CreateSchema.sql
sqlcmd -S localhost -U sa -P YourPassword -d LIMSEnterprise -i Scripts/02-Organism-Table.sql
```

### Option 2: Using SQL Server Management Studio
1. Open SSMS
2. Connect to your server
3. Open each script file
4. Execute in order (01, 02, ...)

## Features

- **Temporal Tables**: All tables use SQL Server System-Versioned Temporal Tables
- **History Tracking**: Complete audit trail in [audit] schema
- **Optimistic Concurrency**: RowVersion columns for conflict detection
- **Audit Data**: ChangedData field stores who/when/why

## Tables

### Organism
- Stores organism data (genus, species, characterization)
- Temporal table with history in [audit].[OrganismHistory]
- Unique constraint on TypeId + Genus + CharacterizationId + Species
- Indexes on filtered columns (TypeId, CharacterizationId, Active)
