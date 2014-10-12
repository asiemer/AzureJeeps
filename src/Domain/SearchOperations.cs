using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedDog.Search;
using RedDog.Search.Http;
using RedDog.Search.Model;

namespace Domain
{
    public class SearchOperations
    {
        public static IndexManagementClient GetIndexManagementClient()
        {
            return new IndexManagementClient(ApiConnection.Create(Keys.ListingsServiceName, Keys.ListingsServiceKey));
        }

        public static async Task DeleteSearchIndex()
        {
            var manager = GetIndexManagementClient();
            await manager.DeleteIndexAsync(Keys.ListingsServiceIndexName);
        }

        public static async Task<IApiResponse<Index>> RetrieveOrCreateIndexAsync()
        {
            var managementClient = GetIndexManagementClient();

            IApiResponse<Index> index = new ApiResponse<Index>();
            try
            {
                index = await managementClient.GetIndexAsync(Keys.ListingsServiceIndexName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Index doesn't yet exist, create it");
            }

            if (index.Body == null)
            {
                var newIndex = new Index(Keys.ListingsServiceIndexName)
                   .WithStringField("Id", opt => opt.IsKey().IsRetrievable())
                   .WithStringField("Color", opt => opt.IsSearchable().IsSortable().IsFilterable().IsRetrievable().IsFacetable())
                   .WithStringField("Package", opt => opt.IsSearchable().IsFilterable().IsRetrievable().IsFacetable())
                   .WithStringField("Options", opt => opt.IsSearchable().IsFilterable().IsRetrievable())
                   .WithStringField("Type", opt => opt.IsSearchable().IsFilterable().IsRetrievable().IsFacetable())
                   .WithStringField("Image", opt => opt.IsRetrievable());

                //var sp = new ScoringProfile();
                //sp.Name = "ByTypeAndPackage";
                //sp.Text = new ScoringProfileText();
                //sp.Text.Weights = new Dictionary<string, double>();
                //sp.Text.Weights.Add("Type", 1.5);
                //sp.Text.Weights.Add("Package", 1.5);
                //newIndex.ScoringProfiles.Add(sp);

                //var colorsSp = new ScoringProfile();
                //colorsSp.Name = "ByColor";
                //colorsSp.Text = new ScoringProfileText();
                //colorsSp.Text.Weights = new Dictionary<string,double>();
                //colorsSp.Text.Weights.Add("Color",2);
                //newIndex.ScoringProfiles.Add(colorsSp);


                index = await managementClient.CreateIndexAsync(newIndex);

                if (!index.IsSuccess)
                {
                    Console.WriteLine("Error when making index");
                }
            }

            return index;
        }

        public static async Task AddRecordsToIndex(Listing[] listings)
        {
            //determine batch size
            if (listings.Length < 1000)
            {
                await AddBatchToIndex(listings);
            }
            //else break up list into chunks of 1000
            else
            {
                for (int i = 0; i < (listings.Length / 1000); i++)
                {
                    await AddBatchToIndex(listings.Skip(i).Take(1000).ToArray());
                }
            }
        }

        private static async Task AddBatchToIndex(Listing[] listings)
        {
            //guard against more than 1000
            if (listings.Length < 1000)
            {
                var managementClient = GetIndexManagementClient();
                List<IndexOperation> operations = new List<IndexOperation>();

                //rip through collection and make a batch of operations
                foreach (Listing l in listings)
                {
                    var flatOptions = string.Concat((IEnumerable<string>) l.Options.Select(o => o.Name));
                    var op = new IndexOperation(IndexOperationType.Upload, "Id", l.Id.ToString())
                        .WithProperty("Color", l.Color)
                        .WithProperty("Options", flatOptions)
                        .WithProperty("Package", l.Package)
                        .WithProperty("Type", l.Type)
                        .WithProperty("Image", l.Image);
                    operations.Add(op);
                }

                //TODO: should be able to batch up the operations
                var result = await managementClient.PopulateAsync(Keys.ListingsServiceIndexName, operations.ToArray());
                if (!result.IsSuccess)
                {
                    Console.WriteLine("Adding records to the index failed!");
                }
            }
        }
        public static async Task<IApiResponse<SearchQueryResult>> Search(string search)
        {
            return await Search(search, null);
        }

        public static async Task<IApiResponse<SearchQueryResult>> Search(string search, string[] facets)
        {
            var conn = ApiConnection.Create(Keys.ListingsServiceUrl, Keys.ListingsServiceKey);
            var queryClient = new IndexQueryClient(conn);
            var query = new SearchQuery(search)
                .Count(true)
                .Select("Id,Color,Options,Type,Package,Image")
                .OrderBy("Color");

            if (facets != null)
            {
                StringBuilder facet = new StringBuilder();
                foreach (string f in facets)
                {
                    facet.Append(f + ",");
                }
                string facetResult = facet.ToString();
                //trim trailing comma
                facetResult = facetResult.Substring(0, facetResult.Count() - 1);
                query.Facet(facetResult);
            }
            var searchResults = await queryClient.SearchAsync(Keys.ListingsServiceIndexName, query);

            return searchResults;
        }

        public static void PrintResults(IApiResponse<SearchQueryResult> searchResults)
        {
            if (searchResults.IsSuccess)
                {
                    List<SearchQueryRecord> results = searchResults.Body.Records.ToList();
                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine();
                        Console.WriteLine(String.Format((string) "\tScore: {0} ", (object) results[i].Score));
                        foreach (var prop in results[i].Properties)
                        {
                            Console.WriteLine("\t" + prop.Key + " " + prop.Value);
                        }
                    }

                    Console.WriteLine();
                }
        }
    }
}
