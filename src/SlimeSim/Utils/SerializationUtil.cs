using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SlimeSim.Models;

namespace SlimeSim.Utils
{
    public static class SerializationUtil
    {

        public static string SerializeToJson(Simulation sim)
        {
            var json = JsonSerializer.Serialize(sim, GetSerializationOptions());
            return json;
        }

        public static Simulation DeserializeFromJson(string json)
        {
            var sim = JsonSerializer.Deserialize<Simulation>(json, GetSerializationOptions());
            return sim;
        }

        private static JsonSerializerOptions GetSerializationOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            options.Converters.Add(new Vector2JsonConverter());
            options.Converters.Add(new Vector2iJsonConverter());
            return options;
        }
    }

    public sealed class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected object for Vector2.");

            float x = 0, y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Vector2(x, y);

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string prop = reader.GetString()!;
                reader.Read();

                switch (prop)
                {
                    case "x": x = reader.GetSingle(); break;
                    case "y": y = reader.GetSingle(); break;
                    default: reader.Skip(); break; // ignore unknown fields
                }
            }

            throw new JsonException("Incomplete Vector2 object.");
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteEndObject();
        }
    }

    public sealed class Vector2iJsonConverter : JsonConverter<Vector2i>
    {
        public override Vector2i Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected object for Vector2i.");

            int x = 0, y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Vector2i(x, y);

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string prop = reader.GetString()!;
                reader.Read();

                switch (prop)
                {
                    case "x": x = reader.GetInt32(); break;
                    case "y": y = reader.GetInt32(); break;
                    default: reader.Skip(); break;
                }
            }

            throw new JsonException("Incomplete Vector2i object.");
        }

        public override void Write(Utf8JsonWriter writer, Vector2i value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteEndObject();
        }
    }
}