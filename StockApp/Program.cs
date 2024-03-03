using StockApp.Utilities;
using System.Collections.Generic;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

HttpClient test = new HttpClient();
String jsonResponse = await await FinancialData.requestJSONDump(test,320193);
Console.WriteLine(jsonResponse);
//FinancialData.parseFinancials(jsonResponse);

//List<String> names = await FinancialData.GetFieldNames("Resources/GAAPTemplates/2009/stm/us-gaap-stm-sfp-cls-def-2009-01-31.xml");
//foreach (string name in names) {
//    Console.WriteLine(name);
//}

/*
Dictionary<string, List<Statement>> mappings = await FinancialData.GetGaapMappings(2009);
foreach (KeyValuePair<string, List<Statement>> pair in mappings)
{

    Console.WriteLine("First : " + pair.Key);
    List<Statement> statements = pair.Value;
    for (int i = 0; i < statements.Count; i++)
    {
        if (i > 0)
        {
            Console.Write("Multiple: ");
        }
        Console.WriteLine(statements[i]);
    }
    Console.WriteLine("***************");
    Console.WriteLine("***************");
}
*/

app.Run();
