using System.ComponentModel.DataAnnotations;

namespace json_api_test.Models.Upload;

public class UploadRequestDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public byte[] Json { get; set; }

    [Required]
    public byte[] File { get; set; }
}
