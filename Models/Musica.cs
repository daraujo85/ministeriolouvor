﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace POCMinisterioLouvor.Models
{
    public class Musica
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("titulo")]
        public string Titulo { get; set; }

        [BsonElement("tom")]
        public string Tom { get; set; }

        [BsonElement("vozPrincipal")]
        public string VozPrincipal { get; set; }

        [BsonElement("tags")]
        public string[] Tags { get; set; }

        [BsonElement("dificuldade")]
        public string Dificuldade { get; set; }

        [BsonElement("linkVideo")]
        public string LinkVideo { get; set; }

        [BsonElement("linkCifra")]
        public string LinkCifra { get; set; }

        [BsonElement("linkLetra")]
        public string LinkLetra { get; set; }


    }
}