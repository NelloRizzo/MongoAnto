using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using SharpCompress.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MongoUtils
{

    public class MongoConnection : IDisposable
    {
        private readonly MongoClient client;
        private const string collectioName = "files";
        private const string dbName = "tonii-dev";
        public MongoConnection() {
            const string connectionUri = "";
            var settings = MongoClientSettings.FromConnectionString(connectionUri);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            client = new MongoClient(settings);
        }


        public async Task Upload(string filePath) {
            try {
                var db = client.GetDatabase(dbName);
                var collection = db.GetCollection<MongoDocument>(collectioName);
                var chunksCollection = db.GetCollection<Chunk>("chunks");
                var f = File.ReadAllBytes(filePath);
                var document = new MongoDocument { Title = Path.GetFileName(filePath), Bytes = f.Length };
                await collection.InsertOneAsync(document);
                var size = 1024 * 1024 * 5;
                var chunks = (int)Math.Ceiling(f.Length / (1.0 * size));
                for (var i = 0; i < chunks - 1; i++) {
                    var start = i * size;
                    var end = size;//(i + 1) * size > f.Length ? f.Length : (i + 1) * size;
                    var span = f.AsSpan(start, end).ToArray();
                    var chunk = new Chunk { Data = span, Document = document.Id, Order = i };
                    await chunksCollection.InsertOneAsync(chunk);
                }
                var last = f.AsSpan((chunks - 1) * size).ToArray();
                await chunksCollection.InsertOneAsync(new Chunk { Data = last, Document = document.Id, Order = chunks - 1 });
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        char[] running = { '|', '/', '|', '\\' };

        public async Task Download(string filePath, string fileName) {
            try {
                var collection = client.GetDatabase(dbName).GetCollection<MongoDocument>(collectioName);
                var chunksCollection = client.GetDatabase(dbName).GetCollection<Chunk>("chunks");
                var titleSearch = Builders<MongoDocument>.Filter.Eq(d => d.Title, fileName);
                var d = await (await collection.FindAsync(titleSearch)).SingleAsync();
                var l = new List<byte>();
                var filter = Builders<Chunk>.Filter.Eq(c => c.Document, d.Id);
                var chunks = await chunksCollection.FindAsync(filter);
                int i = 0;
                await chunks.ForEachAsync(c => {
                    Console.Write($"{running[i++ % running.Length]}\b");
                    l.AddRange(c.Data);
                });
                Console.WriteLine();
                await File.WriteAllBytesAsync(Path.Combine(filePath, fileName), l.ToArray());
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        public void Dispose(bool disposing) {
            client.Cluster.Dispose();
        }
    }
}
