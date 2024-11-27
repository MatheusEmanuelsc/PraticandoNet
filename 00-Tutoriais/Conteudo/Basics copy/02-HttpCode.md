

### 1. **Códigos de Resposta Informacional (1xx)**

Esses códigos indicam que o servidor recebeu a requisição e o cliente pode continuar com o processo.

- **100 Continue**: O servidor recebeu a requisição e o cliente pode continuar o envio.
- **101 Switching Protocols**: O servidor aceitou a solicitação para mudar o protocolo.
- **102 Processing (WebDAV)**: O servidor está processando a solicitação, mas ainda não tem uma resposta final.

---

### 2. **Códigos de Sucesso (2xx)**

Os códigos 2xx indicam que a requisição foi recebida, entendida e processada corretamente pelo servidor.

- **200 OK**: A requisição foi bem-sucedida. Este é o código de sucesso mais comum.
  - Usado em: `GET`, `PUT`, `POST`, `DELETE`, etc.
  
- **201 Created**: A requisição foi bem-sucedida e um novo recurso foi criado.
  - Usado em: `POST` ou `PUT` quando um novo recurso é criado.
  
- **202 Accepted**: A requisição foi aceita para processamento, mas ainda não foi concluída.
  - Usado em: operações assíncronas.
  
- **203 Non-Authoritative Information**: A requisição foi bem-sucedida, mas as informações de resposta podem ter sido alteradas por um proxy.
  
- **204 No Content**: A requisição foi bem-sucedida, mas não há conteúdo no corpo da resposta.
  - Usado em: `DELETE`, ou quando a resposta não precisa enviar dados.
  
- **205 Reset Content**: Instruções para o cliente resetar o conteúdo (por exemplo, resetar um formulário após envio).
  
- **206 Partial Content**: Retorna apenas parte do recurso solicitado.
  - Usado em: requisições que envolvem downloads parciais (como streaming).

---

### 3. **Códigos de Redirecionamento (3xx)**

Os códigos 3xx indicam que o cliente precisa tomar medidas adicionais para completar a requisição.

- **300 Multiple Choices**: Existem várias opções para o recurso solicitado. O usuário ou cliente deve escolher uma delas.
  
- **301 Moved Permanently**: O recurso solicitado foi movido permanentemente para uma nova URI.
  
- **302 Found**: O recurso solicitado foi movido temporariamente para outra URI.
  
- **303 See Other**: O cliente deve buscar o recurso em uma nova URI usando o método `GET`.
  
- **304 Not Modified**: O recurso não foi modificado desde a última requisição. O cliente pode usar a versão em cache.
  
- **307 Temporary Redirect**: O recurso foi movido temporariamente para outra URI, mas o método HTTP original deve ser usado na nova requisição.
  
- **308 Permanent Redirect**: O recurso foi movido permanentemente, e o método HTTP original deve ser usado na nova URI.

---

### 4. **Códigos de Erro do Cliente (4xx)**

Os códigos 4xx indicam que houve um erro por parte do cliente na requisição.

- **400 Bad Request**: A requisição é inválida ou malformada. O servidor não consegue processá-la.
  - Usado em: validação de dados incorretos, parâmetros faltantes.

- **401 Unauthorized**: O cliente precisa autenticar-se para obter a resposta.
  - Usado em: APIs protegidas por autenticação, como JWT.
  
- **402 Payment Required**: Reservado para uso futuro. Originalmente, foi destinado a sistemas de pagamento digital.
  
- **403 Forbidden**: O servidor entendeu a requisição, mas se recusa a autorizá-la.
  - Usado em: acesso a recursos sem permissões suficientes.

- **404 Not Found**: O recurso solicitado não foi encontrado no servidor.
  - Usado em: requisições de recursos inexistentes.

- **405 Method Not Allowed**: O método HTTP utilizado não é permitido para o recurso solicitado.
  
- **406 Not Acceptable**: O servidor não pode produzir uma resposta com o conteúdo aceito pelo cliente.
  
