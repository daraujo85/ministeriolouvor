using MongoDB.Driver;
using MinisterioLouvor.Context;
using MinisterioLouvor.Interfaces;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MinisterioLouvor.Respository
{
  public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
  {
    protected readonly IMongoContext Context;
    protected IMongoCollection<TEntity> DbSet;

    protected BaseRepository(IMongoContext context)
    {
      Context = context;
    }

    public virtual async Task<TEntity> AddAsync(TEntity obj)
    {
      ConfigDbSet();
      await DbSet.InsertOneAsync(obj);

      return obj;
    }

    public void ConfigDbSet()
    {
      DbSet = Context.GetCollection<TEntity>(GetCollectionName());
    }

    public virtual async Task<TEntity> GetById(string id)
    {
      ConfigDbSet();

      var uuid = new ObjectId(id);

      var data = await DbSet.FindAsync(Builders<TEntity>.Filter.Eq("_id", uuid));
      return data.SingleOrDefault();
    }
    public virtual async Task<IEnumerable<TEntity>> GetAll()
    {
      ConfigDbSet();
      var all = await DbSet.FindAsync(Builders<TEntity>.Filter.Empty);
      return all.ToList();
    }
    public virtual async Task<IEnumerable<TEntity>> GetByFilter(Expression<Func<TEntity, bool>> expression)
    {
      ConfigDbSet();
      var data = await DbSet.FindAsync(expression);

      return data.ToList();
    }
    public virtual void Update(string id, TEntity obj)
    {
      ConfigDbSet();

      var uuid = new ObjectId(id);

      Context.AddCommand(() => DbSet.ReplaceOneAsync(Builders<TEntity>.Filter.Eq("_id", uuid), obj));
    }

    public virtual void Remove(string id)
    {
      ConfigDbSet();

      var uuid = new ObjectId(id);

      Context.AddCommand(() => DbSet.DeleteOneAsync(Builders<TEntity>.Filter.Eq("_id", uuid)));
    }

    public void Dispose()
    {
      Context?.Dispose();
    }
    private static string GetCollectionName()
    {
      return (typeof(TEntity).GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault()
          as BsonCollectionAttribute).CollectionName;
    }
  }


}
