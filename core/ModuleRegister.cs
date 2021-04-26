// <copyright file="ModuleRegister.cs" company="Rosen Group">
// Copyright (c) Rosen Group. All rights reserved.
// </copyright>

namespace research_redis_key_pattern.core
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// InjectBuilder class for Middleware module.
    /// </summary>
    public static class ModuleRegister
    {
        /// <summary>
        /// Register log response middleware.
        /// </summary>
        /// <param name="app">app builder.</param>
        public static void UseLogResponseMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogResponseMiddleware>();
        }
    }
}
