using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.OrderAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductService(IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            // This method is not implemented in the provided code snippet.
            // You can implement it similarly to GetProductsAsync if needed.
            try
            {
                ProductDto result = new();

                // Create a named client for Product service same as Program.cs configuration
                var client = _httpClientFactory.CreateClient("Product");
                var response = await client.GetAsync($"/api/product/{productId}");
                if (!response.IsSuccessStatusCode)
                {
                    //throw new Exception($"Error fetching products: {response.ReasonPhrase}");
                    var debugger = new Exception($"Error fetching products: {response.ReasonPhrase}");
                    return null;
                }

                var apiContent = await response.Content.ReadAsStringAsync();


                if (string.IsNullOrEmpty(apiContent))
                {
                    return null;
                    //return Enumerable.Empty<ProductDto>();
                }
                var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

                if (resp == null || !resp.IsSuccess || resp.Result is null)
                {
                    //throw new Exception($"Error fetching products: {resp?.Message}");
                    //return Enumerable.Empty<ProductDto>();
                    return null;
                }
                else
                {
                    result = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(resp.Result)) ?? null;

                }
                return result;

            }
            catch (Exception ex)
            {
                //throw new Exception($"Error fetching products: {ex.Message}");
                return null;

            }

        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            var result = Enumerable.Empty<ProductDto>();
            try
            {
                // Create a named client for Product service same as Program.cs configuration
                var client = _httpClientFactory.CreateClient("Product");
                var response = await client.GetAsync("/api/product");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error fetching products: {response.ReasonPhrase}");
                }

                var apiContent = await response.Content.ReadAsStringAsync();


                if (string.IsNullOrEmpty(apiContent))
                {
                    return result;
                    //return Enumerable.Empty<ProductDto>();
                }
                var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

                if (resp == null || !resp.IsSuccess || resp.Result is null)
                {
                    //throw new Exception($"Error fetching products: {resp?.Message}");
                    //return Enumerable.Empty<ProductDto>();
                    return result;
                }
                else
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(resp.Result)) ?? Enumerable.Empty<ProductDto>();

                }

                return result;

            }
            catch (Exception ex)
            {
                //throw new Exception($"Error fetching products: {ex.Message}");
                return result;

            }
        }
    }
}
