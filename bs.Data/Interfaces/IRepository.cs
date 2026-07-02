namespace bs.Data.Interfaces
{
    /// <summary>
    /// Marker interface for repository classes.
    /// Implement this interface (or extend <see cref="bs.Data.Repository"/>) to create a
    /// repository that can be registered in the DI container and resolved by consumers.
    /// </summary>
    /// <remarks>
    /// This interface is intentionally empty. Concrete repositories expose only the operations
    /// relevant to their aggregate by wrapping the protected methods of <see cref="bs.Data.Repository"/>.
    /// </remarks>
    public interface IRepository
    {
    }
}