using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UPG_SP_2024
{   
        // convertor form json file
        public class Vector2DJsonConverter : JsonConverter<Vector2D>
        {
            public override Vector2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                float x = 0, y = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return new Vector2D(x, y);

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();

                        if (propertyName == "X")
                            x = reader.GetSingle();
                        else if (propertyName == "Y")
                            y = reader.GetSingle();
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Vector2D value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("X", value.X);
                writer.WriteNumber("Y", value.Y);
                writer.WriteEndObject();
            }
        }
}


