namespace bs.Data.Interfaces.BaseEntities
{
    /// <summary>This interface is used to define 'enableable' entities.</summary>
    /// <seealso cref="bs.Data.Interfaces.BaseEntities.IPersistentEntity" />
    public interface IEnableableEntity : IPersistentEntity
    {
        /// <summary>Gets or sets a value indicating whether this entity is enabled.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
        bool IsEnabled { get; set; }
    }
}