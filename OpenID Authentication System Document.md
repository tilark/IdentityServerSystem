[TOC]
# 第三方身份认证系统
## 参考资料
[Adding User Authentication with OpenID Connect)](https://identityserver4.readthedocs.io/en/release/quickstarts/3_interactive_login.html)
本项目的名称:IdentityServerSystem
## 期望
1. 本系统只作为认证及授权系统，不管理人员的详细信息（人员所属科室信息等，这些信息放在Web Api中）。客户端与本系统连接时，推荐采用hybrid(结合Open Id与OAutho2.0协议)模式。
2. 本身拥有权限系统，管理员才可增、删、改用户信息
3. 从另一个Web Api中读取用户（在导入时直接新增用户名和密码，默认将工号作为帐号）
4. 本系统的权限有三种：一种是超级管理员，可增加Client、IdentityResource等基础配置；一种是各客户程序的管理员 ，可更改人员信息，配置人员在Client的IdentityResource，并为每个成员可单独添加UserClaim。

## 用户管理
### 登陆后可新增用户（用户名和密码）
#### ManageUsersController/AddUser
1. 参数
    - Id:Guid
    - UserName:帐号
    - Password:密码
    - FamilyName:姓氏
    - FirstName:名字
    - Telephone：电话
2. 主体
    创建一个新的帐号，Id需与HumanResourceSystem中的人员Id一致
    创建成功后，添加以下几个默认的UserClaim
    ```
      //先创建一个新的ApplicationUser，看是否成功，成功返回该用户信息
            var user = new ApplicationUser { Id = Id, UserName = userName, FamilyName = familyName, FirstName = firstName, Telephone = telephone};
            var result = await _userManager.CreateAsync(user, password);
           
            if (result.Succeeded)
            {
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim("family_name", familyName),
                    new Claim("given_name", firstName),
                    new Claim("preferred_username", user.FullName)
                });
                return user;
            }
    ```
    创建超级管理员权限“Administrator”
    ```
     //如果是UserName是“Administrator"，添加"Administrator"的UserClaim
                if (String.Equals(userName, "Administrator"))
                {
                    await _userManager.AddClaimsAsync(user, new Claim[]
                    {
                    new Claim("Administrator", "Administrator")
                    });
                }
    ```
3. 模型
``CreateUserViewModel``

### 更改用户信息（电话码号等）
#### ManageUsersController/Edit

### 删除用户信息
#### ManageUsersController/DeleteUser
### 修改用户登陆帐号
#### ManageUsersController/ChangeUserAccount

### 用户的权限管理
Administrator拥有最高权限,adminUser 可以给各系统的管理员添加User的基本信息和UserClaims，但无法管理基础值如Client,IdentityScope, ApiScope等
```
services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", policy => policy.RequireClaim("Administrator"));
                options.AddPolicy("adminUser", policy => policy.RequireClaim("adminUser"));

            });
```

### 从人力资源管理系统通过WebApi中获取人员信息
获取的路径为：``~/api/v2/WebApiPersonInfoes``
获得的信息为:
```
userId: Guid, 用户的Guid
employeeNo: string，用户的工号
userName : string，用户的姓名
deleted : bool，删除标志
```

#### 创建人员帐号信息
##### GetUserInfoFromWebApiController/CreateUserIfNotExistFromWebApi
1. 参数
    -webApiUrl:~/api/v2/WebApiPersonInfoes
2. 主体
从WebApi中获取人员信息中删除标志为false的人员列表，帐号是用户工号，密码是123456，User Id与WebApi
中的User Id相同，同时创建Claims:family_name, given_name, preferred_username。
3. 模型
``CreateUserViewModel``

#### 更新人员信息（登录帐号不变）
##### GetUserInfoFromWebApiController/UpdatePersonInfoFromWebApi
1. 参数
    -webApiUrl:~/api/v2/WebApiPersonInfoes
2. 主体
    查询数据库中是否存在该用户，如果存在，则修改用户的姓名及相应的Claims：family_name, given_name, preferred_username，保持登录帐号不变。
3. 模型
``EditUserViewModel``

#### 修改人员登录帐号
##### GetUserInfoFromWebApiController/UpdatePersonInfoAccountFromWebApi
1. 参数
    -webApiUrl:~/api/v2/WebApiPersonInfoes
2. 主体
    - 筛选出用户删除标识为false的用户，再从查询数据库中是否存在该用户，如果存在，则判断登录帐号是否与得到的新的工号信息相同，如果不相同，则修改登录帐号，相同则不处理。
    - 筛选出用户删除标识为true的用户，从数据库查找该用户，存在则删除。
3. 模型
``ChangeUserAccountViewModel``
``DeleteUserViewModel``

## 身份认证管理
### IdentityServer中添加对Client的管理
#### 分析Client
```
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
                        "customResource",
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerConstants.StandardScopes.Phone
                    }
                }
```
1. 每个Client代表一个客户程序
2.  Client中重要的信息是ClientID必须是唯一，ClientName可以是客户程序的名称。客户端的user token中aud对应为ClientID。
3.  AllowedGrantTypes 代表客户端使用的OpenID的授权方式，有三种授权方式，一种是Implicit，一种是密码，另一种是混合方式
4.  RequireConsent 代表是否会显示授权界面，默认为true
5.  RedirectUris 认证成功后返回到客户端程序的哪个界面，是OpenID规范的返回界面路径。
6.  PostLogoutRedirectUris 退出后返回到客户端程序的哪个界面
7.  AllowedScopes 表示客户程序能够使用的授权范围，客户端从该认证系统中所能获得内容，写入到user token中。可理解为将允许的内容可写入到user token中。
8.  UserClaims中定义的Claim能够写入到UserToken中的内容。"customResource"即是在IdentityResource中定义好的内容。

```
return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResources.Phone(),
                new IdentityResource("customResource","CustmeResource" ,new []{"customClaimType1", "CustomClaimType2"})
            };
```
#### ManageClients/Index
1. 参数
无
2. 主体
列出Client的主要内容
3. 模型
Client

#### ManageClients/Create
1. 参数
    - ClientId <string> : 需与客户端的配置关联
    - ClientName <string> : 详细名称
    - GrantTypesEnum <GrantType> OpenID中的固有协议
    - RedirectUris List<string> OpenID的交互式规定，需在登陆成功后有返回界面
    - PostLogoutRedirectUris List<string> OpenID的交互式规定，需在退出登陆后有返回界面
    - AllowedScopes List<string> 允许写入到User token中的Claim
2. 主体
创建Client ,AllowedScopes需是IdentityResource中的内容
3. 模型
CreateClientViewModel

#### ManageClients/Edit
1. 参数
    - Id <int> : Client Id
    - ClientName <string> : 详细名称    
    - RedirectUris List<string> OpenID的交互式规定，需在登陆成功后有返回界面
    - PostLogoutRedirectUris List<string> OpenID的交互式规定，需在退出登陆后有返回界面
    - AllowedScopes List<string> 允许写入到User token中的Claim
2. 主体
更新Client ,更新内容是ClientName、RedirectUris、PostLogoutRedirectUris、AllowedScopes需是IdentityResource中的内容
3. 模型
EditClientViewModel

#### ManageClients/ResetClientSecret
1. 参数
    - Id<int> Client Id
    - ClientSecrets List<string> ClientSecrets
2. 主体
重置ClientSecret，该密码需要重新保存，否则一旦更改了密码，与之相关联的客户程序也需要做出相应改变。
3. 模型
ResetClientSecretViewModel

### 管理IdentityResource
#### ManageIdentityResourcesController/Create
1.参数
    (1) name
    (2)displayName
    (3)claimTypes
2. 主体
  通过在页面端输入IdentityResource的名称和ClaimTypes创建IdentityResource。在数据库中会影响[IdentityResources]和[IdentityClaims]
3. 模型
IdentityResource是元数据
```
namespace IdentityServer4.Models
{
    public class IdentityResource
    {
        public IdentityResource(string name, IEnumerable<string> claimTypes);
        public IdentityResource(string name, string displayName, IEnumerable<string> claimTypes);
    }
}
```
#### ManageIdentityResourcesController/Index
1. 参数
无
2. 主体
显示IdentityResource所有内容，包括claimTypes
3. 模型
IdentityServer4.EntityFramework.Entities.IdentityResource

#### ManageIdentityResourcesController/Edit
1. 参数
Id: IdentityResourceID
2. 主体
更新IdentityResource Name,DisplayName,UserClaims
3. 模型
IdentityResourcesEditViewModel

### 管理UserClaims
1. 采用UserClaim的方式管理该系统与其他程序间的权限分配问题

#### 列出User中所拥有Claim
ManageUserClaimsController/Index
1. 参数
Id: ApplicationUser Id
2. 主体
列出User的基本信息及UserClaims
3. 模型

#### 将Claim添加到各User中
ManageUserClaimsController/AddUserClaim
1. 参数
Id: ApplicationUser Id
PlanAddUserClaimDict:IDictionary<string, string> ：保存UserClaimType，与UserClaimValue
2. 主体
通过选择IdentityResource获得所属的ClaimTypes，填写Value，再加入到User的UserClaim中
3. 模型
AddUserClaimViewModel
#### 更新User中Claim值
ManageUserClaimsController/EditUserClaim
1. 参数
Id: ApplicationUser Id
UserClaims:IDictionary<string, string> ：User中UserClaimType，与UserClaimValue
2. 主体
更改UserClaimValue
3. 模型
EditUserClaimViewModel
#### 删除User中指定的Claim
ManageUserClaimsController/DeleteUserClaim
1. 参数
Id: ApplicationUser Id
UserClaimType:string ：User中UserClaimType
2. 主体
从User中删除UserClaim，传入User Id，找到User信息，利用UserClaimType找到对应的Claim，再删除
3. 模型
DeleteUserClaimViewModel

### 测试版客户端配置
#### 使用Cookies登陆
```
 app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies"
            });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

```
#### 配置Client的Scopes
```
 //For Implicit Flow
            var openIdConnectOptions = new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "Cookies",

                Authority = "http://localhost:5000",
                RequireHttpsMetadata = false,
                Scope = { "customResource", "email" },
                ClientId = "mvc",
                SaveTokens = true,
                GetClaimsFromUserInfoEndpoint = true
            };
            app.UseOpenIdConnectAuthentication(openIdConnectOptions);
```
1. Client从IdentityServer中获取UserClaim由``Scope``决定。默认情况已经添加openid和profile。

#### 在Client中使用Claim-Based Authorization
[参考文献:Claims-Based Authorization](https://docs.microsoft.com/zh-cn/aspnet/core/security/authorization/claims#security-authorization-claims-based)
在WebAPI中Startup.cs中设置policy
```
public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditOwned", policy => policy.RequireClaim("scope", "scope.editownered"));
                options.AddPolicy("ReadOnly", policy => policy.RequireClaim("scope", "scope.readaccess"));
            });
        }
```
在Controller或Action中加入Policy验证
```
        [HttpGet]
        [Authorize(Policy = "ReadOnly")]
        public string GetString()
        {
            return "GetStringGetString2";
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "EditOwned")]

        public string GetString(int id)
        {
            return "GetStringID " + id.ToString();
        }
```

## 资源获取管理
### 管理Api Resource
添加Api Resource，从IdentityResource中获得
### 更改IdentityServer的Client配置，采用HybridAndClientCredentials模式

1. 需要增加ClientSecrets 
2. AllowOfflineAccess  设为 true
3. AllowedScopes中需要增加Api Resource的内容，Api Resource内容需与Web Api中允许的Scope匹配。表示该客户端能够有权限获得Web Api的资源

### 客户端的Client配置
添加``IdentityModel  Microsoft.AspNetCore.Authentication.Cookies  Microsoft.AspNetCore.Authentication.OpenIdConnect ``Nuget包
为了同时拥有OpenID与OAutho2.0的access token，客户端需配置成hybrid flow形式
```
 ////For MVC Hybrid Flow
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "Cookies",
                Authority = "http://localhost:5000",
                RequireHttpsMetadata = false,
                ClientId = "mvchybrid",
                ClientSecret = "secrethybrid",
                ResponseType = "code id_token", //means “use hybrid flow”
                Scope = { "humanresourcesystem", "offline_access" },
                GetClaimsFromUserInfoEndpoint = true,
                SaveTokens = true
            });
```
配置文件需配置的内容:
    1. 认证系统的路径 ``"http://localhost:5000"``
    2. 该客户端的ClientId ``mvchybrid`` 与IdentityServer中ClientID匹配
    3. ClientSecret ``secrethybrid`` 与IdentityServer中ClientSecret匹配
    4. ResponseType ``"code id_token"``（ //means “use hybrid flow”）
    5. Scope ``{ "humanresourcesystem", "offline_access" }`` 如果需要获得WebApi的资源，需添加WebApi允许的Scope。添加offline_access，在IdentityServer中的Client中AllowOfflineAccess要设为true
    6. GetClaimsFromUserInfoEndpoint = true
    7. SaveTokens = true
    8. RequireHttpsMetadata = false; 未使用https协议    


### WebApi Resource Owned配置
1. 安装IdentityServer4.AccessTokenValidation 
2. 在UseMvc 之前添加 middleware，RequireHttpsMetadata = false 表示未使用https协议
```
  app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = Configuration.GetSection("IdentityServier").GetValue<string>("AuthorityUri", "http://localhost:5000"),
                RequireHttpsMetadata = false,
                ApiName = Configuration.GetSection("IdentityServier").GetValue<string>("ApiName", "humanresourcesystem")
            });

    app.UseMvc();
```
3. 配置文件中需配置的内容：
    - 认证系统的路径``"http://localhost:5000"``
    - ApiName在配置文件中为``scope.editownered``

#### 客户端使用Access Token
1. 使用命名空间``Microsoft.AspNetCore.Authentication``
2. 示例代码
```
public async Task<IActionResult> CallApiUsingUserAccessToken()
{
    var accessToken = await HttpContext.Authentication.GetTokenAsync("access_token");

    var client = new HttpClient();
    client.SetBearerToken(accessToken);
    
     var respone = await client.GetAsync("http://localhost:5002/departments/getdepartmentlist");
            if (!respone.IsSuccessStatusCode)
            {
                Console.WriteLine(respone.StatusCode);
            }
            else
            {
                var content = await respone.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
    return View("json");
}
```
3. 使用Access Token获得IdentityServer的科室列表及人员信息，需配置以下信息
    - 该用户的Access Token中需要有WebApi允许的Scope
    - WebApi的资源路径``"http://localhost:5002/departments/getdepartmentlist"``

#### IdentityServer从人力资源管理系统中通过Web Api中获取人员信息
##### 创建人员信息及登录信息
##### 更新人员信息及登录帐号（如果帐号改变的话）
1. 可通过ClientCredentials获得WebApi的资源
```
 DiscoveryResponse disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ClientIDWithEditOwned", "secret3");

            TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync("humanresourcesystem");

            if (tokenResponse.IsError)
            {
                return;
            }
            // Call Cliennt
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            var respone = await client.GetAsync("http://localhost:5002/departments/getdepartmentlist");
            if (!respone.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = respone.StatusCode;
            }
            else
            {
                var content = await respone.Content.ReadAsStringAsync();

                ViewBag.GetContent = JArray.Parse(content);
            }
```
2. 在IdentityServer上登陆的帐号不会获得access token
3. 使用ClientCredentials需要在配置文件中配置以下信息
    - IdentityServer路径``"http://localhost:5000"``
    - Client中拥有WebApi允许的Scope ``scope.editownered``
    - ClientID和Secret
    - WebApi的路径``"http://localhost:5002/departments/getdepartmentlist"``

#### 问题
1. 在使用AccessToken从IdentityServer获取资源时，提示需登陆。

### ID Token中比较重要的字段
|ID Token字段|内容|说明|
|---|---|---|
|aud |mvc|该客户端的ClientID|
|sub|258d296b-fac4-479d-3317-08d4cd83677a|User ID|
|name|9991|User Name(登陆帐号)|
|customClaimType1|test1|IdentityResource中的Claim|

## 数据库更改
 1. migrations
IdentityServer 
```
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
```
ApplicationDbContext中已经创建好了，直接使用
```
dotnet ef database update -c ApplicationDbContext

dotnet ef migrations add InitialApplicationUser -c ApplicationDbContext -o Data/Migrations
dotnet ef migrations remove -c ApplicationDbContext
```
### 正式数据库
发布到Linux服务器，使用Mysql8.0.2

## Nuget包管理
### Mysql数据库
``Install-Package Pomelo.EntityFrameworkCore.MySql -Version 1.1.2``
``Install-Package Pomelo.EntityFrameworkCore.MySql.Design -Version 1.1.2``
### 解决主键不能自增或自动创建的问题
1. 如果是int型作为主键，在Migration中需加
```
 columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    ApiScopeId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 200, nullable: false)
                },
```
2. 如果是Guid作为主键，在Migration中需加
```
columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValue: Guid.NewGuid()),
                    AccessFailedCount = table.Column<int>(nullable: false),
                },
```
3. 在启动程序时自动进行Migration操作
```
 using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                 serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            }
```

## 发布网站
### 使用Docker布署

1. 确定网络为 hahaxi_net
2. 分IP为172.22.16.5
3. 确定对外端口为6005:80
```docker run --name identityserversystem --network=hahaxi_net --ip 172.22.16.5 -it -p 6005:80 -d  identityserversystem:1.0.0``


## 相关问题
1. 测试如果在Client中未加入IdentityResource，而在UserClaim中存在该Resource的Claim，会不会写入到UserToken中?
测试ClientID=mvc
mvc中包括 customResource 
User中包括customResource中的customClaimType1值，也包括NormalUser与AdminUser,但这两个不是属于customResource。
测试在客户端会不会显示出NormalUser与AdminUser
（答案：不会出现）
2. ConsentService的BuildViewModelAsync
var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
该request.ScopesRequested只包含了"openid"和"profile"两个Scope
而搜索到的Client中的AllowedScoped中包含有设置好的4个Scope。

ConsentService的ProcessConsent中// validate return url is still valid
                var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
model.ReturnUrl中的ScopesRequested仍然只有"openid"和"profile"

如何更改model.ReturnUrl中的请求的Scope

```
        RedirectUri "/connect/authorize/consent?client_id=mvc&redirect_uri=http%3A%2F%2Flocalhost%3A5002%2Fsignin-oidc&response_type=id_token&scope=openid%20profile&response_mode=form_post&nonce=636362237773846972.OGM0MDBlNmEtMDA4Mi00ZGZiLTkxZjQtZTQzMTE2ODM5OTYyNzJhOGYzNTYtZjRjOC00ZDY0LWFjZjYtNzg2YzQ5YWUyYTEw&state=CfDJ8Pn_Qo1GkkhHqSP6vv9B-1lUqY2friAHmE8X11Ldes8TLdIU5ipEKGTrcdvOFoFsKB9fooyevRJ8b3-XEF6hPGq3yt-OReImvoPJOuIsh8x3VaLYE756KJtILXYVO_mdoBTt0c67VHKke1mIRgYpnJO_Bido0nZ09ONRoGEV_RxeMcoaMkbMhlwNTLNCa_kptPixJd0ExhcRWZHXo2Pa8n9aZT4c4qGgHziwaJLspGrZsfuA7PQKGCnXsi-y4U1D3LVN4Na472KiWUSvSByQQQOiC9dUgfiuFGKmfqyFB5abAOji2AkVcsKLLSd5mrM1ZVpO3tPEMTYkoeWE-Pd2wHE"   string

```

[参考答案](https://stackoverflow.com/questions/42085522/openidconnect-middleware-keeps-adding-profile-scope-to-the-request):
```

The OpenIdConnectionOptions automatically requests the openid and profile scopes (see source code), with a private setter on the Scope property.

When you set scopes like you are, you are not setting a new list, but adding to the existing.

Clearing and then adding the scope works:

var options = new OpenIdConnectOptions();
options.Scope.Clear();
options.Scope.Add("openid");
app.UseOpenIdConnectAuthentication(options);
```
依据参考答案，在MVC Client中的OpenIdConnectOptions加入了 ``Scope = { "customResource", "email" }``,解决了能够获得customResource的UserClaim。

3. 如果在IdentityServer中用户必须添加customResource所管的所有的IdentityResource，如customResource有“customClaimType1”“CustomClaimType2”。只添加customClaimType2一个值，则会出现错误``IDX10501: Signature validation failed. Unable to match 'kid'``。
如果在没有customClaimType1的情况下，也能够正常运行呢？就像email为空，但仍然可以正常运行，只是不获得该值而已。
答案 ：在创建IdentityResource时，需将Emphasize设为True，这样即使UserClaim中只有customClaimType1，也不会出现验证错误。
4. Sorry, there was an error : unauthorized_client
答：在IdentityServer的数据库中有一个表PersistedGrants。其中在界面端创建的Client并没有写入到此表中

# Client管理
|ClientID|ClientName|Scope|Secret|备注|
|---|---|---|---|---|
|ClientHRMS|人力资源系统的客户供IdentityServer使用|humanresourcesystem|humanresourcesystem|使用ClientID与Secret从HumanResourceManageSystem中获得资源|
|MEMSCore|  医疗设备管理系统|openid 
profile humanresourcesystem|MEMSCore||

Client必须添加 user identifier和 User profile的IdentityResource
# ApiResource管理

|ApiResourceName|DisplayName|备注|
|---|---|
|humanresourcesystem|人力资源管理系统允许的Scope|如果需获得人力资源管理系统的资源，客户端Client的AllowedScope中需包含该值|

Client本身可以从ApiResource和IdentityResource中获得AllowedScopes的值，但在该系统中，ApiResource不能用文本的方式，而是从IdentityResource及ClaimTypes中获取子项。

所以创建ApiResource的步骤是先创建IdentityResource及ClaimType，然后再创建ApiResource。所以ApiResourceName会与IdentityResource的ClaimTypes重名。

# IdentityResource管理
|IdentityResourceName|DisplayName|ClaimTypes|备注|
|---|---|---|
|humanresourcesystem|人力资源管理系统|humanresourcesystem|如果要获得人力资源管理系统的资源|

# 发布到内网
## 使用Windows Service模式发布网站
1. 转移数据库
2. 设定端口号为819


ValidatedAuthorizeRequest 


## http://localhost:5000/.well-known/openid-configuration的信息

```
{"issuer":"http://localhost:5000",
"jwks_uri":"http://localhost:5000/.well-known/openid-configuration/jwks",
"authorization_endpoint":"http://localhost:5000/connect/authorize",
"token_endpoint":"http://localhost:5000/connect/token",
"userinfo_endpoint":"http://localhost:5000/connect/userinfo",
"end_session_endpoint":"http://localhost:5000/connect/endsession",
"check_session_iframe":"http://localhost:5000/connect/checksession",
"revocation_endpoint":"http://localhost:5000/connect/revocation",
"introspection_endpoint":"http://localhost:5000/connect/introspect",
"frontchannel_logout_supported":true,
"frontchannel_logout_session_supported":true,
"scopes_supported":["openid","profile","email","address","phone","scope.readaccess","humanresourcesystem","offline_access"],
"claims_supported":["sub","name","family_name","given_name","middle_name","nickname","preferred_username","profile","picture","website","gender","birthdate","zoneinfo","locale","updated_at","email","email_verified","address","phone_number","phone_number_verified"],
"grant_types_supported":["authorization_code","client_credentials","refresh_token","implicit","password"],
"response_types_supported":["code","token","id_token","id_token token","code id_token","code token","code id_token token"],
"response_modes_supported":["form_post","query","fragment"],
"token_endpoint_auth_methods_supported":["client_secret_basic","client_secret_post"],
"subject_types_supported":["public"],"id_token_signing_alg_values_supported":["RS256"],
"code_challenge_methods_supported":["plain","S256"]
}
```

# 从页面上创建Client后提示unauthorized_client

## 客户端向IdentityServer发出的链接请求
### 失败的链接
```
http://localhost:5000/connect/authorize?
client_id=mvc3test&
redirect_uri=http%3A%2F%2Flocalhost%3A5002%2Fsignin-oidc&
response_type=id_token&
scope=openid%20profile&
response_mode=form_post&
nonce=636447745512701425.Mzc2YThjZTUtNmI1OC00ODU0LTgxZWUtYWI4YmQ3ZDBiZTA5MDc2Y2I1MWUtOGI2NS00ODVkLWE5NDUtMTliNjk5OGNhOTBi&state=CfDJ8KPsDI6qEr9Orw1UkiYtsvN9QbI_FGoGs6sGUDgJWhIQo1U95gM9vf-n4UBwr3lJWufAQyX9NxJXDKQo7cN3ThNSh_7bbB392L5RrVhRam5DXrZ86TSzodTutLFn0cz0yhR35ClsR7ljzCNtgGo_HkVjUJ7teT8SqiIpgY7yAv1NEg0Rs2SWVJX17uF9atQIOajd382GlfuntvQB0XvTvh8dtsI2amfgaJiVnwhzx4UW0oNHMbB-OFiIFkg_aT_kJiQ7h7pfFaobSsxBvrq3EAiDShPLO1bVbGAdV-Tp6etpUaQVLVBcOsb6jrJ44SeVAmeIy7aypngXnHApw1VdTBo
```

### 成功的链接
```
http://localhost:5000/connect/authorize?
client_id=mvc2&
redirect_uri=http%3A%2F%2Flocalhost%3A5002%2Fsignin-oidc&
response_type=id_token&
scope=openid%20profile&
response_mode=form_post&
nonce=636447752216332456.NTk3OWNiZDUtMjNhYS00OWEyLWI5MWYtMTVjYTg1NWI3ZjkzZGI1Y2Y4MGEtOWUyNS00MTA3LWJlYzgtNGEwNDlhMzkyZGZk&state=CfDJ8KPsDI6qEr9Orw1UkiYtsvP5MrK21X5Bw2OfFQPJ7bGecqxHGN7RwGrZqF-iExb5LEwhUjC6Zdz8_c7aX-1tDy3lkEvIYBUsbzhIWFdwkS1LUHtlu7uGh5_ylTLnoQH2shxupazFIYcZzcNyyG_C3qHNrG8paceifogxnxKV6w_V6ZVDnxGsUXPTIxQrnd7uTcS2J9mpeE1Pca9nbRyruPkT8egk2KUb354sWO2waZ9-2GfIdt2F36HiQSwEYyzU8oTwJSt0qHji7Bg97-cR6ea-QdHbGGkukKNqPWsdNxwHxAh6dipKtRBkZX8c9qVzA-ETBjd8_PXC4LB9DAbwh8s
```

### 源代码中的测试
IdentityServer4.IntegrationTests.Endpoints.Authorize
```
 [Fact]
        [Trait("Category", Category)]
        public async Task anonymous_user_should_be_redirected_to_login_page()
        {
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginWasCalled.Should().BeTrue();
        }
```
### 自创建一个控制台测试项目IdentityServerAuthorizationStudy/ImplicitFlowConsoleTest

```
 public async static Task Anonymous_user_should_be_redirected_to_login_page()
        {
            var url = CreateAuthorizeUrl(
                clientId: "mvc",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "http://localhost:5002/signin-oidc",
                state: "123_state",
                nonce: "123_nonce");
            HttpClient client = new HttpClient();

            var response = await client.GetAsync(url);

            Console.ReadLine();
            //_mockPipeline.LoginWasCalled.Should().BeTrue();
        }
```
#### 测试过程及结果
通过更改clientId与redirectUri
mvc、mvc2、mvc3test均为GrantTypes.Implicit

|clientId|创建途径|redirectUri|测试结果(response.RequestMessage )|
|:---: |:---: |:---: |:---: |
|mvc|IdentityServer初始化时创建|"http://localhost:5002/signin-oidc"|返回正确的信息|
|mvc2|IdentityServer初始化时创建|"http://localhost:5002/signin-oidc"|返回正确的信息|
|mvc3test|页面端创建（自制后台创建|"http://localhost:5002/signin-oidc"|返回错误的信息|


返回正确的信息：
```
-       RequestMessage  {Method: GET, RequestUri: 'http://localhost:5000/account/login?returnUrl=%2Fconnect%2Fauthorize%2Flogin%3Fclient_id%3Dmvc%26response_type%3Did_token%26scope%3Dopenid%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%253A5002%252Fsignin-oidc%26state%3D123_state%26nonce%3D123_nonce', Version: 1.1, Content: <null>, Headers:
{
}}  System.Net.Http.HttpRequestMessage

```
返回错误的信息：
```
+       RequestMessage  {Method: GET, RequestUri: 'http://localhost:5000/home/error?errorId=6230335fcbace2c1a7d35df2584ab2a8', Version: 1.1, Content: <null>, Headers:
{
}}  System.Net.Http.HttpRequestMessage

```


#### 辅助方法
```
public const string BaseUrl = "http://localhost:5000";
        public const string AuthorizeEndpoint = BaseUrl + "/connect/authorize";
        public static string CreateAuthorizeUrl(
           string clientId,
           string responseType,
           string scope = null,
           string redirectUri = null,
           string state = null,
           string nonce = null,
           string loginHint = null,
           string acrValues = null,
           string responseMode = null,
           string codeChallenge = null,
           string codeChallengeMethod = null,
           object extra = null)
        {
            var url = new AuthorizeRequest(AuthorizeEndpoint).CreateAuthorizeUrl(
                clientId: clientId,
                responseType: responseType,
                scope: scope,
                redirectUri: redirectUri,
                state: state,
                nonce: nonce,
                loginHint: loginHint,
                acrValues: acrValues,
                responseMode: responseMode,
                codeChallenge: codeChallenge,
                codeChallengeMethod: codeChallengeMethod,
                extra: extra);
            return url;
        }
```
## 解决办法
需要实现IClientStore接口，在IdentityServer.EntityFramework中已经实现了IClientStore。
解决办法：
原来是在创建mvc3test时，在页面端输入字符串时在尾部多了个空格，导致的错误。
由此问题也可以判断出在IdentityServer4中，对Client的验证是通过ClientId与redirectUri共同验证，二者都需要匹配。
