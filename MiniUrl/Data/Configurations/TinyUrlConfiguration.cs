using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniUrl.Entities;

namespace MiniUrl.Data.Configurations;

public class TinyUrlConfiguration : IEntityTypeConfiguration<TinyUrl>
{
    public void Configure(EntityTypeBuilder<TinyUrl> builder)
    {
        builder.ToTable("TinyUrls");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Url)
            .IsRequired()
            .HasColumnType("varchar(2000)");
        builder.Property(t => t.ShortenedUrl)
            .IsRequired()
            .HasColumnType("varchar(300)");
        builder.Property(t => t.Description)
            .HasColumnType("varchar(2000)");
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnType("varchar(50)");
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        builder.Property(t => t.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        builder.Property(t => t.ExpiresAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        // tiny url column index
        builder.HasIndex(t => t.ShortenedUrl)
            .IsUnique();
        // define relationships
        builder.HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .IsRequired();
        builder.HasIndex(t => t.CreatorId);
        builder.HasOne(t => t.Approver)
            .WithMany()
            .HasForeignKey(t => t.ApproverId)
            .IsRequired(false); // this field is present only after Admin has approved
        builder.HasIndex(t => t.ApproverId);
    }
}
