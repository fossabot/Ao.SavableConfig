﻿using Ao.SavableConfig.Binder.Annotations;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ao.SavableConfig.Binder
{
    public class IdentityMapNameTransfer : INameTransfer
    {
        public IdentityMapNameTransfer(IReadOnlyDictionary<PropertyIdentity, string> map)
            :this(null,map)
        {
        }
        public IdentityMapNameTransfer(string basePath,IReadOnlyDictionary<PropertyIdentity, string> map)
        {
            BasePath = basePath;
            Map = map ?? throw new ArgumentNullException(nameof(map));
        }
        public string BasePath { get; }

        public IReadOnlyDictionary<PropertyIdentity,string> Map { get; }

        public string Transfer(object instance, string propertyName)
        {
            if (instance  is null)
            {
                return propertyName;
            }
            var parent = instance.GetType().BaseType;
            var name = propertyName;
            if (!Map.TryGetValue(new PropertyIdentity(parent, propertyName),out name))
            {
                name = propertyName;
            }
            if (BasePath != null)
            {
                return ConfigurationPath.Combine(BasePath, name);
            }
            return name;
        }
        public static IReadOnlyDictionary<Type,INameTransfer> FromTypesAttributes(params Type[] types)
        {
            return types.ToDictionary(x => x, x => (INameTransfer)FromTypeAttributes(x));
        }
        public static IdentityMapNameTransfer FromTypeAttributes(Type type)
        {
            return FromTypeAttributes(null, type, true);
        }
        public static IdentityMapNameTransfer FromTypeAttributes(string prevPathName,Type type,bool usingClassPathName)
        {
            var baseName = prevPathName;
            if (usingClassPathName)
            {
                var className = type.GetCustomAttribute<ConfigPathAttribute>();
                if (className!=null)
                {
                    baseName = ConfigurationPath.Combine(baseName, className.Name);
                }
            }
            var map = type.GetProperties()
                .Select(x => new { Property = x, Attribute = x.GetCustomAttribute<ConfigPathAttribute>() })
                .Where(x=>x.Attribute!=null)
                .ToDictionary(x=>new PropertyIdentity(type,x.Property.Name),x=> x.Attribute.Name);
            return new IdentityMapNameTransfer(baseName,map);
        }
    }
}
