# Guia Rápido de Consumo da API Backend (Decola.net)

Este guia oferece exemplos práticos e diretos para interagir com os principais endpoints da API do Decola.net. Para detalhes completos sobre parâmetros, modelos e respostas, consulte a **Documentação Interativa do Swagger** (`https://localhost:[PORTA_DA_API]/swagger`).

**Pré-requisito:** Certifique-se de que o backend esteja rodando localmente (a porta geralmente aparece no console ao iniciar `Decolei.net`).

### Fluxo de Permissões e Autenticação:

O sistema possui 3 perfis: **`ADMIN`**, **`ATENDENTE`** e **`CLIENTE`**. As permissões são controladas pela sua `role` após o login, usando atributos `[Authorize(Roles = "NomeDaRole")]` nos controladores.

---

### 1. Autenticação e Gerenciamento de Usuários (Controller: `UsuarioController`)

#### 1.1. Login de Usuário
* **Endpoint:** `POST /api/Usuario/login`
* **O que faz:** Autentica um usuário (Admin, Atendente ou Cliente) com e-mail e senha, e retorna um **JWT Bearer Token** para acesso futuro. O sistema reconhece a role do usuário automaticamente.
* **Exemplo de Corpo (JSON Body):**
    ```json
    {
      "email": "admin@decola.net",
      "senha": "SenhaSegura123"
    }
    ```
    * **Observação:** Use `email` e `senha` (em minúsculas, conforme seu DTO implícito) para os campos.
* **Importante:** O `token` retornado deve ser usado no cabeçalho `Authorization: Bearer [SEU_TOKEN]` para acessar endpoints protegidos.

#### 1.2. Registrar Novo Usuário (Para `CLIENTE` - Sem Autenticação Prévia)
* **Endpoint:** `POST /api/Usuario/registrar`
* **O que faz:** Permite que qualquer pessoa crie uma nova conta de `CLIENTE`.
* **Exemplo de Corpo (JSON Body):**
    ```json
    {
      "nome": "Novo Cliente Exemplo",
      "email": "novo.cliente@decola.net",
      "senha": "SenhaCliente!23",
      "telefone": "5511987654321",
      "documento": "123.456.789-00" // CPF/Passaporte
    }
    ```

#### 1.3. Registrar Novo Administrador (Apenas para `ADMIN` Logado)
* **Endpoint:** `POST /api/Usuario/registrar-admin`
* **O que faz:** Um usuário com a role **`ADMIN`** pode criar novas contas de `ADMIN` para outros usuários.
* **Exemplo de Corpo (JSON Body):**
    ```json
    {
      "nome": "Novo Admin Exemplo",
      "email": "novo.admin@decola.net",
      "senha": "SenhaAdmin!23",
      "telefone": "551199887766",
      "documento": "987.654.321-00" // CPF/Passaporte
    }
    ```
* **Como usar:** Requer o token de um `ADMIN` válido no cabeçalho `Authorization`.

#### 1.4. Listar Todos os Usuários (Apenas para `ADMIN` Logado)
* **Endpoint:** `GET /api/Usuario`
* **O que faz:** Retorna uma lista completa de todos os usuários registrados no sistema.
* **Como usar:** Requer o token de um usuário com a role `ADMIN` no cabeçalho `Authorization`.

#### 1.5. Obter Usuário por ID (Permissões Variadas)
* **Endpoint:** `GET /api/Usuario/{id}` (o `{id}` deve ser um número inteiro, `int`)
* **O que faz:** Retorna os detalhes de um usuário específico pelo seu ID.
* **Permissões:**
    * `ADMIN` e `ATENDENTE` podem consultar qualquer usuário.
    * **Observação:** A implementação atual no controlador não permite que um `CLIENTE` obtenha seus próprios dados por ID. Essa funcionalidade pode ser adicionada para o perfil `CLIENTE` se necessário.

