﻿using System;
using System.Threading.Tasks;
using Domain;

namespace DataGenerator
{
    public class Program
    {
        

        static void Main(string[] args)
        {
            try
            {
                RunAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
                Console.WriteLine(e.InnerException.InnerException.Message);
            }
            Console.ReadLine();
        }

        private static async Task RunAsync()
        {
            //deletes
            bool deleteDocDb = false;
            bool deleteIndex = false;
            
            //creates
            bool generateDocDb = false;
            bool generateIndex = true;

            //search
            bool runDbQuery = false;
            bool runSearch = false;

            //generate X number of listings locally
            int numberOfListingsToCreate = 100;
            DataFactory f = new DataFactory();
            Listing[] listings = f.CreateListings(numberOfListingsToCreate);


            if (deleteDocDb)
            {
                await DocumentDbOperations.DeleteDatabase(Keys.ListingsDbName);
            }

            if (deleteIndex)
            {
                await SearchOperations.DeleteSearchIndex();
            }

            if (generateDocDb)
            {
                //create the datastore and persist the listings
                Console.WriteLine("Creating the database");
                DocumentDbOperations.CreateDatabase(listings).Wait();
                Console.WriteLine("Database created");
            }

            if (generateIndex)
            {
                //create the index and persist the listings
                Console.WriteLine("Creating the index");
                var createIndexResult = await SearchOperations.RetrieveOrCreateIndexAsync();
                if (createIndexResult.IsSuccess)
                {
                    Console.WriteLine("Index created!");
                }
                else
                {
                    Console.WriteLine("Index creation failed!");
                }
                await SearchOperations.AddRecordsToIndex(listings);
            }

            if (runDbQuery)
            {
                //DocumentDB direct query
                //var result = await DocumentDbOperations.GetRubicons();
                var result = await DocumentDbOperations.GetHardTops();
                foreach (Listing rubicon in result)
                {
                    Console.WriteLine(String.Format("Package: {0} Color: {1} Dealer: {2}",
                        rubicon.Package, rubicon.Color, rubicon.Dealer.name));
                    foreach (Option option in rubicon.Options)
                    {
                        Console.WriteLine("\t" + option.Name);
                    }
                    Console.WriteLine("");
                }
            }

            if (runSearch)
            {
                string[] facets = new[]
                {
                    "Type",
                    "Package",
                    "Color"
                };

                var search = await SearchOperations.Search("Wrangler Rubicon hard top firecracker red");
                //var search = await SearchOperations.Search("Wrangler Rubicon hard top firecracker red", facets);
                //var search = await SearchOperations.Search("Sport soft top ampd");

                Console.WriteLine("Found " + search.Body.Count);
                SearchOperations.PrintResults(search);
            }
        }
    }
}
