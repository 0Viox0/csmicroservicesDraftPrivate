using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Mappers;
using Task3.Bll.Services;

namespace GrpcServer.Services;

public class ProductGrpcService : ProductsService.ProductsServiceBase
{
    private readonly ProductService _productService;
    private readonly PlMapper _plMapper;

    public ProductGrpcService(
        ProductService productService,
        PlMapper plMapper)
    {
        _productService = productService;
        _plMapper = plMapper;
    }

    public override async Task<Empty> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        await _productService.CreateProduct(_plMapper.ToProductCreationDto(request), context.CancellationToken);

        return new Empty();
    }
}