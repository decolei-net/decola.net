# Desenho da Arquitetura do Backend: Arquitetura em Camadas Monolítica

O backend do Decola.net foi desenvolvido em **.NET** e segue uma **Arquitetura em Camadas Monolítica**. Essa abordagem organiza o código dentro de um **único projeto**, dividindo-o em responsabilidades lógicas (camadas) que promovem a organização e a separação de interesses. É uma estrutura eficiente para gerenciar a lógica de negócio, acesso a dados e interface de API de forma coesa.

### Estrutura de Repositórios e Organização de Pastas

Seu projeto `Decola.net` é organizado em **dois repositórios principais** para separar o código da aplicação principal dos testes, promovendo uma organização clara e facilitando o gerenciamento do código-fonte:

1.  **Repositório Principal (`decola.net`)**: Contém o código-fonte do backend da aplicação.
    ```
    decola.net/                 # Raiz do repositório principal
    ├── .git/                   # Metadados do Git
    ├── src/                    # Pasta que contém o(s) projeto(s) fonte
    │   └── Decolei.net/        # **O ÚNICO PROJETO DO BACKEND (ASP.NET Core Web API)**
    │       ├── Connected Services/ # Serviços externos conectados
    │       ├── Dependencies/       # Dependências do projeto (gerenciado por NuGet)
    │       ├── Properties/         # Propriedades do projeto
    │       ├── Controllers/        # Camada de Apresentação/API (recebe requisições HTTP)
    │       ├── Data/               # Camada de Acesso a Dados (DbContext, configurações de BD)
    │       ├── DTOs/               # Objetos de Transferência de Dados
    │       ├── Enums/              # Definições de Enums (valores pré-definidos)
    │       ├── Interfaces/         # Interfaces de serviços e repositórios
    │       ├── Migrations/         # **Migrações do Entity Framework Core** (evolução do esquema do BD)
    │       ├── Models/             # Modelos de Domínio/Entidades (Lógica de Negócio principal)
    │       ├── Repository/         # Implementações de Repositório (lógica de acesso a dados)
    │       ├── Scripts/            # Scripts diversos (se houver)
    │       ├── Services/           # Serviços da Aplicação/Lógica de Negócio
    │       ├── .gitignore          # Arquivo para controle de versão
    │       ├── appsettings.json    # Arquivos de configuração da aplicação
    │       ├── Decolei.net.http    # Arquivos HTTP (para testes rápidos de API)
    │       ├── libman.json         # Gerenciamento de bibliotecas do lado do cliente
    │       ├── Program.cs          # Ponto de entrada e configuração da aplicação (DI, pipeline)
    │       └── README.md           # README específico do projeto Decolei.net (pode conter detalhes internos)
    └── docs/                   # Pasta para documentação geral do projeto (onde este doc reside)
        └── backend/            # Documentação específica do backend
            └── README.md       # O ponto de entrada da documentação do backend

    ```

2.  **Repositório de Testes (`decolei-net-Testes`)**: Contém os projetos de testes automatizados que validam o comportamento do backend.
    ```
    decolei-net-Testes/         # Raiz do repositório de testes
    ├── .git/                   # Metadados do Git
    ├── Decolei.net.Tests.sln   # Solução do Visual Studio para os projetos de teste
    ├── Decolei.net.Tests/      # Projeto de testes (xUnit, com referências ao projeto principal)
    │   ├── ... (pastas de testes: Unit, Integration)
    │   └── Decolei.net.Tests.csproj
    └── README.md               # README do repositório de testes
    ```

### Explicação Detalhada das Pastas Lógicas Dentro do Projeto `Decolei.net` (Monolítico):

Dentro do projeto `Decolei.net` (localizado em `decola.net/src/Decolei.net/`), as responsabilidades são divididas em pastas lógicas para cada camada da arquitetura monolítica:

1.  **Apresentação / API (`Controllers`, `DTOs`)**
    * **Propósito:** Esta camada lida com a **exposição da API RESTful**. É o ponto de entrada da aplicação, responsável por receber as requisições HTTP do frontend e coordenar a resposta.
    * **Pastas Internas:**
        * `Controllers/`: Contém as classes que herdam de `ControllerBase` e definem os endpoints da API (ex: `UsuarioController`, `PacotesController`, `ReservaController`, `PagamentosController`, `AvaliacaoController`).
        * `DTOs/`: Contém os **Data Transfer Objects (DTOs)**, que são classes simples usadas para moldar os dados de entrada (corpo das requisições) e saída (respostas da API), garantindo que apenas os dados necessários sejam transferidos.

2.  **Serviços da Aplicação (`Services`)**
    * **Propósito:** Esta camada contém a **lógica de aplicação e os casos de uso**. Ela atua como uma orquestradora, validando dados, chamando a lógica de negócio principal e interagindo com a camada de acesso a dados.
    * **Pastas Internas:**
        * `Services/`: Contém as classes que implementam a lógica de negócio específica da aplicação (ex: `EmailService`, `PagamentoService`). Elas utilizam as interfaces de repositório para interagir com o banco de dados.
        * `Interfaces/`: Contém as definições de interfaces para serviços e repositórios (ex: `IPacoteRepository`, `IReservaRepository`), o que promove o baixo acoplamento e facilita a Injeção de Dependência.

