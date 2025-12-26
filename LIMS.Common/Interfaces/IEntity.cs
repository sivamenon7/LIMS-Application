namespace LIMS.Common.Interfaces;

/// <summary>
/// Entity with Id property
/// </summary>
public interface IIdEntity
{
    Guid Id { get; set; }
}

/// <summary>
/// Entity with Id and RowVersion for optimistic concurrency
/// </summary>
public interface IIdAndRowVersionEntity : IIdEntity
{
    byte[] RowVersion { get; set; }
}

/// <summary>
/// Entity with audit data tracking (temporal table support)
/// </summary>
public interface IAuditDataEntity
{
    string ChangedData { get; set; }
    DateTime FromDate { get; set; }
    DateTime ToDate { get; set; }
}

/// <summary>
/// Entity with Active flag
/// </summary>
public interface IActiveEntity
{
    bool Active { get; set; }
}

/// <summary>
/// Complete auditable entity interface
/// </summary>
public interface IAuditableEntity : IIdAndRowVersionEntity, IAuditDataEntity, IActiveEntity
{
    bool Initial { get; set; }
}
