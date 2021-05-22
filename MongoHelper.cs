using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brainfuck
{
    public static class MongoHelper
    {

        #region Add

        public static BsonDocument AddDefaultGuild(ulong guildID)
        {
            var guild = BuildGuild(guildID);
            Config.DB.GetCollection<BsonDocument>("Guilds").InsertOne(document: guild);
            return guild;
        }

        #endregion Add

        #region Builds

        private static BsonDocument BuildGuild(ulong guildID)
        {
            return new BsonDocument
            {
                // Guild ID
                { "_id", (long)guildID },
                // Prefix used in that guild
                { "Prefix", Config.Prefix }
            };
        }

        #endregion Builds

        #region Update

        public static void UpdateGuildPrefix(ulong guildID, string prefix)
        {
            var filter = EqFilter("_id", (long)guildID);
            var update = Builders<BsonDocument>.Update.Set("Prefix", prefix);

            Config.DB.GetCollection<BsonDocument>("Guilds").UpdateOne(filter, update);
        }

        #endregion Update

        #region Get

        public static BsonDocument GetGuild(ulong guildID)
        {
            if (!Exists("Guilds", (long)guildID)) AddDefaultGuild(guildID);

            return Config.DB.GetCollection<BsonDocument>("Guilds").Find(x => x["_id"] == (long)guildID).First();
        }

        public static string GetPrefix(ulong guildID) => GetGuild(guildID)["Prefix"].AsString;

        public static bool Exists(string collection, object ID, string category = "_id")
        {
            try
            {
                var filter = EqFilter(category, ID);
                var count = Config.DB.GetCollection<BsonDocument>(collection).Find(filter).CountDocuments();

                return count > 0 ? true : false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Something wrong: " + e.Message);
                throw;
            }
        }

        #endregion Get

        private static FilterDefinition<BsonDocument> EqFilter(string field, object value)
        {
            return Builders<BsonDocument>.Filter.Eq(field, value);
        }
    }
}
