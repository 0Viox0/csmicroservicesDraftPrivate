using Bll.Dtos.ProductDtos;

namespace GrpcServer.Mappers;

public class ProductMapper
{
    public ProductCreationDto ToProductCreationDto(CreateProductRequest productCreationModel)
    {
        return new ProductCreationDto
        {
            Name = productCreationModel.Name,
            Price = productCreationModel.Price.DecimalValue,
        };
    }
}