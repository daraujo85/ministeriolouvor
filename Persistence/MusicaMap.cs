using MongoDB.Bson.Serialization;
using MinisterioLouvor.Models;

namespace MinisterioLouvor.Persistence
{
    public class MusicaMap
    {
        public static void Configure()
        {
            BsonClassMap.RegisterClassMap<Musica>(map =>
            {
                map.AutoMap();                
                map.SetIgnoreExtraElements(true);                
                map.MapIdMember(x => x.Id);
                map.MapMember(x => x.Titulo).SetIsRequired(true);
            });
        }
    }
}
