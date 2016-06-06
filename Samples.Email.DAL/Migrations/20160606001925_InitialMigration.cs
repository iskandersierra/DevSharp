using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Samples.Email.DAL.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAddress",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    EmailMessageId = table.Column<Guid>(nullable: true),
                    EmailMessageId1 = table.Column<Guid>(nullable: true),
                    EmailMessageId2 = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAddress", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "EmailMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    FromId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessage_EmailAddress_FromId",
                        column: x => x.FromId,
                        principalTable: "EmailAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    EmailMessageId = table.Column<Guid>(nullable: true),
                    FileExtension = table.Column<string>(nullable: true),
                    Length = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SourceReference = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_File", x => x.Id);
                    table.ForeignKey(
                        name: "FK_File_EmailMessage_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.AddForeignKey(
                name: "FK_EmailAddress_EmailMessage_EmailMessageId",
                table: "EmailAddress",
                column: "EmailMessageId",
                principalTable: "EmailMessage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_EmailAddress_EmailMessage_EmailMessageId1",
                table: "EmailAddress",
                column: "EmailMessageId1",
                principalTable: "EmailMessage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_EmailAddress_EmailMessage_EmailMessageId2",
                table: "EmailAddress",
                column: "EmailMessageId2",
                principalTable: "EmailMessage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EmailMessage_EmailAddress_FromId", table: "EmailMessage");
            migrationBuilder.DropTable("File");
            migrationBuilder.DropTable("EmailAddress");
            migrationBuilder.DropTable("EmailMessage");
        }
    }
}
