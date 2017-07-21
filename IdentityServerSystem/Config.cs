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
                new ApiResource("scope.fullaccess", "Example API"),
                new ApiResource("scope.editownered", "Edit You Owned")
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
                new IdentityResources.Phone(),
                new IdentityResource("customResource","CustmeResource" ,new []{"CustomClaimType1", "CustomClaimType2"})
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
                //For MVC Client implicit flow
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RedirectUris = {"http://localhost:5002/signin-oidc"},
                    PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},
                   AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "customResource",
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerConstants.StandardScopes.Phone
                    }
                },
                new Client
                {
                    ClientId = "ClientIDReadAndEdit",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret("secretUserPassword".Sha256())
                    },
                    AllowedScopes = { "scope.readaccess" , "scope.editownered" }
                },
                new Client
                {
                    ClientId = "ClientIdThatCanOnlyRead",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret1".Sha256())
                    },
                    AllowedScopes = {"scope.readaccess"}
                },
                new Client
                {
                    ClientId = "ClientIdWithFullAccess",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret2".Sha256())
                    },
                    AllowedScopes = {"scope.fullaccess"}
                },
                new Client
                {
                    ClientId = "ClientIDWithEditOwned",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret3".Sha256())
                    },
                    AllowedScopes = { "scope.editownered" }
                }
            };
        }

       
    }
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        /// <summary>
        /// 可根据User的Claim自定义IssueClaim的内容
        /// 需要获得ClientID的值，根据Client从UserClaims中取值，可作为Claim-based Aurthorization
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            if (context.IssuedClaims.Count == 0)
            {
                if (context.Subject.Claims.Count() > 0)
                {
                    context.IssuedClaims = context.Subject.Claims.ToList();


                    //contex.Subject为ApplicationUser
                    //var claimStringList = context.Subject.Claims.ToList();
                    //List<Claim> claimList = new List<Claim>();
                    //for (int i = 0; i < claimStringList.Count; i++)
                    //{
                    //    claimList.Add(new Claim("role", claimStringList[i].Value));
                    //}
                    //context.IssuedClaims = claimList;
                }
            }
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var user = _userManager.FindByIdAsync(context.Subject.GetSubjectId());

            context.IsActive = (user != null);
            return Task.FromResult(0);
        }
    }


    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager)
        {
            this._userManager = _userManager;
            this._signInManager = _signInManager;
        }
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // Check The UserName And Password In Database, Return The Subject If Correct, Return Null Otherwise
            // subject = ......

            if (ValidateCredentials(_signInManager, context.UserName, context.Password))
            {
                var user = _userManager.FindByNameAsync(context.UserName).GetAwaiter().GetResult();
                var userClaims = GetUserClaims(user);
                context.Result = new GrantValidationResult(user.Id.ToString(), OidcConstants.AuthenticationMethods.Password, userClaims);
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid_credential");
            }

            return Task.FromResult(0);
        }

        private bool ValidateCredentials(SignInManager<ApplicationUser> signInManager, string userName, string password)
        {
            var result = _signInManager.PasswordSignInAsync(userName, password, false, false).GetAwaiter().GetResult();
            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private List<Claim> GetUserClaims(ApplicationUser user)
        {
            List<Claim> claimList = new List<Claim>();
            var claimStringList = user.Claims.ToList();
            try
            {

                if (claimStringList != null || claimStringList.Count > 0)
                {

                    for (int i = 0; i < claimStringList.Count; i++)
                    {
                        claimList.Add(new Claim("role", claimStringList[i].ClaimValue));
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return claimList;
        }
    }
}
