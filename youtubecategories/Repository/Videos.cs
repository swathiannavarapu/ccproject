namespace YoutubeCategories.Repository
{
    public class Videos
    {
        public long Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public long CategoryId { get; set; }
    }
}