#### 1.6. Recuperação de Senha
* **Solicitar Redefinição:**
    * **Endpoint:** `POST /api/Usuario/recuperar-senha`
    * **O que faz:** Inicia o processo de recuperação de senha. Envia um e-mail (ou mostra no log, se o serviço de e-mail for simulado) com um link/token para redefinição.
    * **Exemplo de Corpo (JSON Body):**
        ```json
        { "email": "seu.email@exemplo.com" }
        ```
* **Redefinir Senha:**
    * **Endpoint:** `POST /api/Usuario/redefinir-senha`
    * **O que faz:** Redefine a senha do usuário utilizando o token recebido e uma nova senha.
    * **Exemplo de Corpo (JSON Body):**
        ```json
        {
          "email": "seu.email@exemplo.com",
          "token": "token_recebido_por_email_ou_log",
          "novaSenha": "MinhaNovaSenhaSegura!"
        }
        ```

#### 1.7. Logout de Usuário (Requer Autenticação)
* **Endpoint:** `POST /api/Usuario/logout`
* **O que faz:** Invalida a sessão do usuário logado.
* **Como usar:** Requer o token JWT do usuário no cabeçalho `Authorization`.

---

### 2. Pacotes de Viagem (Controller: `PacotesController`)

Este controller gerencia o cadastro, busca, visualização, atualização e exclusão de pacotes de viagem.

#### 2.1. Listar e Buscar Pacotes (GET)
* **Endpoint:** `GET /api/Pacotes`
* **O que faz:** Retorna uma lista de todos os pacotes de viagem disponíveis. Suporta filtros opcionais via **Query Parameters**.
* **Query Parameters Aceitos:**
    * `destino` (string): Filtra pacotes por destino (ex: `?destino=Paris`).
    * `precoMin` (decimal): Filtra pacotes com valor mínimo (ex: `?precoMin=1000.00`).
    * `precoMax` (decimal): Filtra pacotes com valor máximo (ex: `?precoMax=5000.00`).
    * `dataInicio` (DateTime): Filtra pacotes disponíveis a partir de uma data (formato `AAAA-MM-DD`).
    * `dataFim` (DateTime): Filtra pacotes disponíveis até uma data (formato `AAAA-MM-DD`).
* **Exemplo de Uso:**
    * `GET /api/Pacotes` (todos os pacotes)
    * `GET /api/Pacotes?destino=Manaus&precoMax=3000.00`
    * `GET /api/Pacotes?dataInicio=2025-08-01`
* **Resposta (Exemplo de um item JSON):**
    ```json
    {
      "id": 1,
      "titulo": "Aventura na Amazônia",
      "descricao": "Pacote ecológico com trilhas e fauna",
      "imagemURL": "[https://example.com/amazon.jpg](https://example.com/amazon.jpg)",
      "videoURL": null,
      "destino": "Manaus, Brasil",
      "valor": 2500.00,
      "dataInicio": "2025-08-10T00:00:00",
      "dataFim": "2025-08-15T00:00:00",
      "usuarioId": 123
    }
    ```

#### 2.2. Obter Pacote por ID (GET)
* **Endpoint:** `GET /api/Pacotes/{id}`
* **O que faz:** Retorna os detalhes de um pacote de viagem específico pelo seu ID.
* **Exemplo de Uso:** `GET /api/Pacotes/1`
* **Resposta (Exemplo JSON):** Retorna o mesmo formato do exemplo de listar pacotes.

#### 2.3. Criar Novo Pacote (POST - Requer `ADMIN` ou `ADMINISTRADOR` Role)
* **Endpoint:** `POST /api/Pacotes`
* **O que faz:** Adiciona um novo pacote de viagem ao sistema. Requer autenticação com perfil de `ADMIN` ou `ADMINISTRADOR`.
* **Exemplo de Corpo (JSON Body - DTO `CriarPacoteViagemDto`):**
    ```json
    {
      "titulo": "Escapada Romântica em Paris",
      "descricao": "Pacote especial para casais com city tour e jantar",
      "imagemURL": "[https://example.com/paris-romance.jpg](https://example.com/paris-romance.jpg)",
      "videoURL": null,
      "destino": "Paris, França",
      "valor": 5500.00,
      "dataInicio": "2025-10-15T00:00:00",
      "dataFim": "2025-10-22T00:00:00"
    }
    ```
