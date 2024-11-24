using Bll.Services;
using Grpc.Core;
using GrpcServer.Mappers;

namespace GrpcServer.Services;

public class ProductController : ProductsService.ProductsServiceBase
{
    private readonly ProductService _productService;
    private readonly ProductMapper _productMapper;

    public ProductController(
        ProductService productService,
        ProductMapper productMapper)
    {
        _productService = productService;
        _productMapper = productMapper;
    }

    public override async Task<CreateProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        await _productService.CreateProduct(_productMapper.ToProductCreationDto(request), context.CancellationToken);

        return new CreateProductResponse();
    }
}