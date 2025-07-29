# Tecnologias Utilizadas no Backend

O backend do Decola.net é construído sobre uma pilha de tecnologias modernas e robustas, escolhidas para garantir performance, escalabilidade, segurança e facilidade de manutenção.

* **Linguagem de Programação:**
    * **C#**
        * **O que é:** Linguagem de programação orientada a objetos desenvolvida pela Microsoft, amplamente utilizada no desenvolvimento de aplicações corporativas e web com o ecossistema .NET.
        * **Por que foi escolhida:** Oferece forte tipagem, um ambiente de desenvolvimento produtivo (Visual Studio), excelente desempenho e uma vasta comunidade de suporte, tornando-a ideal para construir sistemas robustos e escaláveis.

* **Framework Web:**
    * **ASP.NET Core**
        * **O que é:** Um framework de código aberto e multiplataforma para construir aplicações web modernas, incluindo APIs RESTful, com foco em alta performance.
        * **Por que foi escolhido:** Proporciona performance superior, é multiplataforma (pode rodar em Windows, Linux, macOS), oferece um modelo de programação maduro e é altamente extensível, o que facilita a criação de uma API eficiente para a agência.

* **Gerenciamento de Pacotes:**
    * **NuGet**
        * **O que é:** O gerenciador de pacotes oficial para o ecossistema .NET, que simplifica a inclusão, atualização e restauração de bibliotecas e ferramentas de terceiros em projetos .NET.
        * **Por que foi escolhido:** É o padrão da indústria para .NET, garantindo fácil gerenciamento de dependências, acesso a uma vasta gama de bibliotecas e compatibilidade com o ambiente de desenvolvimento.

* **Banco de Dados:**
    * **SQL Server**
        * **O que é:** Um sistema de gerenciamento de banco de dados relacional (SGBDR) da Microsoft, conhecido por sua robustez, segurança e capacidade de lidar com grandes volumes de dados.
        * **Por que foi escolhido:** Oferece alta confiabilidade, ferramentas administrativas poderosas e excelente integração com o ecossistema .NET, sendo uma escolha segura para a persistência dos dados críticos da agência (pacotes, reservas, usuários).

* **Mapeamento Objeto-Relacional (ORM):**
    * **Entity Framework Core (EF Core)**
        * **O que é:** O framework ORM de código aberto da Microsoft para .NET, que permite que desenvolvedores interajam com bancos de dados relacionais usando objetos C# e LINQ, sem a necessidade de escrever SQL puro na maioria dos casos.
        * **Por que foi escolhido:** Simplifica drasticamente o acesso a dados, reduzindo a quantidade de código para operações de banco de dados, facilita a manutenção e permite a evolução do schema do banco através de migrações.

* **Testes:**
    * **xUnit**
        * **O que é:** Um framework de testes unitários moderno e popular para .NET, focado em simplicidade e extensibilidade.
        * **Por que foi escolhido:** Oferece uma estrutura limpa e flexível para escrever testes, incentivando boas práticas e facilitando a automação da validação do código.
    * **Microsoft.AspNetCore.Mvc.Testing**
        * **O que é:** Uma biblioteca para testar aplicações ASP.NET Core que simula um servidor HTTP, permitindo que você execute testes de integração que interagem com sua API.
        * **Por que foi escolhida:** Facilita a escrita de testes de integração realistas para a API, testando o fluxo completo de requisição/resposta sem a necessidade de um servidor real rodando.
    * **Microsoft.EntityFrameworkCore.InMemory**
        * **O que é:** Um provedor de banco de dados em memória para Entity Framework Core, ideal para testes.
        * **Por que foi escolhida:** Permite que os testes de integração que dependem do banco de dados sejam executados de forma rápida e isolada, sem afetar ou depender de uma instância de banco de dados real.

* **Documentação da API:**
    * **Swagger/Swashbuckle**
        * **O que é:** Swagger (agora OpenAPI Specification) é um padrão para descrever APIs RESTful. Swashbuckle é uma biblioteca para ASP.NET Core que gera automaticamente essa documentação (Swagger UI) a partir do código.
        * **Por que foi escolhida:** Automatiza a criação de uma documentação de API interativa e atualizada, facilitando a integração do frontend e o teste dos endpoints, além de melhorar a comunicação entre equipes.