* **Como usar:** Requer o token de um `ADMIN` ou `ADMINISTRADOR` válido no cabeçalho `Authorization`.

#### 2.4. Atualizar Pacote Existente (PUT - Requer `ADMIN` ou `ADMINISTRADOR` Role)
* **Endpoint:** `PUT /api/Pacotes/{id}`
* **O que faz:** Atualiza as informações de um pacote de viagem existente. Apenas os campos fornecidos no corpo da requisição serão atualizados. Requer autenticação com perfil de `ADMIN` ou `ADMINISTRADOR`.
* **Exemplo de Corpo (JSON Body - DTO `UpdatePacoteViagemDto`):**
    * *Para atualizar o valor e a data de início de um pacote com ID `1`.*
    ```json
    {
      "valor": 6000.00,
      "dataInicio": "2026-01-01T00:00:00"
    }
    ```
* **Como usar:** Requer o token de um `ADMIN` ou `ADMINISTRADOR` válido no cabeçalho `Authorization`.

#### 2.5. Deletar Pacote (DELETE - Requer `ADMIN` ou `ADMINISTRADOR` Role)
* **Endpoint:** `DELETE /api/Pacotes/{id}`
* **O que faz:** Remove um pacote de viagem do sistema.
    * **Importante:** Se o pacote tiver reservas associadas, o sistema pode retornar um `409 Conflict` ou uma mensagem de erro indicando que não pode ser excluído devido a dependências.
* **Exemplo de Uso:** `DELETE /api/Pacotes/1`
* **Como usar:** Requer o token de um `ADMIN` ou `ADMINISTRADOR` válido no cabeçalho `Authorization`.
* **Resposta de Sucesso:** Status `204 No Content`.
* **Resposta de Erro:** Status `409 Conflict` se houver dependências.

---

**Observação:** Os DTOs (`CriarPacoteViagemDto`, `UpdatePacoteViagemDto`) são classes C# que definem a estrutura dos dados esperados nos corpos das requisições. O Swagger UI fornecerá a estrutura exata de cada um.

---

### 3. Reservas (Controller: `ReservaController`)

Este controller gerencia a criação, visualização e atualização do status das reservas. Todos os endpoints deste controller exigem autenticação (`[Authorize]`).

#### 3.1. Criar Nova Reserva (POST - Requer Autenticação de qualquer perfil)
* **Endpoint:** `POST /api/Reserva`
* **O que faz:** Cria uma nova reserva para um pacote de viagem. O valor total é calculado automaticamente com base no valor do pacote e no número de viajantes. O usuário que cria a reserva é associado a ela.
* **Exemplo de Corpo (JSON Body - DTO `CriarReservaDto`):**
    ```json
    {
      "pacoteViagemId": 1,
      "viajantes": [
        {
          "nome": "Pedro Alves",
          "documento": "111.111.111-11"
        },
        {
          "nome": "Ana Costa",
          "documento": "222.222.222-22"
        }
      ]
    }
    ```
    * **Observação:** O usuário logado que realiza a reserva é considerado o primeiro viajante e não precisa ser incluído no array `viajantes`. O campo `data` não é enviado na criação, sendo definido pelo backend.
* **Como usar:** Requer um token JWT válido no cabeçalho `Authorization`.

