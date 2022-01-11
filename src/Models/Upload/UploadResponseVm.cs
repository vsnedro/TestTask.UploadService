namespace json_api_test.Models.Upload;

public class UploadResponseVm
{
    /// <summary>
    /// The return name of the upload.
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// Json upload location.
    /// </summary>
    public string? data { get; set; }

    /// <summary>
    /// File upload location.
    /// </summary>
    public string? content { get; set; }
}
