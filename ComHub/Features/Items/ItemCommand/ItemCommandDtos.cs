using System.ComponentModel.DataAnnotations;
using ComHub.Features.Items.ItemQuery;

namespace ComHub.Features.Items.ItemCommand;

public class CreateItemRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public ICollection<int> CategoryIds { get; set; } = [];
}

public class AddEditCategoriesRequest
{
    public List<CategoryModel> Categories { get; set; } = [];
}
