/*
 * Copyright (c) 2019-2023, Incendi (info@incendi.no)
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace spark_example
{
    public class IgnoreRouteMiddleware
    {

        private readonly RequestDelegate next;

        // You can inject a dependency here that gives you access
        // to your ignored route configuration.
        public IgnoreRouteMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue &&
                context.Request.Path.Value.Contains("favicon.ico"))
            {

                context.Response.StatusCode = 404;

                return;
            }

            await next.Invoke(context);
        }
    }
}