#### 3.2. Listar Todas as Reservas (GET - Requer `ATENDENTE` ou `ADMIN` Role)
* **Endpoint:** `GET /api/Reserva`
* **O que faz:** Retorna uma lista completa de todas as reservas no sistema.
* **Como usar:** Requer o token de um usuário com a role `ATENDENTE` ou `ADMIN` no cabeçalho `Authorization`.
* **Resposta (Exemplo de um item JSON - DTO `ReservaDetalhesDto`):**
    ```json
    [
      {
        "id": 1,
        "data": "2025-07-29T15:00:00Z",
        "valorTotal": 5000.00,
        "status": "PENDENTE",
        "numero": "ABCDEF1234",
        "pacoteViagem": {
          "id": 1,
          "titulo": "Aventura na Amazônia",
          "destino": "Manaus, Brasil"
        },
        "usuario": {
          "id": 123,
          "nomeCompleto": "Usuario Cliente",
          "email": "cliente@decola.net"
        },
        "viajantes": [
          {
            "nome": "Pedro Alves",
            "documento": "111.111.111-11"
          },
          {
            "nome": "Ana Costa",
            "documento": "222.222.222-22"
          }
        ]
      }
    ]
    ```

#### 3.3. Listar Minhas Reservas (GET - Requer Autenticação de qualquer perfil)
* **Endpoint:** `GET /api/Reserva/minhas-reservas`
* **O que faz:** Retorna apenas as reservas associadas ao usuário logado que fez a requisição.
* **Como usar:** Requer um token JWT válido (de qualquer perfil) no cabeçalho `Authorization`.
* **Resposta (Exemplo JSON):** Retorna uma lista de `ReservaDetalhesDto`, similar ao `GetAll`, mas filtrada pelo ID do usuário logado.

#### 3.4. Obter Reserva por ID (GET - Permissões Variadas)
* **Endpoint:** `GET /api/Reserva/{id}`
* **O que faz:** Retorna os detalhes de uma reserva específica pelo seu ID.
* **Permissões:**
    * `ADMIN` e `ATENDENTE` podem consultar qualquer reserva.
    * `CLIENTE` pode consultar apenas as reservas que ele próprio criou.
* **Exemplo de Uso:** `GET /api/Reserva/1`
* **Resposta (Exemplo JSON):** Retorna um único `ReservaDetalhesDto`.

#### 3.5. Atualizar Status da Reserva (PUT - Requer `ATENDENTE` ou `ADMIN` Role)
* **Endpoint:** `PUT /api/Reserva/{id}`
* **O que faz:** Permite atualizar o `Status` de uma reserva (ex: de "PENDENTE" para "CONFIRMADA" ou "CANCELADA").
* **Exemplo de Corpo (JSON Body - DTO `UpdateReservaDto`):**
    ```json
    {
      "status": "CONFIRMADA" // Outros valores possíveis: "PENDENTE", "CANCELADA"
    }
    ```
* **Como usar:** Requer o token de um usuário com a role `ATENDENTE` ou `ADMIN` no cabeçalho `Authorization`.
* **Resposta de Sucesso:** Status `204 No Content`.

---

### 4. Pagamentos (Controller: `PagamentosController`)

Este controller gerencia o processo de pagamento e a consulta de status de pagamentos de reservas. As operações de pagamento são **simuladas** internamente.

#### 4.1. Criar Pagamento (POST - Requer `ADMIN` ou `CLIENTE` Role)
* **Endpoint:** `POST /pagamentos`
* **O que faz:** Inicia o processo de pagamento para uma reserva existente. O sistema simula a transação via um gateway de pagamento interno e atualiza o status do pagamento e da reserva. Um e-mail de confirmação é enviado.
* **Status de Retorno do Pagamento (Simulado):**
    * **"APROVADO":** Para pagamentos via Pix, Cartão de Crédito/Débito (se `NumeroCartaoMascarado` for válido).
    * **"PENDENTE":** Para pagamentos via Boleto. (Um processo simulado posterior aprova o boleto após 60 segundos).
    * **"RECUSADO":** Se o método não for reconhecido ou o número do cartão for inválido/vazio para Cartão de Crédito/Débito.
