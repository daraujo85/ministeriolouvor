using MongoDB.Bson.Serialization;
using MinisterioLouvor.Models;

namespace MinisterioLouvor.Persistence
{
    public class CifraMap
    {
        public static void Configure()
        {
            BsonClassMap.RegisterClassMap<Cifra>(map =>
            {
                map.AutoMap();                
                map.SetIgnoreExtraElements(true);                
                map.MapIdMember(x => x.Id);
                map.MapMember(x => x.Conteudo).SetIsRequired(true);
            });
        }
    }
}
