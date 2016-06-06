using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Samples.Email.DAL;

namespace Samples.Email.DAL.Migrations
{
    [DbContext(typeof(EmailDB))]
    partial class EmailDBModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Samples.Email.DAL.EmailAddress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<Guid?>("EmailMessageId");

                    b.Property<Guid?>("EmailMessageId1");

                    b.Property<Guid?>("EmailMessageId2");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Samples.Email.DAL.EmailMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<Guid?>("FromId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Samples.Email.DAL.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Content");

                    b.Property<Guid?>("EmailMessageId");

                    b.Property<string>("FileExtension");

                    b.Property<long>("Length");

                    b.Property<string>("Name");

                    b.Property<string>("SourceReference");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Samples.Email.DAL.EmailAddress", b =>
                {
                    b.HasOne("Samples.Email.DAL.EmailMessage")
                        .WithMany()
                        .HasForeignKey("EmailMessageId");

                    b.HasOne("Samples.Email.DAL.EmailMessage")
                        .WithMany()
                        .HasForeignKey("EmailMessageId1");

                    b.HasOne("Samples.Email.DAL.EmailMessage")
                        .WithMany()
                        .HasForeignKey("EmailMessageId2");
                });

            modelBuilder.Entity("Samples.Email.DAL.EmailMessage", b =>
                {
                    b.HasOne("Samples.Email.DAL.EmailAddress")
                        .WithMany()
                        .HasForeignKey("FromId");
                });

            modelBuilder.Entity("Samples.Email.DAL.File", b =>
                {
                    b.HasOne("Samples.Email.DAL.EmailMessage")
                        .WithMany()
                        .HasForeignKey("EmailMessageId");
                });
        }
    }
}
