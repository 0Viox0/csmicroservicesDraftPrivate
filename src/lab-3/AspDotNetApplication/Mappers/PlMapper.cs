using Task1.Models;
using Task3.Bll.Dtos.ProductDtos;

namespace Task1.Mappers;

public class PlMapper
{
    public ProductCreationDto ToProductCreationDto(ProductCreationModel productCreationModel)
    {
        return new ProductCreationDto
        {
            Name = productCreationModel.Name,
            Price = productCreationModel.Price,
        };
    }
}