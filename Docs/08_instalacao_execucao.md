# Instruções de Instalação e Execução do Backend

Esta seção detalha os passos necessários para configurar o ambiente de desenvolvimento e executar o backend do Decola.net localmente.

### Pré-requisitos

Certifique-se de que as seguintes ferramentas estejam instaladas em sua máquina:

* **SDK do .NET:** Versão `9.0` (ou superior, conforme o `TargetFramework` do seu `.csproj`). Você pode baixar em [dot.net/download](https://dotnet.microsoft.com/download).
* **SQL Server:** Uma instância local ou acessível do SQL Server (ex: SQL Server Express, SQL Server Developer Edition).
* **Visual Studio 2022** ou **Visual Studio Code:** Com as extensões de C# e ASP.NET Core instaladas.
* **Git:** Para clonar os repositórios.

### Passos para Configuração e Execução

1.  **Clonar os Repositórios:**
    * Abra seu terminal (CMD, PowerShell, Git Bash) ou utilize o Visual Studio/VS Code.
    * Crie um diretório pai para seus repositórios (ex: `C:\ProjetosDecola`).
    * Navegue até este diretório e clone os dois repositórios:
        ```bash
        mkdir C:\ProjetosDecola
        cd C:\ProjetosDecola

        git clone [https://github.com/decolei-net/decola.net.git](https://github.com/decolei-net/decola.net.git)
        git clone [https://github.com/decolei-net/decolei-net-Testes.git](https://github.com/decolei-net/decolei-net-Testes.git)
        ```
    * Agora você terá duas pastas na raiz do seu diretório pai: `decola.net` (para o backend) e `decolei-net-Testes` (para os testes).

2.  **Configurar a String de Conexão do Banco de Dados:**
    * Abra a pasta `decola.net/src/Decolei.net/` no seu editor de código (ou a solução `decola.net/Decolei.net.sln` no Visual Studio).
    * Localize o arquivo `appsettings.Development.json`.
    * Atualize a `ConnectionStrings` para apontar para sua instância do SQL Server. **Certifique-se de que o nome do banco de dados seja `DecoleiNetDB` ou o que você desejar, mas mantenha a consistência.**
        ```json
        {
          "ConnectionStrings": {
            "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DecoleiNetDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
            // Exemplo para SQL Server Express. Ajuste 'Server' e credenciais conforme sua instalação.
            // Para SQL Server no Docker ou outro host, o 'Server' seria o IP/nome do host e você precisaria adicionar User Id/Password.
          },
          "Frontend": {
            "ResetPasswordUrl": "http://localhost:3000/reset-password" // Exemplo: URL do frontend para reset de senha
          },
          "JwtSettings": {
            "Key": "SuaChaveSecretaMuitoLongaEComplexaParaJWT", // ATUALIZE ISSO EM PRODUÇÃO
            "Issuer": "Decolei.net",
            "Audience": "Clientes"
          }
        }
        ```
        * **Observação:** O `JwtSettings:Key` deve ser uma string longa e complexa (mínimo de 16 caracteres, idealmente 32+).

3.  **Aplicar Migrações e Realizar o Seeding do Banco de Dados:**
    * Abra o **Console do Gerenciador de Pacotes** no Visual Studio (`Ferramentas > Gerenciador de Pacotes NuGet > Console do Gerenciador de Pacotes`).
    * **Importante:** No console, certifique-se de que o "Projeto Padrão" (Default project) esteja definido como `Decolei.net`.
    * Execute o comando para aplicar as migrações:
        ```powershell
        Update-Database
        ```
    * Alternativamente, se estiver usando o terminal (na pasta `decola.net/src/Decolei.net/`), execute:
        ```bash
        dotnet ef database update
        ```
    * Este processo fará:
        1.  Criar o banco de dados `DecoleiNetDB` (se não existir).
        2.  Aplicar todas as migrações pendentes, criando o esquema das tabelas.
        3.  Executar o **`SeedData.SeedAllAsync`** (configurado no `Program.cs`), que populará o banco com usuários iniciais (ADMIN, ATENDENTE, CLIENTE) e dados de exemplo (pacotes, etc.).

4.  **Executar o Backend:**
    * **Via Visual Studio:**
        * Abra a solução `decola.net/Decolei.net.sln`.
        * Certifique-se de que o projeto `Decolei.net` esteja definido como projeto de inicialização (clique com o botão direito no projeto `Decolei.net` no Solution Explorer > `Definir como Projeto de Inicialização`).
        * Pressione `F5` ou clique no botão `IIS Express` (ou o nome do projeto `Decolei.net` na barra de ferramentas) para iniciar a aplicação.
    * **Via Terminal (na pasta `decola.net/src/Decolei.net/`):**
        ```bash
        dotnet run
        ```
    * O backend será iniciado, e o console exibirá as URLs (geralmente `http://localhost:XXXX` e `https://localhost:YYYY`).

5.  **Acessar o Swagger UI:**
    * Após a inicialização do backend, abra seu navegador.
    * Como o `Program.cs` está configurado para que o Swagger UI seja a página inicial no ambiente de desenvolvimento (`c.RoutePrefix = string.Empty`), você pode acessar diretamente a URL HTTPS que aparece no console (ex: `https://localhost:7000/`).
    * A interface do Swagger permitirá que você explore e teste todos os endpoints da API.

