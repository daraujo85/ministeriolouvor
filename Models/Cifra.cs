using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MinisterioLouvor.Context;

namespace MinisterioLouvor.Models
{
    [BsonCollection("cifras")]
    public class Cifra
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("conteudo")]
        public string Conteudo { get; set; }

        [BsonElement("musicaId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MusicaId { get; set; }
    }
}
