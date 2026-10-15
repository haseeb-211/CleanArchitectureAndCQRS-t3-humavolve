using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Products.Application.DTOs;

namespace Products.Tests.Integration;

public sealed class ProductsApiTests : IClassFixture<ProductsApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ProductsApiFactory _factory;
    private readonly HttpClient _client;

    public ProductsApiTests(ProductsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithProducts_ReturnsAllProducts()
    {
        await _factory.ResetAsync();
        await CreateProductAsync("Laptop", 25000.00m);
        await CreateProductAsync("Keyboard", 1200.00m);

        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>(Json);
        Assert.NotNull(products);
        Assert.Equal(2, products.Count);
        Assert.Contains(products, product => product.Name == "Laptop" && product.Price == 25000.00m);
        Assert.Contains(products, product => product.Name == "Keyboard" && product.Price == 1200.00m);
    }

    [Fact]
    public async Task GetAll_WithoutProducts_ReturnsEmptyCollection()
    {
        await _factory.ResetAsync();

        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>(Json);
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var response = await _client.GetAsync($"/api/products/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);
        Assert.Equal(created.Id, product.Id);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal(25000.00m, product.Price);
    }

    [Fact]
    public async Task GetById_UnknownProduct_Returns404()
    {
        await _factory.ResetAsync();

        var response = await _client.GetAsync("/api/products/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_InvalidId_Returns400(int id)
    {
        await _factory.ResetAsync();

        var response = await _client.GetAsync($"/api/products/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("id", errors.Keys);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("one")]
    public async Task GetById_NonNumericId_Returns400(string id)
    {
        await _factory.ResetAsync();

        var response = await _client.GetAsync($"/api/products/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidProduct_Returns201WithLocationAndBody()
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = "Laptop", price = 25000.00m },
            Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith("/api/products/1", response.Headers.Location.ToString());

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal(25000.00m, product.Price);
    }

    [Fact]
    public async Task Create_ThenGet_ReturnsCreatedResourceFromLocation()
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = "Mouse", price = 450.50m },
            Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var locationResponse = await _client.GetAsync(response.Headers.Location!.ToString());

        Assert.Equal(HttpStatusCode.OK, locationResponse.StatusCode);

        var product = await locationResponse.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);
        Assert.Equal("Mouse", product.Name);
        Assert.Equal(450.50m, product.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyName_Returns400(string name)
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name, price = 100.00m },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("name", errors.Keys);
    }

    [Fact]
    public async Task Create_MissingName_Returns400()
    {
        await _factory.ResetAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
        {
            Content = new StringContent("""{"price":100}""", Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("name", errors.Keys);
    }

    [Fact]
    public async Task Create_NameLongerThan100Characters_Returns400()
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = new string('a', 101), price = 100.00m },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("name", errors.Keys);
    }

    [Fact]
    public async Task Create_ZeroPrice_Returns400()
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = "Keyboard", price = 0 },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("price", errors.Keys);
    }

    [Fact]
    public async Task Create_NegativePrice_Returns400()
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = "Keyboard", price = -99.99m },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("price", errors.Keys);
    }

    [Fact]
    public async Task Create_InvalidPayload_DoesNotPersistProduct()
    {
        await _factory.ResetAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = "", price = -1m },
            Json);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var all = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products", Json);
        Assert.NotNull(all);
        Assert.Empty(all);
    }

    [Fact]
    public async Task Update_ExistingProduct_Returns200WithUpdatedProduct()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{created.Id}",
            new { name = "Updated Laptop", price = 27000.00m },
            Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);
        Assert.Equal(created.Id, product.Id);
        Assert.Equal("Updated Laptop", product.Name);
        Assert.Equal(27000.00m, product.Price);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/products/{created.Id}",
            new { name = "Updated Laptop", price = 27000.00m },
            Json);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);
        Assert.Equal("Updated Laptop", product.Name);
        Assert.Equal(27000.00m, product.Price);
    }

    [Fact]
    public async Task Update_UnknownProduct_Returns404()
    {
        await _factory.ResetAsync();

        var response = await _client.PutAsJsonAsync(
            "/api/products/424242",
            new { name = "Updated Laptop", price = 27000.00m },
            Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_DoesNotCreateMissingProduct()
    {
        await _factory.ResetAsync();

        var response = await _client.PutAsJsonAsync(
            "/api/products/424242",
            new { name = "Updated Laptop", price = 27000.00m },
            Json);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var all = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products", Json);
        Assert.NotNull(all);
        Assert.Empty(all);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_InvalidId_Returns400(int id)
    {
        await _factory.ResetAsync();

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{id}",
            new { name = "Updated Laptop", price = 27000.00m },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("id", errors.Keys);
    }

    [Theory]
    [InlineData("", 100.00)]
    [InlineData("   ", 100.00)]
    [InlineData("Valid", 0)]
    [InlineData("Valid", -5)]
    public async Task Update_InvalidPayload_Returns400(string name, double price)
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{created.Id}",
            new { name, price = (decimal)price },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_NameLongerThan100Characters_Returns400()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{created.Id}",
            new { name = new string('a', 101), price = 27000.00m },
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("name", errors.Keys);
    }

    [Fact]
    public async Task Update_BodyIdCannotOverrideRouteId()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{created.Id}",
            new { id = 999999, name = "Updated Laptop", price = 27000.00m },
            Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);
        Assert.Equal(created.Id, product.Id);
        Assert.NotEqual(999999, product.Id);
    }

    [Fact]
    public async Task Delete_ExistingProduct_Returns204WithoutBody()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var response = await _client.DeleteAsync($"/api/products/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Delete_ThenGetById_Returns404()
    {
        await _factory.ResetAsync();
        var created = await CreateProductAsync("Laptop", 25000.00m);

        var deleteResponse = await _client.DeleteAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var all = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products", Json);
        Assert.NotNull(all);
        Assert.Empty(all);
    }

    [Fact]
    public async Task Delete_UnknownProduct_Returns404()
    {
        await _factory.ResetAsync();

        var response = await _client.DeleteAsync("/api/products/424242");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Delete_InvalidId_Returns400(int id)
    {
        await _factory.ResetAsync();

        var response = await _client.DeleteAsync($"/api/products/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await ReadErrorsAsync(response);
        Assert.Contains("id", errors.Keys);
    }

    [Fact]
    public async Task Swagger_Document_IsAvailable()
    {
        await _factory.ResetAsync();

        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Products API", document.RootElement.GetProperty("info").GetProperty("title").GetString());
    }

    private async Task<ProductDto> CreateProductAsync(string name, decimal price)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { name, price },
            Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(Json);
        Assert.NotNull(product);

        return product;
    }

    private static async Task<IReadOnlyDictionary<string, string[]>> ReadErrorsAsync(HttpResponseMessage response)
    {
        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = payload.RootElement.GetProperty("errors");

        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in errors.EnumerateObject())
        {
            result[property.Name] = property.Value
                .EnumerateArray()
                .Select(value => value.GetString() ?? string.Empty)
                .ToArray();
        }

        return result;
    }
}
