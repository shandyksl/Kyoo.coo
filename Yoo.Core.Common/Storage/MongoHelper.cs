using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Storage
{
    public class MongoHelper 
    {
        private readonly IMongoDatabase _db;

        public MongoHelper(string connectionString, string dbName)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                var client = new MongoClient(connectionString);
                _db = client.GetDatabase(dbName);
            }
        }

        async public Task InsertAsync(string collectionName, string jsonStr, bool addDateinsert = false, string dateinsertParam = "dateinsert")
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var document = BsonSerializer.Deserialize<BsonDocument>(jsonStr);
            if (addDateinsert)
                document.Add(dateinsertParam, DateTime.UtcNow.ToLocalTime());
            await collection.InsertOneAsync(document);
        }

        async public Task<BsonDocument> FindAsync(string collectionName, string keyName, string value) 
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter;
            return  await collection.Find(filter.Eq(keyName, value)).FirstOrDefaultAsync();
        }

        async public Task<BsonDocument> FindAsync(string collectionName, Dictionary<string, int> filterDataList)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter;
            var allfilter = filter.Empty;

            foreach (var filterData in filterDataList)
            {
                string filterKey = filterData.Key;
                int filterValue = filterData.Value;

                var critetiaFilter = filter.Eq(filterKey, filterValue);
                allfilter = allfilter & critetiaFilter;

            }
            return await collection.Find(allfilter).Project("{_id: 0}").FirstOrDefaultAsync();
        }

        async public Task UpdateAsync(string collectionName, string findKey, string findValue, string replaceKey, string replaceValue)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var result = await collection.FindOneAndUpdateAsync(
                   Builders<BsonDocument>.Filter.Eq(findKey,findValue),
                   Builders<BsonDocument>.Update.Set(replaceKey,replaceValue)
                );
        }
        async public Task UpdateAsync(string collectionName, string findKey, string findValue, string replaceKey, int replaceValue)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var result = await collection.FindOneAndUpdateAsync(
                   Builders<BsonDocument>.Filter.Eq(findKey, findValue),
                   Builders<BsonDocument>.Update.Set(replaceKey, replaceValue)
                );
        }

        async public Task UpdateAsync(string collectionName, string findKey, string findValue, string replaceKey, List<object> replaceValue)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var result = await collection.FindOneAndUpdateAsync(
                   Builders<BsonDocument>.Filter.Eq(findKey, findValue),
                   Builders<BsonDocument>.Update.Set(replaceKey, replaceValue)
                );
        }

        async public Task UpdateAsync(string collectionName, string findKey, string findValue, string replaceKey, BsonArray replaceValue)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var result = await collection.FindOneAndUpdateAsync(
                   Builders<BsonDocument>.Filter.Eq(findKey, findValue),
                   Builders<BsonDocument>.Update.Set(replaceKey, replaceValue)
                );
        }
        
        async public Task <List<BsonDocument>> FindAllAsync(string collectionName, Dictionary<string, string> betDetailsFilter)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter;
            var allfilter = filter.Empty;

            foreach (var betFilter in betDetailsFilter)
            {
                string filterKey = betFilter.Key;
                string filterValue = betFilter.Value;
               
                if (filterKey.Contains("gte") || filterKey.Contains("lte"))
                {
                    DateTime dateFilter = DateTime.Parse(filterValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);                 
                   
                    string[] splitKey = filterKey.Split("_");                  
                    if (splitKey[1] == "gte")
                    {                       
                        var critetiaFilter = filter.Gte(splitKey[0], dateFilter);
                        allfilter = allfilter & critetiaFilter;
                    }
                    if (splitKey[1] == "lte")
                    {                        
                        var critetiaFilter = filter.Lte(splitKey[0], dateFilter);
                        allfilter = allfilter & critetiaFilter;
                    }
                }               
                else
                {
                    var critetiaFilter = filter.Eq(filterKey, filterValue);
                    allfilter = allfilter & critetiaFilter;
                }
            }
            return await collection.Find(allfilter).Project("{_id: 0}").ToListAsync();           
        }

        public Task FindAsync(string v, Dictionary<string, string> filterdata)
        {
            throw new NotImplementedException();
        }
    }
}
