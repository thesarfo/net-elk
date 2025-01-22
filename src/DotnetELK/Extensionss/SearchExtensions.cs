using System.Data.Common;
using DotnetELK.Models;
using Nest;

namespace DotnetELK.Extensionss;

public static class SearchExtensions
{
    public static void AddElasticSearch(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var url = configuration["ELKConfiguration:Uri"];
        var defaultIndex = configuration["ELKConfiguration:index"];
        
        var settings = new ConnectionSettings(new Uri(url))
            .PrettyJson()
            .DefaultIndex(defaultIndex);
        
        AddDefaultMappings(settings);

        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
        
        CreateIndex(client, defaultIndex);
    }

    
    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings.DefaultMappingFor<Product>(p =>
            p.Ignore(x => x.Price)
                .Ignore(x => x.Id)
                .Ignore(x => x.Quantity));
    }
    
    private static void CreateIndex(IElasticClient client, string indexName)
    {
        client.Indices.Create(indexName, i => i
            .Map<Product>(m => m.AutoMap())
        );
    }
}