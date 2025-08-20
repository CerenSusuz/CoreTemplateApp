using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.FunctionCalling.FunctionSchema;

/// <summary>
/// Interface for AI functions that can provide a JSON schema.
/// </summary>
public interface IJsonSchemaProvider
{
    object GetJsonSchema();
}
