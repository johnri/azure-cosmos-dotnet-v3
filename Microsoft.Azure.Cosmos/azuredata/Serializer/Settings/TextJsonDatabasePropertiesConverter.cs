﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Azure.Cosmos
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Documents;

    internal class TextJsonDatabasePropertiesConverter : JsonConverter<DatabaseProperties>
    {
        public override DatabaseProperties Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException(string.Format(CultureInfo.CurrentCulture, RMResources.JsonUnexpectedToken));
            }

            using JsonDocument json = JsonDocument.ParseValue(ref reader);
            JsonElement root = json.RootElement;
            DatabaseProperties setting = new DatabaseProperties();

            foreach (JsonProperty property in root.EnumerateObject())
            {
                TextJsonDatabasePropertiesConverter.ReadPropertyValue(setting, property);
            }

            return setting;
        }

        public override void Write(
            Utf8JsonWriter writer,
            DatabaseProperties setting,
            JsonSerializerOptions options)
        {
            if (setting == null)
            {
                return;
            }

            writer.WriteStartObject();
            TextJsonSettingsHelper.WriteId(writer, setting.Id);

            TextJsonSettingsHelper.WriteETag(writer, setting.ETag);

            TextJsonSettingsHelper.WriteResourceId(writer, setting.ResourceId);

            TextJsonSettingsHelper.WriteLastModified(writer, setting.LastModified, options);

            writer.WriteEndObject();
        }

        private static void ReadPropertyValue(
            DatabaseProperties setting,
            JsonProperty property)
        {
            if (property.NameEquals(JsonEncodedStrings.Id.EncodedUtf8Bytes))
            {
                setting.Id = property.Value.GetString();
            }
            else if (property.NameEquals(JsonEncodedStrings.ETag.EncodedUtf8Bytes))
            {
                setting.ETag = TextJsonSettingsHelper.ReadETag(property);
            }
            else if (property.NameEquals(JsonEncodedStrings.RId.EncodedUtf8Bytes))
            {
                setting.ResourceId = property.Value.GetString();
            }
            else if (property.NameEquals(JsonEncodedStrings.LastModified.EncodedUtf8Bytes))
            {
                setting.LastModified = TextJsonUnixDateTimeConverter.ReadProperty(property);
            }
        }
    }
}