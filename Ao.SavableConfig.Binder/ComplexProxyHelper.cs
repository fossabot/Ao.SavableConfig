﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ao.SavableConfig.Binder
{
    public class ComplexProxyHelper
    {
        public static readonly ComplexProxyHelper Default = new ComplexProxyHelper(ProxyHelper.Default);

        private readonly Dictionary<Type, ProxyCreator> creators;
        private readonly ProxyHelper proxyHelper;

        public bool AutoAnalysis { get; }

        public ComplexProxyHelper(ProxyHelper proxyHelper,bool autoAnalysis=true)
        {
            AutoAnalysis = autoAnalysis;
            this.proxyHelper = proxyHelper;
            creators = new Dictionary<Type, ProxyCreator>();
        }

        public bool IsCreated<T>()
        {
            return creators.ContainsKey(typeof(T));
        }
        public ProxyCreator GetCreatorOrDefault<T>()
        {
            if (creators.TryGetValue(typeof(T),out var creator))
            {
                return creator;
            }
            return null;
        }

        public T Build<T>(IConfiguration configuration)
        {
            var type = typeof(T);
            if (!creators.TryGetValue(type, out var creator))
            {
                creator = proxyHelper.CreateComplexProxy<T>(AutoAnalysis);
            }
            return (T)creator.Build(configuration);
        }
    }
}
