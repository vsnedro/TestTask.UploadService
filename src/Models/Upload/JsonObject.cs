namespace json_api_test.Models.Upload;

internal class JsonObject
{
    internal class JsonToken
    {
        public bool Started { get; set; } = false;
        public bool Ended { get; set; } = false;
        public string? Value { get; set; }

        public void Reset()
        {
            Started = false;
            Ended = false;
            Value = default;
        }
    }

    public JsonToken Key { get; } = new();
    public JsonToken Value { get; } = new();

    public void Reset()
    {
        Key.Reset();
        Value.Reset();
    }
}
