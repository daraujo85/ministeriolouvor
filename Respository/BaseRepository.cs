﻿using MongoDB.Driver;
using MinisterioLouvor.Context;
using MinisterioLouvor.Interfaces;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        public virtual void Add(TEntity obj)
        {
            ConfigDbSet();
            Context.AddCommand(() => DbSet.InsertOneAsync(obj));
        }

        public void ConfigDbSet()
        {
            DbSet = Context.GetCollection<TEntity>(GetCollectionName());
        }

        public virtual async Task<TEntity> GetById(Guid id)
        {
            ConfigDbSet();
            var data = await DbSet.FindAsync(Builders<TEntity>.Filter.Eq("_id", id));
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
        public virtual void Update(TEntity obj)
        {
            ConfigDbSet();
            Context.AddCommand(() => DbSet.ReplaceOneAsync(Builders<TEntity>.Filter.Eq("_id", obj.GetId()), obj));
        }

        public virtual void Remove(Guid id)
        {
            ConfigDbSet();
            Context.AddCommand(() => DbSet.DeleteOneAsync(Builders<TEntity>.Filter.Eq("_id", id)));
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
