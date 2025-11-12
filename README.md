<h1>Demo for Microservices using Ocelot and SQL</h1>

Added postman collections for both gateway and direct apis. 

<h3>Steps to Run the Solution</h3>

Configure Database Connections<br>
Open the following files and update the connection strings to match your environment:<br>

ProductService->Models -> DbContext file<br>
OrdersService->Model -> DbContext file<br>
Run Multiple Instances of OrderService<br>
Navigate to the OrdersService project directory in File Explorer.<br>
Open PowerShell in that directory. <br>
Example:
e.g C:\Users\user\source\repos_nov\MicroServicesWithDatabase\OrdersService><br>

Run the first instance: dotnet run --urls https://localhost:5004<br>

Open another PowerShell window and run the second instance: dotnet run --urls https://localhost:5003<br>

Verify both instances are running. You should see output like: Microsoft.Hosting.Lifetime[14] Now listening on: https://localhost:5004 info: Microsoft.Hosting.Lifetime[0] Application started. Press Ctrl+C to shut down. info: Microsoft.Hosting.Lifetime[0] Hosting environment: Development info: Microsoft.Hosting.Lifetime[0] Content root path: C:\Users\user\source\repos_nov\MicroServicesWithDatabase\OrdersService<br>

Configure and Run the Solution in Visual Studio<br>
Open the solution in Visual Studio.<br>

Ensure multiple startup projects are selected:<br>

API_GATEWAY<br>
OrderService<br>
ProductService<br>
Run the solution.<br>

Verify Ocelot Configuration<br>
Make sure the port numbers in ocelot.json match the ports on which your services are running (e.g., 5003 and 5004 for OrderService).<br>
Test the Load Balancing<br>
Use Postman or any API client to call the API via the gateway: e.g https://localhost:5105/api/Orders<br>
Note: 7157 is the API Gateway port. Make sure it matches the port configured in your project.
