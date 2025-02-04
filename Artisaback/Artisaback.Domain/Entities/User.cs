using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Artisaback.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("email")] public string Email { get; set; }

    [BsonElement("passwordHash")] public string PasswordHash { get; set; }

    [BsonElement("role")] public string Role { get; set; }

    [BsonElement("refreshToken")] public string? RefreshToken { get; set; }

    [BsonElement("refreshTokenExpiryTime")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}