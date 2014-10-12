using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Domain
{
    public class DocumentDbOperations
    {
        public DocumentDbOperations()
        {
            
        }

        public static async Task<Listing[]> GetAllJeeps()
        {
            var client = GetClient();
            var collection = await GetCollection(client, Keys.ListingsDbName, Keys.ListingDbCollectionName);
            string sql = String.Format("SELECT * FROM {0}", Keys.ListingDbCollectionName);
            var jeepsQuery = client.CreateDocumentQuery<Listing>(collection.SelfLink, sql).ToArray();
            var jeeps = jeepsQuery.ToArray();

            return jeeps;
        }

        public static async Task<Listing[]> GetHardTops()
        {
            var client = GetClient();
            var collection = await GetCollection(client, Keys.ListingsDbName, Keys.ListingDbCollectionName);

            string sql = String.Format(@"SELECT l.Color, l.Options, l.Package, l.Type, l.Image, l.Dealer, l.Id 
                FROM {0} l 
                    JOIN o IN l.Options
                WHERE o.Name = 'hard top'", Keys.ListingDbCollectionName);
            var hardtopQuery = client.CreateDocumentQuery<Listing>(collection.SelfLink, sql).ToArray();
            var hardtops = hardtopQuery.ToArray();

            return hardtops;
        }

        public static async Task<Listing[]> GetRubicons()
        {
            var client = GetClient();
            var collection = await GetCollection(client, Keys.ListingsDbName, Keys.ListingDbCollectionName);

            string sql = String.Format("SELECT * FROM {0} l WHERE l.Package = 'rubicon'", Keys.ListingDbCollectionName);
            var rubiconQuery = client.CreateDocumentQuery<Listing>(collection.SelfLink, sql).ToArray();
            var rubicons = rubiconQuery.ToArray();

            return rubicons;
        }

        public static async Task<DocumentCollection> GetCollection(DocumentClient client, string dbname, string collectionName)
        {
            //get the database
            var db = await RetrieveOrCreateDatabaseAsync(Keys.ListingsDbName);
            Console.Write("DB SelfLink: ");
            Console.WriteLine((string)db.SelfLink);

            //get the collection
            var collection = await RetrieveOrCreateCollectionAsync(db.SelfLink, Keys.ListingDbCollectionName);
            Console.WriteLine("Collection self link: ");
            Console.WriteLine((string)collection.SelfLink);

            return collection;
        }

        public static DocumentClient GetClient()
        {
            var client = new DocumentClient(new Uri(Keys.ListingsDbUri), Keys.ListingsDbPrimaryKey);
            return client;
        }

        private static async Task<Database> RetrieveOrCreateDatabaseAsync(string id)
        {
            // Try to retrieve the database (Microsoft.Azure.Documents.Database) whose Id is equal to databaseId            
            var database = GetClient().CreateDatabaseQuery().Where(db => db.Id == id).AsEnumerable().FirstOrDefault();

            // If the previous call didn't return a Database, it is necessary to create it
            if (database == null)
            {
                database = await GetClient().CreateDatabaseAsync(new Database { Id = id });
                Console.WriteLine("Created Database: id - {0} and selfLink - {1}", database.Id, database.SelfLink);
            }

            return database;
        }

        private static async Task<DocumentCollection> RetrieveOrCreateCollectionAsync(string databaseSelfLink, string id)
        {
            // Try to retrieve the collection (Microsoft.Azure.Documents.DocumentCollection) whose Id is equal to collectionId
            var collection = GetClient().CreateDocumentCollectionQuery(databaseSelfLink).Where(c => c.Id == id).ToArray().FirstOrDefault();

            // If the previous call didn't return a Collection, it is necessary to create it
            if (collection == null)
            {
                collection = await GetClient().CreateDocumentCollectionAsync(databaseSelfLink, new DocumentCollection { Id = id });
            }

            return collection;
        }

        public static async Task DeleteDatabase(string name)
        {
            Database database = await RetrieveOrCreateDatabaseAsync(name);
            var client = GetClient();
            Console.WriteLine("Attempting to delete " + name);
            await client.DeleteDatabaseAsync(database.SelfLink);
            Console.WriteLine("Database deleted");
        }

        public static async Task CreateDatabase(Listing[] listings)
        {
            var client = GetClient();

            Database database = await RetrieveOrCreateDatabaseAsync(Keys.ListingsDbName);

            Console.Write("Self link for new database: ");
            Console.WriteLine((string) database.SelfLink);

            DocumentCollection documentCollection = await RetrieveOrCreateCollectionAsync(database.SelfLink, Keys.ListingDbCollectionName);

            foreach (Listing listing in listings)
            {
                await client.CreateDocumentAsync(documentCollection.SelfLink, listing);
            }
        }
    }
}