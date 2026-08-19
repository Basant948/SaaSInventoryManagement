using Microsoft.AspNetCore.Authorization;
using SaaSInventoryManagement.Infrastructure.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace SaasInventoryManagement.Test.Authorization
{
    public class PermissionAuthorizationHandlerTests
    {
        private readonly PermissionAuthorizationHandler _handler = new();

        private static AuthorizationHandlerContext CreateContext(
            ClaimsPrincipal user,
            string requiredKey)
        {
            var requirement = new PermissionRequirement(requiredKey);
            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                resource: null);
        }

        private static ClaimsPrincipal CreateUser(params string[] permValues)
        {
            var claims = permValues
                .Select(v => new Claim(PermissionClaimTypes.Permission, v))
                .ToList();

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        [Fact]
        public async Task Succeeds_When_User_Has_Wildcard()
        {
            var user = CreateUser("*");
            var context = CreateContext(user, "inv.products.manage");

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Succeeds_When_User_Has_Exact_Permission()
        {
            var user = CreateUser("inv.products.view", "inv.stock.view");
            var context = CreateContext(user, "inv.products.view");

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Fails_When_User_Has_No_Matching_Permission()
        {
            var user = CreateUser("inv.products.view");
            var context = CreateContext(user, "inv.products.manage");

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Fails_When_User_Has_No_Permission_Claims()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity("Test")); // authenticated but no perms
            var context = CreateContext(user, "inv.dashboard.view");

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Fails_When_User_Is_Anonymous()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated
            var context = CreateContext(user, "inv.dashboard.view");

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }
    }
}
