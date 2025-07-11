-- Garante que o script possa ser executado várias vezes sem erros,
-- recriando o banco de dados do zero a cada execução.
IF DB_ID('AgenciaViagens') IS NOT NULL
BEGIN
    ALTER DATABASE AgenciaViagens SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AgenciaViagens;
END
GO

-- Criação do banco de dados
CREATE DATABASE AgenciaViagens;
GO
USE AgenciaViagens;
GO

-- =================================================================
-- TABELAS DO ASP.NET IDENTITY E USUÁRIO
-- Estas tabelas são essenciais para o funcionamento da autenticação
-- e autorização da sua aplicação.
-- =================================================================

-- Tabela: Usuario (Sua tabela customizada, compatível com Identity)
CREATE TABLE Usuario (
    -- Chave primária
    Usuario_Id INT NOT NULL IDENTITY(1, 1),

    -- Suas colunas de negócio
    -- Usuario_LoginName vai armazenar o email (UserName do Identity)
    Usuario_LoginName VARCHAR(100) NOT NULL,
    Usuario_LoginName_Normalizado VARCHAR(100) NOT NULL,
    Usuario_Documento VARCHAR(20) NOT NULL,
    Usuario_Perfil VARCHAR(20) NOT NULL, -- Coluna para controle simples de perfil
    Usuario_NomeCompleto VARCHAR(100) NULL, -- NOVA COLUNA PARA NOME COMPLETO COM ESPAÇOS

    -- Colunas padrão do Identity que são independentes do UserName/Email
    Usuario_Email VARCHAR(100) NOT NULL,
    Usuario_Email_Normalizado VARCHAR(100) NOT NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    Usuario_Senha VARCHAR(255) NOT NULL, -- Armazenará o HASH da senha
    Usuario_Telefone VARCHAR(20) NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,

    -- Colunas de segurança e estado da conta
    SecurityStamp VARCHAR(255) NULL,       -- Usado para invalidar logins
    ConcurrencyStamp VARCHAR(255) NULL,    -- Para controle de concorrência
    TwoFactorEnabled BIT NOT NULL DEFAULT 0, -- Habilita autenticação de dois fatores
    LockoutEnd DATETIMEOFFSET NULL,         -- Data de fim do bloqueio da conta
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,

    -- Constraints (Regras da tabela)
    CONSTRAINT Usuario_PK PRIMARY KEY(Usuario_Id),
    CONSTRAINT Usuario_Email_UQ UNIQUE(Usuario_Email),
    CONSTRAINT Usuario_Documento_UQ UNIQUE(Usuario_Documento),
    CONSTRAINT Usuario_LoginName_Normalizado_UQ UNIQUE(Usuario_LoginName_Normalizado), -- Índice renomeado/ajustado
    CONSTRAINT Usuario_Email_Normalizado_UQ UNIQUE(Usuario_Email_Normalizado), -- Novo ou ajustado
    CONSTRAINT Usuario_Perfil_CK CHECK (Usuario_Perfil IN ('CLIENTE', 'ATENDENTE', 'ADMIN'))
);
GO

-- Tabela: AspNetRoles (Armazena os papéis/perfis da aplicação)
CREATE TABLE AspNetRoles (
    Id INT NOT NULL IDENTITY(1, 1),
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
);
GO

