using CSG.Data;
using CSG.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Repository.Abstracts
{
    public abstract class RepositoryBase<T, TId> : IRepository<T, TId>
        where T : BaseEntity, new()
        {
            protected GizemContext _context;
            public DbSet<T> Table { get; protected set; }
            protected RepositoryBase(GizemContext gizemContext)
            {
                _context = gizemContext;
                Table = _context.Set<T>();
            }
            public virtual T GetById(TId id)
            {
                return Table.Find(id);
            }
            public virtual IQueryable<T> Get(Func<T, bool> predicate = null)
            {
                return predicate == null ? Table : Table.Where(predicate).AsQueryable();
            }

            public virtual IQueryable<T> Get(string[] includes, Func<T, bool> predicate = null)
            {
                IQueryable<T> query = Table;
                foreach (var include in includes)
                {
                    query = Table.Include(include);
                }
                return predicate == null ? query : query.Where(predicate).AsQueryable();
            }

            public virtual void Add(T entity, bool isSaveLater = false)
            {
                Table.Add(entity);
                if (!isSaveLater)
                    this.Save();

            }
            public virtual void Remove(T entity, bool isSaveLater = false)
            {
                Table.Remove(entity);
                if (!isSaveLater)
                    this.Save();
            }

            public virtual void Update(T entity, bool isSaveLater = false)
            {
                Table.Update(entity);
                if (!isSaveLater)
                    this.Save();
            }
            public virtual int Save()
            {
                return _context.SaveChanges();
            }
        }
}
