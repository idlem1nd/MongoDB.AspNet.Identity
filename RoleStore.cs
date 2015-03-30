using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    public class RoleStore<TRole> : RoleStore<TRole, string, IdentityUserRole>, IQueryableRoleStore<TRole>, IQueryableRoleStore<TRole, string>, IRoleStore<TRole, string>, IDisposable
    where TRole : IdentityRole, new()
    {
        public RoleStore() : base(new IdentityDbContext().db) { }
    }

    public class RoleStore<TRole, TKey, TUserRole> : IQueryableRoleStore<TRole, TKey>, IRoleStore<TRole, TKey>, IDisposable
        where TRole : IdentityRole<TKey, TUserRole>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
    {
        private bool _disposed;
        
        private readonly IMongoDatabase db;
        private const string collectionName = "AspNetRoles";


        public RoleStore(IMongoDatabase context)
        {
            db = context;
        }

        public bool DisposeContext
        {
            get;
            set;
        }

        public IQueryable<TRole> Roles
        {
            get { return db.GetCollection<TRole>(collectionName).Find(t => true).ToListAsync().Result.AsQueryable(); }
        }


        public virtual async Task CreateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await db.GetCollection<TRole>(collectionName).InsertOneAsync(role);
        }

        public virtual async Task DeleteAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await db.GetCollection<TRole>(collectionName).DeleteOneAsync(x => x.Id.Equals(ObjectId.Parse(role.Id.ToString())));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        public async Task<TRole> FindByIdAsync(TKey roleId)
        {
            this.ThrowIfDisposed();
            var result = await db.GetCollection<TRole>(collectionName).Find(x => x.Id.Equals(ObjectId.Parse(roleId.ToString()))).ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<TRole> FindByNameAsync(string roleName)
        {
            this.ThrowIfDisposed();
            var result = await db.GetCollection<TRole>(collectionName).Find(x => x.Name == roleName).ToListAsync();
            return result.FirstOrDefault();
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public virtual async Task UpdateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            var options = new UpdateOptions { IsUpsert = true };
            await db.GetCollection<TRole>(collectionName).ReplaceOneAsync(x => x.Id.Equals(ObjectId.Parse(role.Id.ToString())), role, options);
        }
    }
}