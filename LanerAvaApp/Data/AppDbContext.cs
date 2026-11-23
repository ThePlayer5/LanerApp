using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LanerAvaApp.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alphabet> Alphabets { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<DifficultyLevel> DifficultyLevels { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<LearnedWord> LearnedWords { get; set; }

    public virtual DbSet<LearningProgress> LearningProgresses { get; set; }

    public virtual DbSet<LearningStat> LearningStats { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Symbol> Symbols { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersLang> UsersLangs { get; set; }

    public virtual DbSet<Word> Words { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LanerDB;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alphabet>(entity =>
        {
            entity.HasKey(e => e.IdAlphabet).HasName("alphabets_pkey");

            entity.ToTable("alphabets");

            entity.Property(e => e.IdAlphabet).HasColumnName("id_alphabet");
            entity.Property(e => e.LangId).HasColumnName("lang_id");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Lang).WithMany(p => p.Alphabets)
                .HasForeignKey(d => d.LangId)
                .HasConstraintName("alphabets_lang_id_fkey");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.IdChat).HasName("chats_pkey");

            entity.ToTable("chats");

            entity.Property(e => e.IdChat).HasColumnName("id_chat");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Chats)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("chats_user_id_fkey");
        });

        modelBuilder.Entity<DifficultyLevel>(entity =>
        {
            entity.HasKey(e => e.IdLevel).HasName("difficulty_levels_pkey");

            entity.ToTable("difficulty_levels");

            entity.Property(e => e.IdLevel).HasColumnName("id_level");
            entity.Property(e => e.Difficulty).HasColumnName("difficulty");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.IdLang).HasName("languages_pkey");

            entity.ToTable("languages");

            entity.Property(e => e.IdLang).HasColumnName("id_lang");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<LearnedWord>(entity =>
        {
            entity.HasKey(e => e.IdLearnWord).HasName("learned_words_pkey");

            entity.ToTable("learned_words");

            entity.Property(e => e.IdLearnWord).HasColumnName("id_learn_word");
            entity.Property(e => e.Timing)
                .HasDefaultValueSql("LOCALTIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timing");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WordId).HasColumnName("word_id");

            entity.HasOne(d => d.User).WithMany(p => p.LearnedWords)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("learned_words_user_id_fkey");

            entity.HasOne(d => d.Word).WithMany(p => p.LearnedWords)
                .HasForeignKey(d => d.WordId)
                .HasConstraintName("learned_words_word_id_fkey");
        });

        modelBuilder.Entity<LearningProgress>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("learning_progress");

            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.Percent)
                .HasPrecision(4, 1)
                .HasColumnName("percent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<LearningStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("learning_stats");

            entity.Property(e => e.DiffLevel).HasColumnName("diff_level");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.LearnedQuantity).HasColumnName("learned_quantity");
            entity.Property(e => e.Login).HasColumnName("login");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.Property(e => e.IdMessage).HasColumnName("id_message");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.MessageText).HasColumnName("message_text");
            entity.Property(e => e.Timing)
                .HasDefaultValueSql("LOCALTIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timing");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("messages_chat_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("messages_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Symbol>(entity =>
        {
            entity.HasKey(e => e.IdSymbol).HasName("symbols_pkey");

            entity.ToTable("symbols");

            entity.Property(e => e.IdSymbol).HasColumnName("id_symbol");
            entity.Property(e => e.AlphabetId).HasColumnName("alphabet_id");
            entity.Property(e => e.Symbol1).HasColumnName("symbol");
            entity.Property(e => e.Transliteration).HasColumnName("transliteration");

            entity.HasOne(d => d.Alphabet).WithMany(p => p.Symbols)
                .HasForeignKey(d => d.AlphabetId)
                .HasConstraintName("symbols_alphabet_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Ulogin, "users_ulogin_key").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Ulogin).HasColumnName("ulogin");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<UsersLang>(entity =>
        {
            entity.HasKey(e => e.IdUserLang).HasName("users_langs_pkey");

            entity.ToTable("users_langs");

            entity.Property(e => e.IdUserLang).HasColumnName("id_user_lang");
            entity.Property(e => e.LangId).HasColumnName("lang_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Lang).WithMany(p => p.UsersLangs)
                .HasForeignKey(d => d.LangId)
                .HasConstraintName("users_langs_lang_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UsersLangs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("users_langs_user_id_fkey");
        });

        modelBuilder.Entity<Word>(entity =>
        {
            entity.HasKey(e => e.IdWord).HasName("words_pkey");

            entity.ToTable("words");

            entity.Property(e => e.IdWord).HasColumnName("id_word");
            entity.Property(e => e.LangId).HasColumnName("lang_id");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.Transcription).HasColumnName("transcription");
            entity.Property(e => e.Translation).HasColumnName("translation");
            entity.Property(e => e.Word1).HasColumnName("word");

            entity.HasOne(d => d.Lang).WithMany(p => p.Words)
                .HasForeignKey(d => d.LangId)
                .HasConstraintName("words_lang_id_fkey");

            entity.HasOne(d => d.Level).WithMany(p => p.Words)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("words_level_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
