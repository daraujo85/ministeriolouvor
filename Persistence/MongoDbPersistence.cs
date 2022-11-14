using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using System;

namespace MinisterioLouvor.Persistence
{
  public static class MongoDbPersistence
    {
        [Obsolete]
        public static void Configure()
        {
            MusicaMap.Configure();
            CifraMap.Configure();

            // Set Guid to CSharp style (with dash -)
            BsonDefaults.GuidRepresentation = GuidRepresentation.CSharpLegacy;
            // Conventions
            var pack = new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfDefaultConvention(true)
                };
            ConventionRegistry.Register("My Solution Conventions", pack, t => true);
        }
    }
}
