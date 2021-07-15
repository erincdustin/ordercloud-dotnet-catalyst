﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderCloud.Catalyst
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static Type WithoutGenericArgs(this Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

		public static List<string> GetRequiredOrderCloudRoles(this HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<OrderCloudUserAuthAttribute>() ?? Array.Empty<OrderCloudUserAuthAttribute>();
            return authorizeAttributes.SelectMany(a => a.OrderCloudRoles).ToList();
        }

		/// <summary>
		/// Chain to IServiceCollection (typically in Startup.ConfigureServices) to enable authenticating by passing a valid
		/// OrderCloud access token in the Authorization header. Add [OrderCloudUserAuth] attribute to specific controllers or actions
		/// where this should be enforced. Typical use case is custom endpoints for front-end user apps.
		/// </summary>
		public static IServiceCollection AddOrderCloudUserAuth(this IServiceCollection services)
		{
			services
				.AddHttpContextAccessor()
				.AddSingleton<RequestAuthenticationService>()
				.AddSingleton<ISimpleCache, LazyCacheService>() // Can override by registering own implmentation
				.AddAuthentication()
				.AddScheme<OrderCloudUserAuthOptions, OrderCloudUserAuthHandler>("OrderCloudUser", null);
			return services;
		}

		/// <summary>
		/// Chain to IServiceCollection (typically in Startup.ConfigureServices) to enable validation of incoming webhooks.
		/// </summary>
		public static IServiceCollection AddOrderCloudWebhookAuth(this IServiceCollection services, Action<OrderCloudWebhookAuthOptions> configureOptions)
		{
			services.AddAuthentication()
				.AddScheme<OrderCloudWebhookAuthOptions, OrderCloudWebhookAuthHandler>("OrderCloudWebhook", null, configureOptions);
			return services;
		}
	}
}
