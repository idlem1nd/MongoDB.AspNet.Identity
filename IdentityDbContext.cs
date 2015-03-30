using System;
using System.Configuration;
using System.Linq;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    public class IdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    where TUser : IdentityUser
    {
        public IdentityDbContext() : this("DefaultConnection") { }
        public IdentityDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
    }

    public class IdentityDbContext : IdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public IdentityDbContext() : this("DefaultConnection") { }
        public IdentityDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
    }

    public class IdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> : IDisposable
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>
        where TRole : IdentityRole<TKey, TUserRole>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
    {

        internal readonly IMongoDatabase db;

        public IMongoDatabase Context { get { return db; } }


        /// <summary>
        ///     Gets the database from connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>MongoDatabase.</returns>
        /// <exception cref="System.Exception">No database name specified in connection string</exception>
        private IMongoDatabase GetDatabaseFromSqlStyle(string connectionString)
        {
            throw new NotSupportedException("SQL Style connection strings are not supported. Please use a Mongo URL.");
        }

        /// <summary>
        ///     Gets the database from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>MongoDatabase.</returns>
        private IMongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            
            if (url.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return client.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        /// <summary>
        ///     Uses connectionString to connect to server and then uses databae name specified.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <returns>MongoDatabase.</returns>
        private IMongoDatabase GetDatabase(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(dbName);
        }

        public bool RequireUniqueEmail
        {
            get;
            set;
        }

        public virtual IQueryable<TRole> Roles
        {
            get;
            set;
        }

        public virtual IQueryable<TUser> Users
        {
            get;
            set;
        }

        public IdentityDbContext() : this("DefaultConnection") { }
        public IdentityDbContext(string nameOrConnectionString)
        {
            if (nameOrConnectionString.ToLower().StartsWith("mongodb://"))
            {
                db = GetDatabaseFromUrl(new MongoUrl(nameOrConnectionString));
            }
            else
            {
                string connStringFromManager =
                    ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
                if (connStringFromManager.ToLower().StartsWith("mongodb://"))
                {
                    db = GetDatabaseFromUrl(new MongoUrl(connStringFromManager));
                }
                else
                {
                    db = GetDatabaseFromSqlStyle(connStringFromManager);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}