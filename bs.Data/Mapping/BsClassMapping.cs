using NHibernate.Mapping.ByCode.Impl;

namespace bs.Data.Mapping
{
    public class BsClassMapping<T> : BsClassCustomizer<T> where T : class
    {
        public BsClassMapping() : base(new ExplicitDeclarationsHolder(), new CustomizersHolder()) { }
    }
}
