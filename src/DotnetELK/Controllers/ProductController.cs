using DotnetELK.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace DotnetELK.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IElasticClient elasticClient, 
        ILogger<ProductController> logger)
    {
        _elasticClient = elasticClient;
        _logger = logger;
    }

    [HttpGet(Name = "GetProducts")]
    public async Task<IActionResult> Get(string keyword)
    {
        var results = await _elasticClient.SearchAsync<Product>(
            s => s.Query(
                    q => q.QueryString(
                        d => d.Query('*' + keyword + '*')))
                .Size(1000));
        
        _logger.LogInformation("Search results {@results}", results.Documents);
        
        return Ok(results.Documents.ToList()); // deserialize it since a nested object is returned
    }

    [HttpPost(Name = "AddProduct")]
    public async Task<IActionResult> Post(Product product)
    {
        await _elasticClient.IndexDocumentAsync(product);
        _logger.LogInformation("Product added {@product}", product);
        
        return Ok();
    }
}