-- Tabela: AspNetUserRoles (Tabela de junção para o relacionamento Muitos-para-Muitos entre Usuario e AspNetRoles)
CREATE TABLE AspNetUserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Usuario_UserId FOREIGN KEY (UserId) REFERENCES Usuario(Usuario_Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

-- Tabela: AspNetUserClaims (Armazena informações adicionais, ou "claims", sobre um usuário)
CREATE TABLE AspNetUserClaims (
    Id INT NOT NULL IDENTITY(1, 1),
    UserId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_Usuario_UserId FOREIGN KEY (UserId) REFERENCES Usuario(Usuario_Id) ON DELETE CASCADE
);
GO

-- Tabela: AspNetRoleClaims (Armazena claims/declarações associadas a um papel/role)
CREATE TABLE [dbo].[AspNetRoleClaims](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [RoleId] [int] NOT NULL,
    [ClaimType] [nvarchar](max) NULL,
    [ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetRoleClaims] WITH CHECK ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO

CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
    [RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

-- =================================================================
-- TABELAS DE NEGÓCIO DA APLICAÇÃO
-- Estas são as tabelas específicas do seu sistema de agência de viagens.
-- =================================================================

-- Tabela: PacoteViagem
CREATE TABLE PacoteViagem (
    PacoteViagem_Id INT NOT NULL IDENTITY(1, 1),
    PacoteViagem_Titulo VARCHAR(100) NOT NULL,
    PacoteViagem_Descricao VARCHAR(500),
    PacoteViagem_ImagemURL VARCHAR(255),
    PacoteViagem_VideoURL VARCHAR(255),
    PacoteViagem_Destino VARCHAR(100),
    PacoteViagem_Valor DECIMAL(10,2),
    PacoteViagem_DataInicio DATETIME,
    PacoteViagem_DataFim DATETIME,
    CONSTRAINT PacoteViagem_PK PRIMARY KEY(PacoteViagem_Id)
);
GO

-- Tabela: Reserva
CREATE TABLE Reserva (
    Reserva_Id INT NOT NULL IDENTITY(1, 1),
    Usuario_Id INT NOT NULL,
    PacoteViagem_Id INT NOT NULL,
    Reserva_Data DATE,
    Reserva_ValorTotal DECIMAL(10,2),
    Reserva_Status VARCHAR(20),
    Reserva_Numero VARCHAR(50),
    CONSTRAINT Reserva_PK PRIMARY KEY(Reserva_Id),
    CONSTRAINT Reserva_Numero_UQ UNIQUE(Reserva_Numero),
    CONSTRAINT Reserva_Status_CK CHECK (Reserva_Status IN ('PENDENTE', 'CONFIRMADA', 'CANCELADA')),
    CONSTRAINT Reserva_Usuario_FK FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Usuario_Id),
    CONSTRAINT Reserva_PacoteViagem_FK FOREIGN KEY (PacoteViagem_Id) REFERENCES PacoteViagem(PacoteViagem_Id)
);
GO

-- Tabela: Viajante (Associado a uma Reserva)
CREATE TABLE Viajante (
    Viajante_Id INT NOT NULL IDENTITY(1, 1),
    Reserva_Id INT NOT NULL,
    Viajante_Nome VARCHAR(100) NOT NULL,
    Viajante_Documento VARCHAR(50) NOT NULL,
    CONSTRAINT Viajante_PK PRIMARY KEY(Viajante_Id),
    CONSTRAINT Viajante_Reserva_FK FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Reserva_Id) ON DELETE CASCADE
);
GO

-- Tabela: Pagamento
CREATE TABLE Pagamento (
    Pagamento_Id INT NOT NULL IDENTITY(1, 1),
    Reserva_Id INT NOT NULL,
    Pagamento_Forma VARCHAR(20),
    Pagamento_Status VARCHAR(20),
    Pagamento_ComprovanteURL VARCHAR(255),
    Pagamento_Data DATE,
    CONSTRAINT Pagamento_PK PRIMARY KEY(Pagamento_Id),
    CONSTRAINT Pagamento_Reserva_FK FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Reserva_Id),
    CONSTRAINT Pagamento_Forma_CK CHECK (Pagamento_Forma IN ('CARTAO_CREDITO', 'CARTAO_DEBITO', 'BOLETO', 'PIX')),
    CONSTRAINT Pagamento_Status_CK CHECK (Pagamento_Status IN ('PENDENTE', 'APROVADO', 'RECUSADO'))
);
GO

-- Tabela: Avaliacao
CREATE TABLE Avaliacao (
    Avaliacao_Id INT NOT NULL IDENTITY(1, 1),
    Usuario_Id INT NOT NULL,
    PacoteViagem_Id INT NOT NULL,
    Avaliacao_Nota INT,
    Avaliacao_Comentario VARCHAR(500),
    Avaliacao_Data DATE,
    Avaliacao_Aprovada BIT,
    CONSTRAINT Avaliacao_PK PRIMARY KEY(Avaliacao_Id),
    CONSTRAINT Avaliacao_Usuario_FK FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Usuario_Id),
    CONSTRAINT Avaliacao_PacoteViagem_FK FOREIGN KEY (PacoteViagem_Id) REFERENCES PacoteViagem(PacoteViagem_Id),
    CONSTRAINT Avaliacao_Nota_CK CHECK (Avaliacao_Nota BETWEEN 1 AND 5)
);
GO
