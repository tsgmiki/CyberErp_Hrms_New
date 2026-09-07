using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Hrms.EmployeeExperience: adds Salary — the pay in that role (2026-09-02).
    /// </summary>
    /// <remarks>
    /// <para>⚠️ NULLABLE, with no default. Every existing row genuinely has no recorded salary, and
    /// for external history most never will: a candidate is under no obligation to disclose what a
    /// previous employer paid. Defaulting to 0 would assert "unpaid", which is a different claim
    /// from "not recorded", and it is the kind of difference that later reads as data rather than as
    /// a gap.</para>
    ///
    /// <para>decimal(18,2) matches Hrms.Employee.Salary deliberately — the two are compared when
    /// looking at a person's pay history, and money that rounds differently depending on which table
    /// it came from is worse than money that is missing.</para>
    ///
    /// <para>The column serves BOTH writers of this table: the employee Experience form and the
    /// candidate Experience form (they share the entity), plus the rows auto-registered from an
    /// employee movement, which fill it from <c>movement.FromSalary</c> — the pay of the role being
    /// left, not the one being taken.</para>
    /// </remarks>
    // Hand-written, so it carries its own [Migration] attribute: EF discovers migrations by that
    // attribute, which the scaffolder normally emits into the generated .Designer.cs. Without it the
    // file compiles, sits in the folder, and is silently never applied.
    //
    // Written by hand rather than scaffolded because the installed EF tools (9.0.5) are a major
    // version behind the runtime, and `migrations add` in that state does not update
    // HrmsDbContextModelSnapshot — leaving the next scaffold to "rediscover" this column and emit it
    // a second time. The snapshot is updated alongside this file instead.
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260902120000_AddEmployeeExperienceSalary")]
    public partial class AddEmployeeExperienceSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                schema: "Hrms",
                table: "EmployeeExperience",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                schema: "Hrms",
                table: "EmployeeExperience");
        }
    }
}
