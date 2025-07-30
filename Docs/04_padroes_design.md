# Padrões de Design Adotados no Backend

A organização e a manutenibilidade do código do Decola.net são reforçadas pela aplicação de padrões de design consagrados. Eles nos ajudam a resolver problemas comuns de forma eficiente e a construir um código mais flexível e robusto.

### 1. Injeção de Dependência (Dependency Injection - DI)

* **O que é:** É um padrão que permite que as classes recebam as dependências de que precisam (outras classes, serviços, etc.) de uma fonte externa (o "contêiner de DI"), em vez de criá-las internamente. Isso promove um código mais modular e testável.
* **Como é Usado no Decola.net:**
    * Interfaces de repositórios (ex: `IPacoteRepository`, `IReservaRepository`) são registradas no contêiner de DI.
    * Alguns serviços são registrados por suas classes concretas (ex: `EmailService`, `PagamentoService`).
    * Controladores e outros serviços recebem suas dependências (sejam interfaces ou classes concretas) via construtor, como visto em `Program.cs`.
* **Por que Usamos:**
    * **Acoplamento Reduzido:** As classes se tornam menos dependentes umas das outras, facilitando a modificação e a extensão do código sem impactar muitos locais.
    * **Testabilidade Aprimorada:** Permite "mockar" (simular) as dependências em testes unitários. Por exemplo, podemos testar um `PacoteService` simulando um `IPacoteRepository` sem precisar de um banco de dados real.
    * **Flexibilidade:** Facilita a troca de implementações. Se decidirmos mudar a forma como os dados são armazenados, apenas a implementação do repositório precisa ser alterada, sem impactar os serviços que o utilizam.

### 2. Padrão Repositório (Repository Pattern)

* **O que é:** Um padrão que abstrai a lógica de acesso a dados. Em vez de o código de negócio interagir diretamente com o Entity Framework Core, ele interage com uma interface de repositório, que atua como uma coleção de objetos do domínio.
* **Como é Usado no Decola.net:**
    * Para algumas entidades principais (especificamente `PacoteViagem` e `Reserva`), existem interfaces de repositório (ex: `IPacoteRepository`, `IReservaRepository` na pasta `Interfaces/`) que definem as operações de consulta e manipulação de dados (CRUD).
    * As implementações concretas dessas interfaces (ex: `PacoteRepository.cs`, `ReservaRepository.cs` na pasta `Repository/`) utilizam o `DbContext` do Entity Framework Core para realizar as operações reais no banco de dados.
    * **Observação:** Outras entidades, como `Usuario` (gerenciada pelo ASP.NET Core Identity `UserManager`), `Pagamento` e `Avaliacao`, são acessadas diretamente via `DbContext` em seus respectivos controladores ou serviços, sem um repositório dedicado para elas, uma abordagem comum para simplificar o acesso a entidades mais simples ou quando a abstração do repositório não é necessária para cada uma.
* **Por que Usamos:**
    * **Separação de Preocupações:** O código de negócio não precisa saber detalhes técnicos de como os dados são salvos ou de onde vêm. Ele apenas "pede" os dados ao repositório ou interage com o `DbContext` diretamente quando a complexidade não justifica um repositório.
    * **Manutenibilidade:** Se houver uma mudança na tecnologia de persistência, a maior parte da alteração ficaria encapsulada nas implementações dos repositórios (para Pacotes e Reservas) ou nas classes que acessam o `DbContext` diretamente.
    * **Testabilidade:** Permite que as camadas superiores (serviços) que usam repositórios sejam testadas independentemente da base de dados real, usando implementações de repositório simuladas.

### 3. Data Transfer Objects (DTOs)

* **O que é:** DTOs são objetos simples que carregam dados entre diferentes processos ou camadas de uma aplicação. Eles são usados para moldar os dados de entrada e saída, especialmente em APIs, controlando quais informações são expostas e recebidas.
* **Como é Usado no Decola.net:**
    * Na pasta `DTOs/`, existem classes que representam os dados que a API recebe (ex: `CriarPacoteRequestDTO`, `RegistroUsuarioDto`) e os dados que a API retorna (ex: `PacoteResponseDTO`, `ReservaDetalhesDto`).
    * Esses DTOs são usados nos métodos dos controladores para validar entradas e formatar saídas, bem como na comunicação entre os serviços e a API.
* **Por que Usamos:**
    * **Segurança:** Permite expor apenas os dados necessários na API, protegendo campos sensíveis das entidades de domínio internas.
    * **Flexibilidade da API:** A interface da API pode evoluir independentemente da estrutura interna das entidades do domínio, oferecendo uma camada de proteção contra mudanças.
    * **Validação:** Facilita a validação dos dados de entrada através de atributos de validação, antes que eles atinjam a lógica de negócio principal.
    * **Redução de Carga de Dados:** Permite enviar apenas os dados relevantes para uma operação específica, evitando o envio de toda a entidade se apenas alguns campos forem necessários.

### 4. Middleware (ASP.NET Core)

* **O que é:** São componentes de software que são configurados em uma pipeline de requisição HTTP para lidar com requisições e respostas. Cada middleware pode inspecionar, modificar ou passar a requisição para o próximo componente.
* **Como é Usado no Decola.net:**
    * Usado para lidar com funcionalidades transversais (cross-cutting concerns) como **autenticação** (JWT Bearer), **autorização**, **tratamento de erros globais**, logging, e para servir a documentação interativa do **Swagger UI**.
    * A configuração de cada middleware é realizada no arquivo `Program.cs`.
* **Por que Usamos:**
    * **Centralização de Lógica:** Permite centralizar lógicas comuns a todas (ou a muitas) requisições em um único local, evitando duplicação de código nos controladores.
    * **Modularidade:** Facilita a adição ou remoção de funcionalidades na pipeline de requisição sem alterar a lógica principal da aplicação.