* **Exemplo de Corpo (JSON Body - DTO `PagamentoEntradaDTO`):**
    ```json
    {
      "reservaId": 123,
      "nomeCompleto": "João da Silva",
      "cpf": "111.222.333-44",
      "email": "joao.silva@example.com",
      "metodo": "Credito", // Opções: "Credito", "Debito", "Pix", "Boleto"
      "valor": 2500.00,
      "parcelas": 3,       // Para Cartão de Crédito
      "numeroCartao": "1234567890123456" // Apenas os primeiros 12 dígitos são verificados
    }
    ```
    * **Para Pix/Boleto:** Os campos `parcelas` e `numeroCartao` são opcionais.
    * **Para Cartão de Débito/Crédito:** `numeroCartao` (mascarado ou completo) deve ter pelo menos 12 dígitos válidos (não "string").
* **Como usar:** Requer um token JWT de `ADMIN` ou `CLIENTE` no cabeçalho `Authorization`.
* **Resposta (Exemplo de Sucesso - DTO `PagamentoDto`):**
    ```json
    {
      "id": 1,
      "reserva_Id": 123,
      "forma": "CARTAO_CREDITO",
      "status": "APROVADO",
      "comprovanteURL": "[https://decolei.net/comprovante/ID_DA_TRANSACAO](https://decolei.net/comprovante/ID_DA_TRANSACAO)",
      "data": "2025-07-29T16:00:00Z"
    }
    ```

#### 4.2. Obter Status do Pagamento (GET - Requer `ADMIN` Role)
* **Endpoint:** `GET /pagamentos/status/{id}`
* **O que faz:** Retorna o status de um pagamento específico e o status da reserva a ele relacionada.
* **Exemplo de Uso:** `GET /pagamentos/status/1` (onde `1` é o ID do pagamento)
* **Como usar:** Requer o token de um usuário com a role `ADMIN` no cabeçalho `Authorization`.
* **Resposta (Exemplo JSON):**
    ```json
    {
      "pagamentoId": 1,
      "statusPagamento": "APROVADO",
      "statusReserva": "APROVADO"
    }
    ```

#### 4.3. Atualizar Status do Pagamento (PUT - Requer `ADMIN` Role)
* **Endpoint:** `PUT /pagamentos/{id}`
* **O que faz:** Permite que um `ADMIN` atualize manualmente o `Status` de um pagamento e, consequentemente, o `Status` da reserva associada.
* **Exemplo de Corpo (JSON Body - DTO `AtualizarStatusPagamentoDto`):**
    ```json
    {
      "status": "RECUSADO" // Outros valores válidos: "APROVADO", "PENDENTE"
    }
    ```
* **Como usar:** Requer o token de um `ADMIN` válido no cabeçalho `Authorization`.
* **Resposta de Sucesso:** Status `200 OK` com mensagem de sucesso e o novo status.
    ```json
    {
      "message": "Status atualizado com sucesso.",
      "novoStatus": "RECUSADO"
    }
    ```
* **Resposta de Erro:** `400 Bad Request` se o status for inválido, `404 Not Found` se o pagamento não existir.

---

### 5. Avaliações (Controller: `AvaliacaoController`)

Este controller gerencia o registro de avaliações de pacotes de viagem por clientes e a moderação dessas avaliações por administradores.

#### 5.1. Registrar Avaliação de Pacote (POST)
* **Endpoint:** `POST /avaliacoes`
* **O que faz:** Permite que um usuário registre uma avaliação (nota e comentário) para um pacote de viagem.
* **Validações Importantes:**
    * A nota deve estar entre 1 e 5.
    * A avaliação só pode ser feita **após a data de término** da viagem do pacote.
    * O usuário **deve ter uma reserva confirmada** para aquele pacote.
    * O usuário **não pode avaliar o mesmo pacote mais de uma vez**.
    * Novas avaliações são registradas como **não aprovadas (`Aprovada = false`)** e precisam de moderação.
