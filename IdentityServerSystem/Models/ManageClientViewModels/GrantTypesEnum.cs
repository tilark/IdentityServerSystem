using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSystem.Models.ManageClientViewModels
{
    public enum GrantTypesEnum
    {
        Implicit,
        ImplicitAndClientCredentials,
        Code,
        CodeAndClientCredentials,
        Hybrid,
        HybridAndClientCredentials,
        ClientCredentials,
        ResourceOwnerPassword,
        ResourceOwnerPasswordAndClientCredentials
    }
}
