The project is for ASP.NET applications, Web API's, and Blazors app that can use authentication provider as ADFS WS-Federation and working with PingFederate authentication provider servers.


This project is a Nuget can use directly in ASP.NET application and will list the step how to add authentication to the Blazor Wasm with Server Hosted.

First, You need to know all the information that is related with PingFederate server and how to interact with your ASP.NET application,

Ask the administrator for the following:

1- Create domain (urn) for your web site inside the PingFederate server, like if you need to connect your website https://yourweb.company.com then you need to get an Idp like **urn:yourweb:company:com**, this will be considered as **realm** for your application [PingFederate Idp](https://support.pingidentity.com/s/article/Configure-ADFS-as-IdP-using-WS-Fed)  (I'll explain later).

2- Ask for the full path to the authentication provider url, it will be like "https://company.com:[port]/idp/prp.wsf" the default port is 9031 but you need to know that from your Authentication Provider admin [more information](https://proofid.com/blog/single-sign-on-to-outlook-web-access-using-pingfederate/)

3- Need to create high trust certificate
Second, Create a Blazor Wasm project, and in the Server part of the project add the following:


**Server Project:**

* Add MasterBlazor.AspNetCore.Authentication Nuget

* Open the Server.csproj file, Add the following:
```xml
<ItemGroup>
	<Folder Include="Controllers\" />
</ItemGroup>
```

* appsetting.json



//after "AllowedHosts": "*" add the PingFederate information:
```json
  "issuer": "https://[YOUR_COMPANY_AUTH_SERVER]:[port]/idp/prp.wsf",
  "validIssuer": "urn:[your_company_domain]:[com or net]",
  "metadataAddress": "https://[YOUR_COMPANY_AUTH_SERVER]:[port]/[folder]/metadata.xml",
  "realm": "urn:[your_host_site]:[maybe sub domain]:[com or net or ...]",
  "certificatePath": "[local path]\\[certificate name].cer",
  "validAudience": "urn:[your_host_site]:[maybe sub domain]:[com or net or ...]"
```
an example to these information is:
```json
  "issuer": "https://company.net:9031/idp/prp.wsf",
  "validIssuer": "urn:company:net",
  "metadataAddress": "https://ping.company.net:9031/files/metadata.xml",
  "realm": "urn:yourweb:company:com",
  "certificatePath": "C:\\my certificate folder\\mystrusted.cer",
  "validAudience": "urn:yourweb:company:com"
```




* Open the Program.cs:

```csharp
// Add services to the container.
MasterBlazor.Authentication.Authentication.Auth(builder.Services, new MasterBlazor.Authentication.AuthenticationOption
{
    Issuer = builder.Configuration.GetValue<string>("issuer"),
    ValidIssuer = builder.Configuration.GetValue<string>("validIssuer"),
    Wtrealm = builder.Configuration.GetValue<string>("realm"),
    ValidAudience = builder.Configuration.GetValue<string>("validAudience"),
    MetadataAddress = builder.Configuration.GetValue<string>("metadataAddress"),
    CertificatePath = builder.Configuration.GetValue<string>("certificatePath")
});
```

* Add the 2 lines as well:
```csharp
//Add after routing
app.UseAuthentication();
app.UseAuthorization();
```

**Client Project:**

* Add the folder "Authentication" to the project

* Add UserState.razor page to the that folder
```csharp

@inject HttpClient Http

<div style="float:right;" class="userState">
    <ul class="nav">
        @if (isAuthenticated)
        {
            <li class="nav-link">
                <div class="loginuser" upn="@userName">@userName</div>
            </li>
            <li class="nav-link">
                <a href="/Account/SignOut" id="loginLink">Sign out</a>
            </li>
        }
        else
        {
            <li class="nav-link">
                <a href="/Account/SignIn" id="loginLink">Sign in</a>
            </li>
        }
    </ul>
</div>

```
and this is code part:

```csharp
@code {

    private bool isAuthenticated = false;
    private string userName{get;set;}

    protected override async Task OnInitializedAsync()
    {
        userName="";
        var ret = await Http.GetStringAsync("Account/IsAuthenticated");
        if (ret != "")
        {
            isAuthenticated = true;
            userName = ret;
        }
        else
            isAuthenticated = false;
    }
}



```


Add <UserStatus /> to the layout page
```charp
<MasterBlazor.App.Client.Authentication.UserState/>
```

When signout it will return to the home page, but in case a special page requires just add another .razor inside the "Authenticaion" folder:


Signout.razor:

```csharp
@page "/signout"
<h2><a href="/">Home</a></h2>
<h3>You are signed out successfully.</h3>

```
