namespace bs.Data.Interfaces.BaseEntities
{
    /// <summary>
    /// This is the base Interface for all entity models. Repositories implementation will accept types derived from this interface only.
    /// </summary>
    public interface IPersistentEntity
    {
    }

    //public interface IPersistentEntity<T>
    //{
    //    T Id { get; set; }
    //}
}