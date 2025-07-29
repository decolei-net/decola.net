# Endpoints da API e Documentação Interativa (Swagger/OpenAPI)

O backend do Decola.net expõe uma API RESTful para interação com o frontend e outras aplicações. A documentação completa e interativa de todos os endpoints é gerada automaticamente via **Swagger/OpenAPI**.

### Como Acessar a Documentação Interativa da API

1.  **Garanta que o Backend esteja Rodando:**
    * Siga as instruções de instalação e execução (detalhadas na seção [Instruções de Instalação e Execução do Backend](08_instalacao_execucao.md)) para iniciar o projeto `Decolei.net` localmente.

2.  **Abra o Swagger UI:**
    * Após o backend ser iniciado, abra seu navegador e acesse a seguinte URL:
        `https://localhost:[PORTA_DA_API]/swagger`
    * **Observação:** A `[PORTA_DA_API]` é geralmente exibida no console quando você inicia a aplicação (ex: `5001`, `7000`). Para fácil acesso no ambiente de desenvolvimento, o `Program.cs` está configurado para que o Swagger UI seja a página inicial (`c.RoutePrefix = string.Empty;`), então você pode acessar diretamente `https://localhost:[PORTA_DA_API]/`.

### Conteúdo da Documentação do Swagger

No Swagger UI, você encontrará:

* **Lista de Endpoints:** Todos os caminhos (URIs) e métodos HTTP (GET, POST, PUT, DELETE) disponíveis, organizados por controladores.
* **Parâmetros:** Detalhes dos parâmetros de requisição (path, query, header, body), incluindo tipos de dados e se são obrigatórios.
* **Modelos de Requisição e Resposta:** Estruturas JSON esperadas para os dados enviados e recebidos por cada endpoint.
* **Códigos de Status HTTP:** Descrição dos possíveis códigos de status (200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 404 Not Found, etc.) para cada resposta de endpoint.
* **Funcionalidade "Try it out":** Capacidade de fazer requisições diretamente pela interface do Swagger para testar os endpoints e ver as respostas em tempo real.
* **Autenticação:** Informações sobre como autenticar requisições usando **JWT Bearer Token** para endpoints protegidos (você pode inserir o token obtido no login diretamente na interface do Swagger para testar endpoints autenticados).