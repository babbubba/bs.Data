namespace bs.Data.Interfaces.BaseEntities
{
    /// <summary>
    /// Marker interface that all entity models must implement.
    /// Repository methods are constrained to types that implement this interface,
    /// ensuring only mapped entities are passed to NHibernate operations.
    /// </summary>
    /// <remarks>
    /// This interface is intentionally empty. Its sole purpose is to act as a compile-time
    /// constraint on generic repository methods (e.g., <c>Create&lt;TEntity&gt;</c>,
    /// <c>GetById&lt;TEntity&gt;</c>). The primary key type and shape are left to the
    /// concrete entity class and its NHibernate mapping.
    /// </remarks>
    public interface IPersistentEntity
    {
    }
}