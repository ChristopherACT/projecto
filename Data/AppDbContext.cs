using Azure;
using FG_Corporation.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Globalization;
//using System.Text.RegularExpressions;

namespace FG_Corporation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Follow> Follows { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupMember> GroupMembers { get; set; } = null!;
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<PostFile> PostFiles { get; set; } = null!;
        public DbSet<PostVersion> PostVersions { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<PostTag> PostTags { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<Download> Downloads { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Server> Servers { get; set; } = null!;
        public DbSet<ServerVote> ServerVotes { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<ForumCategory> ForumCategories { get; set; } = null!;
        public DbSet<ForumThread> ForumThreads { get; set; } = null!;
        public DbSet<ForumReply> ForumReplies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===================================================
            // IMPORTANTE: SQL Server no permite multiples "cascade
            // paths" hacia la misma tabla. Como Users llega a tablas
            // como Likes/Comments/etc de dos formas (directo, y a
            // traves de Posts), hay que poner Restrict en la FK de
            // Usuario para evitar el error al crear la migracion.
            // ===================================================

            // ---------- Follows (auto-referencia a Users) ----------
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Followed)
                .WithMany()
                .HasForeignKey(f => f.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Groups ----------
            modelBuilder.Entity<Group>()
                .HasOne(g => g.CreatedBy)
                .WithMany(u => u.GroupsCreated)
                .HasForeignKey(g => g.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Posts ----------
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Posts)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Game)
                .WithMany(g => g.Posts)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PostFile>()
                .HasOne(pf => pf.Post)
                .WithMany(p => p.Files)
                .HasForeignKey(pf => pf.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostVersion>()
                .HasOne(pv => pv.Post)
                .WithMany(p => p.Versions)
                .HasForeignKey(pv => pv.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Tags ----------
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.Tags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.Posts)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Interacciones (Post cascade, User restrict) ----------
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Download>()
                .HasOne(d => d.Post)
                .WithMany(p => p.Downloads)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Download>()
                .HasOne(d => d.User)
                .WithMany(u => u.Downloads)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Servers ----------
            modelBuilder.Entity<Server>()
                .HasOne(s => s.User)
                .WithMany(u => u.Servers)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Server>()
                .HasOne(s => s.Game)
                .WithMany(g => g.Servers)
                .HasForeignKey(s => s.GameId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Server>()
                .HasOne(s => s.Group)
                .WithMany(g => g.Servers)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ServerVote>()
                .HasOne(sv => sv.Server)
                .WithMany(s => s.Votes)
                .HasForeignKey(sv => sv.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServerVote>()
                .HasOne(sv => sv.User)
                .WithMany(u => u.ServerVotes)
                .HasForeignKey(sv => sv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Chat (dos FKs a Users en Conversation) ----------
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.MessagesSent)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Foro ----------
            modelBuilder.Entity<ForumThread>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Threads)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumThread>()
                .HasOne(t => t.User)
                .WithMany(u => u.ForumThreads)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ForumReply>()
                .HasOne(r => r.Thread)
                .WithMany(t => t.Replies)
                .HasForeignKey(r => r.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumReply>()
                .HasOne(r => r.User)
                .WithMany(u => u.ForumReplies)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Indices utiles ----------
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Like>().HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
            modelBuilder.Entity<Follow>().HasIndex(f => new { f.FollowerId, f.FollowedId }).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}

