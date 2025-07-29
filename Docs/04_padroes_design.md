# Padrões de Design Adotados no Backend

A organização e a manutenibilidade do código do Decola.net são reforçadas pela aplicação de padrões de design consagrados. Eles nos ajudam a resolver problemas comuns de forma eficiente e a construir um código mais flexível e robusto.

### 1. Injeção de Dependência (Dependency Injection - DI)

* **O que é:** É um padrão que permite que as classes recebam as dependências de que precisam (outras classes, serviços, etc.) de uma fonte externa, em vez de criá-las internamente. No ASP.NET Core, isso é feito através do contêiner de DI configurado em `Program.cs`.
* **Como é Usado no Decola.net:**
    * As interfaces de serviços (`IServices`) e repositórios (`IRepository`) são registradas no contêiner de DI.
    * Controladores e serviços recebem suas dependências (como instâncias de `IServices` ou `IRepository`) via construtor.
* **Por que Usamos:**
    * **Acoplamento Reduzido:** As classes se tornam menos dependentes umas das outras, pois não precisam saber como suas dependências são criadas. Isso facilita a modificação e a extensão do código.
    * **Testabilidade Aprimorada:** Permite "mockar" (simular) as dependências em testes unitários. Por exemplo, podemos testar um `PacoteService` sem precisar de um banco de dados real, apenas simulando o `IPacoteRepository`.
    * **Flexibilidade:** Facilita a troca de implementações. Se decidirmos mudar a forma como os dados são armazenados, apenas a implementação do repositório precisa ser alterada, sem impactar os serviços que o utilizam.

### 2. Padrão Repositório (Repository Pattern)

* **O que é:** Um padrão que abstrai a lógica de acesso a dados. Em vez de o código de negócio interagir diretamente com o Entity Framework Core (ou qualquer outra tecnologia de persistência), ele interage com uma interface de repositório.
* **Como é Usado no Decola.net:**
    * Para cada entidade principal (ex: `PacoteViagem`, `Usuario`, `Reserva`), existe uma interface de repositório (ex: `IPacoteViagemRepository` em `Interfaces/`) que define as operações de CRUD (Criar, Ler, Atualizar, Deletar).
    * Uma implementação concreta dessa interface (ex: `PacoteViagemRepository` na pasta `Repository/`) utiliza o `DbContext` do Entity Framework Core para realizar as operações reais no banco de dados.
* **Por que Usamos:**
    * **Separação de Preocupações:** O código de negócio não precisa saber como os dados são salvos ou de onde vêm. Ele apenas "pede" os dados ao repositório.
    * **Manutenibilidade:** Se houver uma mudança na tecnologia de persistência (ex: mudar de SQL Server para um NoSQL), a maior parte da alteração ficaria encapsulada nas implementações dos repositórios, sem afetar os serviços ou controladores.
    * **Testabilidade:** Permite que as camadas superiores (serviços) sejam testadas independentemente da base de dados real, usando implementações de repositório simuladas.

### 3. Data Transfer Objects (DTOs)

* **O que é:** DTOs são objetos simples que carregam dados entre diferentes processos ou camadas de uma aplicação. Eles são usados para moldar os dados de entrada e saída, especialmente em APIs.
* **Como é Usado no Decola.net:**
    * Na pasta `DTOs/`, existem classes que representam os dados que a API recebe (ex: `CriarPacoteRequestDTO`) e os dados que a API retorna (ex: `PacoteResponseDTO`).
    * Esses DTOs são usados nos métodos dos controladores e na comunicação entre os serviços e a API.
* **Por que Usamos:**
    * **Segurança:** Permite expor apenas os dados necessários na API, protegendo campos sensíveis da entidade do domínio.
    * **Flexibilidade da API:** A interface da API pode evoluir independentemente da estrutura interna das entidades do domínio.
    * **Validação:** Facilita a validação dos dados de entrada antes que eles atinjam a lógica de negócio principal.
    * **Redução de Carga de Dados:** Permite enviar apenas os dados relevantes para uma operação específica, evitando o envio de toda a entidade se apenas alguns campos forem necessários.

### 4. Middleware (ASP.NET Core)

* **O que é:** São componentes de software que são configurados em uma pipeline de requisição para lidar com requisições e respostas. Cada middleware pode inspecionar ou modificar a requisição/resposta, ou passar para o próximo middleware.
* **Como é Usado no Decola.net:**
    * Usado para lidar com **autenticação** (JWT Bearer), **autorização**, **tratamento de erros globais**, logging, e para servir a documentação do **Swagger UI**.
    * Configurado no arquivo `Program.cs`.
* **Por que Usamos:**
    * **Centralização de Lógica:** Permite centralizar funcionalidades transversais (cross-cutting concerns) como segurança e tratamento de erros em um único local, evitando duplicação de código nos controladores.
    * **Modularidade:** Facilita a adição ou remoção de funcionalidades na pipeline de requisição sem alterar a lógica principal.