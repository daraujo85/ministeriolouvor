using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinisterioLouvor.Interfaces
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity obj);
        Task<TEntity> GetById(string id);
        Task<IEnumerable<TEntity>> GetAll();
        void Update(string id, TEntity obj);
        void Remove(string id);
    }
}
