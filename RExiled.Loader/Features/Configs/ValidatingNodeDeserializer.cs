using System;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace RExiled.Loader.Features.Configs
{
    internal sealed class ValidatingNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        public ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer) => this.nodeDeserializer = nodeDeserializer;

        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value))
            {
                Validator.ValidateObject(value, new ValidationContext(value, null, null), true);

                return true;
            }

            return false;
        }
    }
}