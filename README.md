A .NET Web API project that demonstrates integration with ElasticSearch for resource retrieval and searching functionalities.


Run this command to start the required containers:
```bash
docker compose up -d
```


**Walkthrough**

1. First, we setup the connection and index in the `appsettings.json` - the *index* is simply the item we wanna add to ElasticSearch(products in this case).

```json
{
  "ELKConfiguration": {
    "Uri": "http://localhost:9200",
    "index": "products"
  }
}
```

2. Then, we go ahead and create the entity for that index. in this case - a `Product`

3. After, we create some extensions for the index. This extension includes a `defaultmapping` which essentially tells ElasticSearch the properties of our resource that we want to ignore in our search. And then, we create an `ElasticClient` and register it for dependency injection - as a singleton.

```csharp
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
}
```

4. Next thing is to create the index itself. we have our entity, but ElasticSearch needs this index to be able to perform its functions. So when we start ElasticSearch it checks if we have that index in it, if not it creates a new index.

```csharp
private static void CreateIndex(IElasticClient client, string indexName)
    {
        client.Indices.Create(indexName, i => i
            .Map<Product>(m => m.AutoMap())
        );
    }
```

5. Then we create controller endpoints(refer from the code)
    - one to search for a product - using a query
    - one to add a product to elastic search


Note that, the response from our search will only include the fields we did not ignore.And we also cannot search by an ignored field. Since we ignored the `id`, `price` and `quantity` we will get `0` for those properties(Integer default). 