- **407 Proxy Authentication Required**: O cliente deve se autenticar no proxy antes de processar a requisição.
  
- **408 Request Timeout**: O servidor expirou o tempo de espera para a requisição do cliente.
  
- **409 Conflict**: A requisição não pôde ser processada devido a um conflito com o estado atual do recurso.
  - Usado em: tentativas de modificar recursos de forma inconsistente (concorrência de atualizações, etc.).

- **410 Gone**: O recurso solicitado não está mais disponível e não possui um endereço de redirecionamento.
  
- **411 Length Required**: O servidor precisa que o cabeçalho `Content-Length` esteja presente na requisição.
  
- **412 Precondition Failed**: A pré-condição enviada no cabeçalho da requisição foi avaliada como falsa pelo servidor.
  
- **413 Payload Too Large**: O corpo da requisição é maior do que o servidor está disposto ou pode processar.
  
- **414 URI Too Long**: O URI fornecido pelo cliente é muito longo para o servidor processar.
  
- **415 Unsupported Media Type**: O tipo de mídia da requisição não é suportado pelo servidor.
  
- **416 Range Not Satisfiable**: O cliente requisitou uma parte do recurso que o servidor não pode fornecer (usado com downloads parciais).
  
- **417 Expectation Failed**: O servidor não conseguiu atender ao campo de cabeçalho `Expect` da requisição.
  
- **418 I'm a teapot**: Um código humorístico do protocolo **HTCPCP**; indica que o servidor é um bule de chá e não pode preparar café.
  
- **422 Unprocessable Entity**: O servidor entende a requisição, mas não pode processar os dados recebidos.
  - Usado em: validação de dados em APIs RESTful.
  
- **426 Upgrade Required**: O cliente deve mudar para um protocolo diferente (por exemplo, HTTPS).
  
- **428 Precondition Required**: O servidor requer que a requisição seja condicional.
  
- **429 Too Many Requests**: O cliente enviou muitas requisições em um curto período de tempo (Rate Limiting).
  
- **431 Request Header Fields Too Large**: O cabeçalho da requisição é muito grande para ser processado pelo servidor.
  
- **451 Unavailable For Legal Reasons**: O recurso solicitado não está disponível devido a restrições legais (censura, etc.).

---

### 5. **Códigos de Erro do Servidor (5xx)**

Os códigos 5xx indicam que o servidor encontrou um erro ou não conseguiu completar a requisição.

- **500 Internal Server Error**: O servidor encontrou uma condição inesperada que o impediu de atender à requisição.
  - Usado em: erros genéricos no servidor.

- **501 Not Implemented**: O servidor não reconhece o método HTTP ou não possui capacidade para processar a requisição.

- **502 Bad Gateway**: O servidor, atuando como gateway ou proxy, recebeu uma resposta inválida do servidor upstream.
  
- **503 Service Unavailable**: O servidor está temporariamente indisponível, geralmente por manutenção ou sobrecarga.
  
- **504 Gateway Timeout**: O servidor, atuando como gateway ou proxy, não recebeu uma resposta do servidor upstream a tempo.

- **505 HTTP Version Not Supported**: O servidor não suporta a versão HTTP utilizada na requisição.

- **507 Insufficient Storage**: O servidor não pode armazenar a representação necessária para completar a requisição.
  
- **508 Loop Detected**: O servidor detectou um loop infinito ao processar uma requisição (geralmente em casos de WebDAV).
  
- **510 Not Extended**: Mais extensões são necessárias para que o servidor complete a requisição.

- **511 Network Authentication Required**: O cliente precisa se autenticar para ganhar acesso à rede (geralmente usado em portais cativos de Wi-Fi).

---

### Conclusão

Essa lista cobre a maioria dos **códigos HTTP** que você encontrará ao desenvolver APIs RESTful. Cada código tem seu propósito específico e é importante utilizá-los corretamente para que o cliente da API possa interpretar adequadamente o estado da requisição.