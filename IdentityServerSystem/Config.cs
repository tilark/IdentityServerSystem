using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4;
using System.Security.Claims;
using IdentityServerSystem.Models;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.Validation;
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Extensions;

namespace IdentityServerSystem
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("scope.readaccess", "Example API"),
                new ApiResource("humanresourcesystem", "人力资源系统资源")
            };

        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResources.Phone()
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                //For MVC Client Hybrid Flow
                new Client
                {
                    ClientId = "mvchybrid",
                    ClientName = "MVC Client Hybrid",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    RequireConsent = true,
                    ClientSecrets =
                    {
                        new Secret("secrethybrid".Sha256())
                    },
                     RedirectUris = {"http://localhost:5002/signin-oidc"},
                    PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "scope.editownered"
                    },
                    AllowOfflineAccess = true
                },
                 //For MEMSCore Implicit Flow
                new Client
                {
                    ClientId = "MEMSNetCore",
                    ClientName = "MEMSNetCore",
                    AllowedGrantTypes = GrantTypes.Implicit,
                     RedirectUris = {"http://localhost:6006/signin-oidc"},
                    PostLogoutRedirectUris = {"http://localhost:6006/signout-callback-oidc"},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }                   
                },
                //For MVC Client implicit flow
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = true,
                    RedirectUris = {"http://localhost:5002/signin-oidc"},
                    PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},
                   AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerConstants.StandardScopes.Phone
                    }
                },
                 //For MVC Client implicit flow
                new Client
                {
                    ClientId = "mvc2",
                    ClientName = "MVC Client2",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = true,
                    RedirectUris = {"http://localhost:5002/signin-oidc"},
                    PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},
                   AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerConstants.StandardScopes.Phone
                    }
                }
            };
        }
    }
}
