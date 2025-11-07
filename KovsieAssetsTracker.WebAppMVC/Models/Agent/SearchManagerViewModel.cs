namespace KovsieAssetsTracker.WebAppMVC.Models.Agent
{
    public class SearchManagerViewModel
    {
        public string Query { get; set; } = "";
        public string TypeFilter { get; set; } = "All";

        public List<SearchResultItemViewModel> Results { get; set; } = new();
    }

    public class SearchResultItemViewModel
    {
        public string Type { get; set; } = "";
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Extra { get; set; } = "";
        public DateTime? CreatedDate { get; set; }
    }
}