* **Exemplo de Corpo (JSON Body - DTO `AvaliacaoRequest`):**
    ```json
    {
      "usuario_Id": 123,     // ID do usuário que está avaliando
      "pacoteViagem_Id": 1,  // ID do pacote de viagem avaliado
      "nota": 5,             // Nota de 1 a 5
      "comentario": "Viagem incrível! Superou as expectativas. Recomendo!"
    }
    ```
* **Como usar:** Este endpoint não tem `[Authorize]` explicitamente no código fornecido, o que significa que tecnicamente **não requer autenticação** para ser acessado diretamente (mas as validações internas garantem que apenas usuários com reservas confirmadas e após a viagem possam avaliar). O `usuario_Id` é enviado no corpo.

#### 5.2. Listar Avaliações Aprovadas por Pacote (GET)
* **Endpoint:** `GET /avaliacoes/pacote/{id}`
* **O que faz:** Retorna todas as avaliações que foram **aprovadas** para um pacote de viagem específico, dado o seu ID.
* **Exemplo de Uso:** `GET /avaliacoes/pacote/1`
* **Resposta (Exemplo de um item JSON):**
    ```json
    [
      {
        "id": 1,
        "usuario": "Nome Completo do Cliente",
        "nota": 5,
        "comentario": "Viagem incrível! Superou as expectativas. Recomendo!",
        "data": "2025-07-29T17:30:00Z"
      }
      // ... outras avaliações aprovadas para o pacote 1
    ]
    ```

#### 5.3. Listar Avaliações Pendentes (GET - Requer Autenticação)
* **Endpoint:** `GET /avaliacoes/pendentes`
* **O que faz:** Retorna uma lista de todas as avaliações que ainda não foram aprovadas (ou seja, estão pendentes de moderação).
* **Como usar:** Este endpoint não tem `[Authorize]` explicitamente no código fornecido, o que pode ser uma omissão ou uma decisão de design. Idealmente, ele deveria ser protegido para `ADMIN` ou `ATENDENTE` roles para fins de moderação.
* **Resposta (Exemplo de um item JSON):**
    ```json
    [
      {
        "id": 2,
        "usuario": "Nome do Usuário Pendente",
        "pacote": "Título do Pacote Avaliado",
        "nota": 4,
        "comentario": "Comentário aguardando aprovação.",
        "data": "2025-07-29T18:00:00Z"
      }
      // ... outras avaliações pendentes
    ]
    ```

#### 5.4. Atualizar Status da Avaliação (PUT)
* **Endpoint:** `PUT /avaliacoes/{id}`
* **O que faz:** Permite a moderação de uma avaliação específica. Um `ADMIN` ou usuário autorizado pode "aprovar" (tornando-a visível publicamente) ou "rejeitar" (excluindo-a) uma avaliação.
* **Exemplo de Corpo (JSON Body - DTO `AvaliacaoAcaoDto`):**
    * **Para Aprovar:**
        ```json
        {
          "acao": "aprovar"
        }
        ```
    * **Para Rejeitar:**
        ```json
        {
          "acao": "rejeitar"
        }
        ```
* **Como usar:** Este endpoint não tem `[Authorize]` explicitamente no código fornecido, o que pode ser uma omissão ou decisão de design. Idealmente, ele deveria ser protegido para `ADMIN` ou `ATENDENTE` roles.
* **Resposta de Sucesso:** Status `200 OK` com mensagem de sucesso.
    ```json
    {
      "message": "Avaliação aprovada com sucesso."
      // Ou "Avaliação rejeitada e excluída com sucesso."
    }
    ```
* **Resposta de Erro:** `400 Bad Request` para ação inválida ou se a avaliação já estiver aprovada. `404 Not Found` se a avaliação não existir.
