using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeCategories.Repository;

namespace YoutubeCategories.ReconcileVideoCategories
{
    public class Reconcile
    {
        private readonly YouTubeService _youTubeService;
        private readonly ytvideoContext _context;
        private const string YoutubeVideoUrlFormat = "https://www.youtube.com/watch?v=";
        public Reconcile(ytvideoContext context, YouTubeService youTubeService)
        {
            _context = context;
            _youTubeService = youTubeService;
        }

        [FunctionName("Reconcile")]
        public async Task Run([TimerTrigger("0 0 0 */2 * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {       
            log.LogInformation($"Reconcile function started at: {DateTime.Now}");
            var regionsEntityCollection = _context.Regions.ToList();
            var categoriesEntityCollection = _context.Categories.ToList();
            var videosEntityCollection = _context.Videos.ToList();

            foreach (var region in regionsEntityCollection)
            {
                var videoCategories = await GetVideoCategories(region.RegionCode);
                if (videoCategories != null) {
                    foreach (var videoCategory in videoCategories.Items)
                    {
                        var videos = await GetVideos(videoCategory.Id, region.RegionCode);

                        if (videos != null && videos.Items != null && videos.Items.Any())
                        {
                            var currentCategory = categoriesEntityCollection.FirstOrDefault(x => x.CategoryName.Equals(videoCategory.Snippet.Title) && x.RegionId == region.Id);

                            if (currentCategory is null)
                            {
                                currentCategory = new Categories
                                {
                                    RegionId = region.Id,
                                    CategoryName = videoCategory.Snippet.Title
                                };

                                _context.Categories.Add(currentCategory);
                                _context.SaveChanges();
                            }                            

                            var currentCategoryVideos = videosEntityCollection.Where(v => v.CategoryId == currentCategory.Id);
                            var videoObjects = new List<Videos>();

                            if (currentCategoryVideos == null || !currentCategoryVideos.Any())
                            {
                                foreach (var video in videos.Items)
                                {
                                    var videoObj = new Videos
                                    {
                                        CategoryId = currentCategory.Id,
                                        Url = $"{YoutubeVideoUrlFormat}{video.Id}",
                                        Name = video.Snippet.Title
                                    };
                                    videoObjects.Add(videoObj);
                                }
                            }
                            else
                            {
                                var currentUrlsInDb = new HashSet<string>(currentCategoryVideos.Select(v=>v.Url.Replace(YoutubeVideoUrlFormat, string.Empty)));
                                //Get New Videos
                                var newVideos = videos.Items.Where(vi => !currentUrlsInDb.Contains(vi.Id));
                                //Get videos to Remove
                                var currentNames = new HashSet<string>(videos.Items.Select(vi => vi.Id));
                                var videosToRemove = currentCategoryVideos.Where(v => !currentNames.Contains(v.Url.Replace(YoutubeVideoUrlFormat, string.Empty))).ToList();

                                if (videosToRemove.Any())
                                {
                                    _context.Videos.RemoveRange(videosToRemove);
                                }
                                if (newVideos.Any())
                                {
                                    foreach (var video in newVideos)
                                    {
                                        var videoObj = new Videos
                                        {
                                            CategoryId = currentCategory.Id,
                                            Url = $"{YoutubeVideoUrlFormat}{video.Id}",
                                            Name = video.Snippet.Title
                                        };
                                        videoObjects.Add(videoObj);
                                    }
                                }
                            }

                            if (videoObjects.Any())
                            {
                                _context.AddRange(videoObjects);
                            }

                            _context.SaveChanges();
                        }
                    }
                }                
            }
            log.LogInformation($"Reconcile function ended at: {DateTime.Now}");
        }

        public async Task<VideoCategoryListResponse> GetVideoCategories(string region)
        {
            var videoCategoriesRequest = _youTubeService.VideoCategories.List("snippet");
            videoCategoriesRequest.RegionCode = region;

            // Call the search.list method to retrieve results matching the specified query term.
            VideoCategoryListResponse videoCategories = null;

            try
            {
                videoCategories= await videoCategoriesRequest.ExecuteAsync();
            }
            catch (Exception)
            {

            }

            return videoCategories;
        }

        public async Task<VideoListResponse> GetVideos(string category, string region)
        {
            var videosRequest = _youTubeService.Videos.List("snippet");
            videosRequest.VideoCategoryId = category;
            videosRequest.RegionCode = region;
            videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;

            // Call the search.list method to retrieve results matching the specified query term.
            VideoListResponse videos = null;

            try
            {
                videos = await videosRequest.ExecuteAsync();
            }
            catch (Exception)
            {

            }
            return videos;
        }
    }
}
