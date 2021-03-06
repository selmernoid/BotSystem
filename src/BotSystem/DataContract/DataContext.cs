namespace DataContract {
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DataContext : DbContext {
        public DataContext()
            : base("name=DataContext") {
        }

        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserComment> UserComments { get; set; }
        public virtual DbSet<Community> Communities { get; set; }

        public virtual DbSet<PostTag> Tags { get; set; }
        public virtual DbSet<PostLink> PostLinks { get; set; }
        public virtual DbSet<CommentLink> CommentLinks { get; set; }

        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
                .Property(e => e.Name)
                .IsFixedLength();

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserComments)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);
        }
    }
}
