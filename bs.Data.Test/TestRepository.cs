using bs.Data.Interfaces;

namespace bs.Data.Test
{
    public class TestRepository : Repository
    {
        public TestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
  
}