3.  **Lógica de Negócio / Domínio (`Models`, `Enums`)**
    * **Propósito:** O **coração do sistema**, onde residem as **entidades principais e as regras de negócio** mais fundamentais da agência de viagens. Esta parte do código deve ser o mais independente possível de detalhes tecnológicos.
    * **Pastas Internas:**
        * `Models/`: Contém as classes que representam as entidades do seu domínio (ex: `Usuario`, `PacoteViagem`, `Reserva`, `Pagamento`, `Avaliacao`). Essas classes também podem conter validações e comportamentos intrínsecos ao negócio.
        * `Enums/`: Contém as definições de enumeradores utilizados para tipar dados no domínio (ex: `MetodoPagamento`, `ReservaStatus`).

4.  **Acesso a Dados (`Repository`, `Data`, `Migrations`)**
    * **Propósito:** Esta camada é responsável por **abstrair e gerenciar a interação com o banco de dados**. Ela traduz as operações de objetos C# para operações de persistência e vice-versa.
    * **Pastas Internas:**
        * `Repository/`: Contém as implementações concretas dos padrões de repositório (ex: `PacoteRepository`, `UsuarioRepository`) que utilizam o Entity Framework Core para persistir e recuperar dados no banco de dados.
        * `Data/`: Contém a classe `DbContext` do Entity Framework Core (`DecoleiDbContext`) que é responsável pelo mapeamento objeto-relacional (ORM) entre suas entidades C# e as tabelas do SQL Server, além das configurações de conexão para que o EF Core se conecte ao banco (configurado em `Program.cs` e `DecoleiDbContextFactory`).
        * `Migrations/`: Pasta gerada pelo Entity Framework Core que armazena os scripts de migração do banco de dados. Estas migrações permitem a evolução controlada do esquema do banco de dados (adição de tabelas, colunas, etc.) sem perder dados.

5.  **Outras Pastas e Arquivos Chave (na raiz do projeto `Decolei.net`):**
    * `Connected Services/`: Pasta para serviços conectados e referências externas.
    * `Dependencies/`: Gerenciado pelo NuGet, contém as referências de pacotes e projetos.
    * `Properties/`: Contém configurações e metadados do projeto, como perfis de inicialização.
    * `Scripts/`: Para scripts diversos que não se encaixam em outras categorias, como scripts SQL manuais.
    * `Testes/`: Se os testes unitários e de integração estivessem dentro do projeto principal, esta pasta os conteria. No seu caso, eles estão em um **repositório separado (`decolei-net-Testes`)**, o que será detalhado na seção de Testes.
    * `.gitignore`: Arquivo essencial para o controle de versão com Git, definindo quais arquivos e pastas devem ser ignorados.
    * `appsettings.json`, `appsettings.Development.json`, etc.: Arquivos de configuração da aplicação para diferentes ambientes (conexões com banco de dados, chaves de API, etc.).
    * `Decolei.net.http`: Arquivos para testar endpoints da API diretamente do VS Code.
    * `libman.json`: Usado pelo Library Manager para gerenciar bibliotecas do lado do cliente.
    * `Program.cs`: O ponto de entrada da aplicação ASP.NET Core, onde a aplicação é configurada, os serviços são registrados para Injeção de Dependência e o pipeline de requisição HTTP é construído. É aqui que o **Swagger/Swashbuckle** é configurado.
    * `DecoleiDbContextFactory.cs`: Classe essencial para o design-time das migrações do Entity Framework Core, permitindo que a ferramenta de migração encontre o contexto do banco de dados.

### Argumentos em Defesa da Escolha da Arquitetura em Camadas Monolítica

A escolha de uma arquitetura em camadas monolítica para o Decola.net foi estratégica por diversos motivos, especialmente para o estágio atual do projeto:

* **Simplicidade e Agilidade no Desenvolvimento:**
    * **Defesa:** Manter todas as camadas dentro de um único projeto facilita o desenvolvimento inicial e a depuração. É mais rápido configurar, codificar e implantar um monólito, o que é ideal para um MVP (Produto Mínimo Viável) e equipes menores.

* **Comunicação Direta e Performance:**
    * **Defesa:** As camadas se comunicam diretamente por chamadas de método, sem a sobrecarga de comunicação de rede. Isso pode resultar em um desempenho mais otimizado para operações internas.

* **Menor Complexidade Operacional:**
    * **Defesa:** A implantação e o gerenciamento de um único artefato (o aplicativo monolítico) são mais simples do que gerenciar múltiplos serviços distribuídos. Isso reduz a complexidade de DevOps no início.

* **Reuso de Código Facilitado:**
    * **Defesa:** Como todas as classes estão no mesmo projeto, o reuso de código e a refatoração entre as camadas são diretos.

Embora uma arquitetura de microsserviços ou Clean Architecture com múltiplos projetos traga vantagens para sistemas muito grandes e complexos, a arquitetura em camadas monolítica oferece um excelente equilíbrio entre **rapidez de desenvolvimento, facilidade de manutenção e desempenho** para o contexto do Decola.net. Ela nos permite entregar valor rapidamente, com a possibilidade de refatorar para uma arquitetura distribuída no futuro, se as necessidades do negócio exigirem.