using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    //options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();

// SAML identity provider configuration starts here
builder.Services.BindConfig<Saml2Configuration>(builder.Configuration, "Saml2", (serviceProvider, saml2Configuration) =>
{

    saml2Configuration.SigningCertificate = CertificateUtil.Load(StoreName.My,
        StoreLocation.CurrentUser,
        X509FindType.FindByThumbprint,
        builder.Configuration.GetSection("Saml2:CertificateThumbprint").Value);
    saml2Configuration.SignAuthnRequest = true;

    saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);

    var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
    var entityDescriptor = new EntityDescriptor();
    entityDescriptor.ReadIdPSsoDescriptorFromUrlAsync(httpClientFactory, new Uri(builder.Configuration.GetSection("Saml2:IdPMetadata").Value)).GetAwaiter().GetResult();
    if (entityDescriptor.IdPSsoDescriptor != null)
    {
        saml2Configuration.AllowedIssuer = entityDescriptor.EntityId;
        saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
        saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
        foreach (var signingCertificate in entityDescriptor.IdPSsoDescriptor.SigningCertificates)
        {
            if (signingCertificate.IsValidLocalTime())
            {
                saml2Configuration.SignatureValidationCertificates.Add(signingCertificate);
            }
        }
        if (saml2Configuration.SignatureValidationCertificates.Count <= 0)
        {
            throw new Exception("The IdP signing certificates has expired.");
        }
        if (entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.HasValue)
        {
            saml2Configuration.SignAuthnRequest = entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.Value;
        }
    }
    else
    {
        throw new Exception("IdPSsoDescriptor not loaded from metadata.");
    }

    return saml2Configuration;
});


builder.Services.AddSaml2(slidingExpiration: true);
builder.Services.AddHttpClient();
// SAML identity provider configuration ends here

// ASP.NET session servcie start
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Woodgrove.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.IsEssential = true;
});
// ASP.NET session servcie end

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.Run();
