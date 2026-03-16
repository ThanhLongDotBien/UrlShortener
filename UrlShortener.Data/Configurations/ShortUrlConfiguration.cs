using System;
using System.Collections.Generic;
using System.Text;
using UrlShortener.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UrlShortener.Data.Configurations
{
    public class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
    {
        public void Configure(EntityTypeBuilder<ShortUrl> builder)
        {
            builder.ToTable("ShortUrls");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ClickCount)
                .IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique();
        }
    }
}
