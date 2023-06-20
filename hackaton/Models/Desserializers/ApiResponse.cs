namespace frontend_hackaton.Models.Desserializers
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        public T classObject { get; set; }

        public bool Sucess { get; set; }

        public int statusCode { get; set; }
    }
}
