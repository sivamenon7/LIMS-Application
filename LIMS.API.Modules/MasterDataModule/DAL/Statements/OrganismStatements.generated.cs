namespace MasterDataModule.DAL.Statements;

/// <summary>
/// SQL statements for Organism entity
/// Generated from database schema
/// </summary>
public class OrganismStatements
{
    public string Insert => @"
        INSERT INTO dbo.Organism (
            Id, TypeId, Genus, Species, Description,
            CharacterizationId, SeverityType, PictureId,
            SporeForming, Initial, Active, ChangedData
        )
        VALUES (
            @Id, @TypeId, @Genus, @Species, @Description,
            @CharacterizationId, @SeverityType, @PictureId,
            @SporeForming, @Initial, @Active, @ChangedData
        )";

    public string Update => @"
        UPDATE dbo.Organism
        SET
            TypeId = @TypeId,
            Genus = @Genus,
            Species = @Species,
            Description = @Description,
            CharacterizationId = @CharacterizationId,
            SeverityType = @SeverityType,
            PictureId = @PictureId,
            SporeForming = @SporeForming,
            Active = @Active,
            ChangedData = @ChangedData
        WHERE Id = @Id
        AND RowVersion = @RowVersion";

    public string GetById => @"
        SELECT * FROM dbo.Organism
        WHERE Id = @Id";

    public string GetAll => @"
        SELECT * FROM dbo.Organism
        ORDER BY Genus, Species";

    public string Activate => @"
        UPDATE dbo.Organism
        SET Active = 1, ChangedData = @ChangedData
        WHERE Id = @Id";

    public string Deactivate => @"
        UPDATE dbo.Organism
        SET Active = 0, ChangedData = @ChangedData
        WHERE Id = @Id";

    public string Exists => @"
        SELECT COUNT(1)
        FROM dbo.Organism
        WHERE Id = @Id";

    public string ExistsByCombination => @"
        SELECT COUNT(1)
        FROM dbo.Organism
        WHERE TypeId = @TypeId
        AND Genus = @Genus
        AND CharacterizationId = @CharacterizationId";

    public string Count => @"
        SELECT COUNT(1)
        FROM dbo.Organism";

    // Temporal queries
    public string GetHistory => @"
        SELECT *
        FROM dbo.Organism FOR SYSTEM_TIME ALL
        WHERE Id = @Id
        ORDER BY FromDate DESC";

    public string GetAsOf => @"
        SELECT *
        FROM dbo.Organism FOR SYSTEM_TIME AS OF @AsOf
        WHERE Id = @Id";
}
