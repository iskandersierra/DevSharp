using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;

namespace Samples.Email.DAL
{
    public class EmailDB : DbContext
    {
        public EmailDB()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=.;DataBase=SampleEmailDB;Integrated Security=True");
        }

        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<File> Files { get; set; }
    }

    public class EmailAddress
    {
        [Key]
        public Guid Id { get; set; }
        public string Email { get; set; }
    }

    public class EmailMessage
    {
        [Key]
        public Guid Id { get; set; }
        public EmailAddress From { get; set; }
        public ICollection<EmailAddress> To { get; set; }
        public ICollection<EmailAddress> Cc { get; set; }
        public ICollection<EmailAddress> Bcc { get; set; }
        public string Body { get; set; }
        public ICollection<File> Attachments { get; set; }
    }

    public class File
    {
        [Key]
        public Guid Id { get; set; }
        public string SourceReference { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
        public string FileExtension { get; set; }
        public byte[] Content { get; set; }

        [NotMapped]
        public bool IsEmbedded => SourceReference == null;
    }
}
