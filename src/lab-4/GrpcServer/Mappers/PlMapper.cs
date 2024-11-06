using Task3.Bll.Dtos.ProductDtos;

namespace GrpcServer.Mappers;

public class PlMapper
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