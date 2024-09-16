namespace Blazor.WebApp
{
    public class NavItemModel
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public bool HasDropdown { get; set; }
        public List<NavItemModel> SubItems { get; set; } = new();


        // public string CollapseId { get; set; } = string.Empty;
        // public string Title { get; set; } = string.Empty;
        // public string IconClass { get; set; } = string.Empty;
        // public List<FuncPage> FuncPages = new();
    }

    public class FuncPage
    {
        public string FuncName { get; set; } = string.Empty;
        public string FuncEndpoint { get; set; } = string.Empty;
    }
}