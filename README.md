# ASP.NET Core Microservices (NET 8) — Project Overview

A learning implementation of microservices using .NET (upgraded to .NET 8) inspired by the "DotNet Mastery / Bhrugen Patel" course (Mango Microservices style). This repository demonstrates building a small e-commerce platform by decomposing functionality into multiple microservices, using synchronous and asynchronous communication, and applying clean architecture principles.

> Note: This repo is my course project from DotNet Mastery / Udemy and contains multiple microservices, an API Gateway (Ocelot), and an MVC front-end. I completed the full course and hold the certificate linked below.

Course / Resources
- Udemy course: .NET Core Microservices - The Complete Guide (.NET 8 MVC)  
  https://www.udemy.com/course/net-core-microservices-the-complete-guide-net-6-mvc/?couponCode=DNM_202510_LOW
- YouTube intro: https://www.youtube.com/watch?v=Nw4AZs1kLAs
- Original instructor / project hub: https://dotnetmastery.com/ and https://github.com/bhrugen

Certificate
- Udemy Certificate: https://www.udemy.com/certificate/UC-29f57052-58d3-4534-ab25-d45dfd4bd558/ 
<hr>
<!-- 
![UC-29f57052-58d3-4534-ab25-d45dfd4bd558](https://udemy-certificate.s3.amazonaws.com/image/UC-29f57052-58d3-4534-ab25-d45dfd4bd558.jpg) -->

![UC-29f57052-58d3-4534-ab25-d45dfd4bd558](https://github.com/user-attachments/assets/5eed4673-39a5-4a32-83f1-4b3f856ea3c6)

<br>

Table of contents
- Project vision
- Architecture & components
- Services implemented
- Key technologies
- Local setup (quickstart)
- Environment & configuration
- Running with Docker (basic)
- Development tips & notes
- How to contribute or personalize this README
- Contact & social links

---

## Project vision
The goal of this repository is to:
- Learn and demonstrate microservices concepts in .NET 8
- Apply Clean Architecture and SOLID principles
- Show synchronous (HTTP/gRPC) and asynchronous (Azure Service Bus) communication patterns
- Implement authentication/authorization via a dedicated Identity microservice
- Build a simple e-commerce flow: products → cart → orders → payment → email notifications

This implementation is primarily educational and intended to be a reference for structure, patterns, and integration points.

---

## Architecture & components
High-level architecture:
- Multiple independent microservices (each with its own database and bounded context)
- Ocelot API Gateway sits in front of public-facing APIs (routing, aggregation)
- .NET Identity microservice for user registration, login, and JWT issuance
- Azure Service Bus for asynchronous messaging between services (e.g., orders → email)
- MVC Web App consumes APIs and demonstrates end-to-end flows
- Containerizable services for local/dev Docker use and cloud deployment (ECS, AKS, etc.)

Diagram (conceptual)
- Client (MVC / SPA) → Ocelot Gateway → Product API, Cart API, Order API, Payment API, Coupon API, Identity API
- Order API publishes messages to Service Bus → Email API listens and sends notifications

---

## Services included (typical)
- Product Microservice
- .NET Identity Microservice (authentication & authorization)
- Coupon Microservice
- Shopping Cart Microservice
- Order Microservice
- Email (Notification) Microservice
- Payment Microservice
- Ocelot API Gateway
- MVC Web Application (frontend demo)

(If any service in your fork differs, adapt the names above to match the repository.)

---

## Key technologies
- .NET 8 / ASP.NET Core
- C#, Entity Framework Core, LINQ
- Clean Architecture (layered: API → Application → Domain → Infrastructure)
- Authentication: ASP.NET Core Identity / JWT
- API Gateway: Ocelot
- Messaging: Azure Service Bus (pub/sub)
- Persistence: SQL Server (per-service DB)
- Frontend: ASP.NET Core MVC (sample) / or Angular (optional)
- DevOps: Docker, Docker Compose, GitHub Actions (recommended)
- Observability: OpenTelemetry / logging (recommended)

---

## Local setup (quickstart)

Prerequisites
- .NET 8 SDK
- SQL Server (local or Docker)
- (Optional) Docker & Docker Compose
- (Optional) Azure Service Bus (or local emulator / replacement) — you may switch to a local queue for development
- A code editor (VS Code / Visual Studio)

1. Clone the repository
```bash
git clone https://github.com/Deboraj-roy/ASP.NET-Core-Microservices-NET-8.git
cd ASP.NET-Core-Microservices-NET-8
```

2. Per-service steps
- Each microservice usually has its own folder (e.g., ProductAPI, IdentityAPI, CartAPI, OrderAPI, PaymentAPI, EmailAPI, OcelotGateway, WebApp).
- For each service:
  - Restore & build
    ```bash
    cd ProductAPI
    dotnet restore
    dotnet build
    ```
  - Update configuration (see Environment & configuration below)
  - Apply EF Core migrations (if provided) or run `dotnet ef database update` in the appropriate project
    ```bash
    dotnet ef database update --project ./Infrastructure --startup-project ./ProductAPI
    ```
  - Run the service
    ```bash
    dotnet run --project ./ProductAPI
    ```

3. Run the Ocelot API Gateway and MVC Web App last so they can consume the running APIs.

---

## Environment & configuration (common keys)
Each microservice typically expects environment variables or appsettings values. Example values to set:

- ConnectionStrings: (per-service)
  - DefaultConnection: "Server=localhost;Database=ProductDb;User Id=sa;Password=Your_password123;"
- Identity / JWT:
  - Jwt:Key: "your-very-strong-key"
  - Jwt:Issuer: "your-issuer"
  - Jwt:Audience: "your-audience"
- Azure Service Bus (async messaging):
  - ServiceBusConnectionString: "Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=..."
  - OrderTopicName / SubscriptionName: configurable names used by publish/subscribe
- SMTP (Email API):
  - Smtp:Host, Smtp:Port, Smtp:User, Smtp:Password
- Payment: test-mode keys or provider configuration

Example appsettings snippet
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ServiceDb;User Id=sa;Password=Your_password123;"
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_A_STRONG_KEY",
    "Issuer": "MyIssuer",
    "Audience": "MyAudience",
    "ExpiryMinutes": 60
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=..."
  }
}
```

Security note: never commit secrets. Use user secrets, environment variables, or a secrets manager in CI/CD.

---

## Running with Docker (basic)
If Docker Compose files are present, you can start the stack via:
```bash
docker-compose up --build
```
A minimal recommended Compose setup should include:
- SQL Server container(s) (or a single shared server for local learning)
- A container for each microservice (Product, Identity, Cart, Order, Payment, Email)
- Ocelot Gateway
- (Optional) a local Service Bus emulator or connect to Azure Service Bus

For production, keep databases and message broker managed (Azure SQL / RDS, Azure Service Bus / RabbitMQ).

---

## Development notes, best practices & suggestions
- Keep each microservice small, with a single responsibility and its own database.
- Use retries, exponential backoff, and idempotency for inter-service communication.
- Use DTOs and mapping layers (AutoMapper or custom mappers) between API models and domain models.
- Centralize configuration and secrets using environment variables or services like Azure Key Vault.
- Add health-check endpoints (/health) for each service and configure the gateway and orchestrator to monitor them.
- Add OpenAPI (Swagger) for each service for easier testing and integration.
- Add automated tests: unit tests for domain/application layers and integration tests for service contracts.

---

## What I learned (personal notes)
- How to split an e-commerce app into multiple microservices (Products, Cart, Orders, Payments, Email, Identity).
- How to use .NET Identity in a microservice to issue JWTs and secure internal/external APIs.
- How to use Ocelot as an API Gateway for routing and basic aggregation.
- How to use Azure Service Bus for asynchronous communication and decoupling services.
- Applying Clean Architecture to keep API, application, and infrastructure concerns separated.

---

## Credits & references
- Course author: Bhrugen Patel (DotNet Mastery) — https://dotnetmastery.com/
- Original course GitHub examples: https://github.com/bhrugen
- Udemy course: https://www.udemy.com/course/net-core-microservices-the-complete-guide-net-6-mvc/

---

## Contributing / Personalization
This repo reflects my course work and experiments. If you want the README or project to highlight:
- Your name and role
- Certificate and completion date (I completed the course and have the certificate)
- Favorite services or demos (which ones you'd like to highlight)
- Social links / contact details

I can generate a polished profile README for the repository that includes badges, screenshots, step-by-step run instructions for each project folder, improved Docker Compose, GitHub Actions CI workflow, or a short demo script.

---

## Contact & socials
- GitHub: https://github.com/Deboraj-roy
- Udemy Certificate: https://www.udemy.com/certificate/UC-29f57052-58d3-4534-ab25-d45dfd4bd558/
- (Add LinkedIn / Twitter / personal website here if you want them displayed)

---

## Next steps — tell me what to include
Please reply with any of the details below you want me to add so I can personalize the README and produce a final polished version:

- Full name for the header (how you'd like to appear)
- Professional title (e.g., "Full Stack .NET Developer")
- Short one-line bio or tagline
- Up to 6 top skills (e.g., C#, .NET 8, EF Core, Azure Service Bus, Docker, Angular)
- 3–5 favorite project folders (exact repo paths) and a 1–2 sentence description for each to feature
- Public links to include: personal website, LinkedIn, Twitter, email
- Preferred "how to run" style: raw dotnet commands, Docker Compose, or Kubernetes manifests
- Whether to include GitHub Actions CI example and/or docker-compose.yml improvements
- Do you want badges (GitHub stats, Udemy certificate, build status)?
- A profile photo/avatar URL (optional)
- Any screenshots or GIFs you want embedded

Reply with the details you want included and I'll generate a refined, production-ready README.md tailored to you.
