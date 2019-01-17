using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;
using Remit.Data.Models;
using Remit.Model.Models;
using System.Xml.Serialization;
using System.Xml;
using System.Transactions;
using EntityFramework.BulkInsert.Extensions;


namespace Remit.Data.Infrastructure
{
    public abstract class RepositoryBase<T> where T : class
    {
        private ApplicationEntities dataContext;
        private readonly IDbSet<T> dbset;


        protected RepositoryBase(IDatabaseFactory databaseFactory)
        {
            DatabaseFactory = databaseFactory;
            dbset = DataContext.Set<T>();
        }

        protected IDatabaseFactory DatabaseFactory
        {
            get;
            private set;
        }

        protected ApplicationEntities DataContext
        {
            get { return dataContext ?? (dataContext = DatabaseFactory.Get()); }
        }
        public virtual void Add(T entity)
        {
            dbset.Add(entity);
        }
        public virtual void Update(T entity)
        {
            dbset.Attach(entity);
            dataContext.Entry(entity).State = EntityState.Modified;
        }
        public virtual void Delete(T entity)
        {
            dbset.Remove(entity);
        }
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = dbset.Where<T>(where).AsEnumerable();
            foreach (T obj in objects)
                dbset.Remove(obj);
        }
        public virtual T GetById(long id)
        {
            return dbset.Find(id);
        }
        public virtual T GetById(string id)
        {
            return dbset.Find(id);
        }

        public virtual T GetById(Guid id)
        {
           return dbset.Find(id);
        }

        public virtual IEnumerable<T> GetAll()
        {
          return dbset.ToList();
        }

        public virtual IEnumerable<T> GetMany(Expression<Func<T, bool>> where)
        {
            return dbset.Where(where).ToList();
        }

        public T Get(Expression<Func<T, bool>> where)
        {
            return dbset.Where(where).FirstOrDefault<T>();
        }

        public virtual void BulkSave(List<T> entities)
        {

               using (var transactionScope = new TransactionScope())
                {
                    using (var ctx = new ApplicationEntities())
                    {
                        // some stuff in dbcontext
                        ctx.BulkInsert(entities);
                        ctx.SaveChanges();
                        //ctx.Commit();
                        transactionScope.Complete();
                    }
                }
        }

    }
}
