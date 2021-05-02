namespace MinisterioLouvor.Models
{
    public class Video
    {
        public string Id { get; internal set; }
        public string Titulo { get; set; }
        public string Descricao { get; internal set; }
        public string Url { get; set; }
        public string LargeThumbnail { get; internal set; }
        public string SmallThumbnail { get; internal set; }
       
    }

}
