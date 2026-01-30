using System;
using System.ComponentModel;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace RExiled.Loader.Features.Configs
{
    internal sealed class CommentsPropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor baseDescriptor;

        public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            this.baseDescriptor = baseDescriptor;
            Name = baseDescriptor.Name;
        }

        public string Name { get; set; }

        public Type Type => baseDescriptor.Type;

        public Type TypeOverride
        {
            get => baseDescriptor.TypeOverride;
            set => baseDescriptor.TypeOverride = value;
        }

        public int Order { get; set; }

        public ScalarStyle ScalarStyle
        {
            get => baseDescriptor.ScalarStyle;
            set => baseDescriptor.ScalarStyle = value;
        }

        public bool CanWrite => baseDescriptor.CanWrite;

        public void Write(object target, object value)
        {
            baseDescriptor.Write(target, value);
        }

        public T GetCustomAttribute<T>()
            where T : Attribute
        {
            return baseDescriptor.GetCustomAttribute<T>();
        }

        public IObjectDescriptor Read(object target)
        {
            var description = baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
            return description != null
                ? new CommentsObjectDescriptor(baseDescriptor.Read(target), description.Description)
                : baseDescriptor.Read(target);
        }
    }
}
