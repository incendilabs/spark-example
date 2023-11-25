/*
 * Copyright (c) 2019-2023, Incendi (info@incendi.no)
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace spark_example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
