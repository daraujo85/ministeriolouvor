using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace POCMinisterioLouvor.Models
{
   public class Cifra
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}
