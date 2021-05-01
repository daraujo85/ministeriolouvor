using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioLouvor.Respository
{
    public class MusicaRepository : BaseRepository<Musica>, IMusicaRepository
    {
        public MusicaRepository(IMongoContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Musica>> GetByTom(string tom)
        {
            var data = await GetByFilter(x => x.Tom == tom.ToUpper());

            return data;
        }
        public async Task<IEnumerable<Musica>> GetByTitulo(string titulo)
        {
            var data = await GetByFilter(x => x.Titulo.ToLower().Contains(titulo.ToLower()));

            return data;
        }

        public async Task<IEnumerable<Musica>> GetByTag(string tag)
        {
            var data = await GetByFilter(x => x.Tags.Any(t => t.ToLower().Contains(tag)));

            return data;
        }

        public async Task<IEnumerable<Video>> YoutubeSearch(string titulo)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyDC-NHQjyVTOcwVueXdv50Vbe3QN7GnuZw",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = titulo; // Replace with your search term.
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            var videos = new List<Video>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items.Where(x => x.Id.Kind == "youtube#video"))
            {
                videos.Add(new Video
                {
                    Titulo = searchResult.Snippet.Title,
                    Thumbnail = $"https://img.youtube.com/vi/{searchResult.Id.VideoId}/0.jpg",
                    Url = $"https://www.youtube.com/watch?v={searchResult.Id.VideoId}"
                });
            }

            return videos;

        }
    }
}
