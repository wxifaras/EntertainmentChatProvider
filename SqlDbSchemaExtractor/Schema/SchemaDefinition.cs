﻿using System;
using System.Collections.Generic;

namespace SqlDbSchemaExtractor.Schema;

public sealed class SchemaDefinition
{
    public SchemaDefinition(
        string name,
        string platform,
        string? description,
        IEnumerable<SchemaTable> tables)
    {
        this.Name = name;
        this.Platform = platform;
        this.Description = description;
        this.Tables = tables ?? Array.Empty<SchemaTable>();
    }

    public string Name { get; }

    public string? Description { get; set; }

    public string Platform { get; }

    public IEnumerable<SchemaTable> Tables { get; }
}