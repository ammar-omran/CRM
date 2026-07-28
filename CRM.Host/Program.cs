var builder = DistributedApplication.CreateBuilder(args);


var users = builder.AddProject<Projects.Modules_Users_API>("users");
var customers = builder.AddProject<Projects.Modules_Customers_API>("customers");
var organizations = builder.AddProject<Projects.Modules_Organizations_API>("organizations");
var tickets = builder.AddProject<Projects.TicketManagement_API>("tickets");

var crmBase = builder.AddProject<Projects.CRM_Base>("base")
    .WithReference(users)
    .WithReference(customers)
    .WithReference(organizations)
    .WithReference(tickets);


builder.Build().Run();
