using Decolei.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Decolei.net.Data
{
    public class DecoleiDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        // DbSets para suas tabelas de negócio
        public DbSet<PacoteViagem> PacotesViagem { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Viajante> Viajantes { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Imagem> Imagens { get; set; }

        public DecoleiDbContext(DbContextOptions<DecoleiDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Mapeamento de Usuario e Identity

            // Mapeamento da sua tabela 'Usuario' customizada
            builder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Usuario_Id");

                // Mapeamento do UserName do Identity (agora para Usuario_LoginName)
                entity.Property(e => e.UserName).HasColumnName("Usuario_LoginName").HasMaxLength(100).IsRequired();
                entity.Property(e => e.NormalizedUserName).HasColumnName("Usuario_LoginName_Normalizado").HasMaxLength(100);

                // Mapeamento do Email do Identity
                entity.Property(e => e.Email).HasColumnName("Usuario_Email").HasMaxLength(100).IsRequired();
                entity.Property(e => e.NormalizedEmail).HasColumnName("Usuario_Email_Normalizado").HasMaxLength(100);

                entity.Property(e => e.PasswordHash).HasColumnName("Usuario_Senha").HasMaxLength(255).IsRequired();
                entity.Property(e => e.PhoneNumber).HasColumnName("Usuario_Telefone").HasMaxLength(20);
                entity.Property(e => e.Documento).HasColumnName("Usuario_Documento").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Perfil).HasColumnName("Usuario_Perfil").HasMaxLength(20).IsRequired();

                // NOVO: Mapeamento da propriedade NomeCompleto para a nova coluna do banco
                entity.Property(e => e.NomeCompleto).HasColumnName("Usuario_NomeCompleto").HasMaxLength(100);

                entity.HasIndex(e => e.Email, "Usuario_Email_UQ").IsUnique();
                entity.HasIndex(e => e.Documento, "Usuario_Documento_UQ").IsUnique();
                // Ajustar ou manter índices para o UserName (agora LoginName)
                entity.HasIndex(e => e.NormalizedUserName, "Usuario_LoginName_Normalizado_UQ").IsUnique();
                entity.HasIndex(e => e.NormalizedEmail, "Usuario_Email_Normalizado_UQ").IsUnique();
            });

            // Mapeamos as entidades do Identity para as tabelas padrão que você criou.
            builder.Entity<IdentityUserClaim<int>>(entity => entity.ToTable("AspNetUserClaims"));
            builder.Entity<IdentityRole<int>>(entity => entity.ToTable("AspNetRoles"));
            builder.Entity<IdentityUserRole<int>>(entity => entity.ToTable("AspNetUserRoles"));
            builder.Entity<IdentityRoleClaim<int>>(entity => entity.ToTable("AspNetRoleClaims")); // <--- ESSA LINHA NÃO É MAIS IGNORADA

            // Continuamos ignorando as outras tabelas que não estamos usando para evitar erros.
            builder.Ignore<IdentityUserLogin<int>>();
            builder.Ignore<IdentityUserToken<int>>();
            #endregion

            #region Mapeamento das Outras Entidades

            // ... (Seu mapeamento das outras entidades: PacoteViagem, Reserva, Viajante, Pagamento, Avaliacao) ...
            // Este trecho não foi alterado nas últimas discussões, então mantenha o que você já tem aqui.

            // --- MAPEAMENTO DA ENTIDADE 'PacoteViagem' ---
            builder.Entity<PacoteViagem>(entity =>
            {
                entity.ToTable("PacoteViagem");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("PacoteViagem_Id");
                entity.Property(e => e.Titulo).HasColumnName("PacoteViagem_Titulo").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descricao).HasColumnName("PacoteViagem_Descricao").HasMaxLength(500);
                entity.Property(e => e.Destino).HasColumnName("PacoteViagem_Destino").HasMaxLength(100);
                entity.Property(e => e.Valor).HasColumnName("PacoteViagem_Valor").HasColumnType("decimal(10, 2)");
                entity.Property(e => e.DataInicio).HasColumnName("PacoteViagem_DataInicio").HasColumnType("datetime");
                entity.Property(e => e.DataFim).HasColumnName("PacoteViagem_DataFim").HasColumnType("datetime");

                // --- INÍCIO DA MUDANÇA ---
                // Adicione a coluna para a chave estrangeira
                entity.Property(e => e.UsuarioId).HasColumnName("Usuario_Id").IsRequired();

                // Configura a relação com a tabela Usuario
                entity.HasOne(p => p.Usuario)
                      .WithMany(u => u.PacotesCriados) // um usuário tem muitos pacotes criados
                      .HasForeignKey(p => p.UsuarioId) // a chave estrangeira é UsuarioId
                      .OnDelete(DeleteBehavior.ClientSetNull) // Evita deleção em cascata
                      .HasConstraintName("PacoteViagem_Usuario_FK");
                // --- FIM DA MUDANÇA ---

            });

            // --- MAPEAMENTO DA NOVA ENTIDADE 'Imagem' ---
            builder.Entity<Imagem>(entity =>
            {
                entity.ToTable("Imagem");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Imagem_Id");
                entity.Property(e => e.Url).HasColumnName("Imagem_Url").HasMaxLength(255).IsRequired();

                // LINHA ADICIONADA PARA CONFIGURAR A NOVA PROPRIEDADE
                entity.Property(e => e.IsVideo).HasColumnName("Imagem_IsVideo").IsRequired();

                entity.Property(e => e.PacoteViagemId).HasColumnName("PacoteViagem_Id").IsRequired();

                entity.HasOne(i => i.PacoteViagem)
                      .WithMany(p => p.Imagens)
                      .HasForeignKey(i => i.PacoteViagemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // --- MAPEAMENTO DA ENTIDADE 'Reserva' ---
            builder.Entity<Reserva>(entity =>
            {
                entity.ToTable("Reserva");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Reserva_Id");
                entity.Property(e => e.Usuario_Id).HasColumnName("Usuario_Id").IsRequired();
                entity.Property(e => e.PacoteViagem_Id).HasColumnName("PacoteViagem_Id").IsRequired();
                entity.Property(e => e.Data).HasColumnName("Reserva_Data").HasColumnType("date");
                entity.Property(e => e.ValorTotal).HasColumnName("Reserva_ValorTotal").HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Status).HasColumnName("Reserva_Status").HasMaxLength(20);
                entity.Property(e => e.Numero).HasColumnName("Reserva_Numero").HasMaxLength(50);
                entity.HasIndex(e => e.Numero, "Reserva_Numero_UQ").IsUnique();

                entity.HasOne(d => d.Usuario).WithMany(p => p.Reservas).HasForeignKey(d => d.Usuario_Id).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("Reserva_Usuario_FK");
                entity.HasOne(d => d.PacoteViagem).WithMany(p => p.Reservas).HasForeignKey(d => d.PacoteViagem_Id).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("Reserva_PacoteViagem_FK");
            });

            // --- MAPEAMENTO DA ENTIDADE 'Viajante' ---
            builder.Entity<Viajante>(entity =>
            {
                entity.ToTable("Viajante");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Viajante_Id");
                entity.Property(e => e.Reserva_Id).HasColumnName("Reserva_Id").IsRequired();
                entity.Property(e => e.Nome).HasColumnName("Viajante_Nome").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Documento).HasColumnName("Viajante_Documento").HasMaxLength(50).IsRequired();
                entity.HasOne(d => d.Reserva).WithMany(p => p.Viajantes).HasForeignKey(d => d.Reserva_Id).HasConstraintName("Viajante_Reserva_FK");
            });

            // --- MAPEAMENTO DA ENTIDADE 'Pagamento' ---
            builder.Entity<Pagamento>(entity =>
            {
                entity.ToTable("Pagamento");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Pagamento_Id");
                entity.Property(e => e.Reserva_Id).HasColumnName("Reserva_Id").IsRequired();
                entity.Property(e => e.Forma).HasColumnName("Pagamento_Forma").HasMaxLength(20);
                entity.Property(e => e.Status).HasColumnName("Pagamento_Status").HasMaxLength(20);
                entity.Property(e => e.ComprovanteURL).HasColumnName("Pagamento_ComprovanteURL").HasMaxLength(255);
                entity.Property(e => e.Data).HasColumnName("Pagamento_Data").HasColumnType("date");
                entity.HasOne(d => d.Reserva).WithMany(p => p.Pagamentos).HasForeignKey(d => d.Reserva_Id).HasConstraintName("Pagamento_Reserva_FK");
            });

            // --- MAPEAMENTO DA ENTIDADE 'Avaliacao' ---
            builder.Entity<Avaliacao>(entity =>
            {
                entity.ToTable("Avaliacao");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Avaliacao_Id");
                entity.Property(e => e.Usuario_Id).HasColumnName("Usuario_Id").IsRequired();
                entity.Property(e => e.PacoteViagem_Id).HasColumnName("PacoteViagem_Id").IsRequired();
                entity.Property(e => e.Nota).HasColumnName("Avaliacao_Nota");
                entity.Property(e => e.Comentario).HasColumnName("Avaliacao_Comentario").HasMaxLength(500);
                entity.Property(e => e.Data).HasColumnName("Avaliacao_Data").HasColumnType("date");
                entity.Property(e => e.Aprovada).HasColumnName("Avaliacao_Aprovada");
                entity.HasOne(d => d.Usuario).WithMany(p => p.Avaliacoes).HasForeignKey(d => d.Usuario_Id).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("Avaliacao_Usuario_FK");
                entity.HasOne(d => d.PacoteViagem).WithMany(p => p.Avaliacoes).HasForeignKey(d => d.PacoteViagem_Id).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("Avaliacao_PacoteViagem_FK");
            });
            #endregion


        }
    }
}