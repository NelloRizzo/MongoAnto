using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoUtils
{
    public class Chunk {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId Document { get; set; }
        public int Order {  get; set; }
        public byte[] Data { get; set; }
    }
    public class MongoDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public long Bytes {  get; set; }
        public List<Chunk> Chunks { get; set; }
    }
}
