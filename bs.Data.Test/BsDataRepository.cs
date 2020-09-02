﻿using bs.Data.Interfaces;
using System.Linq;

namespace bs.Data.Test
{
    public class BsDataRepository : Repository
    {
        public BsDataRepository(IUnitOfWork unitOfwork) : base(unitOfwork)
        {
        }

        public BsDataEntityExample[] GetEntityExamples()
        {
            return Query<BsDataEntityExample>().ToArray();
        }

        public void CreateEntityExample(BsDataEntityExample entity)
        {
            Create(entity);
        }

        public async void CreateEntityExampleAsync(BsDataEntityExample entity)
        {
            await CreateAsync(entity);
        }
    }
}