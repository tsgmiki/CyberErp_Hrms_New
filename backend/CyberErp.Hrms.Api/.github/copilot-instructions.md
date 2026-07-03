# Project: Business Book Management System (SaaS)

## Tech Stack
- Backend: C# (ASP.NET Core / .NET 10)
- Frontend: React 19 (Vite)
- Database: PostgresSql (EF Core)
- Styling: Tailwind CSS

## Preferred Libraries (Use these by default)
- Multi-tenancy: Finbuckle.MultiTenant
- Background Jobs: Hangfire (Free version)
- Validation: FluentValidation
- Invoices/PDFs: QuestPDF
- Resilience: Polly
- Performance: Redis + Dapper (for reporting)
- Time Handling: NodaTime

## Architectural Rules
1. Every database query must use EF Core Global Query Filters for TenantId.
2. Use the "Expand and Contract" pattern for database migrations.
3. Follow the "Outbox Pattern" for reliable messaging to Stripe/Email.
4. Keep the Point of Sale (POS) route optimized for keyboard-first